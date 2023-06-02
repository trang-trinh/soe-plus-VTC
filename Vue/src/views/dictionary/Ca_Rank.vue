<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr } from "../../util/function.js";
import moment from "moment";
const cryoptojs = inject("cryptojs");

const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  field_name: {
    operator: FilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
  },
});
const rules = {
  rank: { required },
};
const rankValue = ref({
  rank: "",
  is_order: 1,
});
const selectedFields = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, rankValue);
const issaveField = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);
const options = ref({
  IsNext: true,
  sort: "is_order desc",
  orderfield: 1,
  order: "asc",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
});
//Thêm log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const loadData = (rf) => {
  if (filterTrangthai.value != null) {
    if (filterTrangthai.value == 1) filterTrangthai.value = true;
    else filterTrangthai.value = false;
  }
  loadDonvi();
  if (rf) {
    axios
      .post(
        baseURL + "/api/DictionaryProc/getData",
        {
          str: encr(
            JSON.stringify({
              proc: "ca_rank_list_and_count",
              par: [
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
                { par: "user_id", va: store.state.user.user_id },
                { par: "search", va: options.value.SearchText },
                { par: "orderfield", va: options.value.orderfield },
                { par: "order", va: options.value.order.toUpperCase() },
                { par: "status", va: filterTrangthai.value },
              ],
            }),
            SecretKey,
            cryoptojs,
          ).toString(),
        },
        config,
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        if (isFirst.value) isFirst.value = false;
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        });
        datalists.value = data;
        let data2 = JSON.parse(response.data.data)[1];
        if (data2.length > 0) {
          options.value.totalRecords = data2[0].totalRecords;
          options.value.maxisorder = data2[0].maxisorder;
          sttField.value = options.value.maxisorder + 1;
        }
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        addLog({
          title: "Lỗi Console loadData",
          controller: "FieldView.vue",
          logcontent: error.message,
          loai: 2,
        });
        if (error && error.status === 401) {
          swal.fire({
            title: "Thông báo",
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            icon: "error",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  }
};
const checkSort = ref(false);
//Phân trang dữ liệu
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    options.value.id = null;
    options.value.IsNext = true;
  } else if (event.page > options.value.PageNo + 1) {
    //Trang cuối
    options.value.id = -1;
    options.value.IsNext = false;
  } else if (event.page > options.value.PageNo) {
    //Trang sau

    options.value.id = datalists.value[datalists.value.length - 1].field_id;
    options.value.IsNext = true;
    if (checkSort.value) {
      options.value.id = null;
    }
  } else if (event.page < options.value.PageNo) {
    //Trang trước
    options.value.id = datalists.value[0].field_id;
    options.value.IsNext = false;
  }
  options.value.PageNo = event.page;
  loadData(true);
};
//Hiển thị dialog
const headerDialog = ref();
const DiaglogVisible = ref(false);
const openBasic = (str) => {
  submitted.value = false;
  rankValue.value = {
    rank: "",
    is_order: sttField.value,
    status: true,
  };
  rankValue.value.organization_id = store.state.user.organization_id;
  issaveField.value = false;
  headerDialog.value = str;
  length.value = false;
  DiaglogVisible.value = true;
};
const closeDialog = () => {
  rankValue.value = {
    rank: "",
    is_order: 1,
    status: true,
  };
  DiaglogVisible.value = false;
  loadData(true);
};

//Thêm bản ghi
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: issaveField.value ? "put" : "post",
    url:
      baseURL +
      `/api/ca_Rank/${issaveField.value ? "Update_ca_rank" : "add_Rank"}`,
    data: rankValue.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success(
          response.config.method == "put"
            ? "Cập nhật cấp bậc thành công!"
            : "Thêm mới cấp bậc thành công!",
        );
        closeDialog();
        loadData(true);
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          text:
            ms.includes("field_name") == true
              ? "Tên cấp bậc không quá 250 ký tự!"
              : ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        loadData(true);
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};

const sttField = ref();
//Thêm bản ghi con
const isChirlden = ref(false);

