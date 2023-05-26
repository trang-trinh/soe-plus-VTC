<script setup>
import { ref, inject, onMounted, watch, onUpdated } from "vue";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../util/function.js";
import ClassicEditor from "@ckeditor/ckeditor5-build-classic";
const router = inject("router");
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
const idNewsLoaded = ref(
  window.location.href.substring(
    window.location.href.lastIndexOf("orient-") + 7
  )
);
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
const selectCapcha = ref({});
const selectedKey = ref();
const expandedKeys = ref({});
const selectedNodes = ref([]);
const help_data = ref({});

const isFirst = ref(true);

const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const menuButs = ref();

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
const onNodeSelect = (data) => {
  let srcMs = removeVietnameseTones(data.data.title_name);

  if (router)
    router.push({
      path:
        "/helperview/" +
        srcMs.replace(/','|'.'/g, "").replace(/\s+/g, "-") +
        "-orient-" +
        data.key,
    });
  //   loadDataDetail(data.key);

  //refeshCol(node);
};
function removeVietnameseTones(str) {
  str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
  str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
  str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
  str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
  str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
  str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
  str = str.replace(/đ/g, "d");
  str = str.replace(/À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ|Ặ|Ẳ|Ẵ/g, "A");
  str = str.replace(/È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ/g, "E");
  str = str.replace(/Ì|Í|Ị|Ỉ|Ĩ/g, "I");
  str = str.replace(/Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ|Ợ|Ở|Ỡ/g, "O");
  str = str.replace(/Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ/g, "U");
  str = str.replace(/Ỳ|Ý|Ỵ|Ỷ|Ỹ/g, "Y");
  str = str.replace(/Đ/g, "D");
  // Some system encode vietnamese combining accent as individual utf-8 characters
  // Một vài bộ encode coi các dấu mũ, dấu chữ như một kí tự riêng biệt nên thêm hai dòng này
  str = str.replace(/\u0300|\u0301|\u0303|\u0309|\u0323/g, ""); // ̀ ́ ̃ ̉ ̣  huyền, sắc, ngã, hỏi, nặng
  str = str.replace(/\u02C6|\u0306|\u031B/g, ""); // ˆ ̆ ̛  Â, Ê, Ă, Ơ, Ư
  // Remove extra spaces
  // Bỏ các khoảng trắng liền nhau
  str = str.replace(/ + /g, " ");
  str = str.trim();
  // Remove punctuations
  // Bỏ dấu câu, kí tự đặc biệt
  str = str.replace(
    /!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'|\"|\&|\#|\[|\]|~|\$|_|`|-|{|}|\||\\/g,
    " "
  );
  return str;
}
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
      expandedKeys.value[m[id]] = true;
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em, index) => {
            em.label_order = mm.data.label_order + "." + (index + 1);
            let om1 = { key: em[id], data: em };
            expandedKeys.value[em[id]] = true;
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
        let obj = renderTree(data, "help_title_id", "title_name", "thư mục");
        datalists.value = obj.arrChils;
        treemenus.value = obj.arrtreeChils;
        selectedKey.value = {};
        if (f) loadDataDetail(Number(idNewsLoaded.value));
        selectedKey.value[Number(idNewsLoaded.value)] = true;
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
  help_data.value = null;

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
        isAdd.value = true;
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      console.log(error);
      options.value.loading = false;
    });
};
const divZoom = ref(0.8);
const toogledivZoom = (f) => {
  if (f) {
    divZoom.value = (divZoom.value * 10 + 1) / 10;
  } else {
    divZoom.value = (divZoom.value * 10 - 1) / 10;
  }
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
  selectedKey.value = {};
  selectedKey.value[id] = true;
  isAddMenu.value = true;
  selectCapcha.value = {};
  if (id != null) selectCapcha.value[id] = true;
  headerMenu.value = "Thêm thư mục";
  menu.value = {
    title_name: null,
    is_order: 1,
    parent_id: null,
    status: true,
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
  if (data.parent_id) selectCapcha.value[data.parent_id || -1] = true;
  else selectCapcha.value = {};
  headerMenu.value = "Sửa thư mục";
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
          addTreeMenu(Number(Object.keys(selectCapcha.value)[0]));
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
  idNewsLoaded.value = window.location.href.substring(
    window.location.href.lastIndexOf("orient-") + 7
  );
  loadTudien(true);
  return {};
});
onUpdated(() => {
  idNewsLoaded.value = window.location.href.substring(
    window.location.href.lastIndexOf("orient-") + 7
  );
  loadTudien(true);
  return {};
});
</script>
<template>
  <div>
    <div>
      <Splitter class="w-full">
        <SplitterPanel :size="25" style="min-width: 150px">
          <Toolbar class="w-full p-0">
            <template #start>
              <div class="px-3">
                <h3>Mục lục</h3>
              </div>
            </template>
          </Toolbar>

          <div class="d-lang-table-sl">
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
        </SplitterPanel>
        <SplitterPanel :size="75">
          <div class="w-full ">
            <div >
              <Toolbar class="w-full p-0">
                <template #start>
                  <div class="px-3">
                    <h3>
                      Nội dung
                      <span v-if="help_data">{{ help_data.title_name }}</span>
                    </h3>
                  </div>
                </template>

                <template #end>
                  <div class="mr-4">
                    <Button
                      @click="toogledivZoom(false)"
                      icon="pi pi-search-minus"
                      class="p-button-secondary p-button-outlined ml-1"
                    />
                    <Button
                      @click="divZoom = 1"
                      :label="parseInt(divZoom * 100) + '%'"
                      class="p-button-secondary p-button-outlined ml-1"
                    />
                    <Button
                      @click="toogledivZoom(true)"
                      icon="pi pi-search-plus"
                      class="p-button-secondary p-button-outlined ml-1"
                    />
                  </div>
                </template>
              </Toolbar>

              <div
                class=" w-full d-lang-table-sl-1 "  
                v-if="help_data"
                :style="'zoom:' + divZoom"
                style="overflow:scroll "
              >
                <div class="px-2" v-html="help_data.content"></div>
              </div>

              <div v-else class="w-full">
                <div class="w-full format-center">
                  <img src="../../assets/background/nodata.png" height="144" />
                </div>
                <div class="w-full format-center">
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </div>
            </div>
          </div>
        </SplitterPanel>
      </Splitter>
    </div>
  </div>
</template>
<style scoped>
.ck-editor__editable {
  min-height: calc(100vh - 200px) !important;
  max-height: calc(100vh - 200px) !important;
}
.d-hover {
  display: none;
}

.d-tree-missheader tr:hover .d-hover {
  display: block !important;
}
.d-tree-missheader tr td {
  padding: 0px !important;
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
 
<style lang="scss" scoped>
.d-lang-table-sl {
  margin: 0px;
  height: calc(100vh - 105px);
}

.d-lang-table-sl-1 {
  margin: 0px;
  height: 120vh;
  
  
}
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
</style>