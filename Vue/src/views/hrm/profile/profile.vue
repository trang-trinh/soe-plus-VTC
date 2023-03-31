<script setup>
import { onMounted, inject, ref, watch } from "vue";
import { encr } from "../../../util/function";
import { useToast } from "vue-toastification";
import dilogprofile from "../profile/component/dilogprofile.vue";
import dialogreceipt from "../profile/component/dialogreceipt.vue";
import dialoghealth from "../profile/component/dialoghealth.vue";
import dialogrelate from "../profile/component/dialogrelate.vue";
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
  filterProfile_id: null,
  organizations: [],
  departments: [],
  work_positions: [],
  professional_works: [],
  birthplace_id: null,
  genders: [],
  birthday_start_date: null,
  birthday_end_date: null,
});
const isFilter = ref(false);
const isFirst = ref(true);
const datas = ref([]);
const counts = ref([]);
const profile = ref({});
const selectedNodes = ref({});
const dictionarys = ref([]);
const datachilds = ref([]);

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
  options.value.work_positions = [];
  options.value.professional_works = [];
  options.value.birthplaces = {};
  options.value.genders = [];
  options.value.birthday_start_date = null;
  options.value.birthday_end_date = null;
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

//Watch
watch(selectedNodes, () => {
  goProfile(selectedNodes.value);
});

