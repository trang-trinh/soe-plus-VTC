<script setup>
import { ref, inject, onMounted, watch,nextTick } from "vue";
import { required, maxLength, minLength, email } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import arrIcons from "../../../assets/json/icons.json";
import { encr } from "../../../util/function.js";
import moment from "moment";

const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
//init Model
const tdorganization_types = [
  { value: 0, text: "Đơn vị" },
  // { value: 1, text: "Trường học" },
  { value: 1, text: "Phòng ban" },
];
const donvi = ref({
  organization_name: "",
  is_order: 1,
  status: true,
  organization_type: 0,
});
//color
const bgColor = ref([
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
  "#B0DE09",
]);

//Valid Form
const expandedKeys = ref({})
const id_active = ref();
const department_name = ref();
const datalistsDetails = ref([]);
const store = inject("store");
const selectedNodes = ref([]);
const filters = ref({});
const opition = ref({
  search: "",
  PageNo: 1,
  PageSize: 1000,
  organization_type: null,
  user_id: store.getters.user.user_id,
});

const options = ref({
  IsNext: true,
  sort: "created_date",
  SearchText: null,
  PageNo: 1,
  PageSize: 20,
  limitItem: 20,
  loading: false,
  totalRecords: null,
  loadingP: true,
  pagenoP: 0,
  pagesizeP: 20,
  searchP: "",
  sortP: "created_date",
  total : 0,
});
const donvis = ref();
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
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};
const onNodeSelect = (node) => {
  selectedNodes.value.push(node.data.organization_id);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(
    selectedNodes.value.indexOf(node.data.organization_id),
    1,
  );
};
// on event

const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data = data.sort((a, b) => {
    return a.parent_id - b.parent_id;
  });
  let parent_id = null;
  if (data && data.length > 0) {
    parent_id = data[0].parent_id;
  }
  data
    .filter((x) => x.parent_id == parent_id)
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

const onPage = (event) => {
  opition.value.PageNo = event.page + 1;
  opition.value.PageSize = event.rows;
  loadDonvi(true);
};
const selectedKey = ref();
const loadDonvi = (rf) => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  options.value.loading = true;
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_contact_list_organization",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )

    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (isFirst.value) isFirst.value = false;
      let obj = renderTree(
        data,
        "organization_id",
        "organization_name",
        "đơn vị",
      );
      donvis.value = obj.arrChils;
      
      for (let node of donvis.value) {
        expandedKeys.value[node.key] = true;
        if (node.data.is_level < 1) {   
          expandNode(node);
        }
      }
      // if(!store.getters.user.is_super || store.getters.user.organization_id != null){
      //     donvis.value.forEach((element) => {
      //       expandNode(element);
      //     });
      //   }
      // treedonvis.value = obj.arrtreeChils;

      if (data[0] != null){
        selectedKey.value ={};
        selectedKey.value[data[0].organization_id] = true;
        loadDataDetail(data[0].organization_id, data[0].organization_name, true);

      }
      swal.close();
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      swal.close();
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const expandNode = (node) => {
  if (node.children && node.children.length) {
    expandedKeys.value[node.key] = true;
    // for (let child of node.children) {
    //   expandNode(child);
    // }
  }
};
const onSearch = ()=>{
  loadDataDetail(id_active.value, department_name.value, true);
}
const loadDataDetail = (id, name, rf) => {
  if(rf) {
    options.value.PageNo = 1;
    options.value.limitItem = 20;
    datalistsDetails.value = [];
  }
  options.value.total=0;
  id_active.value = id;
  department_name.value = name;
  options.value.loading = true;
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_contact_list_user",
          par: [
            { par: "organization_id", va: id },
            { par: "user_id", va: store.getters.user.user_id },
            { par: "search", va: options.value.SearchText },
            { par: "pageNo", va: options.value.PageNo },
            { par: "pageSize", va: options.value.PageSize },
            { par: "is_hrm", va: 1 },           
          ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if(data[0].length>0){
        data[0].forEach((element, i) => {
        element.STT = (options.value.PageNo-1) * options.value.PageSize + i + 1;
      });
      datalistsDetails.value = datalistsDetails.value.concat(data[0])
      }
      else datalistsDetails.value= [];
      if (data[1] != null && data[1].length > 0) {
        options.value.total = data[1][0].total;
      }
      options.value.loading = false;

    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
    });
};

