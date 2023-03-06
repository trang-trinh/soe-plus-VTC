<script setup>
import { ref, inject, onMounted, watch, onUpdated } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";
import comment from "../../components/news/comment.vue";
import { encr   } from "../../util/function.js";
import 'animate.css';
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
//Nơi nhận dữ liệu
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "loadComment":
      commentSize.value=obj.data;
     loadComment();
      break;default:
      break;
  }
});
//Khai báo
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const router = inject("router");
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const toast = useToast();
const options = ref({
  IsNext: true,
  sort: "shows_id DESC",
  search: "",
  pageno: 0,
  pagesize: 10,
status:2,
  loading: true,
  totalRecords: null,
  totalNotiRecords: null,
  typefilter: 0,
  typeviews: 1,
  datefilter: 0,
});

const datalists = ref([]);
const onDownloadS=()=>{
  axios
    .post(baseURL + "/api/shows_main/DownloadHtml5/?id=" +
            shows.value.shows_id + "&dvid=" +
            shows.value.organization_id, config)
    .then((response) => {
        if (response.data.path != null) {
            let pathReplace = response.data.path.replace(/\\+/g, '/').replace(/\/+/g, '/').replace(/^\//g, '');
            var listPath = pathReplace.split('/');
            var pathFile = "";
            listPath.forEach(item => {
              if (item.trim() != "")
              {
                  pathFile += "/" + item;
              }
            });
            window.open(baseURL + pathFile);
          }
      
    })
    
  
}
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
        options.value.totalRecords = data[0].totalRecords;
      } else options.value.totalRecords = 0;
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCount",
        controller: "SQLView.vue",
        logcontent: error.message,
        loai: 2,
      });
    });
};
const visibleLeft=ref(false);
const loadData = (rf) => {
  if (rf) {
    options.value.loading = true;
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
          { par: "datefilter", va: 1 },
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
    })
    
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
    
};
const shows = ref();

