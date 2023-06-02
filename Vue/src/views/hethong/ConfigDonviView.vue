<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
//init Model
const tdtypedonvis = [
  { value: 0, text: "Đơn vị" },
  { value: 1, text: "Trường học" },
  { value: 2, text: "Phòng ban" },
];
const donvi = ref({
  organization_name: "",
  STT: 1,
  Trangthai: true,
  TypeDonvi: 0,
});
//Khai báo biến
const store = inject("store");
const selectedKey = ref();
const selectedNodes = ref([]);
const filters = ref({});
const opition = ref({
  search: "",
  PageNo: 1,
  PageSize: 20,
  TypeDonvi: null,
  user_id: store.getters.user.user_id,
});
const donvis = ref();
const toast = useToast();
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const onNodeSelect = (node) => {
  selectedNodes.value.push(node.data.Donvi_ID);
};
const onNodeUnselect = (node) => {
  selectedNodes.value.splice(selectedNodes.value.indexOf(node.data.Donvi_ID), 1);
};
//Thêm sửa xoá
const onRefersh = () => {
  opition.value.search = "";
  loadDonvi(true);
};
const onSearch = () => {
  loadDonvi(true);
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
  if (
    store.getters.user.Role_ID == "admin" ||
    store.getters.user.Role_ID == "administrator"
  )
    arrtreeChils.unshift({ key: -1, data: -1, label: "-----Chọn " + title + "----" });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const tudiens = ref({});
const loadTudien = (rf) => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "sys_organiztion_list_dictionary",
        par: [{ par: "user_id", va: opition.value.user_id }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        tudiens.value = data;
      } else {
        tudiens.value = [];
      }
    })
    .catch((error) => {});
};
const onPage = (event) => {
  opition.value.PageNo = event.page + 1;
  opition.value.PageSize = event.rows;
  loadDonvi(true);
};
const loadDonvi = (rf) => {
  if (rf) {
    opition.value.loading = true;
  }
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "sys_organization_listconfig",
        par: [
          { par: "pageno", va: opition.value.PageNo },
          { par: "pagesize", va: opition.value.PageSize },
          { par: "search", va: opition.value.search },
          { par: "organization_type", va: opition.value.TypeDonvi },
          { par: "user_id", va: opition.value.user_id },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        data[0].forEach((r) => {
          if (r.groups) r.groups = JSON.parse(r.groups);
        });
        let obj = renderTree(data[0], "Donvi_ID", "organization_name", "đơn vị");
        donvis.value = obj.arrChils;
        opition.value.totalRecords = data[1][0].totalRecords;
      } else {
        donvis.value = [];
      }
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
const saveConfigDonvi = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let datas = [];
  const duyetdonvis = (dv) => {
    dv.data.groups.forEach((dg) => {
      dg.Donvi_ID = dv.data.Donvi_ID;
    });
    datas = datas.concat(dv.data.groups);
    if (dv.children && dv.children.length > 0) {
      dv.children.forEach((dv1) => {
        duyetdonvis(dv1);
      });
    }
  };
  donvis.value.forEach((dv) => {
    duyetdonvis(dv);
  });
  axios({
    method: "post",
    url: baseURL + "/api/Phongban/ConfigDonviGroup",
    data: datas,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Thiết lập duyệt cho đơn vị thành công!");
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
};
const exportDonvi = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let par = [{ par: "user_id", va: opition.value.user_id }];
  axios
    .post(
      baseURL + "/api/Excel/" + method,
      {
        excelname: "DANH SÁCH ĐƠN VỊ",
        proc: "Sys_Donvi_ListExport",
        par: par,
      },
      config
    )
    .then((response) => {
      swal.close();
      if (response.data.err != "1") {
        swal.close();
        toast.success("Kết xuất Data thành công!");
        if (response.data.path != null) {
          let pathReplace = response.data.path.replace(/\\+/g, '/').replace(/\/+/g, '/').replace(/^\//g, '');
          var listPath = pathReplace.split('/');
          var pathFile = "";
          listPath.forEach(item => {
            if (item.trim() != "")
            {
                pathFile += "/" + item;
            }
          });
          //window.open(baseURL + response.data.path);
          window.open(baseURL + pathFile);
        }
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
const menuButDuyet = ref();
let nodeSelect, selectIndex;
function selectDequy(choice) {
  const selectdonvis = (dv) => {
    if (dv.data.groups && dv.data.groups.length > selectIndex)
      dv.data.groups[selectIndex].IsDuyet = choice;
    if (dv.children && dv.children.length > 0) {
      dv.children.forEach((dv1) => {
        selectdonvis(dv1);
      });
    }
  };
  selectdonvis(nodeSelect);
}
const itemButDuyet = ref([
  {
    label: "Chọn tất cả",
    icon: "pi pi-check-square",
    command: () => {
      selectDequy(true);
    },
  },
  {
    label: "Bỏ chhọn tất cả",
    icon: "pi pi-stop",
    command: () => {
      selectDequy(false);
    },
  },
]);
const toggleDuyet = (event, data, index) => {
  nodeSelect = data;
  selectIndex = index;
  menuButDuyet.value.toggle(event);
};
onMounted(() => {
  //init
  loadDonvi(true);
  loadTudien();
});
</script>
<template>
  <div class="main-layout flex-grow-1 p-2" v-if="store.getters.islogin">
    <TreeTable
      :value="donvis"
      :loading="opition.loading"
      :filters="filters"
      :showGridlines="true"
      filterMode="lenient"
      class="p-treetable-sm no-outline e-sm"
      :paginator="opition.totalRecords > opition.PageSize"
      :rows="opition.PageSize"
      :lazy="true"
      @page="onPage($event)"
      :pageLinkSize="opition.PageSize"
      :scrollable="true"
      scrollHeight="flex"
    >
      <template #header>
        <h3 class="donvi-title module-title-hidden mt-0 ml-1 mb-2">
          <i class="pi pi-microsoft"></i> Thiết lập duyệt cho đơn vị
        </h3>
        <Toolbar class="w-full custoolbar">
          <template #start>
            <Dropdown
              :showClear="true"
              v-model="opition.TypeDonvi"
              :options="tdtypedonvis"
              optionLabel="text"
              optionValue="value"
              placeholder="Lọc theo đơn vị"
              class="p-dropdown-sm"
              @change="loadDonvi(true)"
            />
            <span class="p-input-icon-left ml-2">
              <i class="pi pi-search" />
              <InputText
                type="text"
                spellcheck="false"
                v-model="opition.search"
                placeholder="Tìm kiếm"
                v-on:keyup.enter="onSearch"
              />
            </span>
          </template>

          <template #end>
            <Button
              class="mr-2 p-button-outlined p-button-secondary"
              icon="pi pi-refresh"
              @click="onRefersh"
            />

            <Button
              label="Export"
              icon="pi pi-file-excel"
              class="mr-2 p-button-outlined p-button-secondary"
              @click="exportDonvi"
            />
            <Button
              label="Cập nhật"
              icon="pi pi-save"
              class="mr-2"
              @click="saveConfigDonvi"
            />
          </template>
        </Toolbar>
      </template>
      <Column field="organization_name" header="Tên đơn vị" :sortable="true" :expander="true">
        <template #body="md">
          <span :class="'donvi' + md.node.data.TypeDonvi">{{
            md.node.data.organization_name
          }}</span>
        </template>
      </Column>
      <Column
        v-for="(col, index) of tudiens[0]"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px"
        bodyStyle="text-align:center;max-width:120px"
        :header="col.Type_Name"
        :field="col.GroupType_ID"
        :key="col.GroupType_ID"
      >
        <template #body="md">
          <div class="w-full h-full" @contextmenu="toggleDuyet($event, md.node, index)">
            <Checkbox
              id="binary"
              v-model="md.node.data.groups[index].IsDuyet"
              :binary="true"
            />
          </div>
        </template>
      </Column>
    </TreeTable>
  </div>
  <ContextMenu ref="menuButDuyet" :model="itemButDuyet" />
</template>
<style scoped>
.classdonvi {
  background-color: aliceblue;
}
span.donvitrue {
  font-weight: 500;
}
.chip0 {
  background-color: #4285f4;
  color: #fff;
  font-size: 0.875rem;
  padding: 5px 10px;
}
.chip1 {
  background-color: #689f38;
  color: #fff;
  font-size: 0.875rem;
  padding: 5px 10px;
}
.chip2 {
  background-color: #607d8b;
  color: #fff;
  font-size: 0.875rem;
  padding: 5px 10px;
}
.ipnone {
  display: none;
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
