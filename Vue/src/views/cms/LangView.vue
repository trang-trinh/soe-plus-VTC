<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import arrIcons from "../../assets/json/icons.json";
import router from "@/router";
//init Model
const lang = ref({
  Name: "",
  STT: 1,
  Trangthai: true,
  TypeLang: 0,
});
//Valid Form
const submitted = ref(false);
const rules = {
  Name: {
    required,
  },
};
const v$ = useVuelidate(rules, lang);
//Khai báo biến
const store = inject("store");
const selectedKey = ref();
const selectedNodes = ref([]);
const filters = ref({});
const opition = ref({
  search: "",
  user_id: store.getters.user.user_id,
});
const langs = ref([]);
const displayAddLang = ref(false);
const isFirst = ref(true);
let files = {};
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
      exportLang("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportLang("ExportExcelMau");
    },
  },
]);

//Khai báo function
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const onNodeSelect = (node) => {
  selectedNodes.value.push(node.data.Lang_ID);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(selectedNodes.value.indexOf(node.data.Lang_ID), 1);
};
const handleFileUpload = (event, ia) => {
  files[ia] = event.target.files[0];
  var output = document.getElementById(ia);
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
//Show Modal
const showModalAddLang = () => {
  submitted.value = false;
  lang.value = {
    Name: "",
    STT: langs.value.length + 1,
    Trangthai: true,
    Hienhanh: false,
  };
  displayAddLang.value = true;
};
const chonanh = (id) => {
  document.getElementById(id).click();
};
const closedisplayAddLang = () => {
  displayAddLang.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  loadLang(true);
};
const onSearch = () => {
  loadLang(true);
};
const loadLang = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "CMS_Lang_List",
        par: [
          { par: "s", va: opition.value.search },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        langs.value = data[0];
        opition.totalRecords=data[0].length;
      } else {
        langs.value = [];
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
const editLang = (md) => {
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddLang.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "CMS_Lang_Get", par: [{ par: "Lang_ID", va: md.Lang_ID }] },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        lang.value = data[0][0];
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
  addLang();
};
const addLang = () => {
  let formData = new FormData();
  for (var k in files) {
    let file = files[k];
    formData.append(k, file);
  }
  formData.append("model", JSON.stringify(lang.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: lang.value.Lang_ID ? "put" : "post",
    url: baseURL + `/api/Lang/${lang.value.Lang_ID ? "Update_Lang" : "Add_Lang"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật ngôn ngữ thành công!");
        loadLang();
        closedisplayAddLang();
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

const delLang = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá ngôn ngữ này không!",
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
          .delete(baseURL + "/api/Lang/Del_Lang", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.Lang_ID] : selectedNodes.value,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá ngôn ngữ thành công!");
              loadLang();
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
const upTrangthaiLang = (md) => {
  let ids = [md.Lang_ID];
  let tts = [md.Trangthai];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/Lang/Update_TrangthaiLang",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái ngôn ngữ thành công!");
        loadLang();
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

const upTrangthaiLangHienhanh = (md) => {
  let ids = [md.Lang_ID];
  let tts = [md.Hienhanh];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/Lang/Update_TrangthaiLangHienhanh",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái hiện hành ngôn ngữ thành công!");
        loadLang();
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

const exportLang = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "CMS_Lang" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH NGÔN NGỮ",
        proc: "CMS_Lang_ListExport",
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
  loadLang(true);
});
</script>
<template>
  <div class="main-layout flex-grow-1 p-2" v-if="store.getters.islogin">
    <DataTable
      :value="langs"
      v-model:selectionKeys="selectedKey"
      :loading="opition.loading"
      :filters="filters"
      :showGridlines="true"
      selectionMode="checkbox"
      filterMode="lenient"
      class="p-datatable-sm e-sm"
      :paginator="opition.totalRecords > opition.PageSize"
      :rows="opition.PageSize"
      :pageLinkSize="opition.PageSize"
      :scrollable="true"
      scrollHeight="flex"
    >
      <template #header>
        <h3 class="lang-title mt-0 ml-1 mb-2">
          <i class="pi pi-microsoft"></i> Danh sách ngôn ngữ
        </h3>
        <Toolbar class="w-full custoolbar">
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
              @click="delLang"
            />
            <Button
              label="Export"
              icon="pi pi-file-excel"
              class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            />
            <Menu vị id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" />
            <Button
              label="Thêm ngôn ngữ"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddLang"
            />
          </template>
        </Toolbar>
      </template>
      <Column
        field="Code"
        :sortable="true"
        header="Mã"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      >
      </Column>
      <Column
        field="Icon"
        header="Icon"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:80px"
        bodyStyle="text-align:center;max-width:80px"
      >
        <template #body="md">
          <img :src="basedomainURL + md.data.icon" height="32">
        </template>
      </Column>
      <Column field="Name" header="Tên" :sortable="true"> </Column>
       <Column
        field="Hienhanh"
        header="Hiện hành"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:110px"
        bodyStyle="text-align:center;max-width:110px"
      >
        <template #body="md">
          <InputSwitch
            v-model="md.data.Hienhanh"
            :binary="true"
            @change="upTrangthaiLangHienhanh(md.data)"
          />
        </template>
      </Column>
      <Column
        field="Trangthai"
        header="Trạng thái"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:110px"
        bodyStyle="text-align:center;max-width:110px"
      >
        <template #body="md">
          <Checkbox
            v-model="md.data.Trangthai"
            :binary="true"
            @change="upTrangthaiLang(md.data)"
          />
        </template>
      </Column>
      <Column
        headerClass="text-center"
        headerStyle="text-align:center;max-width:110px"
        bodyStyle="text-align:center;max-width:110px"
      >
        <template #header> </template>
        <template #body="md">
          <Button
            type="button"
            icon="pi pi-pencil"
            class="p-button-rounded p-button-sm p-button-info"
            style="margin-right: 0.5rem"
            @click="editLang(md.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delLang(md.data)"
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
    header="Cập nhật ngôn ngữ"
    v-model:visible="displayAddLang"
    :style="{ width: '640px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12" v-if="!isAdd">
          <label class="col-2 text-left">Mã</label>
          <InputText
            spellcheck="false"
            :useGrouping="false"
            class="col-10 ip36"
            v-model="lang.Code"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left"
            >Tên <span class="redsao">(*)</span></label
          >
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="lang.Name"
            :class="{ 'p-invalid': v$.Name.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.Name.$invalid && submitted) || v$.Name.$pending.$response"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.Name.required.$message
                .replace("Value", "Tên ngôn ngữ")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12 flex">
          <label class="col-2">Icon</label>
          <div class="col-4 p-0">
            <div class="inputanh" @click="chonanh('AnhLang')">
              <img
                id="LogoLang"
                v-bind:src="
                  lang.icon ? basedomainURL + lang.icon : '/src/assets/image/noimg.jpg'
                "
              />
            </div>
            <input
              class="ipnone"
              id="AnhLang"
              type="file"
              accept="image/*"
              @change="handleFileUpload($event, 'LogoLang')"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-2 ip36 p-0" v-model="lang.STT" />
          <label class="col-2 text-right"
            >Trạng thái</label
          >
          <InputSwitch v-model="lang.Trangthai" style="vertical-align: text-bottom"/>
          <label class="col-2 text-right"
            >Hiện hành</label
          >
          <InputSwitch v-model="lang.Hienhanh" style="vertical-align: text-bottom"/>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddLang"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
    </template>
  </Dialog>
</template>
<style scoped>
.classlang {
  background-color: aliceblue;
}
span.langtrue {
  font-weight: 500;
}
.chip0 {
  background-color: #4285f4;
  color: #fff;
  font-size: 0.875rem;
  padding: 5px 10px;
}
.chip1 {
  background-color: #689f38;
  color: #fff;
  font-size: 0.875rem;
  padding: 5px 10px;
}
.chip2 {
  background-color: #607d8b;
  color: #fff;
  font-size: 0.875rem;
  padding: 5px 10px;
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