//Function
const componentKey = ref({});
const forceRerender = (type) => {
  if (!componentKey.value[type]) {
    componentKey.value[type] = 0;
  }
  componentKey.value[type] += 1;
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
  {
    label: "Cập nhật thay đổi thông tin",
    icon: "pi pi-pencil",
    command: (event) => {
      //editItem(profile.value, "Chỉnh sửa hợp đồng");
    },
  },
  {
    label: "Cấp tài khoản truy cập",
    icon: "pi pi-key",
    command: (event) => {
      //editItem(profile.value, "Chỉnh sửa hợp đồng");
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
      //editItem(profile.value, "Chỉnh sửa hợp đồng");
    },
  },
  {
    label: "Xác nhận là vợ/chồng",
    icon: "pi pi-check",
    command: (event) => {
      openEditDialogRelate(profile.value, "Xác nhận kết hôn với");
    },
  },
  {
    label: "Thiết lập trạng thái",
    icon: "pi pi-cog",
    command: (event) => {
      //editItem(profile.value, "Chỉnh sửa hợp đồng");
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
      //editItem(profile.value, "Chỉnh sửa hợp đồng");
    },
  },
  {
    label: "Bảo hiểm",
    icon: "pi pi-book",
    command: (event) => {
      //editItem(profile.value, "Chỉnh sửa hợp đồng");
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
};
const closeDialog = () => {
  displayDialog.value = false;
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
          if (model.value["birthplace_id"] == null) {
            model.value["select_birthplace"] = model.value["birthplace_name"];
          }
          if (model.value["birthplace_origin_id"] == null) {
            model.value["select_birthplace_origin"] =
              model.value["birthplace_origin_name"];
          }
          if (model.value["place_register_permanent"] == null) {
            model.value["select_place_register_permanent"] =
              model.value["place_register_permanent_name"];
          }
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
          });
          datachilds.value[1] = tbs[1];
        } else {
          datachilds.value[1] = [];
        }
        if (tbs[2] != null && tbs[2].length > 0) {
          tbs[2].forEach((x) => {
            if (x["start_date"] != null) {
              x["start_date"] = new Date(x["start_date"]);
            }
            if (x["end_date"] != null) {
              x["end_date"] = new Date(x["end_date"]);
            }
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
  var work_positions = null;
  if (
    options.value.work_positions != null &&
    options.value.work_positions.length > 0
  ) {
    work_positions = options.value.work_positions
      .map((x) => x["work_position_id"])
      .join(",");
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
  // var birthplaces = null;
  // if (
  //   options.value.birthplaces != null &&
  //   Object.keys(options.value.birthplaces).length > 0
  // ) {
  //   birthplaces = Object.keys(options.value.birthplaces)
  //     .filter(
  //       (a) =>
  //         Object.entries(options.value.birthplaces).findIndex(
  //           (b) => b[0] == a && b[1]["checked"]
  //         ) !== -1
  //     )
  //     .map((x) => x)
  //     .join(",");
  // }
  if (
    options.value.birthplaces != null &&
    Object.keys(options.value.birthplaces).length > 0
  ) {
    options.value.birthplace_id = Object.keys(options.value.birthplaces)[0];
  }
  var genders = null;
  if (options.value.genders != null && options.value.genders.length > 0) {
    genders = options.value.genders.map((x) => x["gender"]).join(",");
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
            proc: "hrm_profile_count_filter",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
              { par: "organizations", va: organizations },
              { par: "departments", va: departments },
              { par: "work_positions", va: work_positions },
              { par: "professional_works", va: professional_works },
              { par: "birthplace_id", va: options.value.birthplace_id },
              { par: "genders", va: genders },
              {
                par: "birthday_start_date",
                va: options.value.birthday_start_date,
              },
              { par: "birthday_end_date", va: options.value.birthday_end_date },
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
  var work_positions = null;
  if (
    options.value.work_positions != null &&
    options.value.work_positions.length > 0
  ) {
    work_positions = options.value.work_positions
      .map((x) => x["work_position_id"])
      .join(",");
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
  // var birthplaces = null;
  // if (
  //   options.value.birthplaces != null &&
  //   Object.keys(options.value.birthplaces).length > 0
  // ) {
  //   birthplaces = Object.keys(options.value.birthplaces)
  //     .filter(
  //       (a) =>
  //         Object.entries(options.value.birthplaces).findIndex(
  //           (b) => b[0] == a && b[1]["checked"]
  //         ) !== -1
  //     )
  //     .map((x) => x)
  //     .join(",");
  // }
  if (
    options.value.birthplaces != null &&
    Object.keys(options.value.birthplaces).length > 0
  ) {
    options.value.birthplace_id = Object.keys(options.value.birthplaces)[0];
  }
  var genders = null;
  if (options.value.genders != null && options.value.genders.length > 0) {
    genders = options.value.genders.map((x) => x["gender"]).join(",");
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_list_filter_2",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
              { par: "pageNo", va: options.value.pageNo },
              { par: "pageSize", va: options.value.pageSize },
              { par: "tab", va: options.value.tab },
              { par: "organizations", va: organizations },
              { par: "departments", va: departments },
              { par: "work_positions", va: work_positions },
              { par: "professional_works", va: professional_works },
              { par: "birthplace_id", va: options.value.birthplace_id },
              { par: "genders", va: genders },
              {
                par: "birthday_start_date",
                va: options.value.birthday_start_date,
              },
              { par: "birthday_end_date", va: options.value.birthday_end_date },
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
    filterProfile_id: null,
  };
  isFilter.value = false;
  initCount();
  initData(true);
};
onMounted(() => {
  initPlace();
  initDictionary();
  initCount();
  initData(true);
});
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
                        <label>Vị trí</label>
                        <MultiSelect
                          :options="dictionarys[22]"
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
                        <TreeSelect
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
                        </TreeSelect>
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
          @click="refresh()"
          class="p-button-outlined p-button-secondary mr-2"
          icon="pi pi-refresh"
          label="Tải lại"
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
          class="p-button-outlined p-button-secondary"
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
            :class="{ highlight: options.tab === tab.status }"
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
        :value="datas"
        :virtualScrollerOptions="{ itemSize: 78 }"
        :scrollable="true"
        v-model:selection="selectedNodes"
        selectionMode="single"
        dataKey="profile_id"
        scrollHeight="calc(100vh - 170px)"
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
                  {{ slotProps.data.profile_id }}</span
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
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;"
          class="align-items-center justify-content-left text-left"
        >
          <template #body="slotProps">
            <div style="min-width: 200px">
              <div class="mb-1" v-if="slotProps.data.gender">
                <span>{{ slotProps.data.gender == 1 ? "Nam" : "Nữ" }}</span>
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
                <span>{{ slotProps.data.work_position_name }}</span>
              </div>
              <div class="mb-1">
                <span>{{ slotProps.data.department_name }}</span>
              </div>
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
          headerStyle="text-align:center;max-width:100px"
          bodyStyle="text-align:center;max-width:100px"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <ul class="flex p-0" style="list-style: none">
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
            style="
              display: flex;
              width: 100%;
              height: calc(100vh - 210px);
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
