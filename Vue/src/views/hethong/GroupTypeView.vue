<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
//init Model
const grouptype = ref({
  Type_Name: "",
  STT: 1,
  Trangthai: true,
});
//Valid Form
const submitted = ref(false);
const rules = {
  GroupType_ID: {
    required,
    $errors: [
      {
        $property: "GroupType_ID",
        $validator: "required",
        $message: "Mã loại nhóm không được để trống!",
      },
    ],
  },
  Type_Name: {
    required,
    $errors: [
      {
        $property: "Type_Name",
        $validator: "required",
        $message: "Tên loại nhóm không được để trống!",
      },
    ],
  },
};
const v$ = useVuelidate(rules, grouptype);
//Khai báo biến
const isAdd = ref(true);
const selectedNodes = ref([]);
const filters = ref({});
const opition = ref({ search: "" });
const grouptypes = ref();
const treegrouptypes = ref();
const displayAddGroupType = ref(false);
const isFirst = ref(true);
const store = inject("store");
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportGroupType("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportGroupType("ExportExcelMau");
    },
  },
]);

//Khai báo function
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const handleFileUpload = (event) => {
  files = event.target.files;
};
//Show Modal
const showModalAddGroupType = () => {
  isAdd.value = true;
  submitted.value = false;
  grouptype.value = {
    Type_Name: "",
    STT: grouptypes.value.length + 1,
    Trangthai: true,
  };
  displayAddGroupType.value = true;
};
const closedisplayAddGroupType = () => {
  displayAddGroupType.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  loadGroupType(true);
};
const onSearch = () => {
  loadGroupType(true);
};
const loadGroupType = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "Sys_GroupType_List", par: [{ par: "s", va: opition.value.search }] },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        grouptypes.value = data;
      } else {
        grouptypes.value = [];
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

const editGroupType = (md) => {
  isAdd.value = false;
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddGroupType.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "Sys_GroupType_Get", par: [{ par: "GroupType_ID", va: md.GroupType_ID }] },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        grouptype.value = data[0][0];
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
  addGroupType();
};
const addGroupType = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: isAdd.value == false ? "put" : "post",
    url: baseURL + `/api/GroupType/${isAdd.value == false ? "Update_GroupType" : "Add_GroupType"}`,
    data: grouptype.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật loại nhóm thành công!");
        loadGroupType();
        closedisplayAddGroupType();
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

const delGroupType = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá loại nhóm này không!",
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
          .delete(baseURL + "/api/GroupType/Del_GroupType", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.GroupType_ID] : selectedNodes.value.map((x) => x.GroupType_ID),
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá loại nhóm thành công!");
              loadGroupType();
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

const upTrangthaiGroupType = (md) => {
  let ids = [md.GroupType_ID];
  let tts = [md.Trangthai];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/GroupType/Update_TrangthaiGroupType",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái loại nhóm thành công!");
        loadGroupType();
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

const exportGroupType = (method) => {
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
        excelname: "DANH SÁCH LOẠI NHÓM DUYỆT",
        proc: "Sys_GroupType_ListExport",
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
  grouptype.value[pcolor] = color.hex;
  if (!color.hex.includes("#")) opColor.value.hide();
};

onMounted(() => {
  //init
  loadGroupType(true);
  return {};
});
</script>
<template>
  <div class="main-layout flex-grow-1 p-2" v-if="store.getters.islogin">
    <DataTable
      class="w-full p-datatable-sm e-sm"
      :value="grouptypes"
      :paginator="grouptypes && grouptypes.length > 20"
      :loading="opition.loading"
      :rows="20"
      dataKey="GroupType_ID"
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
          <i class="pi pi-list"></i> Loại nhóm duyệt
          <span v-if="grouptypes">({{ grouptypes.length }})</span>
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
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
              @click="delGroupType"
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
              label="Thêm loại"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddGroupType"
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
        :sortable="true"
        field="STT"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:90px"
        bodyStyle="text-align:center;max-width:90px"
      ></Column>
      <Column
        field="GroupType_ID"
        :sortable="true"
        header="Mã loại"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      >
        <template #body="md">
          <span v-bind:class="'Trangthai' + md.data.Trangthai">{{
            md.data.GroupType_ID
          }}</span>
        </template>
      </Column>
      <Column field="Type_Name" header="Tên loại" :sortable="true">
        <template #body="md">
          <Chip
            :style="{ background: md.data.Maunen, color: md.data.Mauchu }"
            v-bind:label="md.data.Type_Name"
            class="mr-2 mb-2"
          />
        </template>
      </Column>
      <Column
        field="Trangthai"
        header="Trạng thái"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px"
        bodyStyle="text-align:center;max-width:120px"
      >
        <template #body="md">
          <Checkbox
            v-model="md.data.Trangthai"
            :binary="true"
            @change="upTrangthaiGroupType(md.data)"
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
            @click="editGroupType(md.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delGroupType(md.data)"
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
    header="Cập nhật loại nhóm duyệt"
    v-model:visible="displayAddGroupType"
    :style="{ width: '480px', zIndex: 2 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-4 text-left">Mã loại <span class="redsao">(*)</span></label>
          <InputText
            spellcheck="false"
            v-bind:disabled="!isAdd"
            class="col-8 ip36"
            v-model="grouptype.GroupType_ID"
            :class="{ 'p-invalid': v$.GroupType_ID.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.GroupType_ID.$invalid && submitted) || v$.GroupType_ID.$pending.$response"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-4 text-left"></label>
            <span class="col-8 pl-3">{{
              v$.GroupType_ID.required.$message
                .replace("Value", "Mã loại")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-4 text-left">Tên loại <span class="redsao">(*)</span></label>
          <InputText
            spellcheck="false"
            class="col-8 ip36"
            v-model="grouptype.Type_Name"
            :class="{ 'p-invalid': v$.Type_Name.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.Type_Name.$invalid && submitted) || v$.Type_Name.$pending.$response"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-4 text-left"></label>
            <span class="col-8 pl-3">{{
              v$.Type_Name.required.$message
                .replace("Value", "Tên nhóm")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-4 text-left">Màu chữ</label>
          <Button
            class="p-button-rounded p-button-outlined p-button-secondary col-2"
            :style="{
              backgroundColor: grouptype.Mauchu,
              color: grouptype.Mauchu ? 'transparent' : '#333',
              border: '1px solid #ccc',
            }"
            type="button"
            icon="pi pi-palette"
            @click="toggleColor($event, 'Mauchu')"
          />
          <OverlayPanel ref="opColor">
            <ColorPicker theme="dark" @changeColor="changeColor" :sucker-hide="true" />
          </OverlayPanel>
          <label class="col-4 text-right">Màu nền</label>
          <Button
            class="p-button-rounded p-button-outlined p-button-secondary col-2"
            :style="{
              backgroundColor: grouptype.Maunen,
              color: grouptype.Maunen ? 'transparent' : '#333',
              border: '1px solid #ccc',
            }"
            type="button"
            icon="pi pi-palette"
            @click="toggleColor($event, 'Maunen')"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-4 text-left">STT</label>
          <InputNumber class="col-2 ip36 p-0" v-model="grouptype.STT" />
        </div>
        <div class="field col-12 md:col-12">
          <label style="vertical-align: text-bottom" class="col-4 text-left"
            >Trạng thái</label
          >
          <InputSwitch v-model="grouptype.Trangthai" class="mt-1" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddGroupType"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
    </template>
  </Dialog>
</template>
<style scoped>
</style>
