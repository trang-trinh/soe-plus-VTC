<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
//init Model
const bosach = ref({
  TenBosach: "",
  STT: 1,
  Trangthai: true,
});
//Valid Form
const submitted = ref(false);
const rules = {
  TenBosach: {
    required,
  },
};
const v$ = useVuelidate(rules, bosach);
//Khai báo biến
const isAdd = ref(true);
const selectedNodes = ref([]);
const filters = ref({});
const bosachs = ref();
const displayAddBosach = ref(false);
const isFirst = ref(true);
const toast = useToast();
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const tdTypes = [
  { value: 0, text: "Tính điểm" },
  { value: 1, text: "Nhận xét" },
  { value: 2, text: "Khác" },
];
const opition = ref({
  search: "",
  PageNo: 1,
  PageSize: 20,
  Filteruser_id: null,
  user_id: store.getters.user.user_id,
});
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: () => {
      exportBosach("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: () => {
      exportBosach("ExportExcelMau");
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
const showModalAddBosach = () => {
  selectAll.value = false;
  isAdd.value = true;
  submitted.value = false;
  bosach.value = {
    TenBosach: "",
    STT: bosachs.value.length + 1,
    Trangthai: true
  };
  displayAddBosach.value = true;
};
const closedisplayAddBosach = () => {
  displayAddBosach.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.PageNo = 1;
  opition.value.search = "";
  loadBosach(true);
};
const onSearch = () => {
  loadBosach(true);
};
const onPage = (event) => {
  opition.value.PageNo = event.page + 1;
  opition.value.PageSize = event.rows;
  loadBosach(true);
};
const loadBosach = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Sys_Bosach_List",
        par: [
          { par: "s ", va: opition.value.search },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        data[0].forEach((m, i) => {
          m.STT = opition.value.PageSize * (opition.value.PageNo - 1) + (i + 1);
          if (m.Khoihocs) {
            m.Khoihocs = m.Khoihocs.split(",");
          }
        });
        bosachs.value = data[0];
        opition.value.totalRecords = data[0].length;
      } else {
        bosachs.value = [];
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
const editBosach = (md) => {
  isAdd.value = false;
  selectAll.value=false;
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddBosach.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "Sys_Bosach_Get", par: [{ par: "Bosach_ID", va: md.Bosach_ID }] },
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
        bosach.value = data[0][0];
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
  addBosach();
};
const addBosach = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let md = { ...bosach.value };
  if (md.Khoihocs) md.Khoihocs = md.Khoihocs.join(",");
  axios({
    method: isAdd.value == false ? "put" : "post",
    url: baseURL + `/api/Bosach/${isAdd.value == false ? "Update_Bosach" : "Add_Bosach"}`,
    data: md,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật bộ sách thành công!");
        loadBosach();
        closedisplayAddBosach();
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

const delBosach = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá bộ sách này không!",
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
          .delete(baseURL + "/api/Bosach/Del_Bosach", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data:
              md != null ? [md.Bosach_ID] : selectedNodes.value.map((x) => x.Bosach_ID),
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá bộ sách thành công!");
              loadBosach();
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

const upTrangthaiBosach = (md) => {
  let ids = [md.Bosach_ID];
  let tts = [md.Trangthai];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/Bosach/Update_TrangthaiBosach",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái bộ sách thành công!");
        loadBosach();
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

const exportBosach = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "Sys_Bosach" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH BỘ SÁCH",
        proc: "Sys_Bosach_ListExport",
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
const tudiens = ref([]);
const loadTudien = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Sys_Bosach_Tudiens",
        par: [{ par: "user_id", va: opition.value.user_id }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        data[1].forEach((ch) => {
          ch.items = data[0].filter((x) => x.Caphoc_ID == ch.value);
          ch.chon = false;
        });
        data[1] = data[1].filter((x) => x.items.length > 0);
        tudiens.value = data;
      } else {
        tudiens.value = [];
      }
    })
    .catch((error) => {});
};
const selectAll = ref(false);
const checkAllkhoi = (event) => {
  selectAll.value = event.checked;
  if (event.checked) {
    bosach.value.Khoihocs = [];
    tudiens.value[1].forEach((el) => {
      el.items.forEach((kk) => {
        bosach.value.Khoihocs.push(kk.value);
      });
    });
  } else {
    bosach.value.Khoihocs = [];
  }
};
const checkkhoi = (khoi) => {
  var arr = bosach.value.Khoihocs || [];
  if (khoi.chon) {
    khoi.items.forEach((kk) => {
      arr.push(kk.value);
    });
  } else {
    arr = arr.filter((x) => khoi.items.findIndex((kk) => kk.value == x).length == -1);
  }
  bosach.value.Khoihocs = arr;
};
onMounted(() => {
  //init
  loadBosach(true);
  loadTudien();
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
      :value="bosachs"
      :paginator="opition.totalRecords > opition.PageSize"
      :loading="opition.loading"
      :totalRecords="opition.totalRecords"
      :rows="opition.PageSize"
      dataKey="Bosach_ID"
      :showGridlines="true"
      :rowHover="true"
      v-model:selection="selectedNodes"
      :filters="filters"
      filterMode="lenient"
      :rowsPerPageOptions="[10, 20, 40]"
      :lazy="true"
      @page="onPage($event)"
      :pageLinkSize="opition.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      currentPageReportTemplate=""
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-list"></i> Bộ sách
          <span v-if="bosachs">({{ opition.totalRecords }})</span>
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
              @click="delBosach()"
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
              label="Thêm bộ sách"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddBosach"
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
        headerStyle="text-align:center;max-width:80px"
        bodyStyle="text-align:center;max-width:80px"
      ></Column>
      <Column
        field="Bosach_ID"
        header="Mã"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:80px"
        bodyStyle="text-align:center;max-width:80px"
      ></Column>
      <Column
        field="TenBosach"
        header="Tên bộ sách"
        :sortable="true"
        headerStyle="text-align:center;"
        bodyStyle="text-align:center;"
      >
      </Column>
      <Column
        field="Khoihocs"
        header="Khối"
        headerStyle="text-align:center;max-width:380px"
        bodyStyle="text-align:center;max-width:380px;"
      >
        <template #body="md">
          <div class="block">
            <Badge
              v-for="k in md.data.Khoihocs"
              :key="k"
              :value="k"
              :severity="k < 6 ? 'success' : k < 10 ? 'danger' : 'info'"
              class="mr-1 mt-1"
            ></Badge>
          </div>
        </template>
      </Column>
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
            @change="upTrangthaiBosach(md.data)"
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
            @click="editBosach(md.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delBosach(md.data)"
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
    header="Cập nhật bộ sách"
    v-model:visible="displayAddBosach"
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
            v-model="bosach.Bosach_ID"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left"
            >Tên <span class="redsao">(*)</span></label
          >
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="bosach.TenBosach"
            :class="{ 'p-invalid': v$.TenBosach.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.TenBosach.$invalid && submitted) || v$.TenBosach.$pending.$response"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.TenBosach.required.$message
                .replace("Value", "Tên")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left"
            >NXB </label
          >
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="bosach.Nhaxuatban"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left"
            >Website </label
          >
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="bosachis_link"
          />
        </div>

        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Khối</label>
          <MultiSelect
            v-model="bosach.Khoihocs"
            @selectall-change="checkAllkhoi($event)"
            :selectAll="selectAll"
            class="col-10"
            :popup="true"
            :options="tudiens[1]"
            optionLabel="text"
            optionValue="value"
            optionGroupLabel="text"
            optionGroupChildren="items"
            placeholder=""
          >
            <template #optiongroup="slotProps">
              <div class="flex align-items-center">
                <div>
                  <Checkbox
                    @change="checkkhoi(slotProps.option)"
                    :binary="true"
                    v-model="slotProps.option.chon"
                  />
                  {{ slotProps.option.text }}
                </div>
              </div>
            </template>
          </MultiSelect>
        </div>

        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-4 ip36 p-0" v-model="bosach.STT" />
          <label style="vertical-align: text-bottom" class="col-2 text-right"
            >Trạng thái</label
          >
          <InputSwitch v-model="bosach.Trangthai" class="mt-1" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddBosach"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
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
