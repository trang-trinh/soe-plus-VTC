<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import arrIcons from "../../assets/json/icons.json";
import router from "@/router";
//init Model
const tdtypedonvis = [
  { value: 0, text: "Đơn vị" },
  { value: 1, text: "Trường học" },
  { value: 2, text: "Phòng ban" },
];
const donvi = ref({
  organization_name: "",
  STT: 1,
  Trangthai: true,
  TypeDonvi: 0,
});
//Valid Form
const submitted = ref(false);
const rules = {
  organization_name: {
    required,
  },
};
const v$ = useVuelidate(rules, donvi);
//Khai báo biến
const store = inject("store");
const selectCapcha = ref();
const selectedKey = ref();
const selectedNodes = ref([]);
const filters = ref({});
const opition = ref({
  search: "",
  PageNo: 1,
  PageSize: 20,
  TypeDonvi: null,
  user_id: store.getters.user.user_id,
});
const donvis = ref();
const treedonvis = ref();
const treediadanhs = ref();
const selectDiadanh = ref();
const displayAddDonvi = ref(false);
const isFirst = ref(true);
let files = {};
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const menuButs = ref();
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportDonvi("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportDonvi("ExportExcelMau");
    },
  },
]);

//Khai báo function
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const onNodeSelect = (node) => {
  selectedNodes.value.push(node.data.Donvi_ID);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(selectedNodes.value.indexOf(node.data.Donvi_ID), 1);
};
const handleFileUpload = (event, ia) => {
  files[ia] = event.target.files[0];
  var output = document.getElementById(ia);
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
//Show Modal
const showModalAddDonvi = () => {
  submitted.value = false;
  selectCapcha.value = {};
  selectCapcha.value[store.getters.user.Donvi_ID] = true;
  donvi.value = {
    organization_name: "",
    STT: donvis.value.length + 1,
    Trangthai: true,
    TypeDonvi: 0,
    Donvi_ID: store.getters.user.Donvi_ID,
  };
  displayAddDonvi.value = true;
};
const chonanh = (id) => {
  document.getElementById(id).click();
};
const closedisplayAddDonvi = () => {
  displayAddDonvi.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  loadDonvi(true);
};
const onSearch = () => {
  loadDonvi(true);
};
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
  if (
    store.getters.user.Role_ID == "admin" ||
    store.getters.user.Role_ID == "administrator"
  )
    arrtreeChils.unshift({ key: -1, data: -1, label: "-----Chọn " + title + "----" });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const tudiens = ref({});
const loadTudien = (rf) => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Sys_Donvi_Tudiens",
        par: [{ par: "user_id", va: opition.value.user_id }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        tudiens.value = data;
        if (tudiens.value[2].length > 0) {
          let obj = renderTree(tudiens.value[2], "value", "text", "Địa danh");
          treediadanhs.value = obj.arrtreeChils;
        }
      } else {
        tudiens.value = [];
      }
    })
    .catch((error) => {});
};
const onPage = (event) => {
  opition.value.PageNo = event.page + 1;
  opition.value.PageSize = event.rows;
  loadDonvi(true);
};
const loadDonvi = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Sys_Donvi_List",
        par: [
          { par: "PageNo", va: opition.value.PageNo },
          { par: "PageSize", va: opition.value.PageSize },
          { par: "Search", va: opition.value.search },
          { par: "TypeDonvi", va: opition.value.TypeDonvi },
          { par: "user_id", va: opition.value.user_id },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        let obj = renderTree(data[0], "Donvi_ID", "organization_name", "đơn vị");
        donvis.value = obj.arrChils;
        treedonvis.value = obj.arrtreeChils;
        opition.value.totalRecords = data[1][0].totalRecords;
       
      } else {
        donvis.value = [];
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
const editDonvi = (md) => {
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddDonvi.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "Sys_Donvi_Get", par: [{ par: "Donvi_ID", va: md.Donvi_ID }] },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        donvi.value = data[0][0];
        selectCapcha.value = {};
        selectCapcha.value[donvi.value.Capcha_ID || "-1"] = true;
        selectDiadanh.value = {};
        selectDiadanh.value[donvi.value.Diadanh_ID || "-1"] = true;
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
const handleSubmit = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (selectCapcha.value != null) {
    let keys = Object.keys(selectCapcha.value);
    donvi.value.Capcha_ID = keys[0];
    if (donvi.value.Capcha_ID == -1) {
      donvi.value.Capcha_ID = null;
    }
  }
  if (selectDiadanh.value) {
    let keys = Object.keys(selectDiadanh.value);
    donvi.value.Diadanh_ID = keys[0];
    if (donvi.value.Diadanh_ID == -1) {
      donvi.value.Diadanh_ID = null;
    }
  }

  addDonvi();
};

