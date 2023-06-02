<!-- eslint-disable no-undef -->
<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../util/function.js";
import { FilterMatchMode, FilterOperator } from "primevue/api";
const cryoptojs = inject("cryptojs");
const toast = useToast();
// eslint-disable-next-line no-undef
const basedomainURL = fileURL;
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const addLog = (log) => {
  // eslint-disable-next-line no-undef
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const Department = {
  id: null,
  department_id: null,
  user_id: null,
};
const listDepartments = ref();

const headerAddDepartment = ref();
const listUsers = ref([]);
const displayDepartment = ref(false);
const opition = ref({
  IsNext: true,
  sort: "created_date",
  ob: "DESC",
  PageNo: 1,
  PageSize: 20,
  search: "",
  Filteruser_id: null,
  organization_type: null,
  user_id: store.getters.user_id,
});

const renderTree = (data, id, name, title) => {
  let arrChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em, index) => {
            em.label_order = em.is_order;
            em.STT = mm.data.STT ? mm.data.STT + "." + (index + 1) : index + 1;
            let om1 = { key: em[id], data: em };
            rechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m[id]);
      arrChils.push(om);
    });

  return { arrChils: arrChils[0].children, arrtreeChils: arrChils };
};
const datalist = ref([]);
const listDepartmentsTemp = ref([]);
const loadData = (rf) => {
  if (rf) {
    opition.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      // eslint-disable-next-line no-undef
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_organization_list_doc_role_department",
            par: [
              { par: "pageno", va: opition.value.PageNo },
              { par: "pagesize", va: opition.value.PageSize },
              { par: "search", va: opition.value.search },
              { par: "organization_type", va: opition.value.organization_type },
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
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        data[0].forEach((d) => {
          if (d.ThanhvienRole != null)
            d.ThanhvienRole = JSON.parse(d.ThanhvienRole);
        });
        listDepartmentsTemp.value = [...data[0]];
        let obj = renderTree(
          data[0],
          "organization_id",
          "organization_name",
          "đơn vị",
        );
        datalist.value = obj.arrChils;
        listDepartments.value = obj.arrtreeChils;
        datalist.value.forEach((element) => {
          expandNodeMain(element);
        });
        listDepartments.value.forEach((element) => {
          expandNode(element);
        });

        // opition.value.totalRecords = data[1][0].totalrecords;
      } else {
        listDepartments.value = [];
      }
      if (rf) {
        opition.value.loading = false;
        swal.close();
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "LogsView.vue",
        log_content: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const listdel = ref([]);
const DelDepartmentUser = (data) => {
  listdel.value = [];
  listdel.value.push(data.organization_id);

  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá người duyệt của phòng ban này không!",
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
          .delete(baseURL + "/api/Doc_Role_Department/DeleteUser", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listdel.value != null ? listdel.value : 0,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá người duyệt phòng ban thành công!");
              loadData(true);
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
            swal.close();
            if (error.status === 401) {
              swal.fire({
                title: "Thông báo!",
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

const updateDepartmentUser = (data) => {
  goOrganization(data);
  selectedNodeUser.value = [];
  if (data.data.ThanhvienRole != null) {
    data.data.ThanhvienRole.forEach((x) => {
      let u = listUsers.value.filter((z) => z.user_id == x.user_id);
      selectedNodeUser.value.push(u[0]);
    });
  }
  Department.value = {
    id: -1,
    department_id: data.data.organization_id,
    user_id: null,
  };
  headerAddDepartment.value =
    "Cập nhật người duyệt phòng ban (" + data.data.organization_name + ")";
  displayDepartment.value = true;
};

const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);

const closeDialogDepartment = () => {
  displayDepartment.value = false;
};

const saveDepartmentUser = () => {
  let formData = new FormData();
  let listsend = [];
  if (selectedNodeUser.value) {
    if (selectedNodeUser.value.length > 0) {
      selectedNodeUser.value.forEach((t) => {
        console.log(t);
        Department.value.user_id = t.user_id;
        let dept = {
          department_id: Department.value.department_id,
          user_id: t.user_id,
        };
        listsend.push(dept);
      });
    }
  }

  formData.append("group", JSON.stringify(listsend));
  axios
    .put(baseURL + "/api/Doc_Role_Department/Update_user", formData, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        loadData(true);
        closeDialogDepartment();
        toast.success("Cập nhật phòng ban thành công!");
      } else {
        swal.fire({
          title: "Thông báo!",
          html: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch(() => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const loadUsers = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "[sys_user_list_tree]",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              {
                par: "filter_organization_id",
                va: options.value.filter_organization_id,
              },
              { par: "page_no", va: options.value.pageNo },
              { par: "page_size", va: options.value.pageSize },
              { par: "search", va: options.value.search },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      var data = response.data.data;
      if (data != null) {
        let tbs = JSON.parse(data);

        if (tbs[0] != null && tbs[0].length > 0) {
          tbs[0].forEach((element, i) => {
            if (element["created_date"] != null) {
              var ldate = element["created_date"].split(" ");
              element["created_date"] = ldate[0];
            }
          });
          if (tbs[0].length > 0) {
            tbs[0].forEach((element, i) => {
              element["STT"] = i + 1;
            });
          }
          listUsers.value = tbs[0];
        }
        swal.close();
      }
    })
    .catch((error) => {
      if (options.value.loading) options.value.loading = false;
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    });
};

const selectedKey = ref();
const isFirst = ref(false);
const options = ref({});
const expandedKeys = ref([]);
const selectedNodeOrganization = ref();
const expandNode = (node) => {
  if (node.children && node.children.length) {
    expandedKeys.value[node.key] = true;
    for (let child of node.children) {
      expandNode(child);
    }
  }
};
const expandedKeysMain = ref([]);
const expandNodeMain = (node) => {
  if (node.children && node.children.length) {
    expandedKeysMain.value[node.key] = true;
    for (let child of node.children) {
      expandNode(child);
    }
  }
};
const selectedNodeUser = ref([]);
const filters1 = ref({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  department_id: { value: [], matchMode: FilterMatchMode.IN },
});
const goOrganization = (organization) => {
  filters1.value["department_id"].value = [];
  options.value.filter_organization_id = organization.data.organization_id;
  if (organization.data.parent_id != null) {
    filters1.value["department_id"].value.push(
      organization.data.organization_id,
    );
    if (organization.children != null) {
      organization.children.forEach((x) => {
        filters1.value["department_id"].value.push(x.data.organization_id);
      });
    }
  } else filters1.value["department_id"].value = [];
};

const changeOrganizationChecked = () => {
  let choses = selectedNodeOrganization.value;
  filters1.value["department_id"].value = [];
  for (var o in choses) {
    if (choses[o]["checked"] == true) {
      let organization = listDepartmentsTemp.value.find(
        (x) => x["organization_id"] === parseInt(o),
      );
      filters1.value["department_id"].value.push(parseInt(o));
      let filters = [];
      switch (organization["organization_type"]) {
        case 0:
          filters = listUsers.value.filter(
            (a) =>
              a["organization_id"] === organization["organization_id"] &&
              selectedNodeUser.value.findIndex(
                (b) => b["user_id"] === a["user_id"],
              ) === -1,
          );
          selectedNodeUser.value = selectedNodeUser.value.concat(filters);
          break;
        case 1:
          filters = listUsers.value.filter(
            (a) =>
              a["department_id"] === organization["organization_id"] &&
              selectedNodeUser.value.findIndex(
                (b) => b["user_id"] === a["user_id"],
              ) === -1,
          );

          selectedNodeUser.value = selectedNodeUser.value.concat(filters);
          break;
      }
    }
  }
  let notexists = listDepartmentsTemp.value.filter(
    (a) =>
      selectedNodeOrganization.value[a["organization_id"].toString()] == null,
  );
  if (notexists.length > 0) {
    for (var i = 0; i < notexists.length; i++) {
      var deleted = [];
      switch (notexists[i]["organization_type"]) {
        case 0:
          deleted = listUsers.value.filter(
            (a) =>
              a["organization_id"] === notexists[i]["organization_id"] &&
              selectedNodeUser.value.findIndex(
                (b) => b["user_id"] === a["user_id"],
              ) !== -1,
          );
          break;
        case 1:
          deleted = listUsers.value.filter(
            (a) =>
              a["department_id"] === notexists[i]["organization_id"] &&
              selectedNodeUser.value.findIndex(
                (b) => b["user_id"] === a["user_id"],
              ) !== -1,
          );
          break;
      }
      for (var j = 0; j < deleted.length; j++) {
        var idx = selectedNodeUser.value.findIndex(
          (x) => x["user_id"] === deleted[j]["user_id"],
        );
        if (idx != -1) {
          selectedNodeUser.value = selectedNodeUser.value.splice(idx, 0);
        }
      }
    }
  }
};
watch(selectedNodeOrganization, () => {
  changeOrganizationChecked();
});
const filters2 = ref({});
// const SearchBytext = () => {
//   if (options.value.SearchText != null) {
//     filters2.value["global"] = options.value.SearchText;
//   } else filters2.value["global"] = "";
// };

onMounted(() => {
  loadData(true);
  loadUsers();
  return {};
});
</script>
<template>
  <div
    v-if="store.getters.islogin"
    class="main-layout true flex-grow-1 p-2"
  >
    <TreeTable
      :value="datalist"
      :expandedKeys="expandedKeysMain"
      v-model:selectionKeys="selectedKey"
      :loading="opition.loading"
      :showGridlines="true"
      filterMode="strict"
      class="p-treetable-sm"
      :rowHover="true"
      responsiveLayout="scroll"
      :lazy="true"
      :scrollable="true"
      scrollHeight="flex"
    >
      <template #header>
        <h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
          <i class="pi pi-microsoft"></i> Danh sách phòng ban
        </h3>
        <Toolbar class="w-full custoolbar">
          <!-- <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                type="text"
                spellcheck="false"
                v-model="opition.search"
                placeholder="Tìm kiếm theo tên phòng ban"
                v-on:keyup.enter="loadData(true)"
              />
            </span>
          </template> -->
        </Toolbar>
      </template>
      <Column
        field="STT"
        header="STT"
        class="align-items-center justify-content-center text-center font-bold"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
      >
      </Column>
      <Column
        field="organization_name"
        header="Tên phòng ban"
        :expander="true"
        headerClass="align-items-center justify-content-center text-center"
      >
        <template #body="md">
          <div style="display: flex; align-items: center">
            <span style="margin-left: 5px">{{
              md.node.data.organization_name
            }}</span>
          </div>
        </template>
      </Column>
      <Column
        header="Người duyệt"
        headerStyle="text-align:center;max-width:10rem;height:50px"
        bodyStyle="text-align:center;max-width:10rem"
        class="align-items-center justify-content-center text-center"
      >
        <template #body="data">
          <AvatarGroup>
            <div
              v-for="(value, index) in data.node.data.ThanhvienRole"
              :key="index"
            >
              <Avatar
                v-if="index < 3"
                v-tooltip.bottom="{
                  value:
                    value.full_name +
                    '<br/>' +
                    (value.position_name || '') +
                    '<br/>' +
                    (value.department_name ||
                      value.organization_child_name ||
                      value.organization_name),
                  escape: true,
                }"
                v-bind:label="
                  value.avatar ? '' : (value.full_name ?? '').substring(0, 1)
                "
                v-bind:image="basedomainURL + value.avatar"
                style="
                  background-color: #2196f3;
                  color: #ffffff;
                  width: 32px;
                  height: 32px;
                  font-size: 15px !important;
                  margin-left: -10px;
                "
                :style="{
                  background: bgColor[index % 7] + '!important',
                }"
                class="cursor-pointer"
                size="xlarge"
                shape="circle"
              />
            </div>
            <Avatar
              v-if="
                data.node.data.ThanhvienRole != null &&
                data.node.data.ThanhvienRole.length > 4
              "
              v-tooltip.right="{
                value:
                  'và ' +
                  (data.node.data.ThanhvienRole.length - 3) +
                  ' người khác',
              }"
              :label="'+' + (data.node.data.ThanhvienRole.length - 3)"
              style="
                background-color: #2196f3;
                color: #ffffff;
                width: 32px;
                height: 32px;
                font-size: 15px !important;
                margin-left: -10px;
              "
              :style="{
                background: bgColor[index % 7] + '!important',
              }"
              class="cursor-pointer"
              size="xlarge"
              shape="circle"
            />
          </AvatarGroup>
        </template>
      </Column>
      <Column
        header="Chức năng"
        headerClass="text-center"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
      >
        <template #header> </template>
        <template #body="md">
          <div v-if="md.node.data.parent_id != null">
            <Button
              type="button"
              icon="pi pi-user-plus"
              v-tooltip.top="'Cập nhật người duyệt'"
              class="p-button-rounded p-button-secondary p-button-outlined"
              style="margin-right: 0.5rem"
              @click="updateDepartmentUser(md.node)"
            ></Button>
            <Button
              v-if="md.node.data.ThanhvienRole"
              type="button"
              icon="pi pi-trash"
              v-tooltip.top="'Xóa người duyệt'"
              class="p-button-rounded p-button-secondary p-button-outlined"
              @click="DelDepartmentUser(md.node.data)"
            ></Button>
          </div>
        </template>
      </Column>
      <template #empty>
        <div
          class="align-items-center justify-content-center p-4 text-center m-auto"
          style="
            min-height: calc(100vh - 220px);
            max-height: calc(100vh - 220px);
            display: flex;
            flex-direction: column;
          "
          v-if="!isFirst"
        >
          <img
            src="../../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </TreeTable>
  </div>
  <Dialog
    :header="headerAddDepartment"
    v-model:visible="displayDepartment"
    :style="{ width: '50vw' }"
    :closable="true"
    :maximizable="true"
  >
    <form class="flex col-12">
      <div class="col-5 md:col-5">
        <Toolbar
          class="outline-none surface-0 border-none px-0 pt-0 custom-search"
        >
          <template #start>
            <span
              class="p-input-icon-left"
              style="width: 100%"
            >
              <i class="pi pi-search" />
              <!-- v-model="options.SearchText"
                @keyup.enter="SearchBytext()" -->
              <InputText
                type="text"
                v-model="filters2['global']"
                spellcheck="false"
                placeholder="Tìm kiếm phòng ban"
                style="width: 100%"
              />
            </span>
          </template>
        </Toolbar>
        <TreeTable
          :value="listDepartments"
          v-model:filters="filters2"
          filterMode="lenient"
          :scrollable="true"
          :rowHover="true"
          :expandedKeys="expandedKeys"
          dataKey="organization_id"
          v-model:selectionKeys="selectedNodeOrganization"
          :selectionMode="'checkbox'"
          scrollHeight="flex"
          filterDisplay="menu"
          class="d-lang-table"
          globalFilterFields="['organization_id', 'organization_name']"
        >
          <Column
            field="organization_name"
            header="Đơn vị"
            :expander="true"
            style="min-width: 200px"
          >
            <template #body="slotProps">
              <span
                @click="goOrganization(slotProps.node)"
                :class="
                  slotProps.node.data.organization_id ===
                    options.filter_organization_id ||
                  (options.filter_organization_id == null &&
                    slotProps.node.data.parent_id == null)
                    ? 'row-active'
                    : ''
                "
                >{{ slotProps.node.data.organization_name }}
              </span>
            </template>
          </Column>
        </TreeTable>
      </div>
      <div class="col-7 md:col-7">
        <Toolbar
          class="outline-none surface-0 border-none px-0 pt-0 custom-search"
        >
          <template #start>
            <span
              class="p-input-icon-left"
              style="width: 100%"
            >
              <i class="pi pi-search" />
              <InputText
                type="text"
                v-model="filters1['global'].value"
                spellcheck="false"
                placeholder=" Tìm kiếm người dùng"
                style="width: 100%"
              />
            </span>
          </template>
        </Toolbar>
        <DataTable
          :value="listUsers"
          class="d-lang-table"
          :scrollable="true"
          dataKey="user_id"
          v-model:selection="selectedNodeUser"
          scrollHeight="flex"
          rowGroupMode="subheader"
          groupRowsBy="department_id"
          removableSort
          v-model:filters="filters1"
          :globalFilterFields="['user_id', 'full_name']"
        >
          <template #groupheader="slotProps">
            <i class="pi pi-list mr-2"></i
            >{{ slotProps.data.organization_name ?? "Không thuộc phòng ban" }}
          </template>
          <Column
            :selectionMode="'multiple'"
            headerStyle="text-align:center;max-width:50px"
            bodyStyle="text-align:center;max-width:50px"
            class="align-items-center justify-content-center text-center"
          ></Column>
          <Column
            field="full_name"
            header="Họ và tên"
            headerStyle="height:50px;max-width:auto;"
            bodyStyle="max-height:60px"
            :sortable="true"
          >
            <template #body="slotProps">
              <div class="flex">
                <div class="format-center">
                  <Avatar
                    v-bind:label="
                      slotProps.data.avatar
                        ? ''
                        : slotProps.data.last_name.substring(0, 1)
                    "
                    v-bind:image="basedomainURL + slotProps.data.avatar"
                    style="
                      background-color: #2196f3;
                      color: #ffffff;
                      width: 3rem;
                      height: 3rem;
                    "
                    :style="{
                      background: bgColor[slotProps.data.STT % 7],
                    }"
                    class="mr-2 text-avatar"
                    size="xlarge"
                    shape="circle"
                  />
                </div>
                <div class="format-center justify-content-left ml-3">
                  <div>
                    <div style="text-align: left">
                      {{ slotProps.data.full_name }}
                    </div>
                    <div
                      class="description"
                      style="text-align: left"
                    >
                      {{ slotProps.data.position_name }}
                    </div>
                  </div>
                </div>
              </div>
            </template>
          </Column>
        </DataTable>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogDepartment()"
        class="p-button-text"
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveDepartmentUser()"
      />
    </template>
  </Dialog>
</template>
<style lang="scss" scoped>
.d-lang-table {
  min-height: unset;
  max-height: calc(100vh - 350px);
}
::v-deep(.text-avatar) {
  .p-avatar-text {
    font-size: 1.5rem;
  }
}

::v-deep(.custom-search) {
  .p-toolbar-group-left {
    width: 100%;
  }
  input {
    width: 100%;
  }
}
.row-active {
  color: rgb(13, 137, 236);
}
</style>
