<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import arrIcons from "../../assets/json/icons.json";
import router from "@/router";
//init Model
const tdtypediadanhs = [
  { value: 0, text: "Tỉnh/TP" },
  { value: 1, text: "Quận/Huyện" },
  { value: 2, text: "Xã/Phường" },
];
const diadanh = ref({
  Ten: "",
  STT: 1,
  Trangthai: true,
  Cap: 0,
});
//Valid Form
const submitted = ref(false);
const rules = {
  Ten: {
    required,
  },
};
const v$ = useVuelidate(rules, diadanh);
//Khai báo biến
const store = inject("store");
const selectCapcha = ref();
const selectedKey = ref();
const selectedNodes = ref([]);
const filters = ref({});
const opition = ref({ search: "", PageNo: 1, PageSize: 20, Cap: -1 });
const diadanhs = ref();
const treediadanhs = ref();
const displayAddDiadanh = ref(false);
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
      exportDiadanh("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportDiadanh("ExportExcelMau");
    },
  },
]);

//Khai báo function
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const onNodeSelect = (node) => {
  selectedNodes.value.push(node.data.Diadanh_ID);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(selectedNodes.value.indexOf(node.data.Diadanh_ID), 1);
};
const handleFileUpload = (event, ia) => {
  files[ia] = event.target.files;
  var output = document.getElementById(ia);
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
//Show Modal
const showModalAddDiadanh = () => {
  submitted.value = false;
  selectCapcha.value = {};
  diadanh.value = {
    Ten: "",
    STT: diadanhs.value.length + 1,
    Trangthai: true,
    Cap: 0,
  };
  displayAddDiadanh.value = true;
};
const chonanh = (id) => {
  document.getElementById(id).click();
};
const closedisplayAddDiadanh = () => {
  displayAddDiadanh.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  loadDiadanh(true);
};
const onSearch = () => {
  loadDiadanh(true);
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
  arrtreeChils.unshift({ key: -1, data: -1, label: "-----Chọn " + title + "----" });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const loadDiadanh = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "DM_Diadanh_List",
        par: [
          { par: "PageNo", va: opition.value.PageNo },
          { par: "PageSize", va: opition.value.PageSize },
          { par: "Search", va: opition.value.search },
          { par: "Cap", va: opition.value.Cap },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        let obj = renderTree(data, "Diadanh_ID", "Ten", "địa danh");
        diadanhs.value = obj.arrChils;
        treediadanhs.value = obj.arrtreeChils;
      } else {
        diadanhs.value = [];
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
const editDiadanh = (md) => {
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddDiadanh.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "DM_Diadanh_Get", par: [{ par: "Diadanh_ID", va: md.Diadanh_ID }] },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        diadanh.value = data[0][0];
        selectCapcha.value = {};
        selectCapcha.value[(diadanh.value.Capcha_ID !=null?diadanh.value.Capcha_ID:"-1")] = true;
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
  let keys = Object.keys(selectCapcha.value);
  diadanh.value.Capcha_ID = keys[0];
  if (diadanh.value.Capcha_ID == -1) {
    diadanh.value.Capcha_ID = null;
  }
  addDiadanh();
};

const addTreeDiadanh = (md) => {
  let STT=diadanhs.value.length+1;
  if(md.children){
    STT=md.children[md.children.length-1].data.STT+1;
  }else if(md.Cap!=0){
    STT=1;
  }
  selectCapcha.value = {};
  selectCapcha.value[md.data.Diadanh_ID] = true;
  diadanh.value = {
    Ten: "",
    STT: STT,
    Trangthai: true,
    Cap: md.data.Cap+1,
  };
  submitted.value = false;
  displayAddDiadanh.value = true;
};
const addDiadanh = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: diadanh.value.Diadanh_ID ? "put" : "post",
    url:
      baseURL + `/api/DiaDanh/${diadanh.value.Diadanh_ID ? "Update_Diadanh" : "Add_Diadanh"}`,
    data: diadanh.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật địa danh thành công!");
        loadDiadanh();
        closedisplayAddDiadanh();
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

const delDiadanh = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá địa danh này không!",
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
          .delete(baseURL + "/api/Phongban/Del_Diadanh", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.Diadanh_ID] : selectedNodes.value,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá địa danh thành công!");
              loadDiadanh();
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

const exportDiadanh = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "DM_Diadanh" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH ĐỊA DANH",
        proc: "DM_Diadanh_ListExport",
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
  return data.Cap == 0
    ? "classtinh"
    : data.Cap == 1
    ? "classhuyen"
    : "classxa";
};
onMounted(() => {
  //init
  loadDiadanh(true);
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2" v-if="store.getters.islogin">
    <TreeTable
      :value="diadanhs"
      v-model:selectionKeys="selectedKey"
      :loading="opition.loading"
      @nodeSelect="onNodeSelect"
      @nodeUnselect="onNodeUnselect"
      :filters="filters"
      :showGridlines="true"
      selectionMode="checkbox"
      filterMode="lenient"
      class="p-treetable-sm e-sm"
      :paginator="diadanhs && diadanhs.length > 20"
      :rows="20"
      :scrollable="true"
      scrollHeight="flex"
    >
      <template #header>
        <h3 class="diadanh-title mt-0 ml-1 mb-2">
          <i class="pi pi-microsoft"></i> Địa danh
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
              @click="delDiadanh()"
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
              label="Thêm địa danh"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddDiadanh"
            />
          </template>
        </Toolbar>
      </template>
      <Column
        field="Diadanh_ID"
        :sortable="true"
        header="Mã"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px"
        bodyStyle="text-align:center;max-width:120px"
      >
      </Column>
      <Column
        field="Cap"
        header="Loại"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px"
        bodyStyle="text-align:center;max-width:120px"
      >
        <template #body="md">
          <Chip :class="'chip' + md.node.data.Cap">{{
            md.node.data.Cap == 0
              ? "Tỉnh/TP"
              : md.node.data.Cap == 1
              ? "Quận/Huyện"
              : "Xã/Phường"
          }}</Chip>
        </template>
      </Column>
      <Column field="Ten" header="Tên địa danh" :sortable="true" :expander="true">
        <template #body="md">
          <span :class="'diadanh' + md.node.data.Cap">{{ md.node.data.Ten }}</span>
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
            v-tooltip.top="'Thêm địa danh'"
            @click="addTreeDiadanh(md.node)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-pencil"
            class="p-button-rounded p-button-sm p-button-info"
            style="margin-right: 0.5rem"
            @click="editDiadanh(md.node.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delDiadanh(md.node.data)"
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
    header="Cập nhật Địa danh"
    v-model:visible="displayAddDiadanh"
    :style="{ width: '640px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left"
            >Tên <span class="redsao">(*)</span></label
          >
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="diadanh.Ten"
            :class="{ 'p-invalid': v$.Ten.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.Ten.$invalid && submitted) || v$.Ten.$pending.$response"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.Ten.required.$message
                .replace("Value", "Tên địa danh")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Cấp cha</label>
          <TreeSelect
            class="col-10"
            v-model="selectCapcha"
            :options="treediadanhs"
            :showClear="true"
            placeholder=""
            optionLabel="data.Ten"
            optionValue="data.Diadanh_ID"
          ></TreeSelect>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Loại</label>
          <Dropdown
            class="col-10"
            v-model="diadanh.Cap"
            :options="tdtypediadanhs"
            optionLabel="text"
            optionValue="value"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-2 ip36 p-0" v-model="diadanh.STT" />
          <label style="vertical-align: text-bottom" class="col-2 text-right"
            >Trạng thái</label
          >
          <InputSwitch v-model="diadanh.Trangthai" class="mt-1" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddDiadanh"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
    </template>
  </Dialog>
</template>
<style scoped>
.classtinh {
  background-color: aliceblue;
}
span.diadanhtrue {
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
</style>
