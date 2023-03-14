<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { required, maxLength, minLength ,email } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import arrIcons from "../../../assets/json/icons.json";
import { useRouter, useRoute } from "vue-router";
// import router from "@/router";
import { encr } from "../../../util/function.js";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");

//init Model
const tdorganization_types = [
  { value: 0, text: "Đơn vị" },
  // { value: 1, text: "Trường học" },
  { value: 1, text: "Phòng ban" },
];
const donvi = ref({
  organization_name: "",
  is_order: 1,
  status: true,
  organization_type: 0,
});
//color
const opColor = ref();
let pcolor = "";
const toggleColor = (event, pc) => {
  opColor.value.toggle(event);
  pcolor = pc;
};
const changeColor = (color) => {
  donvi.value[pcolor] = color.hex;
  if (!color.hex.includes("#")) opColor.value.hide();
};
//Valid Form
const submitted = ref(false);
const rules = {
  organization_name: {
    required,
    maxLength: maxLength(500),
  },
  mail :{
    email ,
  },
};
const v$ = useVuelidate(rules, donvi);
//Khai báo biến
const displayPhongban = ref(false);
const isDisplayAvt = ref(false);
const isDisplayNen = ref(false);
const store = inject("store");
const selectCapcha = ref();
const selectedKey = ref();
const selectedNodes = ref([]);
const filters = ref({});
const treelocals = ref();
const opition = ref({
  search: "",
  PageNo: 1,
  PageSize: 1000,
  organization_type: null,
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
  selectedNodes.value.push(node.data.organization_id);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(
    selectedNodes.value.indexOf(node.data.organization_id),
    1
  );
};
const handleFileUpload = (event, ia) => {
  if (ia == "LogoDonvi") isDisplayAvt.value = true;
  else if (ia == "LogoNen") isDisplayNen.value = true;
  files[ia] = event.target.files[0];
  var output = document.getElementById(ia);
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
// on event
const onChangeParent = (item) => {
  const organization_id = parseInt(Object.keys(item)[0]);
  axios
    .post(
        baseURL + "/api/Phongban/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "sys_organization_get_order",
        par: [{ par: "organization_id", va: organization_id }],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        donvi.value.is_order = data[0][0].c + 1;
      }
    });
};
//Show Modal
const showModalAddDonvi = () => {
  files = [];
  submitted.value = false;
  selectCapcha.value = {};
  // selectCapcha.value[store.getters.user.parent_id] = true;
  donvi.value = {
    organization_name: "",
    is_order: donvis.value.length + 1,
    status: true,
    organization_type: 0,
    // parent_id: null,
    // parent_id: store.getters.user.organization_id,
  };
  displayAddDonvi.value = true;
};
const chonanh = (id) => {
  document.getElementById(id).click();
};
const closedisplayAddDonvi = () => {
  displayAddDonvi.value = false;
};
//xóa ảnh
const delLogo = () => {
  files["LogoDonvi"] = [];
  isDisplayAvt.value = false;
  var output = document.getElementById("LogoDonvi");
  output.src = basedomainURL + "/Portals/Image/noimg.jpg";
  donvi.value.logo = null;
};
const delNen = () => {
  files["LogoNen"] = [];
  isDisplayNen.value = false;
  var output = document.getElementById("LogoNen");
  output.src = basedomainURL + "/Portals/Image/noimg.jpg";
  donvi.value.background_image = null;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  selectedKey.value ={};
  selectedNodes.value = [];
  loadDonvi(true);
};
const onSearch = () => {
  loadDonvi(true);
};
const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      m.label_order = m.IsOrder.toString();
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em, index) => {
            em.label_order = mm.data.label_order + "." + (index +1);
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
        let dts = data.filter((x) => x.parent_id == pid);
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
  arrtreeChils.unshift({
    key: -1,
    data: -1,
    label: "-----Chọn " + title + "----",
  });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
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
        baseURL + "/api/Phongban/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "sys_organization_list_main",
        par: [
          { par: "pageno", va: opition.value.PageNo },
          { par: "pagesize", va: opition.value.PageSize },
          { par: "search", va: opition.value.search },
          { par: "organization_type", va: opition.value.organization_type },
          { par: "user_id", va: opition.value.user_id },
        ],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        let obj = renderTree(
          data[0],
          "organization_id",
          "organization_name",
          "đơn vị"
        );
        donvis.value = obj.arrChils;
        treedonvis.value = obj.arrtreeChils;
        opition.value.totalRecords = data[1][0].totalrecords;
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
        baseURL + "/api/Phongban/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "sys_organization_get",
        par: [{ par: "organization_id", va: md.organization_id }],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        donvi.value = data[0][0];
        selectCapcha.value = {};
        selectCapcha.value[donvi.value.parent_id || "-1"] = true;
        selectDiadanh.value = {};
        selectDiadanh.value[donvi.value.place_id || "-1"] = true;
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
  if (donvi.value.organization_name.length > 500) {
    swal.fire({
      title: "Thông báo!",
      text:
        "Vui lòng không được nhập tên " +
        (donvi.value.organization_type == 1 ? "phòng ban" : "đơn vị") +
        " quá 500 ký tự!",
      icon: "error",
      confirmButtonText: "OK",
    });
  }
  if (selectCapcha.value != null) {
    let keys = Object.keys(selectCapcha.value);
    donvi.value.parent_id = keys[0];
    if (donvi.value.parent_id == -1) {
      donvi.value.parent_id = null;
    }
  }
  if (selectDiadanh.value) {
    let keys = Object.keys(selectDiadanh.value);
    donvi.value.place_id = keys[0];
    if (donvi.value.place_id == -1) {
      donvi.value.place_id = null;
    }
  }

  addDonvi();
};

