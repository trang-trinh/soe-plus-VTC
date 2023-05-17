<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";
import { encr, checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const cryoptojs = inject("cryptojs");
//Khai báo
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const toast = useToast();
const isFirst = ref(true);
const datalists = ref([]);
const isSaveNews = ref(false);
 
const sttNews = ref(1);
const options = ref({
  IsNext: true,
  sort: "news_id DESC",
  search: "",
  pageno: 1,
  pagesize: 10,
  pageno1: 0,
  pagesize1: 15,
  pageno2: 0,
  pagesize2: 10,
  loading: true,
  totalRecords: null,
  totalNotiRecords: null,
});
const news = ref({
  is_order: 1,
  title: "",
  des: "",
  contents: "",
  image: "",
  is_hot: false,
  IsLang: store.getters.langid,
  Menu_ID: store.state.idNews,
  news_type: 0,
  key_words: "",
  IsWriter: "",
  start_date: "",
  end_date: null,
  status: false,
});
const checkLoadImg = ref(false);
const datalistsave = ref([]);
const onPageNoti = (event) => {
  options.value.pageno1 = event.page;
  loadDataNotify();
};
const checkSearch = ref(false);
const removeSearch = () => {
  options.value.search = null;
  onSearch();
};
const onSearch = () => {
  checkSearch.value = true;
  if (options.value.search == null || options.value.search == "")
    checkSearch.value = false;
  loadCount();
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "news_main_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "category_id", va: options.value.category_id },

              { par: "news_type", va: 0 },
              { par: "key_words", va: options.value.key_words },
              { par: "search", va: options.value.search },
              { par: "status", va: 2 },
              { par: "is_hot", va: options.value.is_hot },
              { par: "is_notify", va: options.value.is_notify },
              { par: "datefilter", va: 0 },
              { par: "pageno", va: options.value.pageno1 },
              { par: "pagesize", va: options.value.pagesize1 },
              { par: "start_date", va: options.value.start_date },
              { par: "end_date", va: options.value.end_date },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        // relateNew.value = [];

        datalists.value = data;
      } else {
        datalists.value = null;
      }

      options.value.loading = false;
    })
    .catch((error) => {
     
      
      options.value.loading = false;
 
    });
  loadDataNotify();
};
const onPage = (event) => {
  location.href = "#scrollTop1";
  options.value.pageno = event.page + 1;
  options.value.pagesize = event.rows;
  loadData(true);
};
const loadCountNotify = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "news_main_count",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "category_id", va: options.value.category_id },

              { par: "news_type", va: 1 },
              { par: "key_words", va: options.value.key_words },
              { par: "search", va: options.value.search },
              { par: "status", va: 2 },
              { par: "is_hot", va: options.value.is_hot },
              { par: "is_notify", va: options.value.is_notify },
              { par: "start_date", va: options.value.start_date },
              { par: "end_date", va: options.value.end_date },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalNotiRecords = data[0].totalRecords;
      } else options.value.totalNotiRecords = 0;
    })
    
};
const loadCount = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "news_main_count",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "category_id", va: options.value.category_id },

              { par: "news_type", va: 0 },
              { par: "key_words", va: options.value.key_words },
              { par: "search", va: options.value.search },
              { par: "status", va: 2 },
              { par: "is_hot", va: options.value.is_hot },
              { par: "is_notify", va: options.value.is_notify },

              { par: "start_date", va: options.value.start_date },
              { par: "end_date", va: options.value.end_date },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords - 11;
      } else options.value.totalRecords = 0;
    })
    
};
const loadData = () => {
  loadCount();

  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "news_main_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "category_id", va: options.value.category_id },

              { par: "news_type", va: 0 },
              { par: "key_words", va: options.value.key_words },
              { par: "search", va: options.value.search },
              { par: "status", va: 2 },
              { par: "is_hot", va: options.value.is_hot },
              { par: "is_notify", va: options.value.is_notify },
              { par: "datefilter", va: 0 },
              { par: "pageno", va: options.value.pageno },
              { par: "pagesize", va: options.value.pagesize },
              { par: "start_date", va: options.value.start_date },
              { par: "end_date", va: options.value.end_date },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        // relateNew.value = [];
        datalists.value = [];
        data.forEach((element, i) => {
          element.is_order = i + 1;

          // relateNew.value.push({
          //   name: element.title,
          //   code: element.news_id,
          // });

          if (!element.created_date_show) element.created_date_show = null;
          element.created_date_show = moment(
            new Date(element.created_date)
          ).format("DD/MM/YYYY");
          if (!element.start_date_show) element.start_date_show = null;
          element.start_date_show = moment(new Date(element.start_date)).format(
            "DD/MM/YYYY hh:mm:ss"
          );
          if (element.news_id != news.value.news_id && i < 10)
            datalists.value.push(element);
        });
      } else {
        datalists.value = [];
      }

      options.value.loading = false;
    })
    .catch((error) => {
     

      options.value.loading = false;

     
    });
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "news_main_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "category_id", va: options.value.category_id },

              { par: "news_type", va: 0 },
              { par: "key_words", va: options.value.key_words },
              { par: "search", va: options.value.search },
              { par: "status", va: 2 },
              { par: "is_hot", va: options.value.is_hot },
              { par: "is_notify", va: options.value.is_notify },
              { par: "datefilter", va: 1 },
              { par: "pageno", va: options.value.pageno2 },
              { par: "pagesize", va: options.value.pagesize2 },
              { par: "start_date", va: options.value.start_date },
              { par: "end_date", va: options.value.end_date },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        datalistViews.value = data;
      }
      options.value.loading = false;
    })
    .catch((error) => {
      

      options.value.loading = false;

    
    });
};
const datalistsNotify = ref();
const loadDataNotify = () => {
  loadCountNotify();

  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "news_main_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "category_id", va: options.value.category_id },

              { par: "news_type", va: 1 },
              { par: "key_words", va: options.value.key_words },
              { par: "search", va: options.value.search },
              { par: "status", va: 2 },
              { par: "is_hot", va: options.value.is_hot },
              { par: "is_notify", va: options.value.is_notify },
              { par: "datefilter", va: 0 },
              { par: "pageno", va: options.value.pageno1 },
              { par: "pagesize", va: options.value.pagesize1 },
              { par: "start_date", va: options.value.start_date },
              { par: "end_date", va: options.value.end_date },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      if (data.length > 0) {
        // relateNew.value = [];
        data.forEach((element, i) => {
          element.is_order = i + 1;
          if (!element.created_date_show) element.created_date_show = null;
          element.created_date_show = moment(
            new Date(element.created_date)
          ).format("DD/MM/YYYY");
          if (!element.start_date_show) element.start_date_show = null;
          element.start_date_show = moment(new Date(element.start_date)).format(
            "DD/MM/YYYY hh:mm:ss"
          );
          let arrI = [];
          if (element.url_file != "" && element.url_file != null) {
            element.url_file.split(",").forEach((item) => {
              if (item != "" && item != null) arrI.push(item);
            });
            element.url_file = arrI;
          }
        });

        // Array.from(new Set(relateNew.value));
        datalistsNotify.value = data;
      } else {
        datalistsNotify.value = [];
      }

      options.value.loading = false;
    })
    .catch((error) => {
    
      options.value.loading = false;
 
    });
};
const filterMonth = ref();
const dataListFiles = ref([]);
const toggleFilterMonth = (event, value) => {
  dataListFiles.value = value;
  filterMonth.value.toggle(event);
};
const datalistViews = ref([]);
const datalistNews = ref([]);
const isDetailsNews = ref(false);
const showDetails = (data) => {
  localStorage.setItem("news", data.news_id);
  let srcMs = removeVietnameseTones(data.title);
  store.commit("setnews", data);
  if (router)
    router.push({
      path:
        "/news/direct/" +
        srcMs.replace(/','|'.'/g, "").replace(/\s+/g, "-") +
        "-orient-" +
        data.news_id,
    });
};
function removeVietnameseTones(str) {
  str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
  str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
  str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
  str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
  str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
  str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
  str = str.replace(/đ/g, "d");
  str = str.replace(/À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ|Ặ|Ẳ|Ẵ/g, "A");
  str = str.replace(/È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ/g, "E");
  str = str.replace(/Ì|Í|Ị|Ỉ|Ĩ/g, "I");
  str = str.replace(/Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ|Ợ|Ở|Ỡ/g, "O");
  str = str.replace(/Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ/g, "U");
  str = str.replace(/Ỳ|Ý|Ỵ|Ỷ|Ỹ/g, "Y");
  str = str.replace(/Đ/g, "D");
  // Some system encode vietnamese combining accent as individual utf-8 characters
  // Một vài bộ encode coi các dấu mũ, dấu chữ như một kí tự riêng biệt nên thêm hai dòng này
  str = str.replace(/\u0300|\u0301|\u0303|\u0309|\u0323/g, ""); // ̀ ́ ̃ ̉ ̣  huyền, sắc, ngã, hỏi, nặng
  str = str.replace(/\u02C6|\u0306|\u031B/g, ""); // ˆ ̆ ̛  Â, Ê, Ă, Ơ, Ư
  // Remove extra spaces
  // Bỏ các khoảng trắng liền nhau
  str = str.replace(/ + /g, " ");
  str = str.trim();
  // Remove punctuations
  // Bỏ dấu câu, kí tự đặc biệt
  str = str.replace(
    /!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'|\"|\&|\#|\[|\]|~|\$|_|`|-|{|}|\||\\/g,
    " "
  );
  return str;
}
const loadDataMain = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "news_main_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "category_id", va: options.value.category_id },

              { par: "news_type", va: 0 },
              { par: "key_words", va: options.value.key_words },
              { par: "search", va: options.value.search },
              { par: "status", va: 2 },
              { par: "is_hot", va: options.value.is_hot },
              { par: "is_notify", va: options.value.is_notify },
              { par: "datefilter", va: 0 },
              { par: "pageno", va: 0 },
              { par: "pagesize", va: 10 },
              { par: "start_date", va: options.value.start_date },
              { par: "end_date", va: options.value.end_date },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
          
        datalistNews.value = data;
        if (datalistsave.value.length == 0) {
          datalistsave.value = data[0];
          checkLoadImg.value = true;
        }
      }
      options.value.loading = false;
    })
    .catch((error) => {
     

      options.value.loading = false;

    
    });
};
onMounted(() => {   
  loadDataMain();
  loadDataNotify();
  loadData();
  return {
    options,
  };
});
</script>
<template>
  <div>
    <div class="sticky top-0 absolute">
      <div class="w-full flex align-items-center p-2">
        <div class="w-3 font-bold text-2xl text-blue-600 flex">
          <i class="pi pi-id-card mr-2" style="font-size: 1.5rem" />
          <div>Tin tức & sự kiện</div>
        </div>
        <div class="w-4 flex">
          <span class="w-full p-input-icon-left">
            <i class="pi pi-search" />
            <InputText
              type="text"
              v-model="options.search"
              spellcheck="false"
              @keyup.enter="onSearch()"
              placeholder="Tìm kiếm"
            />
          </span>
          <Button
            v-if="options.search"
            @click="removeSearch"
            icon="pi pi-times"
            class="ml-2"
          ></Button>
        </div>
      </div>
    </div>
    <div class="p-0 pt-2 surface-200"></div>
    <div
      class="overflow-y-auto overflow-x-hidden"
      style="height: calc(100vh - 100px)"
    >
      <div class="grid">
        <div class="col-12 flex py-0">
          <div class="surface-200 col-9 pr-0 pb-0" v-if="!checkSearch">
            <div class="col-12 field flex p-0" style="height: 530px">
              <div
                class="col-8 p-3 pb-0 surface-0 cursor-pointer mr-2 relative"
                @click="showDetails(datalistsave)"
                v-if="datalistsave"
              >
                <div class="col-12 p-0">
                  <img
                    v-if="checkLoadImg"
                    class="w-full h-25rem"
                    :src="
                      datalistsave.image
                        ? basedomainURL + datalistsave.image
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    :style="
                      datalistsave.image
                        ? 'object-fit:cover'
                        : 'object-fit:contain; background-color:#eeeeee'
                    "
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                  />
                   <div
            class="
              align-items-center
              justify-content-center
              p-4
              text-center
              m-auto
            "
           v-else
          >
            <img src="../../assets/background/nodata.png" height="144" />
            <h3 class="m-1">Không có dữ liệu</h3>
          </div>
                  <!-- <div
                    style="
                      translate: transform(-50%, -50%);
                      top: 50%;
                      right: 50%;
                      object-fit: cover;
                    "
                    v-else
                    class="absolute"
                  >
                    <ProgressSpinner
                      style="width: 50px; height: 50px"
                      strokeWidth="8"
                      fill="var(--surface-ground)"
                      animationDuration=".5s"
                    />
                  </div> -->
                </div>
                <div
                  class="
                    col-12
                    p-0
                    text-2xl text-primary text-justify text-3line
                    hyper-link
                  "
                >
                  {{ datalistsave.title }}
                </div>
                <div
                  class="pt-2 col-12 p-0 text-justify text-4line line-height-3"
                  style="font-size: 14px"
                >
                  {{ datalistsave.des }}
                </div>
                <div class="pt-2 col-12 p-0">
                  <Toolbar
                    class="w-full surface-0 outline-none border-none p-0"
                  >
                    <template #start>
                      <div v-if="datalistsave.approved_date">
                        <i class="pi pi-clock pr-1" style="font-size: 12px"></i>
                        {{
                          moment(new Date(datalistsave.approved_date)).format(
                            "HH:mm DD/MM/YYYY"
                          )
                        }}
                      </div>
                    </template>
                    <template #end>
                      <div v-if=" datalistsave.views_count>0">
                        <i class="pi pi-eye pr-1" style="font-size: 12px"></i>
                        {{
                          datalistsave.views_count
                            ? datalistsave.views_count
                            : 0
                        }}
                        người xem
                      </div>
                    </template>
                  </Toolbar>
                </div>
              </div>
              <div
                class="
                  col-4
                  surface-0
                  overflow-design overflow-x-hidden
                  pl-3
                  pr-0
                "
              >
                <DataView
                  class="w-full h-full e-sm"
                  responsiveLayout="scroll"
                  :scrollable="true"
                  layout="list"
                  :lazy="true"
                  :value="datalistNews"
                  :loading="options.loading"
                  :rowHover="true"
                >
                  <template #list="slotProps">
                    <div
                      class="grid p-0 w-full"
                      v-if="slotProps.data.news_id != datalistsave.news_id"
                    >
                      <div class="col-12 p-0 pt-2">
                        <div class="col-12 p-0 pt-1 flex">
                          <div
                            class="col-4 p-2 pt-1 pb-0 cursor-pointer"
                            @click="showDetails(slotProps.data)"
                          >
                            <img
                              class="w-full h-4rem"
                              style="border-radius: 5%; object-fit: cover"
                              :src="
                                slotProps.data.image
                                  ? basedomainURL + slotProps.data.image
                                  : basedomainURL + '/Portals/Image/noimg.jpg'
                              "
                            />
                          </div>
                          <div
                            class="col-8 p-0 cursor-pointer"
                            @click="showDetails(slotProps.data)"
                          >
                            <div class="col-12 p-0 pb-1 pr-2">
                              <div
                                class="
                                  font-bold
                                  text-3line text-justify
                                  line-height-3
                                  hyper-link
                                "
                              >
                                {{ slotProps.data.title }}
                              </div>
                            </div>
                            <div
                              class="col-12 pb-0 pl-0 text-500 p-0"
                              style="font-size: 12px"
                            >
                              <i
                                class="pi pi-clock"
                                style="font-size: 12px"
                              ></i>
                              {{
                                moment(
                                  new Date(slotProps.data.approved_date)
                                ).format("HH:mm DD/MM/YYYY")
                              }}
                            </div>
                          </div>
                        </div>
                        <div class="col-12 pt-0">
                          <div
                            class="
                              text-justify text-3line
                              line-height-3
                              text-600
                            "
                            style="font-size: 12px"
                          >
                            {{ slotProps.data.des }}
                          </div>
                        </div>
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
                      v-if="!isFirst"
                    >
                      <img
                        src="../../assets/background/nodata.png"
                        height="144"
                      />
                      <h3 class="m-1">Không có dữ liệu</h3>
                    </div>
                  </template>
                </DataView>
              </div>
            </div>
            <div class="col-12 p-0 surface-0" id="scrollTop1">
              <DataView
                class="w-full h-full e-sm"
                @page="onPage($event)"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink  RowsPerPageDropdown"
                :rowsPerPageOptions="[10, 20, 30, 50]"
                responsiveLayout="scroll"
                scrollHeight="flex"
                :scrollable="true"
                layout="list"
                :lazy="true"
                :value="datalists"
                :loading="options.loading"
                :paginator="true"
                :rows="options.pagesize"
                :totalRecords="options.totalRecords"
                :rowHover="true"
                :showGridlines="true"
                :pageLinkSize="options.pagesize"
                currentPageReportTemplate=""
              >
                <template #list="slotProps">
                  <div
                    class="grid p-0 w-full"
                    v-if="slotProps.data.news_id != datalistsave.news_id"
                  >
                    <div class="col-12 px-3">
                      <div class="col-12">
                        <div
                          class="
                            font-bold
                            text-color-secondary text-xl text-justify
                            cursor-pointer
                            text-3line
                            hyper-link
                          "
                          @click="showDetails(slotProps.data)"
                        >
                          {{ slotProps.data.title }}
                        </div>
                      </div>
                      <div class="col-12 flex">
                        <div
                          class="col-3 p-2 pl-0 pt-0 cursor-pointer"
                          @click="showDetails(slotProps.data)"
                        >
                          <img
                            class="w-full h-10rem"
                            style="border-radius: 5px; object-fit: cover"
                            :src="
                              slotProps.data.image
                                ? basedomainURL + slotProps.data.image
                                : basedomainURL + '/Portals/Image/noimg.jpg'
                            "
                          />
                        </div>
                        <div class="col-9 p-0" style="font-size: 14px">
                          <div
                            class="col-12 p-0 pb-2 text-500"
                            style="font-size: 12px"
                          >
                            <i class="pi pi-clock" style="font-size: 12px"></i>
                            {{
                              moment(
                                new Date(slotProps.data.approved_date)
                              ).format("HH:mm DD/MM/YYYY")
                            }}
                          </div>
                          <div
                            class="
                              col-12
                              p-0
                              text-justify
                              line-height-3
                              text-5line
                            "
                            style="font-size: 14px"
                          >
                            {{ slotProps.data.des }}
                          </div>
                        </div>
                      </div>
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
                    v-if="!isFirst"
                  >
                    <img
                      src="../../assets/background/nodata.png"
                      height="144"
                    />
                    <h3 class="m-1">Không có dữ liệu</h3>
                  </div>
                </template>
              </DataView>
            </div>
          </div>
          <div class="surface-200 col-9 pr-0 pb-0 pt-2" v-else>
            <div
              class="
                col-12
                pt-3
                pb-3
                surface-0
                text-4xl
                font-bold
                align-center
                format-center
                flex
              "
            >
              KẾT QUẢ TÌM KIẾM
            </div>
            <div class="col-12 p-0 surface-0" id="scrollTop1">
              <DataView
                class="w-full h-full e-sm"
                @page="onPage($event)"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink "
                responsiveLayout="scroll"
                scrollHeight="flex"
                :scrollable="true"
                layout="list"
                :lazy="true"
                :value="datalists"
                :loading="options.loading"
                :paginator="options.totalRecords > options.pagesize"
                :rows="options.pagesize"
                :totalRecords="options.totalRecords"
                :rowHover="true"
                :showGridlines="true"
                :pageLinkSize="options.pagesize"
                currentPageReportTemplate=""
              >
                <template #list="slotProps">
                  <div class="grid p-0 w-full">
                    <div class="col-12 px-3">
                      <div class="col-12">
                        <div
                          class="
                            font-bold
                            text-color-secondary text-xl text-justify
                            cursor-pointer
                            text-3line
                            hyper-link
                          "
                          @click="showDetails(slotProps.data)"
                        >
                          {{ slotProps.data.title }}
                        </div>
                      </div>
                      <div class="col-12 flex">
                        <div
                          class="col-3 p-2 pl-0 pt-0 cursor-pointer"
                          @click="showDetails(slotProps.data)"
                        >
                          <img
                            class="w-full h-10rem"
                            style="border-radius: 5px; object-fit: cover"
                            :src="
                              slotProps.data.image
                                ? basedomainURL + slotProps.data.image
                                : basedomainURL + '/Portals/Image/noimg.jpg'
                            "
                          />
                        </div>
                        <div class="col-9 p-0" style="font-size: 14px">
                          <div
                            class="col-12 p-0 pb-2 text-500"
                            style="font-size: 12px"
                          >
                            <i class="pi pi-clock" style="font-size: 12px"></i>
                            {{
                              moment(
                                new Date(slotProps.data.approved_date)
                              ).format("HH:mm DD/MM/YYYY")
                            }}
                          </div>
                          <div
                            class="
                              col-12
                              p-0
                              text-justify
                              line-height-3
                              text-5line
                            "
                            style="font-size: 14px"
                          >
                            {{ slotProps.data.des }}
                          </div>
                        </div>
                      </div>
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
                    <img
                      src="../../assets/background/nodata.png"
                      height="144"
                    />
                    <h3 class="m-1">Không có dữ liệu</h3>
                  </div>
                </template>
              </DataView>
            </div>
            <!-- <div v-else>
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
          </div> -->
          </div>
          <div class="surface-200 col-3">
            <div class="col-12 p-0 field" style="height: 530px">
              <Panel class="p-panel-unset-bottom">
                <template #header>
                  <div
                    class="
                      w-full
                      format-center
                      text-2xl
                      bg-orange-500
                      p-2
                      text-0
                    "
                    style="border-radius: 3px 3px 0px 0px"
                  >
                    <i class="pi pi-bell mx-2 text-2xl" /> Thông báo
                  </div>
                </template>
                <div
                  class="overflow-design p-0"
                  style="height: 477px; margin-right: -6px"
                >
                  <DataView
                    class="w-full h-full e-sm"
                    @page="onPageNoti($event)"
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink "
                    responsiveLayout="scroll"
                    :scrollable="true"
                    scrollHeight="flex"
                    layout="list"
                    :lazy="true"
                    :value="datalistsNotify"
                    :loading="options.loading"
                    :paginator="options.totalNotiRecords > options.pagesize1"
                    :rows="options.pagesize1"
                    :totalRecords="options.totalNotiRecords"
                    :rowHover="true"
                    :showGridlines="true"
                    :pageLinkSize="options.pagesize1"
                    currentPageReportTemplate=""
                  >
                    <template #list="slotProps">
                      <div class="w-full">
                        <Panel
                          class="w-full h-full p-panel-unset"
                          :toggleable="false"
                        >
                          <template #header>
                            <div
                              class="
                                h-full
                                align-items-center
                                justify-content-center
                              "
                            >
                              <div class="text-justify">
                                <span
                                  class="
                                    font-bold
                                    text-color-secondary text-justify text-4line
                                    cursor-pointer
                                    hyper-link
                                  "
                                  @click="showDetails(slotProps.data)"
                                  >{{ slotProps.data.title }}</span
                                >
                              </div>
                              <div
                                class="col-12 pb-0 pl-0 text-500 p-0 pt-2"
                                style="font-size: 12px"
                              >
                                <span
                                  ><i
                                    class="pi pi-clock"
                                    style="font-size: 12px"
                                  ></i>
                                  {{
                                    moment(
                                      new Date(slotProps.data.approved_date)
                                    ).format("HH:mm DD/MM/YYYY")
                                  }}</span
                                >
                                <span v-if="slotProps.data.url_file != null">
                                  <i
                                    class="
                                      pi pi-paperclip
                                      px-2
                                      text-600
                                      cursor-pointer
                                    "
                                    aria:haspopup="true"
                                    aria-controls="overlay_panelMonth"
                                    @click="
                                      toggleFilterMonth(
                                        $event,
                                        slotProps.data.url_file
                                      )
                                    "
                                  ></i>
                                </span>
                              </div>
                            </div>
                          </template>
                        </Panel>
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
                        v-if="!isFirst"
                      >
                        <img
                          src="../../assets/background/nodata.png"
                          height="144"
                        />
                        <h3 class="m-1">Không có dữ liệu</h3>
                      </div>
                    </template>
                  </DataView>
                </div>
              </Panel>
            </div>
            <div class="col-12 p-0 field">
              <Panel class="p-panel-unset-bottom">
                <template #header>
                  <div
                    class="w-full format-center text-2xl p-2 text-0"
                    style="
                      border-radius: 3px 3px 0px 0px;
                      background-color: #0078d4;
                    "
                  >
                    <i class="pi pi-image mx-2 text-2xl" /> Tin xem nhiều
                  </div>
                </template>
                <div class="h-full overflow-y-auto p-0">
                  <DataView
                    class="pr-0"
                    @page="onPage($event)"
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink "
                    responsiveLayout="scroll"
                    :scrollable="true"
                    layout="list"
                    :lazy="true"
                    :value="datalistViews"
                    :loading="options.loading"
                    :paginator="false"
                    :rows="options.pagesize"
                    :totalRecords="options.totalRecords"
                    :rowHover="true"
                    :showGridlines="true"
                    :pageLinkSize="options.pagesize"
                    currentPageReportTemplate=""
                  >
                    <template #list="slotProps">
                      <div class="grid p-0 py-2 w-full">
                        <div class="col-12 p-0">
                          <div class="col-12 p-0 pt-2 flex">
                            <div
                              class="col-3 p-2 pt-0 cursor-pointer"
                              @click="showDetails(slotProps.data)"
                            >
                              <img
                                class="w-full h-4rem"
                                style="border-radius: 5px; object-fit: cover"
                                :src="
                                  slotProps.data.image
                                    ? basedomainURL + slotProps.data.image
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                              />
                            </div>
                            <div
                              class="col-9 p-0 cursor-pointer"
                              @click="showDetails(slotProps.data)"
                            >
                              <div class="col-12 p-0 pb-1 pr-2">
                                <div
                                  class="
                                    font-bold
                                    text-color-secondary text-justify text-3line
                                    hyper-link
                                  "
                                >
                                  {{ slotProps.data.title }}
                                </div>
                              </div>
                              <div
                                class="col-12 pb-0 pl-0 text-500 p-0"
                                style="font-size: 12px"
                              >
                                <i
                                  class="pi pi-clock"
                                  style="font-size: 12px"
                                ></i>
                                {{
                                  moment(
                                    new Date(slotProps.data.approved_date)
                                  ).format("HH:mm DD/MM/YYYY")
                                }}
                              </div>
                            </div>
                          </div>
                          <div class="col-12 pb-0 pt-0">
                            <div
                              class="
                                text-justify text-3line
                                line-height-3
                                text-600
                              "
                              style="font-size: 12px"
                            >
                              {{ slotProps.data.des }}
                            </div>
                          </div>
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
                        v-if="!isFirst"
                      >
                        <img
                          src="../../assets/background/nodata.png"
                          height="144"
                        />
                        <h3 class="m-1">Không có dữ liệu</h3>
                      </div>
                    </template>
                  </DataView>
                </div>
              </Panel>
            </div>
            <!-- <div class="col-12 p-0 field h-30rem">
            <Panel>
              <template #header>
                <div
                  class="w-full format-center text-2xl bg-cyan-500 p-2"
                  style="border-radius: 10px 10px 0px 0px"
                >
                  <i class="pi pi-image mx-2 text-2xl" /> Cá nhân tiêu biểu
                </div>
              </template>
              Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do
              eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut
              enim ad minim veniam, quis nostrud exercitation ullamco laboris
              nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in
              reprehenderit in voluptate velit esse cillum dolore eu fugiat
              nulla pariatur. Excepteur sint occaecat cupidatat non proident,
              sunt in culpa qui officia deserunt mollit anim id est laborum.
            </Panel>
          </div> -->
          </div>
        </div>
      </div>
    </div>
    <OverlayPanel
      ref="filterMonth"
      appendTo="body"
      class="p-0 m-0"
      :showCloseIcon="false"
      id="overlay_panelMonth"
      style="width: 300px"
    >
      <div class="pb-2" v-for="(item, index) in dataListFiles" :key="index">
        <div class="flex align-items-center cursor-pointer">
          <a
            :href="basedomainURL + item"
            download
            class="w-full no-underline flex align-items-center"
          >
            <img
              :src="
                basedomainURL +
                '/Portals/Image/file/' +
                (item ? item.substring(item.indexOf('.') + 1) : 'filess') +
                '.png'
              "
              @error="
                $event.target.src =
                  basedomainURL + '/Portals/Image/file/filess.png'
              "
              style="width: 32px; height: 32px; object-fit: contain"
            />
            <div class="pl-2">
              {{ item.substring(16) }}
            </div>
          </a>
        </div>
      </div>
    </OverlayPanel>
  </div>
