<script setup>
import { onMounted, inject, ref, watch, nextTick, computed } from "vue";
import { encr, change_unsigned } from "../../../util/function";
import { useToast } from "vue-toastification";
import dilogprofile from "../profile/component/dilogprofile.vue";
import dialogreceipt from "../profile/component/dialogreceipt.vue";
import dialoghealth from "../profile/component/dialoghealth.vue";
import dialogrelate from "../profile/component/dialogrelate.vue";
import dialogtag from "../profile/component//dialogtag.vue";
import dialogcontract from "../contract/component/dialogcontract.vue";
import diloginsurance from "../profile/component/diloginsurance.vue";
import dialogstatus from "../profile/component/dialogstatus.vue";
import dialogmatchaccount from "../profile/component/dialogmatchaccount.vue";
import moment from "moment";
import { groupBy } from "lodash";
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
const options = ref({
  loading: true,
  user_id: store.getters.user.user_id,
  search: "",
  pageNo: 1,
  pageSize: 25,
  limitItem: 25,
  total: 0,
  sort: "created_date desc",
  orderBy: "desc",
  tab: -1,
  view: 1,
  view_copy: 1,
  filterProfile_id: null,
  organizations: [],
  departments: [],
  titles: [],
  professional_works: [],
  birthplace_id: null,
  genders: [],
  birthday_start_date: null,
  birthday_end_date: null,
  tags: [],
});
const isFilter = ref(false);
const isFirst = ref(true);
const datas = ref([]);
const dataLimits = ref([]);
const counts = ref([]);
const profile = ref({});
const selectedNodes = ref({});
const dictionarys = ref([]);
const treeOrganization = ref([]);
const datachilds = ref([]);
const groups = ref([
  { view: 1, icon: "pi pi-list", title: "list" },
  { view: 2, icon: "pi pi-align-right", title: "tree" },
]);
const listPlaceDetails1 = ref([]);

//declare dictionary
const tabs = ref([
  { status: -1, title: "Tất cả", icon: "", total: 0 },
  {
    status: 1,
    title: "Đang làm việc",
    icon: "",
    total: 0,
    bg_color: "#AFE362",
    text_color: "#fff",
  },
  {
    status: 2,
    title: "Nghỉ việc",
    icon: "",
    total: 0,
    bg_color: "#E74C3C",
    text_color: "#fff",
  },
  {
    status: 3,
    title: "Nghỉ thai sản",
    icon: "",
    total: 0,
    bg_color: "#9B59B6",
    text_color: "#fff",
  },
  {
    status: 4,
    title: "Nghỉ không lương",
    icon: "",
    total: 0,
    bg_color: "#E67E22",
    text_color: "#fff",
  },
  {
    status: 5,
    title: "Nghỉ đi học",
    icon: "",
    total: 0,
    bg_color: "#F1C40F",
    text_color: "#fff",
  },
  {
    status: 6,
    title: "Nghỉ khác",
    icon: "",
    total: 0,
    bg_color: "#7F8C8D",
    text_color: "#fff",
  },
  {
    status: 0,
    title: "Chưa phân công",
    icon: "",
    total: 0,
    bg_color: "#bbbbbb",
    text_color: "#fff",
  },
]);
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const genders = ref([
  { value: 1, text: "Nam" },
  { value: 2, text: "Nữ" },
  { value: 3, text: "Khác" },
]);
const places = ref();
const marital_status = ref([
  { value: 0, text: "Độc thân" },
  { value: 1, text: "Kết hôn" },
  { value: 2, text: "Ly hôn" },
]);

//filter
const activeTab = (tab) => {
  options.value.pageNo = 1;
  options.value.pageSize = 25;
  options.value.limitItem = 25;
  options.value.total = 0;
  dataLimits.value = [];

  options.value.tab = tab.status;
  if (isFilterAdv.value) {
    initDataFilterAdv(false, sqlSubmit.value);
  }
  else {
    initData(true);
  }
};
const search = () => {
  options.value.pageNo = 1;
  options.value.pageSize = 25;
  options.value.limitItem = 25;
  options.value.total = 0;
  dataLimits.value = [];

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
  options.value.titles = [];
  options.value.professional_works = [];
  options.value.birthplaces = [];
  options.value.genders = [];
  options.value.birthday_start_date = null;
  options.value.birthday_end_date = null;
  options.value.tags = [];
};
const removeFilter = (idx, array, isTree) => {
  if (isTree) {
    array[idx["key"]]["checked"] = false;
  } else {
    array.splice(idx, 1);
  }
};
const filter = (event) => {
  options.value.pageNo = 1;
  options.value.pageSize = 25;
  options.value.limitItem = 25;
  options.value.total = 0;
  dataLimits.value = [];

  opfilter.value.toggle(event);
  isFilter.value = true;
  initCount();
  initData(true);
};
const changeBirthdayDate = () => {};
const changeView = (view) => {
  if (view != null) {
    options.value.view = view;
    options.value.view_copy = view;
  } else {
    options.value.view = options.value.view_copy;
  }
};

//Xuất excel
const menuButs = ref();
const itemButs = ref([
  // {
  //   label: "Export dữ liệu ra Excel",
  //   icon: "pi pi-file-excel",
  //   command: (event) => {
  //     exportData("ExportExcel");
  //   },
  // },
  {
    label: "Import dữ liệu từ Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      importExcel(event);
    },
  },
]);
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const exportData = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Excel/ExportExcel",
      {
        excelname: "DANH SÁCH PHÒNG HỌP",
        proc: "calendar_ca_boardroom_listexport",
        par: [
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "search", va: options.value.search },
        ],
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

        toast.success("Kết xuất Data thành công!");
        if (response.data.path != null) {
          window.open(baseURL + response.data.path);
        }
      } else {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      }
    });
};

//Watch
// watch(selectedNodes, () => {
//   goProfile(selectedNodes.value);
// });

//Function
const componentKey = ref({});
const forceRerender = (type) => {
  if (!componentKey.value[type]) {
    componentKey.value[type] = 0;
  }
  componentKey.value[type] += 1;
};
const addToArray = (temp, array, id, lv, od) => {
  var filter = array.filter((x) => x.parent_id === id);
  filter = filter.sort((a, b) => {
    return b[od] - a[od];
  });
  if (filter.length > 0) {
    var sp = "";
    for (var i = 0; i < lv; i++) {
      sp += "---";
    }
    lv++;
    filter.forEach((item) => {
      item.lv = lv;
      item.close = true;
      if (!item.ids) {
        item.ids = "";
        item.ids += "," + item.organization_id;
      }
      if (!item.newname) item.newname = sp + " " + item.organization_name;
      temp.push(item);
      addToArray(temp, array, item.organization_id, lv);
    });
  }
};
const menuButMores = ref();
const itemButMores = ref([
  {
    label: "Hiệu chỉnh sơ yếu lý lịch",
    icon: "pi pi-file",
    command: (event) => {
      editItem(profile.value, "Cập nhật thông tin hồ sơ");
    },
  },
  // {
  //   label: "Cập nhật thay đổi thông tin",
  //   icon: "pi pi-pencil",
  //   command: (event) => {
  //     //editItem(profile.value, "Chỉnh sửa hợp đồng");
  //   },
  // },
  {
    label: "Cấp tài khoản truy cập",
    icon: "pi pi-key",
    command: (event) => {
      openMatchAccount(profile.value, "Liên kết tài khoản");
    },
  },
  {
    label: "Khóa tài khoản truy cập",
    icon: "pi pi-lock",
    command: (event) => {
      //editItem(profile.value, "Chỉnh sửa hợp đồng");
    },
  },
  {
    label: "Gửi thông báo Notify",
    icon: "pi pi-send",
    command: (event) => {
      //editItem(profile.value, "Chỉnh sửa hợp đồng");
    },
  },
  {
    label: "Gán nhãn",
    icon: "pi pi-tags",
    command: (event) => {
      openEditDialogTag(profile.value, "Gán nhãn");
    },
  },
  {
    label: "Xác nhận là vợ/chồng",
    icon: "pi pi-users",
    command: (event) => {
      openEditDialogRelate(profile.value, "Xác nhận kết hôn với");
    },
  },
  {
    label: "Thiết lập trạng thái",
    icon: "pi pi-sync",
    command: (event) => {
      openAddDialogStatus(profile.value, "Thiết lập trạng thái");
    },
  },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      deleteItem(profile.value);
    },
  },
]);
const toggleMores = (event, item) => {
  profile.value = item;
  menuButMores.value.toggle(event);
  selectedNodes.value = item;
};

const menuButMoresPlus = ref();
const itemButMoresPlus = ref([
  {
    label: "Xác nhận tiếp nhận hồ sơ",
    icon: "pi pi-check-circle",
    command: (event) => {
      openEditDialogReceipt(profile.value, "Xác nhận tiếp nhận hồ sơ");
    },
  },
  {
    label: "Hợp đồng lao động",
    icon: "pi pi-file",
    command: (event) => {
      openAddDialogContract(profile.value, "Thêm mới hợp đồng");
    },
  },
  {
    label: "Bảo hiểm",
    icon: "pi pi-book",
    command: (event) => {
      openAddDialogInsurance(
        profile.value,
        "Cập nhật thay đổi thông tin bảo hiểm"
      );
    },
  },
  {
    label: "Sức khỏe",
    icon: "pi pi-user",
    command: (event) => {
      openEditDialogHealth(profile.value, "Thông tin sức khỏe");
    },
  },
]);
const toggleMoresPlus = (event, item) => {
  profile.value = item;
  menuButMoresPlus.value.toggle(event);
  selectedNodes.value = item;
};
const goFile = (file) => {
  window.open(basedomainURL + file.file_path, "_blank");
};
const goProfile = (profile) => {
  router.push({
    name: "profileinfo",
    params: { id: profile.key_id },
    query: { id: profile.profile_id },
  });
};

