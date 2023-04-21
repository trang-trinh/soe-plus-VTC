<script setup>
import { ref, inject, onMounted, watch } from "vue";
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
const datalistsDetails = ref();
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
const itemButs = ref([
  {
    label: "Xuất Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportDonvi("ExportExcel");
    },
  },
  {
    label: "Xuất Mẫu",
    icon: "pi pi-file-excel",
    command: (event) => {
      exportDonvi("ExportExcelMau");
    },
  },
]);
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
const onChangeParent = (item) => {
  const organization_id = parseInt(Object.keys(item)[0]);
  axios
    .post(
      baseURL + "/api/Phongban/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_get_order",
            par: [{ par: "organization_id", va: organization_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        donvi.value.is_order = data[0][0].c + 1;
      }
    });
};

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

const onPage = (event) => {
  opition.value.PageNo = event.page + 1;
  opition.value.PageSize = event.rows;
  loadDonvi(true);
};
const loadDonvi = (rf) => {
  axios
    .post(
      baseURL + "/api/Phongban/GetDataProc",
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
      data.forEach((element, i) => {
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;
        if(element.is_level>1){
          element.isClosed = true;
        element.isOpened = false;
        }
        else{
          element.isClosed = false;
        element.isOpened = true;
        }

        if (data.find(x => x.parent_id == element.organization_id)) {
          element.canExpand = true;
        }
      });
      donvis.value = data;
      // treedonvis.value = obj.arrtreeChils;
      opition.value.loading = false;

      if (data[0] != null)
        loadDataDetail(data[0].organization_id, data[0].organization_name);
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
const loadDataDetail = (id, name) => {
  id_active.value = id;
  department_name.value = name;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "hrm_contact_list_user",
        par: [
          { par: "organization_id", va: id },
          { par: "user_id", va: store.getters.user.user_id },
          { par: "search", va: options.value.SearchText },
          { par: "pageno", va: options.value.pagenoP },
          { par: "pagesize", va: options.value.pagesizeP },
        ],
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element, i) => {
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;
      });
      if (isFirst.value) isFirst.value = false;

      datalistsDetails.value = data;
      options.value.loadingP = false;
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");

      options.value.loading = false;
    });
};
const item_active = ref();
const toggle_Donvisudung = (dv) => {
  item_active.value = dv.organization_id;
  //angular.forEach($scope.td.diadanh, function (item) {
  //    if (item.Diadanh_ID !== dv.Diadanh_ID) {
  //        item.activeDd = false;
  //    }
  //    else {
  //        item.activeDd = !item.activeDd;
  //    }
  //});
  dv.isOpened = !dv.isOpened;
  var lst = donvis.value.filter(l => l.parent_id === dv.organization_id);
  lst.forEach((o) => {
    o.isClosed = !o.isClosed;
    if (o.isClosed) {
      Expanded(o);
    }
  })
};
function Expanded(dv) {
  dv.isOpened = false;
  var lst = donvis.value.filter(l => l.parent_id === dv.organization_id);
  if (lst !== null || lst.length > 0) {
    lst.forEach((o) => {
      o.isClosed = true;
      if (o.isClosed) {
        Expanded(o);
      }
    })
  }
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
const rowClass = (data) => {
  return data.organization_type == 0
    ? "classdonvi"
    : data.organization_type == 1
      ? "classtruonghoc"
      : "classphongban";
};
const rowClassStatus = (data) => {
  return data.status ? "" : "error";
};
onMounted(() => {
  //init
  loadDonvi(true);
});
</script>

<template>
  <div>
    <div>
      <Splitter class="h-full w-full pb-0 pr-0">
        <SplitterPanel :size="25" class=" ">
            <div class="pl-3 tab-left">
                <div v-for="(dv, index) in donvis" :key="index"
                :style="'margin-left:' +(dv.is_level+2) + 'em'" v-show="!dv.isClosed" class="my-3 mr-2 relative cursor-pointer ">
                <div @mouseover="dv.hover = true" @mouseleave="dv.hover = false" style="min-height:20px"  >
                    <span @click="toggle_Donvisudung(dv)" class="absolute" style="left: -1.7rem;top: 0" v-if="dv.canExpand">
                    <!-- <a v-if="!dv.isOpened"><i style="font-size: 16px;" class="pi pi-plus-circle"></i></a> -->
                    <a v-if="!dv.isOpened"><font-awesome-icon icon="fa-solid fa-square-plus" style="font-size: 16px; color: gray;" /></a>
                    <a v-if="dv.isOpened"><font-awesome-icon icon="fa-solid fa-square-minus" style="font-size: 16px; color: gray;" /></a>
                    </span>
                    <div class="w-full text-lg item-hover" :class="{'active':  id_active=== dv.organization_id}"  @click="loadDataDetail(dv.organization_id, dv.organization_name)">{{ dv.organization_name }}</div>
                </div>

                </div>
            </div>
        </SplitterPanel>
        <SplitterPanel :size="75">
          <div class="d-lang-table-r">
            <div class="p-3" v-if="datalistsDetails">
              <h3 class="module-title m-0">
                <i class="pi pi-book"></i> {{ department_name }} ({{
                  datalistsDetails.length
                }})
              </h3>
            </div>
            <DataTable  class="w-full p-datatable-sm e-sm cursor-pointer" :value="datalistsDetails"
           v-model:filters="filters" :showGridlines="true"
          filterMode="lenient" :paginator="'true'" :rows="options.PageSize" filterDisplay="menu" selectionMode="single"
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          :scrollable="true" scrollHeight="flex" responsiveLayout="scroll" 
          pageLinkSize="4">
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
                      : slotProps.data.last_name.substring(0, 1)
                  "
                  v-bind:image="
                    slotProps.data.avatar
                      ? basedomainURL + slotProps.data.avatar
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                  @error="basedomainURL + '/Portals/Image/noimg.jpg'"
                  size="large"
                  shape="circle"
                  class="cursor-pointer"
                  :style="{
                    backgroundColor: bgColor[(slotProps.data.full_name.length+10) % 7],
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
            field="full_name"
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
            bodyStyle="text-align:center;max-width:80px;"
            class="align-items-center justify-content-center text-center"
            >
            </Column>   
            <Column
            field="email"
            header="Email"
            headerStyle="text-align:center;max-width:140px;height:50px"
            bodyStyle="text-align:center;max-width:140px; word-break:break-word"
            class="align-items-center justify-content-center text-center"
            >
            </Column>   
            <Column
            field="role_name"
            header="Chức vụ"
            headerStyle="text-align:center;max-width:100px;height:50px"
            bodyStyle="text-align:center;max-width:100px;"
            class="align-items-center justify-content-center text-center"
            >
            </Column> 
            <Column
            field="organization_name"
            header="Phòng ban"
            headerStyle="text-align:center;max-width:150px;height:50px"
            bodyStyle="text-align:center;max-width:150px;"
            class="align-items-center justify-content-center text-center"
            >
            </Column> 
            <template #empty>
                <div class="
                    align-items-center
                    justify-content-center
                    p-4
                    text-center
                    m-auto
                    " v-if="!isFirst">
                <img src="../../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
                </div>
            </template>
        </DataTable>
          </div>
        </SplitterPanel>
      </Splitter>
    </div>
  </div>

</template>

<style scoped>
.tab-left{
    overflow: auto;
    max-height: calc(100vh - 105px);
    min-height: calc(100vh - 105px);
}

.ipnone {
  display: none;
}

.row-active,.active {
  color: rgb(13, 137, 236);
}
.c-red-600{
  color:red;
}
.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}

.d-lang-table {
  margin: 0px;
  height: calc(100vh - 122px);
}

.d-lang-table-r {
  margin: 0px;
  height: calc(100vh - 100px);
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
</style>
