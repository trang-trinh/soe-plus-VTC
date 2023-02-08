<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
//init Model
const chudetuan = ref({
  TenChude: "",
  STT: 1,
  Trangthai: true,
});
//Valid Form
const submitted = ref(false);
const rules = {
  TenChude: {
    required,
  },
};
const v$ = useVuelidate(rules, chudetuan);
//Khai báo biến
const isAdd = ref(true);
const selectedNodes = ref([]);
const filters = ref({});
const chudetuans = ref();
const displayAddChudeTuan = ref(false);
const isFirst = ref(true);
const toast = useToast();
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const opition = ref({
  search: "",
  PageNo: 1,
  PageSize: 50,
  Filteruser_id: null,
  user_id: store.getters.user.user_id,
});
const menuButs = ref();
const itemButs = ref([
  {
    label: "Import Excel",
    icon: "pi pi-upload",
    command: () => {
      importChudeTuan();
    },
  },
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: () => {
      exportChudeTuan("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: () => {
      exportChudeTuan("ExportExcelMau");
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
const showModalAddChudeTuan = () => {
  isAdd.value = true;
  submitted.value = false;
  chudetuan.value = {
    TenChude: "",
    STT: chudetuans.value.length + 1,
    Trangthai: true,
  };
  displayAddChudeTuan.value = true;
};
const closedisplayAddChudeTuan = () => {
  displayAddChudeTuan.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.PageNo = 1;
  opition.value.search = "";
  loadChudeTuan(true);
};
const onSearch = () => {
  loadChudeTuan(true);
};
const onPage = (event) => {
  opition.value.PageNo = event.page + 1;
  opition.value.PageSize = event.rows;
  loadChudeTuan(true);
};
const loadChudeTuan = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Sys_ChudeTuan_List",
        par: [{ par: "s ", va: opition.value.search }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        data[0].forEach((m, i) => {
          m.STT = opition.value.PageSize * (opition.value.PageNo - 1) + (i + 1);
        });
        chudetuans.value = data[0];
        opition.value.totalRecords = data[0].length;
      } else {
        chudetuans.value = [];
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
const editChudeTuan = (md) => {
  isAdd.value = false;
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddChudeTuan.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "Sys_ChudeTuan_Get", par: [{ par: "Chude_ID", va: md.Chude_ID }] },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        if (data[0][0].Khoihocs) {
          let arrs = [];
          data[0][0].Khoihocs.split(",").forEach((k) => {
            arrs.push(parseInt(k));
          });
          data[0][0].Khoihocs = arrs;
        }
        chudetuan.value = data[0][0];
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
  addChudeTuan();
};
const addChudeTuan = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let md = { ...chudetuan.value };
  if (md.Khoihocs) md.Khoihocs = md.Khoihocs.join(",");
  axios({
    method: isAdd.value == false ? "put" : "post",
    url:
      baseURL +
      `/api/ChudeTuan/${isAdd.value == false ? "Update_ChudeTuan" : "Add_ChudeTuan"}`,
    data: md,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật chủ đề tuần học thành công!");
        loadChudeTuan();
        closedisplayAddChudeTuan();
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

const delChudeTuan = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá chủ đề tuần học này không!",
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
          .delete(baseURL + "/api/ChudeTuan/Del_ChudeTuan", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.Chude_ID] : selectedNodes.value.map((x) => x.Chude_ID),
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá chủ đề tuần học thành công!");
              loadChudeTuan();
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

const upTrangthaiChudeTuan = (md) => {
  let ids = [md.Chude_ID];
  let tts = [md.Trangthai];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/ChudeTuan/Update_TrangthaiChudeTuan",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái chủ đề tuần học thành công!");
        loadChudeTuan();
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

const exportChudeTuan = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "Sys_ChudeTuan" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH CHỦ ĐỀ TUẦN HỌC",
        proc: "Sys_ChudeTuan_ListExport",
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
const filelist = ref([]);
const file = ref();
//Upload File
const onChange = () => {
  filelist.value = [...file.value.files];
};
const removeFile = (i) => {
  filelist.value.splice(i, 1);
};
const drop = (event) => {
  event.preventDefault();
  file.value.files = event.dataTransfer.files;
  onChange(); // Trigger the onChange event manually
};
const showImportExcel = ref(false);
const importChudeTuan = () => {
  showImportExcel.value = true;
  filelist.value = [];
  file.value = null;
};
const ImportExcel = () => {
  if (filelist.value.length == 0) {
    swal.fire({
      title: "Error!",
      text: "Vui lòng chọn file để import",
      icon: "error",
      confirmButtonText: "OK",
    });
    return false;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let fotmdata = new FormData();
  fotmdata.append("execel", filelist.value[0]);
  axios
    .post(baseURL + "/api/ChudeTuan/ImportExcel", fotmdata, config)
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Import chủ đề thành công!");
        showImportExcel.value = false;
        loadChudeTuan(true);
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
  loadChudeTuan(true);
  return {};
});
</script>
<template>
  <div
    :class="'main-layout ' + (opition.totalRecords > 10) + ' flex-grow-1 p-2'"
    v-if="store.getters.islogin"
  >
    <DataTable
      class="w-full p-datatable-sm e-sm"
      :value="chudetuans"
      :paginator="opition.totalRecords > opition.PageSize"
      :loading="opition.loading"
      :totalRecords="opition.totalRecords"
      :rows="opition.PageSize"
      dataKey="Chude_ID"
      :showGridlines="true"
      :rowHover="true"
      v-model:selection="selectedNodes"
      :filters="filters"
      filterMode="lenient"
      :rowsPerPageOptions="[10, 20, 50, 100]"
      :pageLinkSize="opition.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      currentPageReportTemplate=""
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-list"></i> Chủ đề tuần học
          <span v-if="chudetuans">({{ opition.totalRecords }})</span>
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
              @click="delChudeTuan()"
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
              label="Thêm chủ đề tuần học"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddChudeTuan"
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
      <!-- <Column
        field="STT"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:80px"
        bodyStyle="text-align:center;max-width:80px"
      ></Column> -->
      <Column
        field="Tuanhoc"
        :sortable="true"
        header="Tuần học"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px"
        bodyStyle="text-align:center;max-width:120px"
      ></Column>
      <Column field="TenChude" header="Tên chủ đề tuần học" :sortable="true"> </Column>
      <Column
        field="Trangthai"
        header="Trạng thái"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:90px"
        bodyStyle="text-align:center;max-width:90px"
      >
        <template #body="md">
          <Checkbox
            v-model="md.data.Trangthai"
            :binary="true"
            @change="upTrangthaiChudeTuan(md.data)"
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
            @click="editChudeTuan(md.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delChudeTuan(md.data)"
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
    header="Cập nhật chủ đề tuần học"
    v-model:visible="displayAddChudeTuan"
    :style="{ width: '720px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12" v-if="!isAdd">
          <label class="col-2 text-left">Mã</label>
          <InputNumber
            :disabled="!isAdd"
            spellcheck="false"
            :useGrouping="false"
            class="col-10 ip36 p-0"
            v-model="chudetuan.Chude_ID"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tên <span class="redsao">(*)</span></label>
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="chudetuan.TenChude"
            :class="{ 'p-invalid': v$.TenChude.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.TenChude.$invalid && submitted) || v$.TenChude.$pending.$response"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.TenChude.required.$message
                .replace("Value", "Tên")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tuần học</label>
          <InputNumber class="col-4 ip36 p-0" v-model="chudetuan.Tuanhoc" />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-4 ip36 p-0" v-model="chudetuan.STT" />
          <label style="vertical-align: text-bottom" class="col-2 text-right"
            >Trạng thái</label
          >
          <InputSwitch v-model="chudetuan.Trangthai" class="mt-1" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddChudeTuan"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
    </template>
  </Dialog>
  <Dialog
    header="Import Excel"
    v-model:visible="showImportExcel"
    :style="{ width: '480px', zIndex: 1000 }"
    :autoZIndex="false"
    :modal="true"
  >
    <div class="flex items-center justify-center text-center">
      <div class="dragFile" @dragover="dragover" @dragleave="dragleave" @drop="drop">
        <input
          type="file"
          multiple
          name="fields[assetsFieldHandle][]"
          id="assetsFieldHandle"
          class="w-px h-px opacity-0 overflow-hidden absolute"
          @change="onChange"
          ref="file"
          accept=".xls,.xlsx"
        />
        <i
          class="pi pi-cloud-upload"
          style="font-size: 4rem; margin: 10px; color: #aaa"
        ></i>
        <label for="assetsFieldHandle" class="block cursor-pointer">
          <div>
            Kéo file hoặc
            <span class="underline" style="color: #0d89ec">click vào đây</span> để upload
            File
          </div>
        </label>
      </div>
    </div>
    <div v-for="(f, indexF) in filelist" :key="indexF" class="mt-3 mb-2">
      <Image
        v-bind:src="
          'src/assets/image/file/' +
          f.name.substring(f.name.lastIndexOf('.') + 1) +
          '.png'
        "
        height="16"
      />
      <span class="ml-1">{{ f.name }}</span>
    </div>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="showImportExcel = false"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Thực hiện" icon="pi pi-upload" @click="ImportExcel" />
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
