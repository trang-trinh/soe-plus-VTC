<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../util/function.js";
import ClassicEditor from "@ckeditor/ckeditor5-build-classic";

const cryoptojs = inject("cryptojs");
//init Model
const filters = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
});
const module = ref({
  module_name: "",
  is_order: 1,
  status: true,
  is_admin: true,
  is_target: "_self",
});
//Valid Form
const submitted = ref(false);
const rules = {
  module_name: {
    required,
  },
};
const v$ = useVuelidate(rules, module);
//Khai báo biến
const checkEditMenuName = ref();
const menuNameNew = ref();
const menuID = ref();
const isAdd = ref(true);
const editor = ref(ClassicEditor);
const editorConfig = ref({
  fontSize: {
    options: [8, 10, 12, 14, 16, 18, 20, 22, 24, 28, 32, 36, 40, 44, 48, 56],
  },
  toolbar: {
    items: [
      "heading",
      "bold",
      "italic",
      "underline",
      "Link",
      "|",
      "fontSize",
      "fontColor",
      "fontBackgroundColor",
      "fontFamily",

      "highlight",
      "|",
      "alignment",
      "bulletedList",
      "numberedList",
      "|",

      "insertImage",
      "mediaEmbed",
      "horizontalLine",
      "|",
      "insertTable",
      "tableColumn",
      "tableRow",
      "mergeTableCells",
      "|",

      "imageStyle:inline",
      "imageStyle:block",
      "imageStyle:side",
      "toggleImageCaption",
      "imageTextAlternative",
      "|",

      "strikethrough",
      "outdent",
      "indent",
      "|",
      "codeBlock",
      "linkImage",
      "blockQuote",
      "code",
      "subscript",
      "superscript",
      "|",
      "undo",
      "redo",
      "findAndReplace",
    ],

    shouldNotGroupWhenFull: true,
  },

  removePlugins: ["MediaEmbedToolbar"],
});
const store = inject("store");
const datalists = ref([]);
const selectCapcha = ref();
const selectedKey = ref();
const expandedKeys = ref();
const selectedNodes = ref([]);
const help_data = ref({});
const modules = ref();
const isFirst = ref(true);
let files = [];
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
      exportModule("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportModule("ExportExcelMau");
    },
  },
]);
//Khai báo function
const swalLoadding = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
};
const errorMessage = () => {
  swal.fire({
    text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
    confirmButtonText: "OK",
  });
};
const swalMessage = (title, icon, ms) => {
  swal.fire({
    title: title,
    text: ms,
    icon: icon,
    confirmButtonText: "OK",
  });
};
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const onNodeSelect = (node) => {
  loadDataDetail(node.key);
  //refeshCol(node);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(
    selectedNodes.value.indexOf(node.data.module_id),
    1
  );
};
// get order by parent
const onChangeParent = (item) => {
  const module_id = parseInt(Object.keys(item)[0]);
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(JSON.stringify({}), SecretKey, cryoptojs).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        module.value.is_order = data[0][0].c + 1;
      }
    });
};
//Thêm sửa xoá

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
            em.label_order = mm.data.label_order + "." + (index + 1);
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
const loadTudien = (f) => {
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "help_title_list",
            par: [
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
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        if(f) loadDataDetail(data[0].help_title_id);
        let obj = renderTree(
          data,
          "help_title_id",
          "title_name",
          "thư mục"
        );
        datalists.value = obj.arrChils;
        treemenus.value = obj.arrtreeChils;
      }
    })
    .catch((error) => { });
};
const loadDataDetail = (id) => {
  selectedKey.value = {};
  selectedKey.value[id] = true;
  menuID.value = id;
  help_data.value.help_title_id = id;
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "help_title_get_content",
            par: [
              { par: "help_title_id", va: id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        help_data.value = data[0];
        isAdd.value = false;
        let contentFake = help_data.value.content;
        setTimeout(() => {
          help_data.value.content = contentFake;
        }, "100");
      }
      else {
        help_data.value.content = "";
        isAdd.value = true;
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");

      options.value.loading = false;
    });
}
const saveContent = () => {
  let formData = new FormData();
  formData.append("model", JSON.stringify(help_data.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: isAdd.value == false ? "put" : "post",
    url:
      baseURL +
      `/api/Help/${isAdd.value == false ? "Update_Content" : "Add_Content"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật nội dung thành công!");
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
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
}
const editMenuName = (data) => {
  menuNameNew.value = data.data.title_name;
  checkEditMenuName.value = data.key;
}
const cancelEditMenuName = () => {
  menuNameNew.value = null;
  checkEditMenuName.value = 0;
};
const treemenus = ref();
const displayAddTreeMenu = ref(false);
const headerMenu = ref();
const menu = ref({});
const isAddMenu = ref(false);
const saveMenuName = (data)=>{
  menu.value = data;
  menu.value.title_name = menuNameNew.value;
  let formData = new FormData();
  formData.append("model", JSON.stringify(menu.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method:"put",
    url:
      baseURL +
      `/api/Help/Update_Menu`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        cancelEditMenuName();
        loadTudien();
        toast.success("Cập nhật tên menu thành công!");
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
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
}
const addTreeMenu = (id) => {
  isAddMenu.value = true;
  selectCapcha.value = {};
  if(id != null) selectCapcha.value[id] = true;
  headerMenu.value = "Thêm menu";

  menu.value = {
    title_name: null,
    is_order: 1,
    parent_id: null,
  }
  getOrderChild(id);
  displayAddTreeMenu.value = true;
}
const getOrderChild = (id) => {
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "help_title_get_order",
            par: [
              { par: "help_title_id", va: id },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        menu.value.is_order = data[0].count_stt + 1;
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
    });
}
const editMenu = (data) => {
  selectCapcha.value = {};
  selectCapcha.value[data.parent_id || -1] = true;
  headerMenu.value = "Sửa menu";
  isAddMenu.value = false;
  menu.value = data;
  displayAddTreeMenu.value = true;
}
const saveMenu = () => {
  if (selectCapcha.value) {
    let keys = Object.keys(selectCapcha.value);
    menu.value.parent_id = keys[0];
  }
  if (menu.value.parent_id == -1) menu.value.parent_id = null;
  let formData = new FormData();
  formData.append("model", JSON.stringify(menu.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: isAddMenu.value == false ? "put" : "post",
    url:
      baseURL +
      `/api/Help/${isAddMenu.value == false ? "Update_Menu" : "Add_Menu"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        loadTudien();
        toast.success("Cập nhật menu thành công!");
        displayAddTreeMenu.value = false;
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
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
}
const delMenu = (data) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá menu này không!",
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
          .delete(baseURL + "/api/Help/Del_Menu", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: [data.help_title_id],
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              loadTudien();
              swal.close();
              toast.success("Xoá menu thành công!");
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
onMounted(() => {
  //init
  loadTudien(true);
  return {};
});
</script>
<template>
  <div>
    <Splitter class="w-full">
      <SplitterPanel :size="27" style="min-width: 150px">
        <Toolbar class="w-full py-3">
          <template #start>
            <h2>Mục lục</h2>
            <Button @click="addTreeMenu(null)" class="
    													p-button-rounded
    													p-button-secondary
    													p-button-outlined
    													ml-3
    												" type="button" icon="pi pi-plus" style="width:2rem; height:2rem;"
                    v-tooltip.top="'Thêm menu'"></Button>
          </template>
          <template #end>
          </template>
        </Toolbar>
        <Tree style="max-height: calc(100vh - 90px);min-height: calc(100vh - 90px); overflow:auto;border:none" :value="datalists" @nodeSelect="onNodeSelect"
          @nodeUnselect="onNodeUnselect" selectionMode="single" :metaKeySelection="false"
          v-model:selectionKeys="selectedKey" class="w-full overflow-x-hidden p-0" scrollHeight="flex"
          responsiveLayout="scroll" :scrollable="true" :expandedKeys="expandedKeys">
          <template #default="slotProps">
            <div class="flex" v-on:dblclick="editMenuName(slotProps.node)">
              <div class="country-item flex w-full pt-2" style="padding: 3px 0 !important;align-items: center;">
                <img src="../../assets/image/folder.png" width="22" height="30" style="object-fit: contain" />
                <div v-if="checkEditMenuName != slotProps.node.key" style="width:inherit">
                  <div class="px-2 text-lg font-bold" :class="slotProps.node.data.parent_id?'font-italic':''" style="line-height: 20px;">
                   {{slotProps.node.data.label_order}} {{slotProps.node.data.title_name }} 
                  </div>
                </div>
                <div v-else class="flex w-inherit">
                  <InputText @keyup.enter="saveMenuName(slotProps.node.data)" v-model="menuNameNew" spellcheck="false"
                    autofocus="true" class="m-2 w-inherit"></InputText>
                  <Button class="
    														p-button-rounded
    														p-button-secondary
    														p-button-outlined
    														m-2
    													" style="width:2rem; height:2rem;" type="button" icon="pi pi-times"
                    @click="cancelEditMenuName()"></Button>
                </div>
                <div class="w-3 pt-1 flex" style="padding-top:0.1rem !important;"
                  v-if="menuID == slotProps.node.key && checkEditMenuName != slotProps.node.key">
                  <Button @click="addTreeMenu(slotProps.node.key)" class="
    													p-button-rounded
    													p-button-secondary
    													p-button-outlined
    													mx-1
    												" type="button" icon="pi pi-plus-circle" style="width:1.7rem; height:1.7rem;"
                    v-tooltip.top="'Thêm menu con'"></Button>
                  <Button @click="editMenu(slotProps.node.data)" class="
    													p-button-rounded
    													p-button-secondary
    													p-button-outlined
    													mx-1
    												" type="button" icon="pi pi-pencil" style="width:1.7rem; height:1.7rem;"
                    v-tooltip.top="'Sửa menu'"></Button>
                  <Button @click="delMenu(slotProps.node.data)" class="
    													p-button-rounded
    													p-button-secondary
    													p-button-outlined
    													mx-1
    												" type="button" icon="pi pi-trash" style="width:1.7rem; height:1.7rem;" v-tooltip.top="'Xóa'"></Button>
                </div>
              </div>
            </div>
          </template>
        </Tree>

      </SplitterPanel>
      <SplitterPanel :size="73">
        <div class="w-full h-full ck-helper ck-content">
          <div class="m-2 text-right">
            <Button label="Cập nhật" icon="pi pi-save" @click="saveContent()" />
          </div>
          <ckeditor :editor="editor" :config="editorConfig" v-model="help_data.content"></ckeditor>
        </div>
      </SplitterPanel>
    </Splitter>

  </div>
  <Dialog :header="headerMenu" v-model:visible="displayAddTreeMenu" :style="{ width: '40vw' }">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0">Tên Menu </label>
          <InputText v-model="menu.title_name" spellcheck="false" class="col-10 ip34 px-2" />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0">Menu cấp cha</label>
          <TreeSelect class="col-10" spellcheck="false" v-model="selectCapcha" :options="treemenus" />
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0">STT </label>
          <InputNumber v-model="menu.is_order" class="col-2 ip36 p-0" />
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="displayAddTreeMenu = false" class="p-button-text" />

      <Button label="Lưu" icon="pi pi-check" @click="saveMenu()" />
    </template>
  </Dialog>
</template>
<style scoped>
.ck-editor__editable {
  min-height: 800px !important;
  max-height: 800px !important;
}

.col-12 .p-inputswitch {
  top: 6px;
}

.ipnone {
  display: none;
}

.w-inherit {
  width: inherit
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
<style>
.text-error {
  color: red !important;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-splitter) {
  .p-toolbar {
    background: none !important;
    border: none !important;
  }

  .p-treetable-wrapper {
    margin-top: 6px;
  }

  .p-treetable .p-treetable-thead>tr>th {
    background: #fff !important;
  }
}

::v-deep(.p-tree-wrapper) {
  .p-treenode-label {
    width: 100%;
  }
}
</style>