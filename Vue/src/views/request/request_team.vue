<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { encr, checkURL } from "../../util/function.js";
import detail_request from "./component_request/detail_request.vue";
import toolbar_search_request from "./component_request/toolbar_search_request.vue";
//Khai báo
import moment from "moment";
const router = inject("router");
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");

const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const toast = useToast();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
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
const datalists = ref([]);
const user = store.getters.user;
const listTeams = ref([]);
const loadData = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "request_team_list",
            par: [{ par: "user_id", va: user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element) => {
        element.teams = JSON.parse(element.teams);
        element.actived = false;
        if (element.teams) {
          element.teams.forEach((x) => {
            x.actived = false;
            if (x.team_members) {
              x.team_members.forEach((u) => {
                u.actived = false;
              });
            }
          });
        }
      });
      listTeams.value = data;
      swal.close();
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
const arrayTeam = ref([]);
const arrayUser = ref([]);
const OpenOrg = (id) => {
  if (arrayTeam.value.includes(id) == false) {
    arrayTeam.value.push(id);
  } else {
    arrayTeam.value = arrayTeam.value.filter((x) => x != id);
  }
};
const OpenTeam = (id) => {
  if (arrayUser.value.includes(id) == false) {
    arrayUser.value.push(id);
  } else {
    arrayUser.value = arrayUser.value.filter((x) => x != id);
  }
};
const options = ref({
  pageNo: 0,
  pageSize: 100,
});
const status = ref([
  { value: "-3", label: "Đã xóa" },
  { value: "-2", label: "Trả lại" },
  { value: "-1", label: "Hủy" },
  { value: "0", label: "Mới tạo" },
  { value: "1", label: "Đang trình" },
  { value: "2", label: "Hoàn thành" },
  { value: "3", label: "Thu hồi" },
]);
const statuspasss = ref([
  { id: 0, text: "Mới lập", class: "rqlap" },
  { id: 1, text: "Chờ duyệt", class: "rqchoduyet" },
  { id: 2, text: "Chấp thuận", class: "rqchapthuan" },
  { id: -2, text: "Từ chối", class: "rqtuchoi" },
  { id: -1, text: "Hủy", class: "rqhuy" },
  { id: 3, text: "Thu hồi", class: "rqthuhoi" },
  { id: -3, text: "Xóa", class: "rqxoa" },
]);
const loadMainData = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  axios
    .post(
      baseURL + "/api/DictionaryProc/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "request_master_list_Team",
            par: [{ par: "user_id", va: user.user_id }],
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
        x.STT = options.value.pageNo * options.value.pageSize + (i + 1);
        x.status_display = status.value.filter(
          (z) => z.value == x.status,
        )[0].label;
      });
      datalists.value = data;
      swal.close();
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
const listButton = ref([
  {
    value: "",
    label: "Tất cả",
    style: { background: "#eb6833", color: "#fff" },
    count: "",
    active: true,
  },
  {
    value: "",
    label: "Chờ duyệt",
    style: { background: "#33c9dc", color: "#fff" },
    count: "",
    active: false,
  },
  {
    value: "",
    label: "Đã duyệt",
    style: { background: "#2196F3", color: "#fff" },
    count: "",
    active: false,
  },

  {
    value: "",
    label: "Từ chối",
    style: { background: "#f17ac7", color: "#fff" },
    count: "",
    active: false,
  },
  {
    value: "",
    label: "Hoàn thành",
    style: { background: "#6dd230", color: "#fff" },
    count: "",
    active: false,
  },
  {
    value: "",
    label: "Quá hạn",
    style: { background: "#F00000", color: "#fff" },
    count: "",
    active: false,
  },
]);
emitter.on("SideBarRequest", (obj) => {
  showDetailRequest.value = false;
  selectedRequestID.value = null;
});
const PositionSideBar = ref("right");
emitter.on("psbRequest", (obj) => {
  PositionSideBar.value = obj;
});
const cpnDetailRequest = ref(0);
const forceRerenderDetail = () => {
  cpnDetailRequest.value += 1;
};
const showDetailRequest = ref(false);
const selectedRequestID = ref();
const widthWindow = ref(window.screen.availWidth);

