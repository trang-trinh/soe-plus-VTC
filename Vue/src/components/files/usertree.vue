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
  sort: "full_name asc",
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
  one: Boolean,
  selected: Intl,
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
const selectedNodeUser = ref([]);

watch(selectedNodeUser, () => {
  changeUserChecked();
});
//Function chose
const changeUserChecked = () => {
  debugger;
  tempusers.value.forEach((item) => {
    if (selectedNodeUser.value.length > 0) item.is_check = true;
    else item.is_check = false;
  });
};
const goOrganization = (organization) => {
  options.value.filter_organization_id = organization["organization_id"];
  let listchilds = [];
  if (organization["IDChild"] != null && organization["IDChild"] != "") {
    listchilds = organization["IDChild"]
      .split(",")
      .filter((x) => x !== "")
      .map((x) => parseInt(x));
  }
  switch (organization["organization_type"]) {
    case 0:
      filterusers.value = rootusers.value.filter(
        (a) =>
          a["organization_id"] === organization["organization_id"] ||
          listchilds.findIndex(
            (b) => b["organization_id"] === a["organization_id"]
          ) !== -1 ||
          a["department_id"] === organization["organization_id"] ||
          listchilds.findIndex(
            (b) => b["organization_id"] === a["department_id"]
          ) !== -1
      );
      break;
    case 1:
      filterusers.value = rootusers.value.filter(
        (a) =>
          a["department_id"] === organization["organization_id"] ||
          listchilds.findIndex(
            (b) => b["organization_id"] === a["department_id"]
          ) !== -1
      );
      break;
  }
  debugger;
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
      (x["full_name"] || "").toLowerCase().indexOf(s) !== -1 ||
      (x["full_name_en"] || "").toLowerCase().indexOf(s) !== -1 ||
      (x["last_name"] || "").toLowerCase().indexOf(s) !== -1
  );
};

//Function choice
const choice = () => {
  emitter.emit("emitData", {
    type: "choiceusers",
    data: {
      submit: true,
      displayDialog: false,
      selected: rootusers.value.filter((x) => x.is_check),
    },
  });
};
const cancel = () => {
  props.value = {};
  emitter.emit("emitData", {
    type: "choiceusers",
    data: {
      submit: false,
      displayDialog: false,
    },
  });
};

//Init
const onRefresh = () => {
  options.value.search = "";
};
const initOrganization = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "file_organization_listtree",
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

        if (tbs[0] != null && tbs[0].length > 0) {
          rootorganizations.value = [...tbs[0]];
          let obj = renderTree(
            tbs[0],
            "organization_id",
            "organization_name",
            "Đơn vị"
          );
          temporganizations.value = obj.arrChils;
          temporganizations.value.forEach((element) => {
            expandNode(element);
          });
        } else {
          rootorganizations.value = [];
          temporganizations.value = [];
        }
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
const initUser = (id) => {
  debugger
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "file_share_user_list",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          {
            par: "filter_organization_id",
            va: options.value.filter_organization_id,
          },
          { par: "page_no", va: options.value.pageNo },
          { par: "page_size", va: options.value.pageSize },
          { par: "search", va: options.value.search },
          { par: "file_id", va: id },
        ],
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
              //element["is_check"] = false;
            });
          }
          rootusers.value = tbs[0];
          tempusers.value = [...tbs[0]];
        } else {
          rootusers.value = [];
          tempusers.value = [];
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
const onCheckBox = () => {
  debugger;
};
onMounted(() => {
  debugger
  initOrganization();
  initUser(props.selected);
  //initRender();
  return {};
});
</script>

<template>
  <Dialog
    :header="
      props.headerDialog +
      ' (' +
      rootusers.filter((x) => x.is_check).length +
      ' người dùng)'
    "
    v-model:visible="props.displayDialog"
    :style="{ width: '60%' }"
    :modal="true"
    :closable="false"
    style="z-index: 999"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-5 md:col-5">
          <TreeTable
            :value="temporganizations"
            :scrollable="true"
            :rowHover="true"
            :expandedKeys="expandedKeys"
            :lazy="true"
            dataKey="organization_id"
            v-model:selectionKeys="selectedNodeOrganization"
            filterMode="strict"
            scrollHeight="flex"
            filterDisplay="menu"
            class="d-lang-table"
          >
            <Column
              field="organization_name"
              header="Danh sách phòng ban"
              :expander="true"
              style="min-width: 200px"
            >
              <template #body="slotProps">
                <div
                  @click="goOrganization(slotProps.node.data)"
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
            :value="tempusers.filter(x => x.full_name.toLowerCase().includes(options.search.toLowerCase()))"
            :loading="options.loading"
            :scrollable="true"
            :rowHover="true"
            :totalRecords="options.total"
            :selectionMode="props.one ? 'single' : ''"
            :lazy="true"
            dataKey="user_id"
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
            <!-- <Column
              :selectionMode="props.one ? 'single' : 'multiple'"
              headerStyle="text-align:center;max-width:50px"
              bodyStyle="text-align:center;max-width:50px"
              class="align-items-center justify-content-center text-center"
            ></Column> -->
            <Column
              :selectionMode="'multiple'"
              class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;max-width:50px;height:50px"
              bodyStyle="text-align:center;max-width:50px"
            >
              <template #body="md">
                <Checkbox
                  :binary="true"
                  v-model="md.data.is_check"
                  @change="onCheckBox(md.data, 3)"
                />
              </template>
            </Column>
            <Column
              field="full_name"
              header="Họ và tên"
              headerStyle="height:50px;max-width:auto;"
              bodyStyle="max-height:60px"
              :sortable="false"
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
                    <span>{{ slotProps.data.full_name }}</span>
                  </div>
                </div>
              </template>
              <template #empty>
                <div
                  class="
                    align-items-center
                    justify-content-center
                    p-4
                    text-center
                  "
                >
                  <img src="../../assets/background/nodata.png" height="144" />
                  <h3 class="m-1">Không có dữ liệu</h3>
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
        @click="cancel()"
        class="p-button-text"
      />
      <Button label="Chia sẻ" icon="pi pi-check" @click="choice()" />
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