//Sửa bản ghi
const editField = (dataPlace) => {
  submitted.value = false;
  rankValue.value = dataPlace;
  isChirlden.value = false;
  if (dataPlace.parent_id != null) {
    isChirlden.value = true;
  }
  headerDialog.value = "Cập nhật cấp bậc";
  issaveField.value = true;
  length.value = false;
  DiaglogVisible.value = true;
};
//Xóa bản ghi
const delField = (Field) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá cấp bậc này không!",
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
          .delete(baseURL + "/api/ca_Rank/Delete_ca_rank", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Field != null ? [Field.rank_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá cấp bậc thành công!");
              if (
                (options.value.totalRecords - Field.length) % 2 == 0 &&
                options.value.PageNo > 0
              ) {
                options.value.PageNo = options.value.PageNo - 1;
              }
              loadData(true);
            } else {
              swal.fire({
                title: "Thông báo",
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
                title: "Thông báo",
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

const onSort = (event) => {
  console.log(event);

  if (event.sortField == "is_order") {
    options.value.orderfield = 2;
    options.value.order = event.sortOrder == 1 ? "ASC" : "DESC";
  }
  if (event.sortField == "rank") {
    options.value.orderfield = 3;
    options.value.order = event.sortOrder == 1 ? "ASC" : "DESC";
  }
  loadData(true);
};

const isFirst = ref(true);

//Tìm kiếm
const searchFields = (event) => {
  options.value.loading = true;
  loadData(true);
};

//Checkbox
const onCheckBox = (value) => {
  let data = {
    IntID: value.rank_id,
    TextID: value.rank_id + "",
    IntTrangthai: 1,
    BitTrangthai: value.status,
  };
  if (
    store.state.user.is_super == true ||
    store.state.user.user_id == value.created_by ||
    store.state.user.role_id == "admin"
  ) {
    axios
      .put(baseURL + "/api/ca_Rank/Update_Status_Ca_Rank", data, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Cập nhật trạng thái thành công!");
          loadData(true);
          closeDialog();
        } else {
          swal.fire({
            title: "Thông báo",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Thông báo",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    swal.fire({
      title: "Thông báo!",
      text: "Bạn không có quyền chỉnh sửa! Chỉ có Quản trị viên đơn vị hoặc Quản trị viên hệ thống mới có quyền chỉnh sửa mục này",
      icon: "error",
      confirmButtonText: "OK",
    });
    loadData(true);
  }
};
//Xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedFields.value.length);
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá danh sách này không!",
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
        selectedFields.value.forEach((item) => {
          listId.push(item.rank_id);
        });
        axios
          .delete(baseURL + "/api/ca_Rank/Delete_ca_rank", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá danh sách thành công!");
              checkDelList.value = false;
              if (
                (options.value.totalRecords - listId.length) % 2 == 0 &&
                options.value.PageNo > 0
              ) {
                options.value.PageNo = options.value.PageNo - 1;
              }
              loadData(true);
            } else {
              swal.fire({
                title: "Thông báo",
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
                title: "Thông báo",
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const first = ref();
const refresh = () => {
  first.value = 0;
  options.value.loading = true;
  options.value.PageNo = 0;
  options.value = {
    IsNext: true,
    sort: "is_order desc",
    orderfield: 1,
    order: "asc",
    SearchText: "",
    PageNo: 0,
    PageSize: 20,
    loading: true,
    totalRecords: null,
  };
  selectedFields.value = [];
  options.value.SearchText = "";
  filterPhanloai.value = "";
  filterTrangthai.value = "";
  styleObj.value = "";
  options.value.loading = true;

  loadData(true);
};

//Filter
const showFilter = ref(false);

const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);
const filterPhanloai = ref();
const filterTrangthai = ref();

const reFilterEmail = () => {
  filterPhanloai.value = null;
  filterTrangthai.value = null;
  filterFileds();
  showFilter.value = false;
  styleObj.value = "";
};
const filterFileds = () => {
  styleObj.value = style.value;
  showFilter.value = false;
  loadData(true);
};
watch(selectedFields, () => {
  if (selectedFields.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});
const op = ref();
const toggle = (event) => {
  op.value.toggle(event);
};
const styleObj = ref();
const style = ref({
  "background-color": "#2196F3 !important",
  color: "#fff !important",
  " border": "1px solid #5ca7e3 !important",
});
const loadDonvi = () => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({ proc: "sys_org_list" }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      treedonvis.value = [];
      let data = JSON.parse(response.data.data)[0];
      let sys = { key: 0, label: "Hệ thống", data: { organization_id: 0 } };

      treedonvis.value.push(sys);

      if (data.length > 0) {
        if (data.length > 0) {
          data.forEach((x) => {
            x = { key: x.organization_id, data: x, label: x.organization_name };
            treedonvis.value.push(x);
          });
        } else {
          treedonvis.value = [];
        }
      }
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    });
};

const treedonvis = ref();
const length = ref(false);
const checklength = () => {
  length.value = false;
  const textbox = document.getElementById("rank_name");
  if (textbox.value.length > 250) {
    length.value = true;
  }
  return length.value;
};
onMounted(() => {
  loadData(true);
  return {};
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <DataTable
      v-model:first="first"
      :value="datalists"
      :paginator="true"
      :rows="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[1, 20, 30, 50, 100, 200]"
      :scrollable="true"
      scrollHeight="flex"
      :loading="options.loading"
      v-model:selection="selectedFields"
      :lazy="true"
      @page="onPage($event)"
      @sort="onSort($event)"
      :totalRecords="options.totalRecords"
      dataKey="rank_id"
      :rowHover="true"
      v-model:filters="filters"
      filterDisplay="menu"
      :showGridlines="true"
      filterMode="lenient"
      responsiveLayout="scroll"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-briefcase"></i> Danh sách cấp bậc ({{
            options.totalRecords
          }})
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup.enter="searchFields"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
            /></span>

            <Button
              type="button"
              class="ml-2 p-button-outlined p-button-secondary"
              icon="pi pi-filter"
              @click="toggle"
              aria:haspopup="true"
              aria-controls="overlay_panel"
              :style="[styleObj]"
              v-tooltip="'Bộ lọc'"
            />
            <OverlayPanel
              ref="op"
              appendTo="body"
              class="p-0 m-0"
              :showCloseIcon="false"
              id="overlay_panel"
              :style="'width:400px'"
            >
              <div class="grid formgrid m-0">
                <div class="flex field col-12 p-0">
                  <div
                    :class="'col-4 text-left pt-2 p-0'"
                    style="text-align: left"
                  >
                    Phân loại
                  </div>

                  <div :class="'col-8'">
                    <TreeSelect
                      v-model="filterPhanloai"
                      :options="treedonvis"
                      optionLabel="data.organization_name"
                      optionValue="data.organization_id"
                      placeholder="Chọn đơn vị"
                      class="col-12 p-0 m-0 md:col-12"
                      v-if="store.state.user.is_super == 1"
                      panelClass="d-design-dropdown"
                    />
                  </div>
                </div>

                <div class="flex field col-12 p-0">
                  <div
                    :class="'col-4 text-left pt-2 p-0'"
                    style="text-align: center,justify-content:center"
                  >
                    Trạng thái
                  </div>
                  <div :class="'col-8'">
                    <Dropdown
                      class="col-12 p-0 m-0 d-design-dropdown"
                      v-model="filterTrangthai"
                      :options="trangThai"
                      optionLabel="name"
                      optionValue="code"
                      placeholder="Trạng thái"
                    />
                  </div>
                </div>
                <div class="flex col-12 p-0">
                  <Toolbar
                    class="border-none surface-0 outline-none pb-0 w-full"
                  >
                    <template #start>
                      <Button
                        @click="reFilterEmail"
                        class="p-button-outlined"
                        label="Xóa"
                      ></Button>
                    </template>
                    <template #end>
                      <Button
                        @click="filterFileds"
                        label="Lọc"
                      ></Button>
                    </template>
                  </Toolbar>
                </div>
              </div>
            </OverlayPanel>
          </template>

          <template #end>
            <Button
              v-if="checkDelList"
              @click="deleteList()"
              label="Xóa"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
            />
            <Button
              @click="openBasic('Thêm mới cấp bậc')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
            />
            <Button
              @click="refresh()"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip="'Tải lại'"
            />
          </template>
        </Toolbar>
      </template>

      <Column
        selectionMode="multiple"
        headerStyle="text-align:center;max-width:50px;"
        bodyStyle="text-align:center;max-width:50px"
        class="align-items-center justify-content-center text-center"
      ></Column>
      <Column
        field="is_order"
        header="STT"
        :sortable="true"
        headerStyle="text-align:center;max-width:6rem;height:50px"
        bodyStyle="text-align:center;max-width:6rem"
        class="align-items-center justify-content-center text-center"
      >
      </Column>

      <Column
        field="rank"
        header="Tên cấp bậc"
        :sortable="true"
        headerStyle="text-align:center;"
        headerClass="align-items-center justify-content-center text-center"
        bodyStyle=""
      >
      </Column>

      <Column
        field="status"
        header="Hiển thị"
        headerStyle="text-align:center;max-width:10rem"
        bodyStyle="text-align:center;max-width:10rem"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Checkbox
            :binary="data.data.status"
            v-model="data.data.status"
            @click="onCheckBox(data.data)"
          />
        </template>
      </Column>

      <Column
        header="Chức năng"
        headerStyle="text-align:center;max-width:12rem;"
        bodyStyle="text-align:center;max-width:12rem"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="Field">
          <div
            v-if="
              store.state.user.is_super == true ||
              store.state.user.user_id == Field.data.created_by ||
              (store.state.user.role_id == 'admin' &&
                store.state.user.organization_id == Field.data.organization_id)
            "
          >
            <Button
              @click="editField(Field.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
            ></Button>
            <Button
              @click="delField(Field.data, true)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              v-tooltip="'Xóa'"
            >
            </Button>
          </div>
        </template>
      </Column>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          v-if="!isFirst"
        >
          <img
            src="../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
  </div>
  <Dialog
    v-model:visible="DiaglogVisible"
    :style="'width:40vw;'"
    :showCloseIcon="true"
    :header="headerDialog"
  >
    <div class="col-12">
      <div class="col-12 flex align-items-center">
        <div class="col-4">Cấp bậc <span class="redsao">(*)</span></div>
        <InputText
          id="rank_name"
          v-model="rankValue.rank"
          spellcheck="false"
          class="col-8"
          :class="{
            'p-invalid': v$.rank.$invalid && submitted,
          }"
          autocomplete="off"
          @input="checklength()"
        />
      </div>
      <div
        style="display: flex"
        class="col-12 py-0"
        v-if="length == true"
      >
        <div class="col-4 p-0 text-left"></div>
        <small class="col-8 p-0 p-error">
          <span class="col-12">Cấp bậc không quá 250 kí tự!</span>
        </small>
      </div>
      <div
        style="display: flex"
        class="col-12 py-0"
        v-if="(v$.rank.$invalid && submitted) || v$.rank.$pending.$response"
      >
        <div class="col-4 p-0 text-left"></div>
        <small class="col-8 p-0 p-error">
          <span class="col-12">{{
            v$.rank.required.$message
              .replace("Value", "Tên cấp bậc")
              .replace("is required", "không được để trống")
          }}</span>
        </small>
      </div>
      <div class="col-12 flex align-items-center">
        <div class="col-4">STT</div>
        <InputNumber
          v-model="rankValue.is_order"
          spellcheck="false"
          class="col-8 p-0"
          autocomplete="off"
        />
      </div>
    </div>
    <template #footer>
      <div class="mt-2">
        <Button
          class="p-button-text"
          icon="pi pi-times"
          label="Đóng"
          @click="DiaglogVisible = false"
        />
        <Button
          icon="pi pi-check"
          label="Xác nhận"
          @click="saveData(!v$.$invalid)"
        />
      </div>
    </template>
  </Dialog>
</template>

<style scoped></style>
