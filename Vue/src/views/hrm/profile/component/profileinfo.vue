<script setup>
import { onMounted, inject, ref, watch } from "vue";
import { encr } from "../../../../util/function";
import { useToast } from "vue-toastification";
import { useRoute } from "vue-router";
import dialogcontract from "../../contract/component/dialogcontract.vue";
import dialoginfo from "../../profile/component/dialoginfo.vue";
import dialogtraining from "../../training/component/dialog_training.vue";
import dialogfile from "../../profile/component/dialogfile.vue";
import printprofile from "../component/printprofile.vue";
import diloginsurance from "../../insurance/component/diloginsurance.vue";
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
const views = ref([
  { view: 1, title: "Sơ yếu", icon: "fa-regular fa-address-card" },
  { view: 2, title: "Phân công", icon: "fa-solid fa-list-check" },
  { view: 3, title: "Hợp đồng", icon: "fa-solid fa-file-contract" },
  { view: 4, title: "Chấm công", icon: "fa-solid fa-building-circle-check" },
  { view: 5, title: "Lương", icon: "a-solid fa-money-check-dollar" },
  { view: 6, title: "Bảo hiểm", icon: "fa-solid fa-file-shield" },
  { view: 7, title: "Phép", icon: "fa-regular fa-calendar-days" },
  { view: 8, title: "Đào tạo", icon: "fa-solid fa-person-chalkboard" },
  { view: 9, title: "Quyết định", icon: "fa-solid fa-envelope-open" },
  { view: 10, title: "File", icon: "fa-solid fa-paperclip" },
  { view: 11, title: "Tiếp nhận", icon: "fa-regular fa-file" },
  { view: 12, title: "Sức khỏe", icon: "fa-solid fa-briefcase-medical" },
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
  view: 1,
  profile_id: null,
  key_id: null,
  contract_id: null,
  training_emps: {},
  file: {},
  type_files: [],
  is_type_files: [],
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
const selectedNodes = ref([]);
watch(selectedNodes, () => {});
const selectRow = (event) => {
  if (event && event.data) {
    if (options.value.view === 1) {
      goProfile(event.data);
    } else if (options.value.view === 3) {
      options.value["contract_id"] = event.data["contract_id"];
      openViewDialogContract("Thông tin hợp đồng");
    } else if (options.value.view === 8) {
      options.value["training_emps"] = event.data;
      openViewDialogTranning("Thông tin khóa đào tạo");
    } else if (options.value.view === 10) {
      options.value["file"] = event.data;
      openViewDialogFile(event.data["file_name"]);
    }
  }
};
const goProfile = (profile) => {
  router
    .push({
      name: "profileinfo",
      params: { id: profile.key_id },
      query: { id: profile.profile_id },
    })
    .then(() => {
      router.go();
    });
};

//filter
const searchData = () => {
  initData();
};
const opfilter = ref();
const toggleFilter = (event) => {
  opfilter.value.toggle(event);
};
const resetFilter = () => {
  options.value.type_files = [];
  options.value.is_type_files = [];
};
const removeFilter = (idx, array) => {
  array.splice(idx, 1);
};
const filter = (event) => {
  opfilter.value.toggle(event);
  initData();
};

//data view 1
const profile = ref({});
const places = ref([]);
const marital_status = ref([
  { value: 0, text: "Độc thân" },
  { value: 1, text: "Kết hôn" },
  { value: 2, text: "Ly hôn" },
]);
const dependents = ref([
  { value: 1, title: "Có phụ thuộc" },
  { value: 0, title: "Không phụ thuộc" },
]);
const forms = ref([
  { value: 0, title: "Dự bị" },
  { value: 1, title: "Chính thức" },
  { value: 1, title: "Điều chuyển" },
]);
const dictionarys = ref([]);
const datachilds = ref([]);

//data view 2
const task = ref({});
const tasks = ref([]);

//data view 3
const contracts = ref([]);
const typestatus = ref([
  { value: 0, title: "Chưa hiệu lực", bg_color: "#bbbbbb", text_color: "#fff" },
  { value: 1, title: "Đang hiệu lực", bg_color: "#2196f3", text_color: "#fff" },
  { value: 2, title: "Hết hiệu lực", bg_color: "red", text_color: "#fff" },
  { value: 3, title: "Đã thanh lý", bg_color: "#ff8b4e", text_color: "#fff" },
]);
const isView = ref(false);
const contract = ref({});
const headerDialogContract = ref();
const displayDialogContract = ref(false);
const openViewDialogContract = (str) => {
  forceRerender();
  isView.value = true;
  options.value.loading = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_contract_get",
            par: [{ par: "contract_id", va: options.value["contract_id"] }],
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
          contract.value = tbs[0][0];
          if (contract.value["profiles"] != null) {
            contract.value["profile"] = JSON.parse(
              contract.value["profiles"]
            )[0];
          }
          if (contract.value["sign_users"] != null) {
            contract.value["sign_user"] = JSON.parse(
              contract.value["sign_users"]
            )[0];
          }
          if (contract.value["start_date"] != null) {
            contract.value["start_date"] = new Date(
              contract.value["start_date"]
            );
          }
          if (contract.value["end_date"] != null) {
            contract.value["end_date"] = new Date(contract.value["end_date"]);
          }
          if (contract.value["sign_date"] != null) {
            contract.value["sign_date"] = new Date(contract.value["sign_date"]);
          }
          if (contract.value["professional_works"] != null) {
            contract.value["professional_works"] = contract.value[
              "professional_works"
            ]
              .split(",")
              .map((x) => parseInt(x));
          }
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          contract.value["allowances"] = tbs[1];
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
            contract.value["allowances"].forEach((allowance) => {
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
          contract.value.allowances = [];
        }
        if (tbs[3] != null && tbs[3].length > 0) {
          contract.value["files"] = tbs[3];
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;

      headerDialogContract.value = str;
      displayDialogContract.value = true;
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
const closeDialogContract = () => {
  displayDialogContract.value = false;
};

//data view 6
const insurance_status = ref([
  { status: 1, title: "Trả" },
  { status: 2, title: "Sửa" },
  { status: 3, title: "Chốt" },
  { status: 4, title: "Xin cấp" },
  { status: 5, title: "Gộp" },
  { status: 6, title: "Người lao động giữ sổ" },
]);
const insurance = ref({});
const insurance_pays = ref([]);
const insurance_resolves = ref([]);
function formatNumber(a, b, c, d) {
  var e = isNaN((b = Math.abs(b))) ? 2 : b;
  b = void 0 == c ? "," : c;
  d = void 0 == d ? "," : d;
  c = 0 > a ? "-" : "";
  var g = parseInt((a = Math.abs(+a || 0).toFixed(e))) + "",
    n = 3 < (n = g.length) ? n % 3 : 0;
  return (
    c +
    (n ? g.substr(0, n) + d : "") +
    g.substr(n).replace(/(\d{3})(?=\d)/g, "$1" + d) +
    (e
      ? b +
        Math.abs(a - g)
          .toFixed(e)
          .slice(2)
      : "")
  );
}

//data view 8
const statusTrannings = ref([
  { name: "Lên kế hoạch", code: 1 },
  { name: "Đang thực hiện", code: 2 },
  { name: "Đã hoàn thành", code: 3 },
  { name: "Tạm dừng", code: 4 },
  { name: "Đã hủy", code: 5 },
]);
const typeTrannings = ref([
  { name: "Cấp lãnh đạo", code: 1 },
  { name: "Quản lý", code: 2 },
  { name: "Nhân viên", code: 3 },
]);
const formTrannings = ref([
  { name: "Bắt buộc", code: 1 },
  { name: "Đăng ký", code: 2 },
  { name: "Cả hai", code: 3 },
]);
const trannings = ref([]);
const tranning = ref({});
const headerDialogTranning = ref();
const displayDialogTranning = ref(false);
const openViewDialogTranning = (str) => {
  forceRerender();
  headerDialogTranning.value = str;
  displayDialogTranning.value = true;
};
const closeDialogTranning = () => {
  displayDialogTranning.value = false;
};

//data view 10
const typeFiles = ref([
  { is_type: 0, title: "Sơ yếu lí lịch" },
  { is_type: 1, title: "Hợp đồng" },
  { is_type: 2, title: "Đào tạo" },
]);
const type_files = ref([
  { type_file: "pdf", title: "PDF" },
  { type_file: "jpg,jpeg,png,gif", title: "Ảnh" },
  { type_file: "doc,docs,xls,xlsx", title: "Word,Excel" },
  { type_file: "orther", title: "Khác" },
]);
const is_type_files = ref([
  { is_type: 0, title: "File sơ yếu lý lịch" },
  { is_type: 1, title: "File hợp đồng" },
  { is_type: 2, title: "File đào tạo" },
]);
const files = ref([]);
const formatBytes = (bytes, decimals = 2) => {
  if (bytes === 0) return "0 Bytes";
  const k = 1024;
  const dm = decimals < 0 ? 0 : decimals;
  const sizes = ["Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
};
const headerDialogFile = ref();
const displayDialogFile = ref(false);
const openViewDialogFile = (str) => {
  headerDialogFile.value = str;
  displayDialogFile.value = true;
};
const closeDialogFile = () => {
  displayDialogFile.value = false;
};

//data view 11
const receipts = ref([]);

//data view 12
const injections = ref([
  { id: 1, title: "Mũi 1" },
  { id: 2, title: "Mũi 2" },
  { id: 3, title: "Mũi 3" },
  { id: 4, title: "Mũi 4" },
  { id: 5, title: "Mũi 5" },
  { id: 6, title: "Mũi 6" },
  { id: 7, title: "Mũi 7" },
  { id: 8, title: "Mũi 8" },
  { id: 9, title: "Mũi 9" },
  { id: 10, title: "Mũi 10" },
]);
const type_vaccines = ref([
  { id: "Vaccine Abdala (AICA-Cuba)", title: "Vaccine Abdala (AICA-Cuba)" },
  { id: "Vaccine Hayat-Vax", title: "Vaccine Hayat-Vax" },
  { id: "Covit-19 Vaccine Janssen", title: "Covit-19 Vaccine Janssen" },
  {
    id: "Spikevax (Covit-19 vaccine Modena)",
    title: "Spikevax (Covit-19 vaccine Modena)",
  },
  { id: "Comirnaty (Pfizer BioNtech)", title: "Comirnaty (Pfizer BioNtech)" },
  { id: "Vero-cell (của Sinopharm)", title: "Vero-cell (của Sinopharm)" },
  { id: "AZD1222 (của AstraZeneca)", title: "AZD1222 (của AstraZeneca)" },
  { id: "Sputnik-V (của Gamalaya)", title: "Sputnik-V (của Gamalaya)" },
]);
const health = ref({});
const vaccines = ref([]);

//data view 13
const goPrint = (view) => {
  options.value.view = view;
};

//filter
const goFile = (file) => {
  window.open(basedomainURL + file.file_path, "_blank");
};
const goBack = () => {
  router.push({ name: "profile" });
};

//Function
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
const changeView = (view) => {
  options.value.view = view;
  initData();
};

//function edit
const isType = ref();
const headerDialog = ref();
const displayDialog = ref(false);
const openEditDialog = (type, str) => {
  forceRerender();
  if (type === 1) {
    isType.value = type;
    headerDialog.value = str;
    displayDialog.value = true;
  } else if (type === 2) {
    isType.value = type;
    headerDialog.value = str;
    displayDialog.value = true;
  }
};
const closeDialog = () => {
  displayDialog.value = false;
};

//Function edit bảo hiểm
const statuss = ref([
  { value: 1, text: "Trả" },
  { value: 2, text: "Sửa" },
  { value: 3, text: "Chốt" },
  { value: 4, text: "Xin cấp" },
  { value: 5, text: "Gộp" },
  { value: 6, text: "Người lao động giữ sổ" },
]);
const hinhthucs = ref([
  { value: 1, text: "Bao tăng" },
  { value: 2, text: "Báo giảm" },
]);
const insurance_dictionarys = ref([]);
const initDictionaryInsurance = () => {
  axios
    .post(
      baseURL + "api/insurance/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_insurance_dictionary",
            par: [{ par: "user_id", va: store.state.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data != null) {
        insurance_dictionarys.value = data;
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const headerDialogInsurance = ref();
const displayDialogInsurance = ref(false);
const openEditDialogInsurance = (str) => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_insurance_edit",
            par: [{ par: "profile_id", va: options.value["profile_id"] }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        if (data[0] != null && data[0].length > 0) {
          insurance.value = data[0][0];
        } else {
          insurance.value = {};
        }
        //get child
        if (data[1] != null && data[1].length > 0) {
          insurance_pays.value = data[1];
          insurance_pays.value.forEach((item) => {
            if (item.start_date != null) {
              item.start_date = new Date(item.start_date);
            }
          });
        } else {
          insurance_pays.value = [];
        }

        if (data[2] != null && data[2].length > 0) {
          insurance_resolves.value = data[2];
          insurance_resolves.value.forEach((item) => {
            if (item.received_file_date != null) {
              item.received_file_date = new Date(item.received_file_date);
            }
            if (item.completed_date != null) {
              item.completed_date = new Date(item.completed_date);
            }
            if (item.received_money_date != null) {
              item.received_money_date = new Date(item.received_money_date);
            }
          });
        } else {
          insurance_resolves.value = [];
        }
      }
      headerDialogInsurance.value = str;
      displayDialogInsurance.value = true;
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
const closeDialogInsurance = () => {
  displayDialogInsurance.value = false;
};
const addRow = (type) => {
  //relative
  if (type == 1) {
    let obj = {
      start_date: null,
      payment_form: null,
      reason: null,
      end_date: null,
      organization_payment: null,
      total_payment: null,
      company_payment: null,
      member_payment: null,
    };
    insurance_pays.value.push(obj);
  }
  if (type == 2) {
    let obj = {
      type_mode: null,
      payment_form: null,
      type_mode: null,
      completed_date: null,
      received_money_date: null,
      money: null,
    };
    insurance_resolves.value.push(obj);
  }
};
const deleteRow = (idx, type) => {
  if (type == 1) {
    insurance_pays.value.splice(idx, 1);
  }
  if (type == 2) {
    insurance_resolves.value.splice(idx, 1);
  }
};

//Function mores
const menuButs = ref();
const itemButs = ref([
  {
    label: "Thông tin chung/liên hệ",
    icon: "pi pi-id-card",
    command: (event) => {
      openEditDialog(1, "Cập nhật thay đổi thông tin");
    },
  },
  {
    label: "Gia đình, người phụ thuộc",
    icon: "pi pi-users",
    command: (event) => {
      openEditDialog(
        2,
        "Cập nhật thay đổi thông tin gia đình, người phụ thuộc"
      );
    },
  },
  {
    label: "Thông tin bảo hiểm",
    icon: "pi pi-shield",
    command: (event) => {
      openEditDialogInsurance("Cập nhật thay đổi thông bảo hiểm");
    },
  },
]);
const toggleEdit = (event) => {
  menuButs.value.toggle(event);
};

const menuButMores = ref();
const itemButMores = ref([
  {
    view: 11,
    label: "Tiếp nhận hồ sơ",
    icon: "fa-regular fa-file",
    command: (event) => {
      options.value.view = 11;
      initData();
    },
  },
  {
    view: 12,
    label: "Thông tin sức khỏe",
    icon: "fa-solid fa-briefcase-medical",
    command: (event) => {
      options.value.view = 12;
      initData();
    },
  },
]);
const toggleMores = (event) => {
  menuButMores.value.toggle(event);
};

const menuButPrints = ref();
const itemButPrints = ref([
  {
    view: 13,
    label: "Sơ yếu lý lịch(Mẫu 2C-BNV/2008)",
    icon: "fa-regular fa-file",
    command: (event) => {
      goPrint(13);
    },
  },
  {
    view: 14,
    label: "Sơ yếu lý lịch (Mẫu 2C/TCTW-98)",
    icon: "fa-regular fa-file",
    command: (event) => {
      goPrint(14);
    },
  },
  {
    view: 14,
    label: "Sổ lao động mẫu 145/2020/NĐ-CP",
    icon: "fa-regular fa-file",
    command: (event) => {
      goPrint(15);
    },
  },
]);
const togglePrints = (event) => {
  menuButPrints.value.toggle(event);
};

//init Dictionary view 1
const initPlace = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "ca_places_list",
            par: [
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
      renderPlace(response);
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");

      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const renderPlace = (response) => {
  let list1 = [];
  let list2 = [];
  let list3 = [];
  let d1 = JSON.parse(response.data.data)[0];
  d1.forEach((element, i) => {
    let c = {
      key: element.place_id,
      data: element.place_id,
      label: element.name,
      children: null,
    };
    if (d1[i].children) {
      list2 = JSON.parse(d1[i].children);
      if (list2 != null) {
        list2.forEach((element, i) => {
          element.label = element.data.name;
          element.data = parseInt(element.data.place_id);
          element.key = element.data;
          //đổi is_order
          if (list2[i].children != null && list2[i].children.length > 0) {
            // list3 = list2[i].children;
            // list2[i].children = list3;
            list2[i].children.forEach((element, i) => {
              element.label = element.data.name;
              element.data = parseInt(element.data.place_id);
              element.key = element.data;
            });
          }
        });
      }
      c.children = list2;
    }
    list1.push(c);
  });
  places.value = list1;
};
const initDictionary1 = () => {
  dictionarys.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_dictionary",
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
    })
    .then(() => {
      initPlace();
    })
    .then(() => {
      initView1(true);
    });
};
//init dictionary view 2
const initDictionary2 = () => {
  dictionarys.value = [];
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
    })
    .then(() => {
      initView2(true);
    });
};
//init dictionary view 3
const initDictionary3 = () => {
  dictionarys.value = [];
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
//init dictionary view 6
const initDictionary6 = () => {
  dictionarys.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_insurance_dictionary",
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
    })
    .then(() => {
      initView6(true);
    });
};
//init dictionary view 8
const initDictionary8 = () => {
  dictionarys.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_tranning_dictionary",
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
    })
    .then(() => {
      initView8(true);
    });
};
//init dictionary view 12
const initDictionary12 = () => {
  dictionarys.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_health_dictionary",
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
    })
    .then(() => {
      initView12(true);
    });
};

//Init data
const initView1 = (rf) => {
  datachilds.value = [];
  options.value.loading = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_detail_get",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "profile_id", va: options.value["profile_id"] },
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
          profile.value = tbs[0][0];
          profile.value["relates"] = JSON.parse(profile.value["relates"]);
          profile.value["relate"] = profile.value["relates"][0];
          if(profile.value["relate"]["relate_time"] != null){
            profile.value["relate"]["relate_time"] = moment(new Date(profile.value["relate"]["relate_time"])).format("DD/MM/YYYY");
          }
          profile.value["gender"] =
            profile.value["gender"] == 1
              ? "Nam"
              : profile.value["gender"] == 2
              ? "Nữ"
              : "";
          var idx = places.value.findIndex(
            (x) => x["place_id"] === profile.value["birthplace_id"]
          );
          if (idx !== -1) {
            profile.value["select_birthplace"] =
              places.value[idx]["place_name"];
          }
          var idx = places.value.findIndex(
            (x) => x["place_id"] === profile.value["birthplace_origin_id"]
          );
          if (idx !== -1) {
            profile.value["select_birthplace_origin"] =
              places.value[idx]["place_name"];
          }
          var idx = places.value.findIndex(
            (x) => x["place_id"] === profile.value["place_register_permanent"]
          );
          if (idx !== -1) {
            profile.value["select_place_register_permanent"] =
              places.value[idx]["place_name"];
          }
          if (profile.value["recruitment_date"] != null) {
            profile.value["recruitment_date"] = moment(
              new Date(profile.value["recruitment_date"])
            ).format("DD/MM/YYYY");
          }
          if (profile.value["birthday"] != null) {
            profile.value["birthday"] = moment(
              new Date(profile.value["birthday"])
            ).format("DD/MM/YYYY");
          }
          if (profile.value["identity_date_issue"] != null) {
            profile.value["identity_date_issue"] = moment(
              new Date(profile.value["identity_date_issue"])
            ).format("DD/MM/YYYY");
          }
          //
          var idx = dictionarys.value[0].findIndex(
            (x) =>
              x["identity_papers_id"] === profile.value["identity_papers_id"]
          );
          if (profile.value["identity_papers_id"] != null && idx != -1) {
            profile.value["identity_papers_name"] =
              dictionarys.value[0][idx]["identity_papers_name"];
          }
          //
          var idx = dictionarys.value[17].findIndex(
            (x) => x["identity_place_id"] === profile.value["identity_place_id"]
          );
          if (profile.value["identity_place_id"] != null && idx != -1) {
            profile.value["identity_place_name"] =
              dictionarys.value[17][idx]["identity_place_name"];
          }
          //
          var idx = dictionarys.value[1].findIndex(
            (x) => x["nationality_id"] === profile.value["nationality_id"]
          );
          if (profile.value["nationality_id"] != null && idx != -1) {
            profile.value["nationality_name"] =
              dictionarys.value[1][idx]["nationality_name"];
          }
          //
          var idx = marital_status.value.findIndex(
            (x) => x["value"] === profile.value["marital_status"]
          );
          if (profile.value["marital_status"] != null && idx != -1) {
            profile.value["marital_status"] = marital_status.value[idx]["text"];
          }
          //
          var idx = dictionarys.value[2].findIndex(
            (x) => x["ethnic_id"] === profile.value["ethnic_id"]
          );
          if (profile.value["ethnic_id"] != null && idx != -1) {
            profile.value["ethnic_name"] =
              dictionarys.value[2][idx]["ethnic_name"];
          }
          //
          var idx = dictionarys.value[3].findIndex(
            (x) => x["religion_id"] === profile.value["religion_id"]
          );
          if (profile.value["religion_id"] != null && idx != -1) {
            profile.value["religion_name"] =
              dictionarys.value[3][idx]["religion_name"];
          }
          //
          var idx = dictionarys.value[4].findIndex(
            (x) => x["bank_id"] === profile.value["bank_id"]
          );
          if (profile.value["bank_id"] != null && idx != -1) {
            profile.value["bank_name"] = dictionarys.value[4][idx]["bank_name"];
          }
          //
          var idx = dictionarys.value[5].findIndex(
            (x) => x["cultural_level_id"] === profile.value["cultural_level_id"]
          );
          if (profile.value["cultural_level_id"] != null && idx != -1) {
            profile.value["cultural_level_name"] =
              dictionarys.value[5][idx]["cultural_level_name"];
          }
          //
          var idx = dictionarys.value[6].findIndex(
            (x) => x["academic_level_id"] === profile.value["academic_level_id"]
          );
          if (profile.value["academic_level_id"] != null && idx != -1) {
            profile.value["academic_level_name"] =
              dictionarys.value[6][idx]["academic_level_name"];
          }
          //
          var idx = dictionarys.value[7].findIndex(
            (x) => x["specialization_id"] === profile.value["specialization_id"]
          );
          if (profile.value["specialization_id"] != null && idx != -1) {
            profile.value["specialization_name"] =
              dictionarys.value[7][idx]["specialization_name"];
          }
          //
          var idx = dictionarys.value[14].findIndex(
            (x) =>
              x["management_state_id"] === profile.value["management_state_id"]
          );
          if (profile.value["management_state_id"] != null && idx != -1) {
            profile.value["management_state_name"] =
              dictionarys.value[14][idx]["management_state_name"];
          }
          //
          var idx = dictionarys.value[8].findIndex(
            (x) =>
              x["political_theory_id"] === profile.value["political_theory_id"]
          );
          if (profile.value["political_theory_id"] != null && idx != -1) {
            profile.value["political_theory_name"] =
              dictionarys.value[8][idx]["political_theory_name"];
          }
          //
          var idx = dictionarys.value[9].findIndex(
            (x) => x["language_level_id"] === profile.value["language_level_id"]
          );
          if (profile.value["language_level_id"] != null && idx != -1) {
            profile.value["language_level_name"] =
              dictionarys.value[9][idx]["language_level_name"];
          }
          //
          var idx = dictionarys.value[10].findIndex(
            (x) =>
              x["informatic_level_id"] === profile.value["informatic_level_id"]
          );
          if (profile.value["informatic_level_id"] != null && idx != -1) {
            profile.value["informatic_level_name"] =
              dictionarys.value[10][idx]["informatic_level_name"];
          }
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          tbs[1].forEach((x) => {
            if (x["identification_date_issue"] != null) {
              x["identification_date_issue"] = moment(
                new Date(x["identification_date_issue"])
              ).format("DD/MM/YYYY");
            }
            if (x["start_date"] != null) {
              x["start_date"] = moment(new Date(x["start_date"])).format(
                "DD/MM/YYYY"
              );
            }
            if (x["end_date"] != null) {
              x["end_date"] = moment(new Date(x["end_date"])).format(
                "DD/MM/YYYY"
              );
            }
            //
            var idx = dictionarys.value[11].findIndex(
              (a) => a["relationship_id"] === x["relationship_id"]
            );
            if (x["relationship_id"] != null && idx != -1) {
              x["relationship_name"] =
                dictionarys.value[11][idx]["relationship_name"];
            }
            //
            var idx = dependents.value.findIndex(
              (a) => a["value"] === x["is_dependent"]
            );
            if (x["is_dependent"] != null && idx != -1) {
              x["dependent_name"] = dependents.value[idx]["title"];
            }
          });
          datachilds.value[1] = tbs[1];
        } else {
          datachilds.value[1] = [];
        }
        if (tbs[2] != null && tbs[2].length > 0) {
          tbs[2].forEach((x) => {
            if (x["start_date"] != null) {
              x["start_date"] = moment(new Date(x["start_date"])).format(
                "MM/YYYY"
              );
            }
            if (x["end_date"] != null) {
              x["end_date"] = moment(new Date(x["end_date"])).format("MM/YYYY");
            }
            if (x["certificate_start_date"] != null) {
              x["certificate_start_date"] = moment(
                new Date(x["certificate_start_date"])
              ).format("DD/MM/YYYY");
            }
            if (x["certificate_end_date"] != null) {
              x["certificate_end_date"] = moment(
                new Date(x["certificate_end_date"])
              ).format("DD/MM/YYYY");
            }
            //
            var idx = dictionarys.value[18].findIndex(
              (a) => a["specialization_id"] === x["specialized"]
            );
            if (x["specialized"] != null && idx != -1) {
              x["specialization_name"] =
                dictionarys.value[18][idx]["specialization_name"];
            }
            //
            var idx = dictionarys.value[12].findIndex(
              (a) => a["form_traning_id"] === x["form_traning_id"]
            );
            if (x["form_traning_id"] != null && idx != -1) {
              x["form_traning_name"] =
                dictionarys.value[12][idx]["form_traning_name"];
            }
            //
            var idx = dictionarys.value[13].findIndex(
              (a) => a["certificate_id"] === x["certificate_id"]
            );
            if (x["certificate_id"] != null && idx != -1) {
              x["certificate_name"] =
                dictionarys.value[13][idx]["certificate_name"];
            }
          });
          datachilds.value[2] = tbs[2];
        } else {
          datachilds.value[2] = [];
        }
        if (tbs[3] != null && tbs[3].length > 0) {
          tbs[3].forEach((x) => {
            if (x["start_date"] != null) {
              x["start_date"] = moment(new Date(x["start_date"])).format(
                "DD/MM/YYYY"
              );
            }
            if (x["end_date"] != null) {
              x["end_date"] = moment(new Date(x["end_date"])).format(
                "DD/MM/YYYY"
              );
            }
            //
            var idx = forms.value.findIndex((a) => a["value"] === x["form"]);
            if (x["form"] != null && idx != -1) {
              x["form"] = forms.value[idx]["title"];
            }
          });
          datachilds.value[3] = tbs[3];
        } else {
          datachilds.value[3] = [];
        }
        if (tbs[4] != null && tbs[4].length > 0) {
          tbs[4].forEach((x) => {
            if (x["start_date"] != null) {
              x["start_date"] = moment(new Date(x["start_date"])).format(
                "MM/YYYY"
              );
            }
            if (x["end_date"] != null) {
              x["end_date"] = moment(new Date(x["end_date"])).format("MM/YYYY");
            }
          });
          datachilds.value[4] = tbs[4];
        } else {
          datachilds.value[4] = [];
        }
        if (tbs[5] != null && tbs[5].length > 0) {
          profile.value["files"] = tbs[5];
        } else {
          profile.value["files"] = [];
        }
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
const initView2 = (rf) => {
  if (rf) {
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
            proc: "hrm_profile_task_get",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "profile_id", va: options.value["profile_id"] },
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
          task.value = tbs[0][0];
          var idx = typestatus.value.findIndex(
            (x) => x["value"] === task.value["status"]
          );
          if (idx != -1) {
            task.value["status_name"] = typestatus.value[idx]["title"];
            task.value["bg_color"] = typestatus.value[idx]["bg_color"];
            task.value["text_color"] = typestatus.value[idx]["text_color"];
          } else {
            task.value["status_name"] = "Chưa xác định";
            task.value["bg_color"] = "#bbbbbb";
            task.value["text_color"] = "#fff";
          }
          if (task.value["start_date"] != null) {
            task.value["start_date"] = moment(
              new Date(task.value["start_date"])
            ).format("DD/MM/YYYY");
          }
          if (task.value["end_date"] != null) {
            task.value["end_date"] = moment(
              new Date(task.value["end_date"])
            ).format("DD/MM/YYYY");
          }
          if (task.value["sign_date"] != null) {
            task.value["sign_date"] = moment(
              new Date(task.value["sign_date"])
            ).format("DD/MM/YYYY");
          }

          tbs[1].forEach((item) => {
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
            if (item["start_date"] != null) {
              item["start_date"] = moment(new Date(item["start_date"])).format(
                "DD/MM/YYYY"
              );
            }
            if (item["end_date"] != null) {
              item["end_date"] = moment(new Date(item["end_date"])).format(
                "DD/MM/YYYY"
              );
            }
            if (item["sign_date"] != null) {
              item["sign_date"] = moment(new Date(item["sign_date"])).format(
                "DD/MM/YYYY"
              );
            }
          });
          tasks.value = tbs[1];
        } else {
          task.value = {};
          tasks.value = [];
        }
      }
      swal.close();
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
};
const initView3 = (rf) => {
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
            proc: "hrm_contract_by_user",
            par: [
              { par: "profile_id", va: options.value["profile_id"] },
              { par: "search", va: options.value.search },
              { par: "pageNo", va: options.value.pageNo },
              { par: "pageSize", va: options.value.pageSize },
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
              item["effect"] = "";
              if (item["sign_date"] != null) {
                item["sign_date"] = moment(new Date(item["sign_date"])).format(
                  "DD/MM/YYYY"
                );
              }
              if (item["start_date"] != null) {
                item["start_date"] = moment(
                  new Date(item["start_date"])
                ).format("DD/MM/YYYY");
                item["effect"] += item["sign_date"];
              }
              if (item["end_date"] != null) {
                item["end_date"] = moment(new Date(item["end_date"])).format(
                  "DD/MM/YYYY"
                );
                item["effect"] += "<br/> đến <br/>" + item["sign_date"];
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
            contracts.value = data[0];
            if (data[1] != null && data[1].length > 0) {
              options.value.total = data[1][0].total;
            }
          } else {
            contracts.value = [];
            options.value.total = 0;
          }
        }
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
const initView6 = (rf) => {
  if (rf) {
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
            proc: "hrm_profile_insurance_get",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "profile_id", va: options.value["profile_id"] },
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
          insurance.value = tbs[0][0];
          if (insurance.value["status"] != null) {
            var idx = insurance_status.value.findIndex(
              (x) => x["status"] === insurance.value["status"]
            );
            if (idx != -1) {
              insurance.value["status_name"] = insurance_status.value["title"];
            }
          }
          if (insurance.value["insurance_province_id"] != null) {
            var idx = dictionarys.value.findIndex(
              (x) =>
                x["insurance_province_id"] ===
                insurance.value["insurance_province_id"]
            );
            if (idx != -1) {
              insurance.value["insurance_province_name"] =
                dictionarys.value[1]["insurance_province_name"];
            }
          }
        } else {
          insurance.value = {};
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          tbs[1].forEach((x) => {
            x["start_date"] = moment(new Date(x["start_date"])).format(
              "MM/YYYY"
            );
            x["total_payment"] = formatNumber(x["total_payment"], 0, ".", ".");
            x["company_payment"] = formatNumber(
              x["company_payment"],
              0,
              ".",
              "."
            );
            x["member_payment"] = formatNumber(
              x["member_payment"],
              0,
              ".",
              "."
            );
          });
          insurance_pays.value = tbs[1];
        } else {
          insurance_pays.value = [];
        }
        if (tbs[2] != null && tbs[2].length > 0) {
          tbs[2].forEach((x) => {
            x["received_file_date"] = moment(
              new Date(x["received_file_date"])
            ).format("DD/MM/YYYY");
            x["completed_date"] = moment(new Date(x["completed_date"])).format(
              "DD/MM/YYYY"
            );
            x["received_money_date"] = moment(
              new Date(x["received_money_date"])
            ).format("DD/MM/YYYY");
          });
          insurance_resolves.value = tbs[2];
        } else {
          insurance_resolves.value = [];
        }
      }
      swal.close();
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
};
const initView8 = (rf) => {
  if (rf) {
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
            proc: "hrm_profile_tranning_get",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "profile_id", va: options.value["profile_id"] },
              { par: "pageNo", va: options.value.pageNo },
              { par: "pageSize", va: options.value.pageSize },
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
          tbs[0].forEach((element) => {
            if (element["li_user_verify"]) {
              element["li_user_verify"] = JSON.parse(element["li_user_verify"]);
            } else {
              element["li_user_verify"] = [];
            }
            if (element["start_date"] != null) {
              element["start_date"] = moment(
                new Date(element["start_date"])
              ).format("DD/MM/YYYY");
            }
            if (element["end_date"] != null) {
              element["end_date"] = moment(
                new Date(element["end_date"])
              ).format("DD/MM/YYYY");
            }
          });
          trannings.value = tbs[0];
          if (tbs[1] != null && tbs[1].length > 0) {
            options.value.total = tbs[1][0].total;
          }
        } else {
          trannings.value = [];
          options.value.total = 0;
        }
      }
      swal.close();
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
};
const initView10 = (rf) => {
  if (rf) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  var type_files = null;
  if (options.value.type_files != null && options.value.type_files.length > 0) {
    type_files = options.value.type_files.map((x) => x["type_file"]).join(",");
  }
  var is_type_files = null;
  if (
    options.value.is_type_files != null &&
    options.value.is_type_files.length > 0
  ) {
    is_type_files = options.value.is_type_files
      .map((x) => x["is_type"])
      .join(",");
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_file_get",
            par: [
              { par: "profile_id", va: options.value["profile_id"] },
              { par: "search", va: options.value["search"] },
              { par: "pageNo", va: options.value.pageNo },
              { par: "pageSize", va: options.value.pageSize },
              { par: "type_files", va: type_files },
              { par: "is_type_files", va: is_type_files },
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
          tbs[0].forEach((item) => {
            if (item["file_size"] != null) {
              item["file_size"] = formatBytes(item["file_size"]);
            }
            if (item["created_date"] != null) {
              item["created_date"] = moment(
                new Date(item["created_date"])
              ).format("DD/MM/YYYY");
            }
            var idx = typeFiles.value.findIndex(
              (x) => x["is_type"] === item["is_type"]
            );
            if (idx !== -1) {
              item["is_type_name"] = typeFiles.value[idx]["title"];
            }
          });
          files.value = tbs[0];
          if (tbs[1] != null && tbs[1].length > 0) {
            options.value.total = tbs[1][0].total;
          }
        } else {
          files.value = [];
          options.value.total = 0;
        }
      }
      swal.close();
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
};
const initView11 = (rf) => {
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
            proc: "hrm_profile_receipt_get",
            par: [{ par: "profile_id", va: options.value["profile_id"] }],
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
            data[0].forEach((x) => {
              if (x["receipt_date"] != null) {
                x["receipt_date"] = moment(new Date(x["receipt_date"])).format(
                  "DD/MM/YYYY"
                );
              }
            });
            receipts.value = data[0];
            options.value.total = data[0].length;
            selectedNodes.value = receipts.value.filter((x) => x["is_active"]);
          } else {
            receipts.value = [];
            options.value.total = 0;
          }
        }
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
const initView12 = (rf) => {
  if (rf) {
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
            proc: "hrm_profile_helth_get",
            par: [{ par: "profile_id", va: options.value["profile_id"] }],
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
          health.value = tbs[0][0];
        } else {
          health.value = {};
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          tbs[1].forEach((x) => {
            if (x["injection_date"] != null) {
              x["injection_date"] = moment(
                new Date(x["injection_date"])
              ).format("DD/MM/YYYY");
            }
            if (x["injection_id"] != null) {
              var idx = injections.value.findIndex(
                (a) => a["id"] === x["injection_id"]
              );
              if (idx !== -1) {
                x["injection_name"] = injections.value[idx]["title"];
              }
            }
            if (x["type_vaccine"]) {
              var idx = type_vaccines.value.findIndex(
                (a) => a["id"] === x["type_vaccine"]
              );
              if (idx !== -1) {
                x["type_vaccine_name"] = type_vaccines.value[idx]["title"];
              }
            }
          });
          vaccines.value = tbs[1];
        } else {
          vaccines.value = [{ vaccine_id: -1 }];
        }
      }
      swal.close();
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
};
const replates = ref([]);
const initRelate = (rf) => {
  if (rf) {
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
            proc: "hrm_profile_relate_get",
            par: [{ par: "profile_id", va: options.value["profile_id"] }],
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
        replates.value = tbs;
      }
      swal.close();
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
};
const initData = () => {
  if (options.value.view === 1) {
    initDictionary1();
  } else if (options.value.view === 2) {
    initView2(true);
  } else if (options.value.view === 3) {
    initDictionary3();
    initView3(true);
  } else if (options.value.view === 6) {
    initDictionary6();
  } else if (options.value.view === 8) {
    initDictionary8();
  } else if (options.value.view === 10) {
    initView10(true);
  } else if (options.value.view === 11) {
    initView11(true);
  } else if (options.value.view === 12) {
    initDictionary12();
  }
};
onMounted(() => {
  if (route.params.id != null) {
    options.value["key_id"] = route.params.id;
    options.value["profile_id"] = route.query.id;
    initRelate();
    initDictionaryInsurance();
    initData();
  } else {
    router.back();
    return;
  }
});
//page
const onPage = (event) => {
  if (event.rows != options.value.pageSize) {
    options.value.pageSize = event.rows;
  }
  options.value.pageNo = event.page + 1;
  initData();
};
</script>
<template>
  <div class="surface-100 p-2">
    <Toolbar class="outline-none surface-0 border-none pb-1">
      <template #start>
        <span v-if="options.view === 10" class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText
            @keypress.enter="searchData()"
            v-model="options.search"
            type="text"
            spellcheck="false"
            :placeholder="'Tìm kiếm'"
          />
        </span>
        <Button
          v-if="options.view === 10"
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
          style="width: 400px"
        >
          <div class="grid formgrid m-0">
            <div
              class="col-12 md:col-12 p-0"
              :style="{
                minHeight: 'unset',
                maxHeight: 'calc(100vh - 300px)',
                overflow: 'auto',
              }"
            >
              <div class="row">
                <div class="col-12 md:col-12">
                  <div class="row">
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Loại file</label>
                        <MultiSelect
                          :options="type_files"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.type_files"
                          optionLabel="title"
                          placeholder="Chọn loại file"
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
                                      <span>{{ value.title }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(index, options.type_files);
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
                        <label>Vị trí file</label>
                        <MultiSelect
                          :options="is_type_files"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.is_type_files"
                          optionLabel="title"
                          placeholder="Chọn vị trí"
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
                                      <span>{{ value.title }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(
                                          index,
                                          options.is_type_files
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
        <ul class="flex p-0 m-0 mr-2" style="list-style: none">
          <li>
            <Button
              @click="goBack()"
              type="button"
              label="Quay lại"
              icon="pi pi-arrow-left"
              class="p-button p-button-outlined p-button-secondary"
            />
          </li>
        </ul>
        <Button
          @click="toggleEdit"
          label="Cập nhật thay đổi thông tin"
          class="p-button-warning mr-2"
          icon="pi pi-file-excel"
          aria-haspopup="true"
          aria-controls="overlay_Export"
        >
          <div>
            <span class="mr-2"><i class="pi pi-user-edit"></i></span>
            <b class="mr-2">Cập nhật thay đổi thông tin</b>
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
          @click="
            togglePrints($event);
            $event.stopPropagation();
          "
          class="p-button-outlined p-button-secondary p-button-custom'"
        >
          <span class="mr-2"
            ><font-awesome-icon icon="fa-solid fa-print"
          /></span>
          <span class="mr-2">In</span>
          <span><i class="pi pi-chevron-down"></i></span>
        </Button>
        <OverlayPanel
          :showCloseIcon="false"
          ref="menuButPrints"
          appendTo="body"
          class="p-0 m-0"
          id="overlay_More"
          style="min-width: max-content"
        >
          <ul class="m-0 p-0" style="list-style: none">
            <li
              v-for="(value, key) in itemButPrints"
              :key="key"
              @click="changeView(value.view)"
              class="item-menu"
              :class="{
                'item-menu-highlight': value.view === options.view,
              }"
            >
              <div>
                <span :class="{ 'mr-2': value.label != null }"
                  ><font-awesome-icon :icon="value.icon"
                /></span>
                <span>{{ value.label }}</span>
              </div>
            </li>
          </ul>
        </OverlayPanel>
      </template>
    </Toolbar>
    <Toolbar class="outline-none surface-0 border-none pt-0">
      <template #start>
        <div style="height: 36px; display: flex; align-items: center">
          <SelectButton
            :options="views"
            v-model="options.view"
            @change="changeView(options.view)"
            optionValue="view"
            optionLabel="view"
            dataKey="view"
            aria-labelledby="custom"
            class="selectbutton-custom"
          >
            <template #option="slotProps">
              <b>
                <span
                  v-if="slotProps.option.icon != null"
                  :class="{ 'mr-2': slotProps.option.title != null }"
                >
                  <font-awesome-icon :icon="slotProps.option.icon" />
                </span>
                <span> {{ slotProps.option.title }}</span>
              </b>
            </template>
          </SelectButton>
          <!-- <span class="p-buttonset">
            <Button
              @click="
                toggleMores($event);
                $event.stopPropagation();
              "
              :class="{
                'p-button-outlined p-button-secondary p-button-custom':
                  options.view < 11 || options.view > 12,
              }"
              :style="{
                background:
                  options.view < 11 || options.view > 12 ? '#ffffff' : '',
                borderColor:
                  options.view < 11 || options.view > 12 ? '#ced4da' : '',
                color: options.view < 11 || options.view > 12 ? '#495057' : '',
                transition:
                  'background-color 0.2s, color 0.2s, border-color 0.2s,  boxShadow 0.2s',
                height: '30px',
              }"
            >
              <font-awesome-icon icon="fa-solid fa-ellipsis" />
            </Button>
          </span>
          <OverlayPanel
            :showCloseIcon="false"
            ref="menuButMores"
            appendTo="body"
            class="p-0 m-0"
            id="overlay_More"
            style="min-width: max-content"
          >
            <ul class="m-0 p-0" style="list-style: none">
              <li
                v-for="(value, key) in itemButMores"
                :key="key"
                @click="changeView(value.view)"
                class="item-menu"
                :class="{
                  'item-menu-highlight': value.view === options.view,
                }"
              >
                <div>
                  <span :class="{ 'mr-2': value.label != null }"
                    ><font-awesome-icon :icon="value.icon"
                  /></span>
                  <span>{{ value.label }}</span>
                </div>
              </li>
            </ul>
          </OverlayPanel> -->
        </div>
      </template>
      <template #end> </template>
    </Toolbar>
    <div class="d-lang-table" style="border-top: solid 1px rgba(0, 0, 0, 0.1)">
      <div class="flex">
        <div class="flex-1">
          <div class="d-lang-table-1">
            <div v-show="options.view === 1" class="f-full">
              <div class="row p-2">
                <div class="col-12 md:col-12 p-0">
                  <!-- 1. Thông tin chung -->
                  <Accordion class="w-full mb-2 header-padding-y-0" :activeIndex="0">
                    <AccordionTab>
                      <template #header>
                        <Toolbar class="w-full custoolbar p-0 font-bold py-0">
                          <template #start>
                            <!-- <i class="pi pi-users mr-2"></i> -->
                            <span>1. Thông tin chung</span>
                          </template>
                          <template #end>
                            <div class="relative relative-hover" :style="{ margin: '0.5rem 0' }">
                              <Avatar
                                v-if="profile.relate"
                                v-bind:label="
                                  profile.relate.avatar
                                    ? ''
                                    : (profile.relate.profile_user_name ?? '')
                                        .substring(0, 1)
                                        .toUpperCase()
                                "
                                v-bind:image="
                                  profile.relate.avatar
                                    ? basedomainURL + profile.relate.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                :style="{
                                  background: bgColor[1 % 7],
                                  color: '#ffffff',
                                  width: '2rem',
                                  height: '2rem',
                                  fontSize: '1rem',
                                  borderRadius: '50%',
                                  fontSize: '1rem !important',
                                }"
                                size="xlarge"
                                class="border-radius"
                              />
                              <span class="absolute" :style="{ color: 'red', bottom: '-3px', right: '-3px' }"><i class="pi pi-heart-fill"></i></span>
                              <div
                                v-if="profile.relate"
                                class="absolute absolute-hover"
                                :style="{
                                  backgroundColor: '#fff',
                                  minHeight: 'unset',
                                  top: '100%',
                                  right: '0',
                                  width: '350px',
                                  borderRadius: '3px',
                                  border: 'solid 1px rgba(0,0,0,0.1)',
                                  padding: '0.75rem',
                                }"
                              >
                                <div class="flex">
                                  <div class="mr-2 format-center">
                                    <div>
                                      <Avatar
                                      v-bind:label="
                                        profile.relate.avatar
                                          ? ''
                                          : (
                                              profile.relate
                                                .profile_user_name ?? ''
                                            )
                                              .substring(0, 1)
                                              .toUpperCase()
                                      "
                                      v-bind:image="
                                        profile.relate.avatar
                                          ? basedomainURL +
                                            profile.relate.avatar
                                          : basedomainURL +
                                            '/Portals/Image/noimg.jpg'
                                      "
                                      :style="{
                                        background: bgColor[1 % 7],
                                        color: '#ffffff',
                                        width: '5rem',
                                        height: '5rem',
                                        fontSize: '1.5rem',
                                        borderRadius: '5px',
                                        fontSize: '1.5rem !important',
                                      }"
                                      size="xlarge"
                                      class="border-radius"
                                    />
                                    <div class="description format-center" :style="{fontSize: '11px'}">{{ profile.relate.relate_time }}</div>
                                    </div>
                                  </div>
                                  <div>
                                    <div class="mb-2"><span :style="{ color: 'rgb(0, 90, 158)' }">Đã kết hôn với</span> <b>{{profile.relate.profile_user_name}}</b></div>
                                    <div class="description">
                                      <span>{{
                                        profile.relate.department_name
                                      }}</span>
                                    </div>
                                    <div class="description">
                                      <span>{{
                                        profile.relate.work_position_name
                                      }}</span>
                                    </div>
                                    <div class="description">
                                      <span>{{
                                        profile.relate.position_name
                                      }}</span>
                                    </div>
                                  </div>
                                </div>
                              </div>
                            </div>
                          </template>
                        </Toolbar>
                      </template>
                      <div class="col-12 md:col-12 p-0">
                        <div class="row">
                          <div class="col-3 md:col-3 format-center">
                            <div class="form-group m-0">
                              <div
                                class="inputanh2 relative mb-2"
                                style="margin: 0 auto"
                              >
                                <img
                                  id="avatar"
                                  v-bind:src="
                                    profile.avatar
                                      ? basedomainURL + profile.avatar
                                      : basedomainURL +
                                        '/Portals/Image/noimg.jpg'
                                  "
                                />
                              </div>
                            </div>
                          </div>
                          <div class="col-9 md:col-9 p-0">
                            <div class="row">
                              <div class="col-6 md:col-6">
                                <div class="form-group m-0">
                                  <label>
                                    Mã nhân sự:
                                    <b class="m-0" :style="{ color: '#2ECC71' }">{{
                                      profile.profile_id
                                    }}</b>
                                  </label>
                                </div>
                              </div>
                              <div class="col-6 md:col-6">
                                <div class="form-group m-0">
                                  <label
                                    >Mã chấm công:
                                    <span class="description-2">{{
                                      profile.check_in_id
                                    }}</span></label
                                  >
                                </div>
                              </div>
                              <div class="col-6 md:col-6">
                                <div class="form-group m-0">
                                  <label
                                    >Mã quản lý cấp trên:
                                    <span class="description-2">{{
                                      profile.superior_id
                                    }}</span></label
                                  >
                                </div>
                              </div>
                              <div class="col-6 md:col-6">
                                <div class="form-group m-0">
                                  <label
                                    >Ngày tuyển dụng:
                                    <span class="description-2">{{
                                      profile.recruitment_date
                                    }}</span></label
                                  >
                                </div>
                              </div>
                              <div class="col-6 md:col-6">
                                <div class="form-group m-0">
                                  <label
                                    >Họ và tên:
                                    <span class="description-2">{{
                                      profile.profile_user_name
                                    }}</span></label
                                  >
                                </div>
                              </div>
                              <div class="col-6 md:col-6">
                                <div class="form-group m-0">
                                  <label
                                    >Tên gọi khác:
                                    <span class="description-2">{{
                                      profile.profile_nick_name
                                    }}</span></label
                                  >
                                </div>
                              </div>
                              <div class="col-6 md:col-6">
                                <div class="form-group m-0">
                                  <label
                                    >Ngày sinh:
                                    <span class="description-2">{{
                                      profile.birthday
                                    }}</span></label
                                  >
                                </div>
                              </div>
                              <div class="col-6 md:col-6">
                                <div class="form-group m-0">
                                  <label
                                    >Giới tính:
                                    <span class="description-2">{{
                                      profile.gender
                                    }}</span></label
                                  >
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                      <div class="col-12 md:col-12">
                        <div class="form-group m-0">
                          <label
                            >Nơi sinh:
                            <span class="description-2">{{
                              profile.select_birthplace
                            }}</span></label
                          >
                        </div>
                      </div>
                      <div class="col-12 md:col-12">
                        <div class="form-group m-0">
                          <label
                            >Quê quán:
                            <span class="description-2">{{
                              profile.select_birthplace_origin
                            }}</span></label
                          >
                        </div>
                      </div>
                      <div class="col-12 md:col-12">
                        <div class="form-group m-0">
                          <label
                            >Nơi đăng ký HKTT:
                            <span class="description-2">{{
                              profile.select_place_register_permanent
                            }}</span></label
                          >
                        </div>
                      </div>
                      <div class="col-12 md:col-12 p-0">
                        <div class="row">
                          <div class="col-4 md:col-4">
                            <div class="form-group m-0">
                              <label
                                >Loại giấy tờ:
                                <span class="description-2">{{
                                  profile.identity_papers_name
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-4 md:col-4">
                            <div class="form-group m-0">
                              <label
                                >Số:
                                <span class="description-2">{{
                                  profile.identity_papers_code
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-4 md:col-4">
                            <div class="form-group m-0">
                              <label
                                >Ngày cấp:
                                <span class="description-2">{{
                                  profile.identity_date_issue
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-4 md:col-4">
                            <div class="form-group m-0">
                              <label
                                >Nơi cấp:
                                <span class="description-2">{{
                                  profile.identity_papers_name
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-4 md:col-4">
                            <div class="form-group m-0">
                              <label
                                >Quốc tịch:
                                <span class="description-2">{{
                                  profile.nationality_name
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-4 md:col-4">
                            <div class="form-group m-0">
                              <label
                                >Tình trạng hôn nhân:
                                <span class="description-2">{{
                                  profile.marital_status
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-4 md:col-4">
                            <div class="form-group m-0">
                              <label
                                >Dân tộc:
                                <span class="description-2">{{
                                  profile.ethnic_name
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-4 md:col-4">
                            <div class="form-group m-0">
                              <label
                                >Tôn giáo:
                                <span class="description-2">{{
                                  profile.religion_name
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-4 md:col-4">
                            <div class="form-group m-0">
                              <label
                                >Mã số thuế:
                                <span class="description-2">{{
                                  profile.tax_code
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-4 md:col-4">
                            <div class="form-group m-0">
                              <label
                                >Ngân hàng:
                                <span class="description-2">{{
                                  profile.bank_name
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-4 md:col-4">
                            <div class="form-group m-0">
                              <label
                                >Số tài khoản:
                                <span class="description-2">{{
                                  profile.bank_number
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-4 md:col-4">
                            <div class="form-group m-0">
                              <label
                                >Tên tài khoản:
                                <span class="description-2">{{
                                  profile.bank_account
                                }}</span></label
                              >
                            </div>
                          </div>
                        </div>
                      </div>
                    </AccordionTab>
                  </Accordion>
                  <!-- 2. Trình độ học vấn -->
                  <Accordion class="w-full mb-2" :activeIndex="0">
                    <AccordionTab>
                      <template #header>
                        <!-- <i class="pi pi-book mr-2"></i> -->
                        <span>2. Trình độ học vấn</span>
                      </template>
                      <div class="col-12 md:col-12">
                        <div class="row">
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Trình độ phổ thông:
                                <span class="description-2">{{
                                  profile.cultural_level_name
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Trình độ học vấn cao nhất:
                                <span class="description-2">{{
                                  profile.academic_level_name
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Chuyên ngành học:
                                <span class="description-2">{{
                                  profile.specialization_name
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Quản lý nhà nước:
                                <span class="description-2">{{
                                  profile.management_state_name
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-4 md:col-4">
                            <div class="form-group m-0">
                              <label
                                >Lý luận chính trị:
                                <span class="description-2">{{
                                  profile.political_theory_name
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-4 md:col-4">
                            <div class="form-group m-0">
                              <label
                                >Ngoại ngữ:
                                <span class="description-2">{{
                                  profile.language_level_name
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-4 md:col-4">
                            <div class="form-group m-0">
                              <label
                                >Tin học:
                                <span class="description-2">{{
                                  profile.informatic_level_name
                                }}</span></label
                              >
                            </div>
                          </div>
                        </div>
                      </div>
                    </AccordionTab>
                  </Accordion>
                  <!-- 3. Thông tin liên hệ -->
                  <Accordion class="w-full mb-2" :activeIndex="0">
                    <AccordionTab>
                      <template #header>
                        <!-- <i class="pi pi-info-circle mr-2"></i> -->
                        <span>3. Thông tin liên hệ</span>
                      </template>
                      <div class="col-12 md:col-12">
                        <div class="row">
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Số điện thoại:
                                <span class="description-2">{{
                                  profile.phone
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Email:
                                <span class="description-2">{{
                                  profile.email
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-12 md:col-12">
                            <div class="form-group m-0">
                              <label
                                >Thường trú:
                                <span class="description-2">{{
                                  profile.place_permanent
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-12 md:col-12">
                            <div class="form-group m-0">
                              <label
                                >Chỗ ở hiện nay:
                                <span class="description-2">{{
                                  profile.place_residence
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-12 md:col-12">
                            <div class="form-group m-0">
                              <label class="m-0">Khi cần báo tin cho:</label>
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Họ và tên:
                                <span class="description-2">{{
                                  profile.involved_name
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Số điện thoại:
                                <span class="description-2">{{
                                  profile.involved_phone
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-12 md:col-12">
                            <div class="form-group m-0">
                              <label
                                >Địa chỉ:
                                <span class="description-2">{{
                                  profile.involved_place
                                }}</span></label
                              >
                            </div>
                          </div>
                        </div>
                      </div>
                    </AccordionTab>
                  </Accordion>
                  <!-- 4. Thông tin gia đình, người phụ thuộc -->
                  <Accordion class="w-full padding-0 mb-2" :activeIndex="0">
                    <AccordionTab>
                      <template #header>
                        <Toolbar class="w-full custoolbar p-0 font-bold">
                          <template #start>
                            <!-- <i class="pi pi-users mr-2"></i> -->
                            <span
                              >4. Thông tin gia đình, người phụ thuộc</span
                            ></template
                          >
                        </Toolbar>
                      </template>
                      <div class="col-12 md:col-12 p-0">
                        <DataTable
                          :value="datachilds[1]"
                          :scrollable="true"
                          :lazy="true"
                          :rowHover="true"
                          :showGridlines="true"
                          scrollDirection="both"
                          style="display: grid"
                          class="empty-full"
                        >
                          <Column
                            field="is_root"
                            header=""
                            headerStyle="text-align:center;width:30px;height:50px"
                            bodyStyle="text-align:center;width:30px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span
                                v-if="slotProps.data.is_root"
                                v-tooltip.right="'Theo lý lịch'"
                                ><i class="pi pi-flag-fill"></i
                              ></span>
                              <span
                                v-if="!slotProps.data.is_root"
                                v-tooltip.right="'Bổ sung'"
                                ><i class="pi pi-pencil"></i
                              ></span>
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
                              <span>{{ slotProps.data.relative_name }}</span>
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
                                <span>{{
                                  slotProps.data.relationship_name
                                }}</span>
                              </div>
                            </template>
                          </Column>
                          <Column
                            field="identification_date_issue"
                            header="Năm sinh"
                            headerStyle="text-align:center;width:120px;height:50px"
                            bodyStyle="text-align:center;width:120px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span>{{
                                slotProps.data.identification_date_issue
                              }}</span>
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
                              <span>{{ slotProps.data.phone }}</span>
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
                              <span>{{ slotProps.data.tax_code }}</span>
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
                              <span>{{
                                slotProps.data.identification_citizen
                              }}</span>
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
                              {{ slotProps.data.identification_date_issue }}
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
                              {{ slotProps.data.identification_place_issue }}
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
                                <span>{{ slotProps.data.dependent_name }}</span>
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
                              <span>{{ slotProps.data.start_date }}</span>
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
                              <span>{{ slotProps.data.end_date }}</span>
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
                              <span>{{ slotProps.data.info }}</span>
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
                              <span>{{ slotProps.data.note }}</span>
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
                    </AccordionTab>
                  </Accordion>
                  <!-- 5. Quá trình đào tạo, bồi dưỡng về chuyên môn, nghiệp vụ, lý luận chính trị, ngoại ngữ, tin học -->
                  <Accordion class="w-full padding-0 mb-2" :activeIndex="0">
                    <AccordionTab>
                      <template #header>
                        <Toolbar class="w-full custoolbar p-0 font-bold">
                          <template #start>
                            <!-- <i class="pi pi-replay mr-2"></i> -->
                            <span
                              >5. Quá trình đào tạo, bồi dưỡng về chuyên môn,
                              nghiệp vụ, lý luận chính trị, ngoại ngữ, tin
                              học</span
                            ></template
                          >
                        </Toolbar>
                      </template>
                      <div class="col-12 md:col-12 p-0">
                        <div>
                          <DataTable
                            :value="datachilds[2]"
                            :scrollable="true"
                            :lazy="true"
                            :rowHover="true"
                            :showGridlines="true"
                            scrollDirection="both"
                            style="display: grid"
                            class="empty-full"
                          >
                            <Column
                              field="university_name"
                              header="Tên trường"
                              headerStyle="text-align:center;width:180px;height:50px"
                              bodyStyle="text-align:center;width:180px;"
                              class="align-items-center justify-content-center text-center"
                            >
                              <template #body="slotProps">
                                <span>{{
                                  slotProps.data.university_name
                                }}</span>
                              </template>
                            </Column>
                            <Column
                              field="specialized"
                              header="Chuyên ngành"
                              headerStyle="text-align:center;width:170px;height:50px"
                              bodyStyle="text-align:center;width:170px;"
                              class="align-items-center justify-content-center text-center"
                            >
                              <template #body="slotProps">
                                <span>{{
                                  slotProps.data.specialization_name
                                }}</span>
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
                                <span>{{ slotProps.data.start_date }}</span>
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
                                <span>{{ slotProps.data.end_date }}</span>
                              </template>
                            </Column>
                            <Column
                              field="form_traning_id"
                              header="Hình thức đào tạo"
                              headerStyle="text-align:center;width:170px;height:50px"
                              bodyStyle="text-align:center;width:170px;"
                              class="align-items-center justify-content-center text-center"
                            >
                              <template #body="slotProps">
                                <span>{{
                                  slotProps.data.form_traning_name
                                }}</span>
                              </template>
                            </Column>
                            <Column
                              field="certificate_id"
                              header="Văn bằng, chứng chỉ"
                              headerStyle="text-align:center;width:170px;height:50px"
                              bodyStyle="text-align:center;width:170px;"
                              class="align-items-center justify-content-center text-center"
                            >
                              <template #body="slotProps">
                                <span>{{
                                  slotProps.data.certificate_name
                                }}</span>
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
                                <span>{{
                                  slotProps.data.certificate_start_date
                                }}</span>
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
                                <span>{{
                                  slotProps.data.certificate_end_date
                                }}</span>
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
                                <span>{{
                                  slotProps.data.certificate_key_code
                                }}</span>
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
                                <span>{{
                                  slotProps.data.certificate_version
                                }}</span>
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
                                <span>{{
                                  slotProps.data.certificate_release_time
                                }}</span>
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
                  </Accordion>
                  <!-- 6. Lịch sử Đảng viên -->
                  <Accordion class="w-full padding-0 mb-2" :activeIndex="0">
                    <AccordionTab>
                      <template #header>
                        <Toolbar class="w-full custoolbar p-0 font-bold">
                          <template #start>
                            <!-- <i class="pi pi-replay mr-2"></i> -->
                            <span>6. Lịch sử Đảng viên</span></template
                          >
                        </Toolbar>
                      </template>
                      <div class="col-12 md:col-12 p-0">
                        <div>
                          <DataTable
                            :value="datachilds[3]"
                            :scrollable="true"
                            :lazy="true"
                            :rowHover="true"
                            :showGridlines="true"
                            scrollDirection="both"
                            style="display: grid"
                            class="empty-full"
                          >
                            <Column
                              field="card_number"
                              header="Số thẻ"
                              headerStyle="text-align:center;width:180px;height:50px"
                              bodyStyle="text-align:center;width:180px;"
                              class="align-items-center justify-content-center text-center"
                            >
                              <template #body="slotProps">
                                <span>{{ slotProps.data.card_number }}</span>
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
                                <span>{{ slotProps.data.form }}</span>
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
                                <span>{{ slotProps.data.start_date }}</span>
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
                                <span>{{ slotProps.data.end_date }}</span>
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
                                <span>{{
                                  slotProps.data.admission_place
                                }}</span>
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
                                <span>{{ slotProps.data.transfer_place }}</span>
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
                  </Accordion>
                  <!-- 7. Lịch sử tham gia quân đội -->
                  <Accordion class="w-full mb-2" :activeIndex="0">
                    <AccordionTab>
                      <template #header>
                        <!-- <i class="pi pi-chart-line mr-2"></i> -->
                        <span>7. Lịch sử tham gia quân đội</span>
                      </template>
                      <div class="col-12 md:col-12">
                        <div class="row">
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Ngày nhập ngũ:
                                <span class="description-2">{{
                                  profile.military_start_date
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Ngày xuất ngũ:
                                <span class="description-2">{{
                                  profile.military_end_date
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Quân hàm cao nhất:
                                <span class="description-2">{{
                                  profile.military_rank
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Danh hiệu cao nhất:
                                <span class="description-2">{{
                                  profile.military_title
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Sở trường công tác:
                                <span class="description-2">{{
                                  profile.military_forte
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Sức khỏe:
                                <span class="description-2">{{
                                  profile.military_health
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Khen thưởng:
                                <span class="description-2">{{
                                  profile.military_reward
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-6 md:col-6">
                            <div class="form-group m-0">
                              <label
                                >Kỷ luật:
                                <span class="description-2">{{
                                  profile.military_discipline
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-6 md:col-6 m-0">
                            <div class="form-group m-0">
                              <label
                                >Thương binh hạng:
                                <span class="description-2">{{
                                  profile.military_veterans_rank
                                }}</span></label
                              >
                            </div>
                          </div>
                          <div class="col-6 md:col-6 m-0">
                            <div class="form-group m-0">
                              <label
                                >Con gia đình chính sách:
                                <span class="description-2">{{
                                  profile.military_policy_family
                                }}</span></label
                              >
                            </div>
                          </div>
                        </div>
                      </div>
                    </AccordionTab>
                  </Accordion>
                  <!-- 8. Kinh nghiệm làm việc -->
                  <Accordion class="w-full padding-0 mb-2" :activeIndex="0">
                    <AccordionTab>
                      <template #header>
                        <Toolbar class="w-full custoolbar p-0 font-bold">
                          <template #start>
                            <span>8. Kinh nghiệm làm việc</span></template
                          >
                        </Toolbar>
                      </template>
                      <div class="col-12 md:col-12 p-0">
                        <div>
                          <DataTable
                            :value="datachilds[4]"
                            :scrollable="true"
                            :lazy="true"
                            :rowHover="true"
                            :showGridlines="true"
                            scrollDirection="both"
                            style="display: grid"
                            class="empty-full"
                          >
                            <Column
                              field="start_date"
                              header="Từ tháng, năm"
                              headerStyle="text-align:center;width:120px;height:50px"
                              bodyStyle="text-align:center;width:120px;"
                              class="align-items-center justify-content-center text-center"
                            >
                              <template #body="slotProps">
                                <span>{{ slotProps.data.start_date }}</span>
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
                                <span>{{ slotProps.data.end_date }}</span>
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
                                <span>{{ slotProps.data.company }}</span>
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
                                <span>{{ slotProps.data.role }}</span>
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
                                <span>{{ slotProps.data.reference_name }}</span>
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
                                <span>{{
                                  slotProps.data.reference_phone
                                }}</span>
                              </template>
                            </Column>
                            <Column
                              field="description-2"
                              header="Mô tả công việc"
                              headerStyle="text-align:center;width:200px;height:50px"
                              bodyStyle="text-align:center;width:200px;"
                              class="align-items-center justify-content-center text-center"
                            >
                              <template #body="slotProps">
                                <span>{{
                                  slotProps.data.description - 2
                                }}</span>
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
                  </Accordion>
                  <!-- 9. Đặc điểm lịch sử bản thân -->
                  <Accordion class="w-full mb-2" :activeIndex="0">
                    <AccordionTab>
                      <template #header>
                        <!-- <i class="pi pi-chart-line mr-2"></i> -->
                        <span>9. Đặc điểm lịch sử bản thân</span>
                      </template>
                      <div class="col-12 md:col-12">
                        <div class="form-group m-0">
                          <label
                            >Thông tin 1:
                            <span class="description-2">{{
                              profile.biography_first
                            }}</span></label
                          >
                        </div>
                      </div>
                      <div class="col-12 md:col-12">
                        <div class="form-group m-0">
                          <label
                            >Thông tin 2:
                            <span class="description-2">{{
                              profile.biography_second
                            }}</span></label
                          >
                        </div>
                      </div>
                      <div class="col-12 md:col-12">
                        <div class="form-group m-0">
                          <label
                            >Thông tin 3:
                            <span class="description-2">{{
                              profile.biography_third
                            }}</span></label
                          >
                        </div>
                      </div>
                    </AccordionTab>
                  </Accordion>
                  <!-- 10.	Đính kèm khác (file số hóa liên quan) -->
                  <Accordion class="w-full mb-2" :activeIndex="0">
                    <AccordionTab>
                      <template #header>
                        <!-- <i class="pi pi-chart-line mr-2"></i> -->
                        <span> 10. Đính kèm khác (file số hóa liên quan)</span>
                      </template>
                      <div class="col-12 md:col-12">
                        <div class="form-group m-0">
                          <div
                            v-if="
                              profile.files != null && profile.files.length > 0
                            "
                          >
                            <DataView
                              :lazy="true"
                              :value="profile.files"
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
                <div class="col-12 md:col-12 p-0 mt-2">
                  <div class="form-group">
                    <label
                      >Ghi chú:
                      <span class="description-2">{{
                        profile.note
                      }}</span></label
                    >
                  </div>
                </div>
              </div>
            </div>
            <div v-show="options.view === 2" class="f-full">
              <div class="row p-2">
                <div class="col-12 md:col-12 p-0">
                  <Accordion class="w-full mb-2" :activeIndex="0">
                    <AccordionTab>
                      <template #header>
                        <span>Công việc hiện tại</span>
                      </template>
                      <div class="row">
                        <div class="col-6 md:col-6">
                          <div class="form-group">
                            <label
                              >Trạng thái:
                              <span class="description-2">{{
                                task.status_name
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-6 md:col-6">
                          <div class="form-group">
                            <label
                              >Phòng ban:
                              <span class="description-2">{{
                                task.department_name
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-6 md:col-6">
                          <div class="form-group">
                            <label
                              >Vị trí:
                              <span class="description-2">{{
                                task.work_position_name
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-6 md:col-6">
                          <div class="form-group">
                            <label
                              >Chức vụ:
                              <span class="description-2">{{
                                task.position_name
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-6 md:col-6">
                          <div class="form-group">
                            <label
                              >Ngày hiệu lực:
                              <span class="description-2">{{
                                task.start_date
                              }}</span>
                              <span v-if="task.start_date && task.end_date">
                                -
                              </span>
                              <span
                                v-if="task.end_date"
                                class="description-2"
                                >{{ task.end_date }}</span
                              ></label
                            >
                          </div>
                        </div>
                        <div class="col-6 md:col-6">
                          <div class="form-group">
                            <label
                              >Ngày ký hợp đồng chính thức:
                              <span class="description-2">{{
                                task.sign_date
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-6 md:col-6">
                          <div class="form-group">
                            <label
                              >Công việc chuyên môn:
                              <span class="description-2">{{
                                task.professional_work_name
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-6 md:col-6">
                          <div class="form-group">
                            <label
                              >Loại hợp đồng:
                              <span class="description-2">{{
                                task.contract_name
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-6 md:col-6">
                          <div class="form-group">
                            <label
                              >Hình thức:
                              <span class="description-2">{{
                                task.formality_name
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-6 md:col-6">
                          <div class="form-group">
                            <label
                              >Ngạch lương:
                              <span class="description-2">{{
                                task.wage_name
                              }}</span></label
                            >
                          </div>
                        </div>
                      </div>
                    </AccordionTab>
                  </Accordion>
                </div>
                <div class="col-12 md:col-12 p-0">
                  <Accordion class="w-full padding-0 mb-2" :activeIndex="0">
                    <AccordionTab>
                      <template #header>
                        <span>Quá trình làm việc</span>
                      </template>
                      <div>
                        <DataTable
                          :value="tasks"
                          :scrollable="true"
                          :lazy="true"
                          :rowHover="true"
                          :showGridlines="true"
                          scrollDirection="both"
                          style="display: grid"
                          class="empty-full"
                        >
                          <Column
                            field="start_date"
                            header="Ngày hiệu lực"
                            headerStyle="text-align:center;width:120px;height:50px"
                            bodyStyle="text-align:center;width:120px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span v-html="slotProps.data.start_date"></span>
                            </template>
                          </Column>
                          <Column
                            field="start_date"
                            header="Ngày hết hạn"
                            headerStyle="text-align:center;width:120px;height:50px"
                            bodyStyle="text-align:center;width:120px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span v-html="slotProps.data.end_date"></span>
                            </template>
                          </Column>

                          <Column
                            field="department_name"
                            header="Phòng ban"
                            headerStyle="text-align:center;width:250px;height:50px"
                            bodyStyle="text-align:center;width:250px;"
                            class="align-items-center justify-content-center text-center"
                          />
                          <Column
                            field="work_position_name"
                            header="Vị trí"
                            headerStyle="text-align:center;width:200px;height:50px"
                            bodyStyle="text-align:center;width:200px;"
                            class="align-items-center justify-content-center text-center"
                          />
                          <Column
                            field="position_name"
                            header="Chức vụ"
                            headerStyle="text-align:center;width:150px;height:50px"
                            bodyStyle="text-align:center;width:150px;"
                            class="align-items-center justify-content-center text-center"
                          />
                          <Column
                            field="type_contract_name"
                            header="Loại hợp đồng"
                            headerStyle="text-align:center;width:120px;height:50px"
                            bodyStyle="text-align:center;width:120px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              {{ slotProps.data.type_contract_name }}
                            </template>
                          </Column>
                          <Column
                            field="contract_no"
                            header="Mã HĐ"
                            headerStyle="text-align:center;width:80px;height:50px"
                            bodyStyle="text-align:center;width:80px;"
                            class="align-items-center justify-content-center text-center"
                          />
                          <Column
                            field="status"
                            header="Trạng thái"
                            headerStyle="text-align:center;width:140px;height:50px"
                            bodyStyle="text-align:center;width:140px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <div
                                class="m-2"
                                aria:haspopup="true"
                                aria-controls="overlay_panel_status"
                              >
                                <Button
                                  :label="slotProps.data.status_name"
                                  :style="{
                                    border: slotProps.data.bg_color,
                                    backgroundColor: slotProps.data.bg_color,
                                    color: slotProps.data.text_color,
                                  }"
                                />
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
                    </AccordionTab>
                  </Accordion>
                </div>
              </div>
            </div>
            <div v-show="options.view === 3" class="f-full">
              <div class="d-lang-table-1 p-2">
                <DataTable
                  @page="onPage($event)"
                  @rowSelect="selectRow"
                  :value="contracts"
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
                  <Column
                    field="contract_no"
                    header="Mã HĐ"
                    headerStyle="text-align:center;max-width:80px;height:50px"
                    bodyStyle="text-align:center;max-width:80px;"
                    class="align-items-center justify-content-center text-center"
                  />
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
                      <span v-html="slotProps.data.start_date"></span>
                    </template>
                  </Column>
                  <Column
                    field="start_date"
                    header="Ngày hết hạn"
                    headerStyle="text-align:center;max-width:120px;height:50px"
                    bodyStyle="text-align:center;max-width:120px;"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #body="slotProps">
                      <span v-html="slotProps.data.end_date"></span>
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
                    field="created_date"
                    header="Ngày/Người lập"
                    headerStyle="text-align:center;max-width:130px;height:50px"
                    bodyStyle="text-align:center;max-width:130px;"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #body="slotProps">
                      <span class="mr-2">{{
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
                          :style="{
                            background:
                              bgColor[slotProps.data.created_is_order % 7],
                            color: '#ffffff',
                            width: '2rem',
                            height: '2rem',
                            fontSize: '1rem',
                          }"
                          class="text-avatar"
                          size="xlarge"
                          shape="circle"
                          v-tooltip.top="slotProps.data.full_name"
                        />
                      </div>
                    </template>
                  </Column>
                  <Column
                    field="status"
                    header="Trạng thái"
                    headerStyle="text-align:center;max-width:140px;height:50px"
                    bodyStyle="text-align:center;max-width:140px;"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #body="slotProps">
                      <div
                        class="m-2"
                        aria:haspopup="true"
                        aria-controls="overlay_panel_status"
                      >
                        <Button
                          :label="slotProps.data.status_name"
                          :style="{
                            border: slotProps.data.bg_color,
                            backgroundColor: slotProps.data.bg_color,
                            color: slotProps.data.text_color,
                          }"
                        />
                      </div>
                    </template>
                  </Column>
                  <template #empty>
                    <div
                      class="align-items-center justify-content-center p-4 text-center m-auto"
                      :style="{
                        display: 'flex',
                        width: '100%',
                        height: 'calc(100vh - 303px)',
                        backgroundColor: '#fff',
                      }"
                    >
                      <div v-if="!options.loading && options.total == 0">
                        <img
                          src="../../../../assets/background/nodata.png"
                          height="144"
                        />
                        <h3 class="m-1">Không có dữ liệu</h3>
                      </div>
                    </div>
                  </template>
                </DataTable>
              </div>
            </div>
            <div v-show="options.view === 4" class="f-full">Chấm công</div>
            <div v-show="options.view === 5" class="f-full">Phiếu lương</div>
            <div v-show="options.view === 6" class="f-full">
              <div class="row p-2">
                <div class="col-12 md:col-12 p-0">
                  <Accordion Accordion class="w-full" :activeIndex="0">
                    <AccordionTab>
                      <template #header>
                        <span>1. Thông tin chung</span>
                      </template>
                      <div class="col-12 md:col-12">
                        <label
                          >Số sổ bảo hiểm:
                          <span class="description-2">{{
                            insurance.insurance_id
                          }}</span></label
                        >
                      </div>
                      <div class="col-12 md:col-12">
                        <label
                          >Trạng thái:
                          <span class="description-2">{{
                            insurance.status_name
                          }}</span></label
                        >
                      </div>
                      <div class="col-12 md:col-12">
                        <label
                          >Pháp nhân đóng:
                          <span class="description-2">{{
                            insurance.organization_name
                          }}</span></label
                        >
                      </div>
                      <div class="col-12 md:col-12">
                        <label
                          >Số thẻ BHYT:
                          <span class="description-2">{{
                            insurance.insurance_code
                          }}</span></label
                        >
                      </div>
                      <div class="col-12 md:col-12">
                        <label
                          >Mã tỉnh cấp:
                          <span class="description-2">{{
                            insurance.insurance_province_name
                          }}</span></label
                        >
                      </div>
                      <div class="col-12 md:col-12">
                        <label
                          >Nơi đăng ký:
                          <span class="description-2">{{
                            insurance.hospital_name
                          }}</span></label
                        >
                      </div>
                    </AccordionTab>
                  </Accordion>
                  <Accordion
                    Accordion
                    class="w-full padding-0"
                    :activeIndex="0"
                  >
                    <AccordionTab>
                      <template #header>
                        <span>2. Lịch sử đóng bảo hiểm</span>
                      </template>
                      <div class="col-12 md:col-12 p-0">
                        <DataTable
                          :value="insurance_pays"
                          :scrollable="true"
                          :lazy="true"
                          :rowHover="true"
                          :showGridlines="true"
                          scrollDirection="both"
                          style="display: grid"
                          class="empty-full"
                        >
                          <Column
                            field="start_date"
                            header="Từ tháng"
                            headerStyle="text-align:center;width:120px;height:50px"
                            bodyStyle="text-align:center;width:120px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span>{{ slotProps.data.start_date }}</span>
                            </template>
                          </Column>
                          <Column
                            field="payment_form"
                            header="Hình thức"
                            headerStyle="text-align:center;width:170px;height:50px"
                            bodyStyle="text-align:center;width:170px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <div class="form-group m-0">
                                <span>{{ slotProps.data.payment_form }}</span>
                              </div>
                            </template>
                          </Column>
                          <Column
                            field="reason"
                            header="Lý do"
                            headerStyle="text-align:center;width:120px;height:50px"
                            bodyStyle="text-align:center;width:120px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span>{{ slotProps.data.reason }}</span>
                            </template>
                          </Column>
                          <Column
                            field="organization_payment"
                            header="Pháp nhân đóng"
                            headerStyle="text-align:center;width:250px;height:50px"
                            bodyStyle="text-align:center;width:250px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span>{{
                                slotProps.data.organization_payment
                              }}</span>
                            </template>
                          </Column>
                          <Column
                            field="total_payment"
                            header="Mức đóng"
                            headerStyle="text-align:center;width:150px;height:50px"
                            bodyStyle="text-align:center;width:150px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span>{{ slotProps.data.total_payment }}</span>
                            </template>
                          </Column>
                          <Column
                            field="company_payment"
                            header="Công ty đóng"
                            headerStyle="text-align:center;width:150px;height:50px"
                            bodyStyle="text-align:center;width:150px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span>{{ slotProps.data.company_payment }}</span>
                            </template>
                          </Column>
                          <Column
                            field="member_payment"
                            header="NLĐ đóng"
                            headerStyle="text-align:center;width:150px;height:50px"
                            bodyStyle="text-align:center;width:150px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              {{ slotProps.data.member_payment }}
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
                    </AccordionTab>
                  </Accordion>
                  <Accordion
                    Accordion
                    class="w-full padding-0"
                    :activeIndex="0"
                  >
                    <AccordionTab>
                      <template #header>
                        <span>3. Lịch sử giải quyết chế độ</span>
                      </template>
                      <div class="col-12 md:col-12 p-0">
                        <DataTable
                          :value="insurance_resolves"
                          :scrollable="true"
                          :lazy="true"
                          :rowHover="true"
                          :showGridlines="true"
                          style="display: grid"
                          class="empty-full"
                        >
                          <Column
                            field="type_mode"
                            header="Loại chế độ"
                            headerStyle="text-align:center;width:120px;height:50px"
                            bodyStyle="text-align:center;width:120px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span>{{ slotProps.data.type_mode }}</span>
                            </template>
                          </Column>
                          <Column
                            field="received_file_date"
                            header="Ngày nhận hồ sơ"
                            headerStyle="text-align:center;width:170px;height:50px"
                            bodyStyle="text-align:center;width:170px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <div class="form-group m-0">
                                <span>{{
                                  slotProps.data.received_file_date
                                }}</span>
                              </div>
                            </template>
                          </Column>
                          <Column
                            field="completed_date"
                            header="Ngày HT thủ tục"
                            headerStyle="text-align:center;width:120px;height:50px"
                            bodyStyle="text-align:center;width:120px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span>{{ slotProps.data.completed_date }}</span>
                            </template>
                          </Column>
                          <Column
                            field="received_money_date"
                            header="Ngày NT BH trả"
                            headerStyle="text-align:center;width:250px;height:50px"
                            bodyStyle="text-align:center;width:250px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span>{{
                                slotProps.data.received_money_date
                              }}</span>
                            </template>
                          </Column>
                          <Column
                            field="money"
                            header="Số tiền"
                            headerStyle="text-align:center;width:150px;height:50px"
                            bodyStyle="text-align:center;width:150px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span>{{ slotProps.data.money }}</span>
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
                    </AccordionTab>
                  </Accordion>
                </div>
              </div>
            </div>
            <div v-show="options.view === 7" class="f-full">Phép năm</div>
            <div v-show="options.view === 8" class="f-full">
              <div class="d-lang-table-1 p-2">
                <DataTable
                  @page="onPage($event)"
                  @rowSelect="selectRow"
                  :value="trannings"
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
                  dataKey="training_emps_id"
                  scrollHeight="flex"
                  filterDisplay="menu"
                  filterMode="lenient"
                  paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
                  responsiveLayout="scroll"
                >
                  <Column
                    field="training_emps_code"
                    header="Mã số"
                    headerStyle="text-align:center;max-width:80px;height:50px"
                    bodyStyle="text-align:center;max-width:80px;"
                    class="align-items-center justify-content-center text-center"
                  />
                  <Column
                    field="training_emps_name"
                    header="Tên khóa đào tạo"
                    headerStyle="text-align:center;max-width:250px;height:50px"
                    bodyStyle="text-align:center;max-width:250px;"
                    class="align-items-center justify-content-center text-center"
                  />
                  <Column
                    field="form_training_name"
                    header="Hình thức"
                    headerStyle="text-align:center;max-width:100px;height:50px"
                    bodyStyle="text-align:center;max-width:100px;"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #body="slotProps">
                      <div>
                        {{
                          slotProps.data.form_training == 1
                            ? "Bắt buộc"
                            : slotProps.data.form_training == 2
                            ? "Đăng ký"
                            : "Cả hai"
                        }}
                      </div>
                    </template>
                  </Column>
                  <Column
                    field="start_date"
                    header="Từ ngày"
                    headerStyle="text-align:center;max-width:100px;height:50px"
                    bodyStyle="text-align:center;max-width:100px;"
                    class="align-items-center justify-content-center text-center"
                  />
                  <Column
                    field="end_date"
                    header="Đến ngày"
                    headerStyle="text-align:center;max-width:100px;height:50px"
                    bodyStyle="text-align:center;max-width:100px;"
                    class="align-items-center justify-content-center text-center"
                  />
                  <Column
                    field="li_user_verify"
                    header="Giảng viên"
                    headerStyle="text-align:center;max-width:100px;height:50px"
                    bodyStyle="text-align:center;max-width:100px;"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #body="slotProps">
                      <AvatarGroup
                        v-if="
                          slotProps.data.li_user_verify &&
                          slotProps.data.li_user_verify.length > 0
                        "
                      >
                        <Avatar
                          v-for="(
                            item, index
                          ) in slotProps.data.li_user_verify.slice(0, 3)"
                          v-bind:label="
                            item.avatar ? '' : item.last_name.substring(0, 1)
                          "
                          v-bind:image="
                            item.avatar
                              ? basedomainURL + item.avatar
                              : basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                          v-tooltip.top="item.full_name"
                          :key="item.user_id"
                          style="color: white"
                          @click="onTaskUserFilter(item)"
                          @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                          size="large"
                          shape="circle"
                          class="cursor-pointer"
                          :style="{ backgroundColor: bgColor[index % 7] }"
                        />
                        <Avatar
                          v-if="
                            slotProps.data.li_user_verify &&
                            slotProps.data.li_user_verify.length > 3
                          "
                          v-bind:label="
                            '+' +
                            (
                              slotProps.data.li_user_verify.length - 3
                            ).toString()
                          "
                          shape="circle"
                          size="large"
                          style="background-color: #2196f3; color: #ffffff"
                          class="cursor-pointer"
                        />
                      </AvatarGroup>
                    </template>
                  </Column>
                  <Column
                    field="count_emps"
                    header="Học viên"
                    headerStyle="text-align:center;max-width:100px;height:50px"
                    bodyStyle="text-align:center;max-width:100px"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #body="data">
                      <div>
                        {{ data.data.count_emps ? data.data.count_emps : "0" }}
                      </div>
                    </template>
                  </Column>
                  <Column
                    field="status"
                    header="Trạng thái"
                    headerStyle="text-align:center;max-width:11rem;height:50px"
                    bodyStyle="text-align:center;max-width:11rem"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #body="slotProps">
                      <Button
                        :label="
                          slotProps.data.status == 1
                            ? 'Lên kế hoạch'
                            : slotProps.data.status == 2
                            ? 'Đang thực hiện'
                            : slotProps.data.status == 3
                            ? 'Đã hoàn thành'
                            : slotProps.data.status == 4
                            ? 'Tạm dừng'
                            : 'Đã hủy'
                        "
                        :class="
                          slotProps.data.status == 1
                            ? 'bg-blue-500'
                            : slotProps.data.status == 2
                            ? 'bg-yellow-500'
                            : slotProps.data.status == 3
                            ? 'bg-green-500'
                            : slotProps.data.status == 4
                            ? 'bg-orange-500'
                            : 'bg-pink-500'
                        "
                        class="px-2 w-10rem"
                      />
                    </template>
                  </Column>
                  <template #empty>
                    <div
                      class="align-items-center justify-content-center p-4 text-center m-auto"
                      style="
                        display: flex;
                        width: 100%;
                        height: calc(100vh - 303px);
                        background-color: #fff;
                      "
                    >
                      <div v-if="!options.loading && options.total == 0">
                        <img
                          src="../../../../assets/background/nodata.png"
                          height="144"
                        />
                        <h3 class="m-1">Không có dữ liệu</h3>
                      </div>
                    </div>
                  </template>
                </DataTable>
              </div>
            </div>
            <div v-show="options.view === 9" class="f-full">Quyết định</div>
            <div v-show="options.view === 10" class="f-full">
              <div class="d-lang-table-1 p-2">
                <DataTable
                  @page="onPage($event)"
                  @rowSelect="selectRow"
                  :value="files"
                  :paginator="true"
                  :rows="options.pageSize"
                  :rowsPerPageOptions="[25, 50, 100, 200]"
                  :totalRecords="options.total"
                  :scrollable="true"
                  :lazy="true"
                  :rowHover="true"
                  :showGridlines="false"
                  :globalFilterFields="['file_name']"
                  v-model:selection="selectedNodes"
                  selectionMode="single"
                  dataKey="file_id"
                  scrollHeight="flex"
                  filterDisplay="menu"
                  filterMode="lenient"
                  paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
                  responsiveLayout="scroll"
                  rowGroupMode="subheader"
                  groupRowsBy="is_type_name"
                >
                  <template #groupheader="slotProps">
                    <i class="pi pi-list mr-2"></i
                    >{{ slotProps.data.is_type_name }}
                  </template>
                  <Column
                    field="file_name"
                    header="Tên file số hóa"
                    headerStyle="text-align:center;height:50px"
                    bodyStyle="text-align:left;"
                    class="align-items-center justify-content-left text-left"
                  >
                    <template #body="slotProps">
                      <div class="flex align-items-center">
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
                        <span
                          :style="{
                            wordBreak: 'break-word',
                          }"
                        >
                          {{ slotProps.data.file_name }}</span
                        >
                      </div>
                    </template>
                  </Column>
                  <Column
                    field="file_size"
                    header="Kích cỡ"
                    headerStyle="text-align:center;max-width:150px;height:50px"
                    bodyStyle="text-align:center;max-width:150px;"
                    class="align-items-center justify-content-center text-center"
                  />
                  <Column
                    field="created_date"
                    header="Ngày tạo"
                    headerStyle="text-align:center;max-width:150px;height:50px"
                    bodyStyle="text-align:center;max-width:150px;"
                    class="align-items-center justify-content-center text-center"
                  />
                  <template #empty>
                    <div
                      class="align-items-center justify-content-center p-4 text-center m-auto"
                      style="
                        display: flex;
                        width: 100%;
                        height: calc(100vh - 291px);
                        background-color: #fff;
                      "
                    >
                      <div v-if="!options.loading && options.total == 0">
                        <img
                          src="../../../../assets/background/nodata.png"
                          height="144"
                        />
                        <h3 class="m-1">Không có dữ liệu</h3>
                      </div>
                    </div>
                  </template>
                </DataTable>
              </div>
            </div>
            <div v-show="options.view === 11" class="f-full">
              <div class="d-lang-table-1 p-2">
                <DataTable
                  :value="receipts"
                  :scrollable="true"
                  :lazy="true"
                  :rowHover="true"
                  :showGridlines="true"
                  :globalFilterFields="['receipt_name']"
                  disableSelection="true"
                  v-model:selection="selectedNodes"
                  dataKey="receipt_id"
                  scrollHeight="flex"
                  filterDisplay="menu"
                  filterMode="lenient"
                  paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
                  responsiveLayout="scroll"
                >
                  <Column
                    field="is_active"
                    header=""
                    headerStyle="text-align:center;max-width:50px;height:50px"
                    bodyStyle="text-align:center;max-width:50px;"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #body="slotProps">
                      <span v-if="slotProps.data.is_active"
                        ><i class="pi pi-check"></i
                      ></span>
                    </template>
                  </Column>
                  <Column
                    field="receipt_name"
                    header="Danh sách giấy tờ"
                    headerStyle="max-width:auto;"
                  >
                    <template #body="slotProps">
                      <span>{{ slotProps.data.receipt_name }}</span>
                    </template>
                  </Column>
                  <Column
                    field="receipt_date"
                    header="Ngày tiếp nhận"
                    headerStyle="text-align:center;max-width:150px;height:50px"
                    bodyStyle="text-align:center;max-width:150px;"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #body="slotProps">
                      <span>{{ slotProps.data.receipt_date }}</span>
                    </template>
                  </Column>
                  <Column
                    field="receipt_name"
                    header="Ghi chú"
                    headerStyle="text-align:center;max-width:300px;height:50px"
                    bodyStyle="text-align:center;max-width:300px;"
                    class="align-items-center justify-content-center text-center"
                  >
                    <template #body="slotProps">
                      <span>{{ slotProps.data.note }}</span>
                    </template>
                  </Column>
                </DataTable>
              </div>
            </div>
            <div v-show="options.view === 12" class="f-full">
              <div class="row p-2">
                <div class="col-12 md:col-12 p-0">
                  <Accordion class="w-full mb-2" :activeIndex="0">
                    <AccordionTab>
                      <template #header>
                        <span>1. Thông tin chung</span>
                      </template>
                      <div class="row">
                        <div class="col-6 md:col-6">
                          <div class="form-group">
                            <label
                              >Chiều cao:
                              <span class="description-2">{{
                                health.height
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-6 md:col-6">
                          <div class="form-group">
                            <label
                              >Cân nặng:
                              <span class="description-2">{{
                                health.weight
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-6 md:col-6">
                          <div class="form-group">
                            <label
                              >Nhóm máu:
                              <span class="description-2">{{
                                health.blood_group
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-6 md:col-6">
                          <div class="form-group">
                            <label
                              >Huyết áp:
                              <span class="description-2">{{
                                health.blood_pressure
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-6 md:col-6">
                          <div class="form-group">
                            <label
                              >Nhịp tim:
                              <span class="description-2">{{
                                health.heartbeat
                              }}</span></label
                            >
                          </div>
                        </div>
                        <div class="col-12 md:col-12">
                          <div class="form-group">
                            <label
                              >Ghi chú:
                              <span class="description-2">{{
                                health.note
                              }}</span></label
                            >
                          </div>
                        </div>
                      </div>
                    </AccordionTab>
                  </Accordion>
                  <Accordion class="w-full padding-0 mb-2" :activeIndex="0">
                    <AccordionTab>
                      <template #header>
                        <span>2. Thông tin tiêm Vắc xin</span>
                      </template>
                      <div class="col-12 md:col-12 p-0">
                        <DataTable
                          :value="vaccines"
                          :scrollable="true"
                          :lazy="true"
                          :rowHover="true"
                          :showGridlines="true"
                          scrollDirection="both"
                          style="display: grid"
                          class="empty-full"
                        >
                          <Column
                            field="injection_id"
                            header="Mũi"
                            headerStyle="text-align:center;width:150px;height:50px"
                            bodyStyle="text-align:center;width:150px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span>{{ slotProps.data.injection_name }}</span>
                            </template>
                          </Column>
                          <Column
                            field="injection_date"
                            header="Ngày tiêm"
                            headerStyle="text-align:center;width:150px;height:50px"
                            bodyStyle="text-align:center;width:150px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span>{{ slotProps.data.injection_date }}</span>
                            </template>
                          </Column>
                          <Column
                            field="type_vaccine"
                            header="Loại vắc xin"
                            headerStyle="text-align:center;width:250px;height:50px"
                            bodyStyle="text-align:center;width:250px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span>{{
                                slotProps.data.type_vaccine_name
                              }}</span>
                            </template>
                          </Column>
                          <Column
                            field="lot_number"
                            header="Số lô"
                            headerStyle="text-align:center;width:150px;height:50px"
                            bodyStyle="text-align:center;width:150px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span>{{ slotProps.data.lot_number }}</span>
                            </template>
                          </Column>
                          <Column
                            field="vaccination_facility"
                            header="Cơ sở tiêm chủng"
                            headerStyle="text-align:center;width:250px;height:50px"
                            bodyStyle="text-align:center;width:250px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              <span>{{
                                slotProps.data.vaccination_facility
                              }}</span>
                            </template>
                          </Column>
                          <Column
                            field="sign_user"
                            header="Người ký"
                            headerStyle="text-align:center;width:200px;height:50px"
                            bodyStyle="text-align:center;width:200px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              {{ slotProps.data.sign_user }}
                            </template>
                          </Column>
                          <Column
                            field="sign_user_position"
                            header="Chức vụ"
                            headerStyle="text-align:center;width:200px;height:50px"
                            bodyStyle="text-align:center;width:200px;"
                            class="align-items-center justify-content-center text-center"
                          >
                            <template #body="slotProps">
                              {{ slotProps.data.sign_user_position }}
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
                    </AccordionTab>
                  </Accordion>
                </div>
              </div>
            </div>
            <div v-show="options.view === 13" class="f-full">
              <div class="row p-2 justify-content-center">
                <div class="col-10 md:col-10 p-0">
                  <printprofile :key="componentKey" />
                </div>
              </div>
            </div>
          </div>
        </div>
        <div
          style="
            width: 400px !important;
            border-left: solid 1px rgba(0, 0, 0, 0.1);
            overflow: auto;
            height: calc(100vh - 165px);
          "
        >
          <div class="row">
            <div class="col-12 md:col-12 p-0">
              <Accordion
                class="w-full border-none padding-0 mb-2"
                :activeIndex="0"
              >
                <AccordionTab>
                  <template #header>
                    <span
                      ><span style="color: #005a9e">Nhân sự cùng </span>
                      <span style="font-size: 18px">Phòng ban</span></span
                    >
                  </template>
                  <div>
                    <DataTable
                      @rowSelect="selectRow"
                      :value="replates[0]"
                      :scrollable="false"
                      v-model:selection="selectedNodes"
                      selectionMode="single"
                      dataKey="profile_id"
                      class="disable-header"
                    >
                      <Column
                        field="Avatar"
                        header="Ảnh"
                        headerStyle="text-align:left;"
                        bodyStyle="text-align:left;"
                        class="align-items-center justify-content-center text-left"
                      >
                        <template #body="slotProps">
                          <div class="flex">
                            <div class="mr-2">
                              <Avatar
                                v-bind:label="
                                  slotProps.data.avatar
                                    ? ''
                                    : (slotProps.data.profile_user_name ?? '')
                                        .substring(0, 1)
                                        .toUpperCase()
                                "
                                v-bind:image="
                                  slotProps.data.avatar
                                    ? basedomainURL + slotProps.data.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                style="
                                  background-color: #2196f3;
                                  color: #ffffff;
                                  width: 5rem;
                                  height: 5rem;
                                  font-size: 1.5rem !important;
                                  border-radius: 5px;
                                "
                                :style="{
                                  background: bgColor[slotProps.index % 7],
                                }"
                                size="xlarge"
                                class="border-radius"
                              />
                            </div>
                            <div>
                              <div class="mb-1">
                                <b>{{ slotProps.data.profile_user_name }}</b>
                              </div>
                              <div class="description">
                                <span>{{
                                  slotProps.data.department_name
                                }}</span>
                              </div>
                              <div class="description">
                                <span>{{
                                  slotProps.data.work_position_name
                                }}</span>
                              </div>
                              <div class="description">
                                <span>{{ slotProps.data.position_name }}</span>
                              </div>
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
                </AccordionTab>
              </Accordion>
              <Accordion
                class="w-full border-none padding-0 mb-2"
                :activeIndex="0"
              >
                <AccordionTab>
                  <template #header>
                    <span>
                      <span style="color: #005a9e">Nhân sự cùng </span>
                      <span style="font-size: 18px">Tên</span></span
                    >
                  </template>
                  <div>
                    <DataTable
                      @rowSelect="selectRow"
                      :value="replates[1]"
                      :scrollable="false"
                      v-model:selection="selectedNodes"
                      selectionMode="single"
                      dataKey="profile_id"
                      class="disable-header"
                    >
                      <Column
                        field="Avatar"
                        header="Ảnh"
                        headerStyle="text-align:left;"
                        bodyStyle="text-align:left;"
                        class="align-items-center justify-content-center text-left"
                      >
                        <template #body="slotProps">
                          <div class="flex">
                            <div class="mr-2">
                              <Avatar
                                v-bind:label="
                                  slotProps.data.avatar
                                    ? ''
                                    : (slotProps.data.profile_user_name ?? '')
                                        .substring(0, 1)
                                        .toUpperCase()
                                "
                                v-bind:image="
                                  slotProps.data.avatar
                                    ? basedomainURL + slotProps.data.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                style="
                                  background-color: #2196f3;
                                  color: #ffffff;
                                  width: 5rem;
                                  height: 5rem;
                                  font-size: 1.5rem !important;
                                  border-radius: 5px;
                                "
                                :style="{
                                  background: bgColor[slotProps.index % 7],
                                }"
                                size="xlarge"
                                class="border-radius"
                              />
                            </div>
                            <div>
                              <div class="mb-1">
                                <b>{{ slotProps.data.profile_user_name }}</b>
                              </div>
                              <div class="description">
                                <span>{{
                                  slotProps.data.department_name
                                }}</span>
                              </div>
                              <div class="description">
                                <span>{{
                                  slotProps.data.work_position_name
                                }}</span>
                              </div>
                              <div class="description">
                                <span>{{ slotProps.data.position_name }}</span>
                              </div>
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
                </AccordionTab>
              </Accordion>
              <Accordion
                class="w-full border-none padding-0 mb-2"
                :activeIndex="0"
              >
                <AccordionTab>
                  <template #header>
                    <span
                      ><span style="color: #005a9e">Nhân sự cùng </span>
                      <span style="font-size: 18px">Họ</span></span
                    >
                  </template>
                  <div>
                    <DataTable
                      @rowSelect="selectRow"
                      :value="replates[2]"
                      :scrollable="false"
                      v-model:selection="selectedNodes"
                      selectionMode="single"
                      dataKey="profile_id"
                      class="disable-header"
                    >
                      <Column
                        field="Avatar"
                        header="Ảnh"
                        headerStyle="text-align:left;"
                        bodyStyle="text-align:left;"
                        class="align-items-center justify-content-center text-left"
                      >
                        <template #body="slotProps">
                          <div class="flex">
                            <div class="mr-2">
                              <Avatar
                                v-bind:label="
                                  slotProps.data.avatar
                                    ? ''
                                    : (slotProps.data.profile_user_name ?? '')
                                        .substring(0, 1)
                                        .toUpperCase()
                                "
                                v-bind:image="
                                  slotProps.data.avatar
                                    ? basedomainURL + slotProps.data.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                style="
                                  background-color: #2196f3;
                                  color: #ffffff;
                                  width: 5rem;
                                  height: 5rem;
                                  font-size: 1.5rem !important;
                                  border-radius: 5px;
                                "
                                :style="{
                                  background: bgColor[slotProps.index % 7],
                                }"
                                size="xlarge"
                                class="border-radius"
                              />
                            </div>
                            <div>
                              <div class="mb-1">
                                <b>{{ slotProps.data.profile_user_name }}</b>
                              </div>
                              <div class="description">
                                <span>{{
                                  slotProps.data.department_name
                                }}</span>
                              </div>
                              <div class="description">
                                <span>{{
                                  slotProps.data.work_position_name
                                }}</span>
                              </div>
                              <div class="description">
                                <span>{{ slotProps.data.position_name }}</span>
                              </div>
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
                </AccordionTab>
              </Accordion>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- Dialog contract -->
  <dialogcontract
    :key="componentKey"
    :headerDialog="headerDialogContract"
    :displayDialog="displayDialogContract"
    :closeDialog="closeDialogContract"
    :isView="isView"
    :model="contract"
    :dictionarys="dictionarys"
  />
  <dialoginfo
    :key="componentKey"
    :headerDialog="headerDialog"
    :displayDialog="displayDialog"
    :closeDialog="closeDialog"
    :profile_id="options.profile_id"
    :isType="isType"
    :initData="initView1"
  />
  <dialogtraining
    :key="componentKey"
    :headerDialog="headerDialogTranning"
    :displayBasic="displayDialogTranning"
    :training_emps="options.training_emps"
    :checkadd="false"
    :closeDialog="closeDialogTranning"
    :view="true"
  />
  <dialogfile
    :key="componentKey"
    :headerDialog="headerDialogFile"
    :displayDialog="displayDialogFile"
    :file="options.file"
    :closeDialog="closeDialogFile"
  />
  <diloginsurance
    :key="componentKey"
    :headerDialog="headerDialogInsurance"
    :displayDialog="displayDialogInsurance"
    :closeDialog="closeDialogInsurance"
    :isAdd="false"
    :isView="false"
    :model="insurance"
    :addRow="addRow"
    :deleteRow="deleteRow"
    :insurance_pays="insurance_pays"
    :insurance_resolves="insurance_resolves"
    :statuss="statuss"
    :hinhthucs="hinhthucs"
    :dictionarys="insurance_dictionarys"
    :initData="initView6"
  />
</template>
<style scoped>
@import url(../../profile/component/stylehrm.css);
.d-lang-table {
  height: calc(100vh - 156px) !important;
  background-color: #fff;
  overflow: hidden;
}
.d-lang-table-1 {
  height: calc(100vh - 156px) !important;
  overflow-x: hidden;
  overflow-y: auto;
}
.icon-star {
  color: #f4b400 !important;
}
.item-menu {
  padding: 0.75rem 1rem;
  cursor: pointer;
  background: #ffffff;
  color: #495057;
  transition: background-color 0.2s, color 0.2s, border-color 0.2s,
    box-shadow 0.2s;
}
.item-menu:hover {
  background: #e9ecef;
  border-color: #ced4da;
  color: #495057;
}
.item-menu-highlight {
  background: #2196f3 !important;
  border-color: #2196f3 !important;
  color: #ffffff !important;
}
.p-button.p-button-secondary.p-button-custom:hover {
  background: #e9ecef !important;
  border-color: #ced4da !important;
  color: #495057 !important;
}
.absolute-hover{
  display: none;
  animation: 0.5s;
  z-index: 999;
}
.relative-hover:hover .absolute-hover{
  display: block;
}
</style>
<style lang="scss" scoped>
::v-deep(.disable-header) {
  table thead {
    display: none;
  }
}
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
    height: 100%;
    display: flex;
    align-items: center;
  }
}
::v-deep(.padding-0) {
  .p-accordion-content {
    padding: 0 !important;
  }
}
::v-deep(.empty-full) {
  .p-datatable-emptymessage td {
    width: 100% !important;
  }
}
::v-deep(.border-none) {
  .p-accordion-header a {
    border: none !important;
  }
  .p-accordion-content {
    border: none !important;
  }
  .p-datatable-table tr th,
  .p-datatable-table tr td {
    border: none !important;
  }
}
::v-deep(.selectbutton-custom) {
  .p-button.p-highlight {
    // color: #ffffff;
    // background: #64748b;
    // border: 1px solid #64748b;
    color: #000;
    background: #d3e3f8;
    border: 1px solid #d3e3f8;
  }
}
::v-deep(.border-radius) {
  img {
    border-radius: 5px;
  }
}
::v-deep(.header-padding-y-0){
  .p-accordion-header-link{
    padding-top: 0;
    padding-bottom: 0;
  }
}
</style>
