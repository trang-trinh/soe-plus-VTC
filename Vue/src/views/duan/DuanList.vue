<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import TrangthaiDuanView from "../../components/duan/TrangthaiDuan.vue";
import moment from "moment";
//init Model
const store = inject("store");
const swal = inject("$swal");
const router = inject("router");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const duan = ref({
  Duan_Ten: "",
  STT: 1,
  Trangthai: 1,
});
//Valid Form
const submitted = ref(false);
const rules = {
  Duan_Ten: {
    required,
  },
};
const v$ = useVuelidate(rules, duan);
//Khai báo biến
const isAdd = ref(true);
const selectedKey = ref();
const selectedNodes = ref([]);
const filters = ref({});
const opition = ref({
  search: "",
  PageNo: 1,
  PageSize: 8,
  user_id: store.getters.user.user_id,
  Sort: "Ngaytao",
  Order: "desc",
  Type: 0,
});
const duans = ref();
const modeldate = ref({});
const displayAddDuan = ref(false);
const isFirst = ref(true);
let files = {};
const toast = useToast();
const layout = ref("grid");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const tdTrangthais = ref([
  { value: 0, text: "Đang lập kế hoạch" },
  { value: 1, text: "Đang thực hiện" },
  { value: 2, text: "Đã hoàn thành" },
  { value: 3, text: "Tạm dừng" },
  { value: 4, text: "Đóng" },
]);
const tdLoais = ref([
  { value: 0, text: "Dự án chính" },
  { value: 1, text: "Dự án ngoài" },
]);
const tdUsers = ref([]);
const tdTukhoas = ref([]);
const menuButs = ref();
const menuButMores = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportDuan("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportDuan("ExportExcelMau");
    },
  },
]);
const itemButMores = ref([
  {
    label: "Chi tiết dự án",
    icon: "pi pi-info-circle",
    command: (event) => {
      goDuan(duan.value);
    },
  },
  {
    label: "Sửa dự án",
    icon: "pi pi-pencil",
    command: (event) => {
      editDuan(duan.value);
    },
  },
  {
    label: "Xoá dự án",
    icon: "pi pi-trash",
    command: (event) => {
      delDuan(duan.value);
    },
  },
]);

