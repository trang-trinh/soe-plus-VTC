<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import moment from "moment";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
// Khai báo biến

const isDynamicSQL = ref(true); //phân trang bình thường hay phân trang tối ưu cho dữ liệu lớn
const toast = useToast();
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios
const opition = ref({
  IsNext: true,
  sort: "created_date",
  ob: "DESC",
  PageNo: 0,
  PageSize: 20,
  search: "",
  Filteruser_id: null,
  user_id: store.getters.user_id,
});

const rules = {
  group_name: {
    required,
    $errors: [
      {
        $property: "group_name",
        $validator: "required",
        $message: "Tên nhóm dự án không được để trống!",
      },
    ],
  },
};

const rulesProjectGroup = {
  group_name: {
    required,
  },
};

const ProjectGroup = ref({
  group_name: "",
  status: true,
  is_order: 1,
});

const checkDelList = ref(false);
const menuButs = ref();
const selectedProjectGroups = ref();
const submitted = ref(false);
const listProjectGroups = ref();
const listProjectGroups_new = ref();
const issaveProjectGroup = ref(false);
const sttProjectGroup = ref();
const headerAddProjectGroup = ref();
const v$ = useVuelidate(rules, ProjectGroup);
const validateProjectGroup = useVuelidate(rulesProjectGroup, ProjectGroup);
const displayProjectGroup = ref(false);
const isAdd = ref();

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportGroup("ExportExcel");
    },
  },
  {
    label: "Nhập Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportGroup("ExportExcelMau");
    },
  },
]);

const onPage = (event) => {
  if (event.rows != opition.value.PageSize) {
    opition.value.PageSize = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    opition.value.id = null;
    opition.value.IsNext = true;
  } else if (event.page > opition.value.PageNo + 1) {
    //Trang cuối
    opition.value.id = -1;
    opition.value.IsNext = false;
  } else if (event.page > opition.value.PageNo) {
    //Trang sau

    opition.value.id =
      listProjectGroups.value[listProjectGroups.value.length - 1].group_id;
    opition.value.IsNext = true;
  } else if (event.page < opition.value.PageNo) {
    //Trang trước
    opition.value.id = listProjectGroups.value[0].group_id;
    opition.value.IsNext = false;
  }
  opition.value.PageNo = event.page;
  loadData(true);
};

const addProjectGroup = (str) => {
  submitted.value = false;
  ProjectGroup.value = {
    group_name: "",
    status: true,
    is_order: sttProjectGroup.value,
  };
  if (store.state.user.is_super) {
    ProjectGroup.value.organization_id = 0;
  } else {
    ProjectGroup.value.organization_id = store.state.user.organization_id;
  }
  isAdd.value = true;
  issaveProjectGroup.value = false;
  headerAddProjectGroup.value = str;
  displayProjectGroup.value = true;
};

const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
// get data
const loadCountProjectGroup = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_ca_projectgroup_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "pageno", va: opition.value.PageNo },
              { par: "pagesize", va: opition.value.PageSize },
              { par: "search", va: opition.value.search },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        opition.value.totalRecords = data[0][0].totalrecords;
        data[1].forEach((element, i) => {
          element.STT = opition.value.PageNo * opition.value.PageSize + i + 1;
        });
        listProjectGroups.value = data[1];
      }
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCountProjectGroup",
        controller: "LogsView.vue",
        log_content: error.message,
        loai: 2,
      });
    });
};

