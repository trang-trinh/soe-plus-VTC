<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { encr } from "../../util/function";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { useToast } from "vue-toastification";

const router = inject("router");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const cryoptojs = inject("cryptojs");
const toast = useToast();
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
//Declare
const isFirst = ref(false);
const options = ref({
  loading: false,
  statistical_id: null,
  searchprocedure: "",
  searchparameter: "",
  totalprocedure: 0,
  total: 0,
});
const procedures = ref([]);
const parameters = ref([]);

const ruleprocedure = {
  statistical_name: {
    required,
    $errors: [
      {
        $property: "statistical_name",
        $validator: "required",
        $message: "Tên thủ tục không được để trống!",
      },
    ],
  },
  procedure_name: {
    required,
    $errors: [
      {
        $property: "procedure_name",
        $validator: "required",
        $message: "Mã thủ tục không được để trống!",
      },
    ],
  },
};
const modelprocedure = ref({});
const vp$ = useVuelidate(ruleprocedure, modelprocedure);

const selectedKeyProcedure = ref([]);

watch(selectedKeyProcedure, () => {
  goProcedure(selectedKeyProcedure.value);
});

//Function
const isAdd = ref(false);
const submitted = ref(false);
const headerDialogProcedure = ref();
const displayDialogProcedure = ref(false);

