<script setup>
import { onMounted, inject, ref, watch } from "vue";
import { encr } from "../../../../util/function";
import { useToast } from "vue-toastification";
import framepreview from "../../component/framepreview.vue";
import moment from "moment";

const router = inject("router");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const base_url = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();
const cryoptojs = inject("cryptojs");
const basedomainURL = baseURL;

//Get arguments
const props = defineProps({
  profile_id: String,
});

//Declare
const isFilter = ref(false);
const tabs = ref([
  { id: -1, title: "Tất cả", icon: "", total: 0 },
  { id: 0, title: "Chờ duyệt", icon: "", total: 0 },
  { id: 1, title: "Đã duyệt", icon: "", total: 0 },
  { id: 2, title: "Không duyệt", icon: "", total: 0 },
]);
const options = ref({
  loading: true,
  user_id: store.getters.user.user_id,
  search: "",
  pageNo: 1,
  pageSize: 25,
  total: 0,
  sort: "created_date desc",
  orderBy: "desc",
  tab: -1,
  filterContract_id: null,
  organizations: [],
  departments: [],
  type_contracts: [],
  work_positions: [],
  sign_start_date: null,
  sign_end_date: null,
  users: [],
  start_start_date: null,
  end_start_date: null,
  start_end_date: null,
  end_end_date: null,
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
const typestatus = ref([
  { value: 0, title: "Chờ duyệt", bg_color: "#0078d4", text_color: "#fff" },
  { value: 1, title: "Đã duyệt", bg_color: "#5FC57B", text_color: "#fff" },
  { value: 2, title: "Không duyệt", bg_color: "#DF5249", text_color: "#fff" },
]);
const liquidations = ref([
  { value: 0, title: "Thôi việc" },
  { value: 1, title: "Ký hợp đồng mới" },
  { value: 2, title: "Chấm dứt HĐLĐ" },
  { value: 3, title: "Chấm dứt HĐLĐ" },
  { value: 4, title: "Khác..." },
]);
const selectedNodes = ref({});
const selectedKeys = ref([]);
const expandedKeys = ref([]);
const isFirst = ref(true);
const datas = ref([]);
const counts = ref([]);
const dictionarys = ref([]);
const decision = ref({});

const menuButMores = ref();
const itemButMores = ref([
  {
    label: "Hiệu chỉnh nội dung",
    icon: "pi pi-pencil",
    command: (event) => {
      editItem(decision.value, "Chỉnh sửa quyết định");
    },
  },
  {
    label: "Nhân bản quyết định",
    icon: "pi pi-copy",
    command: (event) => {
      copyItem(decision.value, "Nhân bản quyết định");
    },
  },
  {
    label: "In quyết định",
    icon: "pi pi-print",
    command: (event) => {
      openDialogFrame(decision.value);
    },
  },
  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: (event) => {
      deleteItem(decision.value);
    },
  },
]);
const toggleMores = (event, item) => {
  decision.value = item;
  decision.value.isEdit = true;
  menuButMores.value.toggle(event);
  selectedNodes.value = item;
  options.value["filterContract_id"] = selectedNodes.value["decision_id"];
};

// watch(selectedNodes, () => {
//   options.value["filterContract_id"] = selectedNodes.value["decision_id"];
// });

//filter

//export
const menuButs = ref();
const itemButs = ref([
  {
    label: "Export dữ liệu ra Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      //exportData("ExportExcel");
    },
  },
  {
    label: "Import dữ liệu từ Excel",
    icon: "pi pi-file-excel",
    command: (event) => {
      //exportData("ExportExcel");
    },
  },
]);
const toggleExport = (event) => {
  menuButs.value.toggle(event);
};

const menuAddItems = ref();
const itemAddItems = ref([]);
const toggleAddItem = (event) => {
  menuAddItems.value.toggle(event);
};

