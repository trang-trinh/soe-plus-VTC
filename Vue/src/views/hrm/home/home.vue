<script setup>
import { onMounted, inject, ref, watch } from "vue";
import { encr } from "../../../util/function";
import { useToast } from "vue-toastification";
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
const options = ref([]);
const organizations = ref([]);

//init
const initOrganization = () => {
  axios
    .post(
      baseURL + "/api/hrm/callProc",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_home_organizations",
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
        let data = response.data.data;
        if (data != null) {
          var tbs = JSON.parse(data);
          if (tbs[0] != null && tbs[0].length > 0) {
            organizations.value = tbs[0];
          } else {
            organizations.value = [];
          }
        }
      }
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
  initOrganization();
});
</script>
<template>
  <div class="surface-100 dashboard">
    <div class="d-grid formgrid m-1">
      <div class="col-12 md:col-12 test">
        <template v-for="(item, index) in organizations">
          <div
            class="zoom"
            :style="{ backgroundColor: bgColor[index], color: '#fff' }"
            @click="goRouter('calendarenact')"
          >
            <div class="card-body h-full">
              <div class="format-grid-center h-full">
                <h2 class="m-0">{{ item.short_name }}</h2>
                <h1 class="my-2" style="word-break: break-all">
                  {{ item.total }}
                </h1>
              </div>
            </div>
          </div>
        </template>
      </div>
      <div class="col-8 md:col-8">
        <div class="card m-1">
          <div class="card-header" style="cursor: pointer">
            <span>.</span>
          </div>
          <div class="card-body carousel-hidden-p-link" style="height: 400px">
            <Carousel
              v-show="[].length > 0"
              :value="[]"
              :numVisible="4"
              :numScroll="4"
              :circular="false"
              orientation="vertical"
              verticalViewPortHeight="400px"
            >
              <template #item="slotProps">
                <div
                  class="grid-item carousel-item"
                  @click="
                    goRouter('/news/direct/details', {
                      name: '-orient-' + slotProps.data.news_id,
                    })
                  "
                >
                  <div class="d-grid formgrid px-2">
                    <div class="col-12 md:col-12 p-0 pl-0">
                      <div class="d-grid formgrid">
                        <div class="col-12 md:col-12 p-0 flex pb-2">
                          <div>
                            <img
                              v-if="slotProps.data.is_hot"
                              style="
                                width: 40px;
                                height: 20px;
                                margin-right: 12px;
                              "
                              :src="basedomainURL + '/Portals/News/new.jpg'"
                              alt="new"
                            />
                          </div>
                          <div>
                            <span
                              class="limit-line"
                              :class="slotProps.data.is_hot ? 'font-bold' : ''"
                              >{{ slotProps.data.title }}</span
                            >
                          </div>
                        </div>
                        <div class="col-12 md:col-12 p-0">
                          <div class="description">
                            <i class="pi pi-clock"></i>
                            <span class="ml-2">{{
                              slotProps.data.approved_date
                            }}</span>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0 pt-2">
                      <div class="description">
                        <span class="limit-line">{{ slotProps.data.des }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </template>
            </Carousel>
            <div
              v-show="datanews == null || datanews.length == 0"
              class="w-full h-full format-flex-center"
            >
              <span class="description">Hiện chưa có dữ liệu</span>
            </div>
          </div>
        </div>
      </div>
      <div class="col-4 md:col-4">
        <div class="card m-1">
          <div class="card-header" style="cursor: pointer">
            <span>.</span>
          </div>
          <div class="card-body carousel-hidden-p-link" style="height: 400px">
            <Carousel
              v-show="[].length > 0"
              :value="[]"
              :numVisible="4"
              :numScroll="4"
              :circular="false"
              orientation="vertical"
              verticalViewPortHeight="400px"
            >
              <template #item="slotProps">
                <div
                  class="grid-item carousel-item"
                  @click="
                    goRouter('/news/direct/details', {
                      name: '-orient-' + slotProps.data.news_id,
                    })
                  "
                >
                  <div class="d-grid formgrid px-2">
                    <div class="col-12 md:col-12 p-0 pl-0">
                      <div class="d-grid formgrid">
                        <div class="col-12 md:col-12 p-0 flex pb-2">
                          <div>
                            <img
                              v-if="slotProps.data.is_hot"
                              style="
                                width: 40px;
                                height: 20px;
                                margin-right: 12px;
                              "
                              :src="basedomainURL + '/Portals/News/new.jpg'"
                              alt="new"
                            />
                          </div>
                          <div>
                            <span
                              class="limit-line"
                              :class="slotProps.data.is_hot ? 'font-bold' : ''"
                              >{{ slotProps.data.title }}</span
                            >
                          </div>
                        </div>
                        <div class="col-12 md:col-12 p-0">
                          <div class="description">
                            <i class="pi pi-clock"></i>
                            <span class="ml-2">{{
                              slotProps.data.approved_date
                            }}</span>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0 pt-2">
                      <div class="description">
                        <span class="limit-line">{{ slotProps.data.des }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </template>
            </Carousel>
            <div
              v-show="datanews == null || datanews.length == 0"
              class="w-full h-full format-flex-center"
            >
              <span class="description">Hiện chưa có dữ liệu</span>
            </div>
          </div>
        </div>
      </div>
      <div class="col-4 md:col-4">
        <div class="card m-1">
          <div class="card-header" style="cursor: pointer">
            <span>.</span>
          </div>
          <div class="card-body carousel-hidden-p-link" style="height: 400px">
            <Carousel
              v-show="[].length > 0"
              :value="[]"
              :numVisible="4"
              :numScroll="4"
              :circular="false"
              orientation="vertical"
              verticalViewPortHeight="400px"
            >
              <template #item="slotProps">
                <div
                  class="grid-item carousel-item"
                  @click="
                    goRouter('/news/direct/details', {
                      name: '-orient-' + slotProps.data.news_id,
                    })
                  "
                >
                  <div class="d-grid formgrid px-2">
                    <div class="col-12 md:col-12 p-0 pl-0">
                      <div class="d-grid formgrid">
                        <div class="col-12 md:col-12 p-0 flex pb-2">
                          <div>
                            <img
                              v-if="slotProps.data.is_hot"
                              style="
                                width: 40px;
                                height: 20px;
                                margin-right: 12px;
                              "
                              :src="basedomainURL + '/Portals/News/new.jpg'"
                              alt="new"
                            />
                          </div>
                          <div>
                            <span
                              class="limit-line"
                              :class="slotProps.data.is_hot ? 'font-bold' : ''"
                              >{{ slotProps.data.title }}</span
                            >
                          </div>
                        </div>
                        <div class="col-12 md:col-12 p-0">
                          <div class="description">
                            <i class="pi pi-clock"></i>
                            <span class="ml-2">{{
                              slotProps.data.approved_date
                            }}</span>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0 pt-2">
                      <div class="description">
                        <span class="limit-line">{{ slotProps.data.des }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </template>
            </Carousel>
            <div
              v-show="datanews == null || datanews.length == 0"
              class="w-full h-full format-flex-center"
            >
              <span class="description">Hiện chưa có dữ liệu</span>
            </div>
          </div>
        </div>
      </div>
      <div class="col-4 md:col-4">
        <div class="card m-1">
          <div
            class="card-header"
            style="cursor: pointer"
          >
            <span>.</span>
          </div>
          <div class="card-body carousel-hidden-p-link" style="height: 400px">
            <Carousel
              v-show="[].length > 0"
              :value="[]"
              :numVisible="4"
              :numScroll="4"
              :circular="false"
              orientation="vertical"
              verticalViewPortHeight="400px"
            >
              <template #item="slotProps">
                <div
                  class="grid-item carousel-item"
                  @click="
                    goRouter('/news/direct/details', {
                      name: '-orient-' + slotProps.data.news_id,
                    })
                  "
                >
                  <div class="d-grid formgrid px-2">
                    <div class="col-12 md:col-12 p-0 pl-0">
                      <div class="d-grid formgrid">
                        <div class="col-12 md:col-12 p-0 flex pb-2">
                          <div>
                            <img
                              v-if="slotProps.data.is_hot"
                              style="
                                width: 40px;
                                height: 20px;
                                margin-right: 12px;
                              "
                              :src="basedomainURL + '/Portals/News/new.jpg'"
                              alt="new"
                            />
                          </div>
                          <div>
                            <span
                              class="limit-line"
                              :class="slotProps.data.is_hot ? 'font-bold' : ''"
                              >{{ slotProps.data.title }}</span
                            >
                          </div>
                        </div>
                        <div class="col-12 md:col-12 p-0">
                          <div class="description">
                            <i class="pi pi-clock"></i>
                            <span class="ml-2">{{
                              slotProps.data.approved_date
                            }}</span>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0 pt-2">
                      <div class="description">
                        <span class="limit-line">{{ slotProps.data.des }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </template>
            </Carousel>
            <div
              v-show="datanews == null || datanews.length == 0"
              class="w-full h-full format-flex-center"
            >
              <span class="description">Hiện chưa có dữ liệu</span>
            </div>
          </div>
        </div>
      </div>
      <div class="col-4 md:col-4">
        <div class="card m-1">
          <div
            class="card-header"
            style="cursor: pointer"
          >
            <span>.</span>
          </div>
          <div class="card-body carousel-hidden-p-link" style="height: 400px">
            <Carousel
              v-show="[].length > 0"
              :value="[]"
              :numVisible="4"
              :numScroll="4"
              :circular="false"
              orientation="vertical"
              verticalViewPortHeight="400px"
            >
              <template #item="slotProps">
                <div
                  class="grid-item carousel-item"
                  @click="
                    goRouter('/news/direct/details', {
                      name: '-orient-' + slotProps.data.news_id,
                    })
                  "
                >
                  <div class="d-grid formgrid px-2">
                    <div class="col-12 md:col-12 p-0 pl-0">
                      <div class="d-grid formgrid">
                        <div class="col-12 md:col-12 p-0 flex pb-2">
                          <div>
                            <img
                              v-if="slotProps.data.is_hot"
                              style="
                                width: 40px;
                                height: 20px;
                                margin-right: 12px;
                              "
                              :src="basedomainURL + '/Portals/News/new.jpg'"
                              alt="new"
                            />
                          </div>
                          <div>
                            <span
                              class="limit-line"
                              :class="slotProps.data.is_hot ? 'font-bold' : ''"
                              >{{ slotProps.data.title }}</span
                            >
                          </div>
                        </div>
                        <div class="col-12 md:col-12 p-0">
                          <div class="description">
                            <i class="pi pi-clock"></i>
                            <span class="ml-2">{{
                              slotProps.data.approved_date
                            }}</span>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-12 md:col-12 p-0 pt-2">
                      <div class="description">
                        <span class="limit-line">{{ slotProps.data.des }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </template>
            </Carousel>
            <div
              v-show="datanews == null || datanews.length == 0"
              class="w-full h-full format-flex-center"
            >
              <span class="description">Hiện chưa có dữ liệu</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<style scoped>
@import url(../../calendar/component/stylecalendar.css);
.test {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  grid-auto-rows: minmax(100px, auto);
  column-gap: 15px;
  row-gap: 15px;
}
.dashboard {
  overflow: auto;
  max-height: calc(100vh - 50px);
}
.d-grid {
  display: flex;
  flex-wrap: wrap;
}
.d-lang-table {
  height: calc(100vh - 190px);
  overflow-y: auto;
}
.inputanh {
  border: 1px solid #ccc;
  width: 96px;
  height: 96px;
  cursor: pointer;
  padding: 1px;
}
.ipnone {
  display: none;
}
.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
.format-flex-center {
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
}
.format-grid-center {
  display: grid;
  align-items: center;
  justify-content: center;
  text-align: center;
}
.form-group {
  display: grid;
  margin-bottom: 1rem;
}
.form-group > label {
  margin-bottom: 0.5rem;
}
.ip36 {
  width: 100%;
}
.p-ulchip {
  margin: 0;
  margin-top: 0.5rem;
  padding: 0;
  list-style: none;
}
.p-lichip {
  float: left;
}
.p-multiselect-label {
  height: 100%;
  display: flex;
  align-items: center;
}

.dashboard .card {
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
.dashboard .card-header {
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
.dashboard .card-body {
  -webkit-box-flex: 1;
  -ms-flex: 1 1 auto;
  flex: 1 1 auto;
  padding: 1rem;
  overflow: hidden;
}
.zoom {
  cursor: pointer;
  border-radius: 0.25rem;
  box-shadow: 0 2px 4px rgb(0 0 0 / 23%);
  transition: transform 0.3s !important;
}
.zoom:hover {
  transform: scale(0.9) !important;
  box-shadow: 10px 10px 15px rgb(0 0 0 / 23%) !important;
  cursor: pointer !important;
}
.grid-item {
  display: grid;
}
.flex-item {
  display: flex;
}
.limit-line {
  text-overflow: ellipsis;
  overflow: hidden;
  column-gap: initial;
  -webkit-line-clamp: 2;
  display: -webkit-box;
  -webkit-box-orient: vertical;
}
.description {
  color: #aaa;
  font-size: 12px;
}
.carousel-item {
  cursor: pointer;
  padding-bottom: 0.5rem !important;
}
.row-item {
  cursor: pointer;
  padding: 0.5rem !important;
}
.carousel-item:hover,
.row-item:hover {
  background-color: aliceblue;
}
span.online {
  position: absolute;
  display: block;
  width: 14px;
  height: 14px;
  background-color: rgb(98, 203, 0);
  border-radius: 50%;
  right: 0;
  bottom: 0;
  border: 2px solid #fff;
}
.scroll-outer {
  visibility: hidden;
}
.scroll-inner,
.scroll-outer:hover,
.scroll-outer:focus {
  visibility: visible;
}
</style>
