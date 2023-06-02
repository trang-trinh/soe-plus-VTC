<script setup>
//Declare
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../../util/function";
const cryoptojs = inject("cryptojs");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const isDynamicSQL = ref(false);
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const itemExport = "/Portals/Mau Excel/Mẫu Excel Phòng họp.xlsx";
const rules = {
  boardroom_name: {
    required,
    $errors: [
      {
        $property: "boardroom_name",
        $validator: "required",
        $message: "Tên phòng họp không được để trống!",
      },
    ],
  },
};

//Declare model
const model = ref({
  boardroom_name: "",
  status: true,
  is_order: 1,
});

//Valid Form
const submitted = ref(false);
const v$ = useVuelidate(rules, model);
const toast = useToast();
const basedomainURL = baseURL;

const selectedKeys = ref([]);
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const options = ref({
  isNext: true,
  loading: false,
  user_id: store.getters.user.user_id,
  search: "",
  pageNo: 0,
  pageSize: 20,
  total: 0,
  sort: "created_date desc",
  orderBy: "DESC",
});
const typeOrderBy = ref([
  { name: "Mới nhất", code: 0, value: "created_date" },
  { name: "Thứ tự", code: 1, value: "is_order" },
]);
const expandedKeys = ref({});
const onNodeSelect = (node) => {
  if (expandedKeys.value[node.data.place_id] == true) {
    expandedKeys.value[node.data.place_id] = false;
  } else {
    expandedKeys.value[node.data.place_id] = true;
  }
};
const selectedNodes = ref([]);

//Declare filter
const isFirst = ref(true);
const datas = ref([]);
const filterSQL = ref([]);

//Function Log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const addUserLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddUserLog", log, config);
};

//Function
const searchData = () => {
  options.value.pageNo = 0;
  isDynamicSQL.value = false;
  initData(true);
};
//filter trạng thái
const showFilter = ref(false);
const create_date_form = ref();
const create_date_to = ref();
const filterHienthi = ref();
const styleObj = ref();
const trangThai = ref([
  { name: "Hiển thị", code: 1 },
  { name: "Không hiển thị", code: 0 },
]);
const reFilter = (event) => {
  create_date_form.value = null;
  create_date_to.value = null;
  filterHienthi.value = null;
  filters();
  showFilter.value = false;
  styleObj.value = "";
};
const filters = () => {
  styleObj.value = "";
  filterSQL.value = [];
  let arr = [];
  let obj = {};
  if (create_date_form.value != null) {
    if (typeof create_date_form.value == "string") {
      var eDay = create_date_form.value.split("/");
      create_date_form.value = new Date(
        eDay[2] + "/" + eDay[1] + "/" + eDay[0]
      );
    }
    obj.key = "created_date";
    obj.filteroperator = "and";
    arr = [];
    arr.push({
      matchMode: "dateAfter",
      value: create_date_form.value,
    });
    obj.filterconstraints = arr;
    filterSQL.value.push(obj);
    styleObj.value = style.value;
  }
  let obj1 = {};
  if (create_date_to.value != null) {
    if (typeof create_date_to.value == "string") {
      var eDay = create_date_to.value.split("/");
      create_date_to.value = new Date(eDay[2] + "/" + eDay[1] + "/" + eDay[0]);
    }
    obj1.key = "created_date";
    obj1.filteroperator = "and";
    arr = [];
    arr.push({
      matchMode: "dateBefore",
      value: create_date_to.value,
    });
    obj1.filterconstraints = arr;
    filterSQL.value.push(obj1);
    styleObj.value = style.value;
  }
  let obj2 = {};
  if (filterHienthi.value != null) {
    obj2.key = "status";
    obj2.filteroperator = "and";
    arr = [];
    arr.push({
      matchMode: "equals",
      value: filterHienthi.value,
    });
    obj2.filterconstraints = arr;
    filterSQL.value.push(obj2);
  }
  options.value.pageNo = 0;
  options.value.total = 0;
  isDynamicSQL.value = true;
  initDataSQL();
  showFilter.value = false;
};

