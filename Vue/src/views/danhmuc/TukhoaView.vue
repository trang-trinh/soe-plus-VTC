<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
//init Model
const tukhoa = ref({
  Ten: "",
  STT: 1,
  Trangthai: true,
});
//Valid Form
const submitted = ref(false);
const rules = {
  Tukhoa: {
    required,
    $errors: [
      {
        $property: "Ten",
        $validator: "required",
        $message: "Tên từ khoá không được để trống!",
      },
    ],
  },
};
const v$ = useVuelidate(rules, tukhoa);
//Khai báo biến
const store = inject("store");
const isAdd = ref(true);
const selectedNodes = ref([]);
const filters = ref({});
const opition = ref({ search: "" });
const tukhoas = ref();
const treetukhoas = ref();
const displayAddTukhoa = ref(false);
const isFirst = ref(true);
let files = [];
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
    command: () => {
      exportTukhoa("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: () => {
      exportTukhoa("ExportExcelMau");
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
const showModalAddTukhoa = () => {
  isAdd.value = true;
  submitted.value = false;
  tukhoa.value = {
    Ten: "",
    STT: tukhoas.value.length + 1,
    Trangthai: true,
  };
  displayAddTukhoa.value = true;
};
const chonanh = (id) => {
  document.getElementById(id).click();
};
const closedisplayAddTukhoa = () => {
  displayAddTukhoa.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  loadTukhoa(true);
};
const onSearch = () => {
  loadTukhoa(true);
};
const loadTukhoa = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "Plan_Tukhoa_List", par: [{ par: "s", va: opition.value.search }] },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        tukhoas.value = data;
      } else {
        tukhoas.value = [];
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
const editTukhoa = (md) => {
  isAdd.value = false;
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddTukhoa.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "Plan_Tukhoa_Get", par: [{ par: "Tukhoa_ID", va: md.Tukhoa_ID }] },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        tukhoa.value = data[0][0];
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
  addTukhoa();
};
const addTukhoa = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: isAdd.value == false ? "put" : "post",
    url: baseURL + `/api/Tukhoa/${isAdd.value == false ? "Update_Tukhoa" : "Add_Tukhoa"}`,
    data: tukhoa.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật từ khoá thành công!");
        loadTukhoa();
        closedisplayAddTukhoa();
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

const delTukhoa = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá từ khoá này không!",
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
          .delete(baseURL + "/api/Tukhoa/Del_Tukhoa", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data:
              md != null ? [md.Tukhoa_ID] : selectedNodes.value.map((x) => x.Tukhoa_ID),
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá từ khoá thành công!");
              loadTukhoa();
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

const upTrangthaiTukhoa = (md) => {
  let ids = [md.Tukhoa_ID];
  let tts = [md.Trangthai];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/Tukhoa/Update_TrangthaiTukhoa",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái từ khoá thành công!");
        loadTukhoa();
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

const exportTukhoa = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "Plan_Tukhoa" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH MỨC ĐỘ",
        proc: "Plan_Tukhoa_ListExport",
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
  loadTukhoa(true);
  return {
    displayAddTukhoa,
    isFirst,
    opition,
    showModalAddTukhoa,
    closedisplayAddTukhoa,
    addTukhoa,
    editTukhoa,
    onSearch,
    delTukhoa,
    handleFileUpload,
    chonanh,
    v$,
    handleSubmit,
    submitted,
    basedomainURL,
    filters,
    onRefersh,
    treetukhoas,
    itemButs,
    menuButs,
    toggleExport,
    selectedNodes,
    upTrangthaiTukhoa,
  };
});
</script>
<template>
  <div class="main-layout flex-grow-1 p-2" v-if="store.getters.islogin">
    <DataTable
      class="w-full p-datatable-sm e-sm"
      :value="tukhoas"
      :paginator="tukhoas && tukhoas.length > 20"
      :loading="opition.loading"
      :rows="20"
      dataKey="Tukhoa_ID"
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
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-list"></i> Từ khoá
          <span v-if="tukhoas">({{ tukhoas.length }})</span>
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
              @click="delTukhoa"
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
              label="Thêm từ khoá"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddTukhoa"
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
        field="Tukhoa_ID"
        :sortable="true"
        header="Mã"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      ></Column>
      <Column field="Ten" header="Tên từ khoá" :sortable="true">
        <template #body="md">
          <Chip
            :style="{ background: md.data.Maunen, color: md.data.Mauchu }"
            v-bind:label="md.data.Tukhoa"
            class="mr-2 mb-2"
          />
        </template>
      </Column>
      <Column
        field="Module"
        :sortable="true"
        header="Module"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      ></Column>
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
            @change="upTrangthaiTukhoa(md.data)"
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
            @click="editTukhoa(md.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delTukhoa(md.data)"
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
    header="Cập nhật từ khoá công việc"
    v-model:visible="displayAddTukhoa"
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
            :disabled="true"
            class="col-3 ip36"
            v-model="tukhoa.Tukhoa_ID"
          />
          <label class="col-3 text-right">Module</label>
          <InputText spellcheck="false" class="col-3 ip36" v-model="tukhoa.Module" />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left">Từ khoá <span class="redsao">(*)</span></label>
          <InputText
            spellcheck="false"
            class="col-9 ip36"
            v-model="tukhoa.Tukhoa"
            :class="{ 'p-invalid': v$.Tukhoa.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.Tukhoa.$invalid && submitted) || v$.Tukhoa.$pending.$response"
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
            v-model="tukhoa.Mauchu"
          />
          <label class="col-3 text-right">Màu nền</label>
          <InputText
            type="color"
            spellcheck="false"
            class="col-3"
            v-model="tukhoa.Maunen"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left">STT</label>
          <InputNumber class="col-3 ip36 p-0" v-model="tukhoa.STT" />
          <label style="vertical-align: text-bottom" class="col-3 text-right"
            >Trạng thái</label
          >
          <InputSwitch v-model="tukhoa.Trangthai" class="mt-1" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddTukhoa"
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
