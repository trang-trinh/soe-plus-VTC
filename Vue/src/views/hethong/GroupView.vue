<script setup>
import { ref, inject, onMounted, computed } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
//init Model
const group = ref({
  Group_Name: "",
  STT: 1,
  Trangthai: true,
});
//Valid Form
const submitted = ref(false);
const rules = {
  Group_Name: {
    required,
    $errors: [
      {
        $property: "Group_Name",
        $validator: "required",
        $message: "Tên Nhóm không được để trống!",
      },
    ],
  },
};
const v$ = useVuelidate(rules, group);
//Khai báo biến
const isAdd = ref(true);
const selectedNodes = ref([]);
const filters = ref({});
const groups = ref();
const displayAddGroup = ref(false);
const isFirst = ref(true);
const store = inject("store");
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const opition = ref({
  search: "",
  Donvi_ID: store.getters.user.IsSupper ? -1 : store.getters.user.Donvi_ID,
  user_id: store.getters.user.user_id,
  GroupType_ID: -1,
});
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const tdActionTypes = [
  { value: 0, text: "Chỉ cần 1 người duyệt" },
  { value: 1, text: "Duyệt lần lượt" },
  { value: 2, text: "Duyệt ngẫu nhiên" },
];
function toObject(arr) {
  var rv = {};
  for (var i = 0; i < arr.length; ++i) rv[arr[i].value] = arr[i].text;
  return rv;
}
let objectActionTypes = toObject(tdActionTypes);
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportGroup("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportGroup("ExportExcelMau");
    },
  },
]);

//Khai báo function
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
//Show Modal
const showModalAddGroup = () => {
  isAdd.value = true;
  submitted.value = false;
  group.value = {
    Group_Name: "",
    Group_ID: -1,
    IsSystem: false,
    ActionType: 0,
    STT: groups.value.length + 1,
    Trangthai: true,
  };
  displayAddGroup.value = true;
};
const closedisplayAddGroup = () => {
  displayAddGroup.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  loadGroup(true);
};
const onSearch = () => {
  loadGroup(true);
};
const renderTree = (data, paid, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x[paid] == null || data.findIndex((a) => a[id] == x[paid]) == -1)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x[paid] == pid);
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
        let dts = data.filter((x) => x[paid] == pid);
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
  arrtreeChils.unshift({ key: -1, data: -1, label: "-----Chọn " + title + "----" });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
//Từ điển
const selectCapchaDonvi = ref();
const treedonvis = ref();
const users = ref([]);
const tdGroupTypes = ref([]);
const loadTudien = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Sys_Group_Tudiens",
        par: [{ par: "user_id", va: store.getters.user.user_id }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        let obj = renderTree(data[1], "Capcha_ID", "Donvi_ID", "organization_name", "đơn vị");
        treedonvis.value = obj.arrtreeChils;
        users.value = data[0];
        tdGroupTypes.value = data[2];
      }
    })
    .catch((error) => {});
};
const loadGroup = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Sys_Group_List",
        par: [
          { par: "s", va: opition.value.search },
          { par: "Donvi_ID", va: opition.value.Donvi_ID },
          { par: "GroupType_ID", va: opition.value.GroupType_ID },
          { par: "user_id", va: opition.value.user_id },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        data.forEach((r) => {
          if (r.Users) {
            r.Users = JSON.parse(r.Users);
          }
        });
        groups.value = data;
      } else {
        groups.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      if (rf) {
        opition.value.loading = false;
      }
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const editGroup = (md) => {
  isAdd.value = false;
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddGroup.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "Sys_Group_Get", par: [{ par: "Group_ID", va: md.Group_ID }] },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        group.value = data[0][0];
        selectCapchaDonvi.value = {};
        selectCapchaDonvi.value[
          group.value.Donvi_ID != null ? group.value.Donvi_ID : "-1"
        ] = true;
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
const handleSubmit = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (selectCapchaDonvi.value) {
    let keys = Object.keys(selectCapchaDonvi.value);
    group.value.Donvi_ID = keys[0];
    if (group.value.Donvi_ID == -1) {
      group.value.Donvi_ID = null;
    }
  }
  addGroup();
};
const addGroup = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: isAdd.value == false ? "put" : "post",
    url: baseURL + `/api/Group/${isAdd.value == false ? "Update_Group" : "Add_Group"}`,
    data: group.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật nhóm duyệt thành công!");
        loadGroup();
        closedisplayAddGroup();
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
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};

