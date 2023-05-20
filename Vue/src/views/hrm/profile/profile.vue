<script setup>
import { onMounted, inject, ref, watch, nextTick } from "vue";
import { encr } from "../../../util/function";
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
  options.value.tab = tab.status;
  initData(true);
};
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
  {
    label: "Export dữ liệu ra Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportData("ExportExcel");
    },
  },
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
      editItem(profile.value, "Chỉnh sửa hồ sơ");
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
  forceRerender(0);
  isAdd.value = true;
  model.value = {
    status: 0,
    is_order: options.value.total + 1,
  };
  files.value = [];
  headerDialog.value = str;
  displayDialog.value = true;
  datachilds.value = [];
};
const closeDialog = () => {
  displayDialog.value = false;
  forceRerender(0);
};
const editItem = (item, str) => {
  datachilds.value = [];
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
            proc: "hrm_profile_get_2",
            par: [{ par: "profile_id", va: item.profile_id }],
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
          // model.value["select_birthplace"] = {};
          // model.value["select_birthplace"][
          //   model.value["birthplace_id"] || -1
          // ] = true;
          // model.value["select_birthplace_origin"] = {};
          // model.value["select_birthplace_origin"][
          //   model.value["birthplace_origin_id"] || -1
          // ] = true;
          // model.value["select_place_register_permanent"] = {};
          // model.value["select_place_register_permanent"][
          //   model.value["place_register_permanent"] || -1
          // ] = true;
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
          tbs[4].forEach((x) => {
            if (x["start_date"] != null) {
              x["start_date"] = new Date(x["start_date"]);
            }
            if (x["end_date"] != null) {
              x["end_date"] = new Date(x["end_date"]);
            }
          });
          datachilds.value[4] = tbs[4];
        } else {
          datachilds.value[4] = [];
        }
        if (tbs[5] != null && tbs[5].length > 0) {
          model.value["files"] = tbs[5];
        } else {
          model.value["files"] = [];
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
              initCount();
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
      initData(true);
      initCount();
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
  datas.value = [];
  dataLimits.value = [];
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
            dataLimits.value = data[0].slice(0, options.value.limitItem);
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
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
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
          initData(true);
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
  datas.value = [];
  dataLimits.value = [];
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
            dataLimits.value = data[0].slice(0, options.value.limitItem);
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
  options.value.limitItem = 25;
  isFilter.value = false;
  initCount();
  initTreeOrganization();
  //initData(true);
};
onMounted(() => {
  nextTick(() => {
    //initPlace();
    initDictionary();
    initCount();
    initTreeOrganization();
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
    if (options.value.limitItem + 25 < data.length) {
      options.value.limitItem += 25;
      dataLimits.value = datas.value.slice(0, options.value.limitItem);
    } else {
      options.value.limitItem = data.length;
      dataLimits.value = datas.value.slice(0, options.value.limitItem);
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
                <span>{{ slotProps.data.department_name }}</span>
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
              height: 'calc(100vh - 210px)',
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
                          : item.gender == 1
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
                      <span>{{ item.department_name }}</span>
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
    :model="model"
    :files="files"
    :chooseImage="chooseImage"
    :deleteImage="deleteImage"
    :handleFileAvtUpload="handleFileAvtUpload"
    :selectFile="selectFile"
    :removeFile="removeFile"
    :addRow="addRow"
    :deleteRow="deleteRow"
    :datachilds="datachilds"
    :dictionarys="dictionarys"
    :genders="genders"
    :places="places"
    :marital_status="marital_status"
    :initData="initData"
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
    header="Tải lên file Excel"
    v-model:visible="displayImport"
    :style="{ width: '50vw' }"
    :closable="true"
    :modal="true"
  >
    <div class="grid formgrid mb-3">
      <div class="col-4 md:col-4">
        <div class="card">
          <div
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
