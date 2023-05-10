<script setup>
import { ref, inject, onMounted } from "vue";
import { required, maxLength } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { encr, getParent } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
//init Model
const role = ref({
  role_name: "",
  is_order: 1,
  status: true,
});
const store = inject("store");
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const isDynamicSQL = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
//Valid Form
const submitted = ref(false);
const rules = {
  role_id: {
    required,
    maxLength: maxLength(50),
    $errors: [
      {
        $property: "role_id",
        $validator: "required",
        $message: "Mã nhóm không được để trống!",
      },
    ],
  },
  role_name: {
    required,
    maxLength: maxLength(250),
    $errors: [
      {
        $property: "role_name",
        $validator: "required",
        $message: "Tên Nhóm không được để trống!",
      },
    ],
  },
};
const rules_coppy = {
  role_id: {
    required,
    maxLength: maxLength(50),
    $errors: [
      {
        $property: "role_id",
        $validator: "required",
        $message: "Mã nhóm không được để trống!",
      },
    ],
  },
  role_name: {
    required,
    maxLength: maxLength(250),
    $errors: [
      {
        $property: "role_name",
        $validator: "required",
        $message: "Tên Nhóm không được để trống!",
      },
    ],
  },
};
const data_coppy_module = ref({});
const v$ = useVuelidate(rules, role);
const t$ = useVuelidate(rules_coppy, data_coppy_module);
//Khai báo biến
const dialogCoppy = ref(false);
const selectCapcha = ref();
selectCapcha.value = {};
selectCapcha.value[store.getters.user.organization_id] = true;
const treedonvis = ref();
const is_coppy_module = ref(false);
const data_coppy = ref();
const different_module_move = ref(false);
const role_id_check = ref();
const id_temp = ref();
const options = ref({
  IsNext: true,
  sort: "is_order asc",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
});
const filterSQL = ref([]);
const first = ref(1);
const isAdd = ref(true);
const selectedNodes = ref([]);
const filters = ref({});
const opition = ref({ search: "" });
const roles = ref();
const treeroles = ref();
const displayAddRole = ref(false);
const isFirst = ref(true);
let files = [];

const tdQuyens = [
  { value: 0, text: "Không có quyền (0)" },
  { value: 1, text: "Xem cá nhân (1)" },
  { value: 2, text: "Xem tất cả (2)" },
  { value: 3, text: "Chỉnh sửa cá nhân (3)" },
  { value: 4, text: "Chỉnh sửa tất cả (4)" },
  { value: 5, text: "Duyệt (5)" },
  { value: 6, text: "Full (6)" },
].reverse();
const quyen = ref({});
const menuQuyen = ref();
const itemQuyens = ref([]);
tdQuyens.forEach((element) => {
  itemQuyens.value.push({
    label: element.text,
    icon: "pi pi-key",
    command: (event) => {
      quyen.value.is_permission = element.value;
    },
  });
});
const togglQuyen = (event, q) => {
  quyen.value = q;
  menuQuyen.value.toggle(event);
};
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportRole("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportRole("ExportExcelMau");
    },
  },
]);

