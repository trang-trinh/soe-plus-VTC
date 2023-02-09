<script setup>
import { onMounted, inject, ref, watch } from "vue";
import { encr } from "../../../util/function";
import { useToast } from "vue-toastification";
import dialogcontract from "../contract/component/dialogcontract.vue";
import moment from "moment";
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
  { value: 0, title: "Chưa hiệu lực", bg_color: "#bbbbbb", text_color: "#fff" },
  { value: 1, title: "Đang hiệu lực", bg_color: "#2196f3", text_color: "#fff" },
  { value: 2, title: "Hết hiệu lực", bg_color: "red", text_color: "#fff" },
  { value: 3, title: "Đã thanh lý", bg_color: "#ff8b4e", text_color: "#fff" },
]);
const selectedNodes = ref({});
const selectedKeys = ref([]);
const expandedKeys = ref([]);
const isFirst = ref(true);
const datas = ref([]);
const counts = ref([]);
const organization = ref({});
const organizations = ref([]);
const profiles = ref([]);
const type_contracts = ref([]);
const departments = ref([]);
const work_positions = ref([]);
const positions = ref([]);
const formalitys = ref([]);
const wages = ref([]);
const users = ref([]);
const allowance_formalitys = ref([]);
const allowance_wages = ref([]);
const professional_works = ref([]);
const contract = ref({});

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
    label: "Thiết lập trạng thái",
    icon: "pi pi-cog",
    command: (event) => {},
  },
  {
    label: "Thanh lý",
    icon: "pi pi-check-circle",
    command: (event) => {},
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
  //selectedNodes.value = item;
};

watch(selectedNodes, () => {
  options.value["filterContract_id"] = selectedNodes.value["contract_id"];
});

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

//Function
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
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

