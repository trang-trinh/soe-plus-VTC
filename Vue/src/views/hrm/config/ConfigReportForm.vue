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
function model(STT, organization_id, report_key, report_title, status) {
  this.STT = STT;
  this.organization_id = organization_id;
  this.report_title = report_title;
  this.report_key = report_key;
  this.status = status;
}

const headerDialog = ref();
const DialogVisible = ref(false);
const closeDialog = () => {
  submitted.value = false;
  models.value = [];
  DialogVisible.value = false;
};
const models = ref([]);
const AddNew = () => {
  isEdit.value = false;
  let stt = datalists.value.length > 0 ? datalists.value.length + 1 : 1;
  let u_org = user.organization_id;
  let t = new model(stt, u_org, "", "", true);
  models.value.unshift(t);
  DialogVisible.value = true;
  headerDialog.value = "Thêm mẫu báo cáo";
};
const addALine = () => {
  let stt =
    models.value.length > 0
      ? models.value.length + datalists.value.length + 1
      : 1;
  let u_org = user.organization_id;
  let t = new model(stt, u_org, "", "", true);
  models.value.unshift(t);
};
const isEdit = ref(false);
const index = ref([]);
const EditData = (e, i) => {
  isEdit.value = true;
  let t = JSON.parse(JSON.stringify(e));
  models.value = [];
  headerDialog.value = "Chỉnh sửa mẫu báo cáo";
  DialogVisible.value = true;
  models.value.push(t);
};
const submitted = ref(false);
const saveData = (e) => {
  submitted.value = true;
  let arr = models.value.filter(
    (x) =>
      x.report_title == null ||
      x.report_title == "" ||
      x.report_key == null ||
      x.report_key == "",
  );
  if (arr.length > 0) {
    return;
  } else {
    let formData = new FormData();
    formData.append("sys_organization_report", JSON.stringify(models.value));
    debugger;
    axios({
      method: isEdit.value == false ? "post" : "put",
      url:
        baseURL +
        "/api/sys_organization_report/" +
        (isEdit.value == false ? "AddReportForm" : "UpdateReportForm"),
      data: formData,
      headers: {
        Authorization: `Bearer ${store.getters.token}`,
      },
    })
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Đánh giá báo cáo thành công!");
          loadData();
          closeDialog();
          swal.close();
        } else {
          swal.close();
          let ms = response.data.ms;
          swal.fire({
            title: "Thông báo!",
            html: ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Thông báo",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  }
};
const DelData = (e) => {};
const editingRows = ref([]);
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
        :rowHover="true"
        :showGridlines="true"
        responsiveLayout="scroll"
        scrollHeight="flex"
        dataKey="key_id"
        class="col-12 py-0 px-0"
      >
        <template #header>
          <h3 class="module-title mt-0 ml-1 mb-2">
            <i class="pi pi-file-pdf"></i>
            Danh sách mẫu báo cáo
          </h3>

          <Toolbar class="w-full custoolbar">
            <template #end>
              <Button
                v-if="
                  user.organization_id == options.organization_id ||
                  !user.is_super
                "
                label="Thêm mới"
                icon="pi pi-plus"
                @click="AddNew()"
              ></Button>
            </template>
          </Toolbar>
        </template>
        <Column
          header="STT"
          field="STT"
          class="align-items-center justify-content-center text-center w-1rem"
        >
        </Column>
        <Column
          header="Tên báo cáo1"
          field="report_title"
          headerClass="align-items-center justify-content-center text-center w-15rem"
        >
        </Column>
        <Column
          header="Mẫu báo cáo"
          field="report_name"
          class="align-items-center justify-content-center text-center max-w-10rem"
        >
        </Column>
        <Column
          field="status"
          header="Hiển thị"
          :class="'align-items-center justify-content-center text-center max-w-4rem'"
        >
          <template #body="data">
            <Checkbox
              :binary="data.data.status"
              v-model="data.data.status"
              @click="onCheckBox(data.data)"
            />
          </template>
        </Column>
        <Column
          header="Chức năng"
          field=""
          class="align-items-center justify-content-center text-center max-w-10rem"
        >
          <template #body="data">
            <div>
              <Button
                @click="EditData(data.data, data.index)"
                class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                type="button"
                icon="pi pi-pencil"
                v-tooltip="'Sửa'"
              ></Button>
              <Button
                @click="DelData(data.data, true)"
                class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                type="button"
                icon="pi pi-trash"
                v-tooltip="'Xóa'"
              ></Button>
            </div>
          </template>
        </Column>
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

  <Dialog
    :header="headerDialog"
    v-model:visible="DialogVisible"
    :style="{ width: '50vw' }"
    :closable="true"
    :maximizable="true"
    @hide="closeDialog()"
  >
    <div
      class="col-12"
      v-if="isEdit == false"
    >
      <Button
        label="Thêm mẫu"
        style="float: right"
        @click="addALine()"
      ></Button>
    </div>
    <div class="col-12 flex p-0 align-items-center justify-content-center">
      <div class="col-1 flex align-items-center justify-content-center">
        STT
      </div>
      <div class="col-5 flex align-items-center justify-content-center">
        Tên báo cáo <span class="redsao">(*)</span>
      </div>

      <div class="col-4 flex align-items-center justify-content-center">
        Mẫu báo cáo<span class="redsao">(*)</span>
      </div>
      <div class="col-2 flex align-items-center justify-content-center">
        Trạng thái
      </div>
    </div>
    <div
      v-for="(item, index) in models"
      :key="index"
    >
      <div class="col-12 flex p-0 align-items-center justify-content-center">
        <div class="col-1 flex align-items-center justify-content-center">
          {{ item.STT }}
        </div>
        <InputText
          v-model="item.report_title"
          spellcheck="false"
          type="text"
          class="col-5"
          :class="{
            'p-invalid': !item.report_title && submitted,
          }"
        />

        <div class="col-4 flex align-items-center justify-content-center">
          <Dropdown
            v-model="item.report_key"
            :options="listReportForm"
            optionLabel="report_name"
            optionValue="report_key"
            placeholder="Phân loại"
            showClear
            class="w-full"
            panelClass="d-design-dropdown"
            :class="{
              'p-invalid': !item.report_key && submitted,
            }"
          />
        </div>

        <div class="col-2 flex align-items-center justify-content-center">
          <InputSwitch v-model="item.status" />
        </div>
      </div>
      <div class="col-12 flex p-0 align-items-center justify-content-center">
        <div class="col-1 p-0 flex"></div>
        <div class="col-5 p-0 flex">
          <small
            v-if="!item.report_title && submitted"
            :class="{
              'p-text-invalid': !item.report_title && submitted,
            }"
          >
            Tên báo cáo không được để trống!</small
          >
        </div>
        <div class="col-4">
          <small
            v-if="!item.report_title && submitted"
            :class="{
              'p-text-invalid': !item.report_title && submitted,
            }"
          >
            Mẫu báo cáo không được để trống!</small
          >
        </div>
        <div class="col-2 p-0"></div>
      </div>
    </div>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog()"
        class="p-button-outlined"
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveData()"
      />
    </template>
  </Dialog>
</template>
<style lang="scss" scoped>
.p-text-invalid {
  color: red;
}
</style>
