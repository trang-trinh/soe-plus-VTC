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
const isAdd = ref(false);
const isDynamicSQL = ref(false);
const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const options = ref({});
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
  displayDialog: Boolean,
  folder: String,
});

//Khai báo
const rootusers = ref([]);
const folder = ref();
const ListFolderShare = ref([]);
const typeTab = ref(2);
const temporganizations = ref();
const selectedUsers = ref([]);
const listUsers = ref();
const datalists = ref([]);
const expandedKeys = ref({});
const expandedKeysFolder = ref({});
var tbs = [];
var arr_users_coppy = [];
const listShare = ref([
  { value: 0, label: "Chỉ mình tôi" },
  { value: 1, label: "Mọi người" },
  { value: 2, label: "Nhóm người" },
]);
const selectedNodeUser = ref([]);

const initFolder = (id) => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "file_folder_get",
        par: [{ par: "folder_id", va: id }],
      },
      config
    )
    .then((response) => {
      var data = JSON.parse(response.data.data);
      if (data.length > 0) {
        folder.value = data[0][0];
        folder.value.type_share =
          folder.value.type_share != null ? folder.value.type_share : 0;
        if (folder.value.type_share == 2) {
          datalists.value = data[1].filter((x) => x.type == 2);
          selectedUsers.value = data[1].filter((x) => x.type == 3);
          initOrganization();
          initUser();
        } else if (folder.value.type_share == 1) {
          ListFolderShare.value = data[1].filter((x) => x.type == 1);
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
const activeTab = (type) => {
  typeTab.value = type;
};
const onChangeType = (type) => {
  ListFolderShare.value = [];
  if (type == 1) {
    let obj = {
      type: type,
      is_read: false,
      is_edit: false,
      is_delete: false,
      is_order: 1,
    };
    ListFolderShare.value[0] = obj;
  } else if (type == 2) {
    initOrganization();
    initUser();
  }
};
//Function choice
const choice = () => {
  if (folder.value.type_share == 2) {
    let arr1 = datalists.value.filter(
      (x) => x.is_delete || x.is_delete || x.is_read
    );
    let arr2 = selectedUsers.value.filter(
      (x) => x.is_delete || x.is_delete || x.is_read
    );
    ListFolderShare.value = ListFolderShare.value.concat(arr1, arr2);
  }
  emitter.emit("emitData", {
    type: "choicefolder",
    data: {
      submit: true,
      displayDialog: false,
      selected: ListFolderShare.value,
      type_share: folder.value.type_share,
    },
  });
};
const cancel = () => {
  props.value = {};
  emitter.emit("emitData", {
    type: "choicefolder",
    data: {
      submit: false,
      displayDialog: false,
    },
  });
};
const onCheckBox = (u, type) => {
  if (type == 2) {
    if (datalists.value.length == 0)
      datalists.value = tbs[0]
        .filter((x) => x.is_checked && x.organization_id != u.organization_id)
        .map((a) => ({
          organization_id: a.organization_id,
          organization_name: a.organization_name,
          is_read: false,
          is_edit: false,
          is_delete: false,
          type: 2,
        }));
    if (u.is_checked) {
      datalists.value.push({
        organization_id: u.organization_id,
        organization_name: u.organization_name,
        is_read: false,
        is_edit: false,
        is_delete: false,
        type: 2,
      });
    } else {
      let idx = datalists.value.findIndex(
        (x) => x.organization_id == u.organization_id
      );
      if (idx != -1) datalists.value.splice(idx, 1);
    }
  } else if (type == 3) {
    if (selectedUsers.value.length == 0)
      selectedUsers.value = listUsers.value
        .filter((x) => x.is_checked && x.user_id != u.user_id)
        .map((a) => ({
          user_id: a.user_id,
          full_name: a.full_name,
          is_read: false,
          is_edit: false,
          is_delete: false,
          type: 3,
        }));
    if (u.is_checked) {
      selectedUsers.value.push({
        user_id: u.user_id,
        full_name: u.full_name,
        is_read: false,
        is_edit: false,
        is_delete: false,
        type: 3,
      });
    } else {
      let idx = selectedUsers.value.findIndex((x) => x.user_id == u.user_id);
      if (idx != -1) selectedUsers.value.splice(idx, 1);
    }
  }
};
const unSelect = (event) => {
  let idx = datalists.value.findIndex((x) => x.organization_id == event.key);
  if (idx != -1) datalists.value.splice(idx, 1);
};
const delRowUser = (id, type) => {
  if (type == 2) {
    // temporganizations.value.filter(
    //   (x) => x.organization_id == id
    // )[0].is_checked = false;
    let idx = datalists.value.findIndex((x) => x.organization_id == id);
    if (idx != -1) datalists.value.splice(idx, 1);
  } else if (type == 3) {
    listUsers.value.filter((x) => x.user_id == id)[0].is_checked = false;
    let idx = selectedUsers.value.findIndex((x) => x.user_id == id);
    if (idx != -1) selectedUsers.value.splice(idx, 1);
  }
};
//init function
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
        tbs = JSON.parse(data);

        if (tbs[0] != null && tbs[0].length > 0) {
          tbs[0].forEach((item) => {
            if (
              datalists.value
                .map((x) => x.organization_id)
                .includes(item.organization_id)
            )
              item.is_checked = true;
          });
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
const initUser = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "folder_user_list",
        par: [
          { par: "search", va: options.value.search },
          { par: "user_id", va: store.getters.user.user_id },
        ],
      },
      config
    )
    .then((response) => {
      var data = response.data.data;
      if (data != null) {
        listUsers.value = JSON.parse(data)[0];
        listUsers.value.forEach((item) => {
          if (selectedUsers.value.map((x) => x.user_id).includes(item.user_id))
            item.is_checked = true;
        });
        //   arr_users_coppy = JSON.parse(JSON.stringify(listUsers.value));
        //   let arr = listUsers.value.filter((x) => x.is_checked);
        //   if (arr.length > 0) listUsers.value = arr;
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
//def
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
onMounted(() => {
  debugger;
  initFolder(props.folder);
  //initRender();
  return {};
});
</script>

<template>
  <Dialog
    v-model:visible="props.displayDialog"
    header="Chia sẻ thư mục"
    :modal="true"
    :closable="false"
    :style="{ width: '52vw' }"
    :maximizable="true"
    :autoZIndex="true"
  >
    <div class="grid formgrid m-2">
      <div class="field col-12 md:col-12" v-if="folder">
        <div class="field col-12 md:col-12">
          <label class="text-left col-2 p-0">Đối tượng chia sẻ</label>
          <Dropdown
            class="col-10 p-0"
            v-model="folder.type_share"
            :options="listShare"
            :max-height="200"
            placeholder=""
            optionLabel="label"
            optionValue="value"
            @change="onChangeType(folder.type_share)"
          >
          </Dropdown>
        </div>
        <div
          v-if="folder.type_share == 1"
          class="field col-12 md:col-12 flex pt-1"
        >
          <label class="col-3 text-right mt-1">Đọc: </label>
          <Checkbox v-model="ListFolderShare[0].is_read" :binary="true" />
          <label class="col-3 text-right mt-1">Ghi: </label>
          <Checkbox v-model="ListFolderShare[0].is_edit" :binary="true" />
          <label class="col-3 text-right mt-1">Xóa: </label>
          <Checkbox v-model="ListFolderShare[0].is_delete" :binary="true" />
        </div>
        <div class="field col-12 md:col-12 p-0" v-if="folder.type_share == 2">
          <div style="border-right: 1px solid #efefef">
            <ul
              class="list-group ul-ktra"
              style="flex-direction: row; justify-content: space-around"
            >
              <li
                class="list-group-item"
                :class="typeTab == 2 ? 'active_li' : ''"
                @click="activeTab(2)"
              >
                <i class="pi pi-users mr-2"></i>
                <span>Phòng ban</span>
              </li>
              <li
                class="list-group-item"
                :class="typeTab == 3 ? 'active_li' : ''"
                @click="activeTab(3)"
              >
                <i class="pi pi-user mr-2"></i>
                <span>Cá nhân</span>
              </li>
            </ul>
          </div>
        </div>

        <div
          v-if="folder.type_share == 2 && typeTab == 2"
          class="field col-12 md:col-12 flex p-0"
        >
          <div class="col-5 p-0">
            <TreeTable
              :value="temporganizations"
              :scrollable="true"
              :rowHover="true"
              :expandedKeys="expandedKeys"
              :lazy="true"
              dataKey="organization_id"
              filterMode="strict"
              scrollHeight="flex"
              filterDisplay="menu"
              class="d-lang-table"
            >
              <Column
                field="organization_name"
                header="Chọn phòng ban"
                :expander="true"
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
                    <Checkbox
                      class="mr-1"
                      :binary="true"
                      v-model="slotProps.node.data.is_checked"
                      @change="onCheckBox(slotProps.node.data, 2)"
                    />
                    <span>{{ slotProps.node.data.organization_name }}</span>
                  </div>
                </template>
              </Column>
            </TreeTable>
          </div>
          <div class="col-7 md:col-7">
            <table class="w-full" style="overflow-y: scroll">
              <thead>
                <tr style="background-color: #f8f9fa; height: 40px">
                  <th>STT</th>
                  <th>Tên phòng ban</th>
                  <th>Đọc</th>
                  <th>Ghi</th>
                  <th>Xóa</th>
                  <th></th>
                </tr>
              </thead>
              <tbody class="box-right">
                <tr
                  style="vertical-align: top"
                  v-for="(item, index) in datalists"
                  :key="index"
                >
                  <td class="text-center">{{ index + 1 }}</td>
                  <td class="text-center">
                    {{ item.organization_name }}
                  </td>
                  <td class="text-center">
                    <Checkbox
                      id="IsIdentity"
                      v-model="item.is_read"
                      :binary="true"
                      @change="clickDelFile"
                    />
                  </td>
                  <td class="text-center">
                    <Checkbox
                      id="IsIdentity"
                      v-model="item.is_edit"
                      :binary="true"
                      @change="clickDelFile"
                    />
                  </td>
                  <td class="text-center">
                    <Checkbox
                      id="IsIdentity"
                      v-model="item.is_delete"
                      :binary="true"
                      @change="clickDelFile"
                    />
                  </td>
                  <td class="text-center">
                    <i
                      class="pi pi-trash icon-delete mt-2"
                      v-tooltip.top="'Xóa'"
                      @click="delRowUser(item.organization_id, 2)"
                    ></i>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        <div
          v-if="folder.type_share == 2 && typeTab == 3"
          class="field col-12 md:col-12 flex p-0"
        >
          <div class="col-5 p-0 tab-users">
            <DataTable
              class="w-full p-datatable-sm e-sm"
              :value="listUsers"
              dataKey="user_id"
              :showGridlines="true"
              :rowHover="true"
              currentPageReportTemplate=""
              responsiveLayout="scroll"
              :scrollable="true"
              scrollHeight="flex"
              rowGroupMode="subheader"
              groupRowsBy="organization_name"
              :lazy="true"
              :loading="options.loading"
              :paginator="false"
              @filter="onFilter($event)"
              v-model:first="first"
            >
              <template #groupheader="slotProps">
                <i class="pi pi-building mr-2"></i>
                {{ slotProps.data.organization_name }}
              </template>
              <Column
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:70px;height:40px"
                bodyStyle="text-align:center;max-width:70px"
                field=""
                header=""
              >
                <template #body="md">
                  <Checkbox
                    :binary="true"
                    v-model="md.data.is_checked"
                    @change="onCheckBox(md.data, 3)"
                  />
                </template>
              </Column>
              <Column
                headerStyle="text-align:center;height:40px;"
                bodyStyle="text-align:left;"
                field="full_name"
                header="Họ tên nhân sự"
              >
                <template #body="data">
                  <div>
                    {{ data.data.full_name }}
                  </div>
                </template>
                <template #filter="{ filterModel }">
                  <InputText
                    type="text"
                    v-model="filterModel.value"
                    class="p-column-filter"
                    placeholder="Tài khoản"
                  />
                </template>
              </Column>
              <Column
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:150px"
                bodyStyle="text-align:center;max-width:150px"
                field="position_name"
                header="Chức vụ"
              >
                <template #body="data">
                  <div>
                    {{ data.data.position_name }}
                  </div>
                </template>
              </Column>
            </DataTable>
          </div>
          <div class="col-7 md:col-7">
            <table class="w-full" style="overflow-y: scroll">
              <thead>
                <tr style="background-color: #f8f9fa; height: 40px">
                  <th>STT</th>
                  <th>Họ tên</th>
                  <th>Đọc</th>
                  <th>Ghi</th>
                  <th>Xóa</th>
                  <th></th>
                </tr>
              </thead>
              <tbody class="box-right">
                <tr
                  style="vertical-align: top"
                  v-for="(item, index) in selectedUsers"
                  :key="index"
                >
                  <td class="text-center">{{ index + 1 }}</td>
                  <td class="text-center">
                    {{ item.full_name }}
                  </td>
                  <td class="text-center">
                    <Checkbox
                      id="IsIdentity"
                      v-model="item.is_read"
                      :binary="true"
                      @change="clickDelFile"
                    />
                  </td>
                  <td class="text-center">
                    <Checkbox
                      id="IsIdentity"
                      v-model="item.is_edit"
                      :binary="true"
                      @change="clickDelFile"
                    />
                  </td>
                  <td class="text-center">
                    <Checkbox
                      id="IsIdentity"
                      v-model="item.is_delete"
                      :binary="true"
                      @change="clickDelFile"
                    />
                  </td>
                  <td class="text-center">
                    <i
                      class="pi pi-trash icon-delete mt-2"
                      v-tooltip.top="'Xóa'"
                      @click="delRowUser(item.user_id, 3)"
                    ></i>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
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

.list-group {
  list-style: none;
  padding-left: 0px;
  margin-bottom: 0px;
  cursor: pointer;
  display: flex;
}
.ul-ktra .list-group-item.active_li {
  border-bottom: none;
  background-color: rgb(255, 255, 255);
  border-top: 4px solid rgb(0, 168, 255);
  border-left: 1px solid rgb(204, 204, 204);
  border-right: 1px solid rgb(204, 204, 204);
}
.ul-ktra .list-group-item {
  width: 100%;
  border-right: none;
  border-left: none;
  border-image: initial;
  text-align: center;
  font-weight: bold;
  background-color: rgb(246, 248, 250);
  border-bottom: 1px solid rgb(204, 204, 204);
  border-top: 4px solid transparent;
  border-radius: 0px;
  margin-bottom: 0px;
  color: rgb(108, 122, 134);
  display: flex;
  flex-direction: column;
  font-size: 14px;
  padding: 0.5rem 1.25rem;
  cursor: pointer;
  display: block;
  height: 40px !important;
  padding: 10px;
}
.icon-delete:hover {
  color: red;
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