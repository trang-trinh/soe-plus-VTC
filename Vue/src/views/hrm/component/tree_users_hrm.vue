<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { de } from "date-fns/locale";
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const emitter = inject("emitter");
const isDynamicSQL = ref(false);
const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const options = ref({
  isNext: true,
  loading: false,
  user_id: store.getters.user.user_id,
  filter_organization_id: store.getters.user.organization_id,
  search: "",
  pageNo: 0,
  pageSize: 20,
  total: 0,
  sort: "profile_user_name asc",
  orderBy: "asc",
});
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);

//Get arguments
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  one: Boolean,
  selected: Array,
  choiceUser: Function,
});

//Declare Model
const isFirst = ref(true);
const rootorganizations = ref([]);
const temporganizations = ref([]);
const rootusers = ref([]);
const filterusers = ref([]);
const tempusers = ref([]);
const expandedKeys = ref({});

//watch chose
const selectedNodeOrganization = ref([]);
const selectedNodeUser = ref([]);

watch(selectedNodeOrganization, () => {
  changeOrganizationChecked();
});
watch(selectedNodeUser, () => {
  changeUserChecked();
});

//Function chose
const changeOrganizationChecked = () => {
  let choses = selectedNodeOrganization.value;
  for (var o in choses) {
    if (choses[o]["checked"]) {
      let organization = rootorganizations.value.find(
        (x) => x["organization_id"] === parseInt(o)
      );
      switch (organization["organization_type"]) {
        case 0:
          var filters = rootusers.value.filter(
            (a) =>
              a["organization_id"] === organization["organization_id"] &&
              selectedNodeUser.value.findIndex(
                (b) => b["profile_id"] === a["profile_id"]
              ) === -1
          );
          selectedNodeUser.value = selectedNodeUser.value.concat(filters);
          break;
        case 1:
          var filters = rootusers.value.filter(
            (a) =>
              a["department_id"] === organization["organization_id"] &&
              selectedNodeUser.value.findIndex(
                (b) => b["profile_id"] === a["profile_id"]
              ) === -1
          );
          selectedNodeUser.value = selectedNodeUser.value.concat(filters);
          break;
      }
    }
  }
  let notexists = rootorganizations.value.filter(
    (a) =>
      selectedNodeOrganization.value[a["organization_id"].toString()] == null
  );
  if (notexists.length > 0) {
    for (var i = 0; i < notexists.length; i++) {
      var deleted = [];
      switch (notexists[i]["organization_type"]) {
        case 0:
          deleted = rootusers.value.filter(
            (a) =>
              a["organization_id"] === notexists[i]["organization_id"] &&
              selectedNodeUser.value.findIndex(
                (b) => b["profile_id"] === a["profile_id"]
              ) !== -1
          );
          break;
        case 1:
          deleted = rootusers.value.filter(
            (a) =>
              a["department_id"] === notexists[i]["organization_id"] &&
              selectedNodeUser.value.findIndex(
                (b) => b["profile_id"] === a["profile_id"]
              ) !== -1
          );
          break;
      }
      for (var j = 0; j < deleted.length; j++) {
        var idx = selectedNodeUser.value.findIndex(
          (x) => x["profile_id"] === deleted[j]["profile_id"]
        );
        if (idx != -1) {
          selectedNodeUser.value = selectedNodeUser.value.splice(idx, 0);
        }
      }
    }
  }
};
const changeUserChecked = () => {
  switch (props.one) {
    case true:
      props.selected.splice(0, 1);
      props.selected.push(selectedNodeUser.value);
      break;
    case false:
      var choses = selectedNodeUser.value.filter(
        (a) =>
          props.selected.findIndex(
            (b) => b["profile_id"] === a["profile_id"]
          ) === -1
      );
      if (choses.length > 0) {
        Array.prototype.push.apply(props.selected, choses);
      }
      let notexists = props.selected.filter(
        (a) =>
          selectedNodeUser.value.findIndex(
            (b) => b["profile_id"] === a["profile_id"]
          ) === -1
      );
      if (notexists.length > 0) {
        for (var i = 0; i < notexists.length; i++) {
          let idx = props.selected.findIndex(
            (a) => a["profile_id"] === notexists[i]["profile_id"]
          );
          if (idx != -1) {
            props.selected.splice(idx, 1);
          }
        }
      }
      break;
  }
};
const goOrganization = (organization) => {
  options.value.filter_organization_id = organization.data["organization_id"];
  let listchilds = [];
  if (
    organization.data["IDChild"] != null &&
    organization.data["IDChild"] != ""
  ) {
    listchilds = organization.data["IDChild"]
      .split(",")
      .filter((x) => x !== "")
      .map((x) => parseInt(x));
  }
  switch (organization.data["organization_type"]) {
    case 0:
      filterusers.value = rootusers.value.filter(
        (a) =>
          a["organization_id"] === organization.data["organization_id"] ||
          listchilds.findIndex(
            (b) => b["organization_id"] === a["organization_id"]
          ) !== -1 ||
          a["department_id"] === organization.data["organization_id"] ||
          listchilds.findIndex(
            (b) => b["organization_id"] === a["department_id"]
          ) !== -1
      );
      break;
    case 1:
      filterusers.value = rootusers.value.filter(
        (a) =>
          a["department_id"] === organization.data["organization_id"] ||
          listchilds.findIndex(
            (b) => b["organization_id"] === a["department_id"]
          ) !== -1
      );
      break;
  }
  tempusers.value = [...filterusers.value];
};