//Function update model
const isAdd = ref(false);
const isView = ref(false);
const submitted = ref(false);
const model = ref({});
const files = ref([]);
const headerDialog = ref();
const displayDialog = ref(false);
const openAddDialog = (str) => {
  isAdd.value = true;
  headerDialog.value = str;
  displayDialog.value = true;
  forceRerender(0);
};
const closeDialog = () => {
  displayDialog.value = false;
  forceRerender(0);
};
const editItem = (item, str) => {
  profile.value = item;
  isAdd.value = false;
  headerDialog.value = str;
  displayDialog.value = true;
  forceRerender(0);
};
const deleteItem = (item) => {
  if (item != null || options.value["filterProfile_id"] != null) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá hồ sơ này không!",
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
            ids = [item["profile_id"]];
          } else if (options.value["filterProfile_id"] != null) {
            ids = [options.value["filterProfile_id"]];
          }
          axios
            .delete(baseURL + "/api/hrm_profile/delete_profile", {
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
              initLoad();
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
const addRow = (type) => {
  let obj = {};
  if (type === 1) {
    //relative
    obj = {
      relative_name: null,
      relationship_id: null,
      birthday: null,
      phone: null,
      tax_code: null,
      identification_citizen: null,
      identification_date_issue: null,
      identification_place_issue: null,
      is_dependent: null,
      start_date: null,
      end_date: null,
      info: null,
      note: null,
    };
  } else if (type === 2) {
    // skill
    obj = {
      university_name: null,
      specialized: null,
      start_date: null,
      end_date: null,
      form_traning_id: null,
      certificate_id: null,
      certificate_start_date: null,
      certificate_end_date: null,
      certificate_key_code: null,
      certificate_version: null,
      certificate_release_time: null,
    };
  } else if (type === 3) {
    obj = {
      card_number: null,
      form: null,
      start_date: null,
      end_date: null,
      admission_place: null,
      transfer_place: null,
    };
  } else if (type === 4) {
    obj = {
      company: null,
      address: null,
      role: null,
      wage: null,
      start_date: null,
      end_date: null,
      reference_name: null,
      reference_phone: null,
      description: null,
      reason: null,
    };
  }
  if (datachilds.value[type] == null) {
    datachilds.value[type] = [];
  }
  datachilds.value[type].push(obj);
};
const deleteRow = (type, idx) => {
  datachilds.value[type].splice(idx, 1);
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
  formData.append("ids", JSON.stringify([item["profile_id"]]));
  axios
    .put(baseURL + "/api/hrm_profile/update_star_profile", formData, config)
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

//function receipt
const receipts = ref([]);
const headerDialogReceipt = ref();
const displayDialogReceipt = ref(false);
const openEditDialogReceipt = (item, str) => {
  forceRerender(1);
  headerDialogReceipt.value = str;
  displayDialogReceipt.value = true;
};
const closeDialogReceipt = () => {
  displayDialogReceipt.value = false;
  forceRerender(1);
};

//function helth
const headerDialogHealth = ref();
const displayDialogHealth = ref(false);
const openEditDialogHealth = (item, str) => {
  forceRerender(2);
  headerDialogHealth.value = str;
  displayDialogHealth.value = true;
};
const closeDialogHealth = () => {
  displayDialogHealth.value = false;
  forceRerender(2);
};

//function relate
const headerDialogRelate = ref();
const displayDialogRelate = ref(false);
const openEditDialogRelate = (item, str) => {
  forceRerender(3);
  headerDialogRelate.value = str;
  displayDialogRelate.value = true;
};
const closeDialogRelate = () => {
  displayDialogRelate.value = false;
  forceRerender(3);
};

//function tag
const headerDialogTag = ref();
const displayDialogTag = ref(false);
const openEditDialogTag = (item, str) => {
  forceRerender(4);
  headerDialogTag.value = str;
  displayDialogTag.value = true;
};
const closeDialogTag = () => {
  displayDialogTag.value = false;
  forceRerender(4);
};

//function contract
function CreateGuid() {
  function _p8(s) {
    var p = (Math.random().toString(16) + "000000000").substr(2, 8);
    return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
  }
  return _p8() + _p8(true) + _p8(true) + _p8();
}
const headerDialogContract = ref();
const displayDialogContract = ref(false);
const openAddDialogContract = (item, str) => {
  forceRerender(5);
  isAdd.value = true;
  model.value = {
    profile: item,
    sign_user: null,
    contract_code: "",
    contract_name: "",
    employment: "",
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
  headerDialogContract.value = str;
  displayDialogContract.value = true;
};
const closeDialogContract = () => {
  displayDialogContract.value = false;
  forceRerender(5);
};

//Funtion Insurance
const headerDialogInsurance = ref();
const displayDialogInsurance = ref(false);
const openAddDialogInsurance = (item, str) => {
  profile.value = item;
  forceRerender(6);
  isAdd.value = false;
  isView.value = false;
  headerDialogInsurance.value = str;
  displayDialogInsurance.value = true;
};
const closeDialogInsurance = () => {
  displayDialogInsurance.value = false;
  forceRerender(6);
};

//Function Status
const headerDialogStatus = ref();
const displayDialogStatus = ref(false);
const openAddDialogStatus = (item, str) => {
  profile.value = item;
  forceRerender(7);
  isAdd.value = false;
  isView.value = false;
  headerDialogStatus.value = str;
  displayDialogStatus.value = true;
};
const closeDialogStatus = () => {
  displayDialogStatus.value = false;
  forceRerender(7);
};

//Function Matching account
const headerDialogMatchAccount = ref();
const displayDialogMatchAccount = ref(false);
const openMatchAccount = (item, str) => {
  profile.value = item;
  forceRerender(8);
  headerDialogMatchAccount.value = str;
  displayDialogMatchAccount.value = true;
};
const closeDialogMatchAccount = () => {
  forceRerender(8);
  displayDialogMatchAccount.value = false;
};

//import
const linkformimport = "/Portals/Mau Excel/Mẫu Excel Phép năm.xlsx";
const importFiles = ref([]);
const displayImport = ref(false);
const importExcel = (type) => {
  importFiles.value = [];
  displayImport.value = true;
};
const closeDialogImport = () => {
  displayImport.value = false;
};
const chooseImportFile = (id) => {
  document.getElementById(id).click();
};
const removeImportFile = (event) => {
  importFiles.value = [];
};
const handleImportFile = (event) => {
  importFiles.value = event.target.files;
};
const uploading = ref(false);
const uploadProgress = ref(0);
const execImportExcel = () => {
  if (!importFiles.value || importFiles.value.length === 0) {
    swal.fire({
      title: "Thông báo!",
      text: "Bạn chưa tải file dữ liệu lên!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  uploading.value = true;
  displayImport.value = false;
  let formData = new FormData();
  for (var i = 0; i < importFiles.value.length; i++) {
    let file = importFiles.value[i];
    formData.append("files", file);
  }
  axios
    .post(baseURL + "/api/hrm_profile/import_excel_profile", formData, {
      headers: {
        Authorization: `Bearer ${store.getters.token}`,
        // "Content-Type": "multipart/form-data",
      },
      progress(progressEvent) {
        uploadProgress.value = Math.round(
          (progressEvent.loaded / progressEvent.total) * 100
        );
        debugger;
      },
      onUploadProgress: (progressEvent) => {
        uploadProgress.value = Math.round(
          (progressEvent.loaded / progressEvent.total) * 100
        );
        debugger;
      },
    })
    .then((response) => {
      uploading.value = false;
      uploadProgress.value = 0;
      if (response.data.err === "1") {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
      toast.success("Nhập dữ liệu thành công");
      initLoad();
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

const downloadFile = (file) => {
  let name = "";
  let pathFileDownload = "";
  if (file.files != null && file.files.length > 0) {
    name = file.files[0].file_name || "file_download" + file.files[0].file_type;
    pathFileDownload = file.files[0].file_path;
  } else {
    pathFileDownload = file.file_path;
    name = file.file_name || "file_download." + file.file_type;
  }
  const a = document.createElement("a");
  a.href =
    basedomainURL +
    "/Viewer/DownloadFile?url=" +
    encodeURIComponent(pathFileDownload) +
    "&title=" +
    encodeURIComponent(name);
  a.download = name;
  // a.target = "_blank";
  a.click();
  a.remove();
};

//Init
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
            }
          } else {
            if (type == 1) {
              listPlaceDetails1.value = [];
            }
          }
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
          // if (
          //   dictionarys.value[20] != null &&
          //   dictionarys.value[20].length > 0
          // ) {
          //   treeOrganization.value = JSON.parse(
          //     JSON.stringify(dictionarys.value[20])
          //   );
          //   var temp = [];
          //   addToArray(temp, treeOrganization.value, null, 0, "is_order");
          //   treeOrganization.value = temp;
          // }
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
  var titles = null;
  if (options.value.titles != null && options.value.titles.length > 0) {
    titles = options.value.titles.map((x) => x["title_id"]).join(",");
  }
  var professional_works = null;
  if (
    options.value.professional_works != null &&
    options.value.professional_works.length > 0
  ) {
    professional_works = options.value.professional_works
      .map((x) => x["professional_work_id"])
      .join(",");
  }
  var birthplaces = null;
  if (
    options.value.birthplaces != null &&
    options.value.birthplaces.length > 0
  ) {
    birthplaces = options.value.birthplaces
      .map((x) => x["place_details_id"])
      .join(",");
  }
  var genders = null;
  if (options.value.genders != null && options.value.genders.length > 0) {
    genders = options.value.genders.map((x) => x["gender"]).join(",");
  }
  var tags = null;
  if (options.value.tags != null && options.value.tags.length > 0) {
    tags = options.value.tags.map((x) => x["tags_id"]).join(",");
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
            proc: "hrm_profile_count_filter_2",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
              { par: "organizations", va: organizations },
              { par: "departments", va: departments },
              { par: "work_positions", va: titles },
              { par: "professional_works", va: professional_works },
              { par: "birthplaces", va: birthplaces },
              { par: "genders", va: genders },
              {
                par: "birthday_start_date",
                va: options.value.birthday_start_date,
              },
              { par: "birthday_end_date", va: options.value.birthday_end_date },
              { par: "tags", va: tags },
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
              var idx = counts.value.findIndex(
                (c) => c["status"] == x["status"]
              );
              if (idx !== -1) {
                x["total"] = counts.value[idx]["total"];
              }
            });
          }
        }
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
            proc: "hrm_profile_count",
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
              var idx = counts.value.findIndex(
                (c) => c["status"] == x["status"]
              );
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
  var titles = null;
  if (options.value.titles != null && options.value.titles.length > 0) {
    titles = options.value.titles.map((x) => x["title_id"]).join(",");
  }
  var professional_works = null;
  if (
    options.value.professional_works != null &&
    options.value.professional_works.length > 0
  ) {
    professional_works = options.value.professional_works
      .map((x) => x["professional_work_id"])
      .join(",");
  }
  var birthplaces = null;
  if (
    options.value.birthplaces != null &&
    options.value.birthplaces.length > 0
  ) {
    birthplaces = options.value.birthplaces
      .map((x) => x["place_details_id"])
      .join(",");
  }
  var genders = null;
  if (options.value.genders != null && options.value.genders.length > 0) {
    genders = options.value.genders.map((x) => x["gender"]).join(",");
  }
  var tags = null;
  if (options.value.tags != null && options.value.tags.length > 0) {
    tags = options.value.tags.map((x) => x["tags_id"]).join(",");
  }
  options.value.loading = true;
  datas.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_list_filter_3",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
              { par: "pageNo", va: options.value.pageNo },
              { par: "pageSize", va: options.value.pageSize },
              { par: "tab", va: options.value.tab },
              { par: "organizations", va: organizations },
              { par: "departments", va: departments },
              { par: "work_positions", va: titles },
              { par: "professional_works", va: professional_works },
              { par: "birthplaces", va: birthplaces },
              { par: "genders", va: genders },
              {
                par: "birthday_start_date",
                va: options.value.birthday_start_date,
              },
              { par: "birthday_end_date", va: options.value.birthday_end_date },
              { par: "tags", va: tags },
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
          var arr = [];
          if (data[0] != null && data[0].length > 0) {
            data[0].forEach((item, i) => {
              item["STT"] = i + 1;
              if (item["created_date"] != null) {
                item["created_date"] = moment(
                  new Date(item["created_date"])
                ).format("DD/MM/YYYY");
              }
              if (item["birthday"] != null) {
                item["birthday"] = moment(new Date(item["birthday"])).format(
                  "DD/MM/YYYY"
                );
              }
              if (item["recruitment_date"] != null) {
                item["recruitment_date"] = moment(
                  new Date(item["recruitment_date"])
                ).format("DD/MM/YYYY");
              }
              var idx = tabs.value.findIndex(
                (x) => x["status"] === item["status"]
              );
              if (idx != -1) {
                item["status_name"] = tabs.value[idx]["title"];
                item["bg_color"] = tabs.value[idx]["bg_color"];
                item["text_color"] = tabs.value[idx]["text_color"];
              } else {
                item["status_name"] = "Nghỉ khác";
                item["bg_color"] = "#7F8C8D";
                item["text_color"] = "#fff";
              }
            });
            datas.value = data[0];
            dataLimits.value = dataLimits.value.concat(data[0]);
            var temp = groupBy(data[0], "department_id");
            for (let k in temp) {
              var obj = {
                department_id: k,
                department_name: temp[k][0].department_name,
                organization_id: temp[k][0].organization_id,
                list: temp[k],
              };
              arr.push(obj);
            }
            if (data[1] != null && data[1].length > 0) {
              options.value.total = data[1][0].total;
            }
          } else {
            arr = [];
            options.value.total = 0;
          }
          treeOrganization.value.forEach((o) => {
            o.list = arr.filter((dp) => dp.department_id == o.organization_id);
          });
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
const initTreeOrganization = () => {
  treeOrganization.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_treeOrganization",
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
            treeOrganization.value = JSON.parse(JSON.stringify(tbs[0]));
            // treeOrganization.value.push({
            //   organization_id: -1,
            //   organization_name: "Chưa phân đơn vị",
            //   parent_id: null,
            //   is_order: -1,
            // });
            var temp = [];
            addToArray(temp, treeOrganization.value, null, 0, "is_order");
            treeOrganization.value = temp;
          }
          initLoad();
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
  if (isFilter.value) {
    initDataFilter();
    return;
  }
  options.value.loading = true;
  datas.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_list_2",
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
          var arr = [];
          if (data[0] != null && data[0].length > 0) {
            data[0].forEach((item, i) => {
              const startDate = moment(item.recruitment_date || new Date());
              const endDate = moment(new Date());
              item.duration = moment.duration(endDate.diff(startDate));
              item.diffyear = item.duration.years();
              item.diffmonth = item.duration.months();
              item.diffday = item.duration.days();

              item["STT"] = i + 1;
              if (item["created_date"] != null) {
                item["created_date"] = moment(
                  new Date(item["created_date"])
                ).format("DD/MM/YYYY");
              }
              if (item["birthday"] != null) {
                item["birthday"] = moment(new Date(item["birthday"])).format(
                  "DD/MM/YYYY"
                );
              }
              if (item["recruitment_date"] != null) {
                item["recruitment_date"] = moment(
                  new Date(item["recruitment_date"])
                ).format("DD/MM/YYYY");
              }
              var idx = tabs.value.findIndex(
                (x) => x["status"] === item["status"]
              );
              if (idx != -1) {
                item["status_name"] = tabs.value[idx]["title"];
                item["bg_color"] = tabs.value[idx]["bg_color"];
                item["text_color"] = tabs.value[idx]["text_color"];
              } else {
                item["status_name"] = "Nghỉ khác";
                item["bg_color"] = "#7F8C8D";
                item["text_color"] = "#fff";
              }
            });
            datas.value = data[0];
            dataLimits.value = dataLimits.value.concat(data[0]);
            var temp = groupBy(data[0], "department_id");
            for (let k in temp) {
              var obj = {
                department_id: k,
                department_name: temp[k][0].department_name,
                organization_id: temp[k][0].organization_id,
                list: temp[k],
              };
              arr.push(obj);
            }
            if (data[1] != null && data[1].length > 0) {
              options.value.total = data[1][0].total;
            }
          } else {
            arr = [];
            options.value.total = 0;
          }
          treeOrganization.value.forEach((o) => {
            o.list = arr.filter((dp) => dp.department_id == o.organization_id);
          });
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
  // options.value = {
  //   loading: true,
  //   user_id: store.getters.user.user_id,
  //   search: "",
  //   pageNo: 1,
  //   pageSize: 25,
  //   total: 0,
  //   sort: "created_date desc",
  //   orderBy: "desc",
  //   tab: -1,
  //   view: 1,
  //   view_copy: 1,
  //   filterProfile_id: null,
  // };
  options.value.pageNo = 1;
  options.value.pageSize = 25;
  options.value.limitItem = 25;
  options.value.total = 0;
  dataLimits.value = [];

  isFilter.value = false;
  isFilterAdv.value = false;
  resetFilterAdvanced();
  initCount();
  initTreeOrganization();
  //initData(true);
};

// Filter nâng cao
const opfilterAdvanced = ref();
const toggleFilterAdvanced = (event) => {
  opfilterAdvanced.value.toggle(event);
};
const closeOverlayFilterAdv = () => {
  opfilterAdvanced.value.toggle(event);
};
const showFilterAdv = ref(false);
const isFilterAdv = ref(false);
const activeFilterAdv = () => {
  showFilterAdv.value = true;
};

const drTypes = ref([
    { text: "Lớn hơn", value: ">", types: ",1,2,3," },
    { text: "Lớn hơn hoặc bằng", value: ">=", types: ",1,2,3," },
    { text: "Bằng", value: "=" },
    { text: "Nhỏ hơn", value: "<", types: ",1,2,3," },
    { text: "Nhỏ hơn hoặc bằng", value: "<=", types: ",1,2,3," },
    { text: "Khác", value: "<>", types: ",0,1,2,3,4,",placeholder:"Tìm trong chuỗi khác với 'giá trị' nhập vào"  },
    { text: "Gồm", value: "Contain", types: ",0,",placeholder:"Tìm trong chuỗi có chứa 'giá trị' nhập vào" },
    { text: "Bắt đầu bằng", value: "StartWith", types: ",0,",placeholder:"Tìm trong chuỗi bắt đầu bằng 'giá trị' nhập vào" },
    { text: "Kết thúc bằng", value: "EndWith", types: ",0,",placeholder:"Tìm trong chuỗi kết thúc bằng 'giá trị' nhập vào"  },
    { text: "Trong khoảng", value: "FromTo", types: ",1,2,3,", placeholder: "Giá trị đầu và giá trị cuối cách nhau bởi dấu - , ví dụ 10-15" },
    { text: "Có", value: " =1 ", types: ",4,", hide: true },
    { text: "Không", value: " =0 ", types: ",4,", hide: true },
    { text: "Có giá trị", value: " IS NOT NULL ", hide: true },
    { text: "Không có giá trị", value: " IS NULL ", hide: true }
]);
const drTypeDate = [
    { text: "Mặc định", value: "" },
    { text: "Ngày", value: "DAY(convert(datetime,$date,103))" },
    { text: "Tháng", value: "MONTH(convert(datetime,$date,103))" },
    { text: "Năm", value: "YEAR(convert(datetime,$date,103))" },
    { text: "Giờ", value: "FORMAT(convert(datetime,$date,103),'HH')" },
    { text: "Phút", value: "FORMAT(convert(datetime,$date,103),'mm')" },
    { text: "Ngày tháng", value: "FORMAT(convert(datetime,$date,103),'dd/MM')" },
    { text: "Tháng năm", value: "FORMAT(convert(datetime,$date,103),'MM/yyyy')" },
];
const groupFilterAdvanced = ref([]);
const viewDB = 'View_SearchEngine_Copy';
const cols = ref([]);
const selectedCols = ref();
const expandedKeys = ref({});
const groupBlock = ref([
    {
        stt: 1,
        AND: true,
        datas: []
    }
]);
const AND = ref(true);
const selectedIP = ref("");
const showListComplete = ref(false);
const selectListComplete = () => {
    let txt = ipsearch.value || "";
    let idx = txt.lastIndexOf(" ");
    if (idx != -1) {
        txt = txt.substring(0, idx) + " " + selectedIP.value.title;
    } else {
        txt = selectedIP.value.title;
    }
    ipsearch.value = txt;
    document.getElementById("ipsearch").focus();
    showListComplete.value = true;
}
const onBlur = () => {
    if (!listfocus.value) {
        setTimeout(() => {
            showListComplete.value = false;
        }, 100);
    }
}
const drHelps =
{
    label: "Mẫu tìm kiếm",
    items: [
        { title: "Tên là Cường,Đức" },
        { title: "Tên đệm là Đức,Hồng" },
        { title: "Tuổi 20" },
        { title: "Tuổi = 20" },
        { title: "Tuổi >=20" },
        { title: "Tuổi từ 20" },
        { title: "Tuổi 20-25" },
        { title: "Tuổi từ 20 đến 25" },
        { title: "Tuổi <45" },
        { title: "Tuổi đến 45" },
        { title: "Giới tính nam" },
        { title: "Giới tính nữ" },
        { title: "Giới tính khác" },
        { title: "Sinh nhật hôm nay" },
        { title: "Sinh tháng 12" },
        { title: "Sinh năm 1988" },
        { title: 'Quê quán ở "Hà Nội"' },
        { title: 'Học trường "Bách khoa"' },
    ]
};
drHelps.items.forEach((x) => {
    x.titleen = change_unsigned(x.title);
});
const compComplete = computed(() => {
    let txt = ipsearch.value;
    if (!txt) return [drHelps].concat(colgroups.value);
    txt = txt.substring(txt.lastIndexOf(" ")).trim();
    let drcols = cols.value.filter(x => x.title.toLowerCase().includes(txt) || x.titleen.toLowerCase().includes(txt));
    let drcolshelps = drHelps.items.filter(x => x.title.toLowerCase().includes(txt) || x.titleen.toLowerCase().includes(txt));
    let arr = [];
    if (drcolshelps.length > 0) arr.push({ label: "Mẫu tìm kiếm", items: drcolshelps });
    if (drcols.length > 0) arr.push({ label: "Kết quả tìm kiếm", items: drcols });
    if (arr.length == 0) return [];
    return arr;
});
const listfocus = ref(false);
const goDown = () => {
    document.getElementById("listComp").focus();
    showListComplete.value = true;
    listfocus.value = true;
};

const selectedKey = ref();
let odby = [];
const renderAutoInput = (txt) => {
    if (!txt) txt = "";
    let groups = [];
    let tday = new Date();
    txt = txt.replace(/==/igm, "=");
    txt = txt.replace(/\s+(có|ở)\s+\"?([^\"]+)\"/igm, "=%$2%");
    txt = txt.replace(/\s+có\s+([^\s]+)/igm, "=%$1%");
    txt = txt.replace(/\s+bắt đầu bằng\s+\"?([^\"]+)\"/igm, "=$1%");
    txt = txt.replace(/\s+bắt đầu bằng\s+([^\s]+)/igm, "=$1%");
    txt = txt.replace(/\s+kết thúc bằng\s+\"?([^\"]+)\"/igm, "=%$1");
    txt = txt.replace(/\s+kết thúc bằng\s+([^\s]+)/igm, "=%$1");
    txt = txt.replace(/(lớn hơn)/igm, ">");
    txt = txt.replace(/(lớn hơn hoặc bằng)/igm, ">=");
    txt = txt.replace(/(nhỏ hơn hoặc bằng)/igm, "<=");
    txt = txt.replace(/(nhỏ hơn)/igm, "<=");
    txt = txt.replace(/( khác )/igm, "<>");
    txt = txt.replace(" bằng ", "=");
    txt = txt.replace(/(tên)\s*(nhân sự)?\s*(là)\s*([^\s]+)/igm, (a) => {
        let arr = a.split("là");
        return `tên nhân sự=${arr[1].split(",").map(x => '%' + x).join(",")}`;
    })
    txt = txt.replace(/(tên đệm)\s*(là)\s*([^\s]+)/igm, (a) => {
        let arr = a.split("là");
        return `tên nhân sự=${arr[1].split(",").map(x => '% ' + x + ' %').join(",")}`;
    })
    txt = txt.replace(/(học trường)\s*\"([^\"]+)\"/igm, "Quá trình đào tạo=%$2%");
    txt = txt.replace(/(phòng ban)\s*\"([^\"]+)\"/igm, "Phòng ban=%$2%");
    txt = txt.replace(/(phòng ban)\s+([^\s]+)/igm, 'Phòng ban=%$2%');
    txt = txt.replace(/(tuổi)\s*\s*(\d+)/igm, 'tuổi=$2');
    txt = txt.replace(/(từ|=)\s*(\d+)\s*(đến|-)\s*(\d+)/igm, '# $2 A-N-D $4');
    txt = txt.replace(/(từ)\s*(\d+)/igm, ">=$2");
    txt = txt.replace(/(trên)\s*(\d+)/igm, ">$2");
    txt = txt.replace(/(đến)\s*(\d+)/igm, "<=$2");
    txt = txt.replace(/(dưới)\s*(\d+)/igm, "<$2");
    txt = txt.replace(/sinh năm/igm, "năm sinh");
    txt=txt.replace(/(sinh năm|năm sinh)\s*(\d{4})/igm,"năm sinh =$2");
    txt=txt.replace(/(giới tính)\s*(là)?\s*([^=\s]+)/igm,"giới tính =$3");
    txt = txt.replace(/(sinh nhật\s*(hôm nay)?)/igm, `ngày sinh =${tday.getDate() < 10 ? '0' : ''}${tday.getDate()}/${(tday.getMonth() + 1) < 10 ? '0' : ''}${tday.getMonth() + 1}%`)
    txt = txt.replace(/(sinh\s*(nhật)?\s*tháng)\s*(\d{1,2})\/?(\d{4})?/igm, function (match, p1) {
        let p2 = match.replace(p1, "").trim();
        if (p2.includes("/")) {
            return `ngày sinh =%/${p2}`;
        }
        p2 = (p2.length == 1 ? '0' : '') + p2;
        p2 = p2.replace("00", "0");
        return `ngày sinh =%/${p2}/%`;
    })
    txt = txt.replace(/(sinh\s*ngày)\s*(\d{1,2})\/?(\d{0,2})?\/?(\d{0,4})/igm, function (match, p1) {
        let p2 = match.replace(p1, "").trim();
        let arrp2 = p2.split("/");
        if (arrp2.length == 3) {
            p2 = p2.split("/").map(x => x.trim().length == 1 ? "0" + x : x).join("/");
            if (p2[2].length < 4) {
                p2 = p2 + "%";
            }
            return `ngày sinh =${p2}`;
        }
        return `ngày sinh =${arrp2.map(x => x.trim().length == 1 ? "0" + x : x).join("/")}/%`;
    });
    txt = txt.replace(/(sinh\s*năm)\s*(\d{1,4})/igm, `ngày sinh =%/$2`);
    txt = txt.replaceAll("và", "&").replaceAll("hoặc", "@");
    let strjoin = /\(.+\)\s*@\s*\(.+\)/igm.test(txt) ? 'OR' : 'AND';
    if (!txt.startsWith("(")) {
        txt += `(${txt})`;
    }
    let match = txt.match(/\([^)]+\)/igm);
    if (match) {
        match.forEach(x => {
            x = x.replace(/([^\(&@#]+)(=|>=|<=|<>|<|>|#)/igm, (s, r) => {
                let i = r.indexOf(">");
                let stt = ">";
                if (i == -1) {
                    i = r.indexOf("<");
                    stt = "<";
                }
                if (i == -1) {
                    i = r.length;
                    stt = "";
                }
                let k = r.substring(0, i).trim().toLowerCase();
                let ken = change_unsigned(k).toLowerCase().trim();
                let objk = cols.value.find(x => x.title.toLowerCase() == k || x.titleen.toLowerCase() == k || x.titleen.toLowerCase() == ken);
                if (objk) {
                    k = objk.key;
                    if (odby.findIndex(o => o == '[' + k + ']') == -1) {
                        odby.push('[' + k + ']');
                    }
                    if (selectedCols.value.findIndex(a => a.key == k) == -1) {
                        selectedCols.value.push(objk);
                    }
                }
                s = s.replace(r, ` [${k}]${stt} `);
                return s;
            });
            groups.push(x);
        })
    }
    let rs = groups.join(` ${strjoin} `);
    rs = rs.replace(/(>=|<=|=|<>|#)([^(=|>=|<=|<>|<|>|&|@|#)]+)/igm, " $1 N'$2' ");
    rs = rs.replaceAll(' & ', ' AND ');
    rs = rs.replaceAll(' @ ', ' OR ');
    rs = rs.replaceAll("= N'%", " LIKE N'%");
    rs = rs.replace(/(\[[^\]]+\])[^\]]+%[^\']+'/igm, (s, r) => {
        if (!s.includes("LIKE N")) return s;
        s = s.replaceAll(" '", "");
        let arr = s.split("LIKE N");
        let str = "(";
        arr[1].replaceAll("'", "").split(",").forEach((x, i) => {
            if (i > 0) {
                str += " OR ";
            }
            str += ` ${arr[0]} LIKE N'${x}' `;
        });
        str += ")"
        return str;
    })
    rs = rs.replace(/=( N'[^']+\s*%\s*')/igm, "LIKE $1");
    rs = rs.replace(/%\s*'/igm, "%'");
    rs = rs.replaceAll("#", "BETWEEN");
    rs = rs.replaceAll("A-N-D", "AND");
    rs = rs.replace(/N'\s*(\d+)\s*AND\s*(\d+)\s*'/igm, '$1 AND $2');
    rs = rs.replace(/\s{2}/igm, " ");
    rs = rs.replace(/\(\s+/igm, "(");
    //For json
    // rs = rs.replace(/(học trường)\s*\"([^\"]+)\"/igm, `
    //     profile_id in(select distinct profile_id from View_SearchEngine cross apply OPENJSON([Hồ sơ nhân sự|Quá trình đào tạo|5]) where JSON_VALUE(value,'$.json') like N'%$2%')
    // `);
    return rs;
};
let cacheSQL = '';
const goSearch = async () => {
    if (showListComplete.value == true) showListComplete.value = false;
    let strSelect = ' Select * ';
    //let strSelect = ' Select ';
    let strFrom = ` from ${viewDB} `;
    let strWhere = '';
    strWhere = renderAutoInput(options.value.search);
    if (strWhere != "") {
        strWhere = " Where " + strWhere;
    }
    let strOrderby = ` order by [${cols.value[0].key}] `;
    //let selectColumn = selectedCols.value.map(x => '[' + x.key + ']').join(",");
    //let strOrderby = ` order by ${odby.length > 0 ? odby.join(",") : '[' + cols.value[0].key + ']'} `;
    let sql = `${strSelect} ${strFrom} ${strWhere} ${strOrderby}`;
    //let sql = `${strSelect} ${selectColumn} ${strFrom} ${strWhere} ${strOrderby}`;
    sql = sql.replace(/\s{2}/igm, " ");
    sql = sql.replace(/\(\s+/igm, "(");
    if (cacheSQL != sql) {
        cacheSQL = sql;
        //await initData(false, sql);
        dataLimits.value = [];
        await initDataFilterAdv(false, sql);
    }
};
const dataAdvAll = ref([]);
const initDataFilterAdv = (f, sql) => {
    datas.value = [];
    let strSQL = {
        "query": true,
        "proc": `Select ${f ? ' Top 1 ' : ' distinct '} * from ${viewDB}`,
    };
    if (cols.value.length > 0) {
        strSQL.proc += ` order by [${cols.value[0].key}]`;
        strSQL.proc += ` offset (` + (options.value.pageNo - 1).toString() + `) * ` + options.value.pageSize + ` rows fetch next ` + options.value.pageSize + ` rows only`;
    }
    if (sql) {
        strSQL.proc = sql;
        sqlSubmit.value = sql;
        
        if (options.value.tab != -1) {            
            let hasWhereQuery = strSQL.proc.replaceAll("Where", "where").indexOf("where");
            let hasOrderQuery = strSQL.proc.indexOf("order by");
            let subQuery_start = strSQL.proc.substring(0, hasOrderQuery);
            let subQuery_end = strSQL.proc.substring(hasOrderQuery);
            let strtab = '';
            if (options.value.tab == 6) {
                strtab = ' ([Hồ sơ nhân sự|Trạng thái nhân sự|0|0|0|status] is null or [Hồ sơ nhân sự|Trạng thái nhân sự|0|0|0|status] not in (0,1,2,3,4,5)) ';
                if (hasWhereQuery >= 0) {
                    strSQL.proc = subQuery_start + ' and ' + strtab + subQuery_end;
                }
                else {
                    strSQL.proc = subQuery_start + ' where ' + strtab + subQuery_end;
                }
            }
            else if (arrayStatus.indexOf(options.value.tab) >= 0) {
                strtab = ' ([Hồ sơ nhân sự|Trạng thái nhân sự|0|0|0|status] = ' + options.value.tab.toString() + ') ';                
                if (hasWhereQuery >= 0) {
                    strSQL.proc = subQuery_start + ' and ' + strtab + subQuery_end;
                }
                else {
                    strSQL.proc = subQuery_start + ' where ' + strtab + subQuery_end;
                }
            }
        }

        if (strSQL.proc.indexOf("offset (") < 0) {
          strSQL.proc += ` offset (` + (options.value.pageNo - 1).toString() + `) * ` + options.value.pageSize + ` rows fetch next ` + options.value.pageSize + ` rows only`;
        }
    }
    if (!f) {
      isFilterAdv.value = true;
    }
    swal.fire({
        width: 110,
        didOpen: () => {
        swal.showLoading();
        },
    });
    axios
    .post(
        basedomainURL + "api/hrm_profile/PostProc",
        {
            str: encr(
                JSON.stringify(strSQL),
                SecretKey,
                cryoptojs
            ).toString(),
        },
        config
    )
    .then((response) => {
        if (response.data != null && response.data.data != null && response.data.err == "0") {
            let dts = JSON.parse(response.data.data);
            if (dts[0].length > 0) {
                if (!f) {
                    //datas.value = dts[0];
                    var arr = [];
                    if (dts[0] != null && dts[0].length > 0) {                      
                      let keys = Object.keys(dts[0][0]);
                      dts[0].forEach((item, i) => { 
                        keys.forEach((prop) => {
                          let key = prop.split("|")[5];
                          if (key != null) {
                            item[key] = item[prop];
                            delete item[prop];
                          }
                        });
                        item.diffyear = 0;
                        item.diffmonth = 0;
                        item.seniority = item.seniority.trim();
                        let start_0_year = item.seniority.indexOf('0 năm');
                        let end_0_month = item.seniority.indexOf(' 0 tháng');
                        if (start_0_year >= 0) {
                          item.seniority = item.seniority.substring(start_0_year + 5);
                        }
                        if (end_0_month >= 0) {
                          item.seniority = item.seniority.substring(0, end_0_month);
                        }
                        item["STT"] = i + 1;
                        if (item["created_date"] != null) {
                          if (moment(item["created_date"], moment.ISO_8601, true).isValid()) {
                            item["created_date"] = moment(new Date(item["created_date"])).format("DD/MM/YYYY");
                          }
                        }
                        if (item["birthday"] != null) {
                          if (moment(item["birthday"], moment.ISO_8601, true).isValid()) {
                            item["birthday"] = moment(new Date(item["birthday"])).format("DD/MM/YYYY");
                          }
                        }
                        if (item["recruitment_date"] != null) {
                          if (moment(item["recruitment_date"], moment.ISO_8601, true).isValid()) {
                            item["recruitment_date"] = moment(new Date(item["recruitment_date"])).format("DD/MM/YYYY");
                          }
                        }
                        if (item.gender == "Nam") {
                          item.gender = 1;
                        }
                        else if (item.gender == "Nữ") {
                          item.gender = 2;
                        }
                        else {
                          item.gender = 3;
                        }
                        var idx = tabs.value.findIndex(
                          (x) => x["status"] === item["status"]
                        );
                        if (idx != -1) {
                          item["status_name"] = tabs.value[idx]["title"];
                          item["bg_color"] = tabs.value[idx]["bg_color"];
                          item["text_color"] = tabs.value[idx]["text_color"];
                        } else {
                          item["status_name"] = "Nghỉ khác";
                          item["bg_color"] = "#7F8C8D";
                          item["text_color"] = "#fff";
                        }
                      });
                      datas.value = dts[0];
                      dataLimits.value = dataLimits.value.concat(dts[0]);
                      dataAdvAll.value = [...dataLimits.value];
                      initCountFilterAdv(sql);
                      var temp = groupBy(dts[0], "department_id");
                      for (let k in temp) {
                        var obj = {
                          department_id: k,
                          department_name: temp[k][0].department_name,
                          organization_id: temp[k][0].organization_id,
                          list: temp[k],
                        };
                        arr.push(obj);
                      }
                      if (dts[1] != null && dts[1].length > 0) {
                        options.value.total = dts[1][0].total;
                      }
                    } else {
                      arr = [];
                      options.value.total = 0;
                      dataAdvAll.value = [...dataLimits.value];
                      initCountFilterAdv(sql);
                    }
                    treeOrganization.value.forEach((o) => {
                      o.list = arr.filter((dp) => dp.department_id == o.organization_id);
                    });
                  //closeOverlayFilterAdv();
                }
                else {
                    let keys = Object.keys(dts[0][0]).filter(x => !x.includes("id"));
                    let grs = [];
                    keys.forEach(k => {
                        expandedKeys.value[k.split("|")[0]] = true;
                        let o = {
                            key: k,
                            group: k.split("|")[0],
                            title: k.split("|")[1],
                            label: k.split("|")[1],
                            typdata: k.split("|")[2],
                            AND: true,
                            type: "=",
                            show: k.split("|")[3] == 1,
                            frozen: k.split("|")[4] == 1,
                            field: k.split("|")[5] || '',
                        };
                        o.titleen = change_unsigned(o.title);
                        cols.value.push(o);
                        let obj = grs.find(x => x.label == k.split("|")[0]);
                        if (obj) {
                            obj.col += 1;
                            obj.items.push(o);
                            obj.children.push(o);
                        } else {
                            grs.push({ key: k.split("|")[0], label: k.split("|")[0], col: 1, items: [o], children: [o] });
                        }
                    });
                    groupFilterAdvanced.value = grs;              
                    if (!selectedCols.value) {
                        selectedCols.value = cols.value.filter(x => x.show);
                    }
                }
                swal.close();
            }
            else {
              options.value.total = 0;
              dataAdvAll.value = [...dataLimits.value];
              if (sql) {
                initCountFilterAdv(sql);
              }
              swal.close();
            }
        }
        swal.close();
    })
    .catch((error) => {
        swal.close();
        if (error.status === 401) {
            swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
            });
        }
    });
};
const initCountFilterAdv = (sql) => {
  let sqlDataGet = sql ? sql.substring(0, sql.lastIndexOf('order by [')) : '';
  if (sqlDataGet != "") {
    let proc = `Select (tbn.FieldValue) status, Count(p.profile_id) total 
              from (${sqlDataGet}) p
              cross join (Select * from dbo.udf_PivotParameters('-1,0,1,2,3,4,5,6',',')) as tbn
              where ((tbn.FieldValue = -1)
                    or (tbn.FieldValue in (0,1,2,3,4,5) and tbn.FieldValue = p.[Hồ sơ nhân sự|Trạng thái nhân sự|0|0|0|status])
                    or (tbn.FieldValue = 6 and (p.[Hồ sơ nhân sự|Trạng thái nhân sự|0|0|0|status] is null or p.[Hồ sơ nhân sự|Trạng thái nhân sự|0|0|0|status] not in (0,1,2,3,4,5)))
                  ) 
              group by tbn.FieldValue`;
    let strSQLCount = {
      "query": true,
      "proc": proc,
    };
    axios
      .post(
          basedomainURL + "api/hrm_profile/PostProc",
          {
              str: encr(
                  JSON.stringify(strSQLCount),
                  SecretKey,
                  cryoptojs
              ).toString(),
          },
          config
      )
      .then((response) => {
        var data = response.data.data;
          if (data != null) {
            let tbs = JSON.parse(data);
            if (tbs[0] != null && tbs[0].length > 0) {
              counts.value = tbs[0];
              tabs.value.forEach((x) => {
                var idx = counts.value.findIndex((c) => c["status"] == x["status"]);
                if (idx !== -1) {
                  x["total"] = counts.value[idx]["total"];
                }
                else {
                  x["total"] = 0;
                }
              });
            }
          }
      })
      .catch((error) => {
          swal.close();
          if (error.status === 401) {
              swal.fire({
                  text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                  confirmButtonText: "OK",
              });
          }
      });
  }
};
const arrayStatus = [0,1,2,3,4,5];
// const getDataTabAdvanced = (tab) => {
//   if (tab.status == -1) {
//     dataLimits.value = [...dataAdvAll.value];
//   }
//   else if (tab.status == 6) {
//     dataLimits.value = dataAdvAll.value.filter(x => x.status == null || arrayStatus.indexOf(x.status) < 0);
//   }
//   else {
//     dataLimits.value = dataAdvAll.value.filter(x => x.status == tab.status);
//   }  
// };
const ipsearch = ref();
const sqlSubmit = ref();
const submitFilter = () => {
    ipsearch.value = "";
    let strSelect = ' Select * ';
    //let strSelect = ' Select ';
    let strFrom = ` from ${viewDB} `;
    let strWhere = '';
    let strOrderby = ` order by [${cols.value[0].key}] `;
    let hasSmartSearch = false;
    groupBlock.value.forEach(g => {
        if (strWhere != "") {
            strWhere += ` ${AND.value ? " AND " : " OR "}`;
        }
        strWhere += "(";
        g.datas.forEach((gx, kg) => {
            hasSmartSearch = true;
            if (selectedCols.value.findIndex(a => a.key == gx.key) == -1) {
                selectedCols.value.push(gx);
            }
            if (strWhere != "" && kg > 0) {
                strWhere += ` ${g.AND ? " AND " : " OR "}`;
            }
            strWhere += "(";
            gx.childs.forEach((x, ix) => {
                if (strWhere != "" && ix > 0) {
                    strWhere += ` ${gx.AND ? " AND " : " OR "} (`;
                }
                switch (x.type) {
                    case "FromTo":
                        strWhere += "(";
                        x.value.split(",").forEach((vl, i) => {
                            strWhere += ` ${i != 0 ? "OR" : ""} ${x.typedate ? x.typedate.replace("$date", "[" + x.key + "]") : "[" + x.key + "]"} BETWEEN ${vl.replace("-", " AND ")}`;
                        });
                        strWhere += ")";
                        ipsearch.value += `${ipsearch.value == "" ? "" : (g.AND ? ' và ' : ' hoặc ')}"${x.title}" trong khoảng ${x.value}`
                        break;
                    case "Contain":
                        strWhere += "(";
                        x.value.split(",").forEach((vl, i) => {
                            strWhere += ` ${i != 0 ? "OR" : ""} [${x.key}] like N'%${vl}%'`;
                        });
                        strWhere += ")";
                        ipsearch.value += `${ipsearch.value == "" ? "" : (g.AND ? ' và ' : ' hoặc ')}"${x.title}" có chữ "${x.value || ''}"`
                        break;
                    case "StartWith":
                        strWhere += "(";
                        x.value.split(",").forEach((vl, i) => {
                            strWhere += ` ${i != 0 ? "OR" : ""} [${x.key}] like N'${vl}%'`;
                        });
                        strWhere += ")";
                        ipsearch.value += `${ipsearch.value == "" ? "" : (g.AND ? ' và ' : ' hoặc ')}"${x.title}" bắt đầu bằng chữ "${x.value || ''}"`
                        break;
                    case "EndWith":
                        strWhere += "(";
                        x.value.split(",").forEach((vl, i) => {
                            strWhere += ` ${i != 0 ? "OR" : ""} [${x.key}] like N'%${vl}'`;
                        });
                        strWhere += ")";
                        ipsearch.value += `${ipsearch.value == "" ? "" : (g.AND ? ' và ' : ' hoặc ')}"${x.title}" kết thúc bằng chữ "${x.value || ''}"`
                        break;
                    default:
                        strWhere += "(";
                        (x.value || "").split(",").forEach((vl, i) => {
                          strWhere += ` ${i != 0 ? "OR" : ""} ${x.typedate ? x.typedate.replace("$date", "[" + x.key + "]") : "[" + x.key + "]"} ${x.type} ${vl ? `N'${vl}'` : ""}`;
                        });
                        strWhere += ")";
                        ipsearch.value += `${ipsearch.value == "" ? "" : (g.AND ? ' và ' : ' hoặc ')} "${x.title}" ${x.type.trim() == "IS NOT NULL" ? 'có giá trị' : x.type.trim() == "IS NULL" ? 'không có giá trị' : x.type } "${x.value || ''}"`
                        break;
                }
                if (strWhere != "" && ix > 0) {
                    strWhere += ")";
                }
            });
            strWhere += ")";
        });
        strWhere += ")";
    })
    if (!hasSmartSearch) {
      strWhere = renderAutoInput(options.value.search);
    }
    else {      
      options.value.search = ipsearch.value;
    }
    if (strWhere != "" && strWhere != "()") {
        strWhere = " Where " + strWhere;
    }
    else {
      strWhere = "";
    }
    let sql = `${strSelect} ${strFrom} ${strWhere} ${strOrderby}`;
    //let selectColumn = selectedCols.value.map(x => '[' + x.key + ']').join(",");
    //let sql = `${strSelect} ${selectColumn} ${strFrom} ${strWhere} ${strOrderby}`;
    sql = sql.replace(/\s{2}/igm, " ");
    sql = sql.replace(/\(\s+/igm, "(");    
    dataLimits.value = [];
    closeOverlayFilterAdv();
    if (options.value.search == null || options.value.search.trim() == "") {
      isFilterAdv.value = false;
      options.value.tab = -1;
      options.value.pageNo = 1;
      options.value.pageSize = 25;
      options.value.limitItem = 25;
      options.value.total = 0;
      isFilterAdv.value = false;
      initCount();
      initData(true);
    }
    else {        
      initDataFilterAdv(false, sql);
    }
};
const expandedRows = ref([]);
const blockindex = ref(0);
const onNodeSelectAdv = (node) => {
    if (groupBlock.value[blockindex.value].datas.findIndex(x => x.key == node.key) == -1) {
        let obj = {
            key: node.key,
            title: node.title,
            AND: node.AND,
            type: node.type,
            typdata: node.typdata
        };
        if (node.children) {
            node.children.filter(!x.childs).forEach(x => {
                x.childs = [obj]
            });
            groupBlock.value[blockindex.value].datas = groupBlock.value[blockindex.value].datas.concat(node.children);
        } else {
            node.childs = [obj]
            groupBlock.value[blockindex.value].datas.push(node);
        }
        expandedRows.value.push(node);
    }
};
const onNodeUnselectAdv = (node) => {
    let idx = groupBlock.value[blockindex.value].datas.findIndex(x => x.key == node.key);
    if (idx != -1) {
        //groupBlock.value[blockindex.value].datas.splice(node, idx);
        groupBlock.value[blockindex.value].datas.splice(idx, 1);
    } else if (node.children) {
        groupBlock.value[blockindex.value].datas = groupBlock.value[blockindex.value].datas.filter(x => node.children.findIndex(a => a.key == x.key) == -1);
    }
};
const renderdrTypes = (dt) => {
    return  drTypes.value.filter(x => !x.types || x.types.includes("," + dt.typdata + ","));
};
const delFilter = (idx, rows, type) => {
    if (type == 0) {
      if (selectedKey.value != null) {
        delete selectedKey.value[rows[idx].key];
      }
    }
    rows.splice(idx, 1);
};
const addFilter = (no) => {
    no.childs.push({ ...no });
};
const addBlock = () => {
    let sttMax = 0; 
    if (groupBlock.value.length > 0) {
      let groupMax = groupBlock.value.reduce((prev, current) => (prev.stt > current.stt) ? prev : current);
      if (groupMax != null) {
        sttMax = groupMax.stt;
      }
    }
    groupBlock.value.push({
        stt: sttMax + 1,
        AND: true,
        datas: []
    });
};
const delBlock = (gr) => {
  let idx = groupBlock.value.findIndex(x => x == gr);
  if (idx >= 0) {
    groupBlock.value.splice(idx, 1);
  }
};
const resetFilterAdvanced = () => {
  if (showFilterAdv.value == true) {
    opfilterAdvanced.value.toggle(event);
  }
  showFilterAdv.value = false;
  options.value.search = "";
  selectedKey.value = {};
  groupBlock.value = [
    {
        stt: 1,
        AND: true,
        datas: []
    }
  ];
};

// Tim kiem bang giong noi
const opMic = ref({
    start: false,
    isshow: false
});
var SpeechRecognition = SpeechRecognition || webkitSpeechRecognition;
var SpeechGrammarList = SpeechGrammarList || webkitSpeechGrammarList;
var grammar = '#JSGF V1.0;'
var recognition = new SpeechRecognition();
var speechRecognitionList = new SpeechGrammarList();
speechRecognitionList.addFromString(grammar, 1);
recognition.grammars = speechRecognitionList;
recognition.lang = 'vi-VN';
recognition.interimResults = true;
recognition.continuous = true;
let isok = false;
let resultIndex = 0;
recognition.onresult = async (evt) => {
    let reArr = [];
    for (let i = resultIndex; i < evt.results.length; i++) {
        var result = evt.results[i];
        reArr.push(result);
        if (result.isFinal) CheckForCommand(result)
    }
    const t = reArr
        .map(result => result[0])
        .map(result => result.transcript)
        .join('')
    let str = t.replace(/( xong rồi| song rồi| ok| xoá| tìm| xóa)/igm, "");
    if (ipsearch.value.trim() != str.trim()) {
        ipsearch.value = str.trim();
        options.value.search = ipsearch.value;
    }
    str = t.toLocaleLowerCase().trim();
    if (str.endsWith("xong rồi") || str.endsWith("song rồi")) {
        stopMic(false);
    } else if (str.endsWith("ok") || str.endsWith("tìm")) {
        if (!isok) {
            isok = true;
            await goSearch();
            isok = false;
        }
    } else if (str.endsWith("xoá") || str.endsWith("xóa")) {
        ipsearch.value = "";
        options.value.search = ipsearch.value;
        resultIndex = evt.results.length - 1;
    }
};
recognition.onspeechend = () => {
    recognition.stop();
    opMic.value.start = false;
};
recognition.onend = () => {
    opMic.value.start = false;
}
recognition.onerror = (event) => {
    opMic.value.error = true;
}
const CheckForCommand = (result) => {
    const t = result[0].transcript.toLowerCase();
    if (t.endsWith('ok')) {
        //goSearch();
    }
}
const stopMic = (f) => {
    //ipsearch.value = "";
    //options.value.search = ipsearch.value;
    opMic.value.isshow = f;
    opMic.value.start = false;
    opMic.value.error = false;
    recognition.stop();
}
const goMic = () => {
    isok = false;
    resultIndex = 0;
    ipsearch.value = "";
    options.value.search = ipsearch.value;
    opMic.value.isshow = true;
    opMic.value.start = false;
    opMic.value.error = false;
}
const startMic = () => {
    if (opMic.value.start) {
        toast.success("Bắt đầu tìm kiếm bằng giọng nói");
        recognition.start();
    }
    else {
        toast.info("Kết thúc tìm kiếm bằng giọng nói");
        recognition.stop();
        opMic.value.isshow = false;
    }
}
const checkValidkey = (k) => {
    return true;
}
const validText = (txt) => {
    return txt;
}
// end filter nang cao

const initSave = () => {
  options.value.pageNo = 1;
  options.value.pageSize = 25;
  options.value.limitItem = 25;
  options.value.total = 0;
  dataLimits.value = [];

  initData(true);
};

const initLoad = () => {
  options.value.pageNo = 1;
  options.value.pageSize = 25;
  options.value.limitItem = 25;
  options.value.total = 0;
  dataLimits.value = [];

  initCount();
  initData(true);
};

onMounted(() => {
  nextTick(() => {
    //initPlace();
    initDictionary();
    initCount();
    initTreeOrganization();
    initDataFilterAdv(true);
    //initData(true);

    const el = document.getElementById("buffered-scroll");
    if (el) {
      el.addEventListener("scroll", (event) => {
        const scrollTop = el.scrollTop;
        const scrollHeight = el.scrollHeight;
        const offsetHeight = el.offsetHeight;
        if (scrollTop >= scrollHeight - offsetHeight - 50) {
          loadMoreRow(datas.value);
        }
      });
    }
  });
});
const loadMoreRow = (data) => {
  if (data.length > 0) {
    if (
      !options.value.loading &&
      options.value.limitItem < options.value.total
    ) {
      options.value.limitItem += 25;
      options.value.pageNo += 1;
      //dataLimits.value = datas.value.slice(0, options.value.limitItem);
      if (isFilterAdv.value) {
        initDataFilterAdv(false, sqlSubmit.value);
      }
      else {
        initData(false);
      }
    } else {
      options.value.limitItem = data.length;
      //dataLimits.value = datas.value.slice(0, options.value.limitItem);
      //initData(false);
    }
  }
};
// const test = () => {
//   var str = encr(
//     JSON.stringify({
//       user: {
//         Username: "test",
//         Password: "123456",
//         Email: "maiphien261299@gmail.com",
//         FullName: "test",
//       },
//       isAdd: true,
//     }),
//     SecretKey,
//     cryoptojs
//   ).toString();
//   console.log(str);
// };
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
            style="padding-right: 2rem;max-width: 450px;width:100vw;"
          />
          <i class="pi pi-filter i-filter-advanced"
            :class="isFilterAdv ? 'active-filter-adv' : ''"
            @click="toggleFilterAdvanced($event)"            
            aria:haspopup="true"
            aria-controls="overlay_panel_adv"
            v-tooltip.top="'Tìm kiếm nâng cao'"
          ></i>
          <OverlayPanel
            :showCloseIcon="false"
            ref="opfilterAdvanced"
            appendTo="body"
            class="p-0 m-0"
            id="overlay_panel_adv"
            style="width: 75vw"
          >
            <div class="flex">
              <div>
                  <h3 class="mb-3">Chọn tiêu chí</h3>
                  <Tree
                      :value="groupFilterAdvanced"
                      v-model:selectionKeys="selectedKey"
                      :expandedKeys="expandedKeys"
                      @nodeSelect="onNodeSelectAdv"
                      @nodeUnselect="onNodeUnselectAdv"
                      :showGridlines="true"
                      selectionMode="checkbox"
                      :filter="true"
                      filterPlaceholder="Tìm tiêu chí"
                      filterBy="title,titleen" 
                      class="p-treetable-sm tbltree-filter-adv mr-2"
                      :rowHover="true"
                      responsiveLayout="scroll"
                      :scrollable="true"
                      scrollHeight="flex"
                      :metaKeySelection="false"
                      style="max-height: calc(100vh - 400px);min-width:480px"
                  >
                      <template #default="slotProps">
                          <b v-if="slotProps.node.children">{{ slotProps.node.label }}</b>
                          <span v-else>{{ slotProps.node.label }}</span>
                      </template>
                  </Tree>
              </div>
              <div class="flex-1">
                  <div class="flex mb-0 w-full align-items-center">
                      <i class="pi pi-cog"></i>
                      <h3 class="flex-1 ml-1 mb-3">Cấu hình tìm kiếm</h3>
                      <div class="flex align-items-center">
                          <Checkbox :binary="true" v-model="AND" />
                          <label class="ml-2"> Kết hợp tất cả nhóm tiêu chí </label>
                      </div>
                      <div class="flex-1"></div>
                      <Button class="p-button-sm ml-1" 
                          v-tooltip.top="'Thêm nhóm'"
                          @click="addBlock()" 
                          icon="pi pi-plus"
                      />
                  </div>
                  <Accordion :activeIndex="blockindex" 
                    style="max-height: calc(100vh - 300px);overflow-y: auto;"
                  >
                      <AccordionTab v-for="gr in groupBlock">
                          <template #header="dt">
                              <div class="flex w-full align-items-center">
                                  <b>Nhóm tiêu chí {{ gr.stt }}</b>
                                  <div class="flex align-items-center ml-4">
                                      <Checkbox :binary="true" v-model="gr.AND" />
                                      <label class="ml-2 font-normal"> Kết hợp các tiêu chí </label>
                                  </div>
                                  <div class="flex-1"></div>
                                  <Button class="p-button-sm p-button-text p-button-outlined p-button-danger" 
                                      v-tooltip.top="'Xoá nhóm'" 
                                      @click="delBlock(gr)" 
                                      v-if="groupBlock.length > 1"
                                      icon="pi pi-trash"
                                  />
                              </div>
                          </template>
                          <DataTable 
                              v-model:expandedRows="expandedRows" 
                              scrollable 
                              scrollHeight="calc(100vh - 220px)"
                              :value="gr.datas" 
                              showGridlines 
                              class="p-datatable-sm w-full"
                          >
                              <template #empty>
                                  <div
                                      class="align-items-center justify-content-center p-4 text-center m-auto"
                                      :style="{
                                          display: 'flex',
                                          width: '100%',
                                          height: '100px',
                                          backgroundColor: '#fff',
                                      }"
                                  >
                                  </div>
                              </template>
                              <Column expander
                                  headerStyle="max-width: 4rem" 
                                  bodyStyle="max-width: 4rem" 
                              />
                              <Column header="Tiêu chí"
                                  headerStyle="flex:1;"
                                  bodyStyle="flex:1;"
                              >
                                  <template #body="dt">
                                      <b>{{ dt.data.title }}</b>
                                  </template>
                              </Column>
                              <Column header="Tất cả điều kiện" 
                                  class="justify-content-center" 
                                  headerStyle="max-width:10rem" 
                                  bodyStyle="max-width:10rem"
                              >
                                  <template #body="dt">
                                      <Checkbox v-model="dt.data.AND" :binary="true" />
                                  </template>
                              </Column>
                              <template #expansion="slotProps">
                                  <div class="w-full p-3">
                                      <DataTable class="w-full" :value="slotProps.data.childs">
                                          <Column header="Kiểu giá trị"
                                            headerStyle="max-width:120px"
                                            bodyStyle="max-width:120px"
                                            v-if="slotProps.data.typdata == 2 || slotProps.data.typdata == 3">
                                            <template #body="dt">
                                                <Dropdown filter 
                                                  v-model="dt.data.typedate" 
                                                  :options="drTypeDate"
                                                  optionLabel="text" 
                                                  optionValue="value" 
                                                  class="w-full" 
                                                />
                                            </template>
                                          </Column>
                                          <Column header="Điều kiện"
                                              headerStyle="max-width:250px"
                                              bodyStyle="max-width:250px"
                                          >
                                              <template #body="dt">
                                                  <Dropdown filter v-model="dt.data.type" :options="renderdrTypes(dt.data)"
                                                      optionLabel="text" optionValue="value" class="w-full" />
                                              </template>
                                          </Column>
                                          <Column header="Giá trị"
                                              headerStyle="flex:1;"
                                              bodyStyle="flex:1;"
                                          >
                                            <template #body="dt">
                                                <Textarea rows="1" spellcheck="false" 
                                                    v-if="!drTypes.find(x => x.value == dt.data.type).hide"
                                                    :placeholder="drTypes.find(x => x.value == dt.data.type).placeholder"
                                                    v-model="dt.data.value" 
                                                    autoResize
                                                    class="w-full" 
                                                />
                                            </template>
                                          </Column>
                                          <Column header="" 
                                              class="justify-content-center"
                                              headerStyle="max-width: 60px;"
                                              bodyStyle="max-width: 60px;"
                                          >
                                              <template #body="dt">
                                                  <Button class="p-button-sm p-button-text p-button-outlined p-button-danger"
                                                      v-tooltip.top="'Xoá điều kiện'"
                                                      @click="delFilter(dt.index, slotProps.data.childs, 1)"
                                                      icon="pi pi-trash" 
                                                  />
                                              </template>
                                          </Column>
                                      </DataTable>
                                  </div>
                              </template>
                              <Column header="" 
                                  class="justify-content-center"
                                  headerStyle="max-width: 8rem;"
                                  bodyStyle="max-width: 8rem;"
                              >
                                  <template #body="dt">
                                      <Button class="p-button-sm p-button-text p-button-outlined p-button-danger" 
                                          v-tooltip.top="'Xoá tiêu chí'"
                                          @click="delFilter(dt.index, gr.datas, 0)" 
                                          icon="pi pi-trash"
                                      />
                                      <Button class="p-button-sm p-button-text p-button-outlined ml-1" 
                                          v-tooltip.top="'Thêm điều kiện'"
                                          @click="addFilter(dt.data)" 
                                          icon="pi pi-plus"
                                      />
                                  </template>
                              </Column>
                          </DataTable>
                      </AccordionTab>
                </Accordion>
                </div>
            </div>
            <div class="text-center mt-2">
                <Button class="p-button-sm p-button-danger mr-2 w-7rem" @click="resetFilterAdvanced()" label="Huỷ" />
                <Button class="p-button-sm w-7rem" v-if="groupBlock.length > 0" @click="submitFilter()" label="Thực hiện" />
            </div>
          </OverlayPanel>
        </span>
        <Button
          v-if="options.search"
          @click="goSearch()"
          v-tooltip.top="'Thực hiện tìm kiếm nâng cao'"
          class="ml-2 p-button-outlined p-button-secondary"
          icon="pi pi-send"
        >
        </Button>
        <!-- <Button
          @click="goMic()"
          v-tooltip.top="'Tìm kiếm bằng giọng nói'"
          class="ml-2 p-button-outlined p-button-secondary search-microphone"
          style="padding: 0.65rem 0.75rem 0.6rem;"        
        >
          <font-awesome-icon icon="fa-solid fa-microphone" 
              style="font-size:1rem; display: block; color: #607d8b"
          />
        </Button> -->
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
              :style="{
                minHeight: 'unset',
                maxheight: 'calc(100vh - 300px)',
                overflow: 'auto',
              }"
            >
              <div class="row">
                <div class="col-6 md:col-6">
                  <div class="row">
                    <div class="col-12 md:col-12 p-0">
                      <div class="form-group">
                        <label>Đơn vị</label>
                        <MultiSelect
                          :options="dictionarys[20]"
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
                          :options="dictionarys[21]"
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
                        <label>Chức danh</label>
                        <MultiSelect
                          :options="dictionarys[28]"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.titles"
                          optionLabel="title_name"
                          placeholder="Chọn chức danh"
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
                                      <span>{{ value.title_name }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(index, options.titles);
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
                        <label>Công việc chuyên môn</label>
                        <MultiSelect
                          :options="dictionarys[23]"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.professional_works"
                          optionLabel="professional_work_name"
                          placeholder="Chọn công việc"
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
                                        value.professional_work_name
                                      }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(
                                          index,
                                          options.professional_works
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
                      <div class="form-group">
                        <label>Nơi sinh</label>
                        <!-- <TreeSelect
                          :options="places"
                          v-model="options.birthplaces"
                          placeholder="Chọn nơi sinh"
                          optionLabel="name"
                          optionValue="place_id"
                          style="min-height: 36px"
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
                                      <span>{{ value.label }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        options.birthplaces = {};
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
                        </TreeSelect> -->
                        <!-- <Dropdown
                          @filter="initPlaceFilter($event, 1)"
                          :options="listPlaceDetails1"
                          :filter="true"
                          :editable="false"
                          :showClear="true"
                          v-model="options.birthplaces"
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
                        /> -->
                        <MultiSelect
                          @filter="initPlaceFilter($event, 1)"
                          :options="listPlaceDetails1"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.birthplaces"
                          optionLabel="name"
                          placeholder="Chọn nơi sinh"
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
                                      <span>{{ value.name }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(
                                          index,
                                          options.birthplaces
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
                    <div class="col-12 md:col-12">
                      <div class="form-group">
                        <label>Giới tính</label>
                        <MultiSelect
                          :options="genders"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.genders"
                          optionLabel="text"
                          placeholder="Chọn giới tính"
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
                                      <span>{{ value.text }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(index, options.genders);
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
                    <div class="col-12 md:col-12">
                      <div class="form-group m-0">
                        <label>Ngày sinh</label>
                      </div>
                    </div>
                    <div class="col-6 md:col-6">
                      <div class="form-group">
                        <Calendar
                          :showIcon="true"
                          class="ip36"
                          autocomplete="on"
                          inputId="time24"
                          v-model="options.birthday_start_date"
                          @date-select="changeBirthdayDate()"
                          @input="changeBirthdayDate()"
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
                          v-model="options.birthday_end_date"
                          @date-select="changeBirthdayDate()"
                          @input="changeBirthdayDate()"
                          placeholder="Đến ngày"
                        />
                      </div>
                    </div>
                    <div class="col-12 md:col-12">
                      <div class="form-group">
                        <label>Nhãn</label>
                        <MultiSelect
                          :options="dictionarys[25]"
                          :filter="true"
                          :showClear="true"
                          :editable="false"
                          v-model="options.tags"
                          optionLabel="tags_name"
                          placeholder="Chọn nhãn"
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
                                      <span>{{ value.tags_name }}</span>
                                    </div>
                                    <span
                                      tabindex="0"
                                      class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                      @click="
                                        removeFilter(index, options.tags);
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
        <!-- <Button @click="test()" label="test" icon="pi pi-plus" class="mr-2" /> -->
        <Button
          @click="openAddDialog('Thêm mới hồ sơ')"
          label="Thêm mới"
          icon="pi pi-plus"
          class="mr-2"
        />

        <Button
          icon="pi pi-trash"
          label="Xóa"
          :class="{
            'p-button-danger': options.filterProfile_id != null,
            'btn-hidden p-button-danger': options.filterProfile_id == null,
          }"
          @click="deleteItem()"
          class="mr-2"
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
        <SelectButton
          v-model="options.view"
          :options="groups"
          @change="changeView(options.view)"
          optionValue="view"
          optionLabel="view"
          dataKey="view"
          aria-labelledby="custom"
          class="mr-2"
        >
          <template #option="slotProps">
            <div v-tooptip.top="slotProps.option.title">
              <i :class="slotProps.option.icon"></i>
            </div>
          </template>
        </SelectButton>
        <Button
          @click="refresh()"
          class="p-button-outlined p-button-secondary mr-2"
          icon="pi pi-refresh"
          v-tooltip.top="'Tải lại'"
        />
        <Button
          @click=""
          icon="pi pi-question-circle"
          class="p-button-outlined p-button-secondary"
          v-tooltip.top="'Hướng dẫn sử dụng'"
        />
      </template>
    </Toolbar>
    <div class="tabview">
      <div class="tableview-nav-content">
        <ul class="tableview-nav">
          <template v-if="options.view === 1">
            <li
              v-for="(tab, key) in tabs"
              :key="key"
              @click="activeTab(tab)"
              class="tableview-header"
              :class="{ highlight: options.tab === tab.status }"
            >
              <a>
                <i :class="tab.icon"></i>
                <span>{{ tab.title }} ({{ tab.total }})</span>
              </a>
            </li>
          </template>
          <template v-else-if="options.view === 2">
            <li class="format-center py-0">
              <h3 class="m-0">
                <i class="pi pi-list"></i> Danh sách nhân sự theo cơ cấu tổ chức
              </h3>
            </li>
          </template>
        </ul>
      </div>
    </div>
    <div v-if="options.view === 1" id="buffered-scroll" class="d-lang-table">
      <!-- <DataTable
        @rowSelect="
          (event) => {
            goProfile(event.data);
          }
        "
        :value="datas"
        :virtualScrollerOptions="{ itemSize: 78 }"
        :scrollable="true"
        v-model:selection="selectedNodes"
        selectionMode="single"
        dataKey="profile_id"
        scrollHeight="calc(100vh - 170px)"
        class="disable-header"
      > -->
      <DataTable
        @rowSelect="
          (event) => {
            goProfile(event.data);
          }
        "
        :value="dataLimits"
        :virtualScrollerOptions="{ itemSize: 78 }"
        v-model:selection="selectedNodes"
        selectionMode="single"
        dataKey="profile_id"
        class="disable-header"
      >
        <Column
          field="Avatar"
          header="Ảnh"
          headerStyle="text-align:center;max-width:100px;height:50px"
          bodyStyle="text-align:center;max-width:100px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <div class="relative">
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
                :style="{
                  background: bgColor[slotProps.index % 7],
                  color: '#ffffff',
                  width: '5rem',
                  height: '5rem',
                  fontSize: '1.5rem !important',
                  borderRadius: '5px',
                }"
                size="xlarge"
                class="border-radius"
              />
              <span
                v-if="slotProps.data.isEdit"
                class="is-sign"
                v-tooltip="'Đã hiệu chỉnh hồ sơ'"
              >
                <font-awesome-icon
                  icon="fa-solid fa-circle-check"
                  style="font-size: 16px; display: block; color: #f4b400"
                />
              </span>
            </div>
          </template>
        </Column>
        <Column
          field="profile_user_name"
          header="Họ và tên"
          headerStyle="height:50px;min-width:200px;"
        >
          <template #body="slotProps">
            <div style="min-width: 200px">
              <div class="mb-2">
                <b>{{ slotProps.data.profile_user_name }}</b>
              </div>
              <div class="mb-1">
                <span
                  >{{ slotProps.data.superior_id }}
                  <span
                    v-if="
                      slotProps.data.superior_id && slotProps.data.profile_id
                    "
                    >|</span
                  >
                  {{ slotProps.data.profile_code }}</span
                >
              </div>
              <div class="mb-1" v-if="slotProps.data.recruitment_date">
                {{ slotProps.data.recruitment_date }}
              </div>
            </div>
          </template>
        </Column>
        <Column
          field="profile_user_name"
          header="Họ và tên"
          headerStyle="text-align:center;max-width:300px;height:50px"
          bodyStyle="text-align:center;max-width:300px;"
          class="align-items-center justify-content-left text-left"
        >
          <template #body="slotProps">
            <div style="min-width: 200px">
              <div class="mb-1" v-if="slotProps.data.gender">
                <span>{{
                  slotProps.data.gender == 1
                    ? "Nam"
                    : slotProps.data.gender == 2
                    ? "Nữ"
                    : "Khác"
                }}</span>
              </div>
              <div class="mb-1">
                <span>{{ slotProps.data.birthday }}</span>
              </div>
              <div class="mb-1">
                <span>{{ slotProps.data.birthplace_name }}</span>
              </div>
            </div>
          </template>
        </Column>
        <Column
          field="profile_user_name"
          header="Họ và tên"
          headerStyle="text-align:center;max-width:300px;height:50px"
          bodyStyle="text-align:center;max-width:300px;"
          class="align-items-center justify-content-left text-left"
        >
          <template #body="slotProps">
            <div style="min-width: 200px">
              <div class="mb-1">
                <span
                  >{{ slotProps.data.phone }}
                  <span
                    v-if="
                      slotProps.data.phone != null &&
                      slotProps.data.email != null
                    "
                    >|</span
                  >
                  {{ slotProps.data.email }}</span
                >
              </div>
              <div class="mb-1">
                <span>{{ slotProps.data.identity_papers_code }}</span>
              </div>
              <div class="mb-1">
                <span>{{ slotProps.data.place_residence }}</span>
              </div>
            </div>
          </template>
        </Column>
        <Column
          field="profile_user_name"
          header="Họ và tên"
          headerStyle="text-align:center;max-width:300px;height:50px"
          bodyStyle="text-align:center;max-width:300px;"
          class="align-items-center justify-content-left text-left"
        >
          <template #body="slotProps">
            <div style="min-width: 200px">
              <div class="mb-1">
                <b>{{ slotProps.data.position_name }}</b>
              </div>
              <div class="mb-1">
                <span>{{ slotProps.data.title_name }}</span>
              </div>
              <div class="mb-1">                
                <span v-if="slotProps.data.department_name.includes('<br/>')" v-html="slotProps.data.department_name"></span>
                <span v-else>{{ slotProps.data.department_name }}</span>
              </div>
            </div>
          </template>
        </Column>
        <Column
          field="countRecruitment"
          header="Ngày thâm niên"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;"
          class="align-items-center justify-content-left text-left"
        >
          <template #body="slotProps">
            <div v-tooltip.top="'Thâm niên công tác'">
              <span v-if="slotProps.data.diffyear > 0">
                {{ slotProps.data.diffyear }} năm
              </span>
              <span v-if="slotProps.data.diffmonth > 0">
                {{ slotProps.data.diffmonth }} tháng
              </span>              
              <span v-if="slotProps.data.seniority != null">
                {{ slotProps.data.seniority }}
              </span>
              <!-- <span
                v-if="
                  slotProps.data.diffyear >= 0 && slotProps.data.diffday > 0
                "
                >{{ slotProps.data.diffday }} ngày
              </span> -->
            </div>
          </template>
        </Column>
        <Column
          field="status"
          header="Trạng thái"
          headerStyle="text-align:center;max-width:30px;height:50px"
          bodyStyle="text-align:center;max-width:30px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <div
              :style="{
                borderRadius: '50%',
                border: slotProps.data.bg_color,
                backgroundColor: slotProps.data.bg_color,
                color: slotProps.data.text_color,
                width: '15px',
                height: '15px',
              }"
              v-tooltip.top="slotProps.data.status_name"
            ></div>
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
              class="flex p-0 justify-content-right"
              style="list-style: none; justify-content: right"
            >
              <li v-if="slotProps.data.is_matchaccount">
                <Button
                  @click="
                    openMatchAccount(slotProps.data, 'liên kết tài khoản')
                  "
                  icon="pi pi-user"
                  class="p-button-rounded p-button-text"
                  v-tooltip.top="'Đã được cấp tài khoản truy cập'"
                  style="font-size: 15px; color: #000"
                />
              </li>
              <li>
                <Button
                  :icon="
                    slotProps.data.is_star ? 'pi pi-star-fill' : 'pi pi-star'
                  "
                  :class="{ 'icon-star': slotProps.data.is_star }"
                  class="p-button-rounded p-button-text"
                  @click="
                    setStar(slotProps.data);
                    $event.stopPropagation();
                  "
                  aria-haspopup="true"
                  aria-controls="overlay_MorePlus"
                  v-tooltip.top="
                    slotProps.data.is_star ? 'Hồ sơ cần lưu ý' : ''
                  "
                  style="font-size: 15px; color: #000"
                />
              </li>
              <li>
                <Button
                  icon="pi pi-plus-circle"
                  class="p-button-rounded p-button-text"
                  @click="
                    toggleMoresPlus($event, slotProps.data);
                    $event.stopPropagation();
                  "
                  aria-haspopup="true"
                  aria-controls="overlay_MorePlus"
                  v-tooltip.top="'Nhập bổ sung hồ sơ'"
                />
              </li>
              <li>
                <Button
                  icon="pi pi-ellipsis-h"
                  class="p-button-rounded p-button-text"
                  @click="
                    toggleMores($event, slotProps.data);
                    $event.stopPropagation();
                  "
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
            :style="{
              display: 'flex',
              width: '100%',
              height: 'calc(100vh - 235px)',
              backgroundColor: '#fff',
            }"
          >
            <div v-if="!options.loading && (!isFirst || options.total == 0)">
              <img src="../../../assets/background/nodata.png" height="144" />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </div>
        </template>
      </DataTable>
      <div
        v-if="options.loading"
        class="format-center"
        :style="{ height: '50px' }"
      >
        <i class="pi pi-sync rotate"></i>
        <span class="ml-3 loading-dots"> Đang tải dữ liệu </span>
      </div>
    </div>
    <div v-else-if="options.view === 2" class="d-lang-table">
      <table :style="{ width: '100%', borderSpacing: '0px' }">
        <template
          v-for="(organization, organizationindex) in treeOrganization"
          :key="organizationindex"
        >
          <tbody v-if="!organization.list || organization.list.length === 0">
            <tr>
              <td colspan="7">
                <div
                  class="p-3"
                  :style="{
                    color: '#005a9e',
                    backgroundColor: '#f8f9fa',
                  }"
                >
                  <b>{{ organization.newname }} ({{ organization.total }})</b>
                </div>
              </td>
            </tr>
          </tbody>
          <tbody
            v-for="(department, departmentindex) in organization.list"
            :key="departmentindex"
          >
            <tr>
              <td
                colspan="7"
                @click="department.isOpen = !(department.isOpen || false)"
                :style="{ cursor: 'pointer' }"
              >
                <div
                  class="p-3 flex"
                  :style="{
                    color: '#005a9e',
                    backgroundColor: '#f8f9fa',
                  }"
                >
                  <div class="mr-3 format-center">
                    <i
                      :class="[
                        department.isOpen
                          ? 'pi pi-chevron-down'
                          : 'pi pi-chevron-right',
                      ]"
                    ></i>
                  </div>
                  <div>
                    <b
                      >{{ organization.newname }} ({{
                        department.list.length
                      }})</b
                    >
                  </div>
                </div>
              </td>
            </tr>
            <template
              v-if="department.isOpen"
              v-for="(item, index) in department.list"
              :key="index"
            >
              <tr @click="goProfile(item)" class="tr-list cursor-pointer">
                <td
                  :style="{
                    width: '5rem',
                    textAlign: 'center',
                    borderBottom: 'solid 1px rgba(0,0,0,0.1)',
                    padding: '0.5rem',
                  }"
                >
                  <div class="relative mr-3">
                    <Avatar
                      v-bind:label="
                        item.avatar
                          ? ''
                          : (item.profile_user_name ?? '')
                              .substring(0, 1)
                              .toUpperCase()
                      "
                      v-bind:image="
                        item.avatar
                          ? basedomainURL + item.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      :style="{
                        background: bgColor[index % 7],
                        color: '#ffffff',
                        width: '5rem',
                        height: '5rem',
                        fontSize: '1.5rem !important',
                        borderRadius: '5px',
                      }"
                      size="xlarge"
                      class="border-radius"
                    />
                    <span
                      v-if="item.isEdit"
                      class="is-sign"
                      v-tooltip="'Đã hiệu chỉnh hồ sơ'"
                    >
                      <font-awesome-icon
                        icon="fa-solid fa-circle-check"
                        style="font-size: 16px; display: block; color: #f4b400"
                      />
                    </span>
                  </div>
                </td>
                <td
                  :style="{
                    minWidth: '200px',
                    textAlign: 'left',
                    borderBottom: 'solid 1px rgba(0,0,0,0.1)',
                    padding: '0.5rem',
                  }"
                >
                  <div>
                    <div class="mb-2">
                      <b>{{ item.profile_user_name }}</b>
                    </div>
                    <div class="mb-1">
                      <span
                        >{{ item.superior_id }}
                        <span v-if="item.superior_id && item.profile_id"
                          >|</span
                        >
                        {{ item.profile_code }}</span
                      >
                    </div>
                    <div class="mb-1" v-if="item.recruitment_date">
                      {{ item.recruitment_date }}
                    </div>
                  </div>
                </td>
                <td
                  :style="{
                    minWidth: '200px',
                    textAlign: 'left',
                    borderBottom: 'solid 1px rgba(0,0,0,0.1)',
                    padding: '0.5rem',
                  }"
                >
                  <div>
                    <div class="mb-1" v-if="item.gender">
                      <span>{{
                        item.gender == 1
                          ? "Nam"
                          : item.gender == 2
                          ? "Nữ"
                          : "Khác"
                      }}</span>
                    </div>
                    <div class="mb-1">
                      <span>{{ item.birthday }}</span>
                    </div>
                    <div class="mb-1">
                      <span>{{ item.birthplace_name }}</span>
                    </div>
                  </div>
                </td>
                <td
                  :style="{
                    minWidth: '200px',
                    textAlign: 'left',
                    borderBottom: 'solid 1px rgba(0,0,0,0.1)',
                    padding: '0.5rem',
                  }"
                >
                  <div>
                    <div class="mb-1">
                      <span
                        >{{ item.phone }}
                        <span v-if="item.phone != null && item.email != null"
                          >|</span
                        >
                        {{ item.email }}</span
                      >
                    </div>
                    <div class="mb-1">
                      <span>{{ item.identity_papers_code }}</span>
                    </div>
                    <div class="mb-1">
                      <span>{{ item.place_residence }}</span>
                    </div>
                  </div>
                </td>
                <td
                  :style="{
                    minWidth: '200px',
                    textAlign: 'left',
                    borderBottom: 'solid 1px rgba(0,0,0,0.1)',
                    padding: '0.5rem',
                  }"
                >
                  <div>
                    <div class="mb-1">
                      <b>{{ item.position_name }}</b>
                    </div>
                    <div class="mb-1">
                      <span>{{ item.title_name }}</span>
                    </div>
                    <div class="mb-1">
                      <span v-if="item.department_name.includes('<br/>')" v-html="item.department_name"></span>
                      <span v-else>{{ item.department_name }}</span>
                    </div>
                  </div>
                </td>
                <td
                  :style="{
                    minWidth: '100px',
                    textAlign: 'left',
                    borderBottom: 'solid 1px rgba(0,0,0,0.1)',
                    padding: '0.5rem',
                  }"
                >
                  <span v-if="item.diffyear > 0">
                    {{ item.diffyear }} năm
                  </span>
                  <span v-if="item.diffmonth > 0">
                    {{ item.diffmonth }} tháng
                  </span>
                </td>
                <td
                  :style="{
                    width: '100px',
                    textAlign: 'center',
                    borderBottom: 'solid 1px rgba(0,0,0,0.1)',
                    padding: '0.5rem',
                  }"
                >
                  <ul class="flex p-0" style="list-style: none">
                    <li class="format-center mr-2">
                      <div
                        :style="{
                          borderRadius: '50%',
                          border: item.bg_color,
                          backgroundColor: item.bg_color,
                          color: item.text_color,
                          width: '15px',
                          height: '15px',
                        }"
                        v-tooltip.top="item.status_name"
                      ></div>
                    </li>
                    <li>
                      <Button
                        :icon="item.is_star ? 'pi pi-star-fill' : 'pi pi-star'"
                        :class="{ 'icon-star': item.is_star }"
                        class="p-button-rounded p-button-text"
                        @click="
                          setStar(item);
                          $event.stopPropagation();
                        "
                        aria-haspopup="true"
                        aria-controls="overlay_MorePlus"
                        v-tooltip.top="item.is_star ? 'Hồ sơ cần lưu ý' : ''"
                        style="font-size: 15px; color: #000"
                      />
                    </li>
                    <li>
                      <Button
                        icon="pi pi-plus-circle"
                        class="p-button-rounded p-button-text"
                        @click="
                          toggleMoresPlus($event, item);
                          $event.stopPropagation();
                        "
                        aria-haspopup="true"
                        aria-controls="overlay_MorePlus"
                        v-tooltip.top="'Nhập bổ sung hồ sơ'"
                      />
                    </li>
                    <li style="text-align: center">
                      <Button
                        icon="pi pi-ellipsis-h"
                        class="p-button-rounded p-button-text"
                        @click="
                          toggleMores($event, item);
                          $event.stopPropagation();
                        "
                        aria-haspopup="true"
                        aria-controls="overlay_More"
                        v-tooltip.top="'Tác vụ'"
                      />
                    </li>
                  </ul>
                </td>
              </tr>
            </template>
          </tbody>
        </template>
      </table>
    </div>
  </div>

  <Dialog
    header="Đang tải dữ liệu ..."
    v-model:visible="uploading"
    :style="{ width: '30vw' }"
    :closable="false"
    :modal="false"
  >
    <ProgressBar
      :value="uploadProgress"
      :style="{
        borderBottomLeftRadius: '0 !important',
        borderBottomRightRadius: '0 !important',
      }"
    />
    <ProgressBar
      v-if="uploadProgress < 100"
      mode="indeterminate"
      :style="{
        borderTopLeftRadius: '0 !important',
        borderTopRightRadius: '0 !important',
        height: '.5em',
      }"
    />
  </Dialog>

  <!-- Dialog -->
  <dilogprofile
    :key="componentKey['0']"
    :headerDialog="headerDialog"
    :displayDialog="displayDialog"
    :closeDialog="closeDialog"
    :isAdd="isAdd"
    :profile="profile"
    :dictionarys="dictionarys"
    :initData="initSave"
  />
  <dialogreceipt
    :key="componentKey['1']"
    :headerDialog="headerDialogReceipt"
    :displayDialog="displayDialogReceipt"
    :closeDialog="closeDialogReceipt"
    :profile="profile"
  />
  <dialoghealth
    :key="componentKey['2']"
    :headerDialog="headerDialogHealth"
    :displayDialog="displayDialogHealth"
    :closeDialog="closeDialogHealth"
    :profile="profile"
    :users="dictionarys[19]"
  />
  <dialogrelate
    :key="componentKey['3']"
    :headerDialog="headerDialogRelate"
    :displayDialog="displayDialogRelate"
    :closeDialog="closeDialogRelate"
    :profile="profile"
    :users="dictionarys[24]"
  />
  <dialogtag
    :key="componentKey['4']"
    :headerDialog="headerDialogTag"
    :displayDialog="displayDialogTag"
    :closeDialog="closeDialogTag"
    :profile="profile"
  />
  <dialogcontract
    :key="componentKey['5']"
    :headerDialog="headerDialogContract"
    :displayDialog="displayDialogContract"
    :closeDialog="closeDialogContract"
    :isAdd="isAdd"
    :isView="false"
    :model="model"
    :files="files"
    :selectFile="selectFile"
    :removeFile="removeFile"
    :dictionarys="dictionarys"
    :initData="initData"
  />
  <diloginsurance
    :key="componentKey['6']"
    :headerDialog="headerDialogInsurance"
    :displayDialog="displayDialogInsurance"
    :closeDialog="closeDialogInsurance"
    :isAdd="isAdd"
    :isView="isView"
    :profile="profile"
    :initData="null"
  />
  <dialogstatus
    :key="componentKey['7']"
    :headerDialog="headerDialogStatus"
    :displayDialog="displayDialogStatus"
    :closeDialog="closeDialogStatus"
    :profile="profile"
    :initData="null"
  />
  <dialogmatchaccount
    :key="componentKey['8']"
    :headerDialog="headerDialogMatchAccount"
    :displayDialog="displayDialogMatchAccount"
    :closeDialog="closeDialogMatchAccount"
    :profile="profile"
  />
  <Menu
    id="overlay_More"
    ref="menuButMores"
    :model="itemButMores"
    :popup="true"
  />
  <Menu
    id="overlay_MorePlus"
    ref="menuButMoresPlus"
    :model="itemButMoresPlus"
    :popup="true"
  />

  <Dialog
    header="Import dữ liệu vào hệ thống"
    v-model:visible="displayImport"
    :style="{ width: '50vw' }"
    :closable="true"
    :modal="true"
  >
    <div class="grid formgrid mb-3">
      <div class="col-4 md:col-4">
        <div class="card">
          <div
            @click=""
            class="card-body button-custom zoom"
            :style="{ backgroundColor: '#89c83e', borderColor: '#89c83e' }"
          >
            <div>
              <div class="text-center mb-2">
                <i class="pi pi-arrow-down" :style="{ fontSize: '25px' }"></i>
              </div>
              <div :style="{ fontSize: '15px' }">Tải file mẫu xuống</div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-4 md:col-4">
        <div class="card">
          <div
            @click="chooseImportFile('importFile')"
            class="card-body button-custom zoom"
            :style="{ backgroundColor: '#E9C25C', borderColor: '#E9C25C' }"
          >
            <div>
              <div class="text-center mb-2">
                <i class="pi pi-arrow-up" :style="{ fontSize: '25px' }"></i>
              </div>
              <div :style="{ fontSize: '15px' }">Tải file dữ liệu lên</div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-4 md:col-4">
        <div class="card">
          <div
            @click="execImportExcel()"
            class="card-body button-custom"
            :class="{ 'zoom filter': importFiles.length > 0 }"
            :style="{
              backgroundColor: '#D4673B',
              borderColor: '#D4673B',
              filter: 'opacity(0.3)',
            }"
          >
            <div>
              <div class="text-center mb-2">
                <i class="pi pi-caret-right" :style="{ fontSize: '25px' }"></i>
              </div>
              <div :style="{ fontSize: '15px' }">Thực hiện</div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div>
      <DataView
        :lazy="true"
        :value="importFiles"
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
                      slotProps.data.name.split('.').at(-1) +
                      '.png'
                    "
                    style="object-fit: contain"
                    width="40"
                    height="40"
                  />
                  <span style="line-height: 1.5">
                    {{ slotProps.data.name }}</span
                  >
                </div>
              </template>
              <template #end>
                <Button
                  icon="pi pi-times"
                  class="p-button-rounded p-button-danger"
                  @click="
                    removeImportFile(slotProps.data, slotProps.data.index)
                  "
                />
              </template>
            </Toolbar>
          </div>
        </template>
      </DataView>
      <div class="mt-3">
        <b :style="{ fontSize: '15px' }"
          ><i class="pi pi-comment"></i> Chú ý:</b
        >
        <div :style="{ fontSize: '14px' }">
          <ul>
            <li>
              Bước 1: Trước khi import dữ liệu vào hệ thống bạn phải
              <b>"Tải file mẫu xuống"</b>, sau đó cập nhật thông tin về nhân sự
              vào file mẫu Excel.
            </li>
            <li>
              Bước 2: Sau khi cập nhật thông tin nhân sự xong, bạn chọn
              <b>"Tải file dữ liệu lên"</b>.
            </li>
            <li>
              Bước 3: Sau khi tải file dữ liệu lên thành công, bạn chọn
              <b>"Thực hiện"</b> để hệ thống Import dữ liệu vào.
            </li>
          </ul>
        </div>
      </div>
      <input
        id="importFile"
        type="file"
        accept=".xls,.xlsx"
        @change="handleImportFile"
        style="display: none"
      />
    </div>
    <template #footer>
      <Button
        label="Đóng"
        icon="pi pi-times"
        @click="closeDialogImport()"
        class="p-button-text"
      />
    </template>
  </Dialog>

  <!-- Tim kiem bang giong noi -->
  <Dialog @hide="stopMic(false)" v-model:visible="opMic.isshow" modal header="Tìm kiếm bằng giọng nói"
      :style="{ width: '480px', backgroundColor: '#eee' }">
      <div class="p-2" style="background-color: #eee;">
          <iframe frameborder="none" style="width: 100%;height: 100%;"
              :src="opMic.start ? 'https://embed.lottiefiles.com/animation/91427' : 'https://embed.lottiefiles.com/animation/10627'"></iframe>
          <Card class="mt-2 mb-2" v-if="opMic.start">
              <template #content>
                  <div v-if="opMic.error" style="font-size: 20pt;font-weight: bold;color:red">Không thu được giọng nói của
                      bạn, vui lòng thử lại</div>
                  <div v-else style="font-size: 20pt;font-weight: bold;">{{ ipsearch || "Hãy nói vào mic nhé" }}</div>
              </template>
          </Card>
      </div>
      <div class="text-center mt-2">
          <ToggleButton @click="startMic" v-model="opMic.start" onLabel="Đã xong" offLabel="Bắt đầu nói"
              offIcon="pi pi-play" onIcon="pi pi-stop-circle" />
      </div>
  </Dialog>
  <!-- ... -->
