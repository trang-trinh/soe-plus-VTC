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
const options = ref({
  IsNext: true,
  sort: "created_date",
  SearchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
  loadingP: true,
  pagenoP: 0,
  pagesizeP: 20,
  searchP: "",
  sortP: "created_date",
});
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

  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};

const loadTudien = (f) => {
  options.value.loading = true;
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "help_title_list",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
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
        if (f) loadDataDetail(data[0].help_title_id);
        let obj = renderTree(data, "help_title_id", "title_name", "thư mục");
        datalists.value = obj.arrChils;
        treemenus.value = obj.arrtreeChils;
      }
      options.value.loading = false;
    })
    .catch((error) => {
      options.value.loading = false;
    });
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
            par: [{ par: "help_title_id", va: id }],
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
      } else {
        help_data.value.content = "";
        isAdd.value = true;
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");

      options.value.loading = false;
    });
};
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
};
const editMenuName = (data) => {
  menuNameNew.value = data.data.title_name;
  checkEditMenuName.value = data.key;
};
const cancelEditMenuName = () => {
  menuNameNew.value = null;
  checkEditMenuName.value = 0;
};
const treemenus = ref();
const displayAddTreeMenu = ref(false);
const headerMenu = ref();
const menu = ref({});
const isAddMenu = ref(false);
const saveMenuName = (data) => {
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
    method: "put",
    url: baseURL + `/api/Help/Update_Menu`,
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
};
const addTreeMenu = (id) => {
  selectedKey.value={};
  selectedKey.value[id]=true;
  isAddMenu.value = true;
  selectCapcha.value = {};
  if (id != null) selectCapcha.value[id] = true;
  headerMenu.value = "Thêm thư mục";
  menu.value = {
    title_name: null,
    is_order: 1,
    parent_id: null,
    status:true
  };
  getOrderChild(id);
  displayAddTreeMenu.value = true;
};
const getOrderChild = (id) => {
  axios
    .post(
      baseURL + "/api/Modules/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "help_title_get_order",
            par: [{ par: "help_title_id", va: id }],
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
};
const editMenu = (data) => {
  selectCapcha.value = {};
  selectCapcha.value[data.parent_id || -1] = true;
  headerMenu.value = "Sửa menu";
  isAddMenu.value = false;
  menu.value = data;
  displayAddTreeMenu.value = true;
};
const saveMenu = (check) => {
  submitted.value = true;
  if (menu.value.title_name == null) {
    return;
  }
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

        if (check) {
          addTreeMenu(null);
        } else {
          displayAddTreeMenu.value = false;
        }
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
};
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
    <div class="p-3">
      <h2 class="module-title m-0">
        <i class="pi pi-file-pdf text-lg"></i> Biên tập hướng dẫn sử dụng
      </h2>
    </div>
    <div>
      <Splitter class="w-full">
        <SplitterPanel :size="25" style="min-width: 150px">
          <Toolbar class="w-full p-0">
            <template #start>
              <div class="px-3">
                <h3>
                  <i class="pi pi-folder" style="font-size: 1.25rem"></i>
                  Danh sách thư mục
                </h3>
              </div>
            </template>
            <template #end>
              <div class="px-3">
                <Button
                  class="p-button-outlined p-button-rounded"
                  icon="pi pi-plus"
                  @click="addTreeMenu(null)"
                  v-tooltip.top="'Thêm thư mục'"
                >
                </Button>
              </div>
            </template>
          </Toolbar>

          <TreeTable
            :value="datalists"
            @nodeSelect="onNodeSelect"
            @nodeUnselect="onNodeUnselect"
            v-model:selectionKeys="selectedKey"
            :loading="options.loading"
            :expandedKeys="expandedKeys"

            :showGridlines="false"
            selectionMode="single"
            filterMode="strict"
            class="p-treetable-sm d-tree-missheader"
            :rows="20"
            :rowHover="true"
            responsiveLayout="scroll"
            :scrollable="true"
            scrollHeight="flex"
            :metaKeySelection="false"
          >
            <Column field="title_name" header="Name" :expander="true">
              <template #body="md">
                <div class="align-items-center flex">
                  <div>
                    <img
                      src="../../assets/image/folder.png"
                      width="20"
                      height="25"
                      style="object-fit: contain"
                    />
                  </div>
                  <div class="pl-2">{{ md.node.data.title_name }}</div>
                </div>
              </template>
            </Column>

            <Column
              header="Chức năng"
              headerClass="text-center"
              class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;max-width:120px"
              bodyStyle="text-align:center;max-width:120px"
            >
              <template #header> </template>
              <template #body="md">
                <div class="d-hover">
                  <Button
                    type="button"
                    icon="pi pi-plus"
                    class="p-button-rounded p-button-secondary p-button-outlined"
                    v-tooltip.top="'Thêm  thư mục con'"
                    @click="addTreeMenu(md.node.key)"
                    style="width: 2rem; height: 2rem; margin-right: 0.5rem"
                  ></Button>
                  <Button
                    type="button"
                    icon="pi pi-pencil"
                    v-tooltip.top="'Chỉnh sửa'"
                    class="p-button-rounded p-button-secondary p-button-outlined"
                    style="width: 2rem; height: 2rem; margin-right: 0.5rem"
                    @click="editMenu(md.node.data)"
                  ></Button>
                  <Button
                    type="button"
                    icon="pi pi-trash"
                    v-tooltip.top="'Xóa'"
                    style="width: 2rem; height: 2rem"
                    class="p-button-rounded p-button-secondary p-button-outlined"
                    @click="delMenu(md.node.data)"
                  ></Button>
                </div>
              </template>
            </Column>
            <template #empty>
              <div
                class="m-auto align-items-center justify-content-center p-4 text-center"
                v-if="!isFirst"
              >
                <img src="../../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </template>
          </TreeTable>
        </SplitterPanel>
        <SplitterPanel :size="75">
          <Toolbar class="w-full p-0">
            <template #start>
              <div class="px-3">
                <h3>Nội dung biên tập</h3>
              </div>
            </template>
            <template #end>
              <div class="px-3">
                <Button
                  label="Cập nhật"
                  icon="pi pi-save"
                  @click="saveContent()"
                />
              </div>
            </template>
          </Toolbar>
          <div class="w-full h-full ck-helper ck-content">
            <ckeditor
              :editor="editor"
              :config="editorConfig"
              v-model="help_data.content"
            ></ckeditor>
          </div>
        </SplitterPanel>
      </Splitter>
    </div>
  </div>
  <Dialog
    :header="headerMenu"
    v-model:visible="displayAddTreeMenu"
    :style="{ width: '40vw' }"
    :closable="true"
    :modal="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 p-0">
          <label class="w-10rem text-left p-0"
            >Tên thư mục <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="menu.title_name"
            spellcheck="false"
            style="width: calc(100% - 10rem)"
            :class="{
              'p-invalid': menu.title_name == null && submitted,
            }"
          />
        </div>
        <div class="col-12 md:col-12 p-0">
          <div class="col-12 field p-0 flex align-items-center">
            <div class="col-6 p-0 flex  align-items-center">
              <div class="w-10rem text-left p-0">Thuộc cấp</div>
              <TreeSelect
                style="width: calc(100% - 10rem)"
                spellcheck="false"
                v-model="selectCapcha"
                :options="treemenus"
              />
            </div>
            <div class="col-6 md:col-6 p-0 align-items-center flex">
              <div class="w-10rem text-left p-0 pl-3">Icon</div>
              <InputText
                v-model="menu.icon"
                spellcheck="false"
                style="width: calc(100% - 10rem)"
              />
            </div>
          </div>

          <div class="col-12 field md:col-12 flex p-0">
            <div class="field col-6 md:col-6 p-0 align-items-center flex">
              <div class="w-10rem text-left p-0">STT</div>
              <InputNumber
                v-model="menu.is_order"
                style="width: calc(100% - 10rem)"
                class="ip36 p-0"
              />
            </div>
            <div class="field col-6 md:col-6 p-0 align-items-center flex">
              <div class="w-10rem text-left p-0 pl-3">Trạng thái</div>
              <InputSwitch v-model="menu.status" class="w-4rem lck-checked" />
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="displayAddTreeMenu = false"
        class="p-button-outlined"
      />

      <Button label="Lưu" class="" icon="pi pi-check" @click="saveMenu(false)" />
      <Button
        label="Lưu và Tiếp tục"
        icon="pi pi-check"
        @click="saveMenu(true)"
      />
    </template>
  </Dialog>
</template>
<style scoped>
.ck-editor__editable {
  min-height: calc(100vh - 200px) !important;
  max-height: calc(100vh - 200px) !important;
}
 .d-hover {
  display: none;
}

.d-tree-missheader tr:hover  .d-hover
 {
  display: block !important;
}
.d-tree-missheader tr td
 {
  padding: 0px !important;
}
.ipnone {
  display: none;
}
.w-inherit {
  width: inherit;
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
// ::v-deep(.p-splitter) {
//   .p-toolbar {
//     background: none !important;
//     border: none !important;
//   }

//   .p-treetable-wrapper {
//     margin-top: 6px;
//   }

//   .p-treetable .p-treetable-thead > tr > th {
//     background: #fff !important;
//   }
// }

// ::v-deep(.p-tree-wrapper) {
//   .p-treenode-label {
//     width: 100%;
//   }
// }
::v-deep(.ck-editor__main) {
  .ck-editor__editable {
    height: calc(100vh - 177px) !important;
  }
}
</style>