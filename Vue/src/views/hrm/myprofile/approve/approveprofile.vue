<script setup>
import { ref, inject, onMounted, watch, nextTick } from "vue";
import { encr } from "../../../../util/function";
import { useToast } from "vue-toastification";
import moment from "moment";
import dialogmyprofile from "../component/dialogmyprofile.vue";
import dialogapprove from "../approve/component/dialogapprove.vue";

const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const basedomainURL = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();
const cryoptojs = inject("cryptojs");

//Declare
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const options = ref({
  loading: false,
  user_id: store.getters.user.user_id,
  search: "",
  pageNo: 1,
  pageSize: 25,
  limitItem: 25,
  total: 0,
  sort: "created_date desc",
  orderBy: "DESC",
  tab: 1,
});
const tabs = ref([
  { id: 1, title: "Chờ duyệt", icon: "", total: 0 },
  { id: 2, title: "Đã duyệt", icon: "", total: 0 },
  { id: 3, title: "Trả lại", icon: "", total: 0 },
]);
const selectedNodes = ref([]);
const isFilter = ref(false);
const isFirst = ref(true);
const dictionarys = ref([]);
const counts = ref([]);
const datas = ref([]);
const dataLimits = ref([]);
const profile = ref({});
const files = ref([]);
const submitted = ref(false);

watch(selectedNodes, () => {});

//filter
const search = () => {
  options.value.pageNo = 1;
  options.value.pageSize = 25;
  options.value.limitItem = 25;
  options.value.total = 0;
  dataLimits.value = [];

  initCount();
  initData(true);
};
const activeTab = (tab) => {
  options.value.pageNo = 1;
  options.value.pageSize = 25;
  options.value.limitItem = 25;
  options.value.total = 0;
  dataLimits.value = [];

  options.value.tab = tab.id;
  initData(true);
};

//Function
//Funtion send
const modelapprove = ref({});
const headerDialogApprove = ref();
const displayDialogApprove = ref(false);
const menuSends = ref();
const itemSends = ref([
  {
    id: 2,
    label: "Duyệt hồ sơ",
    icon: "pi pi-check",
  },
  {
    id: 3,
    label: "Trả lại",
    icon: "pi pi-undo",
  },
]);
const toggleSend = (event) => {
  menuSends.value.toggle(event);
};
const openDialogSend = (str, type) => {
  files.value = [];
  submitted.value = false;
  modelapprove.value = {
    is_approve: type,
    content: "",
  };
  headerDialogApprove.value = str;
  displayDialogApprove.value = true;
  forceRerender(1);
};
const closeDialogApprove = () => {
  modelapprove.value = {};
  displayDialogApprove.value = false;
  forceRerender(1);
};

const componentKey = ref({});
const forceRerender = (type) => {
  if (!componentKey.value[type]) {
    componentKey.value[type] = 0;
  }
  componentKey.value[type] += 1;
};
const headerDialog = ref();
const displayDialog = ref(false);
const openEditDialog = (str) => {
  headerDialog.value = str;
  displayDialog.value = true;
  forceRerender(0);
};
const closeDialog = () => {
  displayDialog.value = false;
  forceRerender(0);
};
const goProfile = (item) => {
  if (selectedNodes.value && selectedNodes.value.length === 1) {
    selectedNodes.value = selectedNodes.value.filter((x) => x.is_approve === 1);
    profile.value = item;
    openEditDialog("Cập nhật thông tin hồ sơ");
  }
};

