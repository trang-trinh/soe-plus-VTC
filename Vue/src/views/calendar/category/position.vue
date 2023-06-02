<script setup>
//Declare
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import treeuser from "../../../components/user/treeuser.vue";
import { encr } from "../../../util/function";
const cryoptojs = inject("cryptojs");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const emitter = inject("emitter");
const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const itemExportTrucban = "/Portals/Mau Excel/Mẫu Excel Người trực ban.xlsx";
const itemExportChihuy = "/Portals/Mau Excel/Mẫu Excel Người chỉ huy.xlsx";
const rules = {
  user_id: {
    required,
    $errors: [
      {
        $property: "user_id",
        $validator: "required",
        $message: "Tên phòng họp không được để trống!",
      },
    ],
  },
};

//Declare model
let files = [];
const model = ref({
  status: true,
  is_order: 1,
});

const isFirst = ref(true);
const datas_trucban = ref([]);
const datas_chihuy = ref([]);
const datas_mission = ref({});

//Valid Form
const v$ = useVuelidate(rules, model);
const toast = useToast();

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
  orderBy: "desc",
});
const dictionarys = ref([]);

//Function

//Function Log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const addUserLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddUserLog", log, config);
};

//Function chooser user
const isAdd = ref(true);
const submitted = ref(false);
const selectedUser = ref([]);
const is_one = ref(false);
const is_type = ref();
const headerDialogUser = ref();
const displayDialogUser = ref(false);
const openAddDialogUser = (type) => {
  isAdd.value = true;
  submitted.value = false;
  model.value = {
    is_order: 1,
    status: true,
    is_type: type,
  };

  displayDialogUser.value = true;
};
const closeDialogUser = () => {
  model.value = {
    is_order: 1,
    status: true,
  };
  displayDialogUser.value = false;
};
const showModalUser = (one, type) => {
  if (type) {
    switch (type) {
      case 0:
        selectedUser.value = [...datas_trucban.value];
        headerDialogUser.value = "Chọn người trực ban";
        break;
      case 1:
        selectedUser.value = [...datas_chihuy.value];
        headerDialogUser.value = "Chọn người chỉ huy";
        break;
      default:
        break;
    }
  }
  is_one.value = one;
  is_type.value = type;
  displayDialogUser.value = true;
};

//Function filter

