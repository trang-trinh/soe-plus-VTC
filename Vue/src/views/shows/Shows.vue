<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";

import {  encr,checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
 const cryoptojs = inject("cryptojs");
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const toast = useToast();
const options = ref({
  IsNext: true,
  sort: "news_id DESC",
  search: "",
  pageno: 0,
  pagesize: 20,
  status: 2,
  loading: true,
  totalrecords: null,
  totalNotiRecords: null,
  key_words: null,
  typeviews: 1,
  datefilter: 3,
  
});
const listKeyWords = ref([]);
const typeViewsCount = ref([
  {
    name: "Mới cập nhật",
    code: 1,
  },
  { name: "Xem nhiều", code: 2 },

  { name: "Mới nhất", code: 3 },
]);
const datalists = ref([]);

const loadCount = () => {
   axios
    .post(
          baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "shows_main_count",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
 
          { par: "key_words", va: options.value.key_words },
          { par: "search", va: options.value.search },
          { par: "status", va: options.value.status },
        ],
            }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      if (data.length > 0) {
        options.value.totalrecords = data[0].totalRecords;
      } else options.value.totalrecords = 0;
    })
    .catch((error) => {
      
    });
};
const removeSearch = () => {
  options.value.search = null;
  loadData(true);
};
const loadData = (rf) => {

  options.value.loading=true;

  if (rf) {
  
    loadCount();
  }
   axios
    .post(
          baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "shows_main_list",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
   
          { par: "key_words", va: options.value.key_words },
          { par: "search", va: options.value.search },
          { par: "status", va: options.value.status },
          { par: "datefilter", va: options.value.datefilter },
          { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: options.value.pagesize },
        ],
          }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
         
        datalists.value = data;
      } else datalists.value = [];
      options.value.loading=false;
    })
    .catch((error) => {
      
      options.value.loading=false;
      console.log(error);
    });
};

const showDetails = (data) => {
  let srcMs = removeVietnameseTones(data.title);
  if (router)
    router.push({
      path:
        "/news/shows/" +
        srcMs.replace(/','|'.'/g, "").replace(/\s+/g, "-") +
        "-orient-" +
        data.shows_id,
    });
  // location.href=baseURL+'/Viewer/?title=' + srcMs.replace(/','|'.'/g, "").replace(/\s+/g, "-") + '&url=' + data.path;
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

const datalistNews = ref([]);
const loadDataNews = () => {
  axios
    .post(
          baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "shows_main_list",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
     
          { par: "key_words", va: options.value.key_words },
          { par: "search", va: options.value.search },
          { par: "status", va: options.value.status },
          { par: "datefilter", va: 3 },
          { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: 10 },
        ],
         }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        datalistNews.value = data;
      } else datalistNews.value = [];
    })
    .catch((error) => {
      console.log(error);
    });
};const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const onPage = (event) => {
  location.href = "#scrollTop1";
  options.value.pageno = event.page;
  options.value.pagesize = event.rows;
  loadData();
};
const datalistViews = ref([]);
const loadDataViews = () => {
    axios
    .post(
          baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "shows_main_list",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
      
          { par: "key_words", va: options.value.key_words },
          { par: "search", va: options.value.search },
          { par: "status", va: options.value.status },
          { par: "datefilter", va: 2 },
          { par: "pageno", va: options.value.pageno },
          { par: "pagesize", va: options.value.pagesize },
        ],
       }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        datalistViews.value = data;
      } else datalistViews.value = [];
    })
    .catch((error) => {
      console.log(error);
    });
};
const lengthFor=ref(0);
 
      var strs = "";
   