</template>
<style scoped>
@import url(../profile/component/stylehrm.css);
.d-lang-table {
  height: calc(100vh - 170px) !important;
  background-color: #fff;
}
.icon-star {
  color: #f4b400 !important;
}
.is-sign {
  position: absolute;
  display: block;
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background-color: #fff;
  right: -5px !important;
  bottom: 0;
}
.tr-list:hover td {
  background-color: aliceblue;
}
.button-custom {
  height: 100px;
  border: solid 1px;
  border-radius: 5px;
  display: flex;
  justify-content: center;
  align-items: center;
  color: #fff;
}
.zoom {
  cursor: pointer;
  border-radius: 0.25rem;
  /* box-shadow: 0 2px 4px rgb(0 0 0 / 23%); */
  transition: transform 0.3s !important;
}
.zoom:hover {
  transform: scale(0.9) !important;
  /* box-shadow: 10px 10px 15px rgb(0 0 0 / 23%) !important; */
  cursor: pointer !important;
}

.filter {
  filter: opacity(1) !important;
}

.rotate {
  animation: rotateAnimation 2s infinite linear;
}

@keyframes rotateAnimation {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}

.loading-dots {
  position: relative;
  display: inline-block;
}

.loading-dots::after {
  content: "...";
  display: inline-block;
  opacity: 0;
  animation: dotsAnimation 1.5s infinite;
}

@keyframes dotsAnimation {
  0% {
    opacity: 0;
  }
  25% {
    opacity: 0;
  }
  50% {
    opacity: 1;
  }
  75% {
    opacity: 1;
  }
  100% {
    opacity: 0;
  }
}
.i-filter-advanced {
  right: 0rem;
  color: #6c757d;
  margin-top: -1.1rem;
  padding: 0.6rem;
  border-top-right-radius: 0.25rem;
  border-bottom-right-radius: 0.25rem;
}
.i-filter-advanced:hover {  
  cursor: pointer;
  background-color: #2196f3;
  color: #ffffff;
}
.i-filter-advanced.active-filter-adv {
  background-color: #2196f3;
  color: #ffffff;
}
.search-microphone:hover svg {
  color: #ffffff !important;
}
</style>
<style lang="scss" scoped>
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
::v-deep(.disable-header) {
  table thead {
    display: none;
  }
}
::v-deep(.border-radius) {
  img {
    border-radius: 5px;
  }
}
</style>