const delGroup = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá nhóm duyệt này không!",
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
          .delete(baseURL + "/api/Group/Del_Group", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.Group_ID] : selectedNodes.value.map((x) => x.Group_ID),
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá nhóm duyệt thành công!");
              loadGroup();
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
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

const upTrangthaiGroup = (md) => {
  let ids = [md.Group_ID];
  let tts = [md.Trangthai];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/Group/Update_TrangthaiGroup",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái nhóm duyệt thành công!");
        loadGroup();
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};

const exportGroup = (method) => {
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
        excelname: "DANH SÁCH NHÓM DUYỆT",
        proc: "Sys_Group_ListExport",
        par: par,
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Kết xuất Data thành công!");
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
            //window.open(baseURL + response.data.path);
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
//Người dùng trong nhóm
const refUser = ref();
const displayUsersGroup = ref(false);
const chonUsers = ref();
const compUsers = computed(() =>
  users.value.filter(
    (x) => group.value.Users.findIndex((u) => u.user_id == x.user_id) == -1
  )
);
const openUsersGroup = (md) => {
  displayUsersGroup.value = true;
  group.value = md;
  loadUserGroup();
};
const loadUserGroup = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Sys_GroupUsers_List",
        par: [
          { par: "user_id ", va: store.getters.user.user_id },
          { par: "Group_ID ", va: group.value.Group_ID },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        group.value.Users = data[0];
      } else {
        group.value.Users = [];
      }
    })
    .catch((error) => {});
};
const saveUserGroup = () => {
  let datasUsers = chonUsers.value.map((x, i) => ({
    user_id: x.user_id,
    GroupUser_ID: -1,
    Group_ID: group.value.Group_ID,
    STT: i + 1,
    Trangthai: true,
  }));
  refUser.value.hide();
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(baseURL + "/api/Group/Add_GroupUsers", datasUsers, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Thêm người dùng thành công!");
        loadUserGroup();
        chonUsers.value = [];
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
    });
};
const delUserGroup = (md) => {
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
          .delete(baseURL + "/api/Group/Del_GroupUsers", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: [md.GroupUser_ID],
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá người dùng thành công!");
              loadUserGroup();
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
          });
      }
    });
};
onMounted(() => {
  //init
  loadGroup(true);
  loadTudien();
  return {};
});
</script>
<template>
  <div class="main-layout flex-grow-1 p-2" v-if="store.getters.islogin">
    <DataTable
      class="w-full p-datatable-sm e-sm"
      :value="groups"
      :paginator="groups && groups.length > 20"
      :loading="opition.loading"
      :rows="20"
      dataKey="Group_ID"
      :showGridlines="true"
      :rowHover="true"
      v-model:selection="selectedNodes"
      :filters="filters"
      filterMode="lenient"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[10, 25, 50]"
      currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
    >
      <template #header>
        <h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
          <i class="pi pi-list"></i> Nhóm duyệt
          <span v-if="groups">({{ groups.length }})</span>
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <Dropdown
              class="mr-2"
              v-model="opition.GroupType_ID"
              :options="tdGroupTypes"
              optionLabel="Type_Name"
              optionValue="GroupType_ID"
              placeholder="Loại nhóm"
              :show-clear="true"
              @change="onRefersh"
            />
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                type="text"
                spellcheck="false"
                v-model="opition.search"
                placeholder="Tìm kiếm"
                v-on:keyup.enter="onSearch"
              />
            </span>
          </template>

          <template #end>
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
              @click="delGroup"
            />
            <Button
              label="Export"
              icon="pi pi-file-excel"
              class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            />
            <Menu id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" />
            <Button
              label="Thêm nhóm"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddGroup"
            />
          </template>
        </Toolbar>
      </template>
      <Column
        selectionMode="multiple"
        headerStyle="text-align:center;max-width:50px"
        bodyStyle="text-align:center;max-width:50px"
        class="align-items-center justify-content-center text-center"
      ></Column>
      <Column
        field="STT"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:70px"
        bodyStyle="text-align:center;max-width:70px"
      ></Column>
      <Column field="Group_Name" header="Tên Nhóm" :sortable="true"> </Column>
      <Column
        field="Type_Name"
        :sortable="true"
        header="Loại Nhóm"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      >
      </Column>
      <Column
        field="ActionType"
        :sortable="true"
        header="Kiểu nhóm"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:170px"
        bodyStyle="text-align:center;max-width:170px"
      >
        <template #body="md">
          <Chip
            :label="objectActionTypes[md.data.ActionType]"
            :class="'chip' + md.data.ActionType"
          />
        </template>
      </Column>
      <Column
        field="users"
        header="Thành viên"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:170px"
        bodyStyle="text-align:center;max-width:170px"
      >
        <template #body="slotProps">
          <Button
            class="p-button-text p-button-plain"
            @click="openUsersGroup(slotProps.data)"
          >
            <AvatarGroup v-if="slotProps.data.Users">
              <Avatar
                v-for="item in slotProps.data.Users.slice(0, 3)"
                :key="item.PlanUser_ID"
                v-bind:label="item.Avartar ? '' : item.full_name.substring(0, 1)"
                v-bind:image="basedomainURL + item.Avartar"
                style="background-color: #2196f3; color: #ffffff; vertical-align: middle"
                class="mr-2"
                size="small"
                shape="circle"
              />
              <Avatar
                v-if="slotProps.data.Users && slotProps.data.Users.length > 3"
                v-bind:label="'+' + (slotProps.data.Users.length - 3).toString()"
                shape="circle"
                size="small"
                style="background-color: #2196f3; color: #ffffff"
              />
            </AvatarGroup>
          </Button>
          <Button
            type="button"
            v-if="!slotProps.data.Users || slotProps.data.Users.length == 0"
            icon="pi pi-users"
            class="p-button-rounded p-button-sm p-button-secondary"
            style="margin-right: 0.5rem"
            @click="openUsersGroup(slotProps.data)"
          ></Button>
        </template>
      </Column>
      <Column
        field="Trangthai"
        header="Trạng thái"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      >
        <template #body="md">
          <Checkbox
            v-model="md.data.Trangthai"
            :binary="true"
            @change="upTrangthaiGroup(md.data)"
          />
        </template>
      </Column>
      <Column
        headerClass="text-center"
        bodyClass="text-center"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      >
        <template #header> </template>
        <template #body="md">
          <Button
            type="button"
            icon="pi pi-pencil"
            class="p-button-rounded p-button-sm p-button-info"
            style="margin-right: 0.5rem"
            @click="editGroup(md.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delGroup(md.data)"
          ></Button>
        </template>
      </Column>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          v-if="!isFirst"
        >
          <img src="../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
  </div>
  <Dialog
    header="Cập nhật nhóm duyệt"
    v-model:visible="displayAddGroup"
    :style="{ width: '720px', zIndex: 2 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12" v-if="!isAdd">
          <label class="col-2 text-left">Mã nhóm </label>
          <InputText
            spellcheck="false"
            v-bind:disabled="!isAdd"
            class="col-10 ip36"
            v-model="group.Group_ID"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tên nhóm <span class="redsao">(*)</span></label>
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="group.Group_Name"
            :class="{ 'p-invalid': v$.Group_Name.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.Group_Name.$invalid && submitted) || v$.Group_Name.$pending.$response"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.Group_Name.required.$message
                .replace("Value", "Tên nhóm")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Mô tả </label>
          <Textarea
            spellcheck="false"
            class="col-10 p-2"
            rows="2"
            style="vertical-align: middle"
            v-model="group.Des"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Loại nhóm</label>
          <Dropdown
            class="col-4"
            v-model="group.GroupType_ID"
            :options="tdGroupTypes"
            optionLabel="Type_Name"
            optionValue="GroupType_ID"
            placeholder=""
          />

          <label class="col-2 text-right">kiểu duyệt</label>
          <Dropdown
            class="col-4"
            v-model="group.ActionType"
            :options="tdActionTypes"
            optionLabel="text"
            optionValue="value"
            placeholder=""
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Đơn vị</label>
          <TreeSelect
            class="col-10"
            v-model="selectCapchaDonvi"
            :options="treedonvis"
            :showClear="store.getters.user.IsSupper"
            placeholder=""
            optionLabel="data.organization_name"
            optionValue="data.Donvi_ID"
          ></TreeSelect>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-2 ip36 p-0" v-model="group.STT" />
          <label style="vertical-align: text-bottom" class="col-2 text-right"
            >Trạng thái</label
          >
          <InputSwitch v-model="group.Trangthai" class="mt-1" />
          <label
            style="vertical-align: text-bottom"
            v-if="store.getters.user.IsSupper"
            class="col-2 text-right"
            >Hệ thống</label
          >
          <InputSwitch
            v-if="store.getters.user.IsSupper"
            v-model="group.IsSystem"
            class="mt-1"
          />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddGroup"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
    </template>
  </Dialog>
  <Dialog
    header="Người dùng trong nhóm"
    v-model:visible="displayUsersGroup"
    :style="{ width: '850px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <div class="main-layout true flex-grow-1 p-2'" style="min-height: 450px !important">
      <DataTable
        class="w-full p-datatable-sm e-sm"
        :value="group.Users"
        dataKey="GroupUser_ID"
        :showGridlines="true"
        :rowHover="true"
        currentPageReportTemplate=""
        responsiveLayout="scroll"
        :scrollable="true"
        scrollHeight="flex"
      >
        <template #header>
          <Toolbar class="w-full custoolbar">
            <template #end>
              <div>
                <MultiSelect
                  ref="refUser"
                  :virtualScrollerOptions="{ itemSize: 48 }"
                  style="max-width: 700px"
                  :options="compUsers"
                  v-model="chonUsers"
                  optionLabel="full_name"
                  placeholder="Chọn người dùng"
                  :popup="true"
                  :filter="true"
                  :showClear="true"
                >
                  <template #value="slotProps">
                    <div
                      class="user-item user-item-value"
                      v-if="slotProps.value && slotProps.value.length > 0"
                      v-for="u in slotProps.value"
                      :key="u.user_id"
                    >
                      <Avatar
                        v-bind:label="u.Avartar ? '' : u.full_name.substring(0, 1)"
                        v-bind:image="basedomainURL + u.Avartar"
                        style="background-color: #2196f3; color: #ffffff"
                        class="mr-2"
                        shape="circle"
                      />
                      <div>{{ u.full_name }}</div>
                    </div>
                    <span v-else>
                      {{ slotProps.placeholder }}
                    </span>
                  </template>
                  <template #option="slotProps">
                    <div class="user-item">
                      <Avatar
                        v-bind:label="
                          slotProps.option.Avartar
                            ? ''
                            : slotProps.option.full_name.substring(0, 1)
                        "
                        v-bind:image="basedomainURL + slotProps.option.Avartar"
                        style="background-color: #2196f3; color: #ffffff"
                        class="mr-2"
                        shape="circle"
                      />
                      <div>
                        {{ slotProps.option.full_name
                        }}<i class="gvpb">{{ slotProps.option.organization_name }}</i>
                      </div>
                    </div>
                  </template>
                  <template #footer>
                    <div class="text-right align-items-end p-2">
                      <Button
                        label="Cập nhật"
                        icon="pi pi-save"
                        class="p-button-sm"
                        @click="saveUserGroup"
                      />
                    </div>
                  </template>
                </MultiSelect>
              </div>
              <Button
                class="ml-2 p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                @click="loadUserGroup"
              />
            </template>
          </Toolbar>
        </template>
        <Column
          field="STT"
          header="STT"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:80px"
          bodyStyle="text-align:center;max-width:80px"
        ></Column>
        <Column field="full_name" header="Người dùng">
          <template #body="md">
            <Avatar
              v-bind:label="md.data.Avartar ? '' : md.data.full_name.substring(0, 1)"
              v-bind:image="basedomainURL + md.data.Avartar"
              style="background-color: #2196f3; color: #ffffff"
              class="mr-2"
              shape="circle"
            />
            <div>{{ md.data.full_name }}</div>
          </template>
        </Column>
        <Column
          field="Trangthai"
          header="Trạng thái"
          class="align-items-center justify-content-center text-center"
          headerStyle="text-align:center;max-width:100px"
          bodyStyle="text-align:center;max-width:100px"
        >
          <template #body="md">
            <Checkbox
              v-model="md.data.Trangthai"
              :binary="true"
              @change="upTrangthaiUserGroup(md.data)"
            />
          </template>
        </Column>
        <Column
          headerClass="text-center"
          bodyClass="text-center"
          headerStyle="text-align:center;max-width:60px"
          bodyStyle="text-align:center;max-width:60px"
        >
          <template #header> </template>
          <template #body="md">
            <Button
              type="button"
              icon="pi pi-trash"
              class="p-button-rounded p-button-sm p-button-danger"
              @click="delUserGroup(md.data)"
            ></Button>
          </template>
        </Column>
        <template #empty>
          <div
            class="m-auto align-items-center justify-content-center p-4 text-center"
            v-if="!isFirst"
          >
            <img src="../../assets/background/nodata.png" height="144" />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
        </template>
      </DataTable>
    </div>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="displayUsersGroup = false"
        class="p-button-raised p-button-secondary"
      />
    </template>
  </Dialog>
</template>
<style scoped>
.chip0 {
  background-color: #4285f4;
  color: #fff;
  font-size: 0.875rem;
}
.chip1 {
  background-color: #689f38;
  color: #fff;
  font-size: 0.875rem;
}
.chip2 {
  background-color: #607d8b;
  color: #fff;
  font-size: 0.875rem;
}
</style>