//Init
const initDictionary = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_dictionary",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          dictionarys.value = tbs;
        }
      }
    });
};
const initCount = () => {
  tabs.value.forEach((x) => {
    x["total"] = 0;
  });
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_myprofile_approve_count",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      if (response != null && response.data != null) {
        var data = response.data.data;
        if (data != null) {
          let tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            counts.value = tbs[0];
            tabs.value.forEach((x) => {
              var idx = counts.value.findIndex((c) => c["is_approve"] == x["id"]);
              if (idx !== -1) {
                x["total"] = counts.value[idx]["total"];
              }
            });
          }
        }
      }
    });
};
const initData = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  if (isFilter.value) {
    initDataFilter();
    return;
  }
  options.value.loading = true;
  datas.value = [];
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_myprofile_approve_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "search", va: options.value.search },
              { par: "pageNo", va: options.value.pageNo },
              { par: "pageSize", va: options.value.pageSize },
              { par: "tab", va: options.value.tab },
            ],
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
              const startDate = moment(item.recruitment_date || new Date());
              const endDate = moment(new Date());
              item.duration = moment.duration(endDate.diff(startDate));
              item.diffyear = item.duration.years();
              item.diffmonth = item.duration.months();
              item.diffday = item.duration.days();

              item["STT"] = i + 1;
              if (item["created_date"] != null) {
                item["created_date"] = moment(
                  new Date(item["created_date"])
                ).format("DD/MM/YYYY");
              }
              if (item["birthday"] != null) {
                item["birthday"] = moment(new Date(item["birthday"])).format(
                  "DD/MM/YYYY"
                );
              }
              if (item["recruitment_date"] != null) {
                item["recruitment_date"] = moment(
                  new Date(item["recruitment_date"])
                ).format("DD/MM/YYYY");
              }
            });

            datas.value = data[0];
            dataLimits.value = dataLimits.value.concat(data[0]);
            if (data[1] != null && data[1].length > 0) {
              options.value.total = data[1][0].total;
            }
          } else {
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
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
  selectedNodes.value = [];
  options.value.pageNo = 1;
  options.value.pageSize = 25;
  options.value.limitItem = 25;
  options.value.total = 0;
  dataLimits.value = [];

  isFilter.value = false;
  initData(true);
};
onMounted(() => {
  nextTick(() => {
    initData();
    initCount();
    initDictionary();
    const el = document.getElementById("buffered-scroll");
    if (el) {
      el.addEventListener("scroll", (event) => {
        const scrollTop = el.scrollTop;
        const scrollHeight = el.scrollHeight;
        const offsetHeight = el.offsetHeight;
        if (scrollTop >= scrollHeight - offsetHeight - 50) {
          loadMoreRow(datas.value);
        }
      });
    }
  });
});
const loadMoreRow = (data) => {
  if (data.length > 0) {
    if (
      !options.value.loading &&
      options.value.limitItem < options.value.total
    ) {
      options.value.limitItem += 25;
      options.value.pageNo += 1;
      //dataLimits.value = datas.value.slice(0, options.value.limitItem);
      initData(false);
    } else {
      options.value.limitItem = data.length;
      //dataLimits.value = datas.value.slice(0, options.value.limitItem);
      //initData(false);
    }
  }
};
</script>
<template>
  <div class="surface-100 p-3">
    <Toolbar class="outline-none surface-0 border-none pb-0">
      <template #start>
        <span class="p-input-icon-left">
          <i class="pi pi-search" />
          <InputText
            @keypress.enter="search"
            v-model="options.search"
            type="text"
            spellcheck="false"
            placeholder=" Tìm kiếm"
          />
        </span>
      </template>
      <template #end>
        <Button
          v-if="selectedNodes && selectedNodes.length > 0"
          @click="toggleSend"
          icon="pi pi pi-send"
          label="Xử lý"
          class="mr-2 p-button-outlined p-button-secondary"
          aria:haspopup="true"
          aria-controls="overlay_send"
        />
        <Menu
          :model="itemSends"
          :popup="true"
          id="overlay_send"
          ref="menuSends"
        >
          <template #item="{ item }">
            <a
              class="p-menuitem-link"
              role="menuitem"
              tabindex="0"
              @click="openDialogSend(item.label, item.id)"
            >
              <span class="p-menuitem-icon" :class="item.icon"></span>
              <span class="p-menuitem-text">{{ item.label }}</span>
            </a>
          </template>
        </Menu>
        <Button
          @click="refresh()"
          class="mr-2 p-button-outlined p-button-secondary"
          v-tooltip.top="'Tải lại'"
          icon="pi pi-refresh"
        />
      </template>
    </Toolbar>
    <div class="tabview">
      <div class="tableview-nav-content">
        <ul class="tableview-nav">
          <li
            v-for="(tab, key) in tabs"
            :key="key"
            @click="activeTab(tab)"
            class="tableview-header"
            :class="{ highlight: options.tab === tab.id }"
          >
            <a>
              <i :class="tab.icon"></i>
              <span>{{ tab.title }} ({{ tab.total }})</span>
            </a>
          </li>
        </ul>
      </div>
    </div>
    <div
      id="buffered-scroll"
      class="d-lang-table"
      style="
        height: calc(100vh - 160px) !important;
        border-top: solid 1px rgba(0, 0, 0, 0.1);
      "
    >
      <DataTable
        @rowSelect="
          (event) => {
            goProfile(event.data);
          }
        "
        :value="datas"
        :virtualScrollerOptions="{ itemSize: 78 }"
        v-model:selection="selectedNodes"
        selectionMode="multiple"
        dataKey="profile_id"
        class="disable-header"
      >
        <Column
          v-if="options.tab === 1"
          selectionMode="multiple"
          headerStyle="text-align:center;max-width:50px"
          bodyStyle="text-align:center;max-width:50px"
          class="align-items-center justify-content-center text-center"
        ></Column>
        <Column
          field="Avatar"
          header="Ảnh"
          headerStyle="text-align:center;max-width:100px;height:50px"
          bodyStyle="text-align:center;max-width:100px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <div class="relative">
              <Avatar
                v-bind:label="
                  slotProps.data.avatar
                    ? ''
                    : (slotProps.data.profile_user_name ?? '')
                        .substring(0, 1)
                        .toUpperCase()
                "
                v-bind:image="
                  slotProps.data.avatar
                    ? basedomainURL + slotProps.data.avatar
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                "
                :style="{
                  background: bgColor[slotProps.index % 7],
                  color: '#ffffff',
                  width: '5rem',
                  height: '5rem',
                  fontSize: '1.5rem !important',
                  borderRadius: '5px',
                }"
                size="xlarge"
                class="border-radius"
              />
              <span
                v-if="slotProps.data.isEdit"
                class="is-sign"
                v-tooltip="'Đã hiệu chỉnh hồ sơ'"
              >
                <font-awesome-icon
                  icon="fa-solid fa-circle-check"
                  style="font-size: 16px; display: block; color: #f4b400"
                />
              </span>
            </div>
          </template>
        </Column>
        <Column
          field="profile_user_name"
          header="Họ và tên"
          headerStyle="height:50px;min-width:200px;"
        >
          <template #body="slotProps">
            <div style="min-width: 200px">
              <div class="mb-2">
                <b>{{ slotProps.data.profile_user_name }}</b>
              </div>
              <div class="mb-1">
                <span
                  >{{ slotProps.data.superior_id }}
                  <span
                    v-if="
                      slotProps.data.superior_id && slotProps.data.profile_id
                    "
                    >|</span
                  >
                  {{ slotProps.data.profile_code }}</span
                >
              </div>
              <div class="mb-1" v-if="slotProps.data.recruitment_date">
                {{ slotProps.data.recruitment_date }}
              </div>
            </div>
          </template>
        </Column>
        <Column
          field="profile_user_name"
          header="Họ và tên"
          headerStyle="text-align:center;max-width:300px;height:50px"
          bodyStyle="text-align:center;max-width:300px;"
          class="align-items-center justify-content-left text-left"
        >
          <template #body="slotProps">
            <div style="min-width: 200px">
              <div class="mb-1" v-if="slotProps.data.gender">
                <span>{{
                  slotProps.data.gender == 1
                    ? "Nam"
                    : slotProps.data.gender == 2
                    ? "Nữ"
                    : "Khác"
                }}</span>
              </div>
              <div class="mb-1">
                <span>{{ slotProps.data.birthday }}</span>
              </div>
              <div class="mb-1">
                <span>{{ slotProps.data.birthplace_name }}</span>
              </div>
            </div>
          </template>
        </Column>
        <Column
          field="profile_user_name"
          header="Họ và tên"
          headerStyle="text-align:center;max-width:300px;height:50px"
          bodyStyle="text-align:center;max-width:300px;"
          class="align-items-center justify-content-left text-left"
        >
          <template #body="slotProps">
            <div style="min-width: 200px">
              <div class="mb-1">
                <span
                  >{{ slotProps.data.phone }}
                  <span
                    v-if="
                      slotProps.data.phone != null &&
                      slotProps.data.email != null
                    "
                    >|</span
                  >
                  {{ slotProps.data.email }}</span
                >
              </div>
              <div class="mb-1">
                <span>{{ slotProps.data.identity_papers_code }}</span>
              </div>
              <div class="mb-1">
                <span>{{ slotProps.data.place_residence }}</span>
              </div>
            </div>
          </template>
        </Column>
        <Column
          field="profile_user_name"
          header="Họ và tên"
          headerStyle="text-align:center;max-width:300px;height:50px"
          bodyStyle="text-align:center;max-width:300px;"
          class="align-items-center justify-content-left text-left"
        >
          <template #body="slotProps">
            <div style="min-width: 200px">
              <div class="mb-1">
                <b>{{ slotProps.data.position_name }}</b>
              </div>
              <div class="mb-1">
                <span>{{ slotProps.data.title_name }}</span>
              </div>
              <div class="mb-1">
                <span
                  v-if="
                    slotProps.data.department_name &&
                    slotProps.data.department_name.includes('<br/>')
                  "
                  v-html="slotProps.data.department_name"
                ></span>
                <span v-else>{{ slotProps.data.department_name }}</span>
              </div>
            </div>
          </template>
        </Column>
        <Column
          field="countRecruitment"
          header="Ngày thâm niên"
          headerStyle="text-align:center;max-width:150px;height:50px"
          bodyStyle="text-align:center;max-width:150px;"
          class="align-items-center justify-content-left text-left"
        >
          <template #body="slotProps">
            <div v-tooltip.top="'Thâm niên công tác'">
              <span v-if="slotProps.data.diffyear > 0">
                {{ slotProps.data.diffyear }} năm
              </span>
              <span v-if="slotProps.data.diffmonth > 0">
                {{ slotProps.data.diffmonth }} tháng
              </span>
              <span v-if="slotProps.data.seniority != null">
                {{ slotProps.data.seniority }}
              </span>
              <!-- <span
                v-if="
                  slotProps.data.diffyear >= 0 && slotProps.data.diffday > 0
                "
                >{{ slotProps.data.diffday }} ngày
              </span> -->
            </div>
          </template>
        </Column>
        <Column
          field="status"
          header="Trạng thái"
          headerStyle="text-align:center;max-width:30px;height:50px"
          bodyStyle="text-align:center;max-width:30px;"
          class="align-items-center justify-content-center text-center"
        >
          <template #body="slotProps">
            <div
              :style="{
                borderRadius: '50%',
                border: slotProps.data.bg_color,
                backgroundColor: slotProps.data.bg_color,
                color: slotProps.data.text_color,
                width: '15px',
                height: '15px',
              }"
              v-tooltip.top="slotProps.data.status_name"
            ></div>
          </template>
        </Column>
        <template #empty>
          <div
            class="align-items-center justify-content-center p-4 text-center m-auto"
            :style="{
              display: 'flex',
              width: '100%',
              height: 'calc(100vh - 235px)',
              backgroundColor: '#fff',
            }"
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
  <dialogmyprofile
    :key="componentKey['0']"
    :headerDialog="headerDialog"
    :displayDialog="displayDialog"
    :closeDialog="closeDialog"
    :isAdd="false"
    :profile="profile"
    :approve="true"
    :dictionarys="dictionarys"
    :initData="initData"
  />

  <!--Send-->
  <dialogapprove
    :key="componentKey['1']"
    :headerDialog="headerDialogApprove"
    :displayDialog="displayDialogApprove"
    :closeDialog="closeDialogApprove"
    :model="modelapprove"
    :selectedNodes="selectedNodes"
    :initData="initData"
  />
</template>
<style scoped>
@import url(../../profile/component/stylehrm.css);
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
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label,
  .p-treeselect .p-treeselect-label {
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
::v-deep(.disable-header) {
  table thead {
    display: none;
  }
}
::v-deep(.border-radius) {
  img {
    border-radius: 5px;
  }
}
</style>
