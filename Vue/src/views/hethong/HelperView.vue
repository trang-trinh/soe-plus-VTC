<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../util/function.js";
import ClassicEditor from "@ckeditor/ckeditor5-build-classic";

const cryoptojs = inject("cryptojs");
const router = inject("router");
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
    text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
            // rechildren(om1, em[id]);
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

const loadTudien = () => {
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
        loadDataDetail(data[0].help_title_id);
        let obj = renderTree(data, "help_title_id", "title_name", "thư mục");
        datalists.value = obj.arrChils;
        treemenus.value = obj.arrtreeChils;
      }
      options.value.loading = false;
    })
    .catch((error) => {});
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
const treemenus = ref();
const showDetails = (data) => {
 
    let srcMs = removeVietnameseTones(data.data.title_name);

    if (router)
      router.push({
        path:
          "/helperview/" +
          srcMs.replace(/','|'.'/g, "").replace(/\s+/g, "-") +
          "-orient-" +
          data.key,
      });
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
onMounted(() => {
  //init
  loadTudien();
  return {};
});
</script>
<template>
  <div>
    <form>
      <div class="w-full format-center p-0">
        <h2>HƯỚNG DẪN SỬ DỤNG</h2>
      </div>
      <div class="grid formgrid m-2 mt-0 d-lang-table-sl surface-100">
        <div class="w-full h-full p-0" style="overflow-y: scroll">
          <div
            class="col-12 md:col-12 p-0"
            v-for="(item, index) in datalists"
            :key="index"
          >
            <div class="col-12 p-0 flex">
              <div class="col-2 p-0"></div>
              <div class="col-8 p-0 font-bold pl-2 text-xl pt-4 pb-2">
                {{ index + 1 }}. {{ item.data.title_name }}
              </div>
              <div class="col-2 p-0"></div>
            </div>

            <div class="col-12 p-0 flex">
              <div class="col-2 p-0"></div>
              <div class="col-8 p-0">
                <DataView
                  class="w-full h-full e-sm flex flex-column p-dataview-unset d-dataview-design"
                  :lazy="true"
                  :value="item.children"
                  layout="grid"
                  :pageLinkSize="4"
                  currentPageReportTemplate=""
                  responsiveLayout="scroll"
                  :scrollable="false"
                >
                  <template #grid="slotProps">
                    <div
                      class="md:col-3 col-3 m-1 mx-2 surface-100 card-content"
                      @click="showDetails(slotProps.data)"
                    >
                      <Card class="no-paddcontent p-0">
                        <template #title>
                          <div style="position: relative">
                            <div class="format-center">
                              <font-awesome-icon
                                style="
                                  height: 5vh;
                                  border-radius: 5px 5px 0px 0px;
                                  object-fit: cover;
                                  color: #198cf0;
                                "
                                :icon="slotProps.data.data.icon"
                              />
                            </div>
                          </div>
                        </template>
                        <template #content>
                          <div
                            class="format-center text-xl mx-2 text-3line text-title p-3 pb-0"
                         
                          >
                            {{ slotProps.data.data.title_name }}
                          </div>
                        </template>
                      </Card>
                    </div>
                  </template>

                 
                </DataView>
              </div>
              <div class="col-2 p-0"></div>
            </div>
          </div>
        </div>
      </div>
    </form>
  </div>
</template>
<style scoped>
.d-lang-table-sl {
  margin: 0px;
  height: calc(100vh - 70px);
}

.content-view {
  min-height: calc(100vh - 90px) !important;
  max-height: calc(100vh - 90px) !important;
  overflow: auto !important;
  font-size: 14px;
  line-height: 1.5rem;
}
.no-paddcontent:hover {
  background: aliceblue;
  border: 1px solid #3cc2f3;
}
.no-paddcontent {
  border: 1px solid transparent;
}
.w-inherit {
  width: inherit;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-tree-wrapper) {
  .p-treenode-label {
    width: 100%;
  }
}
</style>