<script setup>
import { ref, inject, onMounted, computed, defineProps } from "vue";
import { required } from "@vuelidate/validators";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import arrIcons from "../../assets/json/icons.json";
import { encr } from "../../util/function";
import moment from "moment";
const props = defineProps({
  ChonFile: Function,
});
const cryoptojs = inject("cryptojs");
//init Model
const thumuc = ref({
  name: "",
});
//Valid Form
const submitted = ref(false);
const rules = {
  name: {
    required,
  },
};
const v$ = useVuelidate(rules, thumuc);
//Khai báo biến
const menuButMores = ref();
const itemButMores = ref([]);
let menuThumuc = [
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
];
let menuAll = [
  {
    label: "Thêm thư mục",
    icon: "pi pi-folder",
    command: (event) => {
      showModalAddThumuc();
    },
  },
  {
    label: "Tải file",
    icon: "pi pi-upload",
    command: (event) => {
      showModalAddFile();
    },
  },
];
const toggleMores = (event, u) => {
  if (u) {
    thumuc.value = u;
    itemButMores.value = menuThumuc;
  } else {
    itemButMores.value = menuAll;
  }
  menuButMores.value.show(event);
};
const store = inject("store");
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
//Khai báo function
const onNodeSelect = (node, event) => {
  selectedNodes.value.push(node.data.path);
  goThumuc(node.data);
  if (event) {
    var elements = document
      .querySelector(".p-treetable")
      .getElementsByClassName("p-highlight");
    while (elements.length > 0) {
      elements[0].classList.remove("p-highlight");
    }
    let tr = event.target.parentNode.parentNode.parentNode;
    if (tr) {
      tr.classList.add("p-highlight");
    }
  }
};
//Show Modal
const showModalAddThumuc = () => {
  submitted.value = false;
  thumuc.value = {
    name: "",
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
  loadFile(true);
};
const renderTree = (data, id, cid, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x[cid] == null || data.findIndex((a) => a[id] == x[cid]) == -1)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x[cid] == pid);
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
        let dts = data.filter((x) => x[cid] == pid);
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
const expandedKeys = ref({});
const loadThumuc = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  let formData = new FormData();
  axios({
    method: "post",
    url: baseURL + `/api/Upload/Folder_ListAll`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      let data = response.data.folders;
      if (data.length > 0) {
        let obj = renderTree(data, "path", "ppath", "name", "thư mục");
        thumucs.value = obj.arrChils;
        treethumucs.value = obj.arrtreeChils;
        thumucs.value.forEach((node) => {
          expandedKeys.value[node.key] = true;
          node.chon = choiceNodes.value.findIndex((x) => x.path == node.path) != -1;
        });
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
  isAdd = false;
  submitted.value = false;
  displayAddThumuc.value = true;
  md.oldname = md.name;
  thumuc.value = { ...md };
};
const handleSubmit = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  addThumuc();
};

const addTreeThumuc = (md) => {
  isAdd = true;
  thumuc.value = {
    name: "",
    path: md.path,
  };
  submitted.value = false;
  displayAddThumuc.value = true;
};
let isAdd = true;
const addThumuc = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: !isAdd ? "put" : "post",
    url: baseURL + `/api/Upload/${isAdd ? "Folder_Add" : "Folder_Edit"}`,
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
        if (thumuc.value) {
          goThumuc(thumuc.value);
        }
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
          .delete(baseURL + "/api/Upload/Folder_Delete", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: md != null ? [md.path] : selectedNodes.value,
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
  return data.path == thumuc.value.path ? "chonthumuc" : "";
};
//File
const layout = ref("grid");
const goThumuc = (tm) => {
  thumuc.value = tm;
  opitionFile.value.loading = true;
  opitionFile.value.path = tm.path;
  if (tm.path) {
    var mmns = [];
    tm.path.split("\\").forEach((fd) => {
      mmns.push({
        label: fd,
        name: fd,
        path: fd,
      });
    });
    itemBreads.value = mmns;
  } else {
    itemBreads.value = [];
  }
  loadFile(true);
};
const files = ref([]);
const file = ref({});
const displayAddFile = ref(false);
const showModalAddFile = () => {
  displayAddFile.value = true;
  file.value.path = thumuc.value.path;
};
const closedisplayAddFile = () => {
  displayAddFile.value = false;
};
const onRefershFile = () => {
  opitionFile.value.loading = true;
};
const images = ref([]);
const activeIndex = ref(0);
const displayFileView = ref(false);
const displayFileHeader = ref("Xem File");
const imageClick = (fi) => {
  let idx = images.value.findIndex((x) => x.path == fi.path);
  activeIndex.value = idx;
  displayAlbum.value = true;
};
const fileClick = (fi) => {
  displayFileView.value = true;
  displayFileHeader.value = fi.name;
  let par = encr(fi.path.replaceAll(fileURL, "") + "&" + fi.name, SecretKey, cryoptojs);
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
  let formData = new FormData();
  if (thumuc.value.path) {
    formData.append("folder", thumuc.value.path);
  }
  axios({
    method: "post",
    url: baseURL + `/api/Upload/File_ListAll`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      let dfolders = response.data.folders;
      let dfiles = response.data.files;
      if (dfolders) {
        dfolders.forEach((f) => {
          f.date = moment(new Date(f.date)).format("DD/MM/YYYY HH:mm:ss");
          f.mdate = moment(new Date(f.mdate)).format("DD/MM/YYYY HH:mm:ss");
          f.DungluongMB = formatBytes(f.size);
          f.IsFolder = true;
          f.chon = choiceNodes.value.findIndex((x) => x.path == f.path) != -1;
        });
      }
      dfiles.forEach((f) => {
        f.date = moment(new Date(f.date)).format("DD/MM/YYYY HH:mm:ss");
        f.mdate = moment(new Date(f.mdate)).format("DD/MM/YYYY HH:mm:ss");
        f.DungluongMB = formatBytes(f.size);
        let idx = f.path.lastIndexOf(".");
        f.filetype = f.path.substring(idx + 1);
        f.IsFolder = false;
        f.cpath = f.path;
        f.chon = choiceNodes.value.findIndex((x) => x.path == f.path) != -1;
        f.path =
          fileURL + "/Portals/Upload/" + store.getters.user.user_id + "/" + f.path;
        //f.path = fileURL + "/Portals/" + f.path;
        f.pathThumb = f.path.substring(0, idx) + "_thumb" + f.path.substring(idx);
      });
      files.value = dfolders.concat(dfiles);
      images.value = dfiles.filter((x) => x.isImage);
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
  let formData = new FormData();
  for (var i = 0; i < event.files.length; i++) {
    let file = event.files[i];
    formData.append(`file${i}`, file);
  }
  formData.append("folder", thumuc.value.path);
  formData.append("rswidth", "240");
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "post",
    url: baseURL + `/api/Upload/Update_AllFile`,
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
const compFiles = computed(() =>
  files.value.filter((x) => x.name.includes(opition.value.search))
);
const homeBread = ref({
  icon: "pi pi-folder",
  label: "Folder",
});
const confirm = useConfirm();
const isChoiceCopy = ref();
const itemBreads = ref([]);
const choiceNodes = ref([]);
const copyChoiceNode = (iscopy) => {
  isChoiceCopy.value = iscopy;
  toast.success(`Vui lòng chọn folder để ${iscopy ? "sao chép" : "di chuyển"}!`);
};
const cancelCopyNode = () => {
  isChoiceCopy.value = null;
  choiceNodes.value = [];
  compFiles.value
    .filter((x) => x.chon)
    .forEach((element) => {
      element.chon = false;
    });
};
const activeChoiceNode = () => {
  if (props.ChonFile) {
    props.ChonFile(choiceNodes.value.map((x) => x.cpath || x.path));
  }
};
const pasteChoiceNode = (event) => {
  confirm.require({
    target: event.currentTarget,
    message: `Bạn có muốn ${
      isChoiceCopy.value ? "sao chép" : "di chuyển"
    } các file này không`,
    icon: "pi pi-info-circle",
    acceptLabel: "Có",
    rejectLabel: "Không",
    accept: () => {
      swal.fire({
        width: 110,
        didOpen: () => {
          swal.showLoading();
        },
      });
      axios({
        method: "put",
        url: baseURL + `/api/Upload/Folder_Copy`,
        data: {
          folders: choiceNodes.value.map((x) => x.cpath || x.path),
          newfolder: thumuc.value.path,
          coppy: isChoiceCopy.value,
        },
        headers: {
          Authorization: `Bearer ${store.getters.token}`,
        },
      })
        .then((response) => {
          swal.close();
          if (response.data.err != "1") {
            swal.close();
            toast.success(
              `${isChoiceCopy.value ? "sao chép" : "di chuyển"} file thành công!`
            );
            if (choiceNodes.value.filter((x) => x.IsFolder).length > 0) {
              loadThumuc(true);
            }
            isChoiceCopy.value = null;
            choiceNodes.value = [];
            loadFile(true);
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
        });
    },
    reject: () => {},
  });
};
const deleChoiceNode = () => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá các file này không!",
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
          .delete(baseURL + "/api/Upload/File_Delete", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: choiceNodes.value.map((x) => x.cpath || x.path),
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá file thành công!");
              if (choiceNodes.value.filter((x) => x.IsFolder).length > 0) {
                loadThumuc(true);
              }
              choiceNodes.value = [];
              loadFile(true);
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
const toogleFileClick = (node) => {
  if (node.chon) {
    node.chon = false;
    let idx = choiceNodes.value.findIndex((x) => x.path == node.path);
    if (idx != -1) {
      choiceNodes.value.splice(idx, 1);
      if (choiceNodes.value.length == 0) {
        isChoiceCopy.value = null;
      }
    }
  } else {
    node.chon = true;
    choiceNodes.value.push(node);
  }
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
  <ConfirmPopup></ConfirmPopup>
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
        :src="slotProps.item.path"
        :alt="slotProps.item.name"
        style="max-width: 100%; max-height: 100%; display: block"
      />
    </template>
    <template #thumbnail="slotProps">
      <img
        :src="slotProps.item.path"
        :alt="slotProps.item.name"
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
          :expandedKeys="expandedKeys"
          selectionMode="single"
          v-model:selectionKeys="selectedKey"
          @nodeSelect="onNodeSelect"
          :loading="opition.loading"
          :filters="filters"
          filterMode="lenient"
          class="p-treetable-sm e-sm"
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
                    type="search"
                    spellcheck="false"
                    v-model="filters['global']"
                    placeholder="Tìm kiếm"
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
          <Column field="name" header="" :expander="true" headerClass="hide-header">
            <template #body="md">
              <Button
                style="color: inherit"
                class="p-button-text p-button-sm p-0 flex-grow-1"
                @click="onNodeSelect(md.node, $event)"
                @contextmenu="toggleMores($event, md.node.data)"
                aria-haspopup="true"
                aria-controls="overlay_More"
              >
                <img
                  src="../../assets/image/file/folder480.svg"
                  height="24"
                  class="mr-1"
                />
                <span :class="'thumuc' + md.node.data.IsThumuc">{{
                  md.node.data.name
                }}</span>
              </Button>
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
        <ContextMenu
          id="overlay_More"
          ref="menuButMores"
          :model="itemButMores"
          :popup="true"
        />
      </SplitterPanel>
      <SplitterPanel
        @contextmenu="toggleMores($event)"
        class="flex-grow-1"
        style="margin-left: 1.5px; background-color: #eee"
      >
        <DataView
          class="w-full h-full e-sm flex flex-column bgWhite"
          :value="compFiles"
          :layout="layout"
          :loading="opitionFile.loading"
          currentPageReportTemplate=""
          responsiveLayout="scroll"
          :scrollable="true"
        >
          <template #header>
            <Breadcrumb
              :home="homeBread"
              :model="itemBreads"
              style="padding: 10px; background-color: transparent; border: none"
            >
              <template #item="{ item }">
                <Button
                  style="color: inherit"
                  class="p-button-text p-button-sm p-0"
                  @click="goThumuc(item)"
                >
                  <img
                    style="vertical-align: bottom; margin-right: 5px"
                    src="../../assets/image/file/folder480.svg"
                    height="24"
                  />
                  {{ item.label }}
                </Button>
              </template>
            </Breadcrumb>

            <Toolbar class="w-full custoolbar">
              <template #start>
                <span class="p-input-icon-left ml-2">
                  <i class="pi pi-search" />
                  <InputText
                    type="search"
                    class="p-inputtext-sm"
                    spellcheck="false"
                    v-model="opition.search"
                    placeholder="Tìm kiếm"
                  />
                </span>
                <Button
                  v-if="choiceNodes.length > 0"
                  type="button"
                  label="Chọn"
                  @click="activeChoiceNode"
                  icon="pi pi-folder"
                  class="p-button-info ml-1 mr-1"
                  :badge="choiceNodes.length"
                  badgeClass="p-badge-warning"
                  style="color: #fff"
                />
                <Button
                  class="ml-2 p-button-sm p-button-outlined p-button-danger"
                  icon="pi pi-trash"
                  v-if="isChoiceCopy == null && choiceNodes.length > 0"
                  @click="deleChoiceNode"
                />
                <Button
                  class="ml-2 p-button-sm p-button-outlined p-button-info"
                  icon="pi pi-arrows-h"
                  v-tooltip.bottom="'Di chuyển'"
                  v-if="isChoiceCopy == null && choiceNodes.length > 0"
                  @click="copyChoiceNode(false)"
                />
                <Button
                  class="ml-2 p-button-sm p-button-outlined"
                  icon="pi pi-clone"
                  v-tooltip.bottom="'Dán'"
                  v-if="isChoiceCopy != null && choiceNodes.length > 0"
                  @click="pasteChoiceNode"
                />
                <Button
                  class="ml-2 p-button-sm p-button-danger p-button-outlined"
                  icon="pi pi-times-circle"
                  v-tooltip.bottom="'Huỷ'"
                  v-if="isChoiceCopy != null && choiceNodes.length > 0"
                  @click="cancelCopyNode"
                />
                <Button
                  class="ml-2 p-button-sm p-button-outlined"
                  icon="pi pi-copy"
                  v-tooltip.bottom="'Sao chép'"
                  v-if="isChoiceCopy == null && choiceNodes.length > 0"
                  @click="copyChoiceNode(true)"
                />
              </template>

              <template #end>
                <DataViewLayoutOptions v-model="layout" />
                <Button
                  class="mr-2 ml-2 p-button-sm p-button-outlined p-button-secondary"
                  icon="pi pi-refresh"
                  @click="onRefershFile"
                />
                <Button
                  label="Thêm file"
                  icon="pi pi-upload"
                  class="mr-2 p-button-sm"
                  @click="showModalAddFile"
                />
              </template>
            </Toolbar>
          </template>
          <template #grid="slotProps">
            <div class="m-2" style="width: 180px">
              <Card
                :class="'no-padd-card choice' + slotProps.data.chon"
                @click="toogleFileClick(slotProps.data)"
              >
                <template #content>
                  <div
                    class="align-items-center justify-content-center text-center"
                    v-if="!slotProps.data.IsFolder"
                  >
                    <Image
                      @dblclick="imageClick(slotProps.data)"
                      v-if="slotProps.data.isImage"
                      v-bind:src="slotProps.data.path"
                      width="160"
                      height="100"
                      loading="lazy"
                      imageClass="img-cover"
                    />
                    <Image
                      @dblclick="fileClick(slotProps.data)"
                      v-if="!slotProps.data.isImage"
                      v-bind:src="
                        '../../src/assets/image/file/' + slotProps.data.filetype + '.png'
                      "
                      height="100"
                    />
                    <div
                      class="align-items-center justify-content-center text-center filename"
                    >
                      {{ slotProps.data.name }}
                    </div>
                  </div>
                  <div
                    class="align-items-center justify-content-center text-center"
                    v-else
                  >
                    <img
                      @dblclick="goThumuc(slotProps.data)"
                      src="../../assets/image/file/folder480.svg"
                      height="100"
                    />
                    <div
                      class="align-items-center justify-content-center text-center filename"
                    >
                      {{ slotProps.data.name }}
                    </div>
                  </div>
                </template>
              </Card>
            </div>
          </template>
          <template #list="slotProps">
            <div :class="'p-2 w-full choice' + slotProps.data.chon">
              <div
                class="flex align-items-center justify-content-center"
                @click="toogleFileClick(slotProps.data)"
              >
                <Image
                  v-if="slotProps.data.isImage"
                  v-bind:src="slotProps.data.path"
                  width="36"
                  height="36"
                  class="mr-2"
                  imageClass="img-cover"
                  loading="lazy"
                />
                <Image
                  v-if="!slotProps.data.IsFolder && !slotProps.data.isImage"
                  v-bind:src="
                    '../../src/assets/image/file/' + slotProps.data.filetype + '.png'
                  "
                  height="36"
                  width="36"
                  class="mr-2"
                />
                <img
                  v-if="slotProps.data.IsFolder"
                  src="../../assets/image/file/folder480.svg"
                  height="36"
                  width="36"
                  class="mr-2"
                />
                <div class="flex flex-column flex-grow-1">
                  <Button
                    class="p-button-text p-0"
                    style="color: inherit; padding: 0 !important"
                    @dblclick="
                      slotProps.data.isImage == true
                        ? imageClick(slotProps.data)
                        : fileClick(slotProps.data)
                    "
                    ><h3 class="mb-1 mt-0">{{ slotProps.data.name }}</h3></Button
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
                  {{ slotProps.data.date }}
                </div>
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
    :style="{ width: '480px', zIndex: 1 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
      <div class="grid formgrid m-3">
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left">Tên <span class="redsao">(*)</span></label>
          <InputText
            autofocus
            spellcheck="false"
            class="col-9 ip36"
            v-model="thumuc.name"
            :class="{ 'p-invalid': v$.name.$invalid && submitted }"
          />
        </div>
        <small
          v-if="(v$.name.$invalid && submitted) || v$.name.$pending.$response"
          class="col-9 p-error"
        >
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left"></label>
            <span class="col-9 pl-3">{{
              v$.name.required.$message
                .replace("Value", "Tên thư mục")
                .replace("is required", "không được để trống")
            }}</span>
          </div></small
        >
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
      <FileUpload
        class="uploadFile"
        name="files[]"
        :customUpload="true"
        @uploader="onUploadFile"
        :multiple="true"
        accept="application/msword, application/vnd.ms-excel, application/vnd.ms-powerpoint,text/plain, application/pdf, image/*"
        :fileLimit="25"
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
.p-card.choicetrue,
div.choicetrue {
  background-color: #ffca28;
}
div.choicefalse {
  background-color: #fff;
}
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
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 1; /* number of lines to show */
  line-clamp: 1;
  -webkit-box-orient: vertical;
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