const addTreeDonvi = (md) => {
  selectCapcha.value = {};
  selectCapcha.value[md.Donvi_ID] = true;
  donvi.value = {
    organization_name: "",
    STT: donvis.value.length + 1,
    Trangthai: true,
    TypeDonvi: 2,
  };
  submitted.value = false;
  displayAddDonvi.value = true;
};
const addDonvi = () => {
  let formData = new FormData();
  for (var k in files) {
    let file = files[k];
    formData.append(k, file);
  }
  formData.append("model", JSON.stringify(donvi.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: donvi.value.Donvi_ID ? "put" : "post",
    url: baseURL + `/api/Phongban/${donvi.value.Donvi_ID ? "Update_Donvi" : "Add_Donvi"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật đơn vị thành công!");
        loadDonvi();
        closedisplayAddDonvi();
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

const delDonvi = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá đơn vị này không!",
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
          .delete(baseURL + "/api/Phongban/Del_Donvi", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.Donvi_ID] : selectedNodes.value,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá đơn vị thành công!");
              loadDonvi();
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

const exportDonvi = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "Sys_Donvi" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH ĐƠN VỊ",
        proc: "Sys_Donvi_ListExport",
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
const filteredItems = ref([]);
const searchItems = (event) => {
  //in a real application, make a request to a remote url with the query and return filtered results, for demo we filter at client side
  let query = event.query;
  let filteItems = [];
  for (let i = 0; i < arrIcons.length; i++) {
    let item = arrIcons[i];
    if (item.toLowerCase().indexOf(query.toLowerCase()) != -1) {
      filteItems.push(item);
    }
  }
  filteredItems.value = filteItems;
};
const rowClass = (data) => {
  return data.TypeDonvi == 0
    ? "classdonvi"
    : data.TypeDonvi == 1
    ? "classtruonghoc"
    : "classphongban";
};
onMounted(() => {
  //init 
  console.log("sk1");
  loadDonvi(true);
  loadTudien();
});
</script>
<template>
  <div class="main-layout flex-grow-1 p-2" v-if="store.getters.islogin">
    <TreeTable
      :value="donvis"
      v-model:selectionKeys="selectedKey"
      :loading="opition.loading"
      @nodeSelect="onNodeSelect"
      @nodeUnselect="onNodeUnselect"
      :filters="filters"
      :showGridlines="true"
      selectionMode="checkbox"
      filterMode="lenient"
      class="p-treetable-sm e-sm"
      :paginator="opition.totalRecords > opition.PageSize"
      :rows="opition.PageSize"
      :lazy="true"
      @page="onPage($event)"
      :pageLinkSize="opition.PageSize"
      :scrollable="true"
      scrollHeight="flex"
    >
      <template #header>
        <h3 class="donvi-title module-title-hidden mt-0 ml-1 mb-2">
          <i class="pi pi-microsoft"></i> Danh sách Đơn vị/Phòng ban/ Trường học
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <Dropdown
              :showClear="true"
              v-model="opition.TypeDonvi"
              :options="tdtypedonvis"
              optionLabel="text"
              optionValue="value"
              placeholder="Lọc theo đơn vị"
              class="p-dropdown-sm"
              @change="loadDonvi(true)"
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
              @click="delDonvi"
            />
            <Button
              label="Export"
              icon="pi pi-file-excel"
              class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            />
            <Menu vị id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" />
            <Button
              label="Thêm đơn vị"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddDonvi"
            />
          </template>
        </Toolbar>
      </template>
      <Column
        field="Donvi_ID"
        :sortable="true"
        header="Mã"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      >
      </Column>
      <Column
        field="Logo"
        header="Logo"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:80px"
        bodyStyle="text-align:center;max-width:80px"
      >
        <template #body="md">
          <Avatar
            v-if="md.node.data.Logo"
            :image="basedomainURL + md.node.data.Logo"
            class="mr-2"
            size="large"
          />
        </template>
      </Column>
      <Column
        field="TypeDonvi"
        header="Loại"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px"
        bodyStyle="text-align:center;max-width:120px"
      >
        <template #body="md">
          <Chip :class="'chip' + md.node.data.TypeDonvi">{{
            md.node.data.TypeDonvi == 0
              ? "Đơn vị"
              : md.node.data.TypeDonvi == 1
              ? "Trường học"
              : "Phòng ban"
          }}</Chip>
        </template>
      </Column>
      <Column field="organization_name" header="Tên đơn vị" :sortable="true" :expander="true">
        <template #body="md">
          <span :class="'donvi' + md.node.data.TypeDonvi">{{
            md.node.data.organization_name
          }}</span>
        </template>
      </Column>
      <Column
        field="STT"
        :sortable="true"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      ></Column>
      <Column
        headerClass="text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #header> </template>
        <template #body="md">
          <Button
            type="button"
            icon="pi pi-plus-circle"
            class="p-button-rounded p-button-sm p-button-success"
            style="margin-right: 0.5rem"
            v-tooltip.top="'Thêm đơn vị'"
            @click="addTreeDonvi(md.node.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-pencil"
            class="p-button-rounded p-button-sm p-button-info"
            style="margin-right: 0.5rem"
            @click="editDonvi(md.node.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delDonvi(md.node.data)"
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
    </TreeTable>
  </div>
  <Dialog
    header="Cập nhật Đơn vị/ Phòng ban/ Trường học"
    v-model:visible="displayAddDonvi"
    :style="{ width: '960px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left"
            >Tên đơn vị <span class="redsao">(*)</span></label
          >
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="donvi.organization_name"
            :class="{ 'p-invalid': v$.organization_name.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.organization_name.$invalid && submitted) || v$.organization_name.$pending.$response"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.organization_name.required.$message
                .replace("Value", "Tên đơn vị")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Cấp cha</label>
          <TreeSelect
            class="col-10"
            v-model="selectCapcha"
            :options="treedonvis"
            :showClear="true"
            placeholder=""
            optionLabel="data.organization_name"
            optionValue="data.Donvi_ID"
          ></TreeSelect>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Loại</label>
          <Dropdown
            class="col-4"
            v-model="donvi.TypeDonvi"
            :options="tdtypedonvis"
            optionLabel="text"
            optionValue="value"
          />
          <label class="col-2 text-right">Tên viết tắt</label>
          <InputText spellcheck="false" class="col-4 ip36" v-model="donvi.TenvietTat" />
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2">Logo</label>
          <div class="col-4 p-0">
            <div class="inputanh" @click="chonanh('AnhDonvi')">
              <img
                id="LogoDonvi"
                v-bind:src="
                  donvi.Logo ? basedomainURL + donvi.Logo : '/src/assets/image/noimg.jpg'
                "
              />
            </div>
            <input
              class="ipnone"
              id="AnhDonvi"
              type="file"
              accept="image/*"
              @change="handleFileUpload($event, 'LogoDonvi')"
            />
          </div>
          <label class="col-2 text-right">Ảnh nền</label>
          <div class="col-4 p-0">
            <div class="inputanh" @click="chonanh('AnhNen')">
              <img
                id="LogoNen"
                v-bind:src="
                  donvi.Anhnen
                    ? basedomainURL + donvi.Anhnen
                    : '/src/assets/image/noimg.jpg'
                "
              />
            </div>
            <input
              class="ipnone"
              id="AnhNen"
              type="file"
              accept="image/*"
              @change="handleFileUpload($event, 'LogoNen')"
            />
          </div>
        </div>
        <Fieldset
          class="mb-4 w-full"
          legend="Thông tin trường"
          :toggleable="true"
          v-if="donvi.TypeDonvi == 1"
          style="background-color: #f5f5f5"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left">Loại trường</label>
            <Dropdown
              class="col-4"
              v-model="donvi.LoaiTruong_ID"
              :options="tudiens[0]"
              optionLabel="text"
              optionValue="value"
            />
            <label class="col-2 text-right">Mô hình</label>
            <Dropdown
              class="col-4"
              v-model="donvi.MohinhTruong_ID"
              :options="tudiens[1]"
              optionLabel="text"
              optionValue="value"
            />
          </div>
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left">Địa chỉ</label>
            <InputText spellcheck="false" class="col-4 ip36" v-model="donvi.Diachi" />
            <label class="col-2 text-right">Địa danh</label>
            <TreeSelect
              class="col-4"
              v-model="selectDiadanh"
              :options="treediadanhs"
              :showClear="true"
              placeholder=""
              optionLabel="data.text"
              optionValue="data.value"
            ></TreeSelect>
          </div>
        </Fieldset>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Url</label>
          <InputText spellcheck="false" class="col-10 ip36" v-model="donvi.IsUrl" />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-2 ip36 p-0" v-model="donvi.STT" />
          <label style="vertical-align: text-bottom" class="col-2 text-right"
            >Trạng thái</label
          >
          <InputSwitch v-model="donvi.Trangthai" class="mt-1" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddDonvi"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
    </template>
  </Dialog>
</template>
<style scoped>
.classdonvi {
  background-color: aliceblue;
}
span.donvitrue {
  font-weight: 500;
}
.chip0 {
  background-color: #4285f4;
  color: #fff;
  font-size: 0.875rem;
  padding: 5px 10px;
}
.chip1 {
  background-color: #689f38;
  color: #fff;
  font-size: 0.875rem;
  padding: 5px 10px;
}
.chip2 {
  background-color: #607d8b;
  color: #fff;
  font-size: 0.875rem;
  padding: 5px 10px;
}
.ipnone {
  display: none;
}
.inputanh {
  /* border: 1px solid #ccc; */
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
