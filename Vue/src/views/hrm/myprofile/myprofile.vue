<script setup>
import { onMounted, ref, inject } from "vue";
import { encr } from "../../../util/function";
import { useToast } from "vue-toastification";
import moment from "moment";
import comview1 from "./component/comview1.vue";
import comview2 from "./component/comview2.vue";
import comview3 from "./component/comview3.vue";
import comview4 from "./component/comview4.vue";
import comview6 from "./component/comview6.vue";
import comview7 from "./component/comview7.vue";
import comview8 from "./component/comview8.vue";
import comview9 from "./component/comview9.vue";
import comview10 from "./component/comview10.vue";
import comview11 from "./component/comview11.vue";

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

//Declare
const options = ref({
  loading: true,
  user_id: store.getters.user.user_id,
  search: "",
  pageNo: 1,
  pageSize: 25,
  total: 0,
  sort: "created_date desc",
  orderBy: "desc",
  tab: 1,
});
const tabs = ref([
  { id: 1, title: "Thông tin chung", icon: "", total: 0 },
  { id: 2, title: "Thông tin khác", icon: "", total: 0 },
  { id: 3, title: "Hợp đồng", icon: "", total: 0 },
  { id: 4, title: "Chấm công", icon: "", total: 0 },
  { id: 5, title: "Phiếu lương", icon: "", total: 0 },
  { id: 11, title: "Bảo hiểm", icon: "", total: 0 },
  { id: 6, title: "Phép năm", icon: "", total: 0 },
  { id: 7, title: "Đào tạo", icon: "", total: 0 },
  { id: 8, title: "Quyết định", icon: "", total: 0 },
  { id: 9, title: "Khen thưởng/Kỷ luật", icon: "", total: 0 },
  { id: 10, title: "Tài liệu", icon: "", total: 0 },
]);
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const profile = ref({});

//Fucntion
const activeTab = (tab) => {
  options.value.tab = tab.id;
};

//init
const iniData = (ref) => {
  if (ref) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  profile.value = {};
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_myprofile_get",
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
        let data = JSON.parse(response.data.data);
        if (data != null) {
          if (data[0] != null && data[0].length > 0) {
            profile.value = data[0][0];

            const startDate = moment(
              profile.value.recruitment_date || new Date()
            );
            const endDate = moment(new Date());
            profile.value.duration = moment.duration(endDate.diff(startDate));
            profile.value.diffyear = profile.value.duration.years();
            profile.value.diffmonth = profile.value.duration.months();
            profile.value.diffday = profile.value.duration.days();
          } else {
            profile.value = {};
            swal.fire({
              title: "Thông báo!",
              text: "Tài khoản truy cập cùa bạn chưa liên kết đến hồ sơ!",
              icon: "error",
              confirmButtonText: "OK",
            });
            return;
          }
        }
      }
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
onMounted(() => {
  iniData();
});
</script>
<template>
  <div class="surface-100 p-2" style="overflow: hidden">
    <Toolbar class="outline-none surface-0 border-none pb-0">
      <template #start>
        <div class="flex">
          <div class="relative mr-5">
            <Avatar
              v-bind:label="
                profile.avatar
                  ? ''
                  : (profile.profile_last_name ?? '')
                      .substring(0, 1)
                      .toUpperCase()
              "
              v-bind:image="
                profile.avatar
                  ? basedomainURL + profile.avatar
                  : basedomainURL + '/Portals/Image/noimg.jpg'
              "
              :style="{
                background: bgColor[1 % 7],
                color: '#ffffff',
                width: '7rem',
                height: '7rem',
                fontSize: '1.5rem !important',
                borderRadius: '50%',
              }"
              size="xlarge"
              class="border-radius"
            />
          </div>
          <div class="format-center mr-5">
            <div class="text-left">
              <div class="mb-2">
                <h3 class="m-0" :style="{ color: '#0078d4' }">
                  {{ profile.profile_name }}
                </h3>
              </div>
              <div class="mb-2">
                <b>{{ profile.profile_code }}</b>
              </div>
              <div>
                <span>{{ profile.title_name }}</span>
                <span v-if="profile.title_name && profile.department_name"
                  >/</span
                >
                <span>{{ profile.department_name }}</span>
              </div>
            </div>
          </div>
          <div class="format-center">
            <div class="text-left">
              <div class="mb-2">
                <label
                  ><span
                    >Ngày vào công ty:
                    <b>{{ profile.recruitment_date_name }}</b></span
                  ></label
                >
              </div>
              <div>
                <label>
                  <span>
                    Thâm niên công tác:
                    <b>
                      <span v-if="profile.diffyear > 0">
                        {{ profile.diffyear }} năm
                      </span>
                      <span v-if="profile.diffmonth > 0">
                        {{ profile.diffmonth }} tháng
                      </span>
                      <span v-if="profile.diffday > 0">
                        {{ profile.diffday }} ngày
                      </span>
                    </b>
                  </span>
                </label>
              </div>
            </div>
          </div>
        </div>
      </template>
      <template #end>
        <Button
          @click="refresh()"
          icon="pi pi-pencil"
          label="Cập nhật thông tin"
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
              <span>{{ tab.title }}</span>
            </a>
          </li>
        </ul>
      </div>
    </div>
    <div
      :style="{
        overflowY: 'auto',
        minHeight: 'unset',
        maxHeight: 'calc(100vh - 220px)',
      }"
    >
      <comview1 v-if="options.tab === 1 && profile.profile_id != null" :profile_id="profile.profile_id" />
      <comview2 v-if="options.tab === 2 && profile.profile_id != null" :profile_id="profile.profile_id" />
      <comview3 v-if="options.tab === 3 && profile.profile_id != null" :profile_id="profile.profile_id" />
      <comview4 v-if="options.tab === 4 && profile.profile_id != null" :profile_id="profile.profile_id" />
      <comview6 v-if="options.tab === 6 && profile.profile_id != null" :profile_id="profile.profile_id" />
      <comview7 v-if="options.tab === 7 && profile.profile_id != null" :profile_id="profile.profile_id" />
      <comview8 v-if="options.tab === 8 && profile.profile_id != null" :profile_id="profile.profile_id" />
      <comview9 v-if="options.tab === 9 && profile.profile_id != null" :profile_id="profile.profile_id" />
      <comview10 v-if="options.tab === 10 && profile.profile_id != null" :profile_id="profile.profile_id" />
      <comview11 v-if="options.tab === 11 && profile.profile_id != null" :profile_id="profile.profile_id" />
    </div>
  </div>
</template>
<style scoped>
@import url(../contract/component/stylehrm.css);
.box-info .card {
  border: none;
  border-radius: 0;
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
}
.box-info .card-header {
  -webkit-box-flex: 1;
  -ms-flex: 1 1 auto;
  flex: 1 1 auto;
  padding: 1rem;
  overflow: hidden;
  border-bottom: solid 1px rgba(0, 0, 0, 0.1);
  font-size: 15px;
  font-weight: bold;
  color: #005a9e;
}
.box-info .card-body {
  -webkit-box-flex: 1;
  -ms-flex: 1 1 auto;
  flex: 1 1 auto;
  padding: 1rem;
  overflow: hidden;
}
</style>
<style lang="scss" scoped>
::v-deep(.border-radius) {
  img {
    border-radius: 50%;
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.p-timeline) {
  background-color: #fff;
  padding: 1rem;
  .p-timeline-event .p-timeline-event-opposite {
    display: none !important;
  }
  .p-timeline-event:nth-child(even) {
    flex-direction: row;
  }
  .p-card-body {
    padding: 0;
  }
}
</style>
