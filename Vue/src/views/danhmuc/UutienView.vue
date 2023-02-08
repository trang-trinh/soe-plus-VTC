<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import arrIcons from "../../assets/json/icons.json";
//init Model
const uutien = ref({
  Ten: "",
  STT: 1,
  Trangthai: true,
});
//Valid Form
const submitted = ref(false);
const rules = {
  Uutien: {
    required,
    $errors: [
      {
        $property: "Uutien",
        $validator: "required",
        $message: "Mã ưu tiên không được để trống!",
      },
    ],
  },
  Ten: {
    required,
    $errors: [
      {
        $property: "Ten",
        $validator: "required",
        $message: "Tên ưu tiên không được để trống!",
      },
    ],
  },
};
const v$ = useVuelidate(rules, uutien);
//Khai báo biến
const isAdd = ref(true);
const selectedNodes = ref([]);
const filters = ref({});
const opition = ref({ search: "" });
const uutiens = ref();
const treeuutiens = ref();
const displayAddUutien = ref(false);
const isFirst = ref(true);
let files = [];
const toast = useToast();
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: () => {
      exportUutien("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: () => {
      exportUutien("ExportExcelMau");
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
const showModalAddUutien = () => {
  isAdd.value = true;
  submitted.value = false;
  uutien.value = {
    Ten: "",
    STT: uutiens.value.length + 1,
    Trangthai: true,
  };
  displayAddUutien.value = true;
};
const chonanh = (id) => {
  document.getElementById(id).click();
};
const closedisplayAddUutien = () => {
  displayAddUutien.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  loadUutien(true);
};
const onSearch = () => {
  loadUutien(true);
};
const loadUutien = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "Sys_Uutien_List", par: [{ par: "s", va: opition.value.search }] },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        uutiens.value = data;
      } else {
        uutiens.value = [];
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
const editUutien = (md) => {
  isAdd.value = false;
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddUutien.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "Sys_Uutien_Get", par: [{ par: "Uutien", va: md.Uutien }] },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        uutien.value = data[0][0];
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
  addUutien();
};
const addUutien = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: isAdd.value == false ? "put" : "post",
    url: baseURL + `/api/Uutien/${isAdd.value == false ? "Update_Uutien" : "Add_Uutien"}`,
    data: uutien.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật ưu tiên thành công!");
        loadUutien();
        closedisplayAddUutien();
      } else {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch(() => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};

const delUutien = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá ưu tiên này không!",
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
          .delete(baseURL + "/api/Uutien/Del_Uutien", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.Uutien] : selectedNodes.value.map((x) => x.Uutien),
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá ưu tiên thành công!");
              loadUutien();
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

const upTrangthaiUutien = (md) => {
  let ids = [md.Uutien];
  let tts = [md.Trangthai];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/Uutien/Update_TrangthaiUutien",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái ưu tiên thành công!");
        loadUutien();
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

const exportUutien = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "Sys_Uutien" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH ƯU TIÊN",
        proc: "Sys_Uutien_ListExport",
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

onMounted(() => {
  //init
  loadUutien(true);
  return {
    displayAddUutien,
    isFirst,
    opition,
    showModalAddUutien,
    closedisplayAddUutien,
    addUutien,
    editUutien,
    onSearch,
    delUutien,
    handleFileUpload,
    chonanh,
    v$,
    handleSubmit,
    submitted,
    basedomainURL,
    filters,
    onRefersh,
    treeuutiens,
    itemButs,
    menuButs,
    toggleExport,
    selectedNodes,
    upTrangthaiUutien,
  };
});
</script>
<template>
  <div class="main-layout flex-grow-1 p-2" v-if="store.getters.islogin">
    <DataTable
      class="w-full p-datatable-sm e-sm"
      :value="uutiens"
      :paginator="uutiens && uutiens.length > 20"
      :loading="opition.loading"
      :rows="20"
      dataKey="Uutien"
      :showGridlines="true"
      :rowHover="true"
      v-model:selection="selectedNodes"
      :filters="filters"
      filterMode="lenient"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
      :rowsPerPageOptions="[10, 25, 50]"
      currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-list"></i> Mức độ ưu tiên công việc
          <span v-if="uutiens">({{ uutiens.length }})</span>
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
              @click="delUutien"
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
              label="Thêm ưu tiên"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddUutien"
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
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      ></Column>
      <Column
        field="Uutien"
        :sortable="true"
        header="Mã ưu tiên"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      ></Column>
      <Column field="Ten" header="Tên ưu tiên" :sortable="true">
        <template #body="md">
          <Chip
            :style="{ background: md.data.Maunen, color: md.data.Mauchu }"
            v-bind:label="md.data.Ten"
            class="mr-2 mb-2"
          />
        </template>
      </Column>
      <Column
        field="Icon"
        header="Icon"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:80px"
        bodyStyle="text-align:center;max-width:80px"
      >
        <template #body="md">
          <i :class="md.data.icon" :style="{ color: md.data.Maunen }"></i>
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
            @change="upTrangthaiUutien(md.data)"
          />
        </template>
      </Column>
      <Column
        headerClass="text-center"
        bodyClass="text-center"
        headerStyle="text-align:center;max-width:120px"
        bodyStyle="text-align:center;max-width:120px"
      >
        <template #header> </template>
        <template #body="md">
          <Button
            type="button"
            icon="pi pi-pencil"
            class="p-button-rounded p-button-sm p-button-info"
            style="margin-right: 0.5rem"
            @click="editUutien(md.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delUutien(md.data)"
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
  <Dialog
    header="Cập nhật ưu tiên công việc"
    v-model:visible="displayAddUutien"
    :style="{ width: '640px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left">Mã <span class="redsao">(*)</span></label>
          <InputText
            spellcheck="false"
            v-bind:disabled="!isAdd"
            class="col-3 ip36"
            v-model="uutien.Uutien"
            :class="{ 'p-invalid': v$.Uutien.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.Uutien.$invalid && submitted) || v$.Uutien.$pending.$response"
          class="col-9 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left"></label>
            <span class="col-9 pl-3">{{
              v$.Uutien.required.$message
                .replace("Value", "Mã")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left"
            >Mức ưu tiên <span class="redsao">(*)</span></label
          >
          <InputText
            spellcheck="false"
            class="col-9 ip36"
            v-model="uutien.Ten"
            :class="{ 'p-invalid': v$.Ten.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.Ten.$invalid && submitted) || v$.Ten.$pending.$response"
          class="col-9 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left"></label>
            <span class="col-9 pl-3">{{
              v$.Ten.required.$message
                .replace("Value", "Tên")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left">Màu chữ</label>
          <InputText
            type="color"
            spellcheck="false"
            class="col-3"
            v-model="uutien.Mauchu"
          />
          <label class="col-3 text-right">Màu nền</label>
          <InputText
            type="color"
            spellcheck="false"
            class="col-3"
            v-model="uutien.Maunen"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left">Icon</label>
          <AutoComplete
            class="col-9 ip36 p-0"
            v-model="uutien.icon"
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
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left">STT</label>
          <InputNumber class="col-3 ip36 p-0" v-model="uutien.STT" />
          <label style="vertical-align: text-bottom" class="col-3 text-right"
            >Trạng thái</label
          >
          <InputSwitch v-model="uutien.Trangthai" class="mt-1" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddUutien"
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