const onRefresh = ()=>{
  options.value = {
    IsNext: true,
    SearchText: null,
    PageNo: 1,
    PageSize: 20,
    limitItem: 20,
    loading: true,
    totalRecords: null,
    loadingP: true,
    searchP: "",
    total: 0,
  };
  datalistsDetails.value = [];
  loadDonvi(true);
}
const itemButs = ref([
  {
    label: "Export dữ liệu ra Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportExcel();
    },
  }
]);
const exportExcel = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let proc = "hrm_contact_list_user_export";
  axios
    .post(
      baseURL + "/api/Excel/ExportExcel" ,
      {
        excelname: "DANH BA_"+department_name.value.toUpperCase(),
        proc: proc,
        par: [
        { par: "organization_id", va: id_active.value },
          { par: "user_id", va: store.getters.user.user_id },
          { par: "search", va: options.value.SearchText },
          { par: "pageno", va: options.value.pagenoP },
          { par: "pagesize", va: options.value.pagesizeP },
        ],
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Kết xuất Data thành công!");
        //window.open(baseURL + response.data.path);
        if (response.data.path != null) {
          let pathReplace = response.data.path
            .replace(/\\+/g, "/")
            .replace(/\/+/g, "/")
            .replace(/^\//g, "");
          var listPath = pathReplace.split("/");
          var pathFile = "";
          listPath.forEach((item) => {
            if (item.trim() != "") {
              pathFile += "/" + item;
            }
          });
          window.open(baseURL + pathFile);
        }
      } else {
        swal.fire({
          title: "Thông báo!",
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
const loadMoreRow = (data) => {
  if (data.length > 0) {
    if (
      !options.value.loading &&
      options.value.limitItem < options.value.total
    ) {
      options.value.limitItem += 20;
      options.value.PageNo += 1;
      //dataLimits.value = datas.value.slice(0, options.value.limitItem);
        loadDataDetail(id_active.value,department_name.value);
      
    } else {
      options.value.limitItem = data.length;
      //dataLimits.value = datas.value.slice(0, options.value.limitItem);
      //initData(false);
    }
  }
};
const goBack = () => {
  history.back();
}; 
onMounted(() => {
  nextTick(() => {
    loadDonvi(true);
  
  const el = document.getElementById("buffered-scroll");
    if (el) {
      el.addEventListener("scroll", (event) => {
        const scrollTop = el.scrollTop;
        const scrollHeight = el.scrollHeight;
        const offsetHeight = el.offsetHeight;
        if (scrollTop >= scrollHeight - offsetHeight - 50) {
          loadMoreRow(datalistsDetails.value);
        }
      });
    }
  })
  //init
  
});
</script>

<template>
  <div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
    <div>
      <div style="background-color: #fff; padding: 1rem;">
        <Toolbar class="w-full custoolbar">
          <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                style="width:320px"
                v-model="options.SearchText"
                v-on:keyup.enter="onSearch()"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />            
            </span>
          </template>

          <template #end>
            <Button
            label="Quay lại"
            icon="pi pi-arrow-left"
            class="p-button-outlined mr-2 p-button-secondary"
            @click="goBack()"
            />
            <Button
              @click="onRefresh"
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              v-tooltip="'Tải lại'"
            />

            <Button
              label="Tiện ích"
              icon="pi pi-file-excel"
              class="mr-2 p-button-outlined p-button-secondary"
              @click="toggleExport"
              aria-haspopup="true"
              aria-controls="overlay_Export"
            />
            <Menu
              id="overlay_Export"
              ref="menuButs"
              :model="itemButs"
              :popup="true"
            /> 
          </template>
        </Toolbar>
    </div>
      <Splitter class="h-full w-full pb-0 pr-0">
        <SplitterPanel :size="25" class=" ">
          <div class="tab-left">
              <!-- <div>
                <Toolbar>
                  <template #start>
                    <span class="p-input-icon-left">
                      <i class="pi pi-search" />
                      <InputText
                        v-model="filters['global']"
                        type="text"
                        spellcheck="false"
                        placeholder="Tìm kiếm đơn vị"
                      />
                    </span>
                  </template>
                  <template #end>
                  </template>
                </Toolbar>
              </div> -->
              <div>
                <div class="w-full d-lang-table pt-1">
                  <TreeTable
                    :value="donvis"
                    v-model:selectionKeys="selectedKey"
                    @nodeUnselect="onNodeUnselect"
                    :expandedKeys="expandedKeys"
                    :filters="filters"
                    :showGridlines="false"
                    filterMode="strict"
                    class="p-treetable-sm"
                    :rowHover="true"
                    responsiveLayout="scroll"
                    :scrollable="true"
                    scrollHeight="flex"
                    metaKeySelection="true"
                    selectionMode="single"
                    @nodeSelect= "(node)=> loadDataDetail(
                    node.data.organization_id,
                    node.data.organization_name,
                    true
                  )"
                  >
                    <Column
                      field="organization_name"
                      header="Tên đơn vị/phòng ban"
                      headerStyle=""
                      bodyStyle="text-align:left;word-break:break-word;padding:0.5rem !important;"
                      :expander="true"
                    >
                      <template #body="md">
                        <div>
                          <span
                            :style="{ color: 'rgb(0, 90, 158)'}"
                            >{{ md.node.data.organization_name }}
                            <span v-if="md.node.data.total && md.node.data.total > 0"
                                >({{ md.node.data.total }})</span
                              >
                            </span
                          >
                        </div>
                      </template>
                    </Column>
                  </TreeTable>
                </div>
              </div>
            </div>
        </SplitterPanel>
        <SplitterPanel :size="75">
          <div class="d-lang-table-r" id="buffered-scroll">
            <DataTable  class="w-full p-datatable-sm e-sm"
             :value="datalistsDetails"
              :virtualScrollerOptions="{ itemSize: 78 }"
              v-model:selection="selectedNodes"
              :showGridlines="true"
              selectionMode="single"
              dataKey="profile_code"
              >
              <Column field="STT"
              header="STT"
              headerStyle="text-align:center;max-width:50px;height:50px"
              bodyStyle="text-align:center;max-width:50px;"
              class="align-items-center justify-content-center text-center"
              >
              </Column>
              <Column
                  field="avatar"
                  header="Ảnh"
                  headerStyle="text-align:center;max-width:80px;height:50px"
                  bodyStyle="text-align:center;max-width:80px;"
                  class="align-items-center justify-content-center text-center"
              >
              <template #body="slotProps">
                  <Avatar
                    v-bind:label="
                      slotProps.data.avatar
                        ? ''
                        : slotProps.data.profile_last_name.substring(0, 1)
                    "
                    v-bind:image="
                      slotProps.data.avatar
                        ? basedomainURL + slotProps.data.avatar
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                    size="large"
                    shape="circle"
                    :style="{
                      backgroundColor: bgColor[(slotProps.data.profile_user_name.length+10) % 7],
                      color: 'white',
                    }"
                  />
              </template>
              </Column>
              <Column
              field="profile_code"
              header="Mã nhân sự"
              headerStyle="text-align:center;max-width:80px;height:50px;"
              bodyStyle="text-align:center;max-width:80px;"
              class="align-items-center justify-content-center text-center"
              >
              </Column>
              <Column
              field="profile_user_name"
              header="Họ và tên"
              headerStyle="text-align:center;height:50px;justify-content:center"
              bodyStyle="text-align:left;word-break:break-word;justify-content:start"
              >
              </Column>
              <Column
              field="birthday"
              header="Ngày sinh"
              headerStyle="text-align:center;max-width:100px;height:50px"
              bodyStyle="text-align:center;max-width:100px;"
              class="align-items-center justify-content-center text-center"
              >
              <template #body="{ data }">
                <span v-if="data.birthday"> {{ moment(new Date(data.birthday)).format("DD/MM/YYYY ") }}</span>
              </template>
              </Column>
              <Column
              field="gender"
              header="Giới tính"
              headerStyle="text-align:center;max-width:80px;height:50px"
              bodyStyle="text-align:center;max-width:80px;"
              class="align-items-center justify-content-center text-center"
              >
              <template #body="{ data }">
                <span> {{data.gender==0?'Nữ':data.gender==1?'Nam':'' }}</span>
              </template>
              </Column>     
              <Column
              field="phone"
              header="Mobile"
              headerStyle="text-align:center;max-width:80px;height:50px"
              bodyStyle="max-width:80px;text-align:left;word-break:break-word;justify-content:start"
              >
              </Column>   
              <Column
              field="email"
              header="Email"
              headerStyle="text-align:center;max-width:140px;height:50px"
              bodyStyle="max-width:140px;text-align:left;word-break:break-word;justify-content:start"
              >
              </Column>   
              <Column
              field="position_name"
              header="Chức vụ"
              headerStyle="text-align:center;max-width:100px;height:50px"
              bodyStyle="max-width:100px;text-align:left;word-break:break-word;justify-content:start"
              >
              </Column> 
              <Column
              field="organization_name"
              header="Phòng ban"
              headerStyle="text-align:center;max-width:150px;height:50px"
              bodyStyle="max-width:150px; text-align:left;word-break:break-word;justify-content:start"
              >
              </Column> 
            </DataTable>
            <div
              v-if="options.loading"
              class="format-center"
              :style="{ height: '50px' }"
            >
              <i class="pi pi-sync rotate"></i>
              <span class="ml-3 loading-dots"> Đang tải dữ liệu </span>
            </div>
          </div>
        </SplitterPanel>
      </Splitter>
    </div>
  </div>

</template>

<style scoped>

.row-active,.active {
  color: rgb(13, 137, 236);
}
.c-red-600{
  color:red;
}
.d-lang-table {
  margin: 0px;
  height: calc(100vh - 122px);
}

.d-lang-table-r {
  margin: 0px;
  height: calc(100vh - 126px);
  overflow: auto;
}
.item-hover:hover{
  color:#0099f3;
}
</style>
<style lang="scss" scoped>
// ::v-deep(.p-treetable-tbody) {
//   tr {
//     cursor: pointer;
//   }
//   tr > td {
//   border:none;
// }
//}
::v-deep(.col-12) {
  .p-inputswitch {
    top: 6px;
  }
}
::v-deep(.p-datatable) {
  .p-datatable-thead > tr > th {
    background-color:#fff ;
  }
}
::v-deep(.avt-org) {
  img {
    object-fit: contain !important;
  }
}
::v-deep(.tab-left) {
  .p-toolbar,.p-treetable .p-treetable-thead > tr > th {
    background:#fff !important;
  }
  thead> tr{
  display:none; 
  }
}
::v-deep(.d-lang-table-r) {
  .p-toolbar {
    border: none;
    padding-bottom:0px !important ;
  }
  .p-datatable-hoverable-rows .p-selectable-row {
    cursor: auto;
}
}
</style>