const loadKeyWords = () => {
  listKeyWords.value = [];
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "shows_keywords_list",
           
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      data.forEach((element) => {
        if (element.key_words != null) {
          let arr = element.key_words.split(",");
            strs="";
          if(arr.length>0){
            lengthFor.value=arr.length;
          }
      
          foreachKeyWord(arr,element.key_words)
        }
      });

      listKeyWords.value = listKeyWords.value.sort(function (a, b) {
        return a.count === b.count ? 0 : a.count > b.count ? -1 : 1;
      });
    });
};
const foreachKeyWord=( arr,str)=>{

    arr.forEach(element => {
       const item = {
              name: element,
              code: element,
              active: false,
              count: 0,
            }
              if (!strs.includes(element)) {
              listKeyWords.value.push(item);
              strs += str + ",";
            } else {
              listKeyWords.value
                .filter((x) => x.name == element)
                .forEach((x) => {
                  x.count++;
                });
            }
    });




     
}
const keyWordsFilter = () => {
  options.value.key_words = keywordsSelected.value.toString();
 
  loadData(true);
};
const removeKey=(e)=>{
 keywordsSelected.value= keywordsSelected.value.filter((x)=>x!=e);
  keyWordsFilter();
}
const keywordsSelected = ref([]);

onMounted(() => {  if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
  loadData(true);
  loadDataViews();
  loadDataNews();
  loadKeyWords();
  return {};
});
</script>
<template>

  <div class="surface-0 mt-3">
    <div class="flex w-full p-3 pt-0">
      <div class="flex w-3 p-0">
        <div class="w-6 format-center mr-2">
          <MultiSelect
            v-model="keywordsSelected"
            :options="listKeyWords"
            optionLabel="name"
            optionValue="code"
            placeholder="Tất cả"
            class="w-full"
            @change="keyWordsFilter(null, true)"
          >
            <template #value="slotProps">
              <div
                class="p-dropdown-car-value flex text-justify"
                v-if="slotProps.value"
              >
                <div class="pr-2 text-justify">
                  <i class="pi pi-sliders-h"></i>
                </div>
                <div class="text-justify flex format-center">
                  {{
                    slotProps.value.toString() == "" || slotProps.value == null
                      ? "Tất cả"
                      : slotProps.value.toString()
                  }}
                </div>
              </div>
              <span v-else>
                {{ slotProps.placeholder }}
              </span>
            </template>
          </MultiSelect>
        </div>
        <div class="w-6 mr-2">
          <Dropdown
            v-model="options.datefilter"
            :options="typeViewsCount"
            optionLabel="name"
            optionValue="code"
            placeholder="Loại trình diễn"
            class="w-full"
            @change="loadData(true)"
          >
            <template #value="slotProps">
              <div
                class="p-dropdown-car-value flex text-justify"
                v-if="slotProps.value"
              >
                <div class="pr-2 text-justify">
                  <i class="pi pi-align-left"></i>
                </div>
                <div class="text-justify flex format-center">
                  {{
                    slotProps.value == 1
                      ? "Mới cập nhật"
                      : slotProps.value == 2
                      ? "Xem nhiều"
                      : slotProps.value == 3
                      ? "Mới nhất"
                      : "Mới cập nhật"
                  }}
                </div>
              </div>
              <span v-else>
                {{ slotProps.placeholder }}
              </span>
            </template></Dropdown
          >
        </div>
      </div>
      <div class="w-3 mr-2 format-center">
        <div class="w-full flex">
          <span class="w-full p-input-icon-left">
            <i class="pi pi-search" />
            <InputText
              type="text"
              v-model="options.search"
              spellcheck="false"
              @keyup.enter="loadData(true)"
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
      <!-- <div class="w-3 mr-2 format-center">
        
      </div> -->
      <div class="w-6 mr-2 format-center " style="justify-content:right">
        <div class="h-full "  v-for="(item, index) in keywordsSelected">
         <Button v-if="index<4" style="border-radius:16px; border:unset"   class="h-full px-3 mr-2 p-chip" :label="item" icon="pi pi-times" iconPos="right" @click="removeKey(item)"  />
         <!-- <Chip  @remove="removeKey(item)" :key="index"
          :label="item" icon="pi pi-key"  class="h-full px-3 mr-2" removable /> -->
      </div>
      </div>
    </div>
    
    <div class="grid p-3 py-0 pr-0 m-0 flex">
      <div
        class="col-9 surface-400 p-0 overflow-y-auto"
        id="scrollTop1"
        style="height: calc(100vh - 115px)"
      >
        <DataView
          class="w-full h-full e-sm flex flex-column p-dataview-unset"
          :lazy="true"
          :value="datalists"
          layout="grid"
          :paginator="true"
          :rows="options.pagesize"
          :totalRecords="options.totalrecords"
          :pageLinkSize="4"
          @page="onPage($event)"
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          :rowsPerPageOptions="[20, 32, 40, 52, 80]"
          currentPageReportTemplate=""
          responsiveLayout="scroll"
          :scrollable="false"
        >
          <template #grid="slotProps">
            <div
              class="md:col-3 col-3 p-2 surface-200 card-content"
              @click="showDetails(slotProps.data)"
            >
              <Card class="no-paddcontent p-0">
                <template #title>
                  <div style="position: relative">
                    <img
                      class="w-full cursor-pointer"
                      v-tooltip.top="'Chi tiết Powerpoint'"
                      style="height: 15vh; border-radius: 5px 5px 0px 0px;    object-fit: cover"
                      id="logoLang"
                      v-bind:src="
                        slotProps.data.image
                          ? basedomainURL + slotProps.data.image
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                    />
                  </div>
                </template>
                <template #content>
                  <div class="pt-0 pb-3">
                    <Toolbar class="custoolbar pt-0">
                      <template #start>
                        <div>
                          <div class="justify-content-center flex text-title">
                            <div>
                              <i
                                class="pi pi-eye p-0 pr-1"
                                style="font-size: 13px"
                              ></i>
                            </div>
                            <div style="font-size: 13px">
                              {{ slotProps.data.views_count }}
                            </div>
                          </div>
                        </div>
                      </template>
                      <template #end>
                        <div>
                          <div
                            class="
                              col-12
                              p-0
                              justify-content-center
                              flex
                              text-title
                            "
                            style="font-size: 13px"
                          >
                            <i
                              class="pi pi-clock pr-1"
                              style="font-size: 13px; padding-top: 1px"
                            ></i>
                            {{
                              moment(
                                new Date(slotProps.data.created_date)
                              ).format("HH:mm DD/MM/YYYY")
                            }}
                          </div>
                        </div>
                      </template>
                    </Toolbar>
                  </div>
                  <div
                    class="
                      format-center
                      font-bold
                      text-lg
                      mx-2
                      text-3line text-title
                      mb-2
                    "
                    style="height: 6vh"
                  >
                    {{ slotProps.data.title }}
                  </div>

                  <div class="w-full p-3">
                    <div class="flex pt-0 grid">
                      <div class="col-12 p-0 flex">
                        <div class="col-2 p-0 format-center">
                         
                           <Avatar
                        v-bind:label="
                          slotProps.data.avatar
                            ? ''
                            : slotProps.data.created_name.substring(
                                slotProps.data.created_name.lastIndexOf(' ') + 1,
                                slotProps.data.created_name.lastIndexOf(' ') + 2
                              )
                        "
                        :image="basedomainURL + slotProps.data.avatar"
                        
                        :style="
                          slotProps.data.avatar
                            ? 'background-color: #2196f3'
                            : 'background:' +
                              bgColor[slotProps.data.created_name.length % 7]
                        "
                        shape="circle"
                        @error="
                          $event.target.src =
                            basedomainURL + '/Portals/Image/nouser1.png'
                        "  class="w-2rem h-2rem"
                      />
                        </div>
                        <div class="col-10 p-0 justify-content-center flex">
                          <div class="col-10 flex align-items-end">
                            <div class="text-md font-bold text-600 text-title">
                              {{ slotProps.data.created_name }}
                            </div>
                          </div>
                          <div class="col-2 p-0 format-center text-md flex">
                            <div><i class="pi pi-play text-title"></i></div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </template>
              </Card>
            </div>
          </template>

          <template #empty>
            <div
              class="align-items-center justify-content-center p-4 text-center"
            >
              <img src="../../assets/background/nodata.png" height="144" />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </template>
        </DataView>
      </div>
      <div class="surface-0 col-3 p-0">
        <div class="col-12 p-0" style="height: 40vh !important">
          <Panel class="p-panel-unset-bottom">
            <template #header>
              <div
                class="
                  w-full
                  align-content-center
                  flex
                  text-xl
                  surface-0
                  p-2
                  text-blue-900
                  font-bold
                "
                style="border-top: 3px solid #0078d4"
              >
                <i class="pi pi-eye mx-2 text-2xl" />Xem nhiều
              </div>
            </template>
            <div class="overflow-design p-0" style="height: 35vh !important">
              <DataView
                class="w-full h-full e-sm"
                @page="onPageNoti($event)"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink "
                responsiveLayout="scroll"
                :scrollable="true"
                scrollHeight="flex"
                layout="list"
                :lazy="true"
                :value="datalistViews"
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
                  <div class="grid p-0 py-2 w-full">
                    <div class="col-12 p-0">
                      <div class="col-12 p-0 flex">
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
                            <i class="pi pi-clock" style="font-size: 12px"></i>
                            {{
                              moment(
                                new Date(slotProps.data.created_date)
                              ).format("HH:mm DD/MM/YYYY")
                            }}
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
          </Panel>
        </div>
        <div class="col-12 p-0 " style="height: 40vh !important">
          <Panel class="p-panel-unset-bottom" >
            <template #header>
              <div
                class="
                  w-full
                  align-content-center
                  flex
                  text-xl
                  surface-0
                  p-2
                  text-blue-900
                  font-bold
                "
                style="border-top: 3px solid #0078d4"
              >
                <i class="pi pi-cloud-upload mx-2 text-2xl" />Tài liệu mới
              </div>
            </template>
            <div class="overflow-design p-0" style="height: 37vh !important">
              <DataView
                class="pr-0"
                @page="onPage($event)"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink "
                responsiveLayout="scroll"
                :scrollable="true"
                layout="list"
                :lazy="true"
                :value="datalistNews"
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
                      <div class="col-12 p-0 flex">
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
                            <i class="pi pi-clock" style="font-size: 12px"></i>
                            {{
                              moment(
                                new Date(slotProps.data.created_date)
                              ).format("HH:mm DD/MM/YYYY")
                            }}
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
          </Panel>
        </div>
      </div>
    </div>
    <!-- <div v-else class="relative surface-200 w-full h-full"    style="
                 z-index: 1000000;
                    " > 
                    <ProgressSpinner  class="absolute" style="    translate: transform(-50%, -50%);
                      top: 50%;
                      right: 50%;
                      object-fit: cover;width:50px;height:50px " strokeWidth="8" fill="var(--surface-ground)" animationDuration=".5s"/>
                  </div> -->
  </div>
</template>




<style scoped>
.text-3line {
  text-overflow: ellipsis;
  overflow: hidden !important;
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
.card-content:hover .text-title {
  color: #1c80cf !important;
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
  .p-paginator-bottom {
    border: 1px solid #fff;
  }
}
::v-deep(.p-dataview.p-dataview-unset) {
  .p-dataview-content {
    background-color: #eeeeee !important;
  }
}
::v-deep(.p-dropdown) {
  .p-dropdown-trigger {
    visibility: visible !important;
  }
}
::v-deep(.p-card) {
  .p-card-body {
    padding: 0px;
  }
  .p-card-title {
    padding-bottom: 0px;
  }
}
</style>