const openAddDialogProcedure = (str) => {
  isAdd.value = true;
  submitted.value = false;
  modelprocedure.value = {
    status: true,
    is_type: 0,
    is_order: 0,
  };
  if (options.value.totalprocedure > 0) {
    modelprocedure.value.is_order = options.value.totalprocedure + 1;
  } else {
    modelprocedure.value.is_order = 1;
  }
  headerDialogProcedure.value = str;
  displayDialogProcedure.value = true;
};
const closeDialogProcedure = () => {
  modelprocedure.value = {
    status: true,
    is_type: 0,
    is_order: 0,
  };
  displayDialogProcedure.value = false;
};
const saveModelProcedure = (isFormValid, is_continue) => {
  submitted.value = true;
  if (!isFormValid) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng điền đầy đủ thông tin trường bôi đỏ!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (modelprocedure.value.statistical_name.length > 500) {
    swal.fire({
      title: "Thông báo!",
      text: "Tên thủ tục không vượt quá 500 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (modelprocedure.value.procedure_name.length > 500) {
    swal.fire({
      title: "Thông báo!",
      text: "Mã thủ tục không vượt quá 500 ký tự!",
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
  formData.append("isAdd", isAdd.value);
  formData.append("model", JSON.stringify(modelprocedure.value));
  axios
    .put(baseURL + "/api/statistical/update_statistical", formData, config)
    .then((response) => {
      if (response.data.err === "1") {
        swal.close();
        if (options.value.loading) options.value.loading = false;
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
      swal.close();
      toast.success(
        isAdd.value
          ? "Thêm thủ tục thành công!"
          : "Cập nhật thủ tục thành công!"
      );
      initProcedure(true);
      if (!is_continue) {
        closeDialogProcedure();
      } else {
        modelprocedure.value.is_order += 1;
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
  if (submitted.value) submitted.value = true;
};
const editProcedure = (item) => {
  submitted.value = false;
  options.value.loading = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  isAdd.value = false;
  axios
    .post(
      baseURL + "/api/statistical/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "statistical_procedure_get",
            par: [{ par: "statistical_id", va: item.statistical_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          var tbs = JSON.parse(data);
          modelprocedure.value = tbs[0][0];
        }
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;

      headerDialogProcedure.value = "Cập nhật thủ tục";
      displayDialogProcedure.value = true;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      if (options.value.loading) options.value.loading = false;
      addLog({
        title: "Lỗi Console editItem",
        controller: "boardroom.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      }
    });
};
const updateStatusProcedure = (item) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let data = { id: item.statistical_id, status: !(item.status || false) };
  axios
    .put(baseURL + "/api/statistical/update_status_statistical", data, config)
    .then((response) => {
      if (response.data.err === "1") {
        swal.close();
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      } else {
        swal.close();
        toast.success("Cập nhật trạng thái thành công!");
        initProcedure(true);
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const deleteProcedure = (item) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá thủ tục này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        options.value.loading = true;
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        var ids = [item["statistical_id"]];
        axios
          .delete(baseURL + "/api/statistical/delete_statistical", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: ids,
          })
          .then((response) => {
            if (response.data.err === "1") {
              swal.close();
              if (options.value.loading) options.value.loading = false;
              swal.fire({
                title: "Thông báo!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
              return;
            }
            //initData(true);
            if (ids.length > 0) {
              ids.forEach((element, i) => {
                var idx = procedures.value.findIndex(
                  (x) => x.statistical_id == element
                );
                if (idx != -1) {
                  procedures.value.splice(idx, 1);
                }
              });
            }
            swal.close();
            toast.success("Xoá thủ tục thành công!");
            if (options.value.loading) options.value.loading = false;
          })
          .catch((error) => {
            swal.close();
            if (options.value.loading) options.value.loading = false;
            addLog({
              title: "Lỗi Console delItem",
              controller: "boardroom.vue",
              logcontent: error.message,
              loai: 2,
            });
            if (error.status === 401) {
              swal.fire({
                title: "Thông báo!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
              return;
            }
          });
      }
    });
};
const goProcedure = (node) => {
  router.push({
    name: "statisticalchart",
    params: { id: node.statistical_id },
  });
};

//Init
const initProcedure = (rf) => {
  if (rf) {
    options.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/statistical/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "statistical_procedure_list",
            par: [{ par: "search", va: options.value.searchprocedure }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            procedures.value = tbs[0];
          } else {
            procedures.value = [];
          }
          if (tbs.length == 2) {
            options.value.totalprocedure = tbs[1][0].total;
          }
        }
      }
      swal.close();
      if (isFirst.value) isFirst.value = false;
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      if (options.value.loading) options.value.loading = false;
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
const initParameter = (rf) => {
  if (rf) {
    options.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/statistical/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "statistical_parameter_list",
            par: [
              { par: "search", va: options.value.searchparameter },
              { par: "statistical_id", va: options.value.statistical_id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            parameters.value = tbs[0];
          } else {
            parameters.value = [];
          }
          if (tbs.length == 2) {
            options.value.totalparameter = tbs[1][0].total;
          }
        }
      }
      swal.close();
      if (isFirst.value) isFirst.value = false;
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      if (options.value.loading) options.value.loading = false;
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
const refreshProcedure = () => {
  options.value = {
    loading: false,
    statistical_id: null,
    searchprocedure: "",
    searchparameter: "",
    totalprocedure: 0,
    total: 0,
  };
  initProcedure(true);
};
onMounted(() => {
  initProcedure(true);
  return {};
});
</script>
<template>
  <div class="surface-100 p-3">
    <Toolbar class="outline-none surface-0 border-none">
      <template #start>
        <span class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText
            @keypress.enter="searchProcedure()"
            v-model="options.searchprocedure"
            type="text"
            spellcheck="false"
            placeholder=" Tìm kiếm tên thủ tục"
          />
        </span>
      </template>
      <template #end>
        <Button
          @click="openAddDialogProcedure('Thêm mới thủ tục')"
          label="Thêm mới"
          icon="pi pi-plus"
          class="mr-2"
        />
        <Button
          @click="refreshProcedure()"
          class="p-button-outlined p-button-secondary"
          icon="pi pi-refresh"
          v-tooltip.top="'Tải lại'"
        />
      </template>
    </Toolbar>
    <div class="d-lang-table">
      <DataTable
        @sort="onSort($event)"
        :value="procedures"
        :totalRecords="options.totalprocedure"
        :lazy="true"
        :rowHover="true"
        :showGridlines="true"
        :scrollable="true"
        v-model:selection="selectedKeyProcedure"
        selectionMode="single"
        dataKey="statistical_id"
        scrollHeight="flex"
        filterDisplay="menu"
        filterMode="lenient"
        responsiveLayout="scroll"
      >
        <Column
          field="is_order"
          header="STT"
          headerStyle="text-align:center;max-width:50px;height:50px"
          bodyStyle="text-align:center;max-width:50px;"
          class="align-items-center justify-content-center text-center"
        >
        </Column>
        <Column
          field="procedure_name"
          header="Tên thủ tục"
          headerStyle="height:50px;max-width:auto;"
        >
          <template #body="slotProps">
            <div>
              <div>{{ slotProps.data.statistical_name }}</div>
              <div class="description mt-2">
                {{ slotProps.data.procedure_name }}
              </div>
            </div>
          </template>
        </Column>
        <Column
          field="status"
          header="Trạng thái"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="data">
            <Checkbox
              :binary="data.data.status"
              v-model="data.data.status"
              @click="updateStatusProcedure(data.data)"
            />
          </template>
        </Column>
        <Column
          header="Chức năng"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="data">
            <div>
              <Button
                @click="editProcedure(data.data)"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                type="button"
                icon="pi pi-pencil"
                v-tooltip="'Sửa'"
              ></Button>
              <Button
                @click="deleteProcedure(data.data)"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                type="button"
                v-tooltip="'Xóa'"
                icon="pi pi-trash"
              ></Button>
            </div>
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
            v-if="!isFirst || options.total == 0"
            style="display: flex; height: calc(100vh - 195px)"
          >
            <div>
              <img src="../../assets/background/nodata.png" height="144" />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </div>
        </template>
      </DataTable>
    </div>
  </div>
  <!--Dialog-->
  <Dialog
    :header="headerDialogProcedure"
    v-model:visible="displayDialogProcedure"
    :style="{ width: '40vw' }"
    :closable="true"
    style="z-index: 1000"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label class="col-3 text-left p-0"
              >Tên thủ tục <span class="redsao">(*)</span></label
            >
            <InputText
              v-model="modelprocedure.statistical_name"
              spellcheck="false"
              class="col-8 ip36 px-2"
              :class="{
                'p-invalid': vp$.statistical_name.$invalid && submitted,
              }"
            />
            <div
              v-if="
                (vp$.statistical_name.$invalid && submitted) ||
                vp$.statistical_name.$pending.$response
              "
            >
              <small class="p-error">
                <span class="col-12 p-0">{{
                  vp$.statistical_name.required.$message
                    .replace("Value", "Tên thủ tục")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label class="col-3 text-left p-0"
              >Mã thủ tục <span class="redsao">(*)</span></label
            >
            <InputText
              v-model="modelprocedure.procedure_name"
              spellcheck="false"
              class="col-8 ip36 px-2"
              :class="{
                'p-invalid': vp$.procedure_name.$invalid && submitted,
              }"
            />
            <div
              v-if="
                (vp$.procedure_name.$invalid && submitted) ||
                vp$.procedure_name.$pending.$response
              "
            >
              <small class="p-error">
                <span class="col-12 p-0">{{
                  vp$.procedure_name.required.$message
                    .replace("Value", "Mã thủ tục")
                    .replace("is required", "không được để trống")
                }}</span>
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Loại</label>
            <InputText v-model="modelprocedure.is_type" class="col-6 ip36" />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <label>Số thứ tự </label>
            <InputText v-model="modelprocedure.is_order" class="col-6 ip36" />
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group" style="height: 100%">
            <div
              class="field-checkbox flex justify-content-center"
              style="height: 100%"
            >
              <InputSwitch v-model="modelprocedure.status" />
              <label for="binary">Kích hoạt</label>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogProcedure()"
        class="p-button-text"
      />
      <Button
        label="Lưu và tiếp tục"
        icon="pi pi-check"
        @click="saveModelProcedure(!vp$.$invalid, true)"
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveModelProcedure(!vp$.$invalid)"
      />
    </template>
  </Dialog>
</template>
<style scoped>
.d-lang-table {
  height: calc(100vh - 130px);
  overflow-y: auto;
}
.inputanh {
  border: 1px solid #ccc;
  width: 96px;
  height: 96px;
  cursor: pointer;
  padding: 1px;
}
.ipnone {
  display: none;
}
.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
.format-flex-center {
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
}
.format-grid-center {
  display: grid;
  align-items: center;
  justify-content: center;
  text-align: center;
}
.style-day {
  line-height: 1.8rem;
}
.style-day.false {
  color: #2196f3 !important;
}
.style-day.true {
  color: red !important;
}
.form-group {
  display: grid;
  margin-bottom: 1rem;
}
.form-group > label {
  margin-bottom: 0.5rem;
}
.ip36 {
  width: 100%;
}
.p-ulchip {
  margin: 0;
  margin-top: 0.5rem;
  padding: 0;
  list-style: none;
}
.p-lichip {
  float: left;
}
.p-multiselect-label {
  height: 100%;
  display: flex;
  align-items: center;
}
.type0 {
  background-color: #ff8b4e;
}
.type1 {
  background-color: #33c9dc;
}
.description {
  color: #aaa;
  font-size: 12px;
}
.format-center {
    display: flex;
    justify-content: center;
    align-items: center;
    text-align: center;
}
</style>
<style lang="scss" scoped>
::v-deep(.d-lang-table) {
  .p-datatable-thead .justify-content-center .p-column-header-content {
    justify-content: center !important;
  }

  .p-datatable-table {
    position: absolute;
  }

  .p-datatable-thead {
    position: sticky;
    top: 0;
    z-index: 1;
  }
}
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
    height: 100%;
    display: flex;
    align-items: center;
  }
  .p-chip img {
    margin: 0;
  }
  .p-avatar-text {
    font-size: 1rem;
  }
}
</style>