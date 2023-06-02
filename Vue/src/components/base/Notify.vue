<script setup>
//Import
import { inject, onMounted, ref } from "vue";
import moment from "moment";
import ModuleMenu from "../../components/base/ModuleMenu.vue";
import Notify from "../../components/base/Notify.vue";

import { useRouter, useRoute } from "vue-router";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const axios = inject("axios"); // inject axios
const swal = inject("$swal");
const route = useRoute();
const basedomainURL = fileURL;
const width1 = ref(window.screen.availWidth);
const filterModule = ref();

//Khai báo biến
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const router = inject("router");
const store = inject("store");
const storetheme = inject("storetheme");
const showSidebarNoti = ref(false);
const count_noti = ref();
const notis = ref([]);
const idNoti = ref();

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
//Get arguments
const props = defineProps({
  showSidebarNoti: Boolean,
});
// nhận dữ liệu
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "reloadViewTask":
      const noti_id = obj.data;
      notis.value.forEach((item) => {
        if (typeof item == "object") {
          if (item.notify_id == noti_id) {
            item.status = 1;
          }
        }
      });
      break;
  }
});
//load noti

// noti
const opition = ref({
  search: "",
  PageNo: 1,
  PageSize: 15,
  totalRecords: 0,
  status: null,
});
//lọc
const options_status = ref([
  { name: "Tất cả", code: null },
  { name: "Chưa đọc", code: 0 },
  { name: "Đã đọc", code: 1 },
]);
const modules = ref([]);
const filterButs = ref();
const checkFilter = ref(false);
const toggleFilter = (event) => {
  filterButs.value.toggle(event);
};
const filterNoti = (check) => {
  opition.value.module_key = filterModule.value
    ? filterModule.value.module_key
    : "";
  opition.value.PageNo = 1;
  opition.value.PageSize = 15;
  opition.value.totalRecords = 0;
  checkFilter.value = true;
  filterButs.value.hide();
  notis.value = [];
  loadNoti(true);
};
const reFilterNoti = () => {
  checkFilter.value = false;
  opition.value = {
    search: "",
    PageNo: 1,
    PageSize: 15,
    totalRecords: 0,
    status: null,
    module_key: null,
  };
  filterModule.value = "";
  filterButs.value.hide();
  notis.value = [];
  loadNoti(true);
};
const list_modules = [
  {
    module_key: "M8",
    module_name: "Trao đổi",
    type: 0,
    is_link: "chat_message/detail",
  },
  {
    module_key: "M10",
    module_name: "Đăng ký cắt cơm",
    type: 2,
    is_link: "bookingmeal/detail",
  },
  {
    module_key: "M10",
    module_name: "Theo dõi hàng ngày",
    type: 1,
    is_link: "/booking/booking_daily",
  },
  {
    module_key: "M3",
    module_name: "Văn bản",
    type: 0,
    is_link: "docreceive/detail",
  },
  {
    module_key: "M4",
    module_name: "Công việc",
    type: -1,
    is_link: "taskmaindetail",
  },
  {
    module_key: "M7",
    module_name: "Thiết bị",
    type: 0,
    is_link: "/device/doc_approved",
  },
  {
    module_key: "M7",
    module_name: "Thiết bị",
    type: 1,
    is_link: "/device/accepthandover",
  },
  {
    module_key: "M7",
    module_name: "Thiết bị",
    type: 2,
    is_link: "/device/repair",
  },
  {
    module_key: "M7",
    module_name: "Thiết bị",
    type: 3,
    is_link: "/device/acceptinventory",
  },
  {
    module_key: "M7",
    module_name: "Thiết bị",
    type: 4,
    is_link: "/device/recall",
  },
  {
    module_key: "M7",
    module_name: "Thiết bị",
    type: 5,
    is_link: "/device/follows",
  },
  {
    module_key: "M2",
    module_name: "Lịch họp tuần",
    type: 0,
    is_link: "calendardetail",
  },
  {
    module_key: "M2",
    module_name: "Lịch công tác",
    type: 1,
    is_link: "calendarplantripdetail",
  },
  {
    module_key: "M2",
    module_name: "Lịch trực ban",
    type: 2,
    is_link: "calendardutyapproved",
  },
  {
    module_key: "M11",
    module_name: "Kho dữ liệu",
    type: 2,
    is_link: "files/file_main_detail",
  },
];
const showDetail = ref(false);
const selectedTaskID = ref();
const goToNotify = (item) => {
  if (!item.seen) {
    // update status seen
    axios
      .put(baseURL + "/api/Notify/Update_Status", item, config)
      .then((response) => {
        //do something...
        // if (response.data.err != "1") {
        //   emitter.emit("emitData", { type: "goToNoti", data: item.senhub_id });
        // }
      });
  }

  let mds = list_modules.filter(
    (x) => x.type == item.is_type && x.module_key == item.module_key,
  );

  if (mds.length > 0) {
    emitter.emit("emitData", {
      type: "closeNoti",
      data: {
        showSidebarNoti: false,
      },
    }); //close bar
    if (mds[0].module_key == "M2") {
      // Lich cong tac
      if (mds[0].type === 0 || mds[0].type === 1) {
        router.push({
          name: mds[0].is_link,
          params: { id: item.id_key } || {},
        });
      } else {
        router.push({ name: mds[0].is_link });
      }
    } else if (mds[0].module_key == "M4") {
      //Cong viec
      if (item.type == 11) {
        router.push({ name: "TaskReviewReport", params: {} }).then(() => {
          router.go(0);
        });
      } else if (item.type == 12) {
        router.push({ name: "TaskPersonCreateReport", params: {} }).then(() => {
          router.go(0);
        });
      } else
        router.push({ name: "taskmain", params: {} }).then(() => {
          router.go(0);
        });
    } else if (mds[0].module_key == "M7") {
      // Thiet bi
      router.push(mds[0].is_link);
    } else if (mds[0].module_key == "M11") {
      // Kho dl
      router
        .push({
          name: mds[0].is_link,
          params: { id: item.id_key, type: item.is_type } || {},
        })
        .then(() => {
          router.go(0);
        });
    } else {
      router
        .push({ name: mds[0].is_link, params: { id: item.id_key } || {} })
        .then(() => {
          router.go(0);
        });
    }
  }
};
const onMoreNoti = () => {
  opition.value.PageNo += 1;
  loadNoti();
};
const loadNoti = (f) => {
  if (f) countNoti();
  axios
    .post(
      baseURL + "/api/Notify/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_noti_list",
            par: [
              { par: "pageno", va: opition.value.PageNo },
              { par: "pagesize", va: opition.value.PageSize },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: opition.value.status },
              { par: "module_key", va: opition.value.module_key },
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
      if (data[0].length > 0) {
        notis.value = notis.value.concat(data[0]);
        count_noti.value = notis.value.length;
      } else {
        notis.value = [];
        count_noti.value = 0;
      }
      showSidebarNoti.value = true;
    });
};
const closeNoti = () => {
  opition.value.status = null;
  opition.value.module_key = null;
  countNoti();
  emitter.emit("emitData", {
    type: "closeNoti",
    data: {
      showSidebarNoti: false,
    },
  });
};
const loadModule = () => {
  axios
    .post(
      baseURL + "/api/Notify/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_modules_listmodule_noti",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
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
        modules.value = data[0];
      }
    })
    .catch((error) => {
      if (error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
};
const countNoti = () => {
  notis.value = [];
  axios
    .post(
      baseURL + "/api/Notify/GetDataProc",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_noti_count_all",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "status", va: opition.value.status },
              { par: "module_key", va: opition.value.module_key },
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
        opition.value.totalRecords = data[0][0].c;
      }
    });
};