const hideall = () => {
  loadMainData();
};
const openViewRequest = (dataRequest) => {
  forceRerenderDetail();
  showDetailRequest.value = true;
  selectedRequestID.value = dataRequest.data.request_id;
};
const dictionarys = ref([]);
const resetFilter = () => {};
const filter = () => {};
const search = () => {};
onMounted(() => {
  loadData();
  loadMainData();
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <div class="header">
      <Toolbar class="outline-none border-none">
        <template #start>
          <toolbar_search_request
            :options="options"
            :dictionarys="dictionarys"
            :search="search"
            :resetFilter="resetFilter"
            :filter="filter"
          ></toolbar_search_request>
        </template>
        <template #end>
          <Button
            @click="openAddDialog('Thêm mới đề xuất')"
            label="Thêm mới"
            icon="pi pi-plus"
            class="mr-2"
          />
          <!-- <Button
                    icon="pi pi-trash"
                    label="Xóa"
                    class="p-button-danger mr-2"
                    @click="deleteRequest()"
                /> -->
          <Button
            @click="refresh()"
            class="p-button-outlined p-button-secondary mr-2"
            icon="pi pi-refresh"
            label="Tải lại"
          />
        </template>
      </Toolbar>
    </div>
    <div class="main-content">
      <div
        class="col-12 flex bg-white p-0 text-3xl font-bold text-center justify-content-evenly py-2"
      >
        Tổng hợp đề xuất
      </div>
      <div class="col-12 flex px-0">
        <div class="col-3 pl-0 pt-0">
          <div
            class="col-12 p-0 flex text-center align-items-center text-xl font-bold text-white bg-blue-400 py-3"
          >
            <div class="px-2">Cơ cấu tổ chức</div>
          </div>
          <div
            class="col-12 bg-white p-0 stack"
            style=""
          >
            <div
              v-for="(item, index) in listTeams"
              :key="index"
            >
              <span
                class="font-bold col flex align-items-center"
                :class="[{ actived: item.actived }]"
              >
                <span
                  style=""
                  class="hover_expand"
                  @click="OpenOrg(item.organization_id)"
                >
                  <icon
                    class="text-xl pi pi-angle-right px-1"
                    v-if="arrayTeam.includes(item.organization_id) == false"
                  >
                  </icon>
                  <icon
                    class="text-xl pi pi-angle-down px-1"
                    v-else
                  >
                  </icon>
                </span>
                <span class="px-2"> {{ item.organization_name }}</span>
              </span>
              <span v-if="arrayTeam.includes(item.organization_id) == true">
                <div
                  class="font-normal col-12 align-items-center"
                  v-for="(item2, index2) in item.teams"
                  :key="index2"
                >
                  <span class="pl-4 py-1">
                    <span
                      @click="OpenTeam(item2.request_team_id)"
                      class="hover_expand"
                    >
                      <icon
                        class="text-xl pi pi-angle-right px-1"
                        v-if="
                          arrayUser.includes(item2.request_team_id) == false
                        "
                      >
                      </icon>
                      <icon
                        class="text-xl pi pi-angle-down px-1"
                        v-else
                      >
                      </icon>
                    </span>
                    <span class="px-2">{{ item2.request_team_name }}</span>
                  </span>
                  <span
                    v-if="arrayUser.includes(item2.request_team_id) == true"
                  >
                    <div
                      v-for="(item3, index3) in item2.team_members"
                      :key="index3"
                    >
                      <span
                        class="col-12 p-0 py-1 pl-6 flex align-items-center justify-content-center"
                        :class="[{ actived: item3.actived }]"
                      >
                        <span class="col-1 p-0"> {{ index3 + 1 }}</span>
                        <span class="col-2 p-0">
                          <Avatar
                            :image="basedomainURL + item3.avatar"
                            size="small"
                            :style="
                              item3.avatar
                                ? 'background-color: #2196f3'
                                : 'background:' +
                                  bgColor[
                                    (item3.full_name
                                      ? item3.full_name.length
                                      : item3.user_id.length) % 7
                                  ]
                            "
                            shape="circle"
                          />
                        </span>

                        <span class="col-9 p-0">
                          {{ item3.full_name ?? item3.user_id }}
                        </span>
                      </span>
                    </div></span
                  >
                </div>
              </span>
            </div>
          </div>
        </div>
        <div class="col-9 p-0 pl-2">
          <div class="col-12 flex p-0 bg-white">
            <div
              v-for="(item, index) in listButton"
              :key="index"
              class="col"
            >
              <div
                class="item font bold text-xl align-items-center justify-content-center line-height-2"
                :class="[{ type_active: item.active == true }]"
                :style="[item.style]"
              >
                <span>{{ item.count }}</span>
                <span>{{ item.label }}</span>
              </div>
            </div>
          </div>
          <DataTable
            :value="datalists"
            :paginator="true"
            :rows="options.pageSize"
            :scrollable="true"
            scrollHeight="flex"
            :rowHover="true"
            :showGridlines="true"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
            :rowsPerPageOptions="[100, 200, 300, 500]"
            responsiveLayout="scroll"
            style="
              overflow-y: auto;
              overflow-x: hidden;
              min-height: calc(100vh - 19rem);
              max-height: calc(100vh - 19rem);
            "
            @row-click="openViewRequest"
          >
            <Column
              header="STT"
              field="STT"
              class="align-items-center justify-content-center max-w-3rem"
            ></Column>
            <Column
              header="Mã đề xuất"
              field="request_code"
              headerClass="align-items-center justify-content-center text-center max-w-8rem "
              bodyClass="align-items-center justify-content-center  text-center max-w-8rem word-break-break-all"
            ></Column>
            <Column
              header="Tên đề xuất"
              field="request_name"
              headerClass="align-items-center justify-content-center "
            ></Column>
            <Column
              header="Người lập"
              field="created_by_full_name"
              headerClass="align-items-center justify-content-center max-w-10rem "
              bodyClass="align-items-center justify-content-center text-center max-w-10rem word-break-break-word"
            ></Column>
            <Column
              header="Ngày lập"
              field="created_date"
              class="align-items-center justify-content-center max-w-8rem"
            >
              <template #body="data">
                {{
                  moment(new Date(data.data.created_date)).format("DD/MM/YYYY")
                }}
              </template></Column
            >
            <Column
              header="Trạng thái"
              field="status_display"
              class="align-items-center justify-content-center text-center max-w-7rem"
            ></Column>
            <Column
              header="Tiến độ"
              field="TienDo"
              class="align-items-center justify-content-center text-center max-w-7rem"
            >
              <template #body="data">
                <!-- {{ (data.data.TienDo = 50) }} -->
                <div style="display: none">
                  {{
                    data.index % 2 == 0
                      ? (data.data.TienDo = 50)
                      : (data.data.TienDo = 0)
                  }}
                </div>
                <ProgressBar
                  v-if="data.data.TienDo > 0"
                  :value="data.data.TienDo || 0"
                  :show-value="true"
                  class="w-full"
                >
                </ProgressBar>
                <div v-else>0%</div>
              </template>
            </Column>
            <Column
              header="Người duyệt"
              field="sign_users"
              class="align-items-center justify-content-center text-center max-w-10rem"
            >
              <template #body="data">
                <div style="display: none">
                  {{
                    data.index % 2 == 0
                      ? (data.data.sign_users = [
                          "Nguyễn Thị Hương",
                          "Trần Cao Duy",
                          "Dữ liệu Fake",
                        ])
                      : (data.data.sign_users = [
                          "Nguyễn Thị Thu Phương",
                          "Trần Cao Duy",
                          "Dữ liệu Fake",
                        ])
                  }}
                </div>
                <div class="block">
                  <span
                    v-for="(user, index) in data.data.sign_users"
                    :key="index"
                  >
                    - {{ user }}<br />
                  </span>
                </div>
              </template>
            </Column>
            <Column
              header="Ngày hoàn thành"
              field="completed_date"
              class="align-items-center justify-content-center text-center max-w-10rem"
            >
              <template #body="data">
                <div v-if="data.data.completed_date">
                  {{
                    moment(new Date(data.data.completed_date)).format(
                      "DD/MM/YYYY",
                    )
                  }}
                </div>
              </template>
            </Column>
            <Column
              header="Số giờ"
              field="date"
              class="align-items-center justify-content-center text-center max-w-5rem"
            >
              <template #body="data">
                <div v-if="data.data.times_processing_max">
                  {{ data.data.SoNgayHan }} /
                  {{ data.data.times_processing_max }}
                </div>
              </template>
            </Column>
          </DataTable>
        </div>
      </div>
    </div>
  </div>
  <Sidebar
    class="sidebar-request"
    v-model:visible="showDetailRequest"
    :position="'right'"
    :style="{
      width:
        PositionSideBar == 'right'
          ? widthWindow > 1800
            ? '65vw'
            : '75vw'
          : '100%',
      'min-height': '100vh !important',
    }"
    :showCloseIcon="false"
    @hide="hideall()"
  >
    <detail_request
      :isShow="showDetailRequest"
      :id="selectedRequestID"
      :key="cpnDetailRequest"
      :listStatusRequests="statuspasss"
    >
    </detail_request>
  </Sidebar>
</template>

<style lang="scss" scoped>
.header {
  height: 4rem;
}
.hover_expand {
  height: 2rem;
  width: 2rem;
  cursor: pointer;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
  user-select: none;
  display: -webkit-inline-box;
  display: -ms-inline-flexbox;
  display: inline-flex;
  -webkit-box-align: center;
  -ms-flex-align: center;
  align-items: center;
  -webkit-box-pack: center;
  -ms-flex-pack: center;
  justify-content: center;
  vertical-align: middle;
  overflow: hidden;
  position: relative;
}
.hover_expand:hover {
  color: #495057;
  border-color: transparent;
  background: #e9ecef;
  border-radius: 50%;
}
.actived {
  cursor: pointer;
  background-color: #ffdead;
}
.button_filter {
  width: 10rem;
  height: 10rem;
}
.item {
  position: relative;
  display: -webkit-box;
  display: -ms-flexbox;
  display: flex;
  -webkit-box-orient: vertical;
  -webkit-box-direction: normal;
  -ms-flex-direction: column;
  flex-direction: column;
  min-width: 0;
  word-wrap: break-word;
  background-color: #fff;
  background-clip: border-box;
  border: 1px solid rgba(0, 0, 0, 0.125);
  border-radius: 0.25rem;
  transition: transform 0.3s !important;
  height: 75px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.23);
  background-color: #ff8b4e;
  color: white;
  cursor: pointer;
}
.item:hover {
  transform: scale(0.9) !important;
  box-shadow: 10px 10px 15px rgba(0, 0, 0, 0.23) !important;
  cursor: pointer !important;
}
.type_active {
  transform: scale(0.9) !important;
  box-shadow: 10px 10px 15px rgba(0, 0, 0, 0.23) !important;
  border: solid 5px #f4d03f !important;
}
.stack {
  overflow-y: auto;
  overflow-x: hidden;
  min-height: calc(100vh - 16rem);
  max-height: calc(100vh - 16rem);
}
.word-break-break-word {
  word-break: break-word;
}
</style>
