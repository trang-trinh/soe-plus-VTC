<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
//init Model
const monhoc = ref({
  Monhoc_Ten: "",
  STT: 1,
  Trangthai: true,
});
//Valid Form
const submitted = ref(false);
const rules = {
  Monhoc_Ten: {
    required,
  },
};
const v$ = useVuelidate(rules, monhoc);
//Khai báo biến
const isAdd = ref(true);
const selectedNodes = ref([]);
const filters = ref({});
const monhocs = ref();
const displayAddMonhoc = ref(false);
const isFirst = ref(true);
const toast = useToast();
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const selectDonvi = ref();
const treedonvis = ref();
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
  PageSize: 50,
  Filteruser_id: null,
  user_id: store.getters.user.user_id,
});
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: () => {
      exportMonhoc("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: () => {
      exportMonhoc("ExportExcelMau");
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
const showModalAddMonhoc = () => {
  selectAll.value = false;
  isAdd.value = true;
  submitted.value = false;
  monhoc.value = {
    Monhoc_Ten: "",
    STT: monhocs.value.length + 1,
    Trangthai: true,
    IsType: 0,
    IsSystem: store.getters.user.IsSupper,
    Loaimon_ID: tudiens.value[0][0].value,
    user_id: store.getters.user.user_id,
    Truonghoc_ID: store.getters.user.IsSupper ? null : store.getters.user.Donvi_ID,
    Maunen: "#eeeeee",
    Mauchu: "#000",
  };
  displayAddMonhoc.value = true;
};
const closedisplayAddMonhoc = () => {
  displayAddMonhoc.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.PageNo = 1;
  opition.value.search = "";
  loadMonhoc(true);
};
const onSearch = () => {
  loadMonhoc(true);
};
const onPage = (event) => {
  opition.value.PageNo = event.page + 1;
  opition.value.PageSize = event.rows;
  loadMonhoc(true);
};
const loadMonhoc = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Sys_Monhoc_List",
        par: [
          { par: "user_id", va: opition.value.user_id },
          { par: "PageNo", va: opition.value.PageNo },
          { par: "PageSizes", va: opition.value.PageSize },
          { par: "Search ", va: opition.value.search },
          { par: "Truonghoc_ID ", va: null },
          { par: "IsSystem", va: store.getters.user.IsSupper ? true : null },
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
          m.IsPermission =
            store.getters.user.IsSupper || m.Nguoicapnhat == store.getters.user.user_id;
        });
        monhocs.value = data[0];
        opition.value.totalRecords = data[1][0].totalRecords;
      } else {
        monhocs.value = [];
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
const editMonhoc = (md) => {
  isAdd.value = false;
  submitted.value = false;
  selectAll.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddMonhoc.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "Sys_Monhoc_Get", par: [{ par: "Monhoc_ID", va: md.Monhoc_ID }] },
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
        monhoc.value = data[0][0];
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
const opColor = ref();
let pcolor = "";
const toggleColor = (event, pc) => {
  opColor.value.toggle(event);
  pcolor = pc;
};
const changeColor = (color) => {
  monhoc.value[pcolor] = color.hex;
  if (!color.hex.includes("#")) opColor.value.hide();
};
const handleSubmit = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  addMonhoc();
};
const addMonhoc = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let md = { ...monhoc.value };
  if (md.Khoihocs) md.Khoihocs = md.Khoihocs.join(",");
  axios({
    method: isAdd.value == false ? "put" : "post",
    url: baseURL + `/api/Monhoc/${isAdd.value == false ? "Update_Monhoc" : "Add_Monhoc"}`,
    data: md,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật môn học thành công!");
        loadMonhoc();
        closedisplayAddMonhoc();
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

const delMonhoc = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá môn học này không!",
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
          .delete(baseURL + "/api/Monhoc/Del_Monhoc", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data:
              md != null ? [md.Monhoc_ID] : selectedNodes.value.map((x) => x.Monhoc_ID),
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá môn học thành công!");
              loadMonhoc();
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

const upTrangthaiMonhoc = (md) => {
  let ids = [md.Monhoc_ID];
  let tts = [md.Trangthai];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/Monhoc/Update_TrangthaiMonhoc",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái môn học thành công!");
        loadMonhoc();
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

const exportMonhoc = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "Sys_Monhoc" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH MÔN HỌC",
        proc: "Sys_Monhoc_ListExport",
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
const tudiens = ref([]);
const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.Capcha_ID == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.Capcha_ID == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em };
            rechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m[id]);
      arrChils.push(om);
      //
      om = { key: m[id], data: m[id], label: m[name] };
      const retreechildren = (mm, pid) => {
        let dts = data.filter((x) => x.Capcha_ID == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em[id], label: em[name] };
            retreechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      retreechildren(om, m[id]);
      arrtreeChils.push(om);
    });
  arrtreeChils.unshift({ key: -1, data: -1, label: "-----Chọn " + title + "----" });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const loadTudien = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Sys_Monhoc_Tudiens",
        par: [{ par: "user_id", va: opition.value.user_id }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        data[3].forEach((ch) => {
          ch.items = data[2].filter((x) => x.Caphoc_ID == ch.value);
          ch.chon = false;
        });
        data[3] = data[3].filter((x) => x.items.length > 0);
        tudiens.value = data;
        let obj = renderTree(data[1], "Donvi_ID", "organization_name", "đơn vị");
        treedonvis.value = obj.arrtreeChils;
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
    monhoc.value.Khoihocs = [];
    tudiens.value[3].forEach((el) => {
      el.items.forEach((kk) => {
        monhoc.value.Khoihocs.push(kk.value);
      });
    });
  } else {
    monhoc.value.Khoihocs = [];
  }
};
const checkkhoi = (khoi) => {
  console.log(khoi);
  var arr = monhoc.value.Khoihocs || [];
  if (khoi.chon) {
    khoi.items.forEach((kk) => {
      arr.push(kk.value);
    });
  } else {
    arr = arr.filter((x) => khoi.items.findIndex((kk) => kk.value == x).length == -1);
  }
  monhoc.value.Khoihocs = arr;
};
onMounted(() => {
  //init
  loadMonhoc(true);
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
      :value="monhocs"
      :paginator="opition.totalRecords > opition.PageSize"
      :loading="opition.loading"
      :totalRecords="opition.totalRecords"
      :rows="opition.PageSize"
      dataKey="Monhoc_ID"
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
      rowGroupMode="subheader"
      groupRowsBy="Loaimon_Ten"
    >
      <template #groupheader="slotProps">
        <i class="pi pi-list mr-2"></i> {{ slotProps.data.Loaimon_Ten }}
      </template>
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-list"></i> Môn học
          <span v-if="monhocs">({{ opition.totalRecords }})</span>
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
              @click="delMonhoc()"
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
              label="Thêm môn học"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddMonhoc"
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
        field="Monhoc_ID"
        header="Mã"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:80px"
        bodyStyle="text-align:center;max-width:80px"
      ></Column>
      <Column
        field="Monhoc_Ten"
        header="Tên môn"
        :sortable="true"
        headerStyle="text-align:center;"
        bodyStyle="text-align:center;"
      >
        <template #body="md">
          <Chip
            :style="{ background: md.data.Maunen, color: md.data.Mauchu }"
            v-bind:label="md.data.Monhoc_Ten"
            class="mr-2 mb-2"
          />
        </template>
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
        field="Loaimon_Ten"
        header="Loại môn"
        headerStyle="max-width:200px"
        bodyStyle="max-width:200px"
      ></Column>
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
            :disabled="!md.data.IsPermission"
            @change="md.data.IsPermission ? upTrangthaiMonhoc(md.data) : null"
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
            v-if="md.data.IsPermission"
            type="button"
            icon="pi pi-pencil"
            class="p-button-rounded p-button-sm p-button-info"
            style="margin-right: 0.5rem"
            @click="editMonhoc(md.data)"
          ></Button>
          <Button
            v-if="md.data.IsPermission"
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delMonhoc(md.data)"
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
    header="Cập nhật môn học"
    v-model:visible="displayAddMonhoc"
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
            v-model="monhoc.Monhoc_ID"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tên môn <span class="redsao">(*)</span></label>
          <InputText
            spellcheck="false"
            class="col-4 ip36"
            v-model="monhoc.Monhoc_Ten"
            :class="{ 'p-invalid': v$.Monhoc_Ten.$invalid && submitted }"
          />
          <label class="col-2 text-right">Viết tắt</label>
          <InputText
            spellcheck="false"
            class="col-4 ip36"
            v-model="monhoc.Monhoc_TenVT"
          />
        </div>
        <small
          v-if="(v$.Monhoc_Ten.$invalid && submitted) || v$.Monhoc_Ten.$pending.$response"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.Monhoc_Ten.required.$message
                .replace("Value", "Tên")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Loại môn</label>
          <Dropdown
            class="col-10 ip36 p-0"
            v-model="monhoc.Loaimon_ID"
            :options="tudiens[0]"
            optionLabel="text"
            optionValue="value"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label style="vertical-align: text-bottom" class="col-2 text-left">Loại</label>
          <Dropdown
            class="col-4 ip36 p-0"
            v-model="monhoc.IsType"
            :options="tdTypes"
            optionLabel="text"
            optionValue="value"
          />
          <label style="vertical-align: text-bottom" class="col-2 text-right"
            >Hệ thống</label
          >
          <InputSwitch v-model="monhoc.IsSystem" class="mt-1" />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Màu chữ</label>
          <Button
            class="p-button-rounded p-button-outlined p-button-secondary col-4"
            :style="{
              backgroundColor: monhoc.Mauchu,
              color: monhoc.Mauchu ? 'transparent' : '#333',
              border: '1px solid #ccc',
            }"
            type="button"
            icon="pi pi-palette"
            @click="toggleColor($event, 'Mauchu')"
          />
          <OverlayPanel ref="opColor">
            <ColorPicker theme="dark" @changeColor="changeColor" :sucker-hide="true" />
          </OverlayPanel>
          <label class="col-5 ml-3 text-right">Màu nền</label>
          <Button
            class="p-button-rounded p-button-outlined p-button-secondary col-4"
            :style="{
              backgroundColor: monhoc.Maunen,
              color: monhoc.Maunen ? 'transparent' : '#333',
              border: '1px solid #ccc',
            }"
            type="button"
            icon="pi pi-palette"
            @click="toggleColor($event, 'Maunen')"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Khối</label>
          <MultiSelect
            v-model="monhoc.Khoihocs"
            @selectall-change="checkAllkhoi($event)"
            :selectAll="selectAll"
            class="col-10"
            :popup="true"
            :options="tudiens[3]"
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
          <InputNumber class="col-4 ip36 p-0" v-model="monhoc.STT" />
          <label style="vertical-align: text-bottom" class="col-2 text-right"
            >Trạng thái</label
          >
          <InputSwitch v-model="monhoc.Trangthai" class="mt-1" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddMonhoc"
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
