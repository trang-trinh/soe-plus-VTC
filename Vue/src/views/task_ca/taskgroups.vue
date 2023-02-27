<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import moment from "moment";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const isDynamicSQL = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

//Khai báo
const isFirst = ref(false);
const expandedKeys = ref({});
const isChildren = ref(false);
const basedomainURL = fileURL;
const options = ref({
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
  allRecord: null,
  user: store.state.user.user_id,
  SearchText: "",
  sqlO: "is_order asc",
});
const datalists = ref();
const tempdata = ref();
const openDialog = ref(false);
const taskGroup = ref({
  parent_id: "",
  group_name: "",
  status: null,
  is_order: null,
  organization_id: null,
});
const editGroup = ref(false);
const submitted = ref(false);
const headerDialog = ref();
let user = store.state.user;
const ParentName = ref();
const selectedKeys = ref();
const selectedNodes = ref([]);
const first = ref();
const selectionMode = ref(
  user.is_super || user.is_admin ? "checkbox" : "multiple",
);
const filterTrangthai = ref();
const filterPhanloai = ref();
const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);
const phanLoai = ref([
  { name: "Hệ thống", code: 0 },
  { name: "Đơn vị", code: 1 },
]);
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
const treedonvis = ref();
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: () => {
      exportData("ExportExcel");
    },
  },
  {
    label: "Nhập Excel",
    icon: "pi pi-file-excel",
    command: () => {
      ImportExcel("ImportExcel");
    },
  },
]);
//Thêm log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
//Tải dữ liệu
const loadDonvi = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_org_list",
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      treedonvis.value = [];
      let data = JSON.parse(response.data.data)[0];
      //let sys = { key: 0, label: "Hệ thống", data: { organization_id: 0 } };

      //treedonvis.value.push(sys);

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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    });
};
const LoadData = (rf) => {
  if (rf) {
    if (isDynamicSQL.value) {
      loadDataSQL();
      return;
    }
    loadCount();
    axios
      .post(
        baseURL + "/api/TaskProc/getTaskData",
        {
          str: encr(
            JSON.stringify({
              proc: "task_ca_taskgroup_list",
              par: [
                { par: "pageno", va: options.value.user },
                { par: "pageno", va: options.value.PageNo },
                { par: "pagesize", va: options.value.PageSize },
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
        RenderData(data);
        options.value.loading = false;
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        // addLog({
        //   title: "Lỗi Console loadData",
        //   controller: "datasView.vue",
        //   logcontent: error.message,
        //   loai: 2,
        // });
        if (error && error.status === 401) {
          swal.fire({
            title: "Thông báo",
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            icon: "error",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  }
};

const RenderData = (data) => {
  datalists.value = [];
  options.value.allRecord = null;
  let arrChils = [];
  tempdata.value = data;
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.STT = options.value.PageNo * options.value.PageSize + (i + 1);
      let om = { key: m.group_id, data: m };
      const rechildren = (mm, group_id) => {
        let dts = data.filter((x) => x.parent_id == group_id);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em, j) => {
            em.STT = mm.data.STT + "." + (j + 1);
            let om1 = { key: em.group_id, data: em };
            om1.data.is_order = j + 1;
            rechildren(om1, em.group_id);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m.group_id);

      arrChils.push(om);
    });
  datalists.value = arrChils;
  if (datalists.value != null && datalists.value != []) isFirst.value = false;
  else isFirst.value = true;
};
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_ca_taskgroup_count",
            par: [{ par: "user_id", va: user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
      }
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCount",
        controller: "taskgroups.vue",
        logcontent: error.message,
        loai: 2,
      });
    });
};
const loadDataSQL = () => {
  let fpl;
  if (filterPhanloai.value != undefined && store.state.user.is_super) {
    fpl = parseInt(Object.keys(filterPhanloai.value)[0]);
  } else {
    fpl =
      filterPhanloai.value != undefined && filterPhanloai.value != null
        ? store.state.user.is_super
          ? filterPhanloai.value
          : filterPhanloai.value == 0
          ? 0
          : store.state.user.organization_id
        : null;
  }
  let data = {
    sqlS: filterTrangthai.value != null ? filterTrangthai.value : null,
    sqlO: options.value.sort,
    Search: options.value.SearchText,
    PageNo: options.value.PageNo,
    PageSize: options.value.PageSize,
    sqlF: fpl,
  };
  datalists.value = [];
  options.value.loading = true;
  axios
    .post(baseURL + "/api/SQL/Filter_TaskGroups", data, config)
    .then((response) => {
      let dt = JSON.parse(response.data.data);
      let data = dt[0];
      if (data.length > 0) {
        data.forEach((element, i) => {
          element.STT = options.value.PageNo * options.value.PageSize + i + 1;
          let om = { key: element.group_id, data: element };
          datalists.value.push(om);
        });
      } else {
        datalists.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      options.value.loading = false;
      //Show Count nếu có
      if (dt.length == 2) {
        options.value.totalRecords = dt[1][0].totalRecords;
      }
    })
    .catch((error) => {
      options.value.loading = false;
      toast.error("Tải dữ liệu không thành công!");
      addLog({
        title: "Lỗi Console loadData",
        controller: "SQLView.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//remake
const reload = () => {
  first.value = 0;
  options.value = {
    PageNo: 0,
    PageSize: 20,
    loading: true,
    totalRecords: null,
    allRecord: null,
    user: store.state.user.user_id,
    SearchText: "",
  };
  isDynamicSQL.value = false;
  LoadData(true);
};
const reFilter = () => {
  styleObj.value = "";
  filterTrangthai.value = "";
  filterPhanloai.value = "";
  LoadData(true);
};
//Thêm mới
const AddItem = (str) => {
  submitted.value = false;
  headerDialog.value = str;
  taskGroup.value = {
    parent_id: "",
    group_name: "",
    status: true,
    is_order: options.value.totalRecords + 1,
    organization_id: user.is_super ? 1 : user.organization_id,
  };
  if (options.value.totalRecords > 0) {
    taskGroup.value.is_order = options.value.totalRecords + 1;
  } else {
    taskGroup.value.is_order = 1;
  }
  openDialog.value = true;
};
const closeDialog = () => {
  openDialog.value = false;
  editGroup.value = false;
  LoadData(true);
};
const EditItem = (vl) => {
  headerDialog.value = "Sửa nhóm công việc";
  openDialog.value = true;
  taskGroup.value = vl;
  editGroup.value = true;
};
const AddChild = (vl) => {
  headerDialog.value = "Thêm nhóm công việc con";
  openDialog.value = true;
  submitted.value = false;
  taskGroup.value = {
    parent_id: vl.node.data.group_id,
    group_name: "",
    status: true,
    is_order: null,
    organization_id: user.is_super ? 1 : user.organization_id,
  };
  if (vl.node.children) {
    taskGroup.value.is_order = vl.node.children.length + 1;
  } else {
    taskGroup.value.is_order = 1;
  }
  openDialog.value = true;
  isChildren.value = true;
  ParentName.value = vl.node.data.group_name;
};
//validate
const rules = {
  group_name: {
    required,
  },
};
const v$ = useVuelidate(rules, taskGroup);
//Gửi dữ liệu
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
    method: editGroup.value ? "put" : "post",
    url:
      baseURL +
      `/api/taskGroups/${
        editGroup.value ? "Update_taskgroup" : "Add_taskgroup"
      }`,
    data: taskGroup.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success(
          response.config.method == "post"
            ? "Thêm mới nhóm công việc thành công!"
            : "Cập nhật nhóm công việc thành công!",
        );
        LoadData(true);
        closeDialog();
        editGroup.value = false;
        isChildren.value = false;
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html:
            ms.includes("group_name") == true
              ? "Tên nhóm công việc không quá 250 ký tự!"
              : ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        LoadData(true);
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const onCheckBox = (value) => {
  expandedKeys.value[value.group_id] = false;
  let data = {
    IntID: value.group_id,
    TextID: value.group_id + "",
    IntTrangthai: 1,
    BitTrangthai: value.status == true ? 1 : 0,
  };
  if (
    store.state.user.is_super == true ||
    store.state.user.user_id == value.created_by ||
    store.state.user.role_id == "admin"
  ) {
    axios
      .put(baseURL + "/api/taskGroups/Update_StatusTaskGroups", data, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa trạng thái nhóm công việc thành công!");
          LoadData(true);
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
    LoadData(true);
  }
};
const DeleteItem = (vl) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá nhóm công việc này không!",
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
          .delete(baseURL + "/api/taskGroups/Delete_taskgroup", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: vl != null ? [vl.group_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá nhóm công việc thành công!");
              LoadData(true);
            } else {
              swal.fire({
                title: "Thông báo",
                html: response.data.ms,
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const DeleteItems = () => {
  let arw = [];
  if (selectedKeys.value) {
    Object.keys(selectedKeys.value).forEach((key) => {
      arw.push(key);
    });
  }
  if (arw == null || arw == []) {
    return;
  }
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá nhóm công việc này không!",
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
          .delete(baseURL + "/api/taskGroups/Delete_taskgroup", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: arw,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá nhóm công việc thành công!");
              LoadData(true);
            } else {
              swal.fire({
                title: "Thông báo",
                html: response.data.ms,
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
  selectedKeys.value = null;
};
const onNodeSelect = () => {};
const onNodeUnselect = () => {};
const onPage = (event) => {
  if (event.rows != options.value.PageSize) {
    options.value.PageSize = event.rows;
  }
  LoadData(true);
};
//Excel
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const ImportExcel = () => {};
const exportData = () => {
  if (filterPhanloai.value == undefined) {
    options.value.filter_Org = 1;
  } else if (filterPhanloai.value == 0) {
    options.value.filter_Org = 3; //list hệ thống
  } else options.value.filter_Org = 2; // list đơn vị
  filterTrangthai.value =
    filterTrangthai.value != null
      ? filterTrangthai.value == 1
        ? true
        : false
      : null;
  filterPhanloai.value =
    filterPhanloai.value != null && filterPhanloai.value != ""
      ? filterPhanloai.value
      : null;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Excel/ExportExcel",
      {
        excelname: "DANH SÁCH NƠI NHẬN",
        proc: "ca_receive_listexport",
        par: [
          { par: "search", va: options.value.SearchText },
          { par: "status", va: filterTrangthai.value },
          { par: "user_id", va: store.state.user.user_id },
          { par: "s_org", va: filterPhanloai.value },
          { par: "filter_Org", va: options.value.filter_Org },
        ],
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
          title: "Thông báo",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
// Tìm kiếm
const searchItem = () => {
  isDynamicSQL.value = true;
  LoadData(true);
};
const filter = () => {
  styleObj.value = style.value;
  isDynamicSQL.value = true;
  LoadData(true);
};
onMounted(() => {
  LoadData(true);
  if (user.is_super == true) {
    loadDonvi();
  }
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <TreeTable
      sortMode="single"
      ref="dt"
      @nodeSelect="onNodeSelect"
      @nodeUnselect="onNodeUnselect"
      :value="datalists"
      :paginator="true"
      :rows="options.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :scrollable="true"
      scrollHeight="flex"
      v-model:selectionKeys="selectedKeys"
      v-model:first="first"
      :loading="options.loading"
      :expandedKeys="expandedKeys"
      :rowHover="true"
      :showGridlines="true"
      responsiveLayout="scroll"
      :totalRecords="options.totalRecords"
      :selectionMode="selectionMode"
      filterMode="lenient"
      @page="onPage($event)"
    >
      {{ datalists }}
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-money-bill"> </i> Danh sách nhóm công việc ({{
            options.totalRecords
          }})
        </h3>
        <Toolbar class="w-full custoolbar pb10">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup.enter="searchItem"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />

              <Button
                type="button"
                class="ml-2 p-button-outlined p-button-secondary"
                icon="pi pi-filter"
                @click="toggle"
                aria:haspopup="true"
                :style="[styleObj]"
                aria-controls="overlay_panel"
                v-tooltip="'Bộ lọc'"
              />
              <OverlayPanel
                ref="op"
                appendTo="body"
                class="p-0 m-0"
                :showCloseIcon="false"
                id="overlay_panel"
                :style="
                  store.state.user.is_super == 1 ? 'width:20vw' : 'width:300px'
                "
              >
                <div class="grid formgrid m-0">
                  <div class="flex field col-12 p-0">
                    <div
                      :class="
                        user.is_super == 1
                          ? 'col-2 text-left pt-2 p-0'
                          : 'col-4 text-left pt-2 p-0'
                      "
                      style="text-align: center,justify-content:center"
                    >
                      Trạng thái
                    </div>
                    <div :class="user.is_super == 1 ? 'col-10' : 'col-8'">
                      <Dropdown
                        class="col-12 p-0 m-0"
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
                          @click="reFilter"
                          class="p-button-outlined"
                          label="Xóa"
                        ></Button>
                      </template>
                      <template #end>
                        <Button
                          @click="filter"
                          label="Lọc"
                        ></Button>
                      </template>
                    </Toolbar>
                  </div>
                </div>
              </OverlayPanel>
            </span>
          </template>

          <template #end>
            <Button
              v-if="selectedKeys != null && user.is_admin"
              @click="DeleteItems()"
              label="Xóa"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
            />
            <Button
              @click="AddItem('Thêm mới')"
              label="Thêm mới"
              icon="pi pi-plus"
              class="mr-2"
              v-if="store.state.user.is_super || store.state.user.is_admin"
            />
            <Button
              @click="reload"
              class="mr-2 p-button-outlined p-button-secondary"
              v-tooltip="'Tải lại'"
              icon="pi pi-refresh"
            />

            <!-- <Button
              label="Tiện ích"
              icon="pi pi-file-excel"
              class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            /> -->
            <Menu
              id="overlay_Export"
              ref="menuButs"
              :model="itemButs"
              :popup="true"
            />
          </template>
        </Toolbar>
      </template>

      <Column
        field="STT"
        header="STT"
        :sortable="true"
        headerStyle="text-align:center;max-width:75px;height:50px"
        bodyStyle="text-align:center;max-width:75px;;max-height:600px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="menu">
          <div
            v-if="menu.node.data.parent_id == null"
            @click="onNodeSelect(menu.node.data)"
            style="font-weight: 1000"
          >
            {{ menu.node.data.STT }}
          </div>
          <div
            v-else
            style="font-weight: 500"
          >
            {{ menu.node.data.STT }}
          </div>
        </template>
      </Column>

      <Column
        field="group_name"
        header="Tên nhóm công việc"
        :expander="true"
        :sortable="true"
        headerStyle="height:50px;max-width:auto;"
        bodyStyle="max-height:600px"
      >
      </Column>
      <Column
        :sortable="true"
        field="created_date"
        header="Ngày tạo"
        headerStyle="text-align:center;max-width:10rem;height:50px"
        bodyStyle="text-align:center;max-width:10rem;max-height:600px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          {{
            moment(new Date(data.node.data.created_date)).format("DD/MM/YYYY")
          }}
        </template>
      </Column>
      <Column
        :sortable="true"
        field="created_by"
        header="Người tạo"
        headerStyle="text-align:center;max-width:240px;height:50px"
        bodyStyle="text-align:center;max-width:240px;max-height:600px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          {{ data.node.data.fullname }}
          <!-- <Avatar
            v-bind:label="
              data.node.data.avt
                ? ''
                : data.node.data.fullname.split(' ').at(-1).substring(0, 1)
            "
            :image="basedomainURL + data.node.data.avt"
            size="normal"
            shape="circle"
            v-tooltip.top="data.node.data.fullname"
            style="
              background-color: #2196f3;
              font-size: large;
              border-color: #ffffff;
            "
          /> -->
        </template>
      </Column>
      <Column
        field="status"
        header="Hiển thị"
        headerStyle="text-align:center;max-width:120px;height:50px"
        bodyStyle="text-align:center;max-width:120px;;max-height:600px"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <Checkbox
            :binary="data.node.data.status"
            v-model="data.node.data.status"
            @click="onCheckBox(data.node.data)"
          />
        </template>
      </Column>
      <Column
        header="Chức năng"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:200px;height:50px"
        bodyStyle="text-align:center;max-width:200px;;max-height:600px"
        v-if="store.state.user.is_super == true || user.is_admin"
      >
        <template #body="data">
          <Button
            @click="AddChild(data)"
            class="p-button-rounded p-button-secondary p-button-outlined mx-1"
            type="button"
            icon="pi pi-plus-circle"
            v-tooltip="'Thêm nhóm công việc con'"
          ></Button>

          <Button
            @click="EditItem(data.node.data)"
            class="p-button-rounded p-button-secondary p-button-outlined mx-1"
            type="button"
            icon="pi pi-pencil"
            v-tooltip="'Sửa'"
          ></Button>
          <Button
            @click="DeleteItem(data.node.data, true)"
            class="p-button-rounded p-button-secondary p-button-outlined mx-1"
            type="button"
            v-tooltip="'Xóa'"
            icon="pi pi-trash"
          ></Button>
        </template>
      </Column>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          v-if="!isFirst || options.allRecord == 0"
        >
          <img
            src="../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </TreeTable>
  </div>

  <Dialog
    :header="headerDialog"
    v-model:visible="openDialog"
    :style="{ width: '40vw' }"
    :closable="false"
  >
    <form>
      <div class="grid formgrid m-2">
        <div
          v-if="isChildren"
          class="field col-12 md:col-12"
        >
          <label class="col-3 text-left p-0"
            >Cấp cha<span class="redsao"></span
          ></label>
          <InputText
            v-model="ParentName"
            :disabled="true"
            class="col-8 ip36 px-2"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0"
            >Tên nhóm công việc <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="taskGroup.group_name"
            spellcheck="false"
            class="col-8 ip36 px-2"
            :class="{ 'p-invalid': v$.group_name.$invalid && submitted }"
          />
        </div>
        <div
          style="display: flex"
          class="field col-12 md:col-12 px-0"
        >
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (v$.group_name.$invalid && submitted) ||
              v$.group_name.$pending.$response
            "
            class="col-9 p-error"
          >
            <span class="col-12 p-0">{{
              v$.group_name.required.$message
                .replace("Value", "Tên nhóm công việc")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div
          style="display: flex"
          class="col-12 field md:col-12"
        >
          <div class="field col-6 md:col-6 p-0">
            <label class="col-6 text-left p-0">STT </label>

            <InputNumber
              v-model="taskGroup.is_order"
              class="col-6 ip36 p-0"
            />
          </div>
          <div class="field col-6 md:col-6 p-0 flex align-items-center">
            <label class="col-6 text-center p-0">Trạng thái </label>
            <InputSwitch
              v-model="taskGroup.status"
              style="justify-content: center; align-items: center"
            />
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

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveData(!v$.$invalid)"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.p-avatar-text {
  font-size: 1.5rem !important;
  color: black;
}
.p-treeselect-panel {
  max-width: 14vw !important;
}
.p-treeselect-panel .p-treeselect-items-wrapper .p-tree {
  max-height: 15vh !important;
}
.p-dropdown-item {
  white-space: normal !important;
}
</style>
