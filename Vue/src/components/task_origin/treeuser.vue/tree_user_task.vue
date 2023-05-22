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

const listDepartments = ref();

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
            proc: "sys_organization_to_task_tree",
            par: [
              // { par: "pageno", va: opition.value.PageNo },
              // { par: "pagesize", va: opition.value.PageSize },
              // { par: "search", va: opition.value.search },
              // { par: "organization_type", va: opition.value.organization_type },
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
        listDepartments.value = obj.arrtreeChils;
        listDepartments.value.forEach((element) => {
          expandNode(element);
        });
      } else {
        listDepartments.value = [];
      }
      if (rf) {
        swal.close();
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");

      addLog({
        title: "Lỗi Console loadData",
        controller: "LogsView.vue",
        log_content: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
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

const loadUsers = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_user_list_tree_task",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              {
                par: "filter_organization_id",
                va: options.value.filter_organization_id,
              },
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
          listUsers.value = tbs[0].map((x) => ({
            user_id: x.user_id,
            full_name: x.full_name,
            full_name_en: x.full_name_en,
            is_order: x.is_order,
            user_key: x.user_key,
            last_name: x.last_name,
            avatar: x.avatar,
            organization_id: x.organization_id,
            position_id: x.position_id,
            position_name: x.position_name,
            department_id: x.department_id,
            organization_name: x.organization_name,
            STT: x.STT,
          }));
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
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  one: Boolean,
  selected: Array,
  choiceUser: Function,
});

onMounted(() => {
  loadData(true);
  loadUsers();

  displayDepartment.value = true;
  selectedNodeUser.value = props.selected;

  return {};
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="displayDepartment"
    :style="{ width: '50vw' }"
    :closable="true"
    :maximizable="true"
    @hide="props.closeDialog"
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
          :selectionMode="props.one == true ? '' : 'checkbox'"
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
            :selectionMode="props.one == true ? 'single' : 'multiple'"
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
        @click="props.closeDialog()"
        class="p-button-text"
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="props.choiceUser(selectedNodeUser)"
      />
    </template>
  </Dialog>
</template>
<style lang="scss" scoped>
.d-lang-table {
  height: calc(100vh - 350px);
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