//Function tree
const renderTree = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];

  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
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
    });

  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const expandNode = (node) => {
  if (node.children && node.children.length) {
    expandedKeys.value[node.key] = true;
    for (let child of node.children) {
      expandNode(child);
    }
  }
};

//Function filter
const searchUser = () => {
  var s = options.value.search.toLowerCase();
  tempusers.value = filterusers.value.filter(
    (x) =>
      (x["profile_user_name"] || "").toLowerCase().indexOf(s) !== -1 ||
      (x["profile_user_name_en"] || "").toLowerCase().indexOf(s) !== -1 ||
      (x["last_name"] || "").toLowerCase().indexOf(s) !== -1
  );
};

//Init
const onRefresh = () => {
  options.value.search = "";
};
const initOrganization = () => {
  options.value.loading = true;
  if (store.getters.listOrgTree.length > 0) {
    temporganizations.value = store.getters.listOrgTree;
    temporganizations.value.forEach((element) => {
      expandNode(element);
    });
    initUser(true);
    options.value.loading = false;
  } else {
    axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "sys_organization_listtree",
          par: [
            { par: "user_id", va: store.getters.user.user_id },
            { par: "search", va: options.value.search },
          ],
        },
        config
      )
      .then((response) => {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          debugger
          if (tbs[0] != null && tbs[0].length > 0) {
            rootorganizations.value = [...tbs[0]];
            let obj = renderTree(
              tbs[0],
              "organization_id",
              "organization_name",
              "Đơn vị"
            );
            if (store.getters.listOrgTree.length == 0) {
              store.commit("setlistOrgTree", obj.arrChils);
            }
            temporganizations.value = obj.arrChils;
            temporganizations.value.forEach((element) => {
              expandNode(element);
            });
          } else {
            rootorganizations.value = [];
            temporganizations.value = [];
          }
          initUser(true);
          options.value.loading = false;
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
        options.value.loading = false;
        return;
      });
  }
};
const initUser = (rf) => {
  if (rf) {
    options.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  if (isDynamicSQL.value) {
    initUserSQL();
    return;
  }

  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "hrm_profile_list_all",
        par: [{ par: "user_id", va: store.getters.user.user_id }],
      },
      config
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
              element["is_check"] = false;
            });
          }
          rootusers.value = tbs[0].filter(
            (a) =>
              props.selected.findIndex(
                (b) => b["profile_id"] === a["profile_id"]
              ) === -1
          );
          tempusers.value = [...rootusers.value];
          filterusers.value = [...tempusers.value];
        } else {
          rootusers.value = [];
          tempusers.value = [];
          filterusers.value = [];
        }
        options.value.total = rootusers.value.length;
      }
      swal.close();
      if (isFirst.value) isFirst.value = false;
      if (options.value.loading) options.value.loading = false;
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
const initRender = () => {
  setTimeout(function () {
    selectedNodeUser.value = props.selected;
  }, 1000);
};
const filterSQL = ref();
const initUserSQL = () => {
  let par = {
    filter_organization_id: store.getters.user.organization_id,
    page_no: options.value.pageNo,
    page_size: options.value.pageSize,
    search: options.value.search,
    fields: filterSQL.value,
    order_by: options.value.sort,
  };
  axios
    .post(baseURL + "/api/tree_user/filter_sys_user_listtree", par, config)
    .then((response) => {
      var data = response.data;
      if (data != null) {
        if (data["err"] != "0") {
          swal.fire({
            title: "Error!",
            text:
              data["err"] == "2"
                ? response.data.ms
                : "Lọc dữ liệu không thành công.",
            icon: "error",
            confirmButtonText: "OK",
          });
          return;
        }

        let tbs = JSON.parse(data.data);
        if (tbs[0] != null && tbs[0].length > 0) {
          tbs[0].forEach((element, i) => {
            if (element["created_date"] != null) {
              var ldate = element["created_date"].split(" ");
              element["created_date"] = ldate[0];
            }
          });

          datas.value = tbs[0];
          if (datas.value.length > 0) {
            datas.value.forEach((element, i) => {
              element["STT"] = i + 1;
            });
          }
        } else {
          datas.value = [];
        }
        if (tbs.length == 2) {
          options.value.total = tbs[1][0].total;
        }
        swal.close();
        if (isFirst.value) isFirst.value = false;
        if (options.value.loading) options.value.loading = false;
      }
    })
    .catch((error) => {
      options.value.loading = false;
      toast.error("Tải dữ liệu không thành công!");

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const displayDialog = ref(false);
onMounted(() => {
  initOrganization();
  displayDialog.value = props.displayDialog;
  return {};
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="displayDialog"
    :style="{ width: '60%' }"
    :modal="true"
    :closable="true"
    @hide="props.closeDialog"
    style="z-index: 9999999"
  >
    <form @submit.prevent="">
      <div class="grid formgrid m-2">
        <div class="col-5 md:col-5">
          <TreeTable
            :value="temporganizations"
            :scrollable="true"
            :rowHover="true"
            :loading="options.loading"
            :expandedKeys="expandedKeys"
            :lazy="true"
            dataKey="organization_id"
            v-model:selectionKeys="selectedNodeOrganization"
            :selectionMode="'single'"
            filterMode="strict"
            scrollHeight="flex"
            filterDisplay="menu"
            class="d-lang-table"
            @node-select="goOrganization($event)"
          >
            <Column
              field="organization_name"
              header="Đơn vị"
              :expander="true"
              style="min-width: 200px"
            >
              <template #body="slotProps">
                <div
                  :class="
                    slotProps.node.data.organization_id ===
                      options.filter_organization_id ||
                    (options.filter_organization_id == null &&
                      slotProps.node.data.parent_id == null)
                      ? 'row-active'
                      : ''
                  "
                  class="py-1"
                  style="width: -webkit-fill-available"
                >
                  <span>{{ slotProps.node.data.organization_name }}</span>
                </div>
              </template>
            </Column>
          </TreeTable>
        </div>
        <div class="col-7 md:col-7">
          <Toolbar
            class="outline-none surface-0 border-none px-0 pt-0 custom-search"
          >
            <template #start>
              <span class="p-input-icon-left" style="width: 100%">
                <i class="pi pi-search" />
                <InputText
                  @input="searchUser"
                  v-model="options.search"
                  type="text"
                  spellcheck="false"
                  placeholder=" Tìm kiếm người dùng"
                  style="width: 100%"
                />
              </span>
            </template>
          </Toolbar>
          <DataTable
            :value="tempusers"
            :loading="options.loading"
            :scrollable="true"
            :rowHover="true"
            :totalRecords="options.total"
            :selectionMode="props.one ? 'single' : ''"
            :lazy="true"
            dataKey="profile_id"
            v-model:selection="selectedNodeUser"
            scrollHeight="flex"
            filterDisplay="menu"
            filterMode="lenient"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
            responsiveLayout="scroll"
            rowGroupMode="subheader"
            groupRowsBy="position_id"
            class="d-lang-table"
          >
            <template #groupheader="slotProps">
              <i class="pi pi-list mr-2"></i>{{ slotProps.data.position_name }}
            </template>
            <Column
              :selectionMode="props.one ? 'single' : 'multiple'"
              headerStyle="text-align:center;max-width:50px"
              bodyStyle="text-align:center;max-width:50px"
              class="align-items-center justify-content-center text-center"
            ></Column>
            <Column
              field="profile_user_name"
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
                        {{ slotProps.data.profile_user_name }}
                      </div>
                      <div class="description" style="text-align: left">
                        {{ slotProps.data.role_name }}
                      </div>
                    </div>
                  </div>
                </div>
              </template>
            </Column>
          </DataTable>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="props.closeDialog()"
        class="p-button-outlined"
      />
      <Button label="Chọn" icon="pi pi-check" @click="props.choiceUser()" />
    </template>
  </Dialog>
</template>
<style scoped>
.d-lang-table {
  min-height: unset;
  max-height: calc(100vh - 300px);
}

.format-center {
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
}

.row-active {
  color: rgb(13, 137, 236);
}

.description {
  color: #aaa;
  font-size: 12px;
}
</style>
<style lang="scss" scoped>
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
</style>