//Function
const componentKey = ref({});
const forceRerender = (type) => {
  if (!componentKey.value[type]) {
    componentKey.value[type] = 0;
  }
  componentKey.value[type] += 1;
};
function CreateGuid() {
  function _p8(s) {
    var p = (Math.random().toString(16) + "000000000").substr(2, 8);
    return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
  }
  return _p8() + _p8(true) + _p8(true) + _p8();
}
const activeTab = (tab) => {
  options.value.tab = tab.id;
  initView8(true);
};
const opstatus = ref();
const toggleStatus = (item, event) => {
  decision.value = item;
  opstatus.value.toggle(event);
};
const goProfile = (item) => {
  if (item.profile_id != null) {
    router.push({
      name: "profileinfo",
      params: { id: item.decision_id },
      query: { id: item.profile_id },
    });
  }
};

const printViewDecision = (row) => {
  if (row && row.report_key) {
    let o = {
      id: row.report_key,
      par: { decision_id: row.decision_id, isedit: true },
    };
    let url = encodeURIComponent(
      encr(JSON.stringify(o), SecretKey, cryoptojs).toString()
    );
    url =
      "https://doconline.soe.vn/decided/" +
      url.replaceAll("%", "==") +
      "?v=" +
      new Date().getTime().toString();
    window.open(url);
  } else {
    swal.fire({
      title: "Thông báo!",
      text: "Chưa thiết lập mẫu in cho quyết định!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
};

const headerDialogFrame = ref();
const displayDialogFrame = ref(false);
const openDialogFrame = () => {
  headerDialogFrame.value = "Thông tin quyết định";
  displayDialogFrame.value = true;
  forceRerender(0);
};
const closeDialogFrame = () => {
  displayDialogFrame.value = false;
  forceRerender(0);
};

const selectRow8 = (event) => {
  if (event && event.data) {
    decision.value = event.data;
    openDialogFrame();
  }
};

//Init
const initView8 = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_myprofile_get_8",
            par: [{ par: "profile_id", va: props.profile_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        let data = JSON.parse(response.data.data);
        if (data != null) {
          if (data[0] != null && data[0].length > 0) {
            data[0].forEach((item, i) => {
              item["STT"] = i + 1;
              if (item.decision_date != null) {
                item.decision_date = moment(
                  new Date(item.decision_date)
                ).format("DD/MM/YYYY");
              }
              if (item.start_date != null) {
                item.start_date = moment(new Date(item.start_date)).format(
                  "DD/MM/YYYY"
                );
              }
              if (item.end_date != null) {
                item.end_date = moment(new Date(item.end_date)).format(
                  "DD/MM/YYYY"
                );
              }
              var idx = typestatus.value.findIndex(
                (x) => x["value"] === item["status"]
              );
              if (idx != -1) {
                item["status_name"] = typestatus.value[idx]["title"];
                item["bg_color"] = typestatus.value[idx]["bg_color"];
                item["text_color"] = typestatus.value[idx]["text_color"];
              } else {
                item["status_name"] = "Chưa xác định";
                item["bg_color"] = "#bbbbbb";
                item["text_color"] = "#fff";
              }
            });
            datas.value = data[0];
            if (data[1] != null && data[1].length > 0) {
              options.value.total = data[1][0].total;
            }
          } else {
            datas.value = [];
            options.value.total = 0;
          }
        }
      }
      if (isFirst.value) isFirst.value = false;
      swal.close();
      if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
      swal.close();
      if (options.value.loading) options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          title: "Thông báo!",
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
        return;
      } else {
        swal.fire({
          title: "Thông báo!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
        return;
      }
    });
};
const refresh = () => {
  selectedNodes.value = {};
  options.value = {
    loading: true,
    user_id: store.getters.user.user_id,
    search: "",
    pageNo: 1,
    pageSize: 25,
    total: 0,
    sort: "created_date desc",
    orderBy: "desc",
    tab: -1,
    filterContract_id: null,
    organizations: [],
    departments: [],
    type_contracts: [],
    work_positions: [],
    sign_start_date: null,
    sign_end_date: null,
    users: [],
    start_start_date: null,
    end_start_date: null,
    start_end_date: null,
    end_end_date: null,
  };
  isFilter.value = false;
  initView8(true);
};
//page
const onPage = (event) => {
  if (event.rows != options.value.pageSize) {
    options.value.pageSize = event.rows;
  }
  options.value.pageNo = event.page + 1;
  initView8(true);
};
onMounted(() => {
  initView8(true);
});
</script>
<template>
  <div class="surface-100">
    <div class="d-lang-table">
      <DataTable
        @page="onPage($event)"
        @rowSelect="selectRow8"
        :value="datas"
        :scrollable="true"
        :lazy="true"
        :rowHover="true"
        :showGridlines="false"
        :globalFilterFields="['decision_name']"
        v-model:selection="selectedNodes"
        selectionMode="single"
        dataKey="decision_id"
        scrollHeight="flex"
        filterDisplay="menu"
        filterMode="lenient"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
        responsiveLayout="scroll"
      >
        <Column
          field="decision_code"
          header="Số QĐ"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        />
        <Column
          field="profile_user_name"
          header="Tên nhân sự"
          headerStyle="height:50px;max-width:auto;"
        >
          <template #body="slotProps">
            {{ slotProps.data.profile_user_name }}
          </template>
        </Column>
        <Column
          field="position_name"
          header="Cấp ra QĐ"
          headerStyle="height:50px;max-width:auto;"
        >
        </Column>
        <Column
          field="type_decision_name"
          header="Loại quyết định"
          headerStyle="height:50px;max-width:auto;"
        >
        </Column>
        <Column
          field="decision_date"
          header="Ngày quyết định"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <span>{{ slotProps.data.decision_date }}</span>
          </template>
        </Column>
        <Column
          field="start_date"
          header="Ngày hiệu lực"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <span>{{ slotProps.data.start_date }}</span>
          </template>
        </Column>
        <Column
          field="end_date"
          header="Ngày hết hạn"
          headerStyle="text-align:center;max-width:120px;height:50px"
          bodyStyle="text-align:center;max-width:120px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <span>{{ slotProps.data.end_date }}</span>
          </template>
        </Column>
        <Column
          field="status"
          header="Trạng thái"
          headerStyle="text-align:center;max-width:130px;height:50px"
          bodyStyle="text-align:center;max-width:130px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <Button
              :label="slotProps.data.status_name"
              class="p-button-outlined"
              :style="{
                borderColor: slotProps.data.bg_color,
                // backgroundColor: slotProps.data.bg_color,
                color: slotProps.data.bg_color,
                borderRadius: '15px',
                padding: '0.3rem 0.75rem !important',
              }"
            />
          </template>
        </Column>
        <template #empty>
          <div
            class="align-items-center justify-content-center p-4 text-center m-auto"
            style="
              display: flex;
              width: 100%;
              height: calc(100vh - 331px);
              background-color: #fff;
            "
          >
            <div v-if="!options.loading && (!isFirst || options.total == 0)">
              <img
                src="../../../../assets/background/nodata.png"
                height="144"
              />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </div>
        </template>
      </DataTable>
    </div>
  </div>

  <!--Dialog-->
  <framepreview
    :key="componentKey['0']"
    :headerDialog="headerDialogFrame"
    :displayDialog="displayDialogFrame"
    :closeDialog="closeDialogFrame"
    :type="3"
    :model="decision"
  />
</template>
<style scoped>
@import url(../../contract/component/stylehrm.css);
.d-lang-table {
  height: calc(100vh - 230px) !important;
  background-color: #fff;
}
.icon-star {
  color: #f4b400 !important;
}
.hover:hover {
  color: #0078d4;
}
</style>
<style lang="scss" scoped>
::v-deep(.d-lang-table) {
  .p-datatable-thead .justify-content-center .p-column-header-content {
    justify-content: center !important;
  }

  .p-datatable-table {
    position: absolute;
  }

  .p-datatable-thead {
    position: sticky;
    top: 0;
    z-index: 1;
  }
}
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
    height: 100%;
    display: flex;
    align-items: center;
  }
  .p-chip img {
    margin: 0;
  }
  .p-avatar-text {
    font-size: 1rem;
  }
}
::v-deep(.avatar-item) {
  .p-avatar.p-avatar-lg {
    width: 3rem;
    height: 3rem;
  }
}
::v-deep(.is-close) {
  .p-panel-header {
    color: red;
  }
}
</style>
