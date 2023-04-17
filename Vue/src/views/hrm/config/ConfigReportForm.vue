<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { encr, checkURL } from "../../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");

const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const toast = useToast();

const expandedKeys = ref([]);

const selectedUser = ref([]);

const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const treedonvis = ref([]);
const checkMultile = ref(false);
const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];

  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.STT = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em, j) => {
            em.STT = m.STT + "." + (j + 1);
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
            let om1 = { key: em[id], data: em, label: em[name] };
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
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const loadDonvi = () => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_list_dictionary",
            par: [{ par: "user_id", va: store.state.user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      treedonvis.value = [];
      let data = JSON.parse(response.data.data);

      if (data.length > 0) {
        let obj = renderTree(
          data[0],
          "organization_id",
          "organization_name",
          "phòng ban",
        );

        treedonvis.value = obj.arrChils;
        treedonvis.value.forEach((element) => {
          expandNodeMain(element);
        });
      } else {
        treedonvis.value = [];
      }
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    });
};
const listReportForm = ref([]);
const loadReportForm = () => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "smartreport_list",
            par: [{ par: "user_id", va: user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      listReportForm.value = data[0];
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    });
};
const datalists = ref([]);

const loadData = () => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_report_list",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "findOrg", va: null },
              { par: "pageno", va: options.value.pageNo },
              { par: "pagesize", va: options.value.pageSize },
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
      data.forEach((x, i) => {
        x.STT = i + 1;
      });
      datalists.value = data;
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo",
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    });
};
const user = store.state.user;
const expandedKeysMain = ref([]);
const expandNodeMain = (node) => {
  if (node.children && node.children.length) {
    expandedKeysMain.value[node.key] = true;
    for (let child of node.children) {
      expandNode(child);
    }
  }
};
const expandNode = (node) => {
  if (node.children && node.children.length) {
    expandedKeys.value[node.key] = true;
    for (let child of node.children) {
      expandNode(child);
    }
  }
};
const options = ref({
  searchText: "",
  pageNo: 0,
  pageSize: 20,
});
const filterss = () => {};
const model = ref({
  STT: "",
  organization_id: user.organization_id,
  report_key: "",
  report_title: "",
  status: true,
});
const AddNew = () => {
  model.value = {
    STT: datalists.value.length > 0 ? datalists.value.length + 1 : 1,
    organization_id: user.organization_id,
    report_key: "a",
    report_title: "a",
    status: true,
  };
  datalists.value.unshift({
    STT: datalists.value.length > 0 ? datalists.value.length + 1 : 1,
    organization_id: user.organization_id,
    report_key: "a",
    report_title: "a",
    status: true,
  });
};

onMounted(() => {
  loadReportForm();
  if (user.is_super) loadDonvi();
  loadData();
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2 flex">
    <div
      :class="user.is_super == true ? 'col-4 h-full' : ''"
      v-if="user.is_super"
    >
      <TreeTable
        :value="treedonvis"
        :expandedKeys="expandedKeysMain"
        v-model:selectionKeys="selectedKey"
        :showGridlines="true"
        filterMode="strict"
        :rowHover="true"
        responsiveLayout="scroll"
        :lazy="true"
        :scrollable="true"
        scrollHeight="flex"
        class="h-full"
      >
        <template #header>
          <h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
            <i class="pi pi-microsoft"> </i> Danh sách đơn vị
          </h3>
          <Toolbar class="w-full custoolbar">
            <template #start>
              <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText
                  type="text"
                  spellcheck="false"
                  v-model="options.searchText"
                  placeholder="Tìm kiếm theo tên đơn vị"
                  v-on:keyup.enter="filterss()"
                />
              </span>
            </template>
          </Toolbar>
        </template>
        <Column
          field="STT"
          header="STT"
          class="align-items-center justify-content-center text-center max-w-4rem"
        ></Column>
        <Column
          field="organization_name"
          header="Tên đơn vị"
          expander
          headerClass="align-items-center justify-content-center text-center"
        ></Column>
      </TreeTable>
    </div>
    <div :class="user.is_super == true ? 'col-8' : 'col-12'">
      <DataTable
        :value="datalists"
        scrollHeight="flex"
        dataKey="STT"
        :rowHover="true"
        :showGridlines="true"
        responsiveLayout="scroll"
      >
        <template #header>
          <h3 class="module-title mt-0 ml-1 mb-2">
            <i class="pi pi-file-pdf"></i>
            Danh sách mẫu báo cáo
          </h3>
          {{ listReportForm }}
          {{ datalists }}
          <Dropdown
            class="col-12 p-0 m-0"
            v-model="options.va"
            :options="listReportForm"
            optionLabel="report_name"
            optionValue="report_key"
            placeholder="Phân loại"
            showClear
            panelClass="d-design-dropdown"
          />
          <Toolbar class="w-full custoolbar">
            <template #start>
              <Button
                label="Thêm mới"
                icon="pi pi-plus"
                @click="AddNew()"
              ></Button>
            </template>

            <template #end> </template>
          </Toolbar>
        </template>
        <Column
          header="STT"
          field="STT"
          class=""
        ></Column>
        <Column
          header="Mẫu báo cáo"
          field="report_name"
          class=""
        ></Column>
        <template #empty>
          <div
            class="align-items-center justify-content-center p-4 text-center m-auto"
          >
            <img
              src="../../../assets/background/nodata.png"
              height="144"
            />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
        </template>
      </DataTable>
    </div>
  </div>
</template>
<style lang="scss" scoped></style>