const onPage = (event) => {
  if (event.rows != options.value.pageSize) {
    options.value.pageSize = event.rows;
  }
  if (event.page == 0) {
    //Trang đầu
    options.value.id = null;
    options.value.isNext = true;
  } else if (event.page > options.value.pageNo + 1) {
    //Trang cuối
    options.value.id = -1;
    options.value.isNext = false;
  } else if (event.page > options.value.pageNo) {
    //Trang sau
    options.value.id = datas.value[datas.value.length - 1].place_id;
    options.value.isNext = true;
  } else if (event.page < options.value.pageNo) {
    //Trang trước
    options.value.id = datas.value[0].place_id;
    options.value.isNext = false;
  }
  options.value.pageNo = event.page;
  initData(true);
};
const onSort = (event) => {
  if (event.sortField == "is_order") {
    options.value.sort = "is_order";
  } else {
    options.value.sort = "boardroom_name";
  }
  options.value.sort += event.sortOrder == 1 ? " asc" : " desc";

  isDynamicSQL.value = true;
  initDataSQL();
};

//Function
//Bộ lọc
const op = ref();
const toggle = (event) => {
  op.value.toggle(event);
};

//Xuất excel
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportData("ExportExcel");
    },
  },
  {
    label: "Nhập Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      importExcel(event);
    },
  },
]);
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const exportData = (method) => {
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
        excelname: "DANH SÁCH PHÒNG HỌP",
        proc: "calendar_ca_boardroom_listexport",
        par: [
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "search", va: options.value.search },
        ],
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

        toast.success("Kết xuất Data thành công!");
        if(response.data.path != null){
          window.open(baseURL + response.data.path);
        }
      } else {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      }
    });
};

// Import excel
const imp = ref(false);
const importExcel = () => {
  imp.value = true;
};
let files = [];
const removeFile = (event) => {
  files = [];
};
const selectFile = (event) => {
  event.files.forEach((element) => {
    files.push(element);
  });
};
const upload = () => {
  imp.value = false;
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  axios
    .post(baseURL + "/api/calendar_boardroom/import_excel", formData, config)
    .then((response) => {
      toast.success("Nhập dữ liệu thành công");
      isDynamicSQL.value = false;
      initData(true);
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

const headerDialog = ref();
const displayDialog = ref(false);
const isAdd = ref(true);
const openAddDialog = (str) => {
  submitted.value = false;
  model.value = {
    boardroom_name: "",
    is_order: 1,
    status: true,
  };
  if (options.value.total > 0) {
    model.value.is_order = options.value.total + 1;
  } else {
    model.value.is_order = 1;
  }
  headerDialog.value = str;
  displayDialog.value = true;
  isAdd.value = true;
};
const closeDialog = () => {
  model.value = {
    name: "",
    is_order: 1,
    status: true,
  };
  displayDialog.value = false;
};
const saveModel = (isFormValid) => {
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
  if (model.value.boardroom_name.length > 500) {
    swal.fire({
      title: "Thông báo!",
      text: "Tên phòng họp không vượt quá 500 ký tự!",
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
    .put(
      baseURL + "/api/calendar_boardroom/update_ca_boardroom",
      formData,
      config
    )
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
        isAdd.value
          ? "Thêm phòng họp thành công!"
          : "Cập nhật phòng họp thành công!"
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
            proc: "calendar_ca_boardroom_get",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "boardroom_id", va: item.boardroom_id },
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

      headerDialog.value = "Sửa phòng họp";
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      }
    });
};
const delItem = (item) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá phòng họp này không!",
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
          ids = [item["boardroom_id"]];
        } else {
          if (selectedNodes.value.length > 0) {
            selectedNodes.value.forEach((row, i) => {
              ids.push(row["boardroom_id"]);
            });
          }
        }
        axios
          .delete(baseURL + "/api/calendar_boardroom/delete_ca_boardroom", {
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
            toast.success("Xoá phòng họp thành công!");
            initData(true);
            // if (ids.length > 0) {
            //   ids.forEach((element, i) => {
            //     var idx = datas.value.findIndex(
            //       (x) => x.user_id == element.boardroom_id
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
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
              return;
            }
          });
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
  let data = { id: item.boardroom_id, status: !(item.status || false) };
  axios
    .put(
      baseURL + "/api/calendar_boardroom/update_status_ca_boardroom",
      data,
      config
    )
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