const addTreeDonvi = (md) => {
  let is_order = donvis.value.length + 1;
  if (md.children) {
    is_order = md.children.length + 1;
  } else {
    is_order = 1;
  }
  selectCapcha.value = {};
  selectCapcha.value[md.data.organization_id] = true;
  donvi.value = {
    organization_name: "",
    is_order: is_order,
    status: true,
    organization_type: 1,
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
    method: donvi.value.organization_id ? "put" : "post",
    url:
      baseURL +
      `/api/Phongban/${
        donvi.value.organization_id ? "Update_Donvi" : "Add_Donvi"
      }`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        donvi.value.organization_type == 0
          ? toast.success("Cập nhật đơn vị thành công!")
          : toast.success("Cập nhật phòng ban thành công!");
        loadDonvi();
        closedisplayAddDonvi();
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
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
// xóa đơn vị
const delDonvi = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text:
        "Bạn có muốn xoá " +
        (md.organization_type == 1 ? "phòng ban" : "đơn vị") +
        " này không!",
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
            data: md != null ? [md.organization_id] : selectedNodes.value,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              md.organization_type == 1
                ? toast.success("Xoá phòng ban thành công!")
                : toast.success("Xoá đơn vị thành công!");
              loadDonvi();
              if (!md) selectedNodes.value = [];
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
// xóa nhiều
const deleteList = () => {
  let listId = new Array(selectedNodes.value.length);

  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xóa danh sách đơn vị này không!",
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
        selectedNodes.value.forEach((item) => {
          listId.push(item);
        });
        axios
          .delete(baseURL + "/api/Phongban/Del_Donvi", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId != null ? listId : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá thành công!");
              selectedNodes.value = [];
              loadDonvi(true);
            } else {
              swal.fire({
                title: "Thông báo!",
                text: "Xóa không thành công, vui lòng thử lại!",
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
  return data.organization_type == 0
    ? "classdonvi"
    : data.organization_type == 1
    ? "classtruonghoc"
    : "classphongban";
};
const rowClassStatus = (data) => {
  return data.status ? "" : "error";
};
const showPhongban = ()=>{
    emitter.emit("emitData", {
    type: "change_type",
    data: {
        isViewList: false,
        isViewTree: true,
    },
  });
//   router.push({ name: "organization_new", params:  {} })
//         .then(() => {
//           router.go(0);
//         });
}
onMounted(() => {
  //init
  loadDonvi(true);
  //loadTudien();
});
</script>
<template>
  <div class="main-layout flex-grow-1 p-2">
    <TreeTable
      :value="donvis"
      v-model:selectionKeys="selectedKey"
      :loading="opition.loading"
      @nodeSelect="onNodeSelect"
      @nodeUnselect="onNodeUnselect"
      :filters="filters"
      :showGridlines="true"
      selectionMode="checkbox"
      filterMode="strict"
      class="p-treetable-sm"
      :rows="20"
      :rowHover="true"
      responsiveLayout="scroll"
      :scrollable="true"
      scrollHeight="flex"
    >
      <template #header>
        <h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
          <i class="pi pi-microsoft"></i> Danh sách
          {{ store.getters.user.is_super ? "đơn vị" : "phòng ban" }} ({{
            opition.totalRecords
          }})
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                type="text"
                spellcheck="false"
                v-model="filters['global']"
                placeholder="Tìm kiếm theo tên đơn vị"
              />
            </span>
          </template>

          <template #end>
            <Button
              v-if="store.getters.user.is_super"
              label="Thêm đơn vị"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddDonvi"
            />
                        <Button
              icon="pi pi-list"
              v-tooltip.left="'Hiển thị phòng ban'"
              class="mr-2"
              v-bind:class="
                'p-button p-button-' +
                (displayPhongban ? 'primary' : 'secondary')
              "
              @click="showPhongban"
            />
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
              @click="deleteList()"
            />
            <!-- <Button
              label="Export"
              icon="pi pi-file-excel"
              class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            />
            <Menu
              id="overlay_Export"
              ref="menuButs"
              :model="itemButs"
              :popup="true"
            /> -->
          </template>
        </Toolbar>
      </template>
      <Column
        field="is_order"
        header="STT"
        class="align-items-center justify-content-center text-center font-bold"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      >
        <template #body="md">
          <div v-bind:class="md.node.data.status ? '' : 'text-error'">
            {{ md.node.data.label_order }}
          </div>
        </template>
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
            v-if="md.node.data.logo"
            :image="basedomainURL + md.node.data.logo"
            class="mr-2"
            size="large"
          />
        </template>
      </Column>
            <Column
        field="organization_id"
        header="Mã đơn vị"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:130px"
        bodyStyle="text-align:center;max-width:130px"
      >
        <template #body="md">
           <span
            :class="'donvi' + md.node.data.organization_type"
            :style="[
              md.node.data.parent_id ? '' : 'font-weight:bold',
              md.node.data.status ? '' : 'color:red !important',
            ]"
            >{{ md.node.data.organization_id }}</span
          >
        </template>
      </Column>
      <Column
        field="organization_name"
        header="Tên đơn vị"
        :expander="true"
      >
        <template #body="md">
          <span
            :class="'donvi' + md.node.data.organization_type"
            :style="[
              md.node.data.parent_id ? '' : 'font-weight:bold',
              md.node.data.status ? '' : 'color:red !important',
            ]"
            >{{ md.node.data.organization_name }}</span
          >
        </template>
      </Column>
      <Column
        field="organization_type"
        header="Loại"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px"
        bodyStyle="text-align:center;max-width:120px"
      >
        <template #body="md">
          <Chip
            :class="'chip' + md.node.data.organization_type"
            :style="
              md.node.data.status ? '' : 'background-color:red !important;'
            "
            >{{
              md.node.data.organization_type == 0 ? "Đơn vị" : "Phòng ban"
            }}</Chip
          >
        </template>
      </Column>
      <!-- <Column
        field="organization_id"
        :sortable="true"
        :class="status ? '' : 'error'"
        header="Mã"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      >
        <template #body="md">
          <div v-bind:class="md.node.data.status ? '' : 'error'">
            {{ md.node.data.organization_id }}
          </div>
        </template>
      </Column> -->
      <Column
        header="Chức năng"
        headerClass="text-center"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #header> </template>
        <template #body="md">
          <Button
            type="button"
            icon="pi pi-plus-circle"
            class="p-button-rounded p-button-secondary p-button-outlined"
            style="margin-right: 0.5rem"
            v-tooltip.top="'Thêm phòng ban'"
            @click="addTreeDonvi(md.node)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-pencil"
            v-tooltip.top="'Chỉnh sửa'"
            class="p-button-rounded p-button-secondary p-button-outlined"
            style="margin-right: 0.5rem"
            @click="editDonvi(md.node.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            v-tooltip.top="'Xóa'"
            class="p-button-rounded p-button-secondary p-button-outlined"
            @click="delDonvi(md.node.data)"
          ></Button>
        </template>
      </Column>
      <template #empty>
        <div
          class="
            m-auto
            align-items-center
            justify-content-center
            p-4
            text-center
          "
          v-if="!isFirst"
        >
          <img src="../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </TreeTable>
  </div>
  <Dialog
    header="Cập nhật Đơn vị/ Phòng ban"
    v-model:visible="displayAddDonvi"
    :style="{width: '860px',zIndex: 2}"
    :maximizable="true"
    :autoZIndex="false"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left"
            >Tên{{ donvi.organization_type == 1 ? " phòng ban " : " đơn vị "
            }}<span class="redsao">(*)</span></label
          >
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="donvi.organization_name"
            :class="{ 'p-invalid': v$.organization_name.$invalid && submitted }"
          />
        </div>
        <small
          v-if="
            (v$.organization_name.required.$invalid && submitted) ||
            v$.organization_name.required.$pending.$response
          "
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3" v-if="donvi.organization_type == 0">{{
              v$.organization_name.required.$message
                .replace("Value", "Tên đơn vị")
                .replace("is required", "không được để trống")
            }}</span>
             <span class="col-10 pl-3"  v-if="donvi.organization_type == 1">{{
              v$.organization_name.required.$message
                .replace("Value", "Tên phòng ban")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
         <small
          v-if="
            (v$.organization_name.maxLength.$invalid && submitted) 
          "
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3"  v-if="donvi.organization_type == 0"
              >{{
                v$.organization_name.maxLength.$message.replace(
                  "The maximum length allowed is",
                  "Tên đơn vị không được vượt quá"
                )
              }}
              ký tự</span
            >
            <span class="col-10 pl-3"  v-if="donvi.organization_type == 1"
              >{{
                v$.organization_name.maxLength.$message.replace(
                  "The maximum length allowed is",
                  "Tên phòng ban không được vượt quá"
                )
              }}
              ký tự</span
            >
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Cấp cha</label>
          <TreeSelect
            @change="onChangeParent"
            class="col-10"
            v-model="selectCapcha"
            :options="treedonvis"
            :showClear="true"
            placeholder=""
            optionLabel="data.organization_name"
            optionValue="data.organization_id"
          >
          </TreeSelect>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Loại</label>
          <Dropdown
            class="col-10"
            v-model="donvi.organization_type"
            :options="tdorganization_types"
            optionLabel="text"
            optionValue="value"
          />
        </div>
        <div class="field col-12 md:col-12" v-if="donvi.organization_type == 0">
          <label class="col-2 text-left">Tên phần mềm</label>
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="donvi.product_name"
          />
        </div>
        <div class="field col-12 md:col-12" v-if="donvi.organization_type == 1">
          <label class="col-2 text-left">Ký hiệu văn bản</label>
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="donvi.department_doc_code"
          />
        </div>
        <div class="field col-12 md:col-12" v-if="donvi.organization_type ==0" >
          <label class="col-2 text-left">SĐT</label>
          <InputText
            spellcheck="false"
            class="col-4 ip36"
            v-model="donvi.phone"
          />
          <label class="col-2 text-right">Email</label>
          <InputText
            spellcheck="false"
            class="col-4 ip36"
            v-model="donvi.mail"
          />
        </div>
          <small
          v-if="
            (v$.mail.email.$invalid && submitted && donvi.mail != null)"
          class="p-error field col-12 md:col-12 mb-3 flex"
        >
        <div class="col-6"></div>
            <label class="col-2 text-left"></label>
            <span class="">{{
              v$.mail.email.$message
                .replace("Value is not a valid email address", "Email không hợp lệ")
            }}</span>
         </small
        >
        <div class="field col-12 md:col-12" v-if="donvi.organization_type ==0">
          <label class="col-2 text-left">Website</label>
          <InputText
            spellcheck="false"
            class="col-4 ip36"
            v-model="donvi.is_url"
          />
          <label class="col-2 text-right">Fax</label>
          <InputText
            spellcheck="false"
            class="col-4 ip36"
            v-model="donvi.fax"
          />
        </div>
        <div class="field col-12 md:col-12" v-if="donvi.organization_type==0">
          <label class="col-2 text-left">Địa chỉ</label>
          <InputText
            spellcheck="false"
            class="col-4 ip36"
            v-model="donvi.address"
          />
          <label class="col-2 text-right">Tên viết tắt</label>
          <InputText
            spellcheck="false"
            class="col-4 ip36"
            v-model="donvi.short_name"
          />
        </div>
        <div class="field col-12 md:col-12 flex" v-if="donvi.organization_type==0">
          <label class="col-2">Logo</label>
          <div class="col-4 p-0">
            <div class="inputanh relative">
              <img
                @click="chonanh('AnhDonvi')"
                id="LogoDonvi"
                v-bind:src="
                  donvi.logo
                    ? basedomainURL + donvi.logo
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                "
              />
              <Button
                v-if="isDisplayAvt || donvi.logo"
                style="width: 1.5rem; height: 1.5rem"
                icon="pi pi-times"
                @click="delLogo"
                class="p-button-rounded absolute top-0 right-0 cursor-pointer"
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
            <div class="inputanh relative">
              <img
                @click="chonanh('AnhNen')"
                id="LogoNen"
                v-bind:src="
                  donvi.background_image
                    ? basedomainURL + donvi.background_image
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                "
              />
              <Button
                v-if="isDisplayNen || donvi.background_image"
                style="width: 1.5rem; height: 1.5rem"
                icon="pi pi-times"
                @click="delNen"
                class="p-button-rounded absolute top-0 right-0 cursor-pointer"
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
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Màu nền</label>
          <Button
            class="p-button-rounded p-button-outlined p-button-secondary col-2"
            :style="{
              backgroundColor: donvi.background_color,
              color: donvi.background_color ? 'transparent' : '#333',
              border: '1px solid #ccc',
            }"
            type="button"
            icon="pi pi-palette"
            @click="toggleColor($event, 'background_color')"
          />
          <OverlayPanel ref="opColor">
            <ColorPicker
              theme="dark"
              @changeColor="changeColor"
              :sucker-hide="true"
            />
          </OverlayPanel>
          <label class="col-3 text-center">Màu chữ</label>
          <Button
            class="p-button-rounded p-button-outlined p-button-secondary col-2"
            :style="{
              backgroundColor: donvi.text_color,
              color: donvi.text_color ? 'transparent' : '#333',
              border: '1px solid #ccc',
            }"
            type="button"
            icon="pi pi-palette"
            @click="toggleColor($event, 'text_color')"
          />
          <OverlayPanel ref="opColor">
            <ColorPicker
              theme="dark"
              @changeColor="changeColor"
              :sucker-hide="true"
            />
          </OverlayPanel>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-2 ip36 p-0" v-model="donvi.is_order" />
          <label class="col-2 text-right">Trạng thái</label>
          <InputSwitch v-model="donvi.status" />
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
      <Button
        label="Cập nhật"
        icon="pi pi-save"
        @click="handleSubmit(!v$.$invalid)"
      />
    </template>
  </Dialog>
</template>
<style>
.p-treeselect-panel .p-treeselect-items-wrapper .p-tree {
  max-width: 660px !important;
}
</style>
<style scoped>
.text-error {
  color: red !important;
}
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
.donvi0{
  font-weight: bold !important;
}
</style>
<style lang="scss" scoped>
::v-deep(.col-12) {
  .p-inputswitch {
    top: 6px;
  }
}
</style>
