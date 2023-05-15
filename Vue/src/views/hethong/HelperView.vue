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
    height: '800px',
    toolbar: [
        "heading",
        "bold",
        "italic",
        "underline",
        "Link",
        '|', 'fontfamily', 'fontsize', 'fontColor', 'fontBackgroundColor',
        "|",
        "alignment",
        "bulletedList",
        "numberedList",
        "|",
        "insertImage",
        "mediaEmbed",
        "horizontalLine",
        "|",
        "fontColor",
        "fontBackgroundColor",
        "fontFamily",
        "fontSize",
        "highlight",

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
    indentBlock: {
        offset: 1,
        unit: "em",
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
const tdTargets = ref([
    { value: "_blank", text: "Mở sang tab mới" },
    { value: "_self", text: "Mở tab hiện tại" },
]);
const tdSize = ref([
    { value: "480px", text: "Nhỏ (480px)" },
    { value: "720px", text: "Trung bình (720px)" },
    { value: "1024px", text: "Lớn (1024px)" },
    { value: "100%", text: "Full (100%)" },
]);
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
const loadTudien = () => {
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
                loadDataDetail(data[0].help_title_id);
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
const treemenus = ref();

onMounted(() => {
    //init
    loadTudien();
    return {};
});
</script>
<template>
    <div>
        <Splitter class="w-full">
            <SplitterPanel :size="25" style="min-width: 150px">
                <div class="pl-3">
                    <h2>Mục lục</h2>
                </div>
                <Tree style="max-height: calc(100vh - 90px);min-height: calc(100vh - 90px);border:none" :value="datalists" @nodeSelect="onNodeSelect"
                    @nodeUnselect="onNodeUnselect" selectionMode="single" :metaKeySelection="false"
                    v-model:selectionKeys="selectedKey" class="w-full overflow-x-hidden p-0" scrollHeight="flex"
                    responsiveLayout="scroll" :scrollable="true" :expandedKeys="expandedKeys">
                    <template #default="slotProps">
                        <div class="flex" >
                            <div class="country-item flex w-full pt-2"
                                style="padding: 3px 0 !important;align-items: center;">
                                <img src="../../assets/image/folder.png" width="22" height="30"
                                    style="object-fit: contain" />
                                <div v-if="checkEditMenuName != slotProps.node.key" style="width:inherit">
                                    <div class="px-2 text-lg font-bold" :class="slotProps.node.data.parent_id?'font-italic':''" style="line-height: 20px;">
                   {{slotProps.node.data.label_order}} {{slotProps.node.data.title_name }} 
                  </div>
                                </div>
                            </div>
                        </div>
                    </template>
                </Tree>

            </SplitterPanel>
            <SplitterPanel :size="75">
                <div class="w-full h-full ck-content">
                    <p v-if="help_data.content"
                    class="content-view px-3"
                  v-html="help_data.content"
                ></p>
                <div v-else>
                    <img  class="w-full" style="height:calc(100vh - 55px)" :src="basedomainURL + '/Portals/Image/nocontent.png'"/>
                </div>
                </div>
            </SplitterPanel>
        </Splitter>

    </div>
</template>
<style scoped>


.content-view{
    min-height: calc(100vh - 90px) !important;
    max-height: calc(100vh - 90px) !important;
    overflow: auto !important;
    font-size: 14px;
    line-height: 1.5rem
}

.w-inherit {
    width: inherit
}


</style>
<style lang="scss" scoped>

::v-deep(.p-tree-wrapper) {
    .p-treenode-label {
        width: 100%;
    }
}
</style>