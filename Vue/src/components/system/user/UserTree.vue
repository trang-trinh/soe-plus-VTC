<script setup>
import { ref, inject, onMounted, watch, reactive } from "vue";
import { required, maxLength, minLength, email } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { helpers } from "@vuelidate/validators";
import moment from "moment";
import { encr, getParent } from "../../../util/function.js";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
//init Model
const is_role = ref(false);
const check_role = ref(true);
const role_name = ref();
const user = ref({
  full_name: "",
  is_order: 1,
  is_admin: true,
  status: 1,
});
//Valid Form
const submitted = ref(false);
const rules = {
  user_id: {
    required,
    maxLength: maxLength(50),
  },
  full_name: {
    required,
    maxLength: maxLength(250),
  },
  is_psword: {
    required,
    minLength: minLength(8),
    maxLength: maxLength(25),
  },
  email: {
    email,
  },
};
const v$ = useVuelidate(rules, user);

//Khai báo biến
const organization_name_label = ref();
const organization_id_label = ref();
const is_coppy_module = ref(false);
const data_coppy = ref();
const different_module_move = ref(false);
const user_id_check = ref();
const id_temp = ref();
const role_temp = ref();
const checkFilter = ref(false);
const filterButs = ref();
const isDisplayAvt = ref(false);
const isChuky = ref(false);
const isKynhay = ref(false);
const user_data = ref({});
const tdQuyens = [
  { value: 0, text: "Không có quyền (0)" },
  { value: 1, text: "Xem cá nhân (1)" },
  { value: 2, text: "Xem tất cả (2)" },
  { value: 3, text: "Chỉnh sửa cá nhân (3)" },
  { value: 4, text: "Chỉnh sửa tất cả (4)" },
  { value: 5, text: "Duyệt (5)" },
  { value: 6, text: "Full (6)" },
].reverse();
const store = inject("store");
const isAdd = ref(true);
const selectedKey = ref();
const selectedNodes = ref([]);
const filters = ref({});
const isPaginator = ref(true);
const opition = ref({
  search: "",
  PageNo: 1,
  PageSize: 20,
  Filteruser_id: null,
  user_id: store.getters.user.user_id,
  status: null,
  is_role: false,
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
const selectCapcha = ref();
selectCapcha.value = {};
selectCapcha.value[store.getters.user.organization_id] = true;
const selectCap = ref();
selectCap.value = {};
selectCap.value[
  store.getters.user.is_super ? -1 : store.getters.user.organization_id
] = true;
const users = ref();
const isShowBtnDel = ref(false);
const displayAddUser = ref(false);
const isFirst = ref(true);
let files = [];
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const layout = ref("grid");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const tdstatuss = ref([
  { value: 0, text: "Khoá" },
  { value: 1, text: "Kích hoạt" },
  { value: 2, text: "Đợi xác thực" },
]);
const tdCheckquyens = ref([
  { value: 0, text: "Quyền theo nhóm người dùng" },
  { value: 1, text: "Quyền cá nhân" },
]);
const genders = ref([
  { value: 1, text: "Nam" },
  { value: 0, text: "Nữ" },
]);
const tdRoles = ref([]);
var data_organization = [];
const chucvus = ref([]);
const menuButs = ref();
const menuButMores = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportUser("ExportExcel");
    },
  },
  {
    label: "Nhập Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      //exportUser("ExportExcelMau");
      ImportExcel("ExportExcel");
    },
  },
]);
const showPhongban = () => {
  emitter.emit("emitData", {
    type: "change_type",
    data: {
      isViewList: true,
      isViewTree: false,
    },
  })
};
const item = "/Portals/Mau Excel/Mẫu Excel Người Dùng.xlsx";
const displayImport = ref(false);
const ImportExcel = () => {
  displayImport.value = true;
  file_import = [];
};
let file_import = [];
const removeFile = (event) => {
  file_import = [];
};
const selectFile = (event) => {
  event.files.forEach((element) => {
    file_import.push(element);
  });
};
const Upload = () => {
  displayImport.value = false;
  let formData = new FormData();
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  for (var i = 0; i < file_import.length; i++) {
    let file = file_import[i];
    formData.append("url_file", file);
  }
  formData.append("id", JSON.stringify("1234"));
  axios
    .post(baseURL + "/api/ImportExcel/Import_User", formData, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        var mss = response.data.count
          ? "Đã thêm thành công " + (response.data.count || 0) + " người dùng"
          : "Nhập dữ liệu thành công";
        toast.success(mss);
        loadUser(true);
      } else {
        swal.fire({
          title: "Thông báo",
          html: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const itemButMores = ref([
  {
    label: "Sửa thông tin",
    icon: "pi pi-user-edit",
    command: (event) => {
      editUser(user.value);
    },
  },
  {
    label: "Phân quyền",
    icon: "pi pi-key",
    command: (event) => {
      configRole(user.value);
    },
  },
  // {
  //   label: "Cấu hình môn học",
  //   icon: "pi pi-book",
  //   command: (event) => {
  //     configMonhoc(user.value);
  //   },
  // },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      delUser(user.value);
    },
  },
]);
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
};
const filterUser = () => {
  checkFilter.value = true;
  loadUser(true);
};
const reFilterUser = () => {
  opition.value.position_id = null;
  opition.value.role_id = null;
  opition.value.status = null;
  opition.value.check_quyen = null;
  checkFilter.value = false;
  selectCapcha.value = {};
  selectCapcha.value[-1] = true;
  loadUser(true);
};
watch(opition, () => {
  if (
    opition.value.position_id !== null ||
    opition.value.role_id !== null ||
    opition.value.status !== null ||
    opition.value.check_quyen !== null ||
    Object.keys(selectCap.value)[0] !== "-1"
  ) {
    checkFilter.value = true;
  }
  if (
    opition.value.position_id == null &&
    opition.value.role_id == null &&
    opition.value.check_quyen == null &&
    opition.value.status == null &&
    Object.keys(selectCap.value)[0] == "-1"
  ) {
    checkFilter.value = false;
  }
});
watch(selectCapcha, () => {
  if (
    opition.value.position_id !== null ||
    opition.value.role_id !== null ||
    opition.value.status !== null ||
    opition.value.check_quyen !== null ||
    Object.keys(selectCap.value)[0] !== "-1"
  ) {
    checkFilter.value = true;
  }
  if (
    opition.value.position_id == null &&
    opition.value.role_id == null &&
    opition.value.status == null &&
    opition.value.check_quyen == null &&
    Object.keys(selectCapcha.value)[0] == "-1"
  ) {
    checkFilter.value = false;
  }
});
//Sao chep
const coppyModule = () => {
  is_coppy_module.value = true;
  user_id_check.value = id_temp.value;
  data_coppy.value = JSON.parse(JSON.stringify(modules.value));
  toast.success("Đã sao chép thành công!");
};
const pasteModule = () => {
  modules.value = data_coppy.value;
  toast.success("Dán thành công!");
};
//Khai báo function
const function_modules = ref([]);
const initModuleFunctions = () => {
  axios
    .post(
      baseURL + "/api/Users/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_functions_module_list",
            par: [
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
            ],
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
        function_modules.value = data[0];
      } else {
        function_modules.value = [];
      }
    })
    .catch((error) => { });
};
const changeModuleFunctions = (md) => {
  if (md.children) {
    md.children.forEach((element) => {
      element.data.module_functions = md.data.module_functions;
    });
  }
};
//----------------------
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const toggleMores = (event, u, type) => {
  user.value = u;
  if (u.role_id != "giaovien") {
    //   if (itemButMores.value[2].label == "Cấu hình môn học")
    //     itemButMores.value.splice(2, 1);
    // } else {
    //   if (itemButMores.value[2].label != "Cấu hình môn học")
    //     itemButMores.value.splice(2, 0, {
    //       label: "Cấu hình môn học",
    //       icon: "pi pi-book",
    //       command: (event) => {
    //         configMonhoc(user.value);
    //       },
    //     });
  }
  menuButMores.value.toggle(event);
};
const onNodeSelect = (node) => {
  selectedNodes.value.push(node.data.User_ID);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(selectedNodes.value.indexOf(node.data.User_ID), 1);
};
const handleFileUpload = (event, type) => {
  switch (type) {
    case "Anhdaidien":
      isDisplayAvt.value = true;
      break;
    case "Chuky":
      isChuky.value = true;
      break;
    case "Kynhay":
      isKynhay.value = true;
      break;
  }
  files[type] = event.target.files[0];
  var output = document.getElementById(type);
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
//Show Modal
const showModalAddUser = () => {
  files = [];
  isAdd.value = true;
  submitted.value = false;
  files = [];
  user.value = {
    full_name: null,
    is_psword: null,
    is_order: opition.value.totalrecords + 1,
    status: 1,
    role_id: "nhanvien",
    is_admin: false,
    organization_id: store.getters.user.organization_id,
    display_birthday: true,
    email: null,
    is_booking: true,
  };
  // selectCapcha.value = {};
  // selectCapcha.value[user.value.organization_id || "-1"] = true;
  displayAddUser.value = true;
  if (document.querySelector("#AnhUser"))
    document.querySelector("#AnhUser").value = "";
  if (document.querySelector("#UserKynhay"))
    document.querySelector("#UserKynhay").value = "";
  if (document.querySelector("#UserChuky"))
    document.querySelector("#UserChuky").value = "";
};
const chonanh = (id) => {
  document.getElementById(id).click();
};
const closedisplayAddUser = () => {
  displayAddUser.value = false;
};
//delete img
const delAvatar = () => {
  files["Anhdaidien"] = [];
  isDisplayAvt.value = false;
  var output = document.getElementById("userAnh");
  output.src = basedomainURL + "/Portals/Image/noimg.jpg";
  user.value.avatar = null;
};
const delImg = (id) => {
  files[id] = [];
  var output = document.getElementById(id);
  output.src = basedomainURL + "/Portals/Image/noimg.jpg";
  switch (id) {
    case "Anhdaidien":
      isDisplayAvt.value = false;
      user.value.avatar = null;
      break;
    case "Chuky":
      isChuky.value = false;
      user.value.signature = null;
      break;
    case "Kynhay":
      isKynhay.value = false;
      user.value.flash_signature = null;
      break;
  }
};
//Thêm sửa xoá
const onRefersh = () => {
  is_coppy_module.value = false;
  different_module_move.value = false;
  role_id_check.value = null;
  opition.value = {
    search: "",
    PageNo: 1,
    PageSize: 20,
    Filteruser_id: null,
    user_id: store.getters.user.user_id,
    status: null,
    tenstatus: "",
    filter_department: -1,
    check_quyen: null,
  };
  layout.value = "grid";
  displayPhongban.value = false;
  loadUser(true);
};
const onSearch = () => {
  opition.value.PageNo = 1;
  loadUser(true);
};
const donvis = ref();
const treedonvis = ref();
const renderTreeDV = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em };
            rechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m[id]);
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
  arrtreeChils.unshift({
    key: -1,
    data: -1,
    label: "-----Chọn " + title + "----",
  });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const tudiens = ref([]);
