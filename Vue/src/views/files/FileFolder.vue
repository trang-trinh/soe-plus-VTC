<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
// khai báo biến
var tbs = [];
var arr_users_coppy = [];
const checkHover = ref({
  folder_id: 0,
  module_key: null,
});
const selectCapcha = ref();
const treefolders = ref();
const folder = ref();
const selectedKey = ref([]);
const ListFolder = ref([]);
const menuButMores = ref();
const dataAddChildFolder = ref();
const isAdd = ref(true);
const temporganizations = ref([]);
const listUsers = ref([]);
const selectedNodeOrganization = ref();
const selectedUsers = ref([]);
const datalists = ref([]);
const typeTab = ref(2);
const options = ref({
  SearchText: null,
  Status: null,
  loading: false,
  parent_id: null,
  totalRecords: 1,
  search: "",
});
const listShare = ref([
  { value: 0, label: "Chỉ mình tôi" },
  { value: 1, label: "Mọi người" },
  { value: 2, label: "Nhóm người" },
]);
const ListFolderShare = ref([]);
const type_share = ref(2);
const ListTaskCategory = ref([]);
const expandedKeys = ref({});
const expandedKeysFolder = ref({});
const TreeData = ref();
const folder_temp = ref();
const loadFolder = (node, id) => {
  folder_temp.value = node.data;
  selectedKey.value = {};
  selectedKey.value[id || node.key] = true;
  //folder.value = node.data;
  isAdd.value = false;
  axios
    .post(
      baseURL + "/api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "file_folder_get",
            par: [{ par: "folder_id", va: id || node.key }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      var data = JSON.parse(response.data.data);
      if (data.length > 0) {
        folder.value = data[0][0];
        if (folder.value.keywords !== null)
          folder.value.keywords = folder.value.keywords.split(",");
        selectCapcha.value = {};
        typeTab.value = 2;
        if (folder.value.parent_id)
          selectCapcha.value[folder.value.parent_id] = true;
        if (folder.value.type_share == 2) {
          datalists.value = data[1].filter((x) => x.type == 2);
          selectedUsers.value = data[1].filter((x) => x.type == 3);
          initOrganization();
          initUser();
        } else if (folder.value.type_share == 1) {
          ListFolderShare.value = data[1].filter((x) => x.type == 1);
        }
      }
    })
    .catch((error) => {
      if (options.value.loading) options.value.loading = false;
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
const unNodeUnselect = (node) => {};
//khởi tạo
const sttCate = ref(1);
const submitted = ref(false);
const rules = {
  folder_name: {
    required,
    maxLength: maxLength(150),
  },
};
const v$ = useVuelidate(rules, folder);
const saveFolder = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (selectCapcha.value) {
    let keys = Object.keys(selectCapcha.value);
    folder.value.parent_id = keys[0];
  }
  if (folder.value.keywords)
    folder.value.keywords = folder.value.keywords.toString();
  if (folder.value.capacity == null) {
    folder.value.capacity = 0;
  }
  if (folder.value.type_share == 2) {
    let arr1 = datalists.value.filter(
      (x) => x.is_delete || x.is_delete || x.is_read
    );
    let arr2 = selectedUsers.value.filter(
      (x) => x.is_delete || x.is_delete || x.is_read
    );
    ListFolderShare.value = arr1.concat(arr2);
  }
  let formData = new FormData();
  formData.append("model", JSON.stringify(folder.value));
  formData.append("folder_share", JSON.stringify(ListFolderShare.value));
  axios({
    method: isAdd.value == false ? "put" : "post",
    url:
      baseURL +
      `/api/FileFolder/${
        isAdd.value == false ? "Update_Folder" : "Add_Folder"
      }`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        let id = response.data.data;
        swal.close();
        let expanted_coppy = JSON.parse(
          JSON.stringify(expandedKeysFolder.value)
        );
        expandedKeysFolder.value = {};
        expandedKeysFolder.value = expanted_coppy;
        if (isAdd.value == false) {
          toast.success("Cập nhật thư mục thành công!");
          folder_temp.value.folder_name = folder.value.folder_name;
        } else {
          toast.success("Thêm thư mục thành công!");
          if (dataAddChildFolder.value) {
            expandedKeysFolder.value[dataAddChildFolder.value.key] = true;
          }
          let folder_id_active = Object.keys(selectedKey.value)[0];
          expandedKeysFolder.value[folder_id_active] = true;
        }
        datalists.value = [];
        selectedUsers.value = [];
        loadTudien(id);
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
      console.log(error);
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const loadTudien = (id_active) => {
  axios
    .post(
      baseURL + "/api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "file_folder_dictionary",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data[0].length > 0) {
        let obj = renderTreeFolder(
          data[0].filter((x) => x.module_key == null),
          "folder_id",
          "folder_name"
        );
        treefolders.value = obj.arrtreeChils;
        ListFolder.value = [];
        // let datas = [...data[0]];
        // datas.forEach((item)=>{
        //   if(item.parent_id == null && item.module_key == null){
        //     if(item.is_public) item.parent_id = 'all'
        //     else item.parent_id = 'me'
        //   }
        // })
        // datas.unshift({folder_id:'all', folder_name:'Chung', parent_id: null, module_key: null, is_public:true});
        // datas.unshift({folder_id:'me', folder_name:'Của tôi', parent_id: null, module_key: null, is_public:false});
        data[0]
          .filter((x) => x.module_key == null)
          .forEach((element) => {
            let taskcategory = {
              key: element.folder_id,
              data: element,
              name: element.folder_name,
            };
            ListFolder.value.push(taskcategory);
          });
        RenderFolder(ListFolder);
        if (TreeData.value.length > 0) loadFolder(TreeData.value[0], id_active);
        // if(TreeData.value[0].children.length > 0){
        // expandedKeysFolder.value[TreeData.value[0].key] = true;
        // loadFolder(TreeData.value[0].children[0], id_active);
        // }
      }
      options.value.totalRecords = data[0].length;
    })
    .catch((error) => {
      console.log(error);
    });
};
//def
const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
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
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const expandNode = (node) => {
  if (node.children && node.children.length) {
    expandedKeys.value[node.key] = true;

    for (let child of node.children) {
      expandNode(child);
    }
  }
};

const initOrganization = () => {
  axios
    .post(
      baseURL + "/api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "file_organization_listtree",
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
        tbs = JSON.parse(data);
        debugger;
        if (tbs[0] != null && tbs[0].length > 0) {
          if (!isAdd.value) {
            tbs[0].forEach((item) => {
              if (
                datalists.value
                  .map((x) => x.organization_id)
                  .includes(item.organization_id)
              )
                item.is_checked = true;
            });
          }
          let obj = renderTree(
            tbs[0],
            "organization_id",
            "organization_name",
            "Đơn vị"
          );
          temporganizations.value = obj.arrChils;
          temporganizations.value.forEach((element) => {
            expandNode(element);
          });
        } else {
          temporganizations.value = [];
        }
      }
    })
    .catch((error) => {
      if (options.value.loading) options.value.loading = false;
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
const initUser = () => {
  axios
    .post(
      baseURL + "/api/FileMain/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "folder_user_list",
            par: [
              { par: "search", va: options.value.search },
              { par: "user_id", va: store.getters.user.user_id },
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
        listUsers.value = JSON.parse(data)[0];
        if (!isAdd.value) {
          listUsers.value.forEach((item) => {
            if (
              selectedUsers.value.map((x) => x.user_id).includes(item.user_id)
            )
              item.is_checked = true;
          });
          // arr_users_coppy = JSON.parse(JSON.stringify(listUsers.value));
          // let arr =  listUsers.value.filter((x) => x.is_checked);
          // if(arr.length > 0)
          // listUsers.value = arr;
        }
      }
    })
    .catch((error) => {
      if (options.value.loading) options.value.loading = false;
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};
const renderTreeFolder = (data, id, name) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
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
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const RenderFolder = (ListTaskCategory) => {
  let List = [];
  ListTaskCategory.value
    .filter((c) => c.data.parent_id == null)
    .forEach((element, i) => {
      sttCate.value = sttCate.value + i + 1;

      let Cat = {
        key: element.data.folder_id,
        data: element.data,
        name: element.data.folder_name,
      };

      const RenderChild = (child, folder_id) => {
        if (!child.children) child.children = [];
        let listChilCate = ListTaskCategory.value.filter(
          (c) => c.data.parent_id == folder_id
        );
        listChilCate.forEach((element) => {
          let CatChild = {
            key: element.data.folder_id,
            data: element.data,
            name: element.data.folder_name,
          };
          if (!CatChild.children) CatChild.children = [];
          RenderChild(CatChild, element.data.folder_id);
          child.children.push(CatChild);
        });
      };
      RenderChild(Cat, element.data.folder_id);
      List.push(Cat);
    });
  TreeData.value = List;
  //active row 1
};
const showEmote = (data) => {
  checkHover.value = { folder_id: data.folder_id, module_key: data.module_key };
};
const itemButMores = ref([
  {
    label: "Thêm folder con",
    icon: "pi pi-plus",
    command: (event) => {
      addChildFolder();
    },
  },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      delFolder();
    },
  },
]);
const toggleMores = (event, u) => {
  dataAddChildFolder.value = u;
  menuButMores.value.toggle(event);
};
const addFolder = () => {
  selectedKey.value = {};
  selectedUsers.value = [];
  datalists.value = [];
  submitted.value = false;
  isAdd.value = true;
  folder.value = {
    organization_id: store.state.user.organization_id,
    status: true,
    is_public: false,
    is_order: options.value.totalRecords,
    capacity: 50,
    type_share: 0,
  };
};
const addChildFolder = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  typeTab.value = 2;
  selectedKey.value = {};
  selectCapcha.value = {};
  selectCapcha.value[dataAddChildFolder.value.key] = true;
  submitted.value = false;
  isAdd.value = true;
  folder.value = {
    organization_id: store.state.user.organization_id,
    status: true,
    is_public: dataAddChildFolder.value.data.is_public,
    is_order: options.value.totalRecords,
    capacity: 50,
    type_share: dataAddChildFolder.value.data.type_share,
  };
  swal.close();
};
const delFolder = () => {
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
          .delete(baseURL + "/api/FileFolder/Del_Folder", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: [dataAddChildFolder.value.key],
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá thư mục thành công!");
              loadTudien();
              if (!md) selectedNodes.value = [];
            } else {
              swal.fire({
                title: "Thông báo!",
                text: "Xóa không thành công, vui lòng thử lại",
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
const changeShare = (type) => {
  type_share.value = type;
  if (type == 2) {
    initOrganization();
  }
  if (type == 3) {
    initUser();
  }
};
const onSelect = (event) => {
  let obj = tbs[0]
    .map((a) => ({
      organization_id: a.organization_id,
      organization_name: a.organization_name,
      is_read: false,
      is_edit: false,
      is_delete: false,
      type: 2,
    }))
    .filter((x) => x.organization_id == event.key);
  if (obj.length > 0) datalists.value.push(obj[0]);
};
const unSelect = (event) => {
  let idx = datalists.value.findIndex((x) => x.organization_id == event.key);
  if (idx != -1) datalists.value.splice(idx, 1);
};
function removeChecked(data, id) {
  data.forEach((item) => {
    if (item.data.organization_id == id) {
      item.data.is_checked = false;
    }
    if (item.children && item.children.length > 0) {
      removeChecked(item.children, id);
    }
  });
}
const delRowUser = (id, type) => {
  if (type == 2) {
    if (temporganizations.value.length > 0)
      removeChecked(temporganizations.value, id);
    let idx = datalists.value.findIndex((x) => x.organization_id == id);
    if (idx != -1) datalists.value.splice(idx, 1);
  } else if (type == 3) {
    listUsers.value.filter((x) => x.user_id == id)[0].is_checked = false;
    let idx = selectedUsers.value.findIndex((x) => x.user_id == id);
    if (idx != -1) selectedUsers.value.splice(idx, 1);
  }
};
const onCheckBox = (u, type) => {
  if (type == 2) {
    if (datalists.value.length == 0)
      datalists.value = tbs[0]
        .filter((x) => x.is_checked && x.organization_id != u.organization_id)
        .map((a) => ({
          organization_id: a.organization_id,
          organization_name: a.organization_name,
          is_read: true,
          is_edit: true,
          is_delete: true,
          type: 2,
        }));
    if (u.is_checked) {
      datalists.value.push({
        organization_id: u.organization_id,
        organization_name: u.organization_name,
        is_read: true,
        is_edit: true,
        is_delete: true,
        type: 2,
      });
    } else {
      let idx = datalists.value.findIndex(
        (x) => x.organization_id == u.organization_id
      );
      if (idx != -1) datalists.value.splice(idx, 1);
    }
  } else if (type == 3) {
    if (selectedUsers.value.length == 0)
      selectedUsers.value = listUsers.value
        .filter((x) => x.is_checked && x.user_id != u.user_id)
        .map((a) => ({
          user_id: a.user_id,
          full_name: a.full_name,
          is_read: true,
          is_edit: true,
          is_delete: true,
          type: 3,
        }));
    if (u.is_checked) {
      selectedUsers.value.push({
        user_id: u.user_id,
        full_name: u.full_name,
        is_read: true,
        is_edit: true,
        is_delete: true,
        type: 3,
      });
    } else {
      let idx = selectedUsers.value.findIndex((x) => x.user_id == u.user_id);
      if (idx != -1) selectedUsers.value.splice(idx, 1);
    }
  }
};
const activeTab = (type) => {
  typeTab.value = type;
};
const onChangeType = (type) => {
  ListFolderShare.value = [];
  if (type == 1) {
    let obj = {
      type: type,
      is_read: true,
      is_edit: true,
      is_delete: true,
      is_order: 1,
    };
    ListFolderShare.value[0] = obj;
  } else if (type == 2) {
    initOrganization();
    initUser();
  }
};
const AddUser = () => {
  listUsers.value = arr_users_coppy;
};
const onRefersh = () => {
  loadTudien();
  // expandedKeysFolder.value["560E01D05C1641C1925552B9F762A6A9"] = true;
};
const changeTypePublic = () => {
  if (folder.value.is_public) {
    folder.value.type_share = 1;
    onChangeType(folder.value.type_share);
  }
};

onMounted(() => {
  //LoadData();
  loadTudien();
  //addFolder();
});
</script>
<template>
  <div>
    <div class="w-full">
      <Splitter class="w-full">
        <SplitterPanel :size="20">
          <div style="height: calc(100vh - 68px)">
            <TreeTable
              :value="TreeData"
              :rowHover="true"
              @nodeSelect="loadFolder"
              @nodeUnselect="unNodeUnselect"
              selectionMode="single"
              v-model:selectionKeys="selectedKey"
              class="h-full w-full overflow-x-hidden p-0"
              scrollHeight="flex"
              responsiveLayout="scroll"
              :scrollable="true"
              :expandedKeys="expandedKeysFolder"
            >
              <Column
                field="name"
                :expander="true"
                class="cursor-pointer flex"
                style="min-height: 50px"
              >
                <template #header>
                  <Toolbar class="w-full p-0 border-none sticky top-0">
                    <template #start>
                      <div class="font-bold text-xl">Danh mục kho dữ liệu</div>
                    </template>
                  </Toolbar>
                </template>

                <template #body="data">
                  <div class="relative flex w-full p-0" v-if="data.node.data">
                    <div class="grid w-full p-0">
                      <div
                        class="field col-12 md:col-12 w-full flex m-0 p-0 pt-2"
                        @mouseover="showEmote(data.node.data)"
                      >
                        <div class="col-2 p-0">
                          <img
                            src="../../assets/image/folder.png"
                            width="28"
                            height="36"
                            style="object-fit: contain"
                          />
                        </div>
                        <div class="col-9 p-0 flex" style="align-items: center">
                          <div class="px-2" style="line-height: 20px">
                            {{ data.node.name }}
                            <span v-if="data.node.children.length > 0"
                              >({{ data.node.children.length }})</span
                            >
                          </div>
                        </div>
                        <div
                          class="col-1 flex format-center"
                          v-if="
                            checkHover.folder_id == data.node.key &&
                            checkHover.module_key == null
                          "
                        >
                          <div
                            @click="toggleMores($event, data.node)"
                            aria-haspopup="true"
                            aria-controls="overlay_More"
                          >
                            <i class="pi pi-ellipsis-h"></i>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </template>
              </Column>
            </TreeTable>
          </div>
        </SplitterPanel>
        <SplitterPanel :size="70">
          <div>
            <Toolbar class="w-full">
              <template #start>
                <h3 class="m-0" v-if="folder">
                  <i class="pi pi-book"></i> Thư mục
                </h3>
              </template>

              <template #end>
                <Button
                  label="Thêm thư mục"
                  icon="pi pi-plus"
                  class="mr-2"
                  @click="addFolder"
                />
                <!-- v-if="folder.folder_id !== 'me' && folder.folder_id !== 'all'" -->
                <Button
                  label="Cập nhật"
                  icon="pi pi-save"
                  class="mr-2"
                  @click="saveFolder(!v$.$invalid)"
                />
                <Button
                  class="mr-2 p-button-outlined p-button-secondary"
                  icon="pi pi-refresh"
                  @click="onRefersh"
                />
              </template>
            </Toolbar>
          </div>
          <div class="col-12" v-if="folder">
            <form
              @submit.prevent="handleSubmit(!v$.$invalid)"
              name="submitform"
            >
              <div
                class="grid formgrid m-2"
                style="max-height: calc(100vh - 114px); overflow: scroll"
              >
                <div class="field col-12 md:col-12">
                  <label class="col-2 text-left"
                    >Tên thư mục <span class="redsao">(*)</span></label
                  >
                  <InputText
                    spellcheck="false"
                    class="col-10 ip36"
                    v-model="folder.folder_name"
                    :class="{
                      'p-invalid': v$.folder_name.$invalid && submitted,
                    }"
                  />
                </div>
                <small
                  v-if="
                    (v$.folder_name.required.$invalid && submitted) ||
                    v$.folder_name.required.$pending.$response
                  "
                  class="col-10 p-error"
                >
                  <div class="field col-12 md:col-12">
                    <label class="col-2 text-left"></label>
                    <span class="col-10 p-0">{{
                      v$.folder_name.required.$message
                        .replace("Value", "Tên thư mục")
                        .replace("is required", "không được để trống")
                    }}</span>
                  </div>
                </small>
                <small
                  v-if="v$.folder_name.maxLength.$invalid && submitted"
                  class="col-12 p-error"
                >
                  <div class="field col-12 md:col-12">
                    <label class="col-2"></label>
                    <span class="col-10 p-0"
                      >{{
                        v$.folder_name.maxLength.$message.replace(
                          "The maximum length allowed is",
                          "Tên thư mục không được vượt quá"
                        )
                      }}
                      ký tự</span
                    >
                  </div>
                </small>
                <div class="field col-12 md:col-12">
                  <label class="col-2 text-left">Cấp cha</label>
                  <TreeSelect
                    class="col-10"
                    v-model="selectCapcha"
                    :options="treefolders"
                    :showClear="true"
                    placeholder=""
                    optionLabel="data.folder_name"
                    optionValue="data.folder_id"
                    :disabled="!isAdd"
                  >
                  </TreeSelect>
                </div>
                <div class="field col-12 md:col-12">
                  <label class="text-left col-2">Từ khóa </label>
                  <Chips
                    class="col-10 p-0"
                    v-model="folder.keywords"
                    placeholder="Nhấn Enter để thêm"
                  />
                </div>
                <div class="field col-12 md:col-12 flex">
                  <label class="col-2 text-left">Mô tả</label>
                  <Textarea
                    style="border-radius: 5px; padding: 0.5rem"
                    class="col-10"
                    spellcheck="false"
                    :autoResize="true"
                    rows="3"
                    v-model="folder.des"
                  />
                </div>
                <div class="field col-12 md:col-12 relative">
                  <label class="col-2">
                    <span class="col-2 text-left absolute top-0"
                      >Dung lượng lưu trữ (MB)(Nhập -1 để không giới hạn)</span
                    >
                  </label>
                  <InputNumber
                    class="col-1 ip36 p-0"
                    v-model="folder.capacity"
                  />
                  <label class="col-2 text-right">STT</label>
                  <InputNumber
                    class="col-1 ip36 p-0"
                    v-model="folder.is_order"
                  />

                  <label class="col-2 text-right">Trạng thái</label>
                  <InputSwitch v-model="folder.status" />
                  <!-- <label class="col-2 text-right">Công khai</label>
                  <InputSwitch
                    v-model="folder.is_public"
                    @change="changeTypePublic"
                  /> -->
                </div>
                <div class="col-12">
                  <h3><i class="pi pi-key"></i> Phân quyền sử dụng</h3>
                </div>
                <div class="field col-12 md:col-12">
                  <label class="text-left col-2">Chọn đối tượng chia sẻ</label>
                  <Dropdown
                    class="col-10 p-0"
                    v-model="folder.type_share"
                    :options="listShare"
                    :max-height="200"
                    placeholder=""
                    optionLabel="label"
                    optionValue="value"
                    @change="onChangeType(folder.type_share)"
                  >
                  </Dropdown>
                </div>
                <div
                  v-if="folder.type_share == 1"
                  class="field col-12 md:col-12 flex pt-1"
                >
                  <label class="col-3 text-right mt-1">Đọc: </label>
                  <Checkbox
                    v-model="ListFolderShare[0].is_read"
                    :binary="true"
                  />
                  <label class="col-3 text-right mt-1">Ghi: </label>
                  <Checkbox
                    v-model="ListFolderShare[0].is_edit"
                    :binary="true"
                  />
                  <label class="col-3 text-right mt-1">Xóa: </label>
                  <Checkbox
                    v-model="ListFolderShare[0].is_delete"
                    :binary="true"
                  />
                </div>
                <div
                  class="field col-12 md:col-12 p-0"
                  v-if="folder.type_share == 2"
                >
                  <div style="border-right: 1px solid #efefef">
                    <ul
                      class="list-group ul-ktra"
                      style="flex-direction: row; justify-content: space-around"
                    >
                      <li
                        class="list-group-item"
                        :class="typeTab == 2 ? 'active_li' : ''"
                        @click="activeTab(2)"
                      >
                        <i class="pi pi-users mr-2"></i>
                        <span>Phòng ban</span>
                      </li>
                      <li
                        class="list-group-item"
                        :class="typeTab == 3 ? 'active_li' : ''"
                        @click="activeTab(3)"
                      >
                        <i class="pi pi-user mr-2"></i>
                        <span>Cá nhân</span>
                      </li>
                    </ul>
                  </div>
                </div>

                <div
                  v-if="folder.type_share == 2 && typeTab == 2"
                  class="field col-12 md:col-12 flex p-0"
                >
                  <div class="col-5 p-0">
                    <TreeTable
                      :value="temporganizations"
                      :scrollable="true"
                      :rowHover="true"
                      :expandedKeys="expandedKeys"
                      :lazy="true"
                      dataKey="organization_id"
                      filterMode="strict"
                      scrollHeight="flex"
                      filterDisplay="menu"
                      class="d-lang-table"
                    >
                      <Column
                        field="organization_name"
                        header="Chọn phòng ban"
                        :expander="true"
                      >
                        <template #body="slotProps">
                          <div
                            :class="
                              slotProps.node.data.organization_id ===
                                options.filter_organization_id ||
                              (options.filter_organization_id == null &&
                                slotProps.node.data.parent_id == null)
                                ? 'row-active'
                                : ''
                            "
                            class="py-1"
                            style="width: -webkit-fill-available"
                          >
                            <Checkbox
                              class="mr-1"
                              :binary="true"
                              v-model="slotProps.node.data.is_checked"
                              @change="onCheckBox(slotProps.node.data, 2)"
                            />
                            <span>{{
                              slotProps.node.data.organization_name
                            }}</span>
                          </div>
                        </template>
                      </Column>
                    </TreeTable>
                  </div>
                  <div class="col-7 md:col-7">
                    <table class="w-full" style="overflow-y: scroll">
                      <thead>
                        <tr style="background-color: #f8f9fa; height: 40px">
                          <th>STT</th>
                          <th>Tên phòng ban</th>
                          <th>Đọc</th>
                          <th>Ghi</th>
                          <th>Xóa</th>
                          <th></th>
                        </tr>
                      </thead>
                      <tbody class="box-right">
                        <tr
                          style="vertical-align: top"
                          v-for="(item, index) in datalists"
                          :key="index"
                        >
                          <td class="text-center">{{ index + 1 }}</td>
                          <td class="text-center">
                            {{ item.organization_name }}
                          </td>
                          <td class="text-center">
                            <Checkbox
                              id="IsIdentity"
                              v-model="item.is_read"
                              :binary="true"
                              @change="clickDelFile"
                            />
                          </td>
                          <td class="text-center">
                            <Checkbox
                              id="IsIdentity"
                              v-model="item.is_edit"
                              :binary="true"
                              @change="clickDelFile"
                            />
                          </td>
                          <td class="text-center">
                            <Checkbox
                              id="IsIdentity"
                              v-model="item.is_delete"
                              :binary="true"
                              @change="clickDelFile"
                            />
                          </td>
                          <td class="text-center">
                            <i
                              class="pi pi-trash icon-delete"
                              v-tooltip.top="'Xóa'"
                              @click="delRowUser(item.organization_id, 2)"
                            ></i>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>
                <div
                  v-if="folder.type_share == 2 && typeTab == 3"
                  class="field col-12 md:col-12 flex p-0"
                >
                  <div class="col-5 p-0 tab-users">
                    <DataTable
                      class="w-full p-datatable-sm e-sm"
                      :value="
                        listUsers.filter((x) =>
                          x.full_name
                            .toUpperCase()
                            .includes(options.search.toUpperCase())
                        )
                      "
                      dataKey="user_id"
                      :showGridlines="true"
                      :rowHover="true"
                      currentPageReportTemplate=""
                      responsiveLayout="scroll"
                      :scrollable="true"
                      scrollHeight="flex"
                      rowGroupMode="subheader"
                      groupRowsBy="organization_name"
                      :lazy="true"
                      :loading="options.loading"
                      :paginator="false"
                      @filter="onFilter($event)"
                      v-model:first="first"
                    >
                      <!-- <template #header v-if="listUsers.filter(x => x.is_checked).length>0">
                        <Toolbar class="w-full custoolbar">
                          <template #end>
                            <Button
                              label="Thêm nhân sự"
                              icon="pi pi-plus"
                              class="mr-2"
                              @click="AddUser"
                            />
                          </template>
                        </Toolbar>
                      </template> -->
                      <template #header>
                        <Toolbar class="w-full custoolbar">
                          <template #start>
                            <span class="p-input-icon-left" style="width: 100%">
                              <i class="pi pi-search" />
                              <InputText
                                v-model="options.search"
                                type="text"
                                spellcheck="false"
                                placeholder=" Tìm kiếm người dùng"
                                style="width: 100%"
                              />
                            </span>
                          </template>
                        </Toolbar>
                      </template>
                      <template #groupheader="slotProps">
                        <i class="pi pi-building mr-2"></i>
                        {{ slotProps.data.organization_name }}
                      </template>
                      <Column
                        class="
                          align-items-center
                          justify-content-center
                          text-center
                        "
                        headerStyle="text-align:center;max-width:70px;height:40px"
                        bodyStyle="text-align:center;max-width:70px"
                        field=""
                        header=""
                      >
                        <template #body="md">
                          <Checkbox
                            :binary="true"
                            v-model="md.data.is_checked"
                            @change="onCheckBox(md.data, 3)"
                          />
                        </template>
                      </Column>
                      <Column
                        headerStyle="text-align:center;height:40px;"
                        bodyStyle="text-align:left;"
                        field="full_name"
                        header="Họ tên nhân sự"
                      >
                        <template #body="data">
                          <div>
                            {{ data.data.full_name }}
                          </div>
                        </template>
                        <template #filter="{ filterModel }">
                          <InputText
                            type="text"
                            v-model="filterModel.value"
                            class="p-column-filter"
                            placeholder="Tài khoản"
                          />
                        </template>
                      </Column>
                      <Column
                        class="
                          align-items-center
                          justify-content-center
                          text-center
                        "
                        headerStyle="text-align:center;max-width:150px"
                        bodyStyle="text-align:center;max-width:150px"
                        field="position_name"
                        header="Chức vụ"
                      >
                        <template #body="data">
                          <div>
                            {{ data.data.position_name }}
                          </div>
                        </template>
                      </Column>
                    </DataTable>
                  </div>
                  <div class="col-7 md:col-7">
                    <table class="w-full" style="overflow-y: scroll">
                      <thead>
                        <tr style="background-color: #f8f9fa; height: 40px">
                          <th>STT</th>
                          <th>Họ tên</th>
                          <th>Đọc</th>
                          <th>Ghi</th>
                          <th>Xóa</th>
                          <th></th>
                        </tr>
                      </thead>
                      <tbody class="box-right">
                        <tr
                          style="vertical-align: top"
                          v-for="(item, index) in selectedUsers"
                          :key="index"
                        >
                          <td class="text-center">{{ index + 1 }}</td>
                          <td class="text-center">
                            {{ item.full_name }}
                          </td>
                          <td class="text-center">
                            <Checkbox
                              id="IsIdentity"
                              v-model="item.is_read"
                              :binary="true"
                              @change="clickDelFile"
                            />
                          </td>
                          <td class="text-center">
                            <Checkbox
                              id="IsIdentity"
                              v-model="item.is_edit"
                              :binary="true"
                              @change="clickDelFile"
                            />
                          </td>
                          <td class="text-center">
                            <Checkbox
                              id="IsIdentity"
                              v-model="item.is_delete"
                              :binary="true"
                              @change="clickDelFile"
                            />
                          </td>
                          <td class="text-center">
                            <i
                              class="pi pi-trash icon-delete"
                              v-tooltip.top="'Xóa'"
                              @click="delRowUser(item.user_id, 3)"
                            ></i>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
            </form>
          </div>
          <div v-else>
            <div
              class="align-items-center justify-content-center p-4 text-center"
            >
              <img src="../../assets/background/nodata.png" height="144" />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </div>
        </SplitterPanel>
      </Splitter>
    </div>
    <div></div>
  </div>
  <Menu
    id="overlay_More"
    ref="menuButMores"
    :model="itemButMores"
    :popup="true"
  />
</template>
<style scoped>
.p-toolbar {
  border-radius: none;
  height: 50px !important;
}

.box-right > tr > td {
  /* border: 1px solid #e9ecef;
  border-width: 0 0 1px 0; */
  padding: 0.5rem 0.5rem;
  cursor: pointer;
}

.box-right > tr:hover {
  background-color: #e9ecef;
}

.icon-delete:hover {
  color: red;
}

.list-group {
  list-style: none;
  padding-left: 0px;
  margin-bottom: 0px;
  cursor: pointer;
  display: flex;
}

.ul-ktra .list-group-item.active_li {
  border-bottom: none;
  background-color: rgb(255, 255, 255);
  border-top: 4px solid rgb(0, 168, 255);
  border-left: 1px solid rgb(204, 204, 204);
  border-right: 1px solid rgb(204, 204, 204);
}

.ul-ktra .list-group-item {
  width: 100%;
  border-right: none;
  border-left: none;
  border-image: initial;
  text-align: center;
  font-weight: bold;
  background-color: rgb(246, 248, 250);
  border-bottom: 1px solid rgb(204, 204, 204);
  border-top: 4px solid transparent;
  border-radius: 0px;
  margin-bottom: 0px;
  color: rgb(108, 122, 134);
  display: flex;
  flex-direction: column;
  font-size: 14px;
  padding: 0.5rem 1.25rem;
  cursor: pointer;
  display: block;
  height: 40px !important;
  padding: 10px;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-treetable) {
  .p-treetable-tbody > tr > td {
    padding: 0;
  }
}

::v-deep(.col-12) {
  .p-inputswitch {
    top: 6px;
  }
}

::v-deep(.tab-users) {
  .p-datatable-header {
    padding: 0 !important;
  }
}
</style>