//Function
const selectedNodeTrucban = ref([]);
const selectedNodeChihuy = ref([]);
const saveModel = () => {
  submitted.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  var ids = [];
  var filters = selectedUser.value.filter((x) => x["position_code"] == null);
  filters.forEach((item, i) => {
    ids.push(item["user_id"]);
  });
  let formData = new FormData();
  formData.append("type", is_type.value);
  formData.append("ids", JSON.stringify(ids));
  axios
    .put(
      baseURL + "/api/calendar_position/update_ca_position",
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
        is_type.value === 0
          ? "Thêm người trực ban thành công!"
          : "Thêm người chỉ huy thành công!"
      );
      initData(true);
      closeDialogUser();
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
const deleteItem = (type, item) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá người này không!",
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
          ids = [item["position_code"]];
        } else {
          if (type === 0) {
            if (selectedNodeTrucban.value.length > 0) {
              selectedNodeTrucban.value.forEach((row, i) => {
                ids.push(row["position_code"]);
              });
            }
          } else {
            if (selectedNodeChihuy.value.length > 0) {
              selectedNodeChihuy.value.forEach((row, i) => {
                ids.push(row["position_code"]);
              });
            }
          }
        }
        if (ids.length > 0) {
          axios
            .delete(baseURL + "/api/calendar_position/delete_ca_position", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data: ids,
            })
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
              initData(true);
              if (type === 0) {
                toast.success("Xoá người trực ban thành công!");
              } else {
                toast.success("Xoá người chỉ huy thành công!");
              }
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
  let data = { id: item.position_code, status: !(item.status || false) };
  axios
    .put(
      baseURL + "/api/calendar_position/update_status_ca_position",
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
const saveMission = (type) => {
  submitted.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  var sign_id = null;
  if (datas_mission.value.sign != null) {
    sign_id = datas_mission.value.sign.user_id;
  }
  let formData = new FormData();
  formData.append("type", type);
  formData.append("mission", datas_mission.value["mission"] || "");
  formData.append("address", datas_mission.value["address"] || "");
  formData.append("sign_id", sign_id);
  formData.append("model", JSON.stringify(datas_mission.value));
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    if (file["key"] === "signature") {
      formData.append("signature", file);
    } else if (file["key"] === "stamp") {
      formData.append("stamp", file);
    }
  }
  axios
    .put(baseURL + "/api/calendar_position/update_ca_mission", formData, config)
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
      files = [];
      clickChange.value = false;
      if (response.data.path_signature != null) {
        datas_mission.value.path_signature = response.data.path_signature;
      }
      if (response.data.path_stamp != null) {
        datas_mission.value.path_stamp = response.data.path_stamp;
      }
      swal.close();
      toast.success("Cập nhật thành công");
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

//Xuất excel
const menuButsTrucban = ref();
const itemButsTrucban = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportData(0);
    },
  },
  {
    label: "Nhập Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      importExcel(0);
    },
  },
]);
const menuButsChihuy = ref();
const itemButsChihuy = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportData(1);
    },
  },
  {
    label: "Nhập Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      importExcel(1);
    },
  },
]);
const toggleExportTrucban = (event) => {
  menuButsTrucban.value.toggle(event);
};
const toggleExportChihuy = (event) => {
  menuButsChihuy.value.toggle(event);
};
const exportData = (type) => {
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
        excelname: "DANH SÁCH " + (type === 0 ? "TRỰC BAN" : "CHỈ HUY"),
        proc: "calendar_ca_position_listexport",
        par: [
          { par: "type", va: type },
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "search", va: options.value.SearchText },
        ],
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();

        toast.success("Kết xuất Data thành công!");
        if (response.data.path != null) {
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
const importExcel = (type) => {
  imp.value = true;
  is_type.value = type;
};
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
  formData.append("type", is_type.value);
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url_file", file);
  }
  axios
    .post(baseURL + "/api/calendar_position/import_excel", formData, config)
    .then((response) => {
      toast.success("Nhập dữ liệu thành công");
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

//Con dấu
const clickChange = ref(false);
const chooseImage = (id) => {
  clickChange.value = true;
  document.getElementById(id).click();
};

const handleFileAvtUpload = (event, id) => {
  files = event.target.files;
  if (files && files.length > 0) {
    files.forEach((f) => {
      f.key = id;
    });
  }
  var output = document.getElementById(id);
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src);
    if (clickChange.value == true) {
      saveMission(3);
    }
  };
};

const deleteImage = (id) => {
  if (id === "signature") {
    datas_mission.value.path_signature = null;
  } else if (id === "stamp") {
    datas_mission.value.path_stamp = null;
  }
  saveMission(3);
};

//Nguuời ký