//Init
const onRefresh = () => {
  selectedNodes.value = false;
  options.value = {
    isNext: true,
    loading: false,
    user_id: store.getters.user.user_id,
    search: "",
    pageNo: 0,
    pageSize: 20,
    total: 0,
    sort: "created_date desc",
    orderBy: "DESC",
  };
  isDynamicSQL.value = false;
  initData(true);
};
const initData = (rf) => {
  if (rf) {
    options.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  if (isDynamicSQL.value) {
    initDataSQL();
    return;
  }

  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_ca_boardroom_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "page_no", va: options.value.pageNo },
              { par: "page_size", va: options.value.pageSize },
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
        let tbs = JSON.parse(data);
        if (tbs[0] != null && tbs[0].length > 0) {
          tbs[0].forEach((element, i) => {
            if (element["created_date"] != null) {
              var ldate = element["created_date"].split(" ");
              element["created_date"] = ldate[0];
            }
          });

          datas.value = tbs[0];
          if (datas.value.length > 0) {
            datas.value.forEach((element, i) => {
              element["STT"] =
                options.value.pageNo * options.value.pageSize + i + 1;
            });
          }
        } else {
          datas.value = [];
        }
        if (tbs.length == 2) {
          options.value.total = tbs[1][0].total;
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
const initDataSQL = () => {
  let par = {
    filter_organization_id: store.getters.user.organization_id,
    page_no: options.value.pageNo,
    page_size: options.value.pageSize,
    search: options.value.search,
    fields: filterSQL.value,
    order_by: options.value.sort,
  };
  axios
    .post(
      baseURL + "/api/calendar_boardroom/filter_calendar_ca_boardroom_list",
      par,
      config
    )
    .then((response) => {
      var data = response.data;
      if (data != null) {
        if (data["err"] != "0") {
          swal.fire({
            title: "Thông báo!",
            text:
              data["err"] == "2"
                ? response.data.ms
                : "Lọc dữ liệu không thành công.",
            icon: "error",
            confirmButtonText: "OK",
          });
          return;
        }

        let tbs = JSON.parse(data.data);
        if (tbs[0] != null && tbs[0].length > 0) {
          tbs[0].forEach((element, i) => {
            if (element["created_date"] != null) {
              var ldate = element["created_date"].split(" ");
              element["created_date"] = ldate[0];
            }
          });

          datas.value = tbs[0];
          if (datas.value.length > 0) {
            datas.value.forEach((element, i) => {
              element["STT"] =
                options.value.pageNo * options.value.pageSize + i + 1;
            });
          }
        } else {
          datas.value = [];
        }
        if (tbs.length == 2) {
          options.value.total = tbs[1][0].total;
        }
        swal.close();
        if (isFirst.value) isFirst.value = false;
        if (options.value.loading) options.value.loading = false;
      }
    })
    .catch((error) => {
      options.value.loading = false;
      toast.error("Tải dữ liệu không thành công!");
      addLog({
        title: "Lỗi Console initDataSQL",
        controller: "boardroom.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      }
    });
};
onMounted(() => {
  initData(true);
  return {};
});
</script>
<template>
  <div class="surface-100 p-3">
    <Toolbar class="outline-none surface-0 border-none pb-0">
      <template #start>
        <div>
          <h3 class="module-title m-0">
            <i class="pi pi-briefcase"></i> Danh sách phòng họp ({{
              options.total
            }})
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
            placeholder=" Tìm kiếm phòng họp"
          />
        </span>
        <Button
          @click="toggle"
          :style="[styleObj]"
          type="button"
          class="ml-2 p-button-outlined p-button-secondary"
          icon="pi pi-filter"
          aria:haspopup="true"
          aria-controls="overlay_panel"
          v-tooltip="'Bộ lọc'"
        />
        <OverlayPanel
          :showCloseIcon="false"
          ref="op"
          appendTo="body"
          class="p-0 m-0"
          id="overlay_panel"
          style="width: 300px"
        >
          <div class="grid formgrid m-0">
            <div class="flex field col-12 p-0">
              <div class="col-4 text-left pt-2 p-0" style="text-align: left">
                Trạng thái
              </div>
              <div class="col-8">
                <Dropdown
                  class="col-12 p-0 m-0"
                  v-model="filterHienthi"
                  :options="trangThai"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Trạng thái"
                />
              </div>
            </div>
            <div class="flex col-12 p-0">
              <Toolbar class="border-none surface-0 outline-none pb-0 w-full">
                <template #start>
                  <Button
                    @click="reFilter"
                    class="p-button-outlined"
                    label="Xóa"
                  ></Button>
                </template>
                <template #end>
                  <Button @click="filters" label="Lọc"></Button>
                </template>
              </Toolbar>
            </div>
          </div>
        </OverlayPanel>
      </template>
      <template #end>
        <Button
          @click="openAddDialog('Thêm mới')"
          label="Thêm mới"
          icon="pi pi-plus"
          class="mr-2"
        />
        <Button
          @click="onRefresh"
          class="mr-2 p-button-outlined p-button-secondary"
          v-tooltip="'Tải lại'"
          icon="pi pi-refresh"
        />
        <Button
          label="Xoá"
          icon="pi pi-trash"
          class="mr-2 p-button-danger"
          v-if="selectedNodes.length > 0"
          @click="delItem()"
        />
        <Button
          @click="toggleExport"
          label="Tiện ích"
          icon="pi pi-file-excel"
          class="mr-2 p-button-outlined p-button-secondary"
          aria-haspopup="true"
          aria-controls="overlay_Export"
        />
        <Menu
          :model="itemButs"
          :popup="true"
          id="overlay_Export"
          ref="menuButs"
        />
      </template>
    </Toolbar>
    <div class="d-lang-table">
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
        dataKey="boardroom_id"
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
          :sortable="true"
        >
        </Column>
        <Column
          field="boardroom_name"
          header="Tên phòng họp"
          headerStyle="height:50px;max-width:auto;"
          :sortable="true"
        >
        </Column>
        <Column
          field="status"
          header="Hiển thị"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="data">
            <Checkbox
              :binary="data.data.status"
              v-model="data.data.status"
              @click="udpateStatusItem(data.data)"
            /> </template
        ></Column>
        <Column
          header="Chức năng"
          headerStyle="text-align:center;max-width:200px;height:50px"
          bodyStyle="text-align:center;max-width:200px;"
          class="align-items-center justify-content-center text-center"
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
                @click="editItem(data.data)"
                class="
                  p-button-rounded p-button-secondary p-button-outlined
                  mx-1
                "
                type="button"
                icon="pi pi-pencil"
                v-tooltip="'Sửa'"
              ></Button>
              <Button
                @click="delItem(data.data, true)"
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
    :closable="false"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0"
            >Tên phòng họp <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="model.boardroom_name"
            spellcheck="false"
            class="col-8 ip36 px-2"
            :class="{
              'p-invalid': v$.boardroom_name.$invalid && submitted,
            }"
          />
        </div>
        <div style="display: flex" class="field col-12 md:col-12">
          <div class="col-4 text-left"></div>
          <small
            v-if="
              (v$.boardroom_name.$invalid && submitted) ||
              v$.boardroom_name.$pending.$response
            "
            class="col-8 p-error"
          >
            <span class="col-12 p-0">{{
              v$.boardroom_name.required.$message
                .replace("Value", "Tên phòng họp")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div style="display: flex" class="col-12 field md:col-12">
          <div class="field col-6 md:col-6 p-0">
            <label class="col-6 text-left p-0">Số thứ tự </label>
            <InputText v-model="model.is_order" class="col-6 ip36" />
          </div>
          <div class="field col-6 md:col-6 p-0">
            <label
              style="vertical-align: text-bottom"
              class="col-6 text-center p-0"
              >Hiển thị
            </label>
            <InputSwitch v-model="model.status" />
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

      <Button label="Lưu" icon="pi pi-check" @click="saveModel(!v$.$invalid)" />
    </template>
  </Dialog>

  <Dialog
    header="Tải lên file Excel"
    v-model:visible="imp"
    :style="{ width: '40vw' }"
    :closable="true"
  >
    <h3>
      <label>
        <a :href="basedomainURL + itemExport" download>Nhấn vào đây</a> để tải
        xuống tệp mẫu.
      </label>
    </h3>
    <form>
      <FileUpload
        accept=".xls,.xlsx"
        @remove="removeFile"
        @select="selectFile"
        :multiple="false"
        :show-upload-button="false"
        choose-label="Chọn tệp"
        cancel-label="Hủy"
      >
        <template #empty>
          <p>Kéo và thả tệp vào đây để tải lên.</p>
        </template>
      </FileUpload>
    </form>
    <template #footer
      ><Button label="Lưu" icon="pi pi-check" @click="upload"
    /></template>
    <!-- Chức năng đang chỉnh sửa vui lòng liên hệ quản trị viên phần mềm -->
  </Dialog>
</template>
<style scoped>
@import url(../component/stylecalendar.css);
</style>
<style scoped>
.d-lang-table {
  height: calc(100vh - 160px);
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
</style>
<style lang="scss" scoped>
</style>