</template>
<style scoped>
.text-3line {
  text-overflow: ellipsis;
  overflow: hidden;
  column-gap: initial;
  -webkit-line-clamp: 2;
  display: -webkit-box;
  -webkit-box-orient: vertical;
}
.text-4line {
  text-overflow: ellipsis;
  overflow: hidden;
  column-gap: initial;
  -webkit-line-clamp: 3;
  display: -webkit-box;
  -webkit-box-orient: vertical;
}
.text-5line {
  text-overflow: ellipsis;
  overflow: hidden;
  column-gap: initial;
  -webkit-line-clamp: 5;
  display: -webkit-box;
  -webkit-box-orient: vertical;
}
.hyper-link {
  color: black;
}
.hyper-link:hover {
  color: #1c80cf !important;
}

.overflow-design::-webkit-scrollbar-thumb {
  background-color: white !important;
}
.overflow-design:hover::-webkit-scrollbar-thumb {
  background-color: #e0e0e0 !important ;
}
.overflow-design {
  overflow-y: scroll !important;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-panel) {
  .p-panel-header {
    padding: 0;
    border: unset;
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.p-panel.p-panel-unset) {
  .p-panel-header {
    padding: 12px 8px;
    background-color: #fff !important;
  }
  .p-panel-content {
    padding: 0 !important;
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.p-panel.p-panel-unset-bottom) {
  .p-panel-content {
    padding-right: 0px !important;
    padding-bottom: 0 !important;
    border: unset;
  }
}
::v-deep(.p-dataview) {
  .p-dataview-content {
    overflow-x: hidden;
  }
}
</style>

