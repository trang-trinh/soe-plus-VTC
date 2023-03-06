<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../util/function.js";
const cryoptojs = inject("cryptojs");
const toast = useToast();
const basedomainURL = fileURL;
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const Department = {
  id: null,
  department_id: null,
  user_id: null,
};
const listDepartments = ref();
const listTreeDepartments = ref();
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
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.IsOrder = i + 1;
      m.label_order = m.IsOrder.toString();
      if (opition.value.PageNo > 0) {
        m.STT = (opition.value.PageNo - 1) * opition.value.PageSize + i + 1;
      } else {
        m.STT = i + 1;
      }
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em, index) => {
            em.label_order = mm.data.label_order + "." + em.is_order;
            em.STT = mm.data.STT + "." + (index + 1);
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
        let obj = renderTree(
          data[0],
          "organization_id",
          "organization_name",
          "đơn vị",
        );
        listDepartments.value = obj.arrChils;
        listTreeDepartments.value = obj.arrtreeChils;
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const listUser = (department_id) => {
  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_users_list_task_origin",
            par: [
              { par: "search", va: opition.value.SearchTextUser },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "role_id", va: null },
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "department_id", va: department_id },
              { par: "position_id", va: null },
              { par: "isadmin", va: null },
              { par: "status", va: null },
              { par: "start_date", va: null },
              { par: "end_date", va: null },
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
      listUsers.value = data.map((x) => ({
        name: x.full_name,
        code: x.user_id,
        avatar: x.avatar,
        is_check: false,
        full_name: x.full_name,
        tenChucVu: x.position_name,
        tenToChuc: x.department_name,
      }));
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

const updateDepartmentUser = (data) => {
  listUser(data.organization_id);
  Department.value = {
    id: -1,
    department_id: data.organization_id,
    user_id: null,
  };
  headerAddDepartment.value =
    "Cập nhật người duyệt phòng ban (" + data.organization_name + ")";
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

const ChangeUserDepartment = (model) => {
  listUsers.value.forEach((u) => {
    if (u.code == model.code) {
      u.is_check = true;
    }
  });
};

const saveDepartmentUser = () => {
  let formData = new FormData();
  let listsend = [];
  if (listUsers.value) {
    if (listUsers.value.filter((x) => x.is_check == true).length > 0) {
      listUsers.value.forEach((t) => {
        Department.value.user_id = t.code;
        if (t.is_check == true) {
          let dept = {
            department_id: Department.value.department_id,
            user_id: t.code,
          };
          listsend.push(dept);
        }
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
const first = ref(0);
const filters = ref();
const selectedKey = ref();
const options = ref();
const temporganizations = ref([]);
const expandedKeys2 = ref();
const selectedNodeOrganization = ref();
const tempusers = ref([]);
const selectedNodeUser = ref();
const searchUser = () => {};
const goOrganization = () => {};
onMounted(() => {
  loadData(true);
  return {};
});
</script>
<template>
  <div
    v-if="store.getters.islogin"
    class="main-layout true flex-grow-1 p-2"
  >
    <TreeTable
      :value="listDepartments"
      v-model:selectionKeys="selectedKey"
      v-model:first="first"
      :loading="opition.loading"
      @page="onPage($event)"
      @sort="onSort($event)"
      :paginator="true"
      :rows="opition.PageSize"
      :totalRecords="opition.totalRecords"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]"
      :filters="filters"
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
        headerStyle="height:50px;max-width:auto;"
        bodyStyle="max-height:60px"
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
        headerStyle="height:50px;max-width:100px;"
        class="align-items-center justify-content-center text-center font-bold"
        bodyStyle="text-align:center;max-height:60px; max-width: 100px;"
      >
        <template #body="data">
          <AvatarGroup>
            <div
              v-for="(value, index) in data.node.data.ThanhvienRole"
              :key="index"
            >
              <div>
                <Avatar
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
            </div>
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
              @click="updateDepartmentUser(md.node.data)"
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

    <Dialog
      :header="headerAddDepartment"
      v-model:visible="displayDepartment"
      :style="{ width: '40vw' }"
      :closable="true"
      :maximizable="true"
    >
      <form>
        <div class="grid formgrid m-2">
          <div class="field col-12 md:col-12">
            <DataTable
              v-model:first="first"
              :rowHover="true"
              :value="listUsers"
              :row-hover="true"
              dataKey="code"
              v-model:selection="selectedTasks"
              @page="onPage($event)"
              @sort="onSort($event)"
              @filter="onFilter($event)"
              :lazy="true"
              selectionMode="single"
              @rowSelect="onRowSelect($event.data)"
              @rowUnselect="onRowUnselect($event.data)"
            >
              <Column
                headerStyle="text-align:center;width:1rem;min-height:3.125rem"
                bodyStyle="text-align:center;width:1rem;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="data">
                  <Checkbox
                    inputId="binary"
                    @change="ChangeUserDepartment(data.data)"
                    v-model="data.data.is_check"
                    :binary="true"
                  />
                </template>
              </Column>
              <Column
                header="Ảnh"
                headerStyle="text-align:center;width:1rem;min-height:3.125rem"
                bodyStyle="text-align:center;width:1rem;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="value">
                  <Avatar
                    v-tooltip.bottom="{
                      value:
                        value.data.full_name +
                        '<br/>' +
                        (value.data.tenChucVu || '') +
                        '<br/>' +
                        (value.data.tenToChuc || ''),
                      escape: true,
                    }"
                    v-bind:label="
                      value.data.avatar != null
                        ? ''
                        : (value.data.full_name ?? '')
                            .split(' ')
                            .at(-1)
                            .substring(0, 1)
                    "
                    v-bind:image="basedomainURL + value.data.avatar"
                    style="
                      background-color: #2196f3;
                      color: #ffffff;
                      width: 2.5rem;
                      height: 2.5rem;
                      font-size: 15px !important;
                    "
                    :style="{
                      background: bgColor[0] + '!important',
                    }"
                    class="cursor-pointer"
                    size="xlarge"
                    shape="circle"
                  />
                </template>
              </Column>
              <Column
                field="name"
                header="Tên người dùng"
                headerStyle="text-align:center;max-width:auto;min-height:3.125rem"
                bodyStyle="text-align:left;max-width:auto;"
                class="align-items-left justify-content-center text-left"
              >
              </Column>
              <template #empty>
                <div
                  class="align-items-center justify-content-center p-4 text-center m-auto"
                  style=""
                  v-if="listUsers != null"
                >
                  <h3 class="m-1">Không có dữ liệu</h3>
                </div>
              </template>
            </DataTable>
          </div>
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
  </div>
</template>
