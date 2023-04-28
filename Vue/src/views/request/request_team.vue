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
const LoadCount = () => {
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
            proc: "request_team_count",
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

      listButton.value[0].count = data[0].allreq;
      listButton.value[1].count = data[0].waiting;
      listButton.value[2].count = data[0].completed;
      listButton.value[3].count = data[0].refused;
      listButton.value[4].count = data[0].finished;
      listButton.value[5].count = data[0].expired;
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
            par: [
              { par: "user_id", va: user.user_id },
              { par: "optionView", va: options.value.optionView },
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
      data.forEach((x, i) => {
        if (x.request_team_id == null) {
          x.request_team_id = -1;
          x.request_team_name = "Cá nhân";
        }
        if (x.request_form_id == null) {
          x.request_form_id = -1;
          x.request_form_name = "Đề xuất khác";
        }
        x.STT = options.value.pageNo * options.value.pageSize + (i + 1);
        x.status_display = status.value.filter(
          (z) => z.value == x.status,
        )[0].label;
        x.sign_users = ["Nguyễn Thị Hương", "Trần Cao Duy", "Dữ liệu Fake"];
        x.TienDo = 50;
      });
      let dataTeam = [];
      if (options.value.optionView == 3) {
        data.forEach((x, i) => {
          if (dataTeam.length == 0) {
            let k = {
              request_form_id: x.request_form_id,
              request_form_name: x.request_form_name,
              request_team_data: [],
            };
            dataTeam.push(k);
          } else {
            let t = dataTeam.filter(
              (z) => z.request_form_id == x.request_form_id,
            );
            if (t.length == 0) {
              let k = {
                request_form_id: x.request_form_id,
                request_form_name: x.request_form_name,
                request_team_data: [],
              };
              dataTeam.push(k);
            }
          }
        });
        dataTeam.forEach((element) => {
          data.forEach((k) => {
            if (element.request_team_data.length == 0) {
              let container = {
                request_team_id: k.request_team_id,
                request_team_name: k.request_team_name,
                team_data: [],
              };
              element.request_team_data.push(container);
            } else {
              let filter = element.request_team_data.filter(
                (u) => u.request_team_id == k.request_team_id,
              );
              if (filter.length == 0) {
                let container = {
                  request_team_id: k.request_team_id,
                  request_team_name: k.request_team_name,
                  team_data: [],
                };
                element.request_team_data.push(container);
              }
            }
          });
        });

        dataTeam.forEach((z, i) => {
          z.request_team_data.forEach((x, k) => {
            let datafilter = data.filter(
              (d) =>
                x.request_team_id == d.request_team_id &&
                z.request_form_id == d.request_form_id,
            );
            datafilter.forEach((x, i) => {
              x.STT = options.value.pageNo * options.value.pageSize + (i + 1);
            });
            x.team_data = datafilter;
          });
        });
        datalists.value = dataTeam;
      } else datalists.value = data;
      // datalists.value = [];
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
  selectedRequestID.value =
    options.value.optionView == 3
      ? dataRequest.request_id
      : dataRequest.data.request_id;
};
const dictionarys = ref([]);
const resetFilter = () => {};
const filter = () => {};
const search = () => {};
const items = ref([
  {
    status: false,
    va: 0,
    label: "Không nhóm",
    returnVa: null,
    command: () => {
      ChangeView(items.value[0]);
    },
  },
  {
    status: false,
    va: 1,
    label: "Team",
    returnVa: "request_team_id",
    command: () => {
      ChangeView(items.value[1]);
    },
  },
  {
    status: false,
    va: 2,
    label: "Loại đề xuất",
    returnVa: "request_form_id",
    command: () => {
      ChangeView(items.value[2]);
    },
  },
  {
    status: false,
    va: 3,
    label: "Loại đề xuất và team",
    returnVa: null,
    command: () => {
      ChangeView(items.value[3]);
    },
  },
]);
const menu = ref();
const toggle = (event) => {
  menu.value.toggle(event);
};
const group = ref();
const ChangeView = (item) => {
  item.status = true;
  items.value
    .filter((x) => x != item)
    .forEach((x) => {
      x.status = false;
    });
  if (item.va != 3) group.value = item.returnVa;
  else group.value = null;
  options.value.optionView = item.va ?? item;
  loadMainData();
};
const onPage = (e) => {
  let old = JSON.parse(
    JSON.stringify([options.value.pageNo, options.value.pageSize]),
  );
  if (options.value.pageNo != e.page) options.value.pageNo = e.page;
  if (options.value.pageSize != e.rows) options.value.pageNo = e.rows;
  let new1 = JSON.parse(
    JSON.stringify([options.value.pageNo, options.value.pageSize]),
  );
  console.log(old, new1, old == new1);
  if (old != new1) {
    LoadCount();
    loadData();
    loadMainData();
  }
};
onMounted(() => {
  options.value.optionView = 3;
  ChangeView(items.value[options.value.optionView]);
  LoadCount();
  loadData();
  loadMainData();
});
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <div class="header">
      <Toolbar class="outline-none border-none">
        <template #start>
          {{ group }}
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
      <div class="col-12 flex bg-white p-0 py-2">
        <div class="col-4 p-0"></div>
        <div
          class="col-4 p-0 text-3xl font-bold flex align-items-center justify-content-center"
        >
          Tổng hợp đề xuất
        </div>
        <div class="col-4 p-0 flex align-items-center justify-content-end">
          <Button
            class="mx-2"
            label="Biểu đồ"
            icon="pi pi-chart-bar"
          ></Button>
          <Button
            class="mx-2"
            icon=""
            type="button"
            label="Nhóm dữ liệu"
            @click="toggle"
            aria-haspopup="true"
            aria-controls="overlay_menu"
          />
          <OverlayPanel
            ref="menu"
            id="overlay_menu"
          >
            <div
              class="px-3 py-1 my-2 sort"
              v-for="(item, index) in items"
              :key="index"
              :class="[{ active: item.status == true }]"
              @click="item.command"
            >
              {{ item.label }}
            </div>
          </OverlayPanel>
        </div>
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
            v-if="options.optionView != 3"
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
            dataKey="request_id"
            :rowGroupMode="group != null ? 'subheader' : ''"
            :groupRowsBy="group != null ? group : ''"
            @page="onPage"
          >
            <template #empty>
              <div
                class="w-full align-items-center justify-content-center p-4 text-center"
              >
                <img
                  src="../../assets/background/nodata.png"
                  height="144"
                />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </template>
            <template #groupheader="slotProps">
              <div class="flex align-items-center gap-2">
                <span v-if="options.optionView == 2">{{
                  slotProps.data.request_form_name
                }}</span>
                <span v-if="options.optionView == 1">{{
                  slotProps.data.request_team_name
                }}</span>
              </div>
            </template>
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
          <div
            v-else
            class="p-datatable p-component p-datatable-hoverable-rows p-datatable-scrollable p-datatable-scrollable-vertical p-datatable-flex-scrollable p-datatable-responsive-scroll p-datatable-gridlines"
            data-scrollselectors=".p-datatable-wrapper"
            style="
              overflow: hidden auto;
              min-height: calc(100vh - 19rem);
              max-height: calc(100vh - 19rem);
            "
          >
            <div class="p-datatable-wrapper">
              <table
                role="table"
                class="p-datatable-table"
                v-if="datalists.length > 0"
              >
                <thead
                  class="p-datatable-thead"
                  role="rowgroup"
                >
                  <tr role="row">
                    <th
                      class="align-items-center justify-content-center max-w-3rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">STT</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center text-center max-w-8rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Mã đề xuất</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Tên đề xuất</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center max-w-10rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Người lập</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center max-w-8rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Ngày lập</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center text-center max-w-7rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Trạng thái</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center text-center max-w-7rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Tiến độ</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center text-center max-w-10rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Người duyệt</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center text-center max-w-10rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Ngày hoàn thành</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center text-center max-w-5rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Số giờ</span>
                      </div>
                    </th>
                  </tr>
                </thead>

                <tbody
                  class="p-datatable-tbody"
                  role="rowgroup"
                  v-for="(item, index) in datalists"
                  :key="index"
                >
                  <tr
                    class="p-rowgroup-header"
                    role="row"
                    style="top: 47px"
                  >
                    <td
                      colspan="9"
                      class="p-2"
                      style="background-color: #f6ddcc; font-size: 16px"
                    >
                      <div class="flex align-items-center gap-2">
                        <span>{{ item.request_form_name }}</span>
                        <!-- <span>{{ item }}</span> -->
                      </div>
                    </td>
                  </tr>
                  <div>
                    <table
                      role="table"
                      class="p-datatable-table"
                    >
                      <tbody
                        class="p-datatable-tbody"
                        role="rowgroup"
                        v-for="(team, teamIndex) in item.request_team_data"
                        :key="teamIndex"
                      >
                        <tr
                          class="p-rowgroup-header"
                          role="row"
                          style="top: 47px"
                        >
                          <td
                            colspan="9"
                            style="background-color: #d5f5e3"
                          >
                            <div class="flex align-items-center gap-2">
                              <span>{{ team.request_team_name }}</span>
                            </div>
                          </td>
                        </tr>
                        <tr
                          class=""
                          role="row"
                          v-for="(request, reqIndex) in team.team_data"
                          :key="reqIndex"
                          @click="openViewRequest(request)"
                        >
                          <td
                            class="align-items-center justify-content-center max-w-3rem"
                            role="cell"
                          >
                            {{ request.STT }}
                          </td>
                          <td
                            class="align-items-center justify-content-center text-center max-w-8rem word-break-break-all"
                            role="cell"
                          >
                            {{ request.request_code }}
                          </td>
                          <td
                            class=""
                            role="cell"
                          >
                            {{ request.request_name }}
                          </td>
                          <td
                            class="align-items-center justify-content-center text-center max-w-10rem word-break-break-word"
                            role="cell"
                          >
                            {{ request.created_by_full_name }}
                          </td>
                          <td
                            class="align-items-center justify-content-center max-w-8rem"
                            role="cell"
                          >
                            {{
                              moment(request.created_date).format("DD/MM/YYYY")
                            }}
                          </td>
                          <td
                            class="align-items-center justify-content-center text-center max-w-7rem"
                            role="cell"
                          >
                            {{ request.status_display }}
                          </td>
                          <td
                            class="align-items-center justify-content-center text-center max-w-7rem"
                            role="cell"
                          >
                            <ProgressBar
                              v-if="request.TienDo > 0"
                              :value="request.TienDo || 0"
                              :show-value="true"
                              class="w-full"
                            >
                            </ProgressBar>
                            <div v-else>0%</div>
                          </td>
                          <td
                            class="align-items-center justify-content-center text-center max-w-10rem"
                            role="cell"
                          >
                            <div class="block">
                              <span
                                v-for="(user, index) in request.sign_users"
                                :key="index"
                              >
                                - {{ user }}<br />
                              </span>
                            </div>
                          </td>
                          <td
                            class="align-items-center justify-content-center text-center max-w-10rem"
                            role="cell"
                          >
                            <div v-if="request.completed_date">
                              {{
                                moment(new Date(request.completed_date)).format(
                                  "DD/MM/YYYY",
                                )
                              }}
                            </div>
                          </td>
                          <td
                            class="align-items-center justify-content-center text-center max-w-5rem"
                            role="cell"
                          >
                            <div v-if="request.times_processing_max">
                              {{ request.SoNgayHan }} /
                              {{ request.times_processing_max }}
                            </div>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </tbody>
              </table>
              <table
                v-else
                role="table"
                class="p-datatable-table"
              >
                <thead
                  class="p-datatable-thead"
                  role="rowgroup"
                >
                  <tr role="row">
                    <th
                      class="align-items-center justify-content-center max-w-3rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">STT</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center text-center max-w-8rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Mã đề xuất</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Tên đề xuất</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center max-w-10rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Người lập</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center max-w-8rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Ngày lập</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center text-center max-w-7rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Trạng thái</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center text-center max-w-7rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Tiến độ</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center text-center max-w-10rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Người duyệt</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center text-center max-w-10rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Ngày hoàn thành</span>
                      </div>
                    </th>
                    <th
                      class="align-items-center justify-content-center text-center max-w-5rem"
                      role="cell"
                    >
                      <div class="p-column-header-content">
                        <span class="p-column-title">Số giờ</span>
                      </div>
                    </th>
                  </tr>
                </thead>

                <tbody
                  class="p-datatable-tbody"
                  role="rowgroup"
                >
                  <tr
                    class="p-datatable-emptymessage"
                    role="row"
                  >
                    <td colspan="10">
                      <div
                        data-v-e7fddb26=""
                        class="w-full align-items-center justify-content-center p-4 text-center"
                      >
                        <img
                          data-v-e7fddb26=""
                          src="/src/assets/background/nodata.png"
                          height="144"
                        />
                        <h3
                          data-v-e7fddb26=""
                          class="m-1"
                        >
                          Không có dữ liệu
                        </h3>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
            <Paginator
              :rows="options.pageSize"
              :totalRecords="datalists.length"
              template="FirstPageLink PrevPageLink PageLinks NextPageLink
            LastPageLink RowsPerPageDropdown"
              :rowsPerPageOptions="[100, 200, 300, 500]"
              @page="onPage"
            />
          </div>
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
.p-overlaypanel.overlaypanel:before {
  bottom: 100%;
  left: calc(var(--overlayArrowLeft, 0) + 6.25rem) !important;
  content: " ";
  height: 0;
  width: 0;
  position: absolute;
  pointer-events: none;
}
.active {
  background-color: #2196f3;
  color: #ffffff;
}
.sort:hover {
  background-color: #2196f3;
  cursor: pointer;
  color: #ffffff;
}
</style>