const initTudien = () => {
  axios
    .post(
      baseURL + "/api/Users/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_users_list_dictionary",
            par: [{ par: "user_id", va: opition.value.user_id }],
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
        tdRoles.value = data[0];
        chucvus.value = data[2];
      }
      data_organization = data[1];
      if (data_organization.length > 0) {
        let obj = renderTreeDV(
          data_organization,
          "organization_id",
          "organization_name",
          "phòng ban"
        );
        //  donvis.value = obj.arrChils;
        treedonvis.value = obj.arrtreeChils;
      } else {
        treedonvis.value = [];
      }
      // data[4].forEach((ch) => {
      //   ch.items = data[3].filter((x) => x.Caphoc_ID == ch.value);
      //   ch.chon = false;
      // });
      // data[4] = data[4].filter((x) => x.items.length > 0);
      //tudiens.value = data;
    })
    .catch((error) => { });
};
const loadPhongban = (rf) => {
  axios
    .post(
      baseURL + "/api/Users/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_users_list_dictionary_1",
            par: [
              { par: "pageno", va: opition.value.PageNo },
              { par: "pagesize", va: opition.value.PageSize },
              { par: "search", va: opition.value.search },
              { par: "user_id", va: store.getters.user.user_id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )

    .then((response) => {
      debugger
      let data = JSON.parse(response.data.data)[0];
      if (isFirst.value) isFirst.value = false;
      let obj = renderTree(data, "organization_id", "organization_name", "đơn vị");
      donvis.value = obj.arrChils;
      opition.value.loading = false;

      if (data[0] != null)
        loadUser(true, data[0].organization_id, data[0].organization_name);
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const loadCount = (id) => {
  axios
    .post(
      baseURL + "/api/Users/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_users_count_new",
            par: [
              { par: "search", va: opition.value.search },
              { par: "user_id", va: opition.value.user_id },
              { par: "role_id", va: opition.value.role_id },
              { par: "organization_id", va: opition.value.organization_id },
              { par: "department_id", va: id || opition.value.department_id },
              { par: "position_id", va: opition.value.position_id },
              { par: "filter_department", va: opition.value.filter_department },
              { par: "filter_permission", va: opition.value.check_quyen },
              { par: "status", va: opition.value.status },
            ],
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
        opition.value.totalrecords = data[0].totalrecords;
        // if (f)
        //   opition.value.totalrecords > opition.value.PageSize
        //     ? (isPaginator.value = true)
        //     : (isPaginator.value = false);
        // opition.value.totalrecords > 0
        //   ? (isPaginator.value = true)
        //   : (isPaginator.value = false);
      }
    })
    .catch((error) => { });
};
const onPage = (event) => {
  isPaginator.value = true;
  opition.value.PageNo = event.page + 1;
  opition.value.PageSize = event.rows;
  loadUser(true);
};
const goDonvi = (u) => {
  selectCapcha.value = {};
  if (u) {
    opition.value.department_id = u.department_id;
    opition.value.organization_id = u.organization_id;
    opition.value.organization_name = u.organization_name;
    selectCapcha.value[opition.value.department_id] = true;
  } else {
    opition.value.department_id = null;
    opition.value.organization_name = null;
    opition.value.organization_id = null;
    selectCapcha.value[store.getters.user.organization_id] = true;
  }
  opition.value.PageNo = 1;
  loadUser(true);
};
const goChucvu = (u) => {
  if (u) {
    opition.value.position_id = u.position_id;
    opition.value.position_name = u.position_name;
  } else {
    opition.value.position_id = null;
    opition.value.position_name = null;
    //     //remove background filter
    // if( opition.value.role_id == null){
    //   checkFilter.value = false;
    // }
  }
  opition.value.PageNo = 1;
  loadUser(true);
};
const goRole = (u) => {
  if (u) {
    opition.value.role_id = u.role_id;
    opition.value.role_name = u.role_name;
    opition.value.text_color = u.text_color;
    opition.value.background_color = u.background_color;
  } else {
    opition.value.role_id = null;
    opition.value.role_name = null;
    // //remove background filter
    // if( opition.value.position_id == null){
    //   checkFilter.value = false;
    // }
  }
  opition.value.PageNo = 1;
  loadUser(true);
};
const gostatus = (u) => {
  if (u) {
    opition.value.status = u.status;
    opition.value.tenstatus = u.tenstatus;
  } else {
    opition.value.status = null;
    opition.value.tenstatus = null;
  }
  opition.value.PageNo = 1;
  loadUser(true);
};
const goQuyen = (u) => {
  if (u) {
    opition.value.check_quyen = u.check_quyen;
    opition.value.check_quyen_label = u.check_quyen_label;
  } else {
    opition.value.check_quyen = null;
    opition.value.check_quyen_label = null;
  }
  opition.value.PageNo = 1;
  loadUser(true);
};
const resetopition = () => {
  if (opition.value.role_id && !opition.value.role_name) {
    let u = tdRoles.value.find((x) => x.role_id == opition.value.role_id);
    opition.value.role_name = u.role_name;
    opition.value.text_color = u.text_color;
    opition.value.background_color = u.background_color;
  }
  if (opition.value.organization_id && !opition.value.organization_name) {
    opition.value.organization_name = users.value.find(
      (x) => x.organization_id == opition.value.organization_id
    ).organization_name;
  }
  if (opition.value.status && !opition.value.tenstatus) {
    opition.value.tenstatus = tdstatuss.value.find(
      (x) => x.status == opition.value.status
    ).tenstatus;
  }
};
const loadUser = (rf, id, name) => {
  organization_name_label.value = name;
  organization_id_label.value = id;
  if (opition.value.role_id != null)
    opition.value.role_name = tdRoles.value.filter(
      (x) => x.role_id == opition.value.role_id
    )[0].role_name;
  else opition.value.role_name = null;
  if (opition.value.status != null)
    opition.value.tenstatus = tdstatuss.value.filter(
      (x) => x.value == opition.value.status
    )[0].text;
  else opition.value.tenstatus = null;
  if (opition.value.position_id != null)
    opition.value.position_name = chucvus.value.filter(
      (x) => x.position_id == opition.value.position_id
    )[0].position_name;
  else opition.value.position_name = null;
  //resetopition();
  if (selectCap.value !== null) {
    opition.value.filter_department = parseInt(Object.keys(selectCap.value)[0]);
  }
  if (opition.value.check_quyen != null)
    opition.value.check_quyen_label = tdCheckquyens.value.filter(
      (x) => x.value == opition.value.check_quyen
    )[0].text;
  else opition.value.check_quyen_label = null;
  if (rf) {
    opition.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
    loadCount(id);
  }
  axios
    .post(
      baseURL + "/api/Users/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_users_list_new",
            par: [
              { par: "search", va: opition.value.search },
              { par: "user_id", va: opition.value.user_id },
              { par: "role_id", va: opition.value.role_id },
              { par: "organization_id", va: opition.value.organization_id },
              { par: "department_id", va: id || opition.value.department_id },
              { par: "position_id", va: opition.value.position_id },
              { par: "filter_department", va: opition.value.filter_department },
              { par: "filter_permission", va: opition.value.check_quyen },
              { par: "pageno", va: opition.value.PageNo || 1 },
              { par: "pagesize", va: opition.value.PageSize || 20 },
              { par: "status", va: opition.value.status },
            ],
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
        data.forEach((item, index) => {
          let fullName = item.full_name.split(" ");
          item.lastName = fullName[fullName.length - 1];
          item.stt = index + 1;
        });
        users.value = data;
        // if(displayPhongban.value== true){
        //   initUserPhongban();
        // }
      } else {
        users.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      if (rf) {
        opition.value.loading = false;
        swal.close();
      }
      // if (rfpb || displayPhongban.value == true) {
      //   initUserPhongban();
      // }
    })
    .catch((error) => {
      console.log(error);
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const editUser = (md) => {
  files = [];
  submitted.value = false;
  isAdd.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddUser.value = true;
  axios
    .post(
      baseURL + "/api/Users/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_users_get",
            par: [{ par: "user_id", va: md.user_id }],
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
        user.value = data[0][0];
        if (user.value.birthday) {
          var dt = new Date(user.value.birthday);
          user.value.birthday = new Date(
            dt.getFullYear(),
            dt.getMonth(),
            dt.getDate()
          );
        }
        getInfoPass(user.value);
        selectCapcha.value = {};
        selectCapcha.value[user.value.department_id || user.value.organization_child_id || user.value.organization_id || "-1"] = true;
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const getInfoPass = (item) => {
  axios
    .post(baseURL + "/api/Users/Get_InfoUser", item, config)
    .then((response) => {
      if (response.data.data) {
        user.value.is_psword = JSON.parse(response.data.data).data;
      }
    });
};
const handleSubmit = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (user.value.user_id.length > 50) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng chọn tên đăng nhập không quá 50 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (user.value.full_name.length > 500) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng chọn họ tên không quá 500 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (user.value.is_psword.length < 8) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng chọn mật khẩu có ít nhất 8 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (!user.value.role_id) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng chọn nhóm người dùng!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  let id_key = Object.keys(selectCapcha.value)[0];
  if (id_key == -1) {
    user.value.department_id = null;
    user.value.organization_id = null;
    user.value.organization_child_id = null;
  }
  else {
    //get organization_parent and child
    let obj = data_organization.filter(x => x.organization_id == id_key);
    if (obj.length > 0) {
      let list_id = obj[0].listparent_id.slice(0, -1).split("/").map(item => parseInt(item) ? parseInt(item) : item);
      user.value.organization_id = list_id[0];
      user.value.department_id = list_id[list_id.length - 1];
      list_id.forEach((id) => {
        let org = data_organization.filter(x => x.organization_id == id);
        if (org.length > 0 && org[0].organization_type == 0) {
          user.value.organization_child_id = id;
        }
      })
      //check 
      if (user.value.department_id == user.value.organization_child_id) {
        user.value.department_id = null;
      }
      if (user.value.organization_id == user.value.department_id) {
        user.value.department_id = null;
        user.value.organization_child_id = null;
      }
      if (user.value.organization_id == user.value.organization_child_id) {
        user.value.organization_child_id = null;
      }
      if (user.value.is_admin && user.value.organization_child_id !== null) user.value.is_admin_child = true;
    }
  }
  // user.value.department_id = keys[0];
  // if (user.value.department_id == -1) {
  //   user.value.department_id = null;
  //   user.value.organization_id = null;
  // }
  // if (user.value.department_id) {
  //   const result = getParent(treedonvis.value, user.value.department_id, "key");
  //   user.value.organization_id = result.key;
  // }
  if (user.value.full_name) {
    user.value.last_name = user.value.full_name.split(" ").slice(-1).join(" ");
    user.value.full_name_en = removeVietnameseTones(user.value.full_name);
  }
  addUser();
};
const addUser = () => {
  let formData = new FormData();
  for (var k in files) {
    let file = files[k];
    formData.append(k, file);
  }
  formData.append("model", JSON.stringify(user.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: isAdd.value == false ? "put" : "post",
    url:
      baseURL +
      `/api/Users/${isAdd.value == false ? "Update_Users" : "Add_Users"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật User thành công!");
        //re-name and re-icon in header-bar
        // if(!isAdd.value && user.value.user_id == store.getters.user.user_id){
        //   store.getters.user.full_name = user.value.full_name;
        //   store.getters.user.avatar = user.value.avatar;
        // }
        loadUser(true);
        closedisplayAddUser();
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
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
// xóa người dùng
const delUser = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá người dùng này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        axios
          .delete(baseURL + "/api/Users/Del_Users", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.user_id] : selectedNodes.value,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá User thành công!");
              loadUser(true);
              if (!md) selectedNodes.value = [];
            } else {
              swal.fire({
                title: "Thông báo!",
                text: "Xóa không thành công, vui lòng thử lại",
                icon: "error",
                confirmButtonText: "OK",
              });
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
    });
};
// xóa nhiều người dùng
const delListUsers = () => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xóa danh sách người dùng này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        let listId = users.value
          .filter((x) => x.chon == true)
          .map((x) => x.user_id);
        axios
          .delete(baseURL + "/api/Users/Del_Users", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá người dùng thành công!");

              loadUser(true);
            } else {
              swal.fire({
                title: "Thông báo!",
                text: "Xóa không thành công, vui lòng thử lại",
                icon: "error",
                confirmButtonText: "OK",
              });
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
    });
};
const exportUser = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let proc = "sys_users_listexport1";
  if (method == "ExportExcelMau") {
    proc = "sys_users_listexport_mau";
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH NGƯỜI DÙNG",
        proc: proc,
        par: [
          { par: "search", va: opition.value.search },
          { par: "user_id", va: opition.value.user_id },
          { par: "role_id", va: opition.value.role_id },
          { par: "organization_id", va: opition.value.organization_id },
          { par: "department_id", va: opition.value.department_id },
          { par: "position_id", va: opition.value.position_id },
          { par: "filter_department", va: opition.value.filter_department },
          { par: "filter_permission", va: opition.value.check_quyen },
          { par: "status", va: opition.value.status },
        ],
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Kết xuất Data thành công!");
        //window.open(baseURL + response.data.path);
        if (response.data.path != null) {
          let pathReplace = response.data.path
            .replace(/\\+/g, "/")
            .replace(/\/+/g, "/")
            .replace(/^\//g, "");
          var listPath = pathReplace.split("/");
          var pathFile = "";
          listPath.forEach((item) => {
            if (item.trim() != "") {
              pathFile += "/" + item;
            }
          });
          window.open(baseURL + pathFile);
        }
      } else {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};

const displayConfigRole = ref(false);
const modules = ref([]);
const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      m.label_order = m.IsOrder.toString();
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em, index) => {
            em.label_order = mm.data.label_order + "." + (index + 1);
            let om1 = { key: em[id], data: em };
            rechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m[id]);
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
  arrtreeChils.unshift({
    key: -1,
    data: -1,
    label: "-----Chọn " + title + "----",
  });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const changePermission = () => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn chắc chắn muốn thay đổi quyền cho người dùng này không?",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        let formData = new FormData();
        formData.append("user_id", user_data.value.user_id);
        axios
          .delete(baseURL + "/api/Users/Del_Roles_User", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: formData,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Phân quyền thành công!");
              displayConfigRole.value = false;
            } else {
              swal.fire({
                title: "Thông báo!",
                text: "Phân quyền không thành công, vui lòng thử lại",
                icon: "error",
                confirmButtonText: "OK",
              });
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
    });
};

const configRole = (md) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (user_id_check.value && md.user_id != user_id_check.value) {
    different_module_move.value = true;
  } else different_module_move.value = false;
  id_temp.value = md.user_id;
  role_temp.value = md.role_id;
  user_data.value = { role_id: md.role_id, user_id: md.user_id };
  modules.value.forEach((el) => {
    el.data.is_permission = null;
    el.data.module_functions = null;
    if (el.children) {
      el.children.forEach((element) => {
        element.data.is_permission = null;
        element.data.module_functions = null;
      });
    }
  });
  //Config quyền
  opition.value.moduleloading = true;
  axios
    .post(
      baseURL + "/api/Users/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_module_listby_user",
            par: [
              { par: "role_id ", va: md.role_id },
              { par: "user_id ", va: md.user_id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      opition.value.moduleloading = false;
      let data = JSON.parse(response.data.data)[0];
      let data1 = JSON.parse(response.data.data)[1];
      debugger
      if (data1.length > 0) {
        is_role.value = data1[0].is_role;
        role_name.value = data1[0].role_name;
      }
      opition.value.is_role = is_role.value;
      check_role.value = is_role.value;
      if (data.length > 0) {
        //is_role.value = data[0].is_role;
        data
          .filter((x) => x.is_permission)
          .forEach((r) => {
            let ds = r.is_permission.toString().split("");
            var arrs = [];
            ds.forEach((e) => {
              arrs.push(parseInt(e));
            });
            r.is_permission = arrs;
          });
        data
          .filter((x) => x.module_functions)
          .forEach((r) => {
            let ds = r.module_functions.toString().split(",");
            var arrs = [];
            ds.forEach((e) => {
              arrs.push(e);
            });
            r.module_functions = arrs;
          });
        renderTree(data);
        swal.close();
        displayConfigRole.value = true;
      } else {
        modules.value = [];
      }
    })
    .catch((error) => {
      opition.value.moduleloading = false;
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const closedisplayConfigRole = () => {
  displayConfigRole.value = false;
};
const changeQuyen = (md) => {
  if (md.children) {
    getPermissionChildren(md);
    // md.children.forEach((element) => {
    //   element.data.is_permission = md.data.is_permission;

    // });
  }
};
function getPermissionChildren(md) {
  md.children.forEach((element) => {
    element.data.is_permission = md.data.is_permission;
    if (element.children) getPermissionChildren(element);
  });
}
const clickDelUser = () => {
  if (users.value.filter((x) => x.chon == true).length > 0) {
    isShowBtnDel.value = true;
  } else isShowBtnDel.value = false;
};
const addConfigRole = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let mdmodules = [];
  modules.value.forEach((element) => {
    mdmodules.push({
      role_module_id: element.data.role_module_id || -1,
      role_id: is_coppy_module.value ? role_temp.value : element.data.role_id,
      user_id: is_coppy_module.value ? id_temp.value : element.data.user_id,
      module_id: element.data.module_id,
      IsCap: element.data.IsCap,
      is_permission: element.data.is_permission
        ? element.data.is_permission.join("")
        : element.data.is_permission,
      module_functions: element.data.module_functions
        ? element.data.module_functions.join()
        : element.data.module_functions,
    });
    if (element.children && element.children.length > 0) {
      element.children.forEach((ec) => {
        mdmodules.push({
          role_module_id: ec.data.role_module_id || -1,
          role_id: is_coppy_module.value ? role_temp.value : ec.data.role_id,
          user_id: is_coppy_module.value ? id_temp.value : element.data.user_id,
          module_id: ec.data.module_id,
          IsCap: ec.data.IsCap,
          is_permission: ec.data.is_permission
            ? ec.data.is_permission.join("")
            : ec.data.is_permission,
          module_functions: element.data.module_functions
            ? element.data.module_functions.join()
            : element.data.module_functions,
        });
        // get value module level 3 (it module khong can de quy - co time xem lai)
        if (ec.children && ec.children.length > 0) {
          ec.children.forEach((ec2) => {
            mdmodules.push({
              role_module_id: ec2.data.role_module_id || -1,
              role_id: is_coppy_module.value ? role_temp.value : ec2.data.role_id,
              user_id: is_coppy_module.value ? id_temp.value : ec2.data.user_id,
              module_id: ec2.data.module_id,
              is_grade: ec2.data.is_grade,
              is_permission: ec2.data.is_permission
                ? ec2.data.is_permission.join("")
                : ec2.data.is_permission,
              module_functions: ec2.data.module_functions
                ? ec2.data.module_functions.join()
                : ec2.data.module_functions,
            });
          });
        }
      });
    }
  });
  axios({
    method: "post",
    url: baseURL + "/api/Roles/Add_RoleModules",
    data: mdmodules,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Phân quyền user thành công!");
        displayConfigRole.value = false;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
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
};
const displayPhongban = ref(false);
// const showPhongban = () => {
//   displayPhongban.value = !displayPhongban.value;
//   if (displayPhongban.value) {
//     layout.value = "list";
//     opition.value.PageSize = opition.value.totalrecords;
//     opition.value.PageNo = 1;
//     loadUser(true, true);
//   }
// };
const initUserPhongban = () => {
  const ipb = (dv) => {
    if (dv.children) {
      dv.children.forEach((el) => {
        ipb(el);
      });
    }
    //if (dv.data.IsDonvi == false) {
    let us = users.value.filter((x) => x.organization_id == dv.key);
    if (us.length > 0) {
      if (!dv.children) dv.children = [];
      dv.children = [];
      // dv.children = dv.children.concat(us);
      dv.children = dv.children.concat({
        data: { organization_name: "", IsDonvi: false },
        users: us,
      });
      if (!dv.data.organization_name.includes("(")) {
        dv.data.organization_name =
          dv.data.organization_name + " (" + us.length + ")";
      } else {
        let idx1 = dv.data.organization_name.indexOf("(");
        let idx2 = dv.data.organization_name.length;
        let getString = dv.data.organization_name.substring(idx1, idx2);
        dv.data.organization_name = dv.data.organization_name.replace(
          getString,
          " (" + us.length + ")"
        );
      }
      //dv.users = us;
    } else {
      dv.children = [];
      if (!dv.data.organization_name.includes("(")) {
        dv.data.organization_name =
          dv.data.organization_name + " (" + us.length + ")";
      } else {
        let idx1 = dv.data.organization_name.indexOf("(");
        let idx2 = dv.data.organization_name.length;
        let getString = dv.data.organization_name.substring(idx1, idx2);
        dv.data.organization_name = dv.data.organization_name.replace(
          getString,
          " (" + us.length + ")"
        );
      }
    }
    //}
  };
  donvis.value.forEach((dv) => {
    ipb(dv);
  });
};
function removeVietnameseTones(str) {
  str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
  str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
  str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
  str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
  str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
  str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
  str = str.replace(/đ/g, "d");
  str = str.replace(/À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ|Ặ|Ẳ|Ẵ/g, "A");
  str = str.replace(/È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ/g, "E");
  str = str.replace(/Ì|Í|Ị|Ỉ|Ĩ/g, "I");
  str = str.replace(/Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ|Ợ|Ở|Ỡ/g, "O");
  str = str.replace(/Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ/g, "U");
  str = str.replace(/Ỳ|Ý|Ỵ|Ỷ|Ỹ/g, "Y");
  str = str.replace(/Đ/g, "D");
  // Some system encode vietnamese combining accent as individual utf-8 characters
  // Một vài bộ encode coi các dấu mũ, dấu chữ như một kí tự riêng biệt nên thêm hai dòng này
  str = str.replace(/\u0300|\u0301|\u0303|\u0309|\u0323/g, ""); // ̀ ́ ̃ ̉ ̣  huyền, sắc, ngã, hỏi, nặng
  str = str.replace(/\u02C6|\u0306|\u031B/g, ""); // ˆ ̆ ̛  Â, Ê, Ă, Ơ, Ư
  // Remove extra spaces
  // Bỏ các khoảng trắng liền nhau
  str = str.replace(/ + /g, " ");
  str = str.trim();
  // Remove punctuations
  // Bỏ dấu câu, kí tự đặc biệt
  str = str.replace(
    /!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'|\"|\&|\#|\[|\]|~|\$|_|`|-|{|}|\||\\/g,
    " "
  );
  return str;
}
function getFormattedDate(date) {
  var year = date.getFullYear();

  var month = (1 + date.getMonth()).toString();
  month = month.length > 1 ? month : "0" + month;

  var day = date.getDate().toString();
  day = day.length > 1 ? day : "0" + day;

  return month + "/" + day + "/" + year;
}

onMounted(() => {
  //init
  loadPhongban(true);
  // loadUser(true);
  initTudien();
  initModuleFunctions();
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <div>
      <!-- <div class="p-3">
        <h3 class="module-title m-0 ">
          <i class="pi pi-book"></i> Danh sách đơn vị, phòng ban
        </h3>

      </div> -->
      <div>
        <Splitter class=" h-full w-full pb-0 pr-0">
          <SplitterPanel :size="35" class=" ">
            <div class="pr-3">
              <div>
                <Toolbar>
                  <template #start>
                    <span class="p-input-icon-left">
                      <i class="pi pi-search" />
                      <InputText v-model="filters['global']" type="text" spellcheck="false"
                        placeholder="Tìm kiếm đơn vị" />
                    </span>
                  </template>
                  <template #end>
                    <Button v-if="store.getters.user.is_super" @click="showModalAddDonvi(0)" label="Thêm đơn vị"
                      icon="pi pi-plus" class="mr-2" />

                  </template>
                </Toolbar>
              </div>
              <div style="border-top:2px solid #dee2e6">

                <div class="w-full d-lang-table">
                  <TreeTable :value="donvis" v-model:selectionKeys="selectedKey" :paginator="true"
                    @nodeSelect="onNodeSelect" @nodeUnselect="onNodeUnselect" :filters="filters" :showGridlines="true"
                    filterMode="strict" class="p-treetable-sm" :rows="20" :rowHover="true" responsiveLayout="scroll"
                    :scrollable="true" scrollHeight="flex">
                    <Column field="Logo" header="Logo" class="align-items-center justify-content-center text-center"
                      headerStyle="text-align:center;max-width:80px" bodyStyle="text-align:center;max-width:80px">
                      <template #body="md">
                        <div :class="md.node.data.organization_id === organization_id_label? 'row-active': ''" @click="loadUser(true,md.node.data.organization_id, md.node.data.organization_name)">
                          <Avatar v-if="md.node.data.logo" :image="basedomainURL + md.node.data.logo" class="mr-2"
                            size="large" />
                        </div>
                      </template>
                    </Column>
                    <Column field="organization_name" header="Tên đơn vị" :expander="true">
                      <template #body="md">
                        <div :class="md.node.data.organization_id === organization_id_label? 'row-active': ''" @click="loadUser(true,md.node.data.organization_id, md.node.data.organization_name)">
                          <span :class="'donvi' + md.node.data.organization_type" :style="[
                            md.node.data ? 'font-weight:bold' : '',
                            md.node.data.status ? '' : 'color:red !important',
                          ]">{{ md.node.data.organization_name }}</span>
                        </div>
                      </template>
                    </Column>
                    <template #empty>
                      <div class="m-auto align-items-center
                                                          justify-content-center
                                                          p-4
                                                          text-center
                         " v-if="!isFirst">
                        <img src="../../assets/background/nodata.png" height="144" />
                        <h3 class="m-1">Không có dữ liệu</h3>
                      </div>
                    </template>
                  </TreeTable>
                </div>
              </div>
            </div>
          </SplitterPanel>
          <SplitterPanel :size="65">
            <div class="w-full d-lang-table-r">
              <DataView class="w-full h-full flex flex-column" :lazy="true" :value="users" :layout="layout"
              :loading="opition.loading" :paginator="isPaginator" :rows="opition.PageSize"
              :totalRecords="opition.totalrecords" :pageLinkSize="opition.PageSize" @page="onPage($event)"
              paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
              :rowsPerPageOptions="[20, 30, 50, 100, 200]" currentPageReportTemplate="" responsiveLayout="scroll"
              :scrollable="true">
              <template #header>
                <h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
                  <i class="pi pi-users"></i> {{organization_name_label}}
                  <span v-if="opition.totalrecords > 0">({{ opition.totalrecords }})</span>
                  <Chip class="custom-chip ml-2 mr-1" @remove="goRole()" v-if="opition.role_name"
                    :label="opition.role_name" :style="{
                      background: opition.background_color,
                      color: opition.text_color,
                    }" removable />
                  <Chip class="custom-chip ml-2 mr-1" @remove="goChucvu()" v-if="opition.position_name"
                    :label="opition.position_name" removable />
                  <Chip class="custom-chip chippb ml-2 mr-1" @remove="goDonvi()"
                    v-if="opition.department_id || opition.organization_id" :label="opition.organization_name"
                    removable />
                  <Chip class="custom-chip ml-2 mr-1" v-bind:class="
                    'chip-' +
                    (opition.status == 0
                      ? 'danger'
                      : opition.status == 1
                        ? 'success'
                        : 'warning')
                  " @remove="gostatus()" v-if="opition.tenstatus" :label="opition.tenstatus" removable />
                  <Chip class="custom-chip chippb ml-2 mr-1" @remove="goQuyen()" v-if="opition.check_quyen_label"
                    :label="opition.check_quyen_label" removable />
                </h3>
                <Toolbar class="w-full custoolbar">
                  <template #start>
                    <div class="p-input-icon-left">
                      <i class="pi pi-search" />
                      <InputText type="text" spellcheck="false" v-model="opition.search" placeholder="Tìm kiếm"
                        v-on:keyup.enter="onSearch" />
                    </div>
                    <Button :class="
                      checkFilter
                        ? 'ml-2'
                        : 'ml-2 p-button-secondary p-button-outlined'
                    " icon="pi pi-filter" @click="toggleFilter" aria-haspopup="true" aria-controls="overlay_panelS" />
                    <OverlayPanel ref="filterButs" appendTo="body" :showCloseIcon="false" id="overlay_panelS"
                      style="width: 350px" :breakpoints="{ '960px': '20vw' }">
                      <div class="grid formgrid m-2">
                        <div class="field col-12 md:col-12 flex align-items-center">
                          <div class="col-4 p-0">Đơn vị/Phòng ban:</div>
                          <TreeSelect class="col-8 p-0" v-model="selectCap" :options="treedonvis" :showClear="true"
                            :max-height="200" placeholder="Chọn phòng ban" optionLabel="organization_name"
                            optionValue="organization_id">
                          </TreeSelect>
                        </div>
                        <div class="field col-12 md:col-12 flex align-items-center">
                          <div class="col-4 p-0">Chức vụ:</div>
                          <Dropdown :showClear="true" v-model="opition.position_id" :options="chucvus"
                            optionLabel="position_name" optionValue="position_id" placeholder="Chọn chức vụ"
                            class="p-dropdown-sm col-8 p-0" />
                        </div>
                        <div class="field col-12 md:col-12 flex align-items-center">
                          <div class="col-4 p-0">Nhóm người dùng:</div>
                          <Dropdown :showClear="true" v-model="opition.role_id" :options="tdRoles" optionLabel="role_name"
                            optionValue="role_id" placeholder="Chọn nhóm người dùng" class="p-dropdown-sm col-8 p-0" />
                        </div>
                        <div class="field col-12 md:col-12 flex align-items-center">
                          <div class="col-4 p-0">Trạng thái:</div>
                          <Dropdown :showClear="true" v-model="opition.status" :options="tdstatuss" optionLabel="text"
                            optionValue="value" placeholder="Chọn trạng thái" class="p-dropdown-sm col-8 p-0" />
                        </div>
                        <div class="field col-12 md:col-12 flex align-items-center">
                          <div class="col-4 p-0">Quyền Module:</div>
                          <Dropdown :showClear="true" v-model="opition.check_quyen" :options="tdCheckquyens"
                            optionLabel="text" optionValue="value" placeholder="Chọn loại quyền"
                            class="p-dropdown-sm col-8 p-0" />
                        </div>
                        <div class="col-12 field p-0">
                          <Toolbar class="toolbar-filter">
                            <template #start>
                              <Button @click="reFilterUser" class="p-button-outlined" label="Xóa"></Button>
                            </template>
                            <template #end>
                              <Button @click="filterUser" label="Lọc"></Button>
                            </template>
                          </Toolbar>
                        </div>
                      </div>
                    </OverlayPanel>
                  </template>

                  <template #end>
                    <Button label="Thêm người dùng" icon="pi pi-plus" class="mr-2" @click="showModalAddUser" />
                    <DataViewLayoutOptions v-model="layout" />
                    <Button icon="pi pi-list" v-tooltip.left="'Hiển thị phòng ban'" v-bind:class="
                      'ml-2 p-button p-button-' +
                      (displayPhongban ? 'primary' : 'secondary')
                    " @click="showPhongban" />
                    <Button class="mr-2 ml-2 p-button p-button-outlined p-button-secondary" icon="pi pi-refresh"
                      @click="onRefersh" />
                    <Button label="Xoá" icon="pi pi-trash" class="mr-2 p-button-danger" v-if="isShowBtnDel"
                      @click="delListUsers" />
                    <Button label="Tiện ích" icon="pi pi-file-excel"
                      class="mr-2 p-button p-button-outlined p-button-secondary" @click="toggleExport"
                      aria-haspopup="true" aria-controls="overlay_Export" />
                    <Menu id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" />
                  </template>
                </Toolbar>
              </template>
              <template #grid="slotProps">
                <div class="md:col-4 p-2 card-content">
                  <Card class="no-paddcontent">
                    <template #title>
                      <div style="position: relative">
                        <div class="align-items-center justify-content-center text-center" style="position: relative">
                          <Avatar v-bind:label="
                            slotProps.data.avatar
                              ? ''
                              : slotProps.data.lastName.substring(0, 1).toUpperCase()
                          " v-bind:image="basedomainURL + slotProps.data.avatar" style="
                        background-color: #2196f3;
                        color: #ffffff;
                        width: 8rem;
                        height: 8rem;
                      " :style="{
                        background: bgColor[slotProps.data.stt % 7],
                      }" class="mr-2" size="xlarge" shape="circle" />
                          <Button @click="gostatus(slotProps.data)" v-bind:class="
                            'p-button p-button-' +
                            (slotProps.data.status == 0
                              ? 'danger'
                              : slotProps.data.status == 1
                                ? 'success'
                                : 'warning') +
                            ' p-button-rounded dot-status'
                          " />
                          <Button @click="gostatus(slotProps.data)" v-bind:class="
                            'p-button p-button-' +
                            (slotProps.data.status == 0
                              ? 'danger'
                              : slotProps.data.status == 1
                                ? 'success'
                                : 'warning') +
                            ' p-button-rounded dot-status'
                          " />
                          <i @click="goQuyen(slotProps.data)" v-if="slotProps.data.check_quyen == 1"
                            v-tooltip.top="slotProps.data.check_quyen_label" class="pi pi-star-fill icon-quyen"></i>
                        </div>
                        <Button style="position: absolute; right: 0px; top: 0px" icon="pi pi-ellipsis-h"
                          class="p-button-rounded p-button-text ml-2" @click="toggleMores($event, slotProps.data)"
                          aria-haspopup="true" aria-controls="overlay_More" />
                      </div>
                    </template>
                    <template #content>
                      <div class="text-center">
                        <Button class="p-button-text m-auto block" style="color: inherit"
                          @click="editUser(slotProps.data)">
                          <h3 class="m-0">
                            {{ slotProps.data.full_name }}
                          </h3>
                        </Button>
                        <Chip @click="goDonvi(slotProps.data)" class="m-1 chippb p-ripple" v-ripple
                          :label="slotProps.data.organization_name"></Chip>
                        <div class="mb-1" v-if="slotProps.data.position_name">
                          <Chip @click="goChucvu(slotProps.data)" v-ripple class="p-ripple chip2"
                            v-bind:label="slotProps.data.position_name" />
                        </div>
                        <div class="mb-1">
                          <Chip @click="goRole(slotProps.data)" v-ripple class="p-ripple" :style="{
                            background: slotProps.data.background_color,
                            color: slotProps.data.text_color,
                          }" v-bind:label="slotProps.data.role_name" />
                        </div>
                        <!-- <div class="mb-1" v-if="slotProps.data.check_quyen != null">
                    <Chip
                      @click="goQuyen(slotProps.data)"
                      v-ripple
                      class="p-ripple chip2"
                      v-bind:label="slotProps.data.check_quyen_label"
                    />
                  </div> -->
                      </div>
                    </template>
                  </Card>
                </div>
              </template>
              <template #list="slotProps">
                <div class="p-2 w-full" style="background-color: #fff">
                  <div class="flex align-items-center justify-content-center">
                    <div class="mx-2">
                      <Checkbox id="IsIdentity" v-model="slotProps.data.chon" :binary="true" @change="clickDelUser" />
                    </div>
                    <Avatar v-bind:label="
                      slotProps.data.avatar
                        ? ''
                        : slotProps.data.lastName.substring(0, 1).toUpperCase()
                    " v-bind:image="basedomainURL + slotProps.data.avatar" style="background-color: #2196f3; color: #ffffff"
                      :style="{
                        background: bgColor[slotProps.data.stt % 7],
                      }" class="mr-2" size="xlarge" shape="circle" />
                    <div class="flex flex-column flex-grow-1">
                      <Button class="p-button-text p-0" style="color: inherit; padding: 0 !important"
                        @click="editUser(slotProps.data)">
                        <h3 class="mb-1 mt-0">
                          {{ slotProps.data.full_name }}
                        </h3>
                      </Button>
                      <i style="font-size: 10pt; color: #999">{{ slotProps.data.user_id }}
                        {{ slotProps.data.phone ? "| " + slotProps.data.phone : "" }}</i>
                      <i style="font-size: 10pt; color: #999">{{
                        slotProps.data.email
                      }}</i>
                    </div>
                    <Chip @click="goDonvi(slotProps.data)" class="ml-2 mr-2 chippb"
                      :label="slotProps.data.organization_name">
                    </Chip>
                    <Chip v-if="slotProps.data.position_name" @click="goChucvu(slotProps.data)" class="ml-2 mr-2 chip2"
                      v-bind:label="slotProps.data.position_name" />
                    <Chip @click="goRole(slotProps.data)" class="ml-2 mr-2" :style="{
                      background: slotProps.data.background_color,
                      color: slotProps.data.text_color,
                    }" v-bind:label="slotProps.data.role_name" />
                    <div v-bind:class="'rolefalse'" style="background-color: #eee; font-size: 10pt">
                      {{
                        moment(new Date(slotProps.data.created_date)).format(
                          "DD/MM/YYYY"
                        )
                      }}
                    </div>
                    <Button @click="gostatus(slotProps.data)" v-bind:label="slotProps.data.tenstatus" v-bind:class="
                      'ml-2 mr-2 p-button p-button-' +
                      (slotProps.data.status == 0
                        ? 'danger'
                        : slotProps.data.status == 1
                          ? 'success'
                          : 'warning') +
                      ' p-button-rounded'
                    " />
                    <Button icon="pi pi-ellipsis-h" class="p-button-outlined p-button-secondary ml-2"
                      @click="toggleMores($event, slotProps.data)" aria-haspopup="true" aria-controls="overlay_More" />
                  </div>
                </div>
              </template>
              <template #empty>
                <div class="align-items-center justify-content-center p-4 text-center" v-if="!isFirst">
                  <img src="../../assets/background/nodata.png" height="144" />
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </template>
            </DataView>
            </div>
          </SplitterPanel>
        </Splitter>
      </div>

    </div>

  </div>
  <Menu id="overlay_More" ref="menuButMores" :model="itemButMores" :popup="true" />
  <Dialog header="Cập nhật người dùng" v-model:visible="displayAddUser" :style="{ width: '55vw' }" :maximizable="true"
    :closable="true">
    <form @submit.prevent="handleSubmit(!v$.$invalid)" name="submitform">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tên đăng nhập <span class="redsao">(*)</span></label>
          <InputText spellcheck="false" v-bind:disabled="!isAdd" class="col-10 ip32" v-model="user.user_id"
            :class="{ 'p-invalid': v$.user_id.$invalid && submitted }" />
        </div>
        <small v-if="
          (v$.user_id.required.$invalid && submitted) ||
          v$.user_id.required.$pending.$response
        " class="col-10 p-error">
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.user_id.required.$message
                .replace("Value", "Tên đăng nhập")
                .replace("is required", "không được để trống")
            }}</span>
          </div>
        </small>
        <small v-if="
          (v$.user_id.maxLength.$invalid && submitted) ||
          v$.user_id.maxLength.$pending.$response
        " class="col-10 p-error">
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.user_id.maxLength.$message.replace(
                "The maximum length allowed is",
                "Tên đăng nhập không được vượt quá"
              )
            }}
              ký tự</span>
          </div>
        </small>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Họ tên <span class="redsao">(*)</span></label>
          <InputText spellcheck="false" class="col-10 ip32 text-transform" v-model="user.full_name"
            autocomplete="username" name="full_name" :class="{ 'p-invalid': v$.full_name.$invalid && submitted }" />
        </div>
        <small v-if="
          (v$.full_name.required.$invalid && submitted) ||
          v$.full_name.required.$pending.$response
        " class="col-10 p-error">
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.full_name.required.$message
                .replace("Value", "Tên người dùng")
                .replace("is required", "không được để trống")
            }}</span>
          </div>
        </small>
        <small v-if="
          (v$.full_name.maxLength.$invalid && submitted) ||
          v$.full_name.maxLength.$pending.$response
        " class="col-10 p-error">
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.full_name.maxLength.$message.replace(
                "The maximum length allowed is",
                "Tên người dùng không được vượt quá"
              )
            }}
              ký tự</span>
          </div>
        </small>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Mật khẩu <span class="redsao">(*)</span></label>
          <Password style="cursor: pointer; width: 33.33%" v-model="user.is_psword" autocomplete="new-password"
            class="ip32" toggleMask>
            <template #header>
              <h6>Chọn mật khẩu</h6>
            </template>
            <template #footer="sp">
              {{ sp.level }}
              <Divider />
              <p class="mt-2">Gợi ý</p>
              <ul class="pl-2 ml-2 mt-0" style="line-height: 1.5">
                <li>Có ít nhất 1 chữ thường</li>
                <li>Có ít nhất 1 chữ hoa</li>
                <li>Có ít nhất 1 ký tự số</li>
                <li>Tối thiểu 8 ký tự</li>
              </ul>
            </template>
          </Password>
        </div>
        <small v-if="
          (v$.is_psword.required.$invalid && submitted) ||
          v$.is_psword.required.$pending.$response
        " class="col-10 p-error">
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.is_psword.required.$message
                .replace("Value", "Mật khẩu")
                .replace("is required", "không được để trống")
            }}</span>
          </div>
        </small>
        <small v-if="v$.is_psword.minLength.$invalid && submitted" class="col-10 p-error">
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.is_psword.minLength.$message
                .replace(
                  "This field should be at least",
                  "Mật khẩu không được ít hơn"
                )
                .replace("long", "ký tự")
            }}</span>
          </div>
        </small>
        <small v-if="v$.is_psword.maxLength.$invalid && submitted" class="col-10 p-error">
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.is_psword.maxLength.$message.replace(
                "The maximum length allowed is",
                "Mật khẩu không được vượt quá"
              )
            }}
              ký tự</span>
          </div>
        </small>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Đơn vị</label>
          <TreeSelect class="col-10 ip32" v-model="selectCapcha" :options="treedonvis" :showClear="true" :max-height="200"
            placeholder="" optionLabel="data.organization_name" optionValue="data.department_id">
          </TreeSelect>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Nhóm</label>
          <Dropdown class="col-4 ip32" v-model="user.role_id" :options="tdRoles" optionLabel="role_name"
            optionValue="role_id" placeholder="Chọn nhóm" />
          <label class="col-2 text-left pl-7">Chức vụ</label>
          <Dropdown class="col-4 ip32" v-model="user.position_id" :options="chucvus" optionLabel="position_name"
            optionValue="position_id" placeholder="Chọn chức vụ" />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Số CA</label>
          <InputNumber class="col-4 ip32 p-0" v-model="user.ca_number" :useGrouping="false" />
          <label class="col-2 text-left pl-7">Ngày sinh</label>
          <Calendar class="col-4 ip32" id="icon" v-model="user.birthday" :showIcon="true" />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Điện thoại</label>
          <InputText class="col-4 ip32" spellcheck="false" v-model="user.phone" />
          <label class="col-2 text-left pl-7">Email</label>
          <InputText class="col-4 ip32" spellcheck="false" v-model="user.email" />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Trạng thái</label>
          <Dropdown class="col-4 ip32" v-model="user.status" :options="tdstatuss" optionLabel="text" optionValue="value"
            placeholder="Chọn trạng thái" />
          <label class="col-2 text-left pl-7">Giới tính</label>
          <Dropdown class="col-4 ip32" v-model="user.gender" :options="genders" optionLabel="text" optionValue="value"
            placeholder="Chọn giới tính" />
        </div>
        <label class="col-2"></label>
        <div class="col-3">
          <div class="field">
            <label class="col-12 text-center">Ảnh đại diện</label>
            <div class="inputanh relative" style="margin: 0 auto">
              <img @click="chonanh('AnhUser')" id="Anhdaidien" v-bind:src="
                user.avatar
                  ? basedomainURL + user.avatar
                  : basedomainURL + '/Portals/Image/noimg.jpg'
              " />
              <Button v-if="user.avatar || isDisplayAvt" style="width: 1.5rem; height: 1.5rem" icon="pi pi-times"
                @click="delImg('Anhdaidien')" class="p-button-rounded absolute top-0 right-0 cursor-pointer" />
              <input class="ipnone" id="AnhUser" type="file" accept="image/*"
                @change="handleFileUpload($event, 'Anhdaidien')" />
            </div>
          </div>
        </div>
        <div class="col-3">
          <div class="field">
            <label class="col-12 text-center">Chữ ký</label>
            <div class="inputanh relative" style="margin: 0 auto">
              <img @click="chonanh('UserChuky')" id="Chuky" v-bind:src="
                user.signature
                  ? basedomainURL + user.signature
                  : basedomainURL + '/Portals/Image/noimg.jpg'
              " />
              <Button v-if="user.signature || isChuky" style="width: 1.5rem; height: 1.5rem" icon="pi pi-times"
                @click="delImg('Chuky')" class="p-button-rounded absolute top-0 right-0 cursor-pointer" />
              <input class="ipnone" id="UserChuky" type="file" accept="image/*"
                @change="handleFileUpload($event, 'Chuky')" />
            </div>
          </div>
        </div>
        <div class="col-3">
          <div class="field">
            <label class="col-12 text-center">Chữ ký nháy</label>
            <div class="inputanh relative" style="margin: 0 auto">
              <img @click="chonanh('UserKynhay')" id="Kynhay" v-bind:src="
                user.flash_signature
                  ? basedomainURL + user.flash_signature
                  : basedomainURL + '/Portals/Image/noimg.jpg'
              " />
              <Button v-if="user.flash_signature || isKynhay" style="width: 1.5rem; height: 1.5rem" icon="pi pi-times"
                @click="delImg('Kynhay')" class="p-button-rounded absolute top-0 right-0 cursor-pointer" />
              <input class="ipnone" id="UserKynhay" type="file" accept="image/*"
                @change="handleFileUpload($event, 'Kynhay')" />
            </div>
          </div>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-1 ip32 p-0" v-model="user.is_order" />
          <label class="col-1"></label>
          <label class="col-2 text-right">Admin</label>
          <InputSwitch class="col-1" v-model="user.is_admin" />
          <label class="col-1" v-if="user.is_super"></label>
          <label class="col-2 text-right" v-if="user.is_super">Is Super</label>
          <InputSwitch v-model="user.is_super" v-if="user.is_super" />
        </div>
        <Accordion class="w-full p-2">
          <!-- 1. Thông tin chung -->
          <AccordionTab>
            <template #header>
              <span>Quyền Module</span>
            </template>
            <div class="field col-12 md:col-12">
              <label class="text-left" style="width: 20%; padding: 0.5rem">Hiển thị sinh nhật</label>
              <InputSwitch class="col-1" v-model="user.display_birthday" />
              <label class="col-1"></label>
              <label class="col-2 text-right">Ban hành</label>
              <InputSwitch class="col-1" v-model="user.calendar_enact" />
              <label class="col-1"></label>
              <label class="col-2 text-right">Tổng hợp CV</label>
              <InputSwitch class="col-1" v-model="user.is_task" />
            </div>
          </AccordionTab>
        </Accordion>

      </div>
    </form>
    <template #footer>
      <Button label="Huỷ" icon="pi pi-times" @click="closedisplayAddUser" class="p-button-raised p-button-secondary" />
      <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
    </template>
  </Dialog>

  <Dialog header="Phân quyền cho người dùng" v-model:visible="displayConfigRole" :style="{ width: '1150px' }"
    :maximizable="true" :autoZIndex="true">
    <TreeTable :value="modules" :loading="opition.moduleloading" :showGridlines="true" filterMode="lenient"
      class="p-treetable-sm e-sm" :paginator="modules && modules.length > 20" :rows="20" :scrollable="true"
      scrollHeight="flex" :rowHover="true">
      <template #header>

        <Toolbar class="w-full custoolbar">
          <template #start>
            <h3 class="module-title mt-0 ml-1 mb-2">
              <i class="pi pi-microsoft"></i> Module chức năng
              <span class="text-base font-normal" style="color: #495057">(Module đang được phân theo
                <span class="font-bold" v-if="!is_role">cá nhân</span>
                <span v-else>nhóm người dùng
                  <span class="font-bold"> {{ role_name }}</span></span>
                )
              </span>
            </h3>
          </template>
          <template #end>
            <Button v-if="!check_role" label="Phân theo nhóm người dùng" icon="pi pi-cloud-download"
              class="p-button-raised p-button-secondary mr-2" @click="changePermission"
              style="font-size: 1rem !important; height: 100% !important" />
            <Button v-if="!different_module_move" class="mr-2 p-button-outlined p-button-secondary" label="Sao chép"
              icon="pi pi-copy" @click="coppyModule()" :disabled="is_coppy_module ? true : false" />
            <Button v-if="is_coppy_module && different_module_move" label="Dán" icon="pi pi-copy" class=""
              @click="pasteModule()" />
          </template>
        </Toolbar>
      </template>
      <Column field="Icon" header="" class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px" bodyStyle="text-align:center;max-width:100px">
        <template #body="md">
          <i v-bind:class="md.node.data.Icon"></i>
        </template>
      </Column>
      <Column field="module_name" header="Tên Menu" :sortable="true" :expander="true">
      </Column>
      <Column field="module_id" header="Mã Quyền" class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px" bodyStyle="max-width:150px">
        <!-- <template #body="md">
            <b v-if="md.node.data.is_permission">{{
              md.node.data.is_permission.join("")
            }}</b>
          </template> -->
      </Column>
      <Column headerClass="align-items-center justify-content-center text-center" header="Quyền"
        headerStyle="text-align:center;width:250px" bodyStyle="text-align:center;width:250px">
        <template #body="md">
          <MultiSelect v-model="md.node.data.is_permission" @change="changeQuyen(md.node)" :style="{ width: '250px' }"
            id="overlay_Quyen" ref="menuQuyen" :popup="true" :options="tdQuyens" optionLabel="text" optionValue="value"
            placeholder="Chọn quyền" />
        </template>
      </Column>
      <Column headerClass="align-items-center justify-content-center text-center" header="Quyền Module"
        headerStyle="text-align:center;width:250px" bodyStyle="text-align:center;width:250px">
        <template #body="md">
          <MultiSelect v-if="md.node.data.module_key" v-model="md.node.data.module_functions"
            @change="changeModuleFunctions(md.node)" :style="{ width: '250px' }" :popup="true" :options="
              function_modules.filter(
                (x) => x.module_key === md.node.data.module_key
              )
            " optionLabel="function_name" optionValue="function_id" placeholder="Chọn quyền module" :filter="true" />
        </template>
      </Column>
    </TreeTable>
    <template #footer>
      <Button label="Huỷ" icon="pi pi-times" @click="closedisplayConfigRole" class="p-button-raised p-button-secondary" />
      <Button label="Cập nhật" icon="pi pi-save" @click="addConfigRole" />
    </template>
  </Dialog>
  <Dialog header="Tải lên file Excel" v-model:visible="displayImport" :style="{ width: '40vw' }" :closable="true">
    <h3>
      <label>
        <a :href="basedomainURL + item" download>Nhấn vào đây</a>
        để tải xuống tệp mẫu.
      </label>
    </h3>
    <form>
      <FileUpload accept=".xls,.xlsx" @remove="removeFile" @select="selectFile" :show-upload-button="false"
        choose-label="Chọn tệp" cancel-label="Hủy" :fileLimit="1"
        :invalidFileTypeMessage="'Chỉ chấp nhận file dạng .xsl,.xlsx,.slsm,.csv'">
        <template #empty>
          <p>Kéo và thả tệp vào đây để tải lên.</p>
        </template>
      </FileUpload>
    </form>
    <template #footer>
      <Button label="Lưu" icon="pi pi-check" @click="Upload" />
    </template>
  </Dialog>