//Init
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_ca_position_dictionary",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
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
        dictionarys.value = tbs;
      }
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
const onRefresh = (type) => {
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

  axios
    .post(
      baseURL + "/api/calendar/get_datas",
      {
        str: encr(
          JSON.stringify({
            proc: "calendar_ca_position_list",
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
        let tbs = JSON.parse(data);
        if (tbs[0] != null && tbs[0].length > 0) {
          tbs[0].forEach((element, i) => {
            if (element["created_date"] != null) {
              var ldate = element["created_date"].split(" ");
              element["created_date"] = ldate[0];
            }
          });

          datas_trucban.value = tbs[0].filter((x) => x["is_type"] === 0);
          datas_trucban.value.forEach((item, i) => {
            item["STT"] = i + 1;
          });
          datas_chihuy.value = tbs[0].filter((x) => x["is_type"] === 1);
          datas_chihuy.value.forEach((item, i) => {
            item["STT"] = i + 1;
          });
        } else {
          datas_trucban.value = [];
          datas_chihuy.value = [];
        }
        if (tbs[1] != null && tbs[1].length > 0) {
          datas_mission.value = tbs[1][0];
          if (datas_mission.value.sign_id != null) {
            datas_mission.value.sign = {
              user_id: datas_mission.value.sign_id,
              full_name: datas_mission.value.full_name,
              last_name: datas_mission.value.last_name,
              avatar: datas_mission.value.avatar,
            };
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

onMounted(() => {
  initDictionary();
  initData(true);
});
</script>
<template>
  <div class="surface-100 p-3">
    <div class="box-scroll">
      <Toolbar class="outline-none surface-0 border-none">
        <template #start>
          <h3 class="module-title m-0">
            1. Danh sách trực ban ({{ datas_trucban.length }})
          </h3>
        </template>
        <template #end>
          <Button
            @click="showModalUser(false, 0)"
            label="Thêm mới người trực ban"
            icon="pi pi-plus"
            class="mr-2"
          />
          <Button
            @click="onRefresh(0)"
            class="mr-2 p-button-outlined p-button-secondary"
            v-tooltip="'Tải lại'"
            icon="pi pi-refresh"
          />
          <Button
            label="Xoá"
            icon="pi pi-trash"
            class="mr-2 p-button-danger"
            v-if="selectedNodeTrucban.length > 0"
            @click="deleteItem(0)"
          />
          <Button
            @click="toggleExportTrucban"
            label="Tiện ích"
            icon="pi pi-file-excel"
            class="mr-2 p-button-outlined p-button-secondary"
            aria-haspopup="true"
            aria-controls="overlay_Export"
          />
          <Menu
            :model="itemButsTrucban"
            :popup="true"
            id="overlay_Export"
            ref="menuButsTrucban"
          />
        </template>
      </Toolbar>
      <div class="d-lang-table mb-3">
        <DataTable
          :value="datas_trucban"
          :loading="options.loading"
          :totalRecords="options.total"
          :scrollable="true"
          :lazy="true"
          :rowHover="true"
          :showGridlines="true"
          v-model:selection="selectedNodeTrucban"
          dataKey="position_code"
          scrollHeight="flex"
          filterDisplay="menu"
          filterMode="lenient"
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
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
            headerStyle="text-align:center;max-width:75px;"
            bodyStyle="text-align:center;max-width:75px;"
            class="align-items-center justify-content-center text-center"
          >
          </Column>
          <Column
            field="avatar"
            header="Ảnh đại diện"
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:100px"
            bodyStyle="text-align:center;max-width:100px"
          >
            <template #body="slotProps">
              <div class="flex">
                <div class="format-center">
                  <Avatar
                    v-bind:label="
                      slotProps.data.avatar
                        ? ''
                        : (slotProps.data.last_name ?? '').substring(0, 1)
                    "
                    v-bind:image="basedomainURL + slotProps.data.avatar"
                    style="
                      background-color: #2196f3;
                      color: #ffffff;
                      width: 3rem;
                      height: 3rem;
                      font-size: 1.4rem !important;
                    "
                    :style="{
                      background: bgColor[slotProps.data.STT % 7],
                    }"
                    class="mr-2 text-avatar"
                    size="xlarge"
                    shape="circle"
                  />
                </div>
              </div>
            </template>
          </Column>
          <Column
            field="full_name"
            header="Họ và tên"
            headerStyle="max-width:auto;"
          >
          </Column>
          <Column
            field="position_name"
            header="Cấp bậc/Chức vụ"
            headerStyle="text-align:center;max-width:200px;"
            bodyStyle="text-align:center;max-width:200px;"
            class="align-items-center justify-content-center text-center"
          >
          </Column>
          <Column
            field="organization_name"
            header="Phòng/Ban"
            headerStyle="text-align:center;max-width:300px;"
            bodyStyle="text-align:center;max-width:300px;"
            class="align-items-center justify-content-center text-centẻ"
          >
          </Column>
          <Column
            field="status"
            header="Kích hoạt"
            headerStyle="text-align:center;max-width:120px;"
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
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:100px;"
            bodyStyle="text-align:center;max-width:100px;"
          >
            <template #body="data">
              <div
                v-if="
                  store.state.user.is_super == true ||
                  store.state.user.user_id == data.data.created_by ||
                  (store.state.user.role_id == 'admin' &&
                    store.state.user.organization_id ==
                      data.data.organization_id)
                "
              >
                <Button
                  @click="deleteItem(0, data.data)"
                  class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                  type="button"
                  v-tooltip="'Xóa'"
                  icon="pi pi-trash"
                ></Button>
              </div>
            </template>
          </Column>
          <template #empty>
            <div
              class="align-items-center justify-content-center p-4 text-center m-auto"
              v-if="!isFirst || options.total == 0"
              style="display: flex"
            >
              <div>
                <img src="../../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </div>
          </template>
        </DataTable>
      </div>
      <Toolbar class="outline-none surface-0 border-none">
        <template #start>
          <h3 class="module-title m-0">
            2. Danh sách chỉ huy ({{ datas_chihuy.length }})
          </h3>
        </template>
        <template #end>
          <Button
            @click="showModalUser(false, 1)"
            label="Thêm mới người chỉ huy"
            icon="pi pi-plus"
            class="mr-2"
          />
          <Button
            @click="onRefresh(1)"
            class="mr-2 p-button-outlined p-button-secondary"
            v-tooltip="'Tải lại'"
            icon="pi pi-refresh"
          />
          <Button
            label="Xoá"
            icon="pi pi-trash"
            class="mr-2 p-button-danger"
            v-if="selectedNodeChihuy.length > 0"
            @click="deleteItem(1)"
          />
          <Button
            @click="toggleExportChihuy"
            label="Tiện ích"
            icon="pi pi-file-excel"
            class="mr-2 p-button-outlined p-button-secondary"
            aria-haspopup="true"
            aria-controls="overlay_Export"
          />
          <Menu
            :model="itemButsChihuy"
            :popup="true"
            id="overlay_Export"
            ref="menuButsChihuy"
          />
        </template>
      </Toolbar>
      <div class="d-lang-table mb-3">
        <DataTable
          :value="datas_chihuy"
          :loading="options.loading"
          :totalRecords="options.total"
          :scrollable="true"
          :lazy="true"
          :rowHover="true"
          :showGridlines="true"
          v-model:selection="selectedNodeChihuy"
          dataKey="position_code"
          scrollHeight="flex"
          filterDisplay="menu"
          filterMode="lenient"
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
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
            headerStyle="text-align:center;max-width:75px;"
            bodyStyle="text-align:center;max-width:75px;"
            class="align-items-center justify-content-center text-center"
          >
          </Column>
          <Column
            field="avatar"
            header="Ảnh đại diện"
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:100px"
            bodyStyle="text-align:center;max-width:100px"
          >
            <template #body="slotProps">
              <div class="flex">
                <div class="format-center">
                  <Avatar
                    v-bind:label="
                      slotProps.data.avatar
                        ? ''
                        : (slotProps.data.last_name ?? '').substring(0, 1)
                    "
                    v-bind:image="basedomainURL + slotProps.data.avatar"
                    style="
                      background-color: #2196f3;
                      color: #ffffff;
                      width: 3rem;
                      height: 3rem;
                      font-size: 1.4rem !important;
                    "
                    :style="{
                      background: bgColor[slotProps.data.STT % 7],
                    }"
                    class="mr-2 text-avatar"
                    size="xlarge"
                    shape="circle"
                  />
                </div>
              </div>
            </template>
          </Column>
          <Column
            field="full_name"
            header="Họ và tên"
            headerStyle="max-width:auto;"
          >
          </Column>
          <Column
            field="position_name"
            header="Cấp bậc/Chức vụ"
            headerStyle="text-align:center;max-width:200px;"
            bodyStyle="text-align:center;max-width:200px;"
            class="align-items-center justify-content-center text-center"
          >
          </Column>
          <Column
            field="organization_name"
            header="Phòng/Ban"
            headerStyle="text-align:center;max-width:300px;"
            bodyStyle="text-align:center;max-width:300px;"
            class="align-items-center justify-content-center text-centẻ"
          >
          </Column>
          <Column
            field="status"
            header="Kích hoạt"
            headerStyle="text-align:center;max-width:120px;"
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
            class="align-items-center justify-content-center text-center"
            headerStyle="text-align:center;max-width:100px;"
            bodyStyle="text-align:center;max-width:100px;"
          >
            <template #body="data">
              <div
                v-if="
                  store.state.user.is_super == true ||
                  store.state.user.user_id == data.data.created_by ||
                  (store.state.user.role_id == 'admin' &&
                    store.state.user.organization_id ==
                      data.data.organization_id)
                "
              >
                <Button
                  @click="deleteItem(1, data.data)"
                  class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                  type="button"
                  v-tooltip="'Xóa'"
                  icon="pi pi-trash"
                ></Button>
              </div>
            </template>
          </Column>
          <template #empty>
            <div
              class="align-items-center justify-content-center p-4 text-center m-auto"
              v-if="!isFirst || options.total == 0"
              style="display: flex"
            >
              <div>
                <img src="../../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </div>
          </template>
        </DataTable>
      </div>
      <Toolbar class="outline-none surface-0 border-none">
        <template #start>
          <h3 class="module-title m-0">3. Nhiệm vụ trực ban</h3>
        </template>
        <template #end>
          <Button
            @click="saveMission(0)"
            label="Cập nhật"
            icon="pi pi-save"
            class="mr-2"
          />
        </template>
      </Toolbar>
      <div class="d-lang-table mb-3">
        <Editor
          class="html"
          v-model="datas_mission.mission"
          editorStyle="height: 220px"
        >
          <template v-slot:toolbar>
            <span class="ql-formats">
              <button class="ql-bold" v-tooltip.bottom="'Bold'"></button>
              <button class="ql-italic" v-tooltip.bottom="'Italic'"></button>
              <button
                class="ql-underline"
                v-tooltip.bottom="'Underline'"
              ></button>
            </span>
          </template>
        </Editor>
      </div>
      <Toolbar class="outline-none surface-0 border-none">
        <template #start>
          <h3 class="module-title m-0">4. Nơi nhận</h3>
        </template>
        <template #end>
          <Button
            @click="saveMission(1)"
            label="Cập nhật"
            icon="pi pi-save"
            class="mr-2"
          />
        </template>
      </Toolbar>
      <div class="d-lang-table mb-3">
        <Editor
          class="html"
          v-model="datas_mission.address"
          editorStyle="height: 220px"
        >
          <template v-slot:toolbar>
            <span class="ql-formats">
              <button class="ql-bold" v-tooltip.bottom="'Bold'"></button>
              <button class="ql-italic" v-tooltip.bottom="'Italic'"></button>
              <button
                class="ql-underline"
                v-tooltip.bottom="'Underline'"
              ></button>
            </span>
          </template>
        </Editor>
      </div>
      <div class="row">
        <div
          class="col-6 md:col-6 p-0"
          style="padding-right: 0.5rem !important"
        >
          <Toolbar class="outline-none surface-0 border-none">
            <template #start>
              <h3 class="module-title m-0">5. Con dấu</h3>
            </template>
          </Toolbar>
          <div class="d-lang-table mb-3">
            <div
              class="format-center"
              style="height: 200px; background-color: white"
            >
              <div
                class="relative"
                style="
                  width: 180px;
                  height: 180px;
                  box-shadow: 0px 0px 2px 2px rgb(0 0 0 / 10%);
                  cursor: pointer;
                  border-radius: 10px;
                "
              >
                <img
                  @click="chooseImage('imgSignature')"
                  id="signature"
                  class="w-full h-full"
                  :src="
                    datas_mission.path_signature
                      ? basedomainURL + datas_mission.path_signature
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                  alt=""
                  style="object-fit: contain; border-radius: 10px"
                />
                <div class="absolute top-0 right-0">
                  <Button
                    v-if="datas_mission.path_signature"
                    style="
                      width: 2rem;
                      height: 2rem;
                      background-color: red;
                      border: 1px solid red;
                    "
                    icon="pi pi-times"
                    @click="deleteImage('signature')"
                    class="p-button-rounded cursor-pointer btn-del-avt"
                  />
                </div>
                <input
                  id="imgSignature"
                  type="file"
                  accept="image/*"
                  @change="handleFileAvtUpload($event, 'signature')"
                  style="display: none"
                />
              </div>
            </div>
          </div>
        </div>
        <div class="col-6 md:col-6 p-0" style="padding-left: 0.5rem !important">
          <Toolbar class="outline-none surface-0 border-none">
            <template #start>
              <h3 class="module-title m-0">6. Chữ ký</h3>
            </template>
          </Toolbar>
          <div class="d-lang-table mb-3">
            <div
              class="format-center"
              style="height: 200px; background-color: white"
            >
              <div
                class="relative"
                style="
                  width: 180px;
                  height: 180px;
                  box-shadow: 0px 0px 2px 2px rgb(0 0 0 / 10%);
                  cursor: pointer;
                  border-radius: 10px;
                "
              >
                <img
                  @click="chooseImage('imgStamp')"
                  id="stamp"
                  class="w-full h-full"
                  :src="
                    datas_mission.path_stamp
                      ? basedomainURL + datas_mission.path_stamp
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                  alt=""
                  style="object-fit: contain; border-radius: 10px"
                />
                <div class="absolute top-0 right-0">
                  <Button
                    v-if="datas_mission.path_stamp"
                    style="
                      width: 2rem;
                      height: 2rem;
                      background-color: red;
                      border: 1px solid red;
                    "
                    icon="pi pi-times"
                    @click="deleteImage('stamp')"
                    class="p-button-rounded cursor-pointer btn-del-avt"
                  />
                </div>
                <input
                  id="imgStamp"
                  type="file"
                  accept="image/*"
                  @change="handleFileAvtUpload($event, 'stamp')"
                  style="display: none"
                />
              </div>
            </div>
          </div>
        </div>
        <div
          class="col-6 md:col-6 p-0"
          style="padding-right: 0.5rem !important"
        >
          <Toolbar class="outline-none surface-0 border-none">
            <template #start>
              <h3 class="module-title m-0">7. Người ký</h3>
            </template>
          </Toolbar>
          <div class="d-lang-table mb-3">
            <div class="format-center" style="background-color: white">
              <div class="col-6 md:col-6">
                <div class="form-group">
                  <Dropdown
                    :options="dictionarys[0]"
                    :filter="true"
                    :showClear="true"
                    v-model="datas_mission.sign"
                    optionLabel="full_name"
                    placeholder="Chọn người ký"
                    class="ip36"
                    @change="saveMission(4)"
                    style="height: auto; min-height: 36px; min-width: 120px"
                  >
                    <template #value="slotProps">
                      <div class="mt-2" v-if="slotProps.value">
                        <Chip
                          :image="slotProps.value.avatar"
                          :label="slotProps.value.full_name"
                          class="mr-2 mb-2 pl-0"
                        >
                          <div class="flex">
                            <div class="format-flex-center">
                              <Avatar
                                v-bind:label="
                                  slotProps.value.avatar
                                    ? ''
                                    : (
                                        slotProps.value.last_name ?? ''
                                      ).substring(0, 1)
                                "
                                v-bind:image="
                                  basedomainURL + slotProps.value.avatar
                                "
                                :style="{
                                  background:
                                    bgColor[slotProps.value.is_order % 7],
                                  color: '#fff',
                                  width: '3rem',
                                  height: '3rem',
                                }"
                                class="mr-2 text-avatar"
                                size="xlarge"
                                shape="circle"
                              />
                            </div>
                            <div class="format-center">
                              <span>{{ slotProps.value.full_name }}</span>
                            </div>
                          </div>
                        </Chip>
                      </div>
                      <span v-else> {{ slotProps.placeholder }} </span>
                    </template>
                    <template #option="slotProps">
                      <div v-if="slotProps.option" class="flex">
                        <div class="format-center">
                          <Avatar
                            v-bind:label="
                              slotProps.option.avatar
                                ? ''
                                : slotProps.option.last_name.substring(0, 1)
                            "
                            v-bind:image="
                              basedomainURL + slotProps.option.avatar
                            "
                            style="
                              background-color: #2196f3;
                              color: #ffffff;
                              width: 3rem;
                              height: 3rem;
                              font-size: 1.4rem !important;
                            "
                            :style="{
                              background: bgColor[slotProps.index % 7],
                            }"
                            class="text-avatar"
                            size="xlarge"
                            shape="circle"
                          />
                        </div>
                        <div class="format-center ml-3">
                          <span>{{ slotProps.option.full_name }}</span>
                        </div>
                      </div>
                      <span v-else> Chưa có dữ liệu tuần </span>
                    </template>
                  </Dropdown>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <!--treeuser-->
  <treeuser
    v-if="displayDialogUser === true"
    :headerDialog="headerDialogUser"
    :displayDialog="displayDialogUser"
    :closeDialog="closeDialogUser"
    :one="is_one"
    :selected="selectedUser"
    :choiceUser="saveModel"
  />
  <Dialog
    header="Tải lên file Excel"
    v-model:visible="imp"
    :style="{ width: '40vw' }"
    :closable="true"
  >
    <h3>
      <label>
        <a
          :href="
            basedomainURL +
            (is_type === 0 ? itemExportTrucban : itemExportChihuy)
          "
          download
          >Nhấn vào đây</a
        >
        để tải xuống tệp mẫu.
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
  <!--Dialog-->
</template>
<style scoped>
.form-group {
  display: grid;
  margin-bottom: 1rem;
  flex: 1;
}

.d-lang-table {
  /* height: calc(100vh - 160px); */
  min-height: unset;
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
.box-scroll {
  min-height: unset;
  max-height: calc(100vh - 75px);
  overflow-y: auto;
}
.html p {
  padding: 0 !important;
  margin: 0 !important;
}
.row {
  display: -webkit-box;
  display: -ms-flexbox;
  display: flex;
  -ms-flex-wrap: wrap;
  flex-wrap: wrap;
}
</style>
<style lang="scss" scoped>
::v-deep(.html) {
  .ql-editor p {
    padding: 0 !important;
    margin: 0 !important;
  }
}
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
    display: flex;
    align-items: center;
  }
  .p-chip img {
    margin: 0;
    width: 3rem;
    height: 3rem;
  }
  .p-avatar-text {
    font-size: 1rem;
  }
}
</style>
