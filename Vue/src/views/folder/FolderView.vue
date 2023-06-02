<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import arrIcons from "../../assets/json/icons.json";
import { encr } from "../../util/function";
import moment from "moment";
import router from "@/router";
const cryoptojs = inject("cryptojs");
//init Model
const thumuc = ref({
  TenThumuc: "",
  STT: 1,
  Trangthai: true,
  IsThumuc: true,
});
//Valid Form
const submitted = ref(false);
const rules = {
  TenThumuc: {
    required,
  },
};
const v$ = useVuelidate(rules, thumuc);
//Khai báo biến
const menuButMores = ref();
const itemButMores = ref([
  {
    label: "Thêm thư mục con",
    icon: "pi pi-plus-circle",
    command: (event) => {
      addTreeThumuc(thumuc.value);
    },
  },
  {
    label: "Sửa thư mục",
    icon: "pi pi-pencil",
    command: (event) => {
      editThumuc(thumuc.value);
    },
  },
  {
    label: "Xoá thư mục",
    icon: "pi pi-trash",
    command: (event) => {
      delThumuc(thumuc.value);
    },
  },
]);
const toggleMores = (event, u) => {
  thumuc.value = u;
  menuButMores.value.toggle(event);
};
const store = inject("store");
const selectCapcha = ref();
const selectedKey = ref();
const selectedNodes = ref([]);
const filters = ref({});
const opition = ref({ search: "", PageNo: 1, PageSize: 20 });
const opitionFile = ref({ search: "", PageNo: 1, PageSize: 50 });
const thumucs = ref();
const treethumucs = ref();
const displayAddThumuc = ref(false);
const isFirst = ref(true);
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
      exportThumuc("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportThumuc("ExportExcelMau");
    },
  },
]);