</template>
<style>
.row-active {
  color: rgb(13, 137, 236);
}
.p-treeselect-panel .p-treeselect-items-wrapper .p-tree {
  max-height: 400px;
  max-width: 500px;
}
</style>
<style scoped>
.text-transform {
  text-transform: capitalize;
}

.ip32 {
  height: 32px;
}

.dot-status {
  height: unset !important;
  padding: 0.08rem 0.54rem !important;
  font-size: 10px !important;
  position: absolute;
  right: calc(50% - 38px);
  bottom: calc(50% - 46px);
}

.icon-quyen {
  position: absolute;
  top: 10%;
  right: calc(25% - 20px);
  color: red;
  font-size: 19px;
}

.p-avatar {
  font-size: 2rem !important;
}

.resize-btn {
  font-size: 20px !important;
}

.p-chip.custom-chip {
  background: var(--primary-color);
  color: var(--primary-color-text);
  font-size: 00.875rem;
}

.p-chip.chip-success,
.chip1 {
  background: #689f38;
  color: #fff;
}

.p-chip.chip-danger {
  background: #d32f2f;
}

.p-chip.chip-warning,
.chip2 {
  background: #fbc02d;
  color: #fff;
}

.chippb,
.chip0 {
  background-color: #4285f4;
  color: #fff;
  font-size: 1rem;
}

