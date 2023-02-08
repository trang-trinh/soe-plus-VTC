<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
//init Model
const topicflag = ref({
  STT: 1,
  Trangthai: true,
});
//Valid Form
const submitted = ref(false);
const rules = {
  TopicFlag_ID: { required },
  TopicFlag_Name: { required },
};
const v$ = useVuelidate(rules, topicflag);
//Khai báo biến
const isAdd = ref(true);
const selectedNodes = ref([]);
const filters = ref({});
const opition = ref({ search: "" });
const topicflags = ref();
const displayAddTopicFlag = ref(false);
const isFirst = ref(true);
const toast = useToast();
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const BearerToken = { Authorization: "Bearer " + store.getters.token };
const config = {
  headers: BearerToken,
};
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
    text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
    icon: "error",
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
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: () => {
      exportTopicFlag("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: () => {
      exportTopicFlag("ExportExcelMau");
    },
  },
]);

//Khai báo function
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
//Show Modal
const closedisplayAddTopicFlag = () => {
  displayAddTopicFlag.value = false;
};
const showModalAddTopicFlag = () => {
  isAdd.value = true;
  submitted.value = false;
  topicflag.value = {
    STT: topicflags.value.length + 1,
    Trangthai: true,
  };
  displayAddTopicFlag.value = true;
};

//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  loadTopicFlag(true);
};
const onSearch = () => {
  loadTopicFlag(true);
};
const loadTopicFlag = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "CMS_TopicFlag_List", par: [{ par: "s", va: opition.value.search }] },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        topicflags.value = data;
      } else {
        topicflags.value = [];
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
const editTopicFlag = (md) => {
  isAdd.value = false;
  submitted.value = false;
  swalLoadding();
  displayAddTopicFlag.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "CMS_TopicFlag_Get",
        par: [{ par: "TopicFlag_ID", va: md.TopicFlag_ID }],
      },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        topicflag.value = data[0][0];
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
  addTopicFlag();
};
const addTopicFlag = () => {
  swalLoadding();
  axios({
    method: isAdd.value == false ? "put" : "post",
    url:
      baseURL +
      "/api/TopicFlag/" +
      (isAdd.value == false ? "Update_TopicFlag" : "Add_TopicFlag"),
    data: topicflag.value,
    headers: BearerToken,
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật loại biên mục thành công!");
        loadTopicFlag();
        closedisplayAddTopicFlag();
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
      swalMessage("Error!", "error", "Có lỗi xảy ra, vui lòng kiểm tra lại!");
    });
};

const delTopicFlag = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá loại biên mục này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        swalLoadding();
        axios
          .delete(baseURL + "/api/TopicFlag/Del_TopicFlag", {
            headers: BearerToken,
            data:
              md != null
                ? [md.TopicFlag_ID]
                : selectedNodes.value.map((x) => x.TopicFlag_ID),
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá loại biên mục thành công!");
              loadTopicFlag();
              if (!md) selectedNodes.value = [];
            } else {
              swalMessage("Error!", "error", response.data.ms);
            }
          })
          .catch((error) => {
            swal.close();
            if (error.status === 401) {
              errorMessage();
            }
          });
      }
    });
};

const upTrangthaiTopicFlag = (md) => {
  let ids = [md.TopicFlag_ID];
  let tts = [md.Trangthai];
  swalLoadding();
  axios({
    method: "put",
    url: baseURL + "/api/TopicFlag/Update_TrangthaiTopicFlag",
    data: { ids: ids, tts: tts },
    headers: BearerToken,
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái loại biên mục thành công!");
        loadTopicFlag();
        if (!md) selectedNodes.value = [];
      } else {
        swalMessage("Error!", "error", response.data.ms);
      }
    })
    .catch((error) => {
      swal.close();
      if (error.status === 401) {
        errorMessage();
      }
    });
};

const exportTopicFlag = (method) => {
  swalLoadding();
  let par = [{ par: "name", va: "DM_TopicFlag" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH LOẠI BIÊN MỤC",
        proc: "DM_TopicFlag_ListExport",
        par: par,
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Kết xuất Loại biên mục thành công!");
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
        swalMessage("Error!", "error", response.data.ms);
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        errorMessage();
      }
    });
};
const filteredItems = ref([]);

onMounted(() => {
  //init
  loadTopicFlag(true);
  return {};
});
</script>
<template>
  <div class="main-layout flex-grow-1 p-2" v-if="store.getters.islogin">
    <DataTable
      class="w-full p-datatable-sm e-sm"
      :value="topicflags"
      :paginator="topicflags && topicflags.length > 20"
      :loading="opition.loading"
      :rows="20"
      dataKey="TopicFlag_ID"
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
          <i class="pi pi-list"></i> Loại biên mục
          <span v-if="topicflags">({{ topicflags.length }})</span>
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
              @click="delTopicFlag()"
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
              label="Thêm loại biên mục"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddTopicFlag"
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
        field="TopicFlag_ID"
        :sortable="true"
        header="Mã loại"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      ></Column>
      <Column field="TopicFlag_Name" header="Tên loại biên mục" :sortable="true"></Column>
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
            @change="upTrangthaiTopicFlag(md.data)"
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
            @click="editTopicFlag(md.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delTopicFlag(md.data)"
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
    header="Cập nhật loại biên mục"
    v-model:visible="displayAddTopicFlag"
    :style="{ width: '540px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left">Mã loại <span class="redsao">(*)</span></label>
          <InputNumber class="col-9 ip36 p-0" v-model="topicflag.TopicFlag_ID" />
        </div>

        <small
          v-if="
            (v$.TopicFlag_ID.$invalid && submitted) || v$.TopicFlag_ID.$pending.$response
          "
          class="col-9 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left"></label>
            <span class="col-9 pl-3">{{
              v$.TopicFlag_ID.required.$message
                .replace("Value", "Mã loại")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >

        <div class="field col-12 md:col-12">
          <label class="col-3 text-left">Tên loại <span class="redsao">(*)</span></label>

          <InputText
            spellcheck="false"
            class="col-9 ip36"
            v-model="topicflag.TopicFlag_Name"
            :class="{ 'p-invalid': v$.TopicFlag_Name.$invalid && submitted }"
          />
        </div>

        <small
          v-if="
            (v$.TopicFlag_Name.$invalid && submitted) ||
            v$.TopicFlag_Name.$pending.$response
          "
          class="col-9 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left"></label>
            <span class="col-9 pl-3">{{
              v$.TopicFlag_Name.required.$message
                .replace("Value", "Tên loại")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >

        <div class="field col-12 md:col-12">
          <label class="col-3 text-left">Nhóm</label>
          <InputText
            spellcheck="false"
            class="col-9 ip36"
            v-model="topicflag.Group_Name"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left">Mô tả</label>
          <InputText spellcheck="false" class="col-9 ip36" v-model="topicflag.Des" />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left">Link</label>
          <InputText spellcheck="false" class="col-9 ip36" v-model="topicflag.Url" />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left">STT</label>
          <InputNumber class="col-3 ip36 p-0" v-model="topicflag.STT" />
          <label class="col-3 text-right">Trạng thái</label>
          <InputSwitch v-model="topicflag.Trangthai" style="vertical-align: bottom" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddTopicFlag"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
    </template>
  </Dialog>
</template>

<style scoped></style>