//Khai báo function
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const onNodeSelect = (node) => {
  selectedNodes.value.push(node.data.Thumuc_ID);
  goThumuc(node.data);
};
const delFile = () => {};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(selectedNodes.value.indexOf(node.data.Thumuc_ID), 1);
};
//Show Modal
const showModalAddThumuc = () => {
  submitted.value = false;
  selectCapcha.value = {};
  thumuc.value = {
    TenThumuc: "",
    STT: thumucs.value.length + 1,
    Trangthai: true,
    IsPublic: true,
    IsSub: true,
    IsUpload: true,
  };
  displayAddThumuc.value = true;
};
const chonfile = (id) => {
  document.getElementById(id).click();
};
const closedisplayAddThumuc = () => {
  displayAddThumuc.value = false;
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  loadThumuc(true);
};
const onSearch = () => {
  loadThumuc(true);
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
const loadThumuc = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "File_Thumuc_List",
        par: [
          { par: "PageNo", va: opition.value.PageNo },
          { par: "PageSize", va: opition.value.PageSize },
          { par: "Search", va: opition.value.search },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        let obj = renderTree(data, "Thumuc_ID", "TenThumuc", "thư mục");
        thumucs.value = obj.arrChils;
        treethumucs.value = obj.arrtreeChils;
      } else {
        thumucs.value = [];
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
const editThumuc = (md) => {
  submitted.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  displayAddThumuc.value = true;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      { proc: "File_Thumuc_Get", par: [{ par: "Thumuc_ID", va: md.Thumuc_ID }] },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        thumuc.value = data[0][0];
        selectCapcha.value = {};
        selectCapcha.value[thumuc.value.Capcha_ID || "-1"] = true;
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
  let keys = Object.keys(selectCapcha.value);
  thumuc.value.Capcha_ID = keys[0];
  if (thumuc.value.Capcha_ID == -1) {
    thumuc.value.Capcha_ID = null;
  }
  addThumuc();
};

const addTreeThumuc = (md) => {
  selectCapcha.value = {};
  selectCapcha.value[md.Thumuc_ID] = true;
  thumuc.value = {
    TenThumuc: "",
    STT: thumucs.value.length + 1,
    Trangthai: true,
    IsPublic: true,
  };
  submitted.value = false;
  displayAddThumuc.value = true;
};
const addThumuc = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: thumuc.value.Thumuc_ID ? "put" : "post",
    url: baseURL + `/api/File/${thumuc.value.Thumuc_ID ? "Update_Thumuc" : "Add_Thumuc"}`,
    data: thumuc.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật thư mục thành công!");
        loadThumuc();
        closedisplayAddThumuc();
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

const delThumuc = (md) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá thư mục này không!",
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
          .delete(baseURL + "/api/File/Del_Thumuc", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.Thumuc_ID] : selectedNodes.value,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá thư mục thành công!");
              loadThumuc();
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

const exportThumuc = (method) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "name", va: "File_Thumuc" }];
  if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
  }
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH ĐƠN VỊ",
        proc: "File_Thumuc_ListExport",
        par: par,
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
const rowTree = (data) => {
  return data.Thumuc_ID == thumuc.value.Thumuc_ID ? "chonthumuc" : "";
};
//File
const layout = ref("grid");
const goThumuc = (tm) => {
  thumuc.value = tm;
  opitionFile.value.loading = true;
  opitionFile.value.Thumuc_ID = tm.Thumuc_ID;
  selectCapcha.value = {};
  selectCapcha.value[thumuc.value.Thumuc_ID || "-1"] = true;
  loadFile(true);
};
const files = ref([]);
const file = ref({});
const displayAddFile = ref(false);
const showModalAddFile = () => {
  displayAddFile.value = true;
  file.value.Thumuc_ID = thumuc.value.Thumuc_ID;
};
const closedisplayAddFile = () => {
  displayAddFile.value = false;
};
const toggleExportFile = () => {};
const onRefershFile = () => {
  opitionFile.value.loading = true;
};
const images = ref([]);
const activeIndex = ref(0);
const displayFileView = ref(false);
const displayFileHeader = ref("Xem File");
const imageClick = (fi) => {
  let idx = images.value.findIndex((x) => x.File_ID == fi.File_ID);
  activeIndex.value = idx;
  displayAlbum.value = true;
};
const fileClick = (fi) => {
  displayFileView.value = true;
  displayFileHeader.value = fi.TenFile;
  let par = encr(fi.Duongdan + "&" + fi.TenFile + "&" + fi.File_ID, SecretKey, cryoptojs);
  let url = baseURL + "/Home/ViewFile?url=" + par;
  setTimeout(() => {
    let iframe = document.getElementById("fileView");
    iframe.src = url;
  }, 0);
};
const displayAlbum = ref(false);
const responsiveOptions = [
  {
    breakpoint: "1024px",
    numVisible: 5,
  },
  {
    breakpoint: "768px",
    numVisible: 3,
  },
  {
    breakpoint: "560px",
    numVisible: 1,
  },
];
const formatBytes = (bytes, decimals = 2) => {
  if (bytes === 0) return "0 Bytes";

  const k = 1024;
  const dm = decimals < 0 ? 0 : decimals;
  const sizes = ["Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
};
const loadFile = (rf) => {
  if (rf) {
    opitionFile.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "File_Info_List",
        par: [
          { par: "Thumuc_ID", va: opitionFile.value.Thumuc_ID },
          { par: "PageNo", va: opitionFile.value.PageNo },
          { par: "PageSize", va: opitionFile.value.PageSize },
          { par: "Search", va: opitionFile.value.search },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((f) => {
        f.Ngay = moment(new Date(f.Ngaytao)).format("DD/MM/YYYY HH:mm:ss");
        f.DungluongMB = formatBytes(f.Dungluong);
        let idx = f.Duongdan.lastIndexOf(".");
        f.DuongdanThumb =
          f.Duongdan.substring(0, idx) + "_thumb" + f.Duongdan.substring(idx);
        f.filetype = f.Duongdan.substring(idx + 1);
      });
      files.value = data;
      images.value = data.filter((x) => x.IsImage);
      if (isFirst.value) isFirst.value = false;
      if (rf) {
        opitionFile.value.loading = false;
      }
    })
    .catch((error) => {
      console.log(error);
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const onUploadFile = (event) => {
  if (selectCapcha.value) {
    let keys = Object.keys(selectCapcha.value);
    file.value.Thumuc_ID = keys[0];
    if (file.value.Thumuc_ID == -1) {
      file.value.Thumuc_ID = null;
    }
  }
  let formData = new FormData();
  for (var i = 0; i < event.files.length; i++) {
    let file = event.files[i];
    formData.append(`file${i}`, file);
  }
  let md = { ...file.value };
  if (md.Tukhoa && md.Tukhoa.length > 0) {
    md.Tukhoa = md.Tukhoa.join(",");
  } else {
    md.Tukhoa = "";
  }
  formData.append("model", JSON.stringify(md));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "post",
    url: baseURL + `/api/File/Update_File`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Thêm file thành công!");
        loadFile();
        closedisplayAddFile();
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

onMounted(() => {
  //init
  if (document.getElementsByClassName("vsm--toggle-btn")[0])
    document.getElementsByClassName("vsm--toggle-btn")[0].click();
  loadThumuc(true);
  loadFile(true);
});
</script>
<template>
  <Galleria
    :value="images"
    v-model:activeIndex="activeIndex"
    :responsiveOptions="responsiveOptions"
    :numVisible="7"
    containerStyle="max-width: 100vw;max-height:100vh"
    :circular="true"
    :fullScreen="true"
    :showItemNavigators="true"
    :showThumbnails="true"
    v-model:visible="displayAlbum"
  >
    <template #item="slotProps">
      <img
        :src="basedomainURL + slotProps.item.Duongdan"
        :alt="slotProps.item.TenFile"
        style="max-width: 100%; max-height: 100%; display: block"
      />
    </template>
    <template #thumbnail="slotProps">
      <img
        :src="basedomainURL + slotProps.item.DuongdanThumb"
        :alt="slotProps.item.TenFile"
        height="80"
        style="display: block"
      />
    </template>
  </Galleria>
  <div class="main-layout true flex-grow-1 flex" v-if="store.getters.islogin">
    <Splitter>
      <SplitterPanel
        class="shadow-1"
        style="
          width: 320px;
          max-width: 360px;
          min-width: 215px;
          background-color: #fff;
          flex-basis: unset;
          flex-grow: unset;
        "
      >
        <TreeTable
          :value="thumucs"
          selectionMode="single"
          v-model:selectionKeys="selectedKey"
          @nodeSelect="onNodeSelect"
          :loading="opition.loading"
          class="p-treetable-sm e-sm"
          :paginator="thumucs && thumucs.length > 20"
          :rows="20"
          :scrollable="true"
          scrollHeight="flex"
        >
          <template #header>
            <h3 class="thumuc-title mt-0 ml-1 mb-2">
              <i class="pi pi-microsoft"></i> Danh sách Thư mục
            </h3>
            <Toolbar class="w-full custoolbar block">
              <template #start>
                <span class="p-input-icon-left w-full">
                  <i class="pi pi-search" />
                  <InputText
                    type="text"
                    spellcheck="false"
                    v-model="opition.search"
                    placeholder="Tìm kiếm"
                    v-on:keyup.enter="onSearch"
                  />
                </span>
                <Button
                  label=""
                  icon="pi pi-plus"
                  class="ml-2"
                  @click="showModalAddThumuc"
                />
              </template>
            </Toolbar>
          </template>
          <Column field="TenThumuc" header="" :expander="true" headerClass="hide-header">
            <template #body="md">
              <img src="../../assets/image/file/folder480.svg" height="24" class="mr-1" />
              <span :class="'thumuc' + md.node.data.IsThumuc">{{
                md.node.data.TenThumuc
              }}</span>
            </template>
          </Column>
          <Column
            headerClass="text-center hide-header"
            headerStyle="text-align:center;max-width:70px"
            bodyStyle="text-align:center;max-width:70px"
          >
            <template #header> </template>
            <template #body="md">
              <Button
                icon="pi pi-ellipsis-h"
                style="color: inherit"
                class="bmorefunction p-button-rounded p-button-text ml-2"
                @click="toggleMores($event, md.node.data)"
                aria-haspopup="true"
                aria-controls="overlay_More"
              />
              <Menu
                id="overlay_More"
                ref="menuButMores"
                :model="itemButMores"
                :popup="true"
              />
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
      </SplitterPanel>
      <SplitterPanel
        class="flex-grow-1"
        style="margin-left: 1.5px; background-color: #fff"
      >
        <DataView
          class="w-full h-full e-sm flex flex-column bgWhite"
          :lazy="true"
          :value="files"
          :layout="layout"
          :loading="opitionFile.loading"
          :paginator="opitionFile.totalRecords > opitionFile.PageSize"
          :rows="opitionFile.PageSize"
          :totalRecords="opitionFile.totalRecords"
          :pageLinkSize="opitionFile.PageSize"
          @page="onPage($event)"
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          :rowsPerPageOptions="[8, 12, 20, 50, 100]"
          currentPageReportTemplate=""
          responsiveLayout="scroll"
          :scrollable="true"
        >
          <template #header>
            <h3 class="module-title mt-0 ml-1 mb-2">
              <i class="pi pi-folder"></i>
              {{ thumuc.TenThumuc ? thumuc.TenThumuc : "File" }}
              <span v-if="opitionFile.totalRecords > 0"
                >({{ opitionFile.totalRecords }})</span
              >
            </h3>
            <Toolbar class="w-full custoolbar">
              <template #start>
                <span class="p-input-icon-left ml-2">
                  <i class="pi pi-search" />
                  <InputText
                    type="text"
                    class="p-inputtext-sm"
                    spellcheck="false"
                    v-model="opitionFile.search"
                    placeholder="Tìm kiếm"
                    v-on:keyup.enter="onSearch"
                  />
                </span>
              </template>

              <template #end>
                <DataViewLayoutOptions v-model="layout" />
                <Button
                  class="mr-2 ml-2 p-button-sm p-button-outlined p-button-secondary"
                  icon="pi pi-refresh"
                  @click="onRefershFile"
                />
                <!-- <Button
                  label="Xoá"
                  icon="pi pi-trash"
                  class="mr-2 p-button-danger"
                  v-if="selectedNodes.length > 0"
                  @click="delFile"
                /> -->
                <Button
                  label="Export"
                  icon="pi pi-file-excel"
                  class="mr-2 p-button-sm p-button-outlined p-button-secondary"
                  @click="toggleExportFile"
                  aria-haspopup="true"
                  aria-controls="overlay_Export"
                />
                <Menu
                  id="overlay_Export"
                  ref="menuButs"
                  :model="itemButs"
                  :popup="true"
                />
                <Button
                  label="Thêm file"
                  icon="pi pi-plus"
                  class="mr-2 p-button-sm"
                  @click="showModalAddFile"
                />
              </template>
            </Toolbar>
          </template>
          <template #grid="slotProps">
            <div class="p-2 m-2" style="width: 160px">
              <div>
                <div class="align-items-center justify-content-center text-center">
                  <Image
                    @click="imageClick(slotProps.data)"
                    v-if="slotProps.data.IsImage"
                    v-bind:src="basedomainURL + slotProps.data.DuongdanThumb"
                    width="160"
                    height="100"
                    loading="lazy"
                    imageClass="img-contain"
                  />
                  <Image
                    @click="fileClick(slotProps.data)"
                    v-if="!slotProps.data.IsImage"
                    v-bind:src="
                      'src/assets/image/file/' + slotProps.data.filetype + '.png'
                    "
                    height="64"
                  />
                </div>
                <div
                  class="align-items-center justify-content-center text-center filename"
                >
                  {{ slotProps.data.TenFile }}
                </div>
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
            </div>
          </template>
          <template #list="slotProps">
            <div class="p-2 w-full" style="background-color: #fff">
              <div class="flex align-items-center justify-content-center">
                <Image
                  @click="imageClick(slotProps.data)"
                  v-if="slotProps.data.IsImage"
                  v-bind:src="basedomainURL + slotProps.data.DuongdanThumb"
                  width="36"
                  height="36"
                  class="mr-2"
                  loading="lazy"
                />
                <Image
                  v-if="!slotProps.data.IsImage"
                  v-bind:src="'src/assets/image/file/' + slotProps.data.filetype + '.png'"
                  height="36"
                  width="36"
                  class="mr-2"
                />
                <div class="flex flex-column flex-grow-1">
                  <Button
                    class="p-button-text p-0"
                    style="color: inherit; padding: 0 !important"
                    @click="imageClick(slotProps.data)"
                    ><h3 class="mb-1 mt-0">{{ slotProps.data.TenFile }}</h3></Button
                  >
                  <i style="font-size: 10pt; color: #999">{{
                    slotProps.data.DungluongMB
                  }}</i>
                </div>
                <div
                  v-bind:class="'rolefalse'"
                  style="
                    background-color: #eee;
                    font-size: 10pt;
                    padding: 5px;
                    border-radius: 5px;
                  "
                >
                  {{ slotProps.data.Ngay }}
                </div>

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
      </SplitterPanel>
    </Splitter>
  </div>
  <Dialog
    header="Cập nhật thư mục"
    v-model:visible="displayAddThumuc"
    :style="{ width: '640px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Tên <span class="redsao">(*)</span></label>
          <InputText
            spellcheck="false"
            class="col-10 ip36"
            v-model="thumuc.TenThumuc"
            :class="{ 'p-invalid': v$.TenThumuc.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.TenThumuc.$invalid && submitted) || v$.TenThumuc.$pending.$response"
          class="col-10 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-2 text-left"></label>
            <span class="col-10 pl-3">{{
              v$.TenThumuc.required.$message
                .replace("Value", "Tên thư mục")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Cấp cha</label>
          <TreeSelect
            class="col-10"
            v-model="selectCapcha"
            :options="treethumucs"
            :showClear="true"
            placeholder=""
            optionLabel="data.TenThumuc"
            optionValue="data.Thumuc_ID"
          ></TreeSelect>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">STT</label>
          <InputNumber class="col-2 ip36 p-0" v-model="thumuc.STT" />
          <label style="vertical-align: text-bottom" class="col-2 text-right"
            >Trạng thái</label
          >
          <InputSwitch v-model="thumuc.Trangthai" class="mt-1" />
          <label style="vertical-align: text-bottom" class="col-2 text-right"
            >Public</label
          >
          <InputSwitch v-model="thumuc.IsPublic" class="mt-1" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAddThumuc"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
    </template>
  </Dialog>
  <Dialog
    header="Thêm File"
    v-model:visible="displayAddFile"
    :style="{ width: '760px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <div class="field col-12 md:col-12">
      <label class="col-2 text-left">Thư mục</label>
      <TreeSelect
        class="col-10"
        v-model="selectCapcha"
        :options="treethumucs"
        :showClear="true"
        placeholder=""
        optionLabel="data.TenThumuc"
        optionValue="data.Thumuc_ID"
      ></TreeSelect>
    </div>
    <div class="field col-12 md:col-12">
      <label class="col-2 text-left">Từ khoá</label>
      <Chips
        spellcheck="false"
        class="col-10 p-0"
        v-model="file.Tukhoa"
        :addOnBlur="true"
        separator=","
      />
    </div>
    <div class="field col-12 md:col-12">
      <label class="col-2 text-left">Public</label>
      <InputSwitch v-model="file.IsPublic" class="mt-1" />
    </div>
    <div class="field col-12 md:col-12">
      <FileUpload
        class="uploadFile"
        name="files[]"
        :customUpload="true"
        @uploader="onUploadFile"
        :multiple="true"
        accept="application/msword, application/vnd.ms-excel, application/vnd.ms-powerpoint,text/plain, application/pdf, image/*"
        :maxFileSize="100000000"
      >
        <template #empty>
          <p>Kéo các file vào đây để tải lên.</p>
        </template>
      </FileUpload>
    </div>
  </Dialog>
  <Dialog
    :header="displayFileHeader"
    v-model:visible="displayFileView"
    :style="{ width: '1024px', height: '70vh', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <iframe id="fileView" frameBorder="0"></iframe>
  </Dialog>
</template>
<style scoped>
#fileView {
  width: 100%;
  height: 100%;
  min-height: 70vh;
}
.hide-header {
  display: none;
  margin: 0;
  padding: 0;
  border: none;
}
.filename {
  word-break: break-word;
  font-size: 11t;
}
.bmorefunction {
  visibility: hidden;
}
tr:hover .bmorefunction {
  visibility: visible;
}
.classthumuc {
  background-color: aliceblue;
}
span.thumuctrue {
  font-weight: 500;
}
.chiptrue {
  background-color: #4285f4;
  color: #fff;
  font-size: 0.875rem;
  padding: 5px 10px;
}
.chipfalse {
  background-color: #689f38;
  color: #fff;
  font-size: 0.875rem;
  padding: 5px 10px;
}
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
