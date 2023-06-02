<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import CompLopMonView from "./Comp/CompLopMonView.vue";
import CompLopGVView from "./Comp/CompLopGVView.vue";
import CompLopHSView from "./Comp/CompLopHSView.vue";
//init Model
const lophoc = ref({
  Tenlop: "",
  STT: 1,
  Trangthai: true,
});
//Valid Form
const submitted = ref(false);
const rules = {
  Tenlop: {
    required,
  },
};
const v$ = useVuelidate(rules, lophoc);
//Khai báo biến
const isAdd = ref(true);
const selectedNodes = ref([]);
const filters = ref({});
const lophocs = ref();
const displayAddLophoc = ref(false);
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
      exportLophoc("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: () => {
      exportLophoc("ExportExcelMau");
    },
  },
]);
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
//More
const buttonFullGV = [
  {
    label: "Thông tin lớp",
    icon: "pi pi-info-circle",
    command: () => {
      displayConfigMonhoc.value = true;
    },
  },
  {
    label: "Môn học",
    icon: "pi pi-book",
    command: () => {
      displayConfigMonhoc.value = true;
    },
  },
  {
    label: "Giáo viên",
    icon: "pi pi-users",
    command: () => {
      displayConfigGV.value = true;
    },
  },
  {
    label: "Học sinh",
    icon: "pi pi-user-plus",
    command: () => {
      displayConfigHS.value = true;
    },
  },
  {
    label: "Thời khoá biểu",
    icon: "pi pi-calendar-minus",
    command: () => {
      displayConfigMonhoc.value = true;
    },
  },
  {
    label: "Học tập",
    icon: "pi pi-folder-open",
    command: () => {
      displayConfigMonhoc.value = true;
    },
  },
];
const menuButLops = ref();
const itemButLops = ref(buttonFullGV);
const toggleLop = (event, lh) => {
  if (lh.IsPermision) {
    itemButLops.value = buttonFullGV;
  } else {
    itemButLops.value = [
      {
        label: "Thông tin lớp",
        icon: "pi pi-info-circle",
        command: () => {
          displayConfigMonhoc.value = true;
        },
      },
      {
        label: "Học sinh",
        icon: "pi pi-user-plus",
        command: () => {
          displayConfigHS.value = true;
        },
      },
      {
        label: "Thời khoá biểu",
        icon: "pi pi-calendar-minus",
        command: () => {
          displayConfigMonhoc.value = true;
        },
      },
      {
        label: "Học tập",
        icon: "pi pi-folder-open",
        command: () => {
          displayConfigMonhoc.value = true;
        },
      },
    ];
  }
  lophoc.value = lh;
  menuButLops.value.toggle(event);
};
const showConfigmon = (lh) => {
  lophoc.value = lh;
  displayConfigMonhoc.value = true;
};
const showConfiggv = (lh) => {
  lophoc.value = lh;
  displayConfigGV.value = true;
};
const showConfighs = (lh) => {
  lophoc.value = lh;
  displayConfigHS.value = true;
};
//Khai báo function