//Khai báo function
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const toggleMores = (event, u) => {
  duan.value = u;
  menuButMores.value.toggle(event);
};
const onNodeSelect = (node) => {
  selectedNodes.value.push(node.data.Duan_ID);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(selectedNodes.value.indexOf(node.data.Duan_ID), 1);
};
const handleFileUpload = (event, ianh) => {
  files[ianh] = event.target.files[0];
  var output = document.getElementById(ianh);
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
//Show Modal
const showModalAddDuan = () => {
  submitted.value = false;
  duan.value = {
    Duan_Ten: "",
    STT: duans.value.length + 1,
    Trangthai: 1,
    IsLoaiDuan: 0,
  };
  modeldate.value = {};
  isAdd.value = true;
  displayAddDuan.value = true;
};
const chonanh = (id) => {
  document.getElementById("ip" + id).click();
};
const closedisplayAddDuan = () => {
  displayAddDuan.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  opition.value.PageNo = 1;
  opition.value.PageSize = 20;
  loadDuan(true);
};
const onSearch = () => {
  loadDuan(true);
};
const initTudien = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Plan_Duan_ListTudien",
        par: [{ par: "Duan_ID", va: opition.value.Duan_ID }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        tdUsers.value = data[0];
        tdTukhoas.value = data[1];
      }
    })
    .catch((error) => {});
};
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Plan_Duan_Count",
        par: [
          { par: "Search", va: opition.value.search },
          { par: "Tukhoa", va: opition.value.Tukhoa },
          { par: "user_id", va: opition.value.user_id },
          { par: "Trangthai", va: opition.value.Trangthai },
          { par: "StartDate", va: opition.value.StartDate },
          { par: "EndDate", va: opition.value.EndDate },
          { par: "Type ", va: opition.value.Type },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        opition.value.totalRecords = data[0].totalRecords;
      }
    })
    .catch((error) => {});
};
const onPage = (event) => {
  opition.value.PageNo = event.page + 1;
  opition.value.PageSize = event.rows;
  loadDuan(true);
};
const loadDuan = (rf) => {
  if (rf) {
    opition.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
    if (opition.value.PageNo == 1) loadCount();
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Plan_Duan_List",
        par: [
          { par: "Search", va: opition.value.search },
          { par: "Tukhoa", va: opition.value.Tukhoa },
          { par: "user_id", va: opition.value.user_id },
          { par: "PageNo", va: opition.value.PageNo },
          { par: "PageSize", va: opition.value.PageSize },
          { par: "Trangthai", va: opition.value.Trangthai },
          { par: "StartDate", va: opition.value.StartDate },
          { par: "EndDate", va: opition.value.EndDate },
          { par: "Sort", va: opition.value.Sort },
          { par: "Order", va: opition.value.Order },
          { par: "Type ", va: opition.value.Type },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        data.forEach(function (r) {
          if (r.Ngaytao)
            r.NgaytaoS = moment(new Date(r.Ngaytao)).format("DD/MM/YYYY HH:mm:ss");
          if (r.NgayBD)
            r.NgayBDS = moment(new Date(r.NgayBD)).format("DD/MM/YYYY HH:mm:ss");
          if (r.NgayKT)
            r.NgayKTS = moment(new Date(r.NgayKT)).format("DD/MM/YYYY HH:mm:ss");
          if (r.NgayKT) r.NgayKTD = moment(new Date(r.NgayKT)).format("DD/MM/YYYY");
          if (r.NgayTH)
            r.NgayTHS = moment(new Date(r.NgayTH)).format("DD/MM/YYYY HH:mm:ss");
          if (r.NgayHT)
            r.NgayHTS = moment(new Date(r.NgayHT)).format("DD/MM/YYYY HH:mm:ss");
          if (r.Users) {
            r.Users = JSON.parse(r.Users);
          }
        });
        duans.value = data;
      } else {
        duans.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      if (rf) {
        opition.value.loading = false;
        swal.close();
      }
    })
    .catch((error) => {
      console.log(error);
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const goDuan = (md) => {
  //router.push({ path: "/project/" + md.Duan_ID });
  router.push({ name: "projectdetail", params: { duanid: md.Duan_ID } });
};
const editDuan = (md) => {
  submitted.value = false;
  isAdd.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddDuan.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Plan_Duan_Get",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "Duan_ID", va: md.Duan_ID },
        ],
      },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        let obj = data[0][0];
        if (obj.Tukhoa) obj.Tukhoa = obj.Tukhoa.split(",");
        modeldate.value.thoigiankehoach = [];
        if (obj.NgayBD) {
          modeldate.value.thoigiankehoach.push(new Date(obj.NgayBD));
        }
        if (obj.NgayKT) {
          modeldate.value.thoigiankehoach.push(new Date(obj.NgayKT));
        }
        if (modeldate.value.thoigiankehoach.length == 0) {
          modeldate.value.thoigiankehoach = null;
        }
        duan.value = obj;
        //
        modeldate.value.thuchiens = data[1]
          .filter((x) => x.TypeUser == 1)
          .map((x) => ({
            user_id: x.user_id,
            full_name: x.full_name,
            Avartar: x.Avartar,
          }));
        modeldate.value.quanlys = data[1]
          .filter((x) => x.TypeUser == 2)
          .map((x) => ({
            user_id: x.user_id,
            full_name: x.full_name,
            Avartar: x.Avartar,
          }));
        modeldate.value.theodoi = data[1]
          .filter((x) => x.TypeUser == 0)
          .map((x) => ({
            user_id: x.user_id,
            full_name: x.full_name,
            Avartar: x.Avartar,
          }));
      }
    })
    .catch((error) => {
      console.log(error);
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
  addDuan();
};
const addDuan = () => {
  let formData = new FormData();
  for (const [key, value] of Object.entries(files)) {
    formData.append(key, value);
  }
  let md = { ...duan.value };
  if (md.Tukhoa instanceof Array) {
    md.Tukhoa = md.Tukhoa.join(",");
  }
  if (modeldate.value.thoigiankehoach) {
    if (modeldate.value.thoigiankehoach.length > 0) {
      md.NgayBD = modeldate.value.thoigiankehoach[0];
    }
    if (modeldate.value.thoigiankehoach.length > 1) {
      md.NgayKT = modeldate.value.thoigiankehoach[1];
    }
  }
  let users = [];
  if (modeldate.value.thuchiens) {
    modeldate.value.thuchiens.forEach((u) => {
      users.push({ user_id: u.user_id, TypeUser: 1, Trangthai: true, IsCheck: false });
    });
  }
  if (modeldate.value.quanlys) {
    modeldate.value.quanlys.forEach((u) => {
      users.push({ user_id: u.user_id, TypeUser: 2, Trangthai: true, IsCheck: false });
    });
  }
  if (modeldate.value.theodoi) {
    modeldate.value.theodoi.forEach((u) => {
      users.push({ user_id: u.user_id, TypeUser: 0, Trangthai: true, IsCheck: false });
    });
  }
  if (users.length > 0) {
    formData.append("users", JSON.stringify(users));
  }
  formData.append("model", JSON.stringify(md));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: isAdd.value == false ? "put" : "post",
    url: baseURL + `/api/Duan/${isAdd.value == false ? "Update_Duan" : "Add_Duan"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật Dự án thành công!");
        loadDuan();
        closedisplayAddDuan();
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

const delDuan = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá dự án này không!",
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
          .delete(baseURL + "/api/Duan/Del_Duan", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.Duan_ID] : selectedNodes.value,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá Dự án thành công!");
              loadDuan();
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

const exportDuan = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH DỰ ÁN",
        proc: "Plan_Duans_ListExport",
        par: [
          { par: "Search", va: opition.value.search },
          { par: "Duan_ID", va: opition.value.Duan_ID },
          { par: "Role_ID", va: opition.value.Role_ID },
          { par: "IsAdmin", va: opition.value.IsAdmin },
          { par: "Trangthai", va: opition.value.Trangthai },
          { par: "StartDate", va: opition.value.StartDate },
          { par: "EndDate", va: opition.value.EndDate },
        ],
      },
      config
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

onMounted(() => {
  //init
  loadDuan(true);
  initTudien();
  return {
    displayAddDuan,
    isFirst,
    opition,
    showModalAddDuan,
    closedisplayAddDuan,
    addDuan,
    editDuan,
    onSearch,
    delDuan,
    handleFileUpload,
    chonanh,
    v$,
    handleSubmit,
    submitted,
    basedomainURL,
    filters,
    onRefersh,
    itemButs,
    menuButs,
    toggleExport,
    selectedKey,
    onNodeSelect,
    onNodeUnselect,
    selectedNodes,
    isAdd,
    layout,
    toggleMores,
    itemButMores,
    menuButMores,
    onPage,
    tdTukhoas,
  };
});
</script>
<template>
  <div class="main-layout flex-grow-1 p-2" v-if="store.getters.islogin">
    <DataView
      class="w-full h-full p-datatable-sm e-sm flex flex-column"
      :lazy="true"
      :value="duans"
      :layout="layout"
      :loading="opition.loading"
      :paginator="opition.totalRecords > opition.PageSize"
      :rows="opition.PageSize"
      :totalRecords="opition.totalRecords"
      :pageLinkSize="opition.PageSize"
      @page="onPage($event)"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[8, 12, 20, 50, 100]"
      currentPageReportTemplate=""
      responsiveLayout="scroll"
      :scrollable="true"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-building"></i> Dự án
          <span v-if="opition.totalRecords > 0"
            >({{ opition.totalRecords.toLocaleString() }})</span
          >
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <Dropdown
              :showClear="true"
              v-model="opition.Trangthai"
              :options="tdTrangthais"
              optionLabel="text"
              optionValue="value"
              placeholder="Lọc theo trạng thái"
              @change="loadDuan(true)"
            />
            <span class="p-input-icon-left ml-2">
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
            <DataViewLayoutOptions v-model="layout" />
            <Button
              class="mr-2 ml-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              @click="onRefersh"
            />
            <Button
              label="Xoá"
              icon="pi pi-trash"
              class="mr-2 p-button-danger"
              v-if="selectedNodes.length > 0"
              @click="delDuan"
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
              label="Thêm Dự án"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddDuan"
            />
          </template>
        </Toolbar>
      </template>
      <template #grid="slotProps">
        <div class="col-12 md:col-3 p-2">
          <Card>
            <template #title>
              <div style="position: relative">
                <div class="align-items-center justify-content-center text-center">
                  <Avatar
                    v-bind:label="
                      slotProps.data.Logo ? '' : slotProps.data.Duan_Ten.substring(0, 1)
                    "
                    v-bind:image="basedomainURL + slotProps.data.Logo"
                    style="color: #000; width: 8rem; height: 8rem"
                    class="mr-2"
                    size="xlarge"
                    shape="circle"
                  />
                </div>
                <Avatar
                  v-tooltip.right="'Người tạo: ' + slotProps.data.full_nameTao"
                  v-bind:label="
                    slotProps.data.AvartarTao
                      ? ''
                      : slotProps.data.full_nameTao.substring(0, 1)
                  "
                  v-bind:image="basedomainURL + slotProps.data.AvartarTao"
                  style="
                    position: absolute;
                    left: 0px;
                    top: 0px;
                    background-color: #2196f3;
                    color: #ffffff;
                    vertical-align: middle;
                  "
                  class="mr-2"
                  size="small"
                  shape="circle"
                />
                <Button
                  style="position: absolute; right: 0px; top: 0px"
                  icon="pi pi-ellipsis-h"
                  class="p-button-rounded p-button-text ml-2"
                  @click="toggleMores($event, slotProps.data)"
                  aria-haspopup="true"
                  aria-controls="overlay_More"
                />
                <Menu
                  id="overlay_More"
                  ref="menuButMores"
                  :model="itemButMores"
                  :popup="true"
                />
              </div>
            </template>
            <template #content>
              <div class="text-center">
                <Button
                  class="p-button-text p-button-plain block m-auto mb-2"
                  @click="goDuan(slotProps.data)"
                >
                  <h3 class="m-0 textoneline">{{ slotProps.data.Duan_Ten }}</h3>
                </Button>
                <TrangthaiDuanView
                  :Trangthai="slotProps.data.Trangthai"
                ></TrangthaiDuanView>
              </div>
              <div class="flex flex-row align-items-center justify-content-center">
                <div
                  v-if="slotProps.data.NgayKTS"
                  class="text-right spandate"
                  v-tooltip.top="'Ngày kết thúc (' + slotProps.data.NgayKTS + ')'"
                >
                  {{ slotProps.data.NgayKTD }}
                </div>
                <div class="flex flex-grow-1 justify-content-end">
                  <AvatarGroup class="mt-2" v-if="slotProps.data.Users">
                    <Avatar
                      v-for="item in slotProps.data.Users.slice(0, 3)"
                      :key="item.PlanUser_ID"
                      v-bind:label="item.Avartar ? '' : item.full_name.substring(0, 1)"
                      v-bind:image="basedomainURL + item.Avartar"
                      style="
                        background-color: #2196f3;
                        color: #ffffff;
                        vertical-align: middle;
                      "
                      class="mr-2"
                      size="small"
                      shape="circle"
                    />
                    <Avatar
                      v-if="slotProps.data.Users && slotProps.data.Users.length > 3"
                      v-bind:label="'+' + (slotProps.data.Users.length - 3).toString()"
                      shape="circle"
                      size="small"
                      style="background-color: #2196f3; color: #ffffff"
                    />
                  </AvatarGroup>
                </div>
              </div>
            </template>
          </Card>
        </div>
      </template>
      <template #list="slotProps">
        <div class="p-2 w-full" style="background-color: #fff">
          <div class="flex align-items-center justify-content-center">
            <Avatar
              v-bind:label="
                slotProps.data.Logo ? '' : slotProps.data.Duan_Ten.substring(0, 1)
              "
              v-bind:image="basedomainURL + slotProps.data.Logo"
              style="color: #000000"
              class="mr-2"
              size="xlarge"
              shape="circle"
            />
            <div class="flex flex-grow-1 flex-column">
              <Button
                class="p-button-text p-button-plain block"
                @click="goDuan(slotProps.data)"
              >
                <h3 class="m-0 textoneline text-left" v-ripple>
                  {{ slotProps.data.Duan_Ten }}
                </h3>
              </Button>
              <div class="flex ml-2">
                <Avatar
                  v-tooltip.right="'Người tạo: ' + slotProps.data.full_nameTao"
                  v-bind:label="
                    slotProps.data.AvartarTao
                      ? ''
                      : slotProps.data.full_nameTao.substring(0, 1)
                  "
                  v-bind:image="basedomainURL + slotProps.data.AvartarTao"
                  style="
                    background-color: #2196f3;
                    color: #ffffff;
                    vertical-align: middle;
                  "
                  class="mr-2 mt-1"
                  size="small"
                  shape="circle"
                />
                <div>
                  <div class="spduan-name">{{ slotProps.data.full_nameTao }}</div>
                  <div class="spduan-ngay">{{ slotProps.data.NgaytaoS }}</div>
                </div>
              </div>
            </div>
            <AvatarGroup class="ml-2 mr-2" v-if="slotProps.data.Users">
              <Avatar
                v-for="item in slotProps.data.Users.slice(0, 3)"
                :key="item.PlanUser_ID"
                v-bind:label="item.Avartar ? '' : item.full_name.substring(0, 1)"
                v-bind:image="basedomainURL + item.Avartar"
                style="background-color: #2196f3; color: #ffffff; vertical-align: middle"
                class="mr-2"
                size="small"
                shape="circle"
              />
              <Avatar
                v-if="slotProps.data.Users && slotProps.data.Users.length > 3"
                v-bind:label="'+' + (slotProps.data.Users.length - 3).toString()"
                shape="circle"
                size="small"
                style="background-color: #2196f3; color: #ffffff"
              />
            </AvatarGroup>
            <div
              v-if="slotProps.data.NgayKTS"
              class="text-right spandate mt-0"
              v-tooltip.top="'Ngày kết thúc (' + slotProps.data.NgayKTS + ')'"
            >
              {{ slotProps.data.NgayKTD }}
            </div>
            <TrangthaiDuanView :Trangthai="slotProps.data.Trangthai"></TrangthaiDuanView>
            <Button
              icon="pi pi-ellipsis-h"
              class="p-button-outlined p-button-secondary ml-2"
              @click="toggleMores($event, slotProps.data)"
              aria-haspopup="true"
              aria-controls="overlay_More"
            />
            <Menu
              id="overlay_More"
              ref="menuButMores"
              :model="itemButMores"
              :popup="true"
            />
          </div>
        </div>
      </template>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center"
          v-if="!isFirst"
        >
          <img src="../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataView>
  </div>
  <Dialog
    header="Cập nhật Dự án"
    v-model:visible="displayAddDuan"
    :style="{ width: '960px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Mã </label>
          <InputText
            spellcheck="false"
            placeholder=""
            v-bind:disabled="!isAdd"
            class="col-4 ip36"
            v-model="duan.Duan_ID"
          />
          <label class="col-2 text-right">Loại dự án</label>
          <Dropdown
            class="col-4"
            :showClear="true"
            v-model="duan.IsLoaiDuan"
            :options="tdLoais"
            optionLabel="text"
            optionValue="value"
            placeholder="Chọn loại"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tên <span class="redsao">(*)</span></label>
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="duan.Duan_Ten"
            :class="{ 'p-invalid': v$.Duan_Ten.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.Duan_Ten.$invalid && submitted) || v$.Duan_Ten.$pending.$response"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.Duan_Ten.required.$message
                .replace("Value", "Tên dự án")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Thời gian từ </label>
          <Calendar
            class="col-4 ml-0 p-0"
            id="thoigiankehoach"
            v-model="modeldate.thoigiankehoach"
            selectionMode="range"
            :showIcon="true"
            :manualInput="true"
          />
          <label class="col-2 text-right">Từ khoá</label>
          <Chips
            spellcheck="false"
            class="col-4 p-0"
            v-model="duan.Tukhoa"
            :addOnBlur="true"
            separator=","
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Người thực hiện</label>
          <MultiSelect
            v-model="modeldate.thuchiens"
            :virtualScrollerOptions="{ itemSize: 10 }"
            :options="tdUsers"
            optionLabel="full_name"
            placeholder="Chọn người thực hiện"
            :filter="true"
            class="col-10 multiselect-thuchien"
          >
            <template #value="slotProps">
              <div
                class="user-item user-item-value"
                v-for="option of slotProps.value"
                :key="option.user_id"
              >
                <Avatar
                  v-bind:label="option.Avartar ? '' : option.Duan_Ten.substring(0, 1)"
                  v-bind:image="basedomainURL + option.Avartar"
                  style="background-color: #2196f3; color: #ffffff"
                  class="mr-2"
                  shape="circle"
                />
                <div>{{ option.full_name }}</div>
              </div>
              <template v-if="!slotProps.value || slotProps.value.length === 0">
                Chọn người thực hiện
              </template>
            </template>
            <template #option="slotProps">
              <div class="user-item">
                <Avatar
                  v-bind:label="
                    slotProps.option.Avartar
                      ? ''
                      : slotProps.option.full_name.substring(0, 1)
                  "
                  v-bind:image="basedomainURL + slotProps.option.Avartar"
                  style="background-color: #2196f3; color: #ffffff"
                  class="mr-2"
                  shape="circle"
                />
                <div>{{ slotProps.option.full_name }}</div>
              </div>
            </template>
          </MultiSelect>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Người quản lý</label>
          <MultiSelect
            v-model="modeldate.quanlys"
            :virtualScrollerOptions="{ itemSize: 10 }"
            :options="tdUsers"
            optionLabel="full_name"
            placeholder="Chọn người quản lý"
            :filter="true"
            class="col-10 multiselect-quanly"
          >
            <template #value="slotProps">
              <div
                class="user-item user-item-value"
                v-for="option of slotProps.value"
                :key="option.user_id"
              >
                <Avatar
                  v-bind:label="option.Avartar ? '' : option.Duan_Ten.substring(0, 1)"
                  v-bind:image="basedomainURL + option.Avartar"
                  style="background-color: #22c55e; color: #ffffff"
                  class="mr-2"
                  shape="circle"
                />
                <div>{{ option.full_name }}</div>
              </div>
              <template v-if="!slotProps.value || slotProps.value.length === 0">
                Chọn người quản lý
              </template>
            </template>
            <template #option="slotProps">
              <div class="user-item">
                <Avatar
                  v-bind:label="
                    slotProps.option.Avartar
                      ? ''
                      : slotProps.option.full_name.substring(0, 1)
                  "
                  v-bind:image="basedomainURL + slotProps.option.Avartar"
                  style="background-color: #22c55e; color: #ffffff"
                  class="mr-2"
                  shape="circle"
                />
                <div>{{ slotProps.option.full_name }}</div>
              </div>
            </template>
          </MultiSelect>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Người Theo dõi</label>
          <MultiSelect
            v-model="modeldate.theodoi"
            :virtualScrollerOptions="{ itemSize: 10 }"
            :options="tdUsers"
            optionLabel="full_name"
            placeholder="Chọn người theo dõi"
            :filter="true"
            class="col-10 multiselect-theodoi"
          >
            <template #value="slotProps">
              <div
                class="user-item user-item-value"
                v-for="option of slotProps.value"
                :key="option.user_id"
              >
                <Avatar
                  v-bind:label="option.Avartar ? '' : option.Duan_Ten.substring(0, 1)"
                  v-bind:image="basedomainURL + option.Avartar"
                  style="background-color: #64748b; color: #ffffff"
                  class="mr-2"
                  shape="circle"
                />
                <div>{{ option.full_name }}</div>
              </div>
              <template v-if="!slotProps.value || slotProps.value.length === 0">
                Chọn người theo dõi
              </template>
            </template>
            <template #option="slotProps">
              <div class="user-item">
                <Avatar
                  v-bind:label="
                    slotProps.option.Avartar
                      ? ''
                      : slotProps.option.full_name.substring(0, 1)
                  "
                  v-bind:image="basedomainURL + slotProps.option.Avartar"
                  style="background-color: #64748b; color: #ffffff"
                  class="mr-2"
                  shape="circle"
                />
                <div>{{ slotProps.option.full_name }}</div>
              </div>
            </template>
          </MultiSelect>
        </div>
        <div class="col-8">
          <label class="col-12 text-rigth">Mô tả</label>
          <div class="p-2">
            <Editor
              v-model="duan.Mota"
              spellcheck="false"
              editorStyle="height: 200px;font-size:14px"
            />
          </div>
        </div>
        <div class="col-4">
          <div class="field">
            <label class="col-12 text-rigth">Logo</label>
            <div class="inputanh" @click="chonanh('Logo')">
              <img
                id="Logo"
                v-bind:src="
                  duan.Logo ? basedomainURL + duan.Logo : '/src/assets/image/noimg.jpg'
                "
              />
            </div>
            <input
              class="ipnone"
              id="ipLogo"
              type="file"
              accept="image/*"
              @change="handleFileUpload($event, 'Logo')"
            />

            <label class="col-12 text-rigth mt-2">Hình nền</label>
            <div
              class="inputanh w-full"
              style="height: 115px"
              @click="chonanh('Hinhanh')"
            >
              <img
                id="Hinhanh"
                v-bind:src="
                  duan.Hinhanh
                    ? basedomainURL + duan.Hinhanh
                    : '/src/assets/image/noimg.jpg'
                "
              />
            </div>
            <input
              class="ipnone"
              id="ipHinhanh"
              type="file"
              accept="image/*"
              @change="handleFileUpload($event, 'Hinhanh')"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-4 ip36 p-0" v-model="duan.STT" />

          <label class="col-2 text-right">Trạng thái</label>
          <Dropdown
            class="col-4"
            v-model="duan.Trangthai"
            :options="tdTrangthais"
            optionLabel="text"
            optionValue="value"
            placeholder="Chọn trạng thái"
          />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddDuan"
        class="p-button-raised p-button-secondary"
      />
      <Button
        label="Cập nhật"
        icon="pi pi-save"
        @click="handleSubmit(!v$.$invalid)"
        
      />
    </template>
  </Dialog>
</template>
<style scoped>
.spduan-name {
  font-weight: 500;
  color: #6c757d;
  font-size: 0.87rem;
}
.spduan-ngay {
  color: #6c757d;
  font-size: 0.87rem;
}
.ipnone {
  display: none;
}
.inputanh {
  border: 1px solid #ccc;
  width: 96px;
  height: 96px;
  cursor: pointer;
  background-color: #eee;
  padding: 1px;
}
.inputanh img {
  object-fit: contain;
  width: 100%;
  height: 100%;
}
.spandate {
  background-color: #eee;
  padding: 5px 10px;
  width: max-content;
  border-radius: 5px;
  margin: 10px 0;
  height: fit-content;
  margin-bottom: 0;
  margin-right: 5px;
  font-weight: 500;
  color: #6c757d;
  font-size: 0.85rem;
}
.user-item {
  display: -webkit-box;
  display: -ms-flexbox;
  display: flex;
  -webkit-box-align: center;
  -ms-flex-align: center;
  align-items: center;
}
.user-item-value {
  padding: 0.25rem 0.5rem;
  border-radius: 3px;
  display: -webkit-inline-box;
  display: -ms-inline-flexbox;
  display: inline-flex;
  margin-right: 0.5rem;
  background-color: var(--primary-color);
  color: var(--primary-color-text);
}
.multiselect-thuchien .user-item-value {
  background-color: var(--primary-color);
}
.multiselect-quanly .user-item-value {
  background-color: #15803d;
}
.multiselect-theodoi .user-item-value {
  background-color: #64748b;
}
</style>
