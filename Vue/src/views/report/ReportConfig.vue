<script setup>
import { ref, inject, onMounted } from "vue";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import arrIcons from "../../assets/json/icons.json";
import RadioButton from "primevue/radiobutton";
import router from "@/router";
import moment from "moment";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
//init Model
const arrroutes = ref([]);
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  full_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const filtersUsers = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
});
const tdVitris = [
  { value: "Menuleft", text: "Menu trái" },
  { value: "Menutop", text: "Menu trên" },
];
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
//Valid Form
const submitted = ref(false);
const rules = {
  module_name: {
    required,
  },
};
//Khai báo biến
const first_module = ref(0);
const first_user = ref(0);
const store = inject("store");
const selectCapcha = ref();
const selectedKey = ref();
const selectedNodes = ref([]);
const selectCapchaOrganization = ref();
const temporganizations = ref();
const opition = ref({ search: "" });
const modules = ref();
const treemodules = ref();
const expandedKeys = ref({});
const displayAddModule = ref(false);
const isFirst = ref(true);
const is_coppy_module = ref(false);
const data_coppy = ref();
const different_module_move = ref(false);
let files = [];
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const users = ref([]);
const users_main = ref([]);
const displayAddPhongban = ref(false);
const displayAddUser = ref(false);
const row_selected = ref();
const datalists = ref([]);
const row_module = ref();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const tdQuyens = ref([
  { value: 1, text: "Tất cả (công ty)" },
  { value: 2, text: "Phòng ban" },
  { value: 3, text: "Cá nhân" },
]);
const options = ref({});
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
    title: "Error!",
    text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
    icon: "error",
    confirmButtonText: "OK",
  });
};
const swalMessage = (title, icon, ms) => {
  swal.fire({
    title: title,
    text: "Có lỗi xảy ra, vui lòng thử lại!",
    icon: icon,
    confirmButtonText: "OK",
  });
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
        str: encr(JSON.stringify({}), SecretKey, cryoptojs).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        module.value.is_order = data[0][0].c + 1;
      }
    });
};
//Show Modal
const closedisplayAddModule = () => {
  displayAddModule.value = false;
};
//Thêm sửa xoáloadUser
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
const loadUser = (data) => {
  var list_users =
    data.report_module_user && data.report_module_user.length > 0
      ? data.report_module_user.map((x) => x.user_id)
      : [];
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "report_users_list",
            par: [
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 20 },
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
        data.forEach((item) => {
          item.is_checked = list_users.includes(item.user_id) ? true : false;
        });
        users.value = data;
      }
    })
    .catch((error) => {});
};
const initUser = (data) => {
  filters.value["global"].value = null;
  if (data_coppy.value && data.module_id !== data_coppy.value.module_id) {
    different_module_move.value = true;
  } else different_module_move.value = false;
  row_module.value = data;
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "report_users_list1",
            par: [
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 20 },
              { par: "module_id", va: data.module_id },
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
        data.forEach((item) => {
          item.is_link = row_module.value.is_link;
          item.module_id = row_module.value.module_id;
          item.report_module_user = item.nguoidungs
            ? JSON.parse(item.nguoidungs)
            : [];
          item.report_module_organization = item.phongbans
            ? JSON.parse(item.phongbans)
            : [];
        });
        users_main.value = data;
        displayAddModule.value = true;
      }
    })
    .catch((error) => {});
};
const loadOrganization = (data) => {
  var pb_string =
    data.report_module_organization &&
    data.report_module_organization.length > 0
      ? data.report_module_organization.map((x) => parseInt(x.organization_id))
      : [];
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "report_organization_listtree",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
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
        data.forEach((item) => {
          item.is_checked = pb_string.includes(parseInt(item.organization_id))
            ? true
            : false;
        });
        // data = data.map(obj => ({ ...obj, Active: 'false' }))
        let obj = renderTree(
          data,
          "organization_id",
          "organization_name",
          "Đơn vị"
        );
        temporganizations.value = obj.arrChils;
        temporganizations.value.forEach((element) => {
          expandNode(element);
        });
      }
    })
    .catch((error) => {});
};
const addTreeModule = (data) => {
  initUser(data);
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
            proc: "report_modules_list",
            par: [{ par: "@user_id", va: store.state.user.user_id }],
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
        modules.value = obj.arrChils;
        modules.value.forEach((element) => {
          expandNode(element);
        });
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
const expandNode = (node) => {
  if (node.children && node.children.length) {
    expandedKeys.value[node.key] = true;
    for (let child of node.children) {
      expandNode(child);
    }
  }
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
        window.open(baseURL + response.data.path);
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
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
const changeSubject = (data) => {
  row_selected.value = data;
  if (data.is_permission == 2) {
    loadOrganization(data);
    datalists.value = [];
    displayAddPhongban.value = true;
  } else if (data.is_permission == 3) {
    loadUser(data);
    displayAddUser.value = true;
  }
};
const onCheckBox = (data) => {
  checkAllChildren(data);
};
function checkAllChildren(data) {
  if (data.children && data.children.length > 0) {
    data.children.forEach((item) => {
      item.data.is_checked = data.data.is_checked;
      checkAllChildren(item);
    });
  }
}
function pushItemChecked(data) {
  data.forEach((item) => {
    if (item.data.is_checked) {
      datalists.value.push({
        organization_id: item.data.organization_id,
        organization_name: item.data.organization_name,
      });
    }
    if (item.children && item.children.length > 0) {
      pushItemChecked(item.children);
    }
  });
}
const savePhongban = () => {
  if (temporganizations.value.length > 0)
    pushItemChecked(temporganizations.value);
  row_selected.value.report_module_organization = datalists.value.map(
    (obj) => ({ ...obj, user_sub: row_selected.value.user_id })
  );
  row_selected.value.report_module_user = [];
  displayAddPhongban.value = false;
};
const saveUser = () => {
  row_selected.value.report_module_user = users.value
    .filter((x) => x.is_checked)
    .map((obj) => ({ ...obj, user_sub: row_selected.value.user_id }));
  row_selected.value.report_module_organization = [];
  displayAddUser.value = false;
};
const saveModule = () => {
  users_main.value.forEach((item) => {
    item.not_valid = false;
    if (
      (item.is_permission == 2 &&
        item.report_module_organization &&
        item.report_module_organization.length == 0) ||
      (item.is_permission == 3 &&
        item.report_module_user &&
        item.report_module_user.length == 0)
    )
      item.not_valid = true;
  });
  if (users_main.value.filter((x) => x.not_valid).length > 0) {
    users_main.value = users_main.value.sort(
      (a, b) => b.not_valid - a.not_valid
    );
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng nhập đầy đủ thông tin trường bôi đỏ!",
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
  let formData = new FormData();
  debugger
  formData.append("is_link", JSON.stringify(users_main.value.length>0 ?users_main.value[0].is_link:''));
  formData.append("module_id", JSON.stringify(users_main.value.length>0 ?users_main.value[0].module_id:''));
  formData.append("user_module", JSON.stringify(users_main.value));
  axios
    .put(baseURL + "/api/ReportModule/update_module", formData, config)
    .then((response) => {
      if (response.data.err === "0") {
        swal.close();
        toast.success("Cập nhật thành công!");
        displayAddModule.value = false;
      }
    });
};
const rowClass = (data) => {
  return data.not_valid ? "not-vaild" : "";
};
const coppyModule = () => {
  is_coppy_module.value = true;
  data_coppy.value = JSON.parse(JSON.stringify(row_module.value));
  toast.success("Đã sao chép thành công!");
};
const pasteModule = () => {
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "report_users_list1",
            par: [
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 20 },
              { par: "module_id", va: data_coppy.value.module_id },
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
        data.forEach((item) => {
          item.is_link = row_module.value.is_link;
          item.module_id = row_module.value.module_id;
          item.report_module_user = item.nguoidungs
            ? JSON.parse(item.nguoidungs)
            : [];
          item.report_module_organization = item.phongbans
            ? JSON.parse(item.phongbans)
            : [];
        });
        users_main.value = data;
        toast.success("Đã dán cấu hình thành công!");
      }
    })
    .catch((error) => {});
};
const onRefershModule = () => {
  filters.value["global"].value = null;
  first_module.value = 0;
};
onMounted(() => {
  //init
  loadModule(true);
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
      :showGridlines="true"
      filterMode="strict"
      class="p-treetable-sm"
      :paginator="modules && modules.length > 20"
      :rows="20"
      :rowHover="true"
      responsiveLayout="scroll"
      :lazy="true"
      :scrollable="true"
      scrollHeight="flex"
      :expandedKeys="expandedKeys"
    >
      <template #header>
        <h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
          <i class="pi pi-microsoft"></i> Thiết lập quyền báo cáo
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />
            </span>
          </template>

          <template #end>
            <Button
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              @click="onRefersh"
            />
          </template>
        </Toolbar>
      </template>
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
        field="module_name"
        header="Tên Module"
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
        header="Phân quyền"
        headerClass="text-center"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #header> </template>
        <template #body="md">
          <Button
            v-if="md.node.data.parent_id !== null"
            type="button"
            icon="pi pi-key"
            class="p-button-rounded p-button-secondary p-button-outlined"
            style="margin-right: 0.5rem"
            v-tooltip.top="'Phân quyền'"
            @click="addTreeModule(md.node.data)"
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
    header="Phân quyền truy cập"
    v-model:visible="displayAddModule"
    :style="{ width: '65vw' }"
    :maximizable="true"
    :autoZIndex="false"
    style="z-index: 1000"
    class="modal-report-config"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <DataTable
          class="w-full p-datatable-sm e-sm cursor-pointer"
          :value="users_main"
          v-model:filters="filters"
          :showGridlines="true"
          filterMode="lenient"
          :paginator="users_main && users_main.length > 20"
          :rows="20"
          filterDisplay="menu"
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          :rowsPerPageOptions="[20, 30, 50, 100, 200]"
          :scrollable="true"
          scrollHeight="flex"
          responsiveLayout="scroll"
          :rowClass="rowClass"
          :globalFilterFields="[
            'full_name',
            'user_id',
            'position_name',
            'organization_name',
          ]"
          v-model:first="first_module"
        >
          <template #header>
            <h3 class="module-title mt-0 ml-1 mb-2">
              <i class="pi pi-microsoft"></i> Danh sách người dùng ({{
                users_main.length || 0
              }})
            </h3>
            <Toolbar class="w-full custoolbar">
              <template #start>
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
                  v-if="!different_module_move"
                  class="mr-2 p-button-outlined p-button-secondary"
                  label="Sao chép"
                  icon="pi pi-copy"
                  @click="coppyModule()"
                  :disabled="is_coppy_module ? true : false"
                />
                <Button
                  v-if="is_coppy_module && different_module_move"
                  label="Dán"
                  icon="pi pi-copy"
                  class="mr-2"
                  @click="pasteModule()"
                />
                <Button
                  class="mr-2 p-button-outlined p-button-secondary"
                  icon="pi pi-refresh"
                  @click="onRefershModule()"
                />
              </template>
            </Toolbar>
          </template>
          <Column
            field="avatar"
            header=""
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:100px"
            bodyStyle="text-align:center;max-width:100px"
          >
            <template #body="md">
              <Avatar
                v-bind:label="
                  md.data.avatar
                    ? ''
                    : md.data.last_name.substring(0, 1).toUpperCase()
                "
                v-bind:image="basedomainURL + md.data.avatar"
                style="
                  background-color: #2196f3;
                  color: #ffffff;
                  width: 3rem;
                  height: 3rem;
                "
                :style="{
                  background: bgColor[md.data.full_name.length % 7],
                }"
                class="mr-2"
                size="xlarge"
                shape="circle"
              />
            </template>
          </Column>
          <Column
            field="full_name"
            header="Họ tên"
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;"
            bodyStyle="text-align:center;"
            :showFilterMatchModes="false"
          >
          </Column>
          <Column
            field="position_name"
            header="Chức vụ"
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:150px"
            bodyStyle="text-align:center;max-width:150px"
          >
          </Column>
          <Column
            field="organization_name"
            header="Phòng ban"
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:250px"
            bodyStyle="text-align:center;max-width:250px"
          >
          </Column>
          <Column
            headerClass="align-items-center justify-content-center text-center"
            header="Phạm vi truy cập"
            headerStyle="text-align:center;width:200px"
            bodyStyle="text-align:center;width:200px"
            :showFilterMatchModes="false"
          >
            <template #body="md">
              <Dropdown
                v-model="md.data.is_permission"
                :options="tdQuyens"
                :popup="true"
                class="w-full"
                optionLabel="text"
                optionValue="value"
                placeholder="Chọn phạm vi"
                :showClear="true"
              />
            </template>
          </Column>
          <Column
            header="Lựa chọn"
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;width:100px"
            bodyStyle="text-align:center;width:100px"
          >
            <template #body="md">
              <!-- <Button
                @click="changeSubject(md.data)"
                v-if="md.data.is_permission > 1"
                class="
                  align-items-center
                  justify-content-center
                  text-center
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                type="button"
                icon="pi pi-plus-circle"
                v-tooltip.left="'Thêm phòng ban'"
              ></Button> -->
              <div
                class="flex justify-content-center"
                v-if="md.data.is_permission > 1"
              >
                <AvatarGroup
                  v-if="
                    md.data.is_permission == 3 &&
                    md.data.report_module_user &&
                    md.data.report_module_user.length > 0
                  "
                  @click="changeSubject(md.data)"
                >
                  <Avatar
                    v-for="(item, index) in md.data.report_module_user.slice(
                      0,
                      3
                    )"
                    v-bind:label="
                      item.avatar ? '' : item.last_name.substring(0, 1)
                    "
                    v-bind:image="
                      item.avatar
                        ? basedomainURL + item.avatar
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    v-tooltip.top="
                      item.full_name +
                      (item.position_name != null
                        ? '<br>' + item.position_name
                        : '') +
                      (item.department_name != null
                        ? '<br>' + item.department_name
                        : '')
                    "
                    :key="item.user_id"
                    style="border: 2px solid orange; color: white"
                    @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                    size="large"
                    shape="circle"
                    class="cursor-pointer"
                    :style="{ backgroundColor: bgColor[index % 7] }"
                  />
                  <Avatar
                    v-if="
                      md.data.report_module_user &&
                      md.data.report_module_user.length > 3
                    "
                    v-bind:label="
                      '+' + (md.data.report_module_user.length - 3).toString()
                    "
                    shape="circle"
                    size="large"
                    style="background-color: #2196f3; color: #ffffff"
                    class="cursor-pointer"
                  />
                </AvatarGroup>
                <AvatarGroup
                  v-else-if="
                    md.data.is_permission == 2 &&
                    md.data.report_module_organization &&
                    md.data.report_module_organization.length > 0
                  "
                  @click="changeSubject(md.data)"
                >
                  <Avatar
                    v-for="(
                      item, index
                    ) in md.data.report_module_organization.slice(0, 3)"
                    v-bind:label="item.organization_name.substring(0, 1)"
                    v-tooltip.top="item.organization_name"
                    :key="item.organization_id"
                    style="border: 2px solid orange; color: white"
                    @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                    size="large"
                    shape="circle"
                    class="cursor-pointer avt-pb"
                    :style="{ backgroundColor: bgColor[(index + 3) % 7] }"
                  />
                  <Avatar
                    v-if="
                      md.data.report_module_organization &&
                      md.data.report_module_organization.length > 3
                    "
                    v-bind:label="
                      '+' +
                      (md.data.report_module_organization.length - 3).toString()
                    "
                    shape="circle"
                    size="large"
                    style="background-color: #2196f3; color: #ffffff"
                    class="cursor-pointer avt-pb"
                  />
                </AvatarGroup>
                <div v-else class="format-center">
                  <Button
                    @click="changeSubject(md.data)"
                    class="
                      p-button-rounded p-button-secondary p-button-outlined
                      mx-1
                    "
                    type="button"
                    icon="pi pi-plus-circle"
                    v-tooltip.top="'Thêm người chủ trì'"
                  ></Button>
                </div>
              </div>
            </template>
          </Column>
        </DataTable>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddModule"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="saveModule()" />
    </template>
  </Dialog>
  <Dialog
    header="Chọn phòng ban"
    v-model:visible="displayAddPhongban"
    :style="{ width: '50vw' }"
    :maximizable="true"
    :autoZIndex="false"
    style="z-index: 1001"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <TreeTable
        :value="temporganizations"
        :scrollable="true"
        :rowHover="true"
        :expandedKeys="expandedKeys"
        :lazy="true"
        dataKey="organization_id"
        filterMode="strict"
        scrollHeight="flex"
        filterDisplay="menu"
        class="d-lang-table"
      >
        <Column field="organization_name" :expander="true">
          <template #body="slotProps">
            <div
              :class="
                slotProps.node.data.organization_id ===
                  options.filter_organization_id ||
                (options.filter_organization_id == null &&
                  slotProps.node.data.parent_id == null)
                  ? 'row-active'
                  : ''
              "
              class="py-1"
              style="width: -webkit-fill-available"
            >
              <Checkbox
                class="mr-1"
                :binary="true"
                v-model="slotProps.node.data.is_checked"
                @change="onCheckBox(slotProps.node)"
              />
              <span>{{ slotProps.node.data.organization_name }}</span>
            </div>
          </template>
        </Column>
      </TreeTable>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="displayAddPhongban == false"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="savePhongban()" />
    </template>
  </Dialog>
  <Dialog
    header="Chọn người dùng"
    v-model:visible="displayAddUser"
    :style="{ width: '50vw' }"
    :maximizable="true"
    :autoZIndex="false"
    style="z-index: 1001"
    :modal="true"
    class="modal-choice-user"
  >
    <form>
      <DataTable
        class="w-full p-datatable-sm e-sm cursor-pointer"
        :value="users"
        v-model:filters="filtersUsers"
        dataKey="user_id"
        :showGridlines="true"
        :rowHover="true"
        currentPageReportTemplate=""
        filterMode="lenient"
        :paginator="users && users.length > 20"
        :rows="20"
        filterDisplay="menu"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
        :rowsPerPageOptions="[20, 30, 50, 100]"
        :scrollable="true"
        scrollHeight="flex"
        responsiveLayout="scroll"
        rowGroupMode="subheader"
        groupRowsBy="organization_name"
        :globalFilterFields="['full_name', 'user_id']"
        v-model:first="first_user"
      >
        <template #header>
          <Toolbar class="w-full custoolbar">
            <template #start>
              <span class="p-input-icon-left" style="width: 100%">
                <i class="pi pi-search" />
                <InputText
                  type="text"
                  spellcheck="false"
                  placeholder=" Tìm kiếm người dùng"
                  style="width: 100%"
                  v-model="filtersUsers['global'].value"
                />
              </span>
            </template>
            <template #end> </template>
          </Toolbar>
        </template>
        <template #groupheader="slotProps">
          <i class="pi pi-building mr-2"></i>
          {{ slotProps.data.organization_name }}
        </template>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:70px;height:40px"
          bodyStyle="text-align:center;max-width:70px"
          field=""
          header=""
        >
          <template #body="md">
            <Checkbox :binary="true" v-model="md.data.is_checked" />
          </template>
        </Column>
        <Column
          headerStyle="text-align:center;height:40px;"
          bodyStyle="text-align:left;"
          field="full_name"
          header="Họ tên"
        >
          <template #body="data">
            <div>
              {{ data.data.full_name }}
            </div>
          </template>
        </Column>
        <Column
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:150px"
          bodyStyle="text-align:center;max-width:150px"
          field="position_name"
          header="Chức vụ"
        >
          <template #body="data">
            <div>
              {{ data.data.position_name }}
            </div>
          </template>
        </Column>
      </DataTable>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="displayAddUser == false"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="saveUser()" />
    </template>
  </Dialog>
</template>
<style scoped>
.avt-pb {
  border-radius: 30% !important;
}
.ipnone {
  display: none;
}
.p-avatar {
  font-size: 1.25rem !important;
}
.p-datatable-header {
  widows: 95%;
}
</style>
<style lang="scss" scoped>
</style>