//Khai báo function
const function_modules = ref([]);
const initModuleFunctions = () => {
  axios
    .post(
      baseURL + "/api/Roles/GetDataProc",
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
//------------------------
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const handleFileUpload = (event) => {
  files = event.target.files;
};
//Sao chep
const coppyModule = () => {
  is_coppy_module.value = true;
  role_id_check.value = id_temp.value;
  data_coppy.value = JSON.parse(JSON.stringify(modules.value));
  toast.success("Đã sao chép thành công!");
};
const pasteModule = () => {
  modules.value = data_coppy.value;
  toast.success("Dán thành công!");
}
//Phân trang dữ liệu
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
  options.value.PageNo = event.page;
  loadRole(true);
};
//Show Modal
const showModalAddRole = () => {
  isAdd.value = true;
  submitted.value = false;
  role.value = {
    role_name: "",
    SearchText: "",
    is_order: options.value.totalRecords + 1,
    status: true,
  };
  displayAddRole.value = true;
};
const chonanh = (id) => {
  document.getElementById(id).click();
};
const closedisplayAddRole = () => {
  displayAddRole.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  is_coppy_module.value = false;
  different_module_move.value = false;
  role_id_check.value = null;
  selectedNodes.value = [];
  options.value = {
    IsNext: true,
    sort: "is_order desc",
    PageNo: 0,
    PageSize: 20,
    loading: true,
    SearchText: "",
  };
  first.value = 1;
  loadRole(true);
};
const onSearch = () => {
  first.value = 1;
  loadRole(true);
};
const loadDataSQL = () => {
  let data = {
    id: null,
    next: options.value.IsNext,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
    fieldSQLS: filterSQL.value,
  };

  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/FilterRoles", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);

      let data = dt[0];

      if (data.length > 0) {
        roles.value = data;
      } else {
        roles.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;
      //Show Count nếu có
      if (dt.length == 2) {
        options.value.totalRecords = dt[1][0].totalRecords;
      }
    })
    .catch((error) => {
      options.value.loading = false;
      toast.error("Tải dữ liệu không thành công!");
      addLog({
        title: "Lỗi Console loadData",
        controller: "Video.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/Roles/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_roles_count_new",
            par: [
              { par: "user_id", va: store.getters.user.user_id},
              { par: "s", va: options.value.SearchText }
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
        options.value.totalRecords = data[0].totalRecords;
      }
    })
    .catch((error) => {
      swal.fire({
        text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
        confirmButtonText: "OK",
      });
    });
};
const onSort = (event) => {
  options.value.sort =
    event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
  if (event.sortField != "role_id") {
    options.value.sort +=
      ",role_id " + (event.sortOrder == 1 ? " ASC" : " DESC");
  }
  isDynamicSQL.value = true;
  loadRole();
};
const loadRole = (rf) => {
  if (isDynamicSQL.value) {
    loadDataSQL();
    return false;
  }
  if (rf) {
    opition.value.loading = true;
    loadCount();
  }
  axios
    .post(
      baseURL + "/api/Roles/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_roles_list_new",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "pageno", va: options.value.PageNo },
              { par: "pagesize", va: options.value.PageSize },
              { par: "s", va: options.value.SearchText },
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
        roles.value = data;
        roles.value.forEach((item, index) => {
          item.stt = index + 1;
        });
      } else {
        roles.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      if (rf) {
        opition.value.loading = false;
      }
    })
    .catch((error) => {
      if (error && error.status === 401) {
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
const configRole = (md, type) => {
  debugger
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (role_id_check.value && md.role_id != role_id_check.value ) {
    different_module_move.value = true;
  }
  else different_module_move.value = false;
  id_temp.value = md.role_id;
  if(type == 2) different_module_move.value = true;
  //Config quyền
  opition.value.moduleloading = true;
  axios
    .post(
      baseURL + "/api/Roles/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "modulelistbyrole",
            par: [{ par: "role_id ", va: md.role_id }],
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
      if (data.length > 0) {
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
        //renderTree(data);
        let obj = renderTree(data, "module_id", "module_name", "module");
        modules.value = obj.arrChils;
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
const addConfigRole = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let mdmodules = [];
  debugger;
  modules.value.forEach((element) => {
    mdmodules.push({
      role_module_id: element.data.role_module_id || -1,
      role_id: is_coppy_module.value ? id_temp.value : element.data.role_id,
      module_id: element.data.module_id,
      is_grade: element.data.is_grade,
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
          role_id: is_coppy_module.value ? id_temp.value : ec.data.role_id,
          module_id: ec.data.module_id,
          is_grade: ec.data.is_grade,
          is_permission: ec.data.is_permission
            ? ec.data.is_permission.join("")
            : ec.data.is_permission,
          module_functions: ec.data.module_functions
            ? ec.data.module_functions.join()
            : ec.data.module_functions,
          // is_permission: (ec.data.is_permission )
          //   ? ec.data.is_permission.join("")
          //   : element.data.is_permission
          //   ? element.data.is_permission.join("")
          //   : element.data.is_permission,
          //   module_functions: ec.data.module_functions
          //   ? ec.data.module_functions.join()
          //   : element.data.module_functions
          //   ? element.data.module_functions.join()
          //   : element.data.module_functions,
        });
        // get value module level 3 (it module khong can de quy - co time xem lai)
        if (ec.children && ec.children.length > 0) {
          ec.children.forEach((ec2) => {
            mdmodules.push({
              role_module_id: ec2.data.role_module_id || -1,
              role_id: is_coppy_module.value ? id_temp.value : ec2.data.role_id,
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
        toast.success("Phân quyền Role thành công!");
        displayConfigRole.value = false;
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
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const editRole = (md) => {
  isAdd.value = false;
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddRole.value = true;
  axios
    .post(
      baseURL + "/api/Roles/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "Sys_Roles_Get",
            par: [{ par: "role_id", va: md.role_id }],
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
        role.value = data[0][0];
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
const handleSubmit = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (role.value.role_id.length > 50) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng không được nhập mã nhóm người dùng quá 50 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
  }
  if (role.value.role_name.length > 250) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng không được nhập tên nhóm người dùng quá 50 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
  }
  if (store.getters.user.is_super) {
    role.value.is_system = true;
    role.value.is_organization = false;
    role.value.organization_id = null;
    role.value.department_id = null;
  }
  else if (!store.getters.user.is_admin_child) {
    role.value.is_system = false;
    role.value.is_organization = true;
    role.value.organization_id = store.getters.user.organization_child_id;
    role.value.organization_child_id = null;
  }
  else {
    role.value.is_system = false;
    role.value.is_organization = false;
    role.value.organization_id = store.getters.user.organization_id;
    role.value.organization_child_id = store.getters.user.organization_child_id;
  }
  addRole();
};
const addRole = () => {
  debugger;
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
      `/api/Roles/${isAdd.value == false ? "Update_Roles" : "Add_Roles"}`,
    data: role.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật nhóm người dùng thành công!");
        loadRole(true);
        closedisplayAddRole();
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Mã nhóm người dùng đã tồn tại, vui lòng chọn lại!",
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
};
//xóa nhiều
const deleteList = () => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xóa danh sách nhóm người dùng này không!",
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
          .delete(baseURL + "/api/Roles/Del_Roles", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: selectedNodes.value.map((x) => x.role_id),
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá thành công!");
              selectedNodes.value = [];
              loadRole(true);
            } else {
              swal.fire({
                title: "Thông báo!",
                text: "Xóa không thành công, vui lòng thử lại!",
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
const delRole = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá nhóm người dùng này không!",
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
          .delete(baseURL + "/api/Roles/Del_Roles", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data:
              md != null
                ? [md.role_id]
                : selectedNodes.value.map((x) => x.role_id),
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá Role thành công!");
              loadRole();
              if (!md) selectedNodes.value = [];
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

const upstatusRole = (md) => {
  let ids = [md.role_id];
  let tts = [md.status];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/Roles/Update_statusRoles",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái Role thành công!");
        loadRole();
        if (!md) selectedNodes.value = [];
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
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};

const exportRole = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "Sys_Modules" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH NHÓM NGƯỜI DÙNG",
        proc: "Sys_Roles_ListExport",
        par: par,
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
          let pathReplace = response.data.path.replace(/\\+/g, '/').replace(/\/+/g, '/').replace(/^\//g, '');
          var listPath = pathReplace.split('/');
          var pathFile = "";
          listPath.forEach(item => {
            if (item.trim() != "") {
              pathFile += "/" + item;
            }
          });
          window.open(baseURL + pathFile);
        }
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
      if (error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const opColor = ref();
let pcolor = "";
const toggleColor = (event, pc) => {
  opColor.value.toggle(event);
  pcolor = pc;
};
const changeColor = (color) => {
  role.value[pcolor] = color.hex;
  if (!color.hex.includes("#")) opColor.value.hide();
};
const coppyRole = (id) => {
  data_coppy_module.value = {
    role_id: null,
    role_name: null,
    id_coppy: id,
    is_system: store.getters.user.is_super || false,
    is_organization: !store.getters.user.is_admin_child || false,
    organization_id: store.getters.user.organization_id || false,
    organization_child_id: store.getters.user.organization_child_id || null,
  }
  submitted.value = false;
  dialogCoppy.value = true;
}
const saveCoppyCofig = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  let formData = new FormData();
  formData.append("model", JSON.stringify(data_coppy_module.value));
  formData.append("id_coppy", JSON.stringify(data_coppy_module.value.id_coppy));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "post",
    url:
      baseURL +
      `/api/Roles/Coppy_Role`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Thêm mới nhóm người dùng thành công!");
        dialogCoppy.value = false;
        loadRole(true);
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
}
const initTudien = () => {
  axios
    .post(
      baseURL + "/api/Roles/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_roles_list_dictionary",
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
        let obj = renderTree(
          data[0],
          "organization_id",
          "organization_name",
          "phòng ban"
        );
        treedonvis.value = obj.arrtreeChils;
      } else {
        treedonvis.value = [];
      }
    })
    .catch((error) => { });
};
onMounted(() => {
  //init
  loadRole(true);
  initTudien();
  initModuleFunctions();
  return {};
});
</script>
<template>
  <div class="main-layout flex-grow-1 p-2" v-if="store.getters.islogin">
    <DataTable class="w-full p-datatable-sm" :value="roles" :paginator="true" :loading="opition.loading" dataKey="role_id"
      :showGridlines="true" v-model:selection="selectedNodes" @page="onPage($event)" @filter="onFilter($event)"
      @sort="onSort($event)" :filters="filters" :rows="options.PageSize" :rowHover="true" filterMode="lenient"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]" :totalRecords="options.totalRecords" responsiveLayout="scroll"
      :scrollable="true" scrollHeight="flex" :lazy="true" v-model:first="first">
      <template #header>
        <h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
          <i class="pi pi-list"></i> Nhóm người dùng
          <span v-if="options.totalRecords">({{ options.totalRecords }})</span>
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText type="text" spellcheck="false" v-model="options.SearchText" placeholder="Tìm kiếm"
                v-on:keyup.enter="onSearch" />
            </span>
          </template>

          <template #end>
            <Button label="Thêm nhóm người dùng" icon="pi pi-plus" class="mr-2" @click="showModalAddRole" />
            <Button class="mr-2 p-button-outlined p-button-secondary" icon="pi pi-refresh" @click="onRefersh" />
            <Button label="Xoá" icon="pi pi-trash" class="mr-2 p-button-danger" v-if="selectedNodes.length > 0"
              @click="deleteList()" />
            <Button label="Export" icon="pi pi-file-excel" class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport" aria-haspopup="true" aria-controls="overlay_Export" />
            <Menu id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" />
          </template>
        </Toolbar>
      </template>
      <Column selectionMode="multiple" headerStyle="text-align:center;max-width:50px"
        bodyStyle="text-align:center;max-width:50px" class="align-items-center justify-content-center text-center">
      </Column>
      <Column field="stt" header="STT" class="align-items-center justify-content-center text-center font-bold"
        headerStyle="text-align:center;max-width:100px" bodyStyle="text-align:center;max-width:100px"></Column>
      <Column field="role_id" header="Mã Nhóm" class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px" bodyStyle="text-align:center;max-width:150px">
        <template #body="md">
          <span v-bind:class="'status' + md.data.status">{{
            md.data.role_id
          }}</span>
        </template>
      </Column>
      <Column field="role_name" header="Tên Nhóm">
        <template #body="md">
          <Chip :style="{
            background: md.data.background_color,
            color: md.data.text_color,
          }" v-bind:label="md.data.role_name" class="mr-2 mb-2" />
        </template>
      </Column>
      <Column field="organization_from" header="Đơn vị/Phòng ban"
        class="align-items-center justify-content-center text-center" headerStyle="text-align:center;max-width:250px"
        bodyStyle="text-align:center;max-width:250px">
      </Column>
      <Column field="status" header="Trạng thái" class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px" bodyStyle="text-align:center;max-width:120px">
        <template #body="md">
          <Checkbox v-model="md.data.status" :binary="true" @change="upstatusRole(md.data)" />
        </template>
      </Column>
      <Column header="Chức năng" class="align-items-center justify-content-center text-center" headerClass="text-center"
        bodyClass="text-center" headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px">
        <template #header> </template>
        <template #body="md">
          <div v-if="(md.data.is_system && store.getters.user.is_super)
            || (md.data.is_organization && !!store.getters.user.is_admin_child)
            || (!md.data.is_system && !md.data.is_organization && (md.data.organization_child_id == store.getters.user.organization_child_id))
          ">
            <Button type="button" icon="pi pi-key" class="p-button-rounded p-button-secondary p-button-outlined"
              style="margin-right: 0.5rem" v-tooltip.top="'Phân quyền'" @click="configRole(md.data,1)"></Button>
            <Button type="button" icon="pi pi-pencil" class="p-button-rounded p-button-secondary p-button-outlined"
              style="margin-right: 0.5rem" v-tooltip.top="'Sửa'" @click="editRole(md.data)"></Button>
            <Button type="button" icon="pi pi-trash" v-tooltip.top="'Xóa'"
              class="p-button-rounded p-button-secondary p-button-outlined" @click="delRole(md.data)"></Button>
          </div>
          <div v-else>
            <Button type="button" icon="pi pi-key" class="p-button-rounded p-button-secondary p-button-outlined"
              style="margin-right: 0.5rem" v-tooltip.top="'Xem phân quyền'" @click="configRole(md.data,2)"></Button>
            <Button type="button" icon="pi pi-copy" class="p-button-rounded p-button-secondary p-button-outlined"
              style="margin-right: 0.5rem" v-tooltip.top="'Sao chép phân quyền'"
              @click="coppyRole(md.data.role_id)"></Button>
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="
              align-items-center
              justify-content-center
              p-4
              text-center
              m-auto
            " v-if="!isFirst">
          <img src="../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
  </div>
  <Dialog header="Cập nhật nhóm người dùng" v-model:visible="displayAddRole" :style="{ width: '42vw', zIndex: 2 }"
    :maximizable="true" :autoZIndex="false">
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-4 text-left">Mã nhóm <span class="redsao">(*)</span></label>
          <InputText spellcheck="false" v-bind:disabled="!isAdd" class="col-8 ip36" v-model="role.role_id"
            :class="{ 'p-invalid': v$.role_id.$invalid && submitted }" />
        </div>
        <small v-if="
          (v$.role_id.required.$invalid && submitted) ||
          v$.role_id.required.$pending.$response
        " class="col-12 p-error">
          <div class="field col-12 md:col-12">
            <label class="col-4 text-left"></label>
            <span class="col-8">{{
              v$.role_id.required.$message
                .replace("Value", "Mã nhóm")
                .replace("is required", "không được để trống")
            }}</span>
          </div>
        </small>
        <small v-if="v$.role_id.maxLength.$invalid && submitted" class="col-12 p-error">
          <div class="field col-12 md:col-12">
            <label class="col-4 text-left"></label>
            <span class="col-8">{{
              v$.role_id.maxLength.$message.replace(
                "The maximum length allowed is",
                "Mã nhóm không được vượt quá"
              )
            }}
              ký tự</span>
          </div>
        </small>
        <div class="field col-12 md:col-12">
          <label class="col-4 text-left">Tên nhóm <span class="redsao">(*)</span></label>
          <InputText spellcheck="false" class="col-8 ip36" v-model="role.role_name"
            :class="{ 'p-invalid': v$.role_name.$invalid && submitted }" />
        </div>
        <small v-if="
          (v$.role_name.required.$invalid && submitted) ||
          v$.role_name.required.$pending.$response
        " class="col-12 p-error">
          <div class="field col-12 md:col-12">
            <label class="col-4 text-left"></label>
            <span class="col-8">{{
              v$.role_name.required.$message
                .replace("Value", "Tên nhóm")
                .replace("is required", "không được để trống")
            }}</span>
          </div>
        </small>
        <small v-if="v$.role_name.maxLength.$invalid && submitted" class="col-12 p-error">
          <div class="field col-12 md:col-12">
            <label class="col-4 text-left"></label>
            <span class="col-8">{{
              v$.role_name.maxLength.$message.replace(
                "The maximum length allowed is",
                "Tên nhóm không được vượt quá"
              )
            }}
              ký tự</span>
          </div>
        </small>
        <!-- <div class="field col-12 md:col-12" v-if="!store.getters.user.is_admin_child">
            <label class="col-4 text-left">Phòng ban</label>
            <TreeSelect
              class="col-8 ip32"
              v-model="selectCapcha"
              :options="treedonvis"
              :showClear="true"
              :max-height="200"
              placeholder=""
              optionLabel="data.organization_name"
              optionValue="data.department_id"
            >
            </TreeSelect>
          </div> -->
        <div class="field col-12 md:col-12">
          <label class="col-4 text-left">Màu chữ</label>
          <Button class="p-button-rounded p-button-outlined p-button-secondary col-2" :style="{
            backgroundColor: role.text_color,
            color: role.text_color ? 'transparent' : '#333',
            border: '1px solid #ccc',
          }" type="button" icon="pi pi-palette" @click="toggleColor($event, 'text_color')" />
          <OverlayPanel ref="opColor">
            <ColorPicker theme="dark" @changeColor="changeColor" :sucker-hide="true" />
          </OverlayPanel>
          <label class="col-4 text-right">Màu nền</label>
          <Button class="p-button-rounded p-button-outlined p-button-secondary col-2" :style="{
            backgroundColor: role.background_color,
            color: role.background_color ? 'transparent' : '#333',
            border: '1px solid #ccc',
          }" type="button" icon="pi pi-palette" @click="toggleColor($event, 'background_color')" />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-4 text-left">Thứ tự</label>
          <InputNumber class="col-2 ip36 p-0" v-model="role.is_order" />
        </div>
        <div class="field col-12 md:col-12">
          <label style="vertical-align: text-bottom" class="col-4 text-left">Trạng thái</label>
          <InputSwitch v-model="role.status" class="mt-1" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Huỷ" icon="pi pi-times" @click="closedisplayAddRole" class="p-button-raised p-button-secondary" />
      <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
    </template>
  </Dialog>
  <Dialog header="Coppy quyền nhóm người dùng" v-model:visible="dialogCoppy" :style="{ width: '42vw', zIndex: 2 }"
    :maximizable="true" :autoZIndex="false">
    <form @submit.prevent="saveCoppyCofig(!t$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-4 text-left">Nhập mã nhóm quyền mới <span class="redsao">(*)</span></label>
          <InputText spellcheck="false" class="col-8 ip36" v-model="data_coppy_module.role_id" 
          :class="{ 'p-invalid': t$.role_id.$invalid && submitted }" />
        </div>
        <small v-if="
          (t$.role_id.required.$invalid && submitted) ||
          t$.role_id.required.$pending.$response
        " class="col-12 p-error">
          <div class="field col-12 md:col-12">
            <label class="col-4 text-left"></label>
            <span class="col-8">{{
              t$.role_id.required.$message
                .replace("Value", "Mã nhóm")
                .replace("is required", "không được để trống")
            }}</span>
          </div>
        </small>
        <small v-if="t$.role_id.maxLength.$invalid && submitted" class="col-12 p-error">
          <div class="field col-12 md:col-12">
            <label class="col-4 text-left"></label>
            <span class="col-8">{{
              t$.role_id.maxLength.$message.replace(
                "The maximum length allowed is",
                "Mã nhóm không được vượt quá"
              )
            }}
              ký tự</span>
          </div>
        </small>
        <div class="field col-12 md:col-12">
          <label class="col-4 text-left">Nhập tên nhóm quyền mới <span class="redsao">(*)</span></label>
          <InputText spellcheck="false" class="col-8 ip36" v-model="data_coppy_module.role_name"
          :class="{ 'p-invalid': t$.role_id.$invalid && submitted }" />
        </div>
        <small v-if="
          (t$.role_name.required.$invalid && submitted) ||
          t$.role_name.required.$pending.$response
        " class="col-12 p-error">
          <div class="field col-12 md:col-12">
            <label class="col-4 text-left"></label>
            <span class="col-8">{{
              t$.role_name.required.$message
                .replace("Value", "Tên nhóm")
                .replace("is required", "không được để trống")
            }}</span>
          </div>
        </small>
        <small v-if="t$.role_name.maxLength.$invalid && submitted" class="col-12 p-error">
          <div class="field col-12 md:col-12">
            <label class="col-4 text-left"></label>
            <span class="col-8">{{
              t$.role_name.maxLength.$message.replace(
                "The maximum length allowed is",
                "Tên nhóm không được vượt quá"
              )
            }}
              ký tự</span>
          </div>
        </small>
      </div>
    </form>
    <template #footer>
      <Button label="Huỷ" icon="pi pi-times" @click="dialogCoppy = false" class="p-button-raised p-button-secondary" />
      <Button label="Lưu" icon="pi pi-save" @click="saveCoppyCofig(!t$.$invalid)" />
    </template>
  </Dialog>
  <Dialog header="Phân quyền cho nhóm người dùng" v-model:visible="displayConfigRole" :style="{ width: '1150px' }"
    :maximizable="true" :autoZIndex="true">
    <TreeTable :value="modules" :loading="opition.moduleloading" :showGridlines="true" filterMode="lenient"
      class="p-treetable-sm" :paginator="modules && modules.length > 20" :rows="20" :scrollable="true"
      scrollHeight="flex">
      <template #header>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <h3 class="module-title mt-0 ml-1 mb-2">
              <i class="pi pi-microsoft"></i> Module chức năng
            </h3>
          </template>
          <template #end>
            <Button v-if="!different_module_move" class="mr-2 p-button-outlined p-button-secondary" label="Sao chép"
              icon="pi pi-copy" @click="coppyModule()" :disabled="is_coppy_module ? true : false" />
            <Button v-if="is_coppy_module && different_module_move" label="Dán" icon="pi pi-copy" class="mr-2"
              @click="pasteModule()" />
          </template>
        </Toolbar>
      </template>
      <Column field="Icon" header="" class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px" bodyStyle="text-align:center;max-width:100px">
        <template #body="md">
          <i v-bind:class="md.node.data.icon"></i>
        </template>
      </Column>
      <Column field="module_name" header="Tên Menu" :sortable="true" :expander="true">
      </Column>
      <Column field="module_id" header="Mã Quyền" class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:200px" bodyStyle="text-align:center;max-width:200px">
        <!-- <template #body="md">
            <b v-if="md.node.data.is_permission">{{
              md.node.data.is_permission.join("")
            }}</b>
          </template> -->
      </Column>
      <Column headerClass="align-items-center justify-content-center text-center" header="Quyền"
        headerStyle="text-align:center;width:250px" bodyStyle="text-align:center;width:250px">
        <template #body="md">
          <MultiSelect v-if="md.node.data.permission" v-model="md.node.data.is_permission" @change="changeQuyen(md.node)" :style="{ width: '250px' }"
            id="overlay_Quyen" ref="menuQuyen" :popup="true" :options="tdQuyens.filter(x => md.node.data.permission.toString().split('').includes(x.value.toString()))" optionLabel="text" optionValue="value"
            placeholder="Chọn quyền" />
        </template>
      </Column>
      <Column headerClass="align-items-center justify-content-center text-center" header="Quyền Module"
        headerStyle="text-align:center;width:250px" bodyStyle="text-align:center;width:250px">
        <template #body="md">
          <MultiSelect v-if="md.node.data.module_key && function_modules.filter(
            (x) => x.module_key === md.node.data.module_key
          ).length > 0" v-model="md.node.data.module_functions" @change="changeModuleFunctions(md.node)"
            :style="{ width: '250px' }" :popup="true" :options="
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
</template>
<style scoped>
.ipnone {
  display: none;
}

.inputanh {
  border: 1px solid #ccc;
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
</style>
<style lang="scss" scoped>
::v-deep(.p-datatable) {
  tr {
    cursor: pointer;
  }
}
</style>