//Vue App
onMounted(() => {
  loadNoti(true);
  loadModule();
  return {};
});
</script>
<template>
  <Sidebar
    v-model:visible="showSidebarNoti"
    :baseZIndex="100"
    position="right"
    class="p-sidebar-lg overflow-hidden sidebar-noti"
    style="width: 34vw"
    :showCloseIcon="false"
    @hide="closeNoti"
  >
    <div class="grid w-full p-0">
      <div class="pl-3 flex col-12 pb-0">
        <div class="col-8">
          <h2 style="margin: 2px auto; font-size: 1.3rem">
            Danh sách thông báo ({{ opition.totalRecords }})
          </h2>
        </div>
        <div class="col-4 item-right-filter">
          <Button
            :class="
              checkFilter ? 'ml-2' : 'ml-2 p-button-secondary p-button-outlined'
            "
            style="margin-right: 10px"
            icon="pi pi-filter"
            @click="toggleFilter"
            aria-haspopup="true"
            aria-controls="overlay_panelS"
          />
          <OverlayPanel
            ref="filterButs"
            appendTo="body"
            :showCloseIcon="true"
            id="overlay_panelS"
            style="width: 350px"
            :breakpoints="{ '960px': '20vw' }"
          >
            <div class="grid formgrid m-2">
              <div class="field col-12 md:col-12 flex align-items-center">
                <div class="col-4 p-0">Trạng thái:</div>
                <Dropdown
                  v-model="opition.status"
                  :options="options_status"
                  optionLabel="name"
                  optionValue="code"
                  placeholder="Trạng thái"
                  class="col-8 p-0"
                  :showClear="true"
                />
              </div>
              <div class="field col-12 md:col-12 flex align-items-center">
                <div class="col-4 p-0">Module:</div>
                <Dropdown
                  v-model="filterModule"
                  :options="modules"
                  optionLabel="module_name"
                  placeholder="Chọn module"
                  :showClear="true"
                  class="col-8 p-0"
                >
                  <template #value="slotProps">
                    <div
                      class="flex align-items-center"
                      v-if="slotProps.value"
                    >
                      <img
                        class="icon-modules"
                        v-bind:src="basedomainURL + slotProps.value.image"
                      />
                      <div class="ml-2">
                        {{ slotProps.value.module_name }}
                      </div>
                    </div>
                    <span v-else>
                      {{ slotProps.placeholder }}
                    </span>
                  </template>
                  <template #option="slotProps">
                    <div class="country-item flex">
                      <img
                        class="icon-modules"
                        v-bind:src="basedomainURL + slotProps.option.image"
                      />
                      <div style="margin-left: 5px">
                        {{ slotProps.option.module_name }}
                      </div>
                    </div>
                  </template>
                </Dropdown>
              </div>
              <div class="col-12 field p-0">
                <Toolbar class="toolbar-filter">
                  <template #start>
                    <Button
                      @click="reFilterNoti"
                      class="p-button-outlined"
                      label="Xóa"
                    ></Button>
                  </template>
                  <template #end>
                    <Button
                      @click="filterNoti"
                      label="Lọc"
                    ></Button>
                  </template>
                </Toolbar>
              </div>
            </div>
          </OverlayPanel>
        </div>
      </div>
      <div
        style="max-height: calc(100vh - 50px); overflow: auto; width: 100%"
        v-if="notis"
      >
        <div style="position: relative">
          <div
            v-for="(item, index) in notis"
            :key="index"
            @click="goToNotify(item)"
            :class="[item.seen ? 'seen' : 'no-seen']"
          >
            <div class="grid w-full p-0 m-0">
              <div
                class="field col-12 flex m-0 cursor-pointer"
                style="
                  background-color: none;
                  display: flex;
                  align-items: center;
                  justify-content: center;
                "
              >
                <div class="col-1 p-0">
                  <Avatar
                    v-bind:label="
                      item.avatar ? '' : item.last_name.substring(0, 1)
                    "
                    v-bind:image="basedomainURL + item.avatar"
                    style="
                      background-color: #2196f3;
                      color: #ffffff;
                      width: 3rem;
                      height: 3rem;
                    "
                    :style="{
                      background: bgColor[index % 7],
                    }"
                    class="mr-2"
                    size="xlarge"
                    shape="circle"
                  />
                </div>
                <div class="col-11 p-0">
                  <div class="pt-2 ml-2">
                    <div v-if="item.full_name">
                      {{ item.full_name }}
                    </div>
                    <div
                      v-if="item.contents"
                      class="font-medium"
                      style="
                        display: -webkit-box;
                        -webkit-line-clamp: 2;
                        -webkit-box-orient: vertical;
                        overflow: hidden;
                        text-overflow: ellipsis;
                      "
                    >
                      <span v-html="item.contents"></span>
                    </div>
                    <div
                      class="fsz-xs flex w-full"
                      style="align-items: center"
                    >
                      <div
                        v-if="item.module_name"
                        class="pr-3"
                      >
                        <Tag severity="success">{{ item.module_name }}</Tag>
                      </div>
                      <div
                        class="text-sm"
                        style="margin: 2px"
                        v-if="item.created_date"
                      >
                        Ngày gửi:
                        {{
                          moment(new Date(item.created_date)).format(
                            "DD/MM/YYYY HH:mm",
                          )
                        }}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div
            class="col-12 flex show-more"
            v-if="opition.totalRecords > count_noti"
            style="justify-content: center"
          >
            <span
              class="font-bold text-600 text-lg cursor-pointer"
              style="line-height: 50px"
              @click="onMoreNoti()"
            >
              Xem thêm...</span
            >
          </div>
        </div>
      </div>
    </div>
  </Sidebar>