const loadData = (rf) => {
  if (rf) {
    opition.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_ca_projectgroup_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "pageno", va: opition.value.PageNo },
              { par: "pagesize", va: opition.value.PageSize },
              { par: "search", va: opition.value.search },
              { par: "sort", va: opition.value.sort },
              { par: "ob", va: opition.value.ob },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        opition.value.totalRecords = data[0][0].totalrecords;
        sttProjectGroup.value = data[2][0].total + 1;
        data[1].forEach((element, i) => {
          element.STT = opition.value.PageNo * opition.value.PageSize + i + 1;
          // element.projectgroup = (element.);
        });
        listProjectGroups.value = data[1];
        listProjectGroups_new.value = data[1];
      }
      if (rf) {
        opition.value.loading = false;
        swal.close();
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "LogsView.vue",
        log_content: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

//open modal thêm mới
const closeDialogProjectGroup = () => {
  ProjectGroup.value = {
    group_name: "",
    status: true,
    is_order: 1,
  };
  displayProjectGroup.value = false;
  loadCountProjectGroup();
};

//Thêm bản ghi
const saveProjectGroup = (isFormValid) => {
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
  if (!issaveProjectGroup.value) {
    axios
      .post(
        baseURL +
          "/api/task_ca_projectgroup/" +
          (isAdd.value
            ? "Add_Task_Ca_ProjectGroup"
            : "Update_Task_Ca_ProjectGroup"),
        ProjectGroup.value,
        config,
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          if (isAdd.value) {
            toast.success("Thêm nhóm dự án thành công!");
          } else {
            toast.success("Sửa nhóm dự án thành công!");
          }
          closeDialogProjectGroup();
          loadData(true);
        } else {
          let ms = response.data.ms;
          swal.fire({
            title: "Thông báo!",
            text:
              ms.includes("group_name") == true
                ? "Tên nhóm dự án không quá 250 ký tự!"
                : ms,
            icon: "error",
            confirmButtonText: "OK",
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
      });
  }
};

const editProjectGroup = (dataProjectGroup) => {
  submitted.value = false;
  isAdd.value = false;

  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_ca_projectgroup_get",
            par: [{ par: "group_id", va: dataProjectGroup.group_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        ProjectGroup.value = data[0][0];
        headerAddProjectGroup.value = "Sửa nhóm dự án";
        issaveProjectGroup.value = false;
        displayProjectGroup.value = true;
        if (store.state.user.is_super) {
          ProjectGroup.value.organization_id = 0;
        } else {
          ProjectGroup.value.organization_id = store.state.user.organization_id;
        }
        swal.close();
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "LogsView.vue",
        log_content: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const DelProjectGroup = (dataProjectGroup) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá nhóm dự án này không!",
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
        var listId = [];
        if (!dataProjectGroup) {
          selectedProjectGroups.value.forEach(function (pg) {
            listId.push(pg.group_id);
          });
        }
        axios
          .delete(
            baseURL + "/api/task_ca_projectgroup/Delete_task_ca_projectgroup",
            {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data:
                dataProjectGroup != null ? [dataProjectGroup.group_id] : listId,
            },
          )
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá nhóm dự án thành công!");
              checkDelList.value = false;
              loadData(true);
            } else {
              swal.fire({
                title: "Thông báo!",
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
                title: "Thông báo!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

const onRefersh = () => {
  opition.value = {
    IsNext: true,
    sort: "created_date",
    ob: "DESC",
    PageNo: 0,
    PageSize: 20,
    search: "",
    Filteruser_id: null,
    user_id: store.getters.user_id,
  };
  first.value = 0;
  selectedProjectGroups.value = [];
  loadData(true);
};

const toggleExport = (event) => {
  menuButs.value.toggle(event);
};

const exportGroup = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "task_ca_projectgroup" }];
  if (method != "ExportExcelMau") {
    par = [
      { par: "user_id", va: store.getters.user.user_id },
      { par: "search", va: opition.value.search },
    ];
  } else {
    par = [
      { par: "user_id", va: "-1" },
      { par: "search", va: opition.value.search },
    ];
  }
  // let par = [
  //     { par: "user_id", va: store.getters.user.user_id },
  //     { par: "search", va: opition.value.search }
  //   ];
  axios
    .post(
      baseURL + "/api/Excel/ExportExcel",
      {
        excelname: "DANH SÁCH NHÓM DỰ ÁN",
        proc: "task_ca_projectgroup_list_export",
        par: par,
      },
      config,
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Kết xuất Data thành công!");
        window.open(baseURL + response.data.path);
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

const onSort = (event) => {
  if (event.sortField == "STT") {
    opition.value.sort = "is_order";
    opition.value.ob = event.sortOrder == 1 ? "ASC" : "DESC";
  } else {
    opition.value.sort = "group_name";
    opition.value.ob = event.sortOrder == 1 ? "ASC" : "DESC";
  }
  opition.value.PageNo = 0;
  loadData(true);
};

watch(selectedProjectGroups, () => {
  if (selectedProjectGroups.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});

const first = ref(0);
onMounted(() => {
  loadData(true);
  return {};
});
</script>

<template>
  <div v-if="store.getters.islogin" class="main-layout true flex-grow-1 p-2">
    <DataTable
      v-model:first="first"
      :show-gridlines="true"
      :rowHover="true"
      :value="listProjectGroups"
      :paginator="true"
      :rows="opition.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :scrollable="true"
      scrollHeight="flex"
      :totalRecords="opition.totalRecords"
      :row-hover="true"
      dataKey="group_id"
      v-model:selection="selectedProjectGroups"
      @page="onPage($event)"
      @sort="onSort($event)"
      @filter="onFilter($event)"
      :lazy="true"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-th-large"></i> Nhóm dự án
          <span>({{ opition.totalRecords }})</span>
        </h3>
        <div class="flex justify-content-center align-items-center">
          <Toolbar class="w-full custoolbar">
            <template #start>
              <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText
                  type="text"
                  spellcheck="false"
                  v-model="opition.search"
                  placeholder="Tìm kiếm"
                  @keyup.enter="loadData(true)"
                />
              </span>
            </template>

            <template #end>
              <Button
                @click="addProjectGroup('Thêm mới nhóm dự án')"
                label="Thêm mới"
                icon="pi pi-plus"
                class="mr-2"
              />
              <Button
                class="mr-2 p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                v-tooltip="'Tải lại'"
                @click="onRefersh"
              />
              <Button
                label="Xoá"
                icon="pi pi-trash"
                class="mr-2 p-button-danger"
                v-if="checkDelList"
                @click="DelProjectGroup(null)"
              />
              <!-- <Button label="Export" icon="pi pi-file-excel" class="mr-2 p-button-outlined p-button-secondary"
                @click="toggleExport" aria-haspopup="true" aria-controls="overlay_Export" /> -->
              <Menu
                id="overlay_Export"
                ref="menuButs"
                :model="itemButs"
                :popup="true"
              />
            </template>
          </Toolbar>
        </div>
      </template>
      <Column
        selectionMode="multiple"
        headerStyle="text-align:center;max-width:4rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:4rem;"
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="STT"
        header="STT"
        :sortable="true"
        headerStyle="text-align:center;max-width:6rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:6rem; "
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        field="group_name"
        header="Nhóm dự án"
        :sortable="true"
        headerStyle="height:3.125rem"
        bodyStyle=" "
      >
      </Column>
      <Column
        field="status"
        header="Hiển thị"
        headerStyle="text-align:center;max-width:7.5rem;height:3.125rem"
        bodyStyle="text-align:center;max-width:7.5rem; "
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
        field="full_name"
        header="Người tạo"
        headerStyle="text-align:center;max-width:150px;height:3.125rem"
        bodyStyle="text-align:center;max-width:150px; "
        class="align-items-center justify-content-center text-center"
      >
      </Column>
      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:7.5rem;height:3.125rem;min-width:9.375rem;"
        bodyStyle="text-align:center;max-width:7.5rem ;min-width:9.375rem"
      >
        <template #body="data">
          <div
            v-if="
              store.state.user.is_super == true ||
              store.state.user.user_id == data.data.created_by ||
              (store.state.user.role_id == 'admin' &&
                store.state.user.organization_id == data.data.organization_id)
            "
          >
            <Button
              @click="editProjectGroup(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
            ></Button>
            <Button
              @click="DelProjectGroup(data.data)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-trash"
              v-tooltip="'Xóa'"
            ></Button>
          </div>
        </template>
      </Column>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          style="
            min-height: calc(100vh - 210px);
            max-height: calc(100vh - 210px);
            display: flex;
            flex-direction: column;
          "
          v-if="!isFirst"
        >
          <img src="../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
  </div>
  <Dialog
    :header="headerAddProjectGroup"
    v-model:visible="displayProjectGroup"
    :closable="true"
    :maximizable="true"
    :style="{ width: '40vw' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0"
            >Nhóm dự án<span class="redsao"> (*) </span></label
          >
          <InputText
            v-model="ProjectGroup.group_name"
            spellcheck="false"
            class="col-8 ip36 px-2"
            :class="{
              'p-invalid':
                validateProjectGroup.group_name.$invalid && submitted,
            }"
          />
        </div>
        <div style="display: flex" class="field col-12 md:col-12">
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (validateProjectGroup.group_name.$invalid && submitted) ||
              validateProjectGroup.group_name.$pending.$response
            "
            class="col-9 p-error p-0"
          >
            <span class="col-12 p-0">{{
              validateProjectGroup.group_name.required.$message
                .replace("Value", "Tên nhóm dự án")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>

        <div style="display: flex" class="col-12 field md:col-12">
          <div class="field col-6 md:col-6 p-0">
            <label class="col-6 text-left p-0">STT </label>
            <InputNumber
              v-model="ProjectGroup.is_order"
              class="col-6 ip36 p-0"
            />
          </div>
          <div class="field col-6 md:col-6 p-0">
            <label
              style="vertical-align: text-bottom"
              class="col-6 text-center p-0"
              >Hiển thị
            </label>
            <InputSwitch v-model="ProjectGroup.status" class="col-6" />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogProjectGroup"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveProjectGroup(!validateProjectGroup.$invalid)"
      />
    </template>
  </Dialog>
</template>