const onViewsCount = (data) => {
  let bnt = {
    IntID: data.shows_id,
    TextID: data.shows_id + "",
    user_id: store.getters.user.user_id,
  };
  axios
    .put(baseURL + "/api/shows_main/update_visitor", bnt, config)
    .catch((error) => {
    
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};

const showDetails = (data) => {
  let srcMs = removeVietnameseTones(data.title);
  store.commit("setnews", data);
  location.href =
    "/news/shows/" +
    srcMs.replace(/','|'.'/g, "").replace(/\s+/g, "-") +
    "-orient-" +
    data.shows_id;
  // if (router) router.push({ path: "/news/details/"+srcMs.replace(/','|'.'/g,"").replace(/\s+/g,"-")+"-orient-"+data.news_id });
};
const idNewsLoaded = ref(
  window.location.href.substring(
    window.location.href.lastIndexOf("orient-") + 7
  )
);
const dataComments=ref();
const commentSize=ref(10);
const bugComment = ref();
const optionsComment=ref({
  isShowInput:true,
  isUploadFile:false, isReply:false,
})
const loadComment = () => {
   axios
    .post(
          baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "shows_comment_list",
        par: [
          { par: "shows_id", va: shows.value.shows_id },
          { par: "pagesize", va: commentSize.value },
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
        data.forEach(element => {
          if(element.is_show_more)
          element.is_show_more=false;
        });
        dataComments.value = data;
      }
    })
    .catch((error) => {
    
    
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const reloadComment=()=>{
  loadComment();
  loadCommentCount(shows.value.shows_id);
};
const comment_Count=ref(0);
const loadCommentCount=(id)=>{
    axios
    .post(
          baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "shows_comment_count",
        par: [
          { par: "shows_id", va: id },
         
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
        comment_Count.value = data[0].totalRecords;
      }
    })
 
}
const getData = () => {
  axios
    .post(
          baseURL + "/api/device_card/getData",
        {
          str: encr(
            JSON.stringify({
        proc: "shows_main_get",
        par: [{ par: "shows_id", va: idNewsLoaded.value }],
          }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        shows.value = data[0];
        // let arrI = [];
        // if (shows.value.url_file != "" && shows.value.url_file != null) {
        //   shows.value.url_file.split(",").forEach((item) => {
        //     if (item != "" && item != null) arrI.push(item);
        //   });
        //   shows.value.url_file = arrI;
        // }

        bugComment.value={
          des:null,shows_id:  shows.value.shows_id,user_id:store.getters.user.user_id
        }
        loadComment();
        let arrII = [];
        if (shows.value.key_words != "" && shows.value.key_words != null) {
          shows.value.key_words.split(",").forEach((item) => {
            if (item != "" && item != null) arrII.push(item);
          });
          shows.value.key_words = arrII;
        }
        if (!shows.value.pathname) {
          let srcMs = removeVietnameseTones(shows.value.title);
          shows.value.pathname =
            srcMs.replace(/','|'.'/g, "").replace(/\s+/g, "-") +
            "-orient-" +
            shows.value.shows_id;
        }
        loadCommentCount(shows.value.shows_id);
        onViewsCount(shows.value);

        // if (shows.value.is_comment) {
        //   loadComment();
        // }
      }
    })
    .catch((error) => {
     
      toast.error("Tải dữ liệu không thành công!");

 
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
const prePage = () => {
  if (router)
    router.push({
      path: "/news/shows",
    });
};
const showHtmlPoint=()=>{
  window.open(baseURL +'/HtmlPoint/Index/?title=' +
            shows.value.pathname +
            '&url=' +
            shows.value.path);
  
}
const isShowComment=ref(false);
const showComment=()=>{
  if(isShowComment.value)
  isShowComment.value=false;
  else
  isShowComment.value=true;
}
onMounted(() => {
  loadData(true);
  loadDataViews();
 getData();
});
</script>

<template>
  <div class="surface-0 mt-2" v-if="shows">
    <div class="flex w-full p-3 py-0">
      <div class="w-full grid">
        <div class="col-12 flex p-2">
         
          <div class="col-8 p-0 flex">
             <div class="format-center p-0 ">
            <i class="pi pi-images pr-2" style="font-size: 2rem"></i>
          </div>
            <div>
              <div class="w-full font-bold text-2xl pb-1">
              {{ shows.title }}
            </div>
            <div class="text-md text-600 flex">
              Ngày tạo:
              {{
                moment(new Date(shows.created_date)).format("HH:mm DD/MM/YYYY")
              }}
              | {{ shows.created_name }}<span  v-if="shows.is_comment" class="px-1" > |</span>
              <div  v-if="shows.is_comment"  class="justify-content-center flex pr-2  style-comment">
                <div> 
                  <i class="pi pi-comment px-1" style="font-size: 16px"></i>
                </div>
                <div class="cursor-pointer " @click="showComment">
                Bình luận ({{
                    comment_Count
                  }})
                </div>
              </div>
            </div>
            </div>
          </div>

          <div class=" col-4 p-0">
            <div class="format-center flex" style="justify-content:right !important">
               <Button
                label="Quay lại"
                icon="pi pi-arrow-left"
                class="p-button-outlined mt-3 mr-2"
                @click="prePage"
              />
                 <!-- <Button
                label="Trình chiếu"
                icon="pi pi-directions"
                class="p-button-outlined mt-3 mr-2"
              
              /> -->
              <!-- @click="showHtmlPoint" -->
              <div v-if="shows.file_folder==null">
              <a :href="basedomainURL+shows.path" download class="no-underline">
                 <Button
                label="Tải xuống"
                icon="pi pi-download"
                class="p-button-outlined mt-3 mr-2"
                
              />
              </a></div>
              <div v-else>

                <Button
                label="Tải xuống"
                icon="pi pi-download"
                class="p-button-outlined mt-3 mr-2"
                @click="onDownloadS"
                />
              </div>
              
              
            </div>
          </div>
        </div>
      </div>
    </div>
    <div
      class="grid p-3 pt-0 pr-0 m-0 flex "
  
    >
      <div class="col-9 surface-0 p-0 relative overflow-y-auto" style="height:calc(100vh - 100px)">
      
        <iframe allowfullscreen
          :src="
            basedomainURL +
            '/Viewer/?title=' +
            shows.pathname +
            '&url=' +
            shows.path
          "
          :height="isShowComment==true?'80%':'100%'"
          width="100%"
          title="Iframe Example"
          v-if="!shows.file_folder"
        >
        </iframe>
        <iframe allowfullscreen
        v-else
        :height="isShowComment==true?'80%':'100%'"
          width="100%"
          title="Iframe Example"
          :src="
            basedomainURL+ shows.path"
          >


      </iframe>
        <div  v-if="isShowComment" class=" animate__animated animate__fadeInUp pt-2 relative mb-5"  >
          <comment :options="optionsComment"
           :refreshData="reloadComment"
            :objectData="bugComment"
             :comment_count="comment_Count" 
             :dataComments="dataComments" 
             :Controller="'shows_comment'"/>
        </div>

      </div>
      <div class="surface-0 col-3 p-0"  >
        <div class="col-12 p-0 field" style="height: 40vh !important">
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
            <div class="overflow-design p-0"  style="height: 40vh !important">
              <DataView
                class="w-full h-full e-sm"
                @page="onPageNoti($event)"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink "
                responsiveLayout="scroll"
                :scrollable="true"
                scrollHeight="flex"
                layout="list"
                :lazy="true"
                :value="datalists"
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
                  <div class="grid p-0 py-2 w-full" v-if="shows.shows_id!=slotProps.data.shows_id">
                    <div class="col-12 p-0">
                      <div class="col-12 p-0  flex">
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
        <div class="col-12 p-0 field" >
          <Panel class="p-panel-unset-bottom" style="height:40vh !important">
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
            <div class="overflow-design p-0"  style="height: 40vh !important">
              <DataView
                class="pr-0"
            
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink "
                responsiveLayout="scroll"
                :scrollable="true"
                layout="list"
                :lazy="true"
                :value="datalists"
                :loading="options.loading"
                :paginator="false"
                :rows="options.pagesize1"
                :totalRecords="options.totalRecords"
                :rowHover="true"
                :showGridlines="true"
                :pageLinkSize="options.pagesize1"
                currentPageReportTemplate=""
              >
                <template #list="slotProps">
                  <div class="grid p-0 py-2 w-full" v-if="shows.shows_id!=slotProps.data.shows_id">
                    <div class="col-12 p-0">
                      <div class="col-12 p-0  flex">
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
  </div>
</template>

<style  scoped>
.style-comment:hover{
  color: #0078D4 !important;
}
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
.overflow-design {
  overflow-y: scroll;
}
.overflow-design::-webkit-scrollbar-thumb {
  background-color: white !important;
}
.overflow-design:hover::-webkit-scrollbar-thumb {
  background-color: #e0e0e0 !important ;
}
.hyper-link {
  color: black;
}
.hyper-link:hover {
  color: #1c80cf !important;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-panel) {
  .p-panel-header {
    padding: 0;
    border: unset;
  }
  .p-panel-content {
    padding-right: 0px !important;
    border-bottom: unset !important;
    padding-top:0px !important;
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.p-panel.p-panel-unset) {
  .p-panel-header {
    padding: 16px 8px;
    background-color: #fff !important;
  }
  .p-panel-content {
    padding: 0;
  }
}
::v-deep(.p-grid) {
  div {
    border-bottom: unset !important;
    border-width: 1px 0px 0px 0px !important;
  }
}
</style>