.ipnone {
  display: none;
}

.inputanh {
  /* border: 1px solid #ccc; */
  width: 96px;
  height: 96px;
  cursor: pointer;
  padding: 1px;
}

.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}

.roletrue {
  background-color: orange;
  padding: 5px 10px;
  margin: 5px auto;
  width: max-content;
  border-radius: 5px;
  margin-bottom: 10px;
  height: fit-content;
  color: #fff;
  font-size: 0.875rem;
}

.rolefalse {
  background-color: #eee;
  padding: 5px 10px;
  margin: 5px auto;
  width: max-content;
  border-radius: 5px;
  margin-bottom: 10px;
  height: fit-content;
  font-size: 0.875rem;
}

.card-content>.no-paddcontent {
  height: 100% !important;
}

.p-calendar-w-btn {
  padding: 0px !important;
}

.toolbar-filter {
  border: unset;
  outline: unset;
  background-color: #fff;
  padding-bottom: 0px;
}
.d-lang-table {
    margin: 0px;
    height: calc(100vh - 125px);
}
.d-lang-table-r {
    margin: 0px;
    height: calc(100vh - 64px);
}
</style>
<style lang="scss" scoped>
::v-deep(.p-treetable-tbody) {
  tr {
    cursor: pointer;
  }
}

::v-deep(.col-12) {
  .p-inputswitch {
    top: 6px;
  }
}

::v-deep(.p-chip) {
  .p-chip-text {
    cursor: pointer;
  }
}

::v-deep(.p-dataview) {
  .p-dataview-content {
    background: #eee;
  }
}

::v-deep(.p-password) {
  .p-password-input {
    width: 100%;
    height: 32px;
  }
}

::v-deep(.p-treeselect-panel) {
  .p-treeselect-panel .p-treeselect-items-wrapper {
    max-height: 200px !important;
    max-width: 300px !important;
  }
}
</style>
