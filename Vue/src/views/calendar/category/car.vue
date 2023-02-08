<script setup>
import { ref, inject, onMounted } from "vue";
import { encr } from "../../../util/function";
import { useToast } from "vue-toastification";
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();
const cryoptojs = inject("cryptojs");

//Declare
const options = ref({
  loading: false,
  user_id: store.getters.user.user_id,
  search: "",
  pageNo: 0,
  pageSize: 20,
  total: 0,
  sort: "created_date desc",
  orderBy: "DESC",
});
const selectedNodes = ref([]);
const isFirst = ref(false);
const datas = ref([]);

//filter
const searchData = () => {
  options.value.pageNo = 0;
  initData(true);
};

//Function
const isAdd = ref(false);
const submitted = ref(false);
const model = ref({});
const headerDialog = ref();
const displayDialog = ref(false);
const openAddDialog = (str) => {
  isAdd.value = true;
  model.value = {
    car_code: null,
    car_name: null,
    status: true,
    is_order: options.value.total + 1,
  };
  headerDialog.value = str;
  displayDialog.value = true;
};
const closeDialog = () => {
  model.value = {};
  displayDialog.value = false;
};
const saveModel = () => {
  submitted.value = true;
  if (!model.value.car_code || !model.value.car_name) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng điền đầy đủ thông tin trường bôi đỏ!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (model.value.car_code.length > 50) {
    swal.fire({
      title: "Thông báo!",
      text: "Mã xe không vượt quá 50 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (model.value.car_name.length > 250) {
    swal.fire({
      title: "Thông báo!",
      text: "Tên xe không vượt quá 250 ký tự!",
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
  formData.append("model", JSON.stringify(model.value));
  axios
    .put(baseURL + "/api/calendar_car/update_ca_car", formData, config)
    .then((response) => {
      if (response.data.err === "1") {
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
        isAdd.value ? "Thêm xe thành công!" : "Cập xe họp thành công!"
      );
      initData(true);
      closeDialog();
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
const editItem = (item) => {
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
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_ca_car_get",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "car_id", va: item.car_id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      var data = response.data.data;
      if (data != null) {
        var tbs = JSON.parse(data);
        model.value = tbs[0][0];
      }
      swal.close();
      if (options.value.loading) options.value.loading = false;

      headerDialog.value = "Sửa thông tin xe";
      displayDialog.value = true;
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
const udpateStatusItem = (item) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let data = { id: item.car_id, status: !(item.status || false) };
  axios
    .put(baseURL + "/api/calendar_car/update_status_ca_car", data, config)
    .then((response) => {
      if (response.data.err === "1") {
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
        initData(true);
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
};
const delItem = (item) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá xe này không!",
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
        var ids = [];
        if (item != null) {
          ids = [item["car_id"]];
        } else {
          if (selectedNodes.value.length > 0) {
            selectedNodes.value.forEach((row, i) => {
              ids.push(row["car_id"]);
            });
          }
        }
        axios
          .delete(baseURL + "/api/calendar_car/delete_ca_car", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: ids,
          })
          .then((response) => {
            if (response.data.err === "1" || response.data.err === "2") {
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
            toast.success("Xoá xe thành công!");
            initData(true);
            // if (ids.length > 0) {
            //   ids.forEach((element, i) => {
            //     var idx = datas.value.findIndex(
            //       (x) => x.user_id == element.car_id
            //     );
            //     if (idx != -1) {
            //       datas.value.splice(idx, 1);
            //     }
            //   });
            // }

            swal.close();
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

//Init
const initData = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_ca_car_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      var data = response.data.data;
      if (data != null) {
        swal.close();
        let tbs = JSON.parse(data);
        datas.value = tbs[0];
        datas.value.forEach((element, i) => {
          element["STT"] =
            options.value.pageNo * options.value.pageSize + i + 1;
        });
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
};
const refresh = () => {
  selectedNodes.value = [];
  options.value = {
    loading: false,
    user_id: store.getters.user.user_id,
    search: "",
    pageNo: 0,
    pageSize: 20,
    total: 0,
    sort: "created_date desc",
    orderBy: "DESC",
  };
  initData(true);
};
onMounted(() => {
  initData();
});
</script>
<template>
  <div class="surface-100 p-3">
    <Toolbar class="outline-none surface-0 border-none pb-0">
      <template #start>
        <div>
          <h3 class="module-title m-0">
            <i class="pi pi-briefcase"></i> Danh sách xe ({{ options.total }})
          </h3>
        </div>
      </template>
    </Toolbar>
    <Toolbar class="outline-none surface-0 border-none">
      <template #start>
        <span class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText
            @keypress.enter="searchData"
            v-model="options.search"
            type="text"
            spellcheck="false"
            placeholder=" Tìm kiếm tên, mã xe"
          />
        </span>
      </template>
      <template #end>
        <Button
          @click="openAddDialog('Thêm mới')"
          label="Thêm mới"
          icon="pi pi-plus"
          class="mr-2"
        />
        <Button
          @click="refresh()"
          class="mr-2 p-button-outlined p-button-secondary"
          v-tooltip.top="'Tải lại'"
          icon="pi pi-refresh"
        />
        <Button
          label="Xoá"
          icon="pi pi-trash"
          class="mr-2 p-button-danger"
          v-if="selectedNodes.length > 0"
          @click="delItem()"
        />
      </template>
    </Toolbar>
    <div class="d-lang-table" style="height: calc(100vh - 160px) !important;">
      <DataTable
        @page="onPage($event)"
        @sort="onSort($event)"
        :value="datas"
        :loading="options.loading"
        :paginator="true"
        :rows="options.pageSize"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
        :totalRecords="options.total"
        :scrollable="true"
        :lazy="true"
        :rowHover="true"
        :showGridlines="true"
        :globalFilterFields="['boardroom_name']"
        v-model:selection="selectedNodes"
        dataKey="car_id"
        scrollHeight="flex"
        filterDisplay="menu"
        filterMode="lenient"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks  NextPageLink LastPageLink RowsPerPageDropdown"
        responsiveLayout="scroll"
      >
        <Column
          selectionMode="multiple"
          headerStyle="text-align:center;max-width:50px"
          bodyStyle="text-align:center;max-width:50px"
          class="align-items-center justify-content-center text-center"
        ></Column>
        <Column
          field="STT"
          header="STT"
          headerStyle="text-align:center;max-width:75px;height:50px"
          bodyStyle="text-align:center;max-width:75px;"
          class="align-items-center justify-content-center text-center"
        >
        </Column>
        <Column
          field="car_code"
          header="Mã xe"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;"
          class="align-items-center justify-content-center text-center"
        >
        </Column>
        <Column
          field="car_name"
          header="Tên xe"
          headerStyle="height:50px;max-width:auto;"
        >
        </Column>
        <Column
          field="status"
          header="Hiển thị"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <Checkbox
              :binary="slotProps.data.status"
              v-model="slotProps.data.status"
              @click="udpateStatusItem(slotProps.data)"
            /> </template
        ></Column>
        <Column
          header="Chức năng"
          headerStyle="text-align:center;max-width:200px;height:50px"
          bodyStyle="text-align:center;max-width:200px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <div v-if="slotProps.data.is_function">
              <Button
                @click="editItem(slotProps.data)"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                type="button"
                icon="pi pi-pencil"
                v-tooltip="'Sửa'"
              ></Button>
              <Button
                @click="delItem(slotProps.data, true)"
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
            style="display: flex; height: calc(100vh - 268px)"
          >
            <div>
              <img src="../../../assets/background/nodata.png" height="144" />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </div>
        </template>
      </DataTable>
    </div>
  </div>
  <Dialog
    :header="headerDialog"
    v-model:visible="displayDialog"
    :style="{ width: '40vw' }"
    :closable="true"
  >
    <form>
      <div class="row m-2">
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Biển số xe <span class="redsao">(*)</span></label>
            <InputText
              type="text"
              v-model="model.car_code"
              :class="{
                'p-invalid': !model.car_code && submitted,
              }"
            />
            <div v-if="!model.car_code && submitted">
              <small class="p-error">
                <span class="col-12 p-0">Mã xe không được để trống</span>
              </small>
            </div>
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="form-group">
            <label>Tên xe <span class="redsao">(*)</span></label>
            <Textarea
              v-model="model.car_name"
              :autoResize="true"
              :class="{
                'p-invalid': !model.car_name && submitted,
              }"
              rows="5"
              cols="30"
            />
            <div v-if="!model.car_name && submitted">
              <small class="p-error">
                <span class="col-12 p-0">Tên xe không được để trống</span>
              </small>
            </div>
          </div>
        </div>
        <div class="col-6 md:col-6">
          <div class="form-group">
            <div
              class="field-checkbox flex justify-content-center"
              style="height: 100%"
            >
              <InputSwitch v-model="model.status" />
              <label for="binary">Hiển thị</label>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog"
        class="p-button-text"
      />

      <Button label="Lưu" icon="pi pi-check" @click="saveModel()" />
    </template>
  </Dialog>
</template>
<style scoped>
@import url(../component/stylecalendar.css);
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
::v-deep(.avatar-item) {
  .p-avatar.p-avatar-lg {
    width: 3rem;
    height: 3rem;
  }
}
::v-deep(.is-close) {
  .p-panel-header {
    color: red;
  }
}
</style>