const handleFileUpload = (event) => {
  files = event.target.files;
};
//Show Modal
const showModalAddLophoc = () => {
  selectAll.value = false;
  isAdd.value = true;
  submitted.value = false;
  lophoc.value = {
    Tenlop: "",
    STT: lophocs.value.length + 1,
    Trangthai: true,
    Loailop_ID: 1,
    Nguoiquanly_ID: store.getters.user.user_id,
    Truonghoc_ID: store.getters.user.IsSupper ? null : store.getters.user.Donvi_ID,
  };
  Nguoiquanly_ID.value = store.getters.user;
  displayAddLophoc.value = true;
};
const closedisplayAddLophoc = () => {
  displayAddLophoc.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.PageNo = 1;
  opition.value.search = "";
  loadLophoc(true);
};
const onSearch = () => {
  loadLophoc(true);
};
const onPage = (event) => {
  opition.value.PageNo = event.page + 1;
  opition.value.PageSize = event.rows;
  loadLophoc(true);
};
const loadLophoc = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "Sys_Lophoc_List",
        par: [
          { par: "PageNo", va: opition.value.PageNo },
          { par: "PageSizes", va: opition.value.PageSize },
          { par: "Search ", va: opition.value.search },
          { par: "Loailop_ID ", va: opition.value.Loailop_ID },
          { par: "Khoihoc_ID", va: opition.value.Khoihoc_ID },
          { par: "Truonghoc_ID", va: opition.value.Truonghoc_ID },
          { par: "Filteruser_id", va: opition.value.Filteruser_id },
          { par: "user_id", va: opition.value.user_id },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        data[0].forEach((m, i) => {
          m.STT = opition.value.PageSize * (opition.value.PageNo - 1) + (i + 1);
          m.IsPermision =
            m.IsPermision ||
            store.getters.user.IsSupper ||
            m.Nguoiquanly_ID == store.getters.user.user_id ||
            m.Nguoicapnhat == store.getters.user.user_id;
        });
        lophocs.value = data[0];
        opition.value.totalRecords = data[1][0].totalRecords;
      } else {
        lophocs.value = [];
      }
      if (isFirst.value) isFirst.value = false;
      if (rf) {
        opition.value.loading = false;
      }
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const Nguoiquanly_ID = ref({});
const editLophoc = (md) => {
  isAdd.value = false;
  submitted.value = false;
  selectAll.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddLophoc.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "Sys_Lophoc_Get", par: [{ par: "Lophoc_ID", va: md.Lophoc_ID }] },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        let obj = data[0][0];
        if (obj.Tukhoa) obj.Tukhoa = obj.Tukhoa.split(",");
        lophoc.value = obj;
        Nguoiquanly_ID.value = {
          user_id: obj.user_id,
          full_name: obj.full_name,
          Avartar: obj.Avartar,
        };
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
  addLophoc();
};
const addLophoc = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let md = { ...lophoc.value };
  if (md.Tukhoa instanceof Array) {
    md.Tukhoa = md.Tukhoa.join(",");
  }
  if (Nguoiquanly_ID.value) {
    md.Nguoiquanly_ID = Nguoiquanly_ID.value.user_id;
  }
  axios({
    method: isAdd.value == false ? "put" : "post",
    url: baseURL + `/api/Lophoc/${isAdd.value == false ? "Update_Lophoc" : "Add_Lophoc"}`,
    data: md,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật lớp học thành công!");
        loadLophoc();
        closedisplayAddLophoc();
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

const delLophoc = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá lớp học này không!",
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
          .delete(baseURL + "/api/Lophoc/Del_Lophoc", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data:
              md != null ? [md.Lophoc_ID] : selectedNodes.value.map((x) => x.Lophoc_ID),
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá lớp học thành công!");
              loadLophoc();
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
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

const upTrangthaiLophoc = (md) => {
  let ids = [md.Lophoc_ID];
  let tts = [md.Trangthai];
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/Lophoc/Update_TrangthaiLophoc",
    data: { ids: ids, tts: tts },
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái lớp học thành công!");
        loadLophoc();
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};

const exportLophoc = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "Sys_Lophoc" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH LỚP HỌC",
        proc: "Sys_Lophoc_ListExport",
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
        proc: "Sys_Lophoc_Tudiens",
        par: [{ par: "user_id", va: opition.value.user_id }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        data[0].forEach((ch) => {
          ch.items = data[1].filter((x) => x.Caphoc_ID == ch.value);
          ch.chon = false;
        });
        data[0] = data[0].filter((x) => x.items.length > 0);
        tudiens.value = data;
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
    lophoc.value.Khoihocs = [];
    tudiens.value[3].forEach((el) => {
      el.items.forEach((kk) => {
        lophoc.value.Khoihocs.push(kk.value);
      });
    });
  } else {
    lophoc.value.Khoihocs = [];
  }
};
const checkkhoi = (khoi) => {
  var arr = lophoc.value.Khoihocs || [];
  if (khoi.chon) {
    khoi.items.forEach((kk) => {
      arr.push(kk.value);
    });
  } else {
    arr = arr.filter((x) => khoi.items.findIndex((kk) => kk.value == x).length == -1);
  }
  lophoc.value.Khoihocs = arr;
};
//Componnent
const displayConfigMonhoc = ref(false);
const displayConfigGV = ref(false);
const displayConfigHS = ref(false);
const reloadComponnent = (data) => {
  let lh = lophocs.value.find((x) => x.Lophoc_ID == data.Lophoc_ID);
  for (var k in data) {
    if (k != "Lophoc_ID") {
      lh[k] = data[k];
    }
  }
};
const emitter = inject("emitter");
emitter.on("namhoc", (obj) => {
  loadLophoc(true);
});
onMounted(() => {
  //init
  loadLophoc(true);
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
      :value="lophocs"
      :paginator="opition.totalRecords > opition.PageSize"
      :loading="opition.loading"
      :totalRecords="opition.totalRecords"
      :rows="opition.PageSize"
      dataKey="Lophoc_ID"
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
      groupRowsBy="Tenloai"
    >
      <template #groupheader="slotProps">
        <i class="pi pi-list mr-2"></i> {{ slotProps.data.Tenloai }}
      </template>
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-list"></i> Lớp học
          <span v-if="lophocs">({{ opition.totalRecords }})</span>
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <Dropdown
              class="mr-2"
              v-model="opition.Khoihoc_ID"
              :options="tudiens[0]"
              optionLabel="text"
              optionValue="value"
              :filter="true"
              optionGroupLabel="text"
              optionGroupChildren="items"
              placeholder="Chọn khối"
              :show-clear="true"
              @change="loadLophoc(true)"
            />
            <Dropdown
              class="mr-2"
              v-model="opition.Loailop_ID"
              :options="tudiens[2]"
              optionLabel="text"
              optionValue="value"
              placeholder="Chọn loại lớp"
              :show-clear="true"
              @change="loadLophoc(true)"
            />
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
              @click="delLophoc()"
            />
            <Button
              label="Export"
              icon="pi pi-file-excel"
              class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            />
            <Button
              label="Thêm lớp"
              icon="pi pi-plus"
              class="mr-2"
              @click="showModalAddLophoc"
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
        field="Tenlop"
        header="Tên lớp"
        headerStyle="text-align:center;"
        bodyStyle="text-align:center;"
      >
      </Column>
      <Column
        field="Khoihoc_Ten"
        header="Khối"
        headerStyle="text-align:center;max-width:80px"
        bodyStyle="text-align:center;max-width:80px;"
      >
      </Column>
      <Column
        field="full_name"
        header="Giáo viên CN"
        headerStyle="text-align:center;max-width:200px"
        bodyStyle="text-align:center;max-width:200px;"
      >
      </Column>
      <Column
        field="Somon"
        header="Môn"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:50px"
        bodyStyle="text-align:center;max-width:50px"
      >
        <template #body="md">
          <Badge
            @click="showConfigmon(md.data)"
            v-if="md.data.IsPermision && md.data.Somon > 0"
            :value="md.data.Somon"
            severity="success"
          ></Badge>
        </template>
      </Column>
      <Column
        field="Sogv"
        header="GV"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:50px"
        bodyStyle="text-align:center;max-width:50px"
      >
        <template #body="md">
          <Badge
            @click="showConfiggv(md.data)"
            v-if="md.data.IsPermision && md.data.Sogv + md.data.Soql > 0"
            :value="md.data.Sogv + md.data.Soql"
            severity="info"
          ></Badge>
        </template>
      </Column>
      <Column
        field="Sohs"
        header="HS"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:50px"
        bodyStyle="text-align:center;max-width:50px"
      >
        <template #body="md">
          <Badge
            @click="showConfighs(md.data)"
            v-if="md.data.Sohs > 0"
            :value="md.data.Sohs"
            severity="danger"
          ></Badge>
        </template>
      </Column>
      <Column
        field="Trangthai"
        header="Trạng thái"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:90px"
        bodyStyle="text-align:center;max-width:90px"
      >
        <template #body="md">
          <Checkbox
            v-if="md.data.IsPermision"
            v-model="md.data.Trangthai"
            :binary="true"
            :disabled="!md.data.IsPermision"
            @change="md.data.IsPermision ? upTrangthaiLophoc(md.data) : null"
          />
        </template>
      </Column>
      <Column
        headerClass="text-center"
        bodyClass="text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #header> </template>
        <template #body="md">
          <Button
            icon="pi pi-ellipsis-h"
            class="mr-2 p-button-text p-button-secondary"
            @click="toggleLop($event, md.data)"
            aria-haspopup="true"
            aria-controls="overlay_Lop"
          />
          <Menu id="overlay_Lop" ref="menuButLops" :model="itemButLops" :popup="true" />
          <Button
            type="button"
            icon="pi pi-pencil"
            class="p-button-rounded p-button-sm p-button-info"
            style="margin-right: 0.5rem"
            v-if="md.data.IsPermision"
            @click="editLophoc(md.data)"
          ></Button>
          <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="delLophoc(md.data)"
            v-if="md.data.IsPermision"
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
    header="Cập nhật lớp học"
    v-model:visible="displayAddLophoc"
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
            v-model="lophoc.Lophoc_ID"
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tên lớp <span class="redsao">(*)</span></label>
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="lophoc.Tenlop"
            :class="{ 'p-invalid': v$.Tenlop.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.Tenlop.$invalid && submitted) || v$.Tenlop.$pending.$response"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.Tenlop.required.$message
                .replace("Value", "Tên")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Loại lớp</label>
          <Dropdown
            class="col-4 ip36 p-0"
            v-model="lophoc.Loailop_ID"
            :options="tudiens[2]"
            optionLabel="text"
            optionValue="value"
          />
          <label class="col-2 text-right">Khối</label>
          <Dropdown
            class="col-4 ip36 p-0"
            v-model="lophoc.Khoihoc_ID"
            :options="tudiens[0]"
            optionLabel="text"
            optionValue="value"
            :filter="true"
            optionGroupLabel="text"
            optionGroupChildren="items"
          />
        </div>
        <div class="field col-12 md:col-12" v-if="store.getters.user.IsAdminTruong">
          <label class="col-2 text-left">GVCN</label>
          <Dropdown
            :virtualScrollerOptions="{ itemSize: 48 }"
            :options="tudiens[3]"
            v-model="Nguoiquanly_ID"
            optionLabel="full_name"
            placeholder="Chọn giáo viên chủ nhiệm"
            :filter="true"
            :showClear="true"
            class="col-10"
          >
            <template #value="slotProps">
              <div
                class="user-item user-item-value"
                v-if="slotProps.value && slotProps.value.full_name"
              >
                <Avatar
                  v-bind:label="
                    slotProps.value.Avartar
                      ? ''
                      : slotProps.value.full_name.substring(0, 1)
                  "
                  v-bind:image="basedomainURL + slotProps.value.Avartar"
                  style="background-color: #2196f3; color: #ffffff"
                  class="mr-2"
                  shape="circle"
                />
                <div>{{ slotProps.value.full_name }}</div>
              </div>
              <span v-else>
                {{ slotProps.placeholder }}
              </span>
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
                <div>
                  {{ slotProps.option.full_name
                  }}<i class="gvpb">{{ slotProps.option.organization_name }}</i>
                </div>
              </div>
            </template>
          </Dropdown>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Từ khoá</label>
          <Chips
            spellcheck="false"
            class="col-10 p-0"
            v-model="lophoc.Tukhoa"
            :addOnBlur="true"
            separator=","
          />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-4 ip36 p-0" v-model="lophoc.STT" />
          <label style="vertical-align: text-bottom" class="col-2 text-right"
            >Trạng thái</label
          >
          <InputSwitch v-model="lophoc.Trangthai" class="mt-1" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddLophoc"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
    </template>
  </Dialog>
  <Dialog
    :header="
      'Cấu hình môn học cho lớp ' +
      (lophoc ? lophoc.Tenlop : '') +
      ' - Năm học ' +
      store.getters.namhoc.Namhoc_Ten
    "
    v-model:visible="displayConfigMonhoc"
    :style="{ width: '80vw', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <CompLopMonView
      :lophoc="lophoc"
      :tudiens="tudiens"
      :reload-componnent="reloadComponnent"
    ></CompLopMonView>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="displayConfigMonhoc = false"
        class="p-button-raised p-button-secondary"
      />
    </template>
  </Dialog>
  <Dialog
    :header="
      'Quản lý giáo viên trong lớp ' +
      (lophoc ? lophoc.Tenlop : '') +
      ' - Năm học ' +
      store.getters.namhoc.Namhoc_Ten
    "
    v-model:visible="displayConfigGV"
    :style="{ width: '1024px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <CompLopGVView
      :lophoc="lophoc"
      :tudiens="tudiens"
      :reload-componnent="reloadComponnent"
    ></CompLopGVView>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="displayConfigGV = false"
        class="p-button-raised p-button-secondary"
      />
    </template>
  </Dialog>
  <Dialog
    :header="
      'Quản lý học sinh trong lớp ' +
      (lophoc ? lophoc.Tenlop : '') +
      ' - Năm học ' +
      store.getters.namhoc.Namhoc_Ten
    "
    v-model:visible="displayConfigHS"
    :style="{ width: '1024px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <CompLopHSView
      :lophoc="lophoc"
      :tudiens="tudiens"
      :reload-componnent="reloadComponnent"
    ></CompLopHSView>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="displayConfigHS = false"
        class="p-button-raised p-button-secondary"
      />
    </template>
  </Dialog>
   <Menu id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" />
</template>
<style scoped>
.gvpb {
  display: block;
  color: #607d8b;
  font-size: 13px;
}
</style>