//add model
const isAdd = ref(false);
const submitted = ref(false);
const model = ref({});
const headerDialog = ref();
const displayDialog = ref(false);
const files = ref([]);
const openAddDialog = (str) => {
  forceRerender();
  isAdd.value = true;
  model.value = {
    profile: null,
    sign_user: null,
    contract_no: "",
    contract_name: "",
    employment: organization.value.address,
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

      headerDialog.value = str;
      displayDialog.value = true;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
                  text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
      initData(true);
    })
    .catch((error) => {
      swal.close();
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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

//Init
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
          if (tbs[0] != null && tbs[0].length > 0) {
            organization.value = tbs[0][0];
          } else {
            organization.value = [];
          }
          if (tbs[1] != null && tbs[1].length > 0) {
            profiles.value = tbs[1];
          } else {
            profiles.value = [];
          }
          if (tbs[2] != null && tbs[2].length > 0) {
            type_contracts.value = tbs[2];
          } else {
            type_contracts.value = [];
          }
          if (tbs[3] != null && tbs[3].length > 0) {
            departments.value = tbs[3];
          } else {
            departments.value = [];
          }
          if (tbs[4] != null && tbs[4].length > 0) {
            work_positions.value = tbs[4];
          } else {
            work_positions.value = [];
          }
          if (tbs[5] != null && tbs[5].length > 0) {
            positions.value = tbs[5];
          } else {
            positions.value = [];
          }
          if (tbs[6] != null && tbs[6].length > 0) {
            formalitys.value = tbs[6];
          } else {
            formalitys.value = [];
          }
          if (tbs[7] != null && tbs[7].length > 0) {
            wages.value = tbs[7];
          } else {
            wages.value = [];
          }
          if (tbs[8] != null && tbs[8].length > 0) {
            users.value = tbs[8];
          } else {
            users.value = [];
          }
          if (tbs[9] != null && tbs[9].length > 0) {
            allowance_formalitys.value = tbs[9];
          } else {
            allowance_formalitys.value = [];
          }
          if (tbs[10] != null && tbs[10].length > 0) {
            allowance_wages.value = tbs[10];
          } else {
            allowance_wages.value = [];
          }
          if (tbs[11] != null && tbs[11].length > 0) {
            professional_works.value = tbs[11];
          } else {
            professional_works.value = [];
          }
          if (tbs[12] != null && tbs[12].length > 0) {
            organizations.value = tbs[12];
          } else {
            organizations.value = [];
          }
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
            proc: "hrm_contract_count_filter",
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
            proc: "hrm_contract_count",
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
            proc: "hrm_contract_list_filter",
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
            proc: "hrm_contract_list",
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
  initCount();
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
  <div class="surface-100 p-3">
    <Toolbar class="outline-none surface-0 border-none pb-0">
      <template #start>
        <div>
          <h3 class="module-title m-0">
            <i class="pi pi-file"></i> Danh sách hợp đồng ({{ options.total }})
          </h3>
        </div>
      </template>
    </Toolbar>
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
            class="ip36 input-search"
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
                          :options="organizations"
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
                                      class="
                                        p-chip-remove-icon
                                        pi pi-times-circle
                                        format-flex-center
                                      "
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
                          :options="departments"
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
                                      class="
                                        p-chip-remove-icon
                                        pi pi-times-circle
                                        format-flex-center
                                      "
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
                          :options="type_contracts"
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
                                      class="
                                        p-chip-remove-icon
                                        pi pi-times-circle
                                        format-flex-center
                                      "
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
                          :options="work_positions"
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
                                      class="
                                        p-chip-remove-icon
                                        pi pi-times-circle
                                        format-flex-center
                                      "
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
                          :options="users"
                          v-model="options.users"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          optionLabel="full_name"
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
                                  :label="value.full_name"
                                  class="mr-2 mb-2 px-3 py-2"
                                >
                                  <div class="flex">
                                    <div class="format-flex-center">
                                      <Avatar
                                        v-bind:label="
                                          value.avatar
                                            ? ''
                                            : (value.last_name ?? '').substring(
                                                0,
                                                1
                                              )
                                        "
                                        v-bind:image="
                                          value.avatar
                                            ? basedomainURL + value.avatar
                                            : basedomainURL +
                                              '/Portals/Image/noimg.jpg'
                                        "
                                        style="
                                          background-color: #2196f3;
                                          color: #ffffff;
                                          width: 2rem;
                                          height: 2rem;
                                        "
                                        :style="{
                                          background:
                                            bgColor[value.is_order % 7],
                                        }"
                                        class="mr-2 text-avatar"
                                        size="xlarge"
                                        shape="circle"
                                      />
                                    </div>
                                    <div class="format-flex-center text-left">
                                      <span>{{ value.full_name }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="
                                        p-chip-remove-icon
                                        pi pi-times-circle
                                        format-flex-center
                                      "
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
                                      : slotProps.option.last_name.substring(
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
                                  style="
                                    background-color: #2196f3;
                                    color: #ffffff;
                                    width: 3rem;
                                    height: 3rem;
                                    font-size: 1.4rem !important;
                                  "
                                  :style="{
                                    background:
                                      bgColor[slotProps.option.is_order % 7],
                                  }"
                                  class="text-avatar"
                                  size="xlarge"
                                  shape="circle"
                                />
                              </div>
                              <div class="ml-3">
                                <div class="mb-1">
                                  {{ slotProps.option.full_name }}
                                </div>
                                <div class="description">
                                  <div>
                                    {{ slotProps.option.position_name }}
                                  </div>
                                  <div>
                                    {{ slotProps.option.department_name }}
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
          @click="openAddDialog('Thêm mới hợp đồng')"
          label="Thêm mới"
          icon="pi pi-plus"
          class="mr-2"
        />
        <Button
          @click="refresh()"
          class="p-button-outlined p-button-secondary mr-2"
          icon="pi pi-refresh"
          label="Tải lại"
        />
        <Button
          icon="pi pi-trash"
          label="Xóa"
          :class="{
            'p-button-danger': options.filterContract_id != null,
            'btn-hidden p-button-danger': options.filterContract_id == null,
          }"
          @click="deleteItem()"
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
          field="contract_no"
          header="Mã HĐ"
          headerStyle="text-align:center;max-width:80px;height:50px"
          bodyStyle="text-align:center;max-width:80px;"
          class="align-items-center justify-content-center text-center"
        />
        <Column
          field="profile_user_name"
          header="Tên nhân sự"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            {{ slotProps.data.profile_user_name }}
          </template>
        </Column>
        <Column
          field="department_name"
          header="Phòng ban"
          headerStyle="height:50px;max-width:auto;"
        >
          <template #body="slotProps">
            {{ slotProps.data.department_name }}
          </template>
        </Column>
        <Column
          field="type_contract_name"
          header="Loại hợp đồng"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
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
        <Column
          field="sign_user_name"
          header="Người ký"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            {{ slotProps.data.sign_user_name }}
          </template>
        </Column>
        <Column
          field="status"
          header="Trạng thái"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <Tag
              class="px-3 py-1"
              :value="slotProps.data.status_name"
              :style="{
                backgroundColor: slotProps.data.bg_color,
                color: slotProps.data.text_color,
              }"
              style="font-size: 11px"
            ></Tag>
          </template>
        </Column>
        <!-- <Column
          header="Chức năng"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <div v-if="slotProps.data.is_function">
              <Button
                @click="editItem(slotProps.data, 'Cập nhật thông tin nhóm')"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                type="button"
                icon="pi pi-pencil"
                v-tooltip.top="'Sửa'"
              ></Button>
              <Button
                @click="deleteItem(slotProps.data, true)"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                type="button"
                icon="pi pi-trash"
                v-tooltip.top="'Xóa'"
              ></Button>
            </div>
          </template>
        </Column> -->
        <Column
          header=""
          headerStyle="text-align:center;max-width:50px"
          bodyStyle="text-align:center;max-width:50px"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <div>
              <a
                @click="setStar(slotProps.data)"
                v-tooltip.top="slotProps.data.is_star ? 'Quan trọng' : ''"
              >
                <i
                  :class="{
                    'pi pi-star-fill icon-start': slotProps.data.is_star,
                    'pi pi-star': !slotProps.data.is_star,
                  }"
                  style="font-size: 15px"
                ></i>
              </a>
            </div>
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
              class="p-button-rounded p-button-text ml-2"
              @click="toggleMores($event, slotProps.data)"
              aria-haspopup="true"
              aria-controls="overlay_More"
            />
          </template>
        </Column>
        <template #empty>
          <div
            class="
              align-items-center
              justify-content-center
              p-4
              text-center
              m-auto
            "
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
    :key="componentKey"
    :headerDialog="headerDialog"
    :displayDialog="displayDialog"
    :closeDialog="closeDialog"
    :isAdd="isAdd"
    :model="model"
    :files="files"
    :selectFile="selectFile"
    :removeFile="removeFile"
    :profiles="profiles"
    :type_contracts="type_contracts"
    :departments="departments"
    :work_positions="work_positions"
    :positions="positions"
    :formalitys="formalitys"
    :wages="wages"
    :users="users"
    :allowance_formalitys="allowance_formalitys"
    :allowance_wages="allowance_wages"
    :professional_works="professional_works"
    :initData="initData"
  />
  <Menu
    id="overlay_More"
    ref="menuButMores"
    :model="itemButMores"
    :popup="true"
  />