</template>
<style>
.sidebar-noti > .p-sidebar-content {
  overflow-y: hidden !important;
  padding-right: 0 !important;
}
</style>

<style scoped>
.seen {
  background-color: #fff;
}
.no-seen {
  background-color: #dae6f6;
}
.no-seen:hover,
.seen:hover {
  background-color: #eef3f9;
}
.icon-modules {
  width: 14px;
  height: 14px;
}
.item-right-filter {
  display: flex;
  justify-content: end;
  align-items: center;
}
.headerbar {
  background-color: #fff;
  height: 50px;
}
.scroll-item :hover {
  background-color: #f5f5f5;
  border-radius: 3px !important;
}
.scroll-item {
  border-bottom: 1px solid rgba(0, 0, 0, 0.0625) !important;
}
.show-more {
  position: absolute;
  height: 50px;
  background: linear-gradient(
    to bottom,
    rgb(245 245 245 / 17%) 10%,
    #f5f5f5 80%
  );
  bottom: -15px;
  cursor: pointer;
}
.toolbar-filter {
  border: unset;
  outline: unset;
  background-color: #fff;
  padding-bottom: 0px;
}
.p-avatar {
  font-size: 1rem !important;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-button-icon) {
  .p-badge {
    background-color: #ff0000 !important;
  }
}
::v-deep(.p-sidebar .p-sidebar-lg) {
  .p-sidebar-content {
    overflow-y: hidden !important;
  }
}

.virtualscroller-demo {
  ::v-deep(.p-virtualscroller) {
    height: 200px;
    width: 200px;
    border: 1px solid var(--surface-border);

    .scroll-item {
      background-color: var(--surface-card);
      display: flex;
      align-items: center;
    }

    .custom-scroll-item {
      flex-direction: column;
      align-items: stretch;
    }

    .odd {
      background-color: var(--surface-ground);
    }
  }

  ::v-deep(.p-horizontal-scroll) {
    .p-virtualscroller-content {
      display: flex;
      flex-direction: row;
    }

    .scroll-item {
      writing-mode: vertical-lr;
    }
  }

  ::v-deep(.custom-loading > .p-virtualscroller-loader) {
    display: block;
  }
}
</style>
