<script setup>
import { ref, inject, onMounted } from "vue";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import arrIcons from "../../assets/json/icons.json";
import router from "@/router";
import moment from "moment";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
//init Model
const arrroutes = ref([]);
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
});
const tdVitris = [
  { value: "Menuleft", text: "Menu trái" },
  { value: "Menutop", text: "Menu trên" },
];
const module = ref({
  module_name: "",
  is_order: 1,
  status: true,
  is_admin: true,
  is_target: "_self",
});
//Valid Form
const submitted = ref(false);
const rules = {
  module_name: {
    required,
  },
};
const v$ = useVuelidate(rules, module);
//Khai báo biến

const groupedPermissions = ref([
    {
        label: 'Thêm mới',
        items: [
            { label: 'Thêm mới', value: 1 },
        ]
    },
    {
        label: 'Chỉnh sửa',
        items: [
            { label: 'Chỉnh sửa phòng ban', value: 2 },
            { label: 'Chỉnh sửa đơn vị', value: 3 },
            { label: 'Chỉnh sửa tất cả', value: 4 },
        ]
    },
    {
        label: 'Xem',
        items: [
            { label: 'Xem phòng ban', value: 5 },
            { label: 'Xem đơn vị', value: 6 },
            { label: 'Xem tất cả', value: 7 },
        ]
    },
    {
        label: 'Duyệt',
        items: [
            { label: 'Duyệt', value: 8 }
        ]
    }
]);
const tdQuyens = [
  { value: 1, text: "Tất cả các quyền" },
  { value: 2, text: "Xem cá nhân" },
  { value: 3, text: "Xem phòng ban" },
  { value: 4, text: "Xem công ty" },
  { value: 5, text: "Xem tất cả" },
  { value: 6, text: "Chỉnh sửa (thêm, sửa, xóa)" },
  { value: 7, text: "Chỉnh sửa cá nhân" },
  { value: 8, text: "Duyệt chỉnh sửa hồ sơ" },
  { value: 9, text: "Thiết lập ban đầu" },
  { value: 10, text: "Duyệt đề xuất" },
  { value: 11, text: "Lập đề xuất" },
  { value: 12, text: "Phê duyệt" },
  { value: 13, text: "Chiến dịch tuyển dụng" },
  { value: 14, text: "Ứng viên" },
  { value: 15, text: "Lịch phỏng vấn" },
  { value: 16, text: "Tạo đánh giá" },
  { value: 17, text: "Duyệt đánh giá" },
  { value: 18, text: "Quản trị người dùng" },
  { value: 19, text: "Quản trị người dùng đơn vị" },
  { value: 20, text: "Backup dữ liệu" },
].reverse();
const store = inject("store");
const selectCapcha = ref();
const selectedKey = ref();
const selectedNodes = ref([]);
const selectCapchaOrganization = ref();
const treeOrganizations = ref();
const opition = ref({ search: "" });
const modules = ref();
const treemodules = ref();
const displayAddModule = ref(false);
const isFirst = ref(true);
let files = [];
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const tdTargets = ref([
  { value: "_blank", text: "Mở sang tab mới" },
  { value: "_self", text: "Mở tab hiện tại" },
]);
const tdSize = ref([
  { value: "480px", text: "Nhỏ (480px)" },
  { value: "720px", text: "Trung bình (720px)" },
  { value: "1024px", text: "Lớn (1024px)" },
  { value: "100%", text: "Full (100%)" },
]);
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportModule("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportModule("ExportExcelMau");
    },
  },
]);
//Khai báo function
const swalLoadding = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
};
const errorMessage = () => {
  swal.fire({
    text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
    confirmButtonText: "OK",
  });
};
const swalMessage = (title, icon, ms) => {
  swal.fire({
    title: title,
    text: ms,
    icon: icon,
    confirmButtonText: "OK",
  });
};
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const onNodeSelect = (node) => {
  selectedNodes.value.push(node.data.module_id);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(
    selectedNodes.value.indexOf(node.data.module_id),
    1
  );
};
const handleFileUpload = (event) => {
  files = event.target.files;
  var output = document.getElementById("moduleAnh");
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
// get order by parent
const onChangeParent = (item) => {
  const module_id = parseInt(Object.keys(item)[0]);
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_module_get_order_pemission",
            par: [{ par: "module_id", va: module_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        module.value.is_order = data[0][0].c + 1;
        module.value.permission =  convertIntToArray(data[1][0].permission);
      }
    });
};
//Show Modal
const showModalAddModule = () => {
  submitted.value = false;
  selectCapcha.value = {};
  module.value = {
    module_name: "",
    is_order: modules.value.length + 1,
    status: true,
    is_admin: true,
    is_target: "_self",
    is_stand: [{ value: "Menuleft", text: "Menu trái" }],
    is_size: "720px",
    is_view_parent: true,
    permission: [1],
  };
  displayAddModule.value = true;
};
const chonanh = (id) => {
  document.getElementById(id).click();
};
const closedisplayAddModule = () => {
  displayAddModule.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  selectedKey.value = {};
  selectedNodes.value = [];
  loadModule(true);
};
const onSearch = () => {
  loadModule(true);
};
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
const loadOrganization = () => {
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_list_all",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "organization_type", va: -1 },
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
        let obj = renderTree(
          data,
          "organization_id",
          "organization_name",
          "đơn vị"
        );
        treeOrganizations.value = obj.arrtreeChils;
      }
    })
    .catch((error) => {});
};
const loadModule = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_modules_list",
            par: [{ par: "search", va: opition.value.search }],
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
        treemodules.value = obj.arrtreeChils;
        modules.value = obj.arrChils;
      } else {
        modules.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      if (rf) {
        opition.value.loading = false;
      }
    })
    .catch((error) => {
      if (error && error.status === 401) {
        errorMessage();
      }
    });
};
const editModule = (md) => {
  submitted.value = false;
  swalLoadding();
  displayAddModule.value = true;
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_modules_get",
            par: [{ par: "module_id", va: md.module_id }],
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
        if (data[0][0].is_stand) {
          data[0][0].is_stand = tdVitris.filter((x) =>
            data[0][0].is_stand.includes(x.value)
          );
        }
        if (data[0][0].permission) {
          data[0][0].permission = convertIntToArray(data[0][0].permission);
        }
        module.value = data[0][0];
        selectCapcha.value = {};
        selectCapcha.value[module.value.parent_id || "-1"] = true;
        selectCapchaOrganization.value = {};
        selectCapchaOrganization.value[
          module.value.organization_id || "-1"
        ] = true;
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        errorMessage();
      }
    });
};
const handleSubmit = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  let keys = Object.keys(selectCapcha.value);
  module.value.parent_id = keys[0];
  if (module.value.parent_id == -1) {
    module.value.parent_id = null;
  }
  if (selectCapchaOrganization.value) {
    keys = Object.keys(selectCapchaOrganization.value);
    module.value.organization_id = keys[0];
    if (module.value.organization_id == -1) {
      module.value.organization_id = null;
    }
  }
  if(module.value.permission) module.value.permission = module.value.permission.join(",");
  addModule();
};
const addTreeModule = (md) => {
  let is_order = modules.value.length + 1;
  if (md.children) {
    is_order = md.children.length + 1;
  } else {
    is_order = 1;
  }
  selectCapcha.value = {};
  selectCapcha.value[md.data.module_id] = true;
  module.value = {
    module_name: "",
    is_order: is_order,
    status: true,
    is_admin: true,
    is_target: "_self",
    parent_id: md.data.module_id,
    is_stand: [{ value: "Menuleft", text: "Menu trái" }],
    is_size: "720px",
  };
  loadParentPemission(md.data.module_id);
};
const addModule = () => {
  let or = arrroutes.value.find((x) => x.path == module.value.is_link);
  if (or) {
    module.value.IsFilePath = or.component
      .toString()
      .replace("() => import('", "")
      .replace("')", "");
  }
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("anh", file);
  }
  let md = { ...module.value };
  if (md.is_stand) {
    md.is_stand = md.is_stand.map((x) => x.value).join(",");
  }
  formData.append("model", JSON.stringify(md));
  swalLoadding();
  axios({
    method: module.value.module_id ? "put" : "post",
    url:
      baseURL +
      `/api/Modules/${module.value.module_id ? "Update_Module" : "Add_Module"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật Module thành công!");
        loadModule();
        closedisplayAddModule();
      } else {
        swalMessage("Error!", "error", "Có lỗi xảy ra, vui lòng kiểm tra lại!");
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
const loadParentPemission = (id)=>{
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_module_get_order_pemission",
            par: [{ par: "module_id", va: id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        module.value.permission = convertIntToArray(data[1][0].permission);
        //module.value.permission = data[1][0].permission.split("");
      }
      submitted.value = false;
      displayAddModule.value = true;
    });
}
const delModule = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá menu này không!",
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
          .delete(baseURL + "/api/Modules/Del_Module", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.module_id] : selectedNodes.value,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá Module thành công!");
              loadModule();
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
const exportModule = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "post",
    url: baseURL + `/api/Excel/${method}`,
    data: {
      excelname: "DANH SÁCH MENU",
      proc: "sys_modules_listExport",
      par: [{ par: "user_id", va: store.getters.user.user_id }],
    },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Kết xuất Module thành công!");
        //window.open(baseURL + response.data.path);
        if (response.data.path != null) {
            let pathReplace = response.data.path.replace(/\\+/g, '/').replace(/\/+/g, '/').replace(/^\//g, '');
            var listPath = pathReplace.split('/');
            var pathFile = "";
            listPath.forEach(item => {
              if (item.trim() != "")
              {
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
const convertIntToArray = (str)=>{
  var arrs = [];
  if(str != null){
    str.toString().split(",").forEach((item)=>{
    arrs.push(parseInt(item));
    })
  }
  return arrs;
}
const filteredItems = ref([]);
const searchItems = (event) => {
  //in a real application, make a request to a remote url with the query and return filtered results, for demo we filter at client side
  let query = event.query;
  let filteItems = [];
  for (let i = 0; i < arrIcons.length; i++) {
    let item = arrIcons[i];
    if (item.toLowerCase().indexOf(query.toLowerCase()) != -1) {
      filteItems.push(item);
    }
  }
  filteredItems.value = filteItems;
};
onMounted(() => {
  //init
  loadOrganization();
  loadModule(true);
  let routes = [];
  router.options.routes.forEach((ro) => {
    routes.push(ro);
    if (ro.children) {
      ro.children.forEach((roc) => {
        roc.path = ro.path + "/" + roc.path;
        routes.push(roc);
      });
    }
  });
  arrroutes.value = routes;
  return {};
});
</script>
<template>
  <div class="main-layout flex-grow-1 p-2" v-if="store.getters.islogin">
    <TreeTable
      :value="modules"
      v-model:selectionKeys="selectedKey"
      :loading="opition.loading"
      @nodeSelect="onNodeSelect"
      @nodeUnselect="onNodeUnselect"
      :filters="filters"
      :showGridlines="true"
      selectionMode="checkbox"
      filterMode="strict"
      class="p-treetable-sm"
      :paginator="modules && modules.length > 20"
      :rows="20"
      :rowHover="true"
      responsiveLayout="scroll"
      :lazy="true"
      :scrollable="true"
      scrollHeight="flex"
      :globalFilterFields="['module_name']"
    >
      <template #header>
        <h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
          <i class="pi pi-microsoft"></i> Module chức năng
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <Dropdown
              v-model="filters['is_stand']"
              :options="tdVitris"
              optionLabel="text"
              optionValue="value"
              placeholder="Chọn vị trí"
              class="p-column-filter mr-2"
              :showClear="true"
            >
            </Dropdown>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                type="text"
                spellcheck="false"
                v-model="filters['global'].value"
                placeholder="Tìm kiếm"
              />
            </span>
          </template>

          <template #end>
            <Button
              label="Thêm Module"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddModule"
            />
            <Button
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              @click="onRefersh"
            />
            <Button
              label="Xoá"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
              v-if="selectedNodes.length > 0"
              @click="delModule"
            />
            <Button
              label="Export"
              icon="pi pi-file-excel"
              class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            />
            <Menu
              id="overlay_Export"
              ref="menuButs"
              :model="itemButs"
              :popup="true"
            />
          </template>
        </Toolbar>
      </template>
      <!-- <Column
        :sortable="true"
        field="IsOrder"
        header="No."
        class="align-items-center justify-content-center text-center font-bold"
        headerStyle="text-align:center;max-width:120px"
        bodyStyle="text-align:center;max-width:120px"
      ></Column> -->
      <Column
        field="is_order"
        :sortable="true"
        header="STT"
        class="align-items-center justify-content-center text-center font-bold"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      >
        <template #body="md">
          <div v-bind:class="md.node.data.status ? '' : 'text-error'">
            {{ md.node.data.is_order }}
          </div>
        </template>
      </Column>
      <Column
        field="icon"
        header="Icon"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:70px"
        bodyStyle="text-align:center;max-width:70px"
      >
        <template #body="md">
          <i
            v-bind:class="[
              md.node.data.icon,
              md.node.data.status ? '' : 'text-error',
            ]"
          ></i>
        </template>
      </Column>
      <Column
        field="image"
        header="Ảnh"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:70px"
        bodyStyle="text-align:center;max-width:70px"
      >
        <template #body="md">
          <Avatar
            v-if="md.node.data.image"
            v-ripple
            v-bind:image="basedomainURL + md.node.data.image"
          />
        </template>
      </Column>
      <Column
        field="module_name"
        header="Tên Menu"
        :sortable="true"
        :expander="true"
      >
        <template #body="md">
          <div
            v-bind:class="[
              md.node.data.parent_id ? '' : 'font-bold',
              md.node.data.status ? '' : 'text-error',
            ]"
          >
            {{ md.node.data.module_name }}
          </div>
        </template>
      </Column>
      <Column
        field="is_stand"
        header="Vị trí"
        class="align-items-center justify-content-center"
        headerStyle="text-align:center;max-width:180px"
        bodyStyle="text-align:center;max-width:180px"
        filterMatchMode="contains"
      >
        <template #body="md">
          <div v-bind:class="md.node.data.status ? '' : 'text-error'">
            {{ md.node.data.is_stand }}
          </div>
        </template>
      </Column>
      <Column
        header="Chức năng"
        headerClass="text-center"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #header> </template>
        <template #body="md">
          <Button
            type="button"
            icon="pi pi-plus-circle"
            class="p-button-rounded p-button-secondary p-button-outlined"
            style="margin-right: 0.5rem"
            v-tooltip.top="'Thêm module con'"
            @click="addTreeModule(md.node)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-pencil"
            class="p-button-rounded p-button-secondary p-button-outlined"
            style="margin-right: 0.5rem"
            v-tooltip.top="'Chỉnh sửa'"
            @click="editModule(md.node.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-secondary p-button-outlined"
            v-tooltip.top="'Xóa'"
            @click="delModule(md.node.data)"
          ></Button>
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
          v-if="!isFirst"
        >
          <img src="../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </TreeTable>
  </div>
  <Dialog
    header="Cập nhật Module"
    v-model:visible="displayAddModule"
    :style="{ width: '40vw' }"
    :maximizable="true"
    :autoZIndex="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left"
            >Tên menu <span class="redsao">(*)</span></label
          >
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="module.module_name"
            :class="{ 'p-invalid': v$.module_name.$invalid && submitted }"
          />
        </div>
        <small
          v-if="
            (v$.module_name.$invalid && submitted) ||
            v$.module_name.$pending.$response
          "
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.module_name.required.$message
                .replace("Value", "Tên menu")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Cấp cha</label>
          <TreeSelect
            class="col-10"
            @change="onChangeParent"
            v-model="selectCapcha"
            :options="treemodules"
            :showClear="true"
            placeholder=""
            optionLabel="data.module_name"
            optionValue="data.module_id"
          ></TreeSelect>
        </div>

        <div class="col-8">
          <div class="field">
            <label class="col-3 text-left">Icon</label>
            <AutoComplete
              class="col-8 ip36 p-0"
              v-model="module.icon"
              :suggestions="filteredItems"
              @complete="searchItems"
              :virtualScrollerOptions="{ itemSize: 20 }"
              dropdown
            >
              <template #item="slotProps">
                <div class="flex align-items-center">
                  <i :class="slotProps.item" style="font-size: large"></i>
                  <div class="ml-2">{{ slotProps.item }}</div>
                </div>
              </template>
            </AutoComplete>
          </div>
          <div class="field">
            <label class="col-3 text-left">Link</label>
            <Dropdown
              class="col-8 ip36 p-0"
              v-model="module.is_link"
              :options="arrroutes"
              :filter="true"
              optionLabel="path"
              optionValue="path"
              :editable="true"
            />
          </div>
          <div class="field">
            <label class="col-3 text-left">Mã báo cáo</label>
            <InputText
            spellcheck="false"
            class="col-8 ip36"
            v-model="module.report_code"
          />
          </div>
          <div class="field">
            <label class="col-3 text-left">Kiểu mở</label>
            <Dropdown
              class="col-8"
              v-model="module.is_target"
              :options="tdTargets"
              optionLabel="text"
              optionValue="value"
              placeholder="Chọn kiểu mở"
            />
          </div>
        </div>
        <div class="col-4">
          <div class="field">
            <label class="col-12 text-rigth">Ảnh</label>
            <div class="inputanh" @click="chonanh('AnhModule')">
              <img
                id="moduleAnh"
                v-bind:src="
                  module.image
                    ? basedomainURL + module.image
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                "
              />
            </div>
            <input
              class="ipnone"
              id="AnhModule"
              type="file"
              accept="image/*"
              @change="handleFileUpload"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Đơn vị</label>
          <TreeSelect
            class="col-10"
            v-model="selectCapchaOrganization"
            :options="treeOrganizations"
            :showClear="true"
            placeholder=""
            optionLabel="data.organization_name"
            optionValue="data.organization_id"
          ></TreeSelect>
        </div>
        <!-- <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Quyền</label>
          <MultiSelect
            :filter="true"
            class="col-10"
            v-model="module.permission"
            :options="tdQuyens.sort((a, b) => a.value - b.value)"
            optionLabel="text"
            optionValue="value"
            placeholder="Chọn quyền"
            display="chip"
          />
        </div> -->
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Quyền</label>
          <MultiSelect v-model="module.permission" :options="groupedPermissions" optionLabel="label"
           optionGroupLabel="label" optionGroupChildren="items" display="chip"  optionValue="value"
           placeholder="Chọn quyền" class="col-10"
           :filter="true"
           >
            <template #optiongroup="slotProps">
                <div class="flex align-items-center">
                    <div>{{ slotProps.option.label }}</div>
                </div>
            </template>
        </MultiSelect>
        </div>
        <div class="field col-12 md:col-12">
          <label style="vertical-align: text-left" class="col-2"
            >Vị trí</label
          >
          <MultiSelect
            class="col-4"
            v-model="module.is_stand"
            :options="tdVitris"
            optionLabel="text"
            placeholder="Chọn vị trí"
            display="chip"
          />
          <label class="col-4 text-right">Hiển thị menu cha</label>
          <!-- <InputNumber  class="col-1 ip36 p-0" v-model="module.max_length_file" /> -->
          <InputSwitch v-model="module.is_view_parent" />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-bottom">STT</label>
          <InputNumber class="col-2 ip36 p-0" v-model="module.is_order" />
          <label style="" class="col-2 text-right"
            >Trạng thái</label
          >
          <InputSwitch v-model="module.status" class="mt-1" />
          <label style="" class="col-3 text-right"
            >Module chính</label
          >
          <InputSwitch v-model="module.is_main" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddModule"
        class="p-button-raised p-button-secondary"
      />
      <Button
        label="Cập nhật"
        icon="pi pi-save"
        @click="handleSubmit(!v$.$invalid)"
      />
    </template>
  </Dialog>
</template>
<style scoped>
.col-12 .p-inputswitch {
    top: 6px;
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
</style>
<style>
.text-error {
  color: red !important;
}
</style>