</template>
<style scoped>
@import url(../contract/component/stylehrm.css);
</style>
<style scoped>
.d-lang-table {
  height: calc(100vh - 215px) !important;
  background-color: #fff;
}
.tableview-nav {
  background: #ffffff;
  border: 1px solid #dee2e6;
  border-width: 0 0 2px 0;
  display: flex;
  flex: 1 1 auto;
  list-style-type: none;
  margin: 0;
  padding: 0;
}
.tableview-header {
  display: inline-block;
}
.tableview-nav li {
  border: solid #dee2e6;
  border-width: 0 0 2px 0;
  padding: 1.25rem;
  font-weight: 700;
  margin: 0 0 -2px 0;
  transition: background-color 0.2s, border-color 0.2s, box-shadow 0.2s;
}
.tableview-nav li:hover {
  cursor: pointer;
}
.tableview-nav li.highlight {
  background: #ffffff;
  border-color: #3b82f6;
  color: #3b82f6;
}
.tableview-nav li:not(.highlight):hover {
  background: #ffffff;
  border-color: #adb5bd;
  color: #6c757d;
}
.tableview-nav li a:focus {
  outline: 0 none;
  outline-offset: 0;
  box-shadow: inset 0 0 0 0.2rem #bfdbfe;
}
.btn-hidden {
  filter: opacity(40%) !important;
  cursor: auto !important;
}
.icon-start {
  color: orange !important;
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