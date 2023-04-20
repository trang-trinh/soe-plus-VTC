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

const treedonvis = ref([]);

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
            par: [{ par: "user_id", va: user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      treedonvis.value = [];
      let data = JSON.parse(response.data.data)[0];
      console.log(data, "dảa");
      if (data.length > 0) {
        let obj = renderTree(
          data,
          "organization_id",
          "organization_name",
          "phòng ban",
        );
        console.log(obj, " " + "obj");
        treedonvis.value = obj.arrChils;
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
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  if (user.is_super == true) loadDonvi();
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_report_list",
            par: [
              { par: "user_id", va: user.user_id },
              { par: "findOrg", va: options.value.filtersOrg },
              { par: "pageno", va: options.value.pageNo },
              { par: "pagesize", va: options.value.pageSize },
              { par: "search", va: options.value.searchText },
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
      let data2 = JSON.parse(response.data.data)[1];
      data.forEach((x, i) => {
        x.STT = options.value.pageNo * options.value.pageSize + (i + 1);
      });
      datalists.value = data;
      swal.close();
      options.value.totalRecords = data2[0].totalRecords;
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
const user = store.getters.user;
const expandedKeysMain = ref([]);
const expandNodeMain = (node) => {
  if (node.children && node.children.length > 0) {
    expandedKeysMain.value[node.key] = true;
    for (let child of node.children) {
      expandNodeMain(child);
    }
  }
};

const options = ref({
  searchText: "",
  pageNo: 0,
  pageSize: 20,
  organization_id: user.organization_id,
  searchTextOrg: "",
  filtersOrg: null,
  totalRecords: null,
});
const filterss = () => {
  if (options.value.searchTextOrg != null) {
    filters.value["global"] = options.value.searchTextOrg;
  } else filters.value["global"] = "";
};
function model(STT, organization_id, report_key, report_title, status) {
  this.STT = STT;
  this.organization_id = organization_id;
  this.report_title = report_title;
  this.report_key = report_key;
  this.status = status;
  this.is_system = user.is_super ? true : false;
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
const EditMultiple = () => {
  isEdit.value = true;
  let t = [];
  t = JSON.parse(JSON.stringify(selectedProduct.value));
  models.value = [];
  models.value = [...t];
  headerDialog.value = "Chỉnh sửa mẫu báo cáo";
  DialogVisible.value = true;
};

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
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
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
          toast.success("Sửa mẫu báo cáo thành công!");
          loadData();
          closeDialog();
          selectedProduct.value = [];
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
const DelData = (e, str) => {
  let arrID = [];
  if (str === "single") {
    arrID.push(e.key_id);
  } else
    e.forEach((x) => {
      arrID.push(x.key_id);
    });
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá danh sách này không!",
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
          .delete(baseURL + "/api/sys_organization_report/DeleteReportForm", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: arrID != null ? arrID : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá danh sách thành công!");
              selectedProduct.value = [];
              loadData(true);
            } else {
              swal.fire({
                title: "Thông báo",
                html: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
            if (error.status === 401) {
              swal.fire({
                title: "Thông báo",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const selectedProduct = ref([]);
const onCheckBox = (e) => {
  let data = {
    IntID: e.key_id,
    TextID: e.key_id + "",
    IntTrangthai: 1,
    BitTrangthai: e.status,
  };
  axios
    .put(
      baseURL + "/api/sys_organization_report/UpdateStatusReportForm",
      data,
      config,
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái thành công!");
        loadData(true);
        closeDialog();
      } else {
        swal.fire({
          title: "Thông báo",
          text: response.data.ms,
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
};
const clickRow = (node) => {
  options.value.filtersOrg = node.data.organization_id;
  loadData();
};
const UNclickRow = () => {
  options.value.filtersOrg = null;
  loadData();
};
const RefreshData = () => {
  options.value = {
    searchText: "",
    pageNo: 0,
    pageSize: 20,
    organization_id: user.organization_id,
    searchTextOrg: "",
    filtersOrg: null,
    totalRecords: null,
  };
  loadData();
};
const selectedKey = ref();
const filters = ref({});
const onPage = (e) => {
  options.value.pageNo = e.page;
  options.value.pageSize = e.rows;
  loadData(true);
};
const first = ref(0);
onMounted(() => {
  loadReportForm();
  loadData();
  if (user.is_super == true) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
    setTimeout(() => {
      treedonvis.value.forEach((element) => {
        console.log(element);
        expandNodeMain(element);
        console.log(expandedKeysMain.value);
      });
    }, 750);
    swal.close();
  }
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2 inline-flex">
    <div
      v-if="user.is_super == true && treedonvis.length > 0"
      class="col-3"
    >
      <TreeTable
        :value="treedonvis"
        :expandedKeys="expandedKeysMain"
        :showGridlines="true"
        :rowHover="true"
        responsiveLayout="scroll"
        :scrollable="true"
        filterMode="lenient"
        scrollHeight="flex"
        :filters="filters"
        :globalFilterFields="['organization_id', 'organization_name']"
        v-model:selectionKeys="selectedKey"
        selectionMode="single"
        :metaKeySelection="false"
        @node-select="clickRow"
        @nodeUnselect="UNclickRow"
      >
        <template #header>
          <Toolbar class="w-full custoolbar">
            <template #start>
              <h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
                <i class="pi pi-microsoft"> </i> Danh sách đơn vị
              </h3>
            </template>
            <template #end>
              <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText
                  type="text"
                  spellcheck="false"
                  v-model="options.searchTextOrg"
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
    <div class="col">
      <DataTable
        :value="datalists"
        show-gridlines="true"
        :scrollable="true"
        scroll-height="flex"
        v-model:selection="selectedProduct"
        dataKey="key_id"
        :paginator="true"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
        :rowsPerPageOptions="[20, 30, 50, 100, 200]"
        :rows="options.pageSize"
        :totalRecords="options.totalRecords"
        responsiveLayout="scroll"
        v-model:first="first"
        scrollHeight="flex"
        :loading="options.loading"
        :lazy="true"
        @page="onPage($event)"
        :rowHover="true"
        :showGridlines="true"
      >
        <template #header>
          <h3 class="module-title mt-0 ml-1 mb-2">
            <i class="pi pi-file-pdf word-break-break-all"></i>
            Danh sách cấu hình mẫu báo cáo ({{ options.totalRecords }})
            <!-- <br />{{
              options
            }}
            <br />{{ user }} <br />{{ selectedProduct }} -->
            <!-- <div class="col-12">
              {{ store.getters.user }}
              <br />
              {{ store.state.user }}
            </div> -->
          </h3>

          <Toolbar class="w-full custoolbar">
            <template #start>
              <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText
                  type="text"
                  spellcheck="false"
                  v-model="options.searchText"
                  placeholder="Tìm kiếm "
                  v-on:keyup.enter="loadData()"
                />
              </span>
            </template>
            <template #end>
              <Button
                v-if="
                  ((user.is_super &&
                    user.organization_id == options.filtersOrg) ||
                    !user.is_super) &&
                  selectedProduct.length > 0
                "
                class="p-button-danger mx-2"
                label="Xóa"
                icon="pi pi-trash"
                @click="DelData(selectedProduct, 'multiples')"
              ></Button>
              <Button
                v-if="
                  ((user.is_super &&
                    user.organization_id == options.filtersOrg) ||
                    !user.is_super) &&
                  selectedProduct.length > 0
                "
                class="mx-2"
                label="Sửa"
                icon="pi pi-pencil"
                @click="EditMultiple()"
              ></Button>
              <Button
                v-if="
                  (user.is_super &&
                    user.organization_id == options.organization_id) ||
                  !user.is_super
                "
                class="mx-2"
                label="Thêm mới"
                icon="pi pi-plus"
                @click="AddNew()"
              ></Button>
              <Button
                class="mx-2 p-button-outlined"
                label="Tải lại"
                icon="pi pi-refresh"
                @click="RefreshData()"
              ></Button>
            </template>
          </Toolbar>
        </template>
        <Column
          selectionMode="multiple"
          class="align-items-center justify-content-center text-center max-w-3rem"
          v-if="user.organization_id == options.filtersOrg"
        ></Column>
        <Column
          header="STT"
          field="STT"
          class="align-items-center justify-content-center text-center max-w-3rem"
        >
        </Column>
        <Column
          header="Tên báo cáo"
          field="report_title"
          class="align-items-center justify-content-center text-center"
        >
        </Column>
        <Column
          header="Mẫu báo cáo"
          field="report_name"
          class="align-items-center justify-content-center text-center"
        >
        </Column>
        <Column
          field="status"
          header="Hiển thị"
          class="align-items-center justify-content-center text-center max-w-6rem"
        >
          <template #body="data">
            <Checkbox
              :binary="data.data.status"
              v-model="data.data.status"
              :disabled="
                data.data.organization_id == user.organization_id ? false : true
              "
              @click="onCheckBox(data.data)"
            />
          </template>
        </Column>

        <Column
          field="is_system"
          header="Hệ thống"
          class="align-items-center justify-content-center text-center max-w-7rem"
        >
          <template #body="data">
            <div v-if="data.data.is_system == true">
              <i
                class="pi pi-check text-blue-400"
                style="font-size: 1.5rem"
              ></i>
            </div>
            <div v-else></div>
          </template>
        </Column>
        <Column
          field="organization_name"
          header="Đơn vị"
          class="align-items-center justify-content-center text-center min-w-25rem"
        ></Column>
        <Column
          header="Chức năng"
          field=""
          class="align-items-center justify-content-center text-center max-w-7rem"
        >
          <template #body="data">
            <div
              v-if="
                user.organization_id == data.data.organization_id &&
                user.is_admin
              "
            >
              <Button
                @click="EditData(data.data, data.index)"
                class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                type="button"
                icon="pi pi-pencil"
                v-tooltip="'Sửa'"
              ></Button>
              <Button
                @click="DelData(data.data, 'single')"
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
      <div class="col-4 flex align-items-center justify-content-center">
        Tên báo cáo <span class="redsao">(*)</span>
      </div>

      <div class="col-4 flex align-items-center justify-content-center">
        Mẫu báo cáo<span class="redsao">(*)</span>
      </div>
      <div class="col-3 flex align-items-center justify-content-center">
        <div class="col-6 flex align-items-center justify-content-center">
          Trạng thái
        </div>
        <div
          class="col-6 flex align-items-center justify-content-center"
          v-if="user.is_super"
        >
          Hệ thống
        </div>
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
          class="col-4"
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

        <div class="col-3 flex align-items-center justify-content-center">
          <div class="col-6 flex align-items-center justify-content-center">
            <InputSwitch v-model="item.status" />
          </div>
          <div
            class="col-6 flex align-items-center justify-content-center"
            v-if="user.is_super"
          >
            <InputSwitch v-model="item.is_system" />
          </div>
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
          <small
            v-if="
              item.report_title &&
              item.report_title.toString().length > 250 &&
              submitted
            "
            :class="{
              'p-text-invalid': !item.report_title && submitted,
            }"
          >
            Tên báo cáo không quá 250 kí tự!</small
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
