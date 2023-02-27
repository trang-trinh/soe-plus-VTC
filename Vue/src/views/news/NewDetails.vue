<script setup>
import { ref, inject, onMounted, watch, onUpdated } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";
import { VuemojiPicker } from "vuemoji-picker";
import commentNews from "../../components/news/comment.vue";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const toast = useToast();
const router = inject("router");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const news = ref();
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const bugComment = ref();
const datalistViews = ref([]);
const isFirst = ref(true);
const emitter = inject("emitter");

//Nơi nhận dữ liệu

emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "loadComment":
   
      commentSize.value = obj.data;
      loadComment();
      break;
      default:
      break;
  }
});
const options = ref({
  IsNext: true,
  sort: "news_id DESC",
  search: "",
  pageno: 1,
  pagesize: 10,
  pageno1: 0,
  pagesize1: 15,
  loading: true,
  totalRecords: null,
  totalNotiRecords: null,
});
const optionsComment = ref({
  isShowInput: true,
  isUploadFile: false,
  isReply: false,
  isReaction: false,
});
const onViewsCount = (data) => {
  let bnt = {
    IntID: data.news_id,
    TextID: data.news_id + "",
    user_id: store.getters.user.user_id,
  };
  axios
    .put(baseURL + "/api/news_main/update_visitor", bnt, config)
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
const reloadComment = () => {
  loadComment();
  loadCommentCount(news.value.news_id);
};
const comment_Count = ref(0);
const loadCommentCount = (id) => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "news_comment_count",
            par: [{ par: "news_id", va: id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        comment_Count.value = data[0].totalRecords;
      }
    })
   
};
const onPageNoti = (event) => {
  options.value.pageno1 = event.page;
  loadDataNotify();
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
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalNotiRecords = data[0].totalRecords;
      } else options.value.totalNotiRecords = 0;
    })
    .catch((error) => {
      console.log(error);
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
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      if (data.length > 0) {
        // relateNew.value = [];
        data.forEach((element, i) => {
          element.is_order = i + 1;
          if (!element.created_date_show) element.created_date_show = null;
          element.created_date_show = moment(
            new Date(element.created_date),
          ).format("DD/MM/YYYY");
          if (!element.start_date_show) element.start_date_show = null;
          element.start_date_show = moment(new Date(element.start_date)).format(
            "DD/MM/YYYY hh:mm:ss",
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
      console.log(error);

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
const showDetailsKey = (value) => {
  let srcMs = removeVietnameseTones(value);
  if (router)
    router.push({
      path:
        "/news/direct/keywords/" +
        "tu-khoa-orient-" +
        srcMs.replace(/','|'.'/g, "").replace(/\s+/g, "-"),
    });
};
const comment = ref("");
const panelEmoij4 = ref();
const panelEmoij3 = ref();
const checkEditEmoij = ref(true);

const handleEmojiClick = (event) => {
  if (checkEditEmoij.value)
    if (comment.value) comment.value = comment.value + event.unicode;
    else comment.value = event.unicode;
  else if (commentEdit.value.des)
    commentEdit.value.des = commentEdit.value.des + event.unicode;
  else commentEdit.value.des = event.unicode;
};

const filterMonth = ref();
const idNewsLoaded = ref(
  window.location.href.substring(
    window.location.href.lastIndexOf("orient-") + 7,
  ),
);
const toggleFilterMonth = (event) => {
  filterMonth.value.toggle(event);
};
const onloadNewsViews = () => {
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
              { par: "is_hot", va: true },
              { par: "is_notify", va: options.value.is_notify },
              { par: "datefilter", va: 0 },
              { par: "pageno", va: options.value.pageno1 },
              { par: "pagesize", va: options.value.pagesize1 },
              { par: "start_date", va: options.value.start_date },
              { par: "end_date", va: options.value.end_date },
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
      if (data.length > 0) {
        datalistViews.value = data;
      }
      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
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
const listRelated = ref([]);
const onloadRelated = (value) => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "news_related_list",
            par: [{ par: "related_id", va: value }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      if (data.length > 0) {
        listRelated.value = data;
      }
    })
    .catch((error) => {
      

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const onloadNews = () => {
  news.value = null;
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "news_main_get",
            par: [{ par: "news_id", va: idNewsLoaded.value }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
       
      if (data.length > 0) {
        news.value = data[0];
        let arrI = [];
        if (news.value.url_file != "" && news.value.url_file != null) {
          news.value.url_file.split(",").forEach((item) => {
            if (item != "" && item != null) arrI.push(item);
          });
          news.value.url_file = arrI;
        }
        let arrII = [];
        if (news.value.key_words != "" && news.value.key_words != null) {
          news.value.key_words.split(",").forEach((item) => {
            if (item != "" && item != null) arrII.push(item);
          });
          news.value.key_words = arrII;
        }
        bugComment.value = {
          des: null,
          news_id: news.value.news_id,
          user_id: store.getters.user.user_id,
        };
        if (news.value.related_id != null && news.value.related_id != "")
          onloadRelated(news.value.related_id);
        loadCommentCount(news.value.news_id);
        onViewsCount(news.value);
        if (news.value.news_type == 0) {
          onloadNewsViews();
        } else {
          loadDataNotify();
        }
        if (news.value.is_comment) {
          loadComment();
        }
      } else news.value = store.getters.news;
    })
    .catch((error) => {
      console.log(error);

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const datalistsDiff = ref();
const onloadNewsDiff = () => {
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
              { par: "pagesize", va: 7 },
              { par: "start_date", va: options.value.start_date },
              { par: "end_date", va: options.value.end_date },
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
      if (data.length > 0) {
        // relateNew.value = [];
        data.forEach((element, i) => {
          element.is_order = i + 1;

          // relateNew.value.push({
          //   name: element.title,
          //   code: element.news_id,
          // });

          if (!element.created_date_show) element.created_date_show = null;
          element.created_date_show = moment(
            new Date(element.created_date),
          ).format("DD/MM/YYYY");
          if (!element.start_date_show) element.start_date_show = null;
          element.start_date_show = moment(new Date(element.start_date)).format(
            "DD/MM/YYYY hh:mm:ss",
          );
        });

        // Array.from(new Set(relateNew.value));
        datalistsDiff.value = data;
      } else {
        datalistsDiff.value = [];
      }

      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);

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

const showDetails = (data) => {
  let srcMs = removeVietnameseTones(data.title);
  store.commit("setnews", data);
  location.href =
    "/news/direct/" +
    srcMs.replace(/','|'.'/g, "").replace(/\s+/g, "-") +
    "-orient-" +
    data.news_id;
  // if (router) router.push({ path: "/news/details/"+srcMs.replace(/','|'.'/g,"").replace(/\s+/g,"-")+"-orient-"+data.news_id });
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
    " ",
  );
  return str;
}

const commentSize = ref(10);
const dataComments = ref([]);

const loadComment = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "news_comment_list",
            par: [
              { par: "news_id", va: news.value.news_id },
              { par: "pagesize", va: commentSize.value },
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
      if (data.length > 0) {
        dataComments.value = data;
      } else dataComments.value = [];
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

const commentEdit = ref({ comment_id: null, des: "" });

const prePage = () => {
  if (router)
    router.push({
      path: "/news/direct",
    });
};
const pageUp = () => {
  const html = document.getElementById("scrollTop");
  html.scrollTop = 0;
};
onMounted(() => {
   
  onloadNews();
  onloadNewsDiff();
  return {};
});
</script>

<template>
  <div
    class="overflow-y-auto overflow-x-hidden relative mt-3"
    id="scrollTop"
    v-if="news"
    style="height: calc(100vh - 50px)"
  >
    <div class="grid relative">
      <div class="col-12 flex pt-0 relative">
        <div class="surface-200 col-9 pr-0 pb-0" v-if="news.news_type == 0">
          <div class="col-12 flex p-0 surface-0">
            <div class="grid formgrid m-2">
              <div class="field col-12 md:col-12">
                <div class="font-bold text-4xl">{{ news.title }}</div>
              </div>
              <div class="field col-12 md:col-12">
                <img
                  v-if="news.is_hot"
                  style="width: 40px; height: 20px; margin-right: 12px"
                  src="/src/assets/image/new.jpg"
                  alt="new"
                />
                <span style="color: cornflowerblue; fon-size: 14px"
                  >{{ news.created_name }},</span
                >
                <span class="pl-1">
                  Ngày:
                  {{
                    moment(new Date(news.start_date)).format("DD/MM/YYYY HH:mm")
                  }}</span
                >
              </div>
              <div class="field col-12 md:col-12">
                <hr />
              </div>
              <div class="field col-12 md:col-12">
                <h3>{{ news.des }}</h3>
              </div>

              <div
                style="padding: 0px 24px"
                class="field col-12 md:col-12 ck-content"
              >
                <p
                  v-html="news.contents"
                  style="font-size: 16px; line-height: 1.5rem"
                ></p>
                <div class="flex align-items-center">
                  <div class="text-lg font-bold mb-2" v-if="news.key_words">
                    Từ khóa:
                  </div>
                  <div class="flex pl-2">
                    <div
                      class="mr-2"
                      v-for="(item, index) in news.key_words"
                      :key="index"
                    >
                      <Chip
                        @click="showDetailsKey(item)"
                        class="cursor-pointer"
                        :label="item"
                      />
                    </div>
                  </div>
                </div>
                <Toolbar class="w-full surface-0 outline-none border-none p-0">
                  <template #end>
                    <div class="flex">
                      <div
                        class="justify-content-center flex pr-2"
                        v-if="news.is_comment"
                      >
                        <div>
                          <i
                            class="pi pi-comment pr-1"
                            style="font-size: 16px"
                          ></i>
                        </div>
                        <div>
                          {{ news.comment_count ? news.comment_count : 0 }} bình
                          luận
                        </div>
                      </div>
                      <div
                        class="justify-content-center flex"
                        v-if="news.views_count > 0"
                      >
                        <div>
                          <i class="pi pi-eye pr-1" style="font-size: 16px"></i>
                        </div>
                        <div>
                          {{ news.views_count ? news.views_count : 0 }} người
                          xem
                        </div>
                      </div>
                    </div>
                  </template>
                </Toolbar>
                <Toolbar
                  class="w-full surface-0 outline-none border-none pb-0 pr-0 mr-0"
                >
                  <template #end>
                    <div class="flex">
                      <div
                        class="pr-3 text-lg text-blue-800 font-bold cursor-pointer"
                        @click="prePage()"
                      >
                        <i
                          class="pi pi-directions-alt pr-1"
                          style="font-size: 1rem"
                        ></i>
                        Về trang trước
                      </div>
                      <div class="cursor-pointer" @click="pageUp()">
                        <a class="no-underline text-lg text-blue-800 font-bold"
                          ><i
                            class="pi pi-arrow-circle-up pr-1"
                            style="font-size: 1rem"
                          ></i
                          >Lên đầu trang</a
                        >
                      </div>
                    </div>
                  </template>
                </Toolbar>
              </div>
              <div class="col-12">
                <hr />
              </div>
              <div class="col-12 p-0" v-if="news.is_comment">
                <commentNews
                  :options="optionsComment"
                  :refreshData="reloadComment"
                  :objectData="bugComment"
                  :comment_count="comment_Count"
                  :dataComments="dataComments"
                  :Controller="'news_comment'"
                />
              </div>
              <div
                class="col-12 p-0 flex m-2 text-2xl font-bold mb-1 mt-5 px-2"
                style="border-left: 3px solid #0078d4"
                v-if="listRelated.length > 0"
              >
                Tin liên quan
              </div>
              <div class="col-12 p-0 flex m-2">
                <DataView
                  class="w-full h-full"
                  responsiveLayout="scroll"
                  :scrollable="true"
                  layout="list"
                  :lazy="true"
                  :value="listRelated"
                  :loading="options.loading"
                  :paginator="false"
                  :rowHover="true"
                  :showGridlines="true"
                  currentPageReportTemplate=""
                >
                  <template #list="slotProps">
                    <div
                      class="grid p-0 w-full"
                      v-if="slotProps.data.news_id != news.news_id"
                    >
                      <div class="col-12 field p-3 pb-0">
                        <div class="col-12 field">
                          <div
                            class="font-bold text-xl text-justify cursor-pointer text-5line hyper-link"
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
                              <i
                                class="pi pi-clock"
                                style="font-size: 12px"
                              ></i>
                              {{
                                moment(
                                  new Date(slotProps.data.approved_date),
                                ).format("HH:mm DD/MM/YYYY")
                              }}
                            </div>
                            <div
                              class="col-12 p-0 text-justify line-height-3"
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
                      class="align-items-center justify-content-center p-4 text-center"
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
              <div
                class="col-12 p-0 flex m-2 text-2xl font-bold mb-1 mt-5 px-2"
                style="border-left: 3px solid #0078d4"
              >
                Tin tức khác
              </div>
              <div class="col-12 p-0 flex m-2">
                <DataView
                  class="w-full h-full"
                  responsiveLayout="scroll"
                  :scrollable="true"
                  layout="list"
                  :lazy="true"
                  :value="datalistsDiff"
                  :loading="options.loading"
                  :paginator="false"
                  :rowHover="true"
                  :showGridlines="true"
                  currentPageReportTemplate=""
                >
                  <template #list="slotProps">
                    <div
                      class="grid p-0 w-full"
                      v-if="slotProps.data.news_id != news.news_id"
                    >
                      <div class="col-12 field p-3 pb-0">
                        <div class="col-12 field">
                          <div
                            class="font-bold text-xl text-justify cursor-pointer text-5line hyper-link"
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
                              <i
                                class="pi pi-clock"
                                style="font-size: 12px"
                              ></i>
                              {{
                                moment(
                                  new Date(slotProps.data.approved_date),
                                ).format("HH:mm DD/MM/YYYY")
                              }}
                            </div>
                            <div
                              class="col-12 p-0 text-justify line-height-3"
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
                      class="align-items-center justify-content-center p-4 text-center"
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
          </div>
          <div class="col-12 p-0 surface-0"></div>
        </div>
        <div class="surface-200 col-9 pr-0 pb-0" v-else>
          <div class="col-12 p-0 surface-0 pt-2 pl-2">
            <div class="field col-12 md:col-12">
              <div class="font-bold text-4xl">{{ news.title }}</div>
            </div>
            <div class="field col-12 md:col-12">
              <span style="color: cornflowerblue; fon-size: 14px"
                >{{ news.created_name }},</span
              >
              <span class="pl-3"
                >Ngày:
                {{
                  moment(new Date(news.start_date)).format(
                    "DD/MM/YYYY HH:mm:ss",
                  )
                }}</span
              >
            </div>
            <div class="col-12 md:col-12 p-0">
              <hr />
            </div>
            <div class="field col-12 md:col-12">
              <h3>{{ news.des }}</h3>
            </div>
            <div class="field col-12 md:col-12" style="padding: 0px 24px"></div>

            <div style="padding: 0px 24px" class="col-12 md:col-12 ck-content">
              <p
                v-html="news.contents"
                style="font-size: 16px; line-height: 1.5rem"
              ></p>
              <div class="fcol-12 p-0 flex" v-if="news.url_file">
                <div class="col-11 p-0">
                  <span class="font-bold text-600 text-lg">
                    Danh sách File</span
                  >
                </div>
              </div>
              <div class="pb-2">
                <div v-for="(item, index) in news.url_file" :key="index">
                  <div class="flex align-items-center cursor-pointer my-3">
                    <a
                      :href="basedomainURL + item"
                      download
                      class="w-full no-underline flex align-items-center"
                    >
                      <img
                        :src="
                          basedomainURL +
                          '/Portals/Image/file/' +
                          (item
                            ? item.substring(item.indexOf('.') + 1)
                            : 'filess') +
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
              </div>
              <div class="flex align-items-center">
                <div
                  class="font-bold text-600 text-lg mb-2"
                  v-if="news.key_words"
                >
                  Từ khóa:
                </div>
                <div class="flex pl-2">
                  <div
                    class="mr-2"
                    v-for="(item, index) in news.key_words"
                    :key="index"
                  >
                    <Chip class="cursor-pointer" :label="item" />
                  </div>
                </div>
              </div>
              <Toolbar class="w-full surface-0 outline-none border-none p-0">
                <template #end>
                  <div class="justify-content-center flex">
                    <div>
                      <i class="pi pi-eye pr-1" style="font-size: 16px"></i>
                    </div>
                    <div>
                      {{ news.views_count ? news.views_count : 0 }} người xem
                    </div>
                  </div>
                </template>
              </Toolbar>
              <Toolbar
                class="w-full surface-0 outline-none border-none pb-0 pr-0 mr-0"
              >
                <template #end>
                  <div class="flex">
                    <div
                      class="pr-3 text-lg text-blue-800 font-bold cursor-pointer"
                      @click="prePage()"
                    >
                      <i
                        class="pi pi-directions-alt pr-1"
                        style="font-size: 1rem"
                      ></i>
                      Về trang trước
                    </div>
                    <div class="cursor-pointer" @click="pageUp()">
                      <a class="no-underline text-lg text-blue-800 font-bold"
                        ><i
                          class="pi pi-arrow-circle-up pr-1"
                          style="font-size: 1rem"
                        ></i
                        >Lên đầu trang</a
                      >
                    </div>
                  </div>
                </template>
              </Toolbar>
            </div>
          </div>
          <div class="col-12 surface-0">
            <hr />

            <div class="col-12 p-0" v-if="news.is_comment">
              <commentNews
                :options="optionsComment"
                :refreshData="reloadComment"
                :objectData="bugComment"
                :comment_count="comment_Count"
                :dataComments="dataComments"
                :Controller="'news_comment'"
              />
            </div>

            <div
              class="col-12 p-0 flex m-2 text-2xl font-bold mb-1 mt-5 px-2"
              style="border-left: 3px solid #0078d4"
              v-if="listRelated.length > 0"
            >
              Thông báo liên quan
            </div>
            <div class="col-12 p-0 flex m-2">
              <DataView
                class="w-full h-full"
                responsiveLayout="scroll"
                :scrollable="true"
                layout="list"
                :lazy="true"
                :value="listRelated"
                :loading="options.loading"
                :paginator="false"
                :rowHover="true"
                :showGridlines="true"
                currentPageReportTemplate=""
              >
                <template #list="slotProps">
                  <div
                    class="grid p-0 w-full"
                    v-if="slotProps.data.news_id != news.news_id"
                  >
                    <div class="col-12 field p-3 pb-0">
                      <div class="col-12 field">
                        <div
                          class="font-bold text-xl text-justify cursor-pointer text-5line hyper-link"
                          @click="showDetails(slotProps.data)"
                        >
                          {{ slotProps.data.title }}
                        </div>
                      </div>
                      <div class="col-12 flex">
                        <div class="col-12 p-0" style="font-size: 14px">
                          <div
                            class="col-12 p-0 pb-2 text-500"
                            style="font-size: 12px"
                          >
                            <i class="pi pi-clock" style="font-size: 12px"></i>
                            {{
                              moment(
                                new Date(slotProps.data.approved_date),
                              ).format("HH:mm DD/MM/YYYY")
                            }}
                          </div>
                          <div
                            class="col-12 p-0 text-justify line-height-3"
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
                    class="align-items-center justify-content-center p-4 text-center"
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
        </div>
        <div class="surface-0 col-3 sticky top-0">
          <div class="col-12 p-0 field sticky top-0" v-if="news.news_type == 0">
            <div>
              <div class="col-12 p-0 field">
                <Panel class="p-panel-unset-bottom">
                  <template #header>
                    <div class="pt-5 pb-3 surface-0 w-full">
                      <div
                        class="surface-0 flex text-2xl font-bold px-2 text-align-center w-full m-0"
                        style="border-left: 3px solid #0078d4"
                      >
                        <i class="pi pi-image mx-2 text-2xl" /> Tin nổi bật
                      </div>
                      <div></div>
                    </div>
                  </template>
                  <div
                    class="overflow-design p-0"
                    style="height: calc(100vh - 150px)"
                  >
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
                      :paginator="options.totalRecords > options.pagesize"
                      :rows="options.pagesize"
                      :totalRecords="options.totalRecords"
                      :rowHover="true"
                      :showGridlines="true"
                      :pageLinkSize="options.pagesize"
                      currentPageReportTemplate=""
                    >
                      <template #list="slotProps">
                        <div
                          class="grid p-0 py-2 w-full"
                          v-if="slotProps.data.news_id != news.news_id"
                        >
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
                                      : basedomainURL +
                                        '/Portals/Image/noimg.jpg'
                                  "
                                />
                              </div>
                              <div
                                class="col-9 p-0 cursor-pointer"
                                @click="showDetails(slotProps.data)"
                              >
                                <div class="col-12 p-0 pb-1 pr-2">
                                  <div
                                    class="font-bold text-color-secondary text-justify text-3line hyper-link"
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
                                      new Date(slotProps.data.approved_date),
                                    ).format("HH:mm DD/MM/YYYY")
                                  }}
                                </div>
                              </div>
                            </div>
                            <div class="col-12 pt-0">
                              <div
                                class="text-justify text-3line line-height-3 text-600"
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
                          class="align-items-center justify-content-center p-4 text-center"
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
            </div>
          </div>
          <div class="col-12 p-0 field sticky top-0" v-else>
            <div class="col-12 p-0">
              <Panel class="p-panel-unset-bottom top-0 absolute">
                <template #header>
                  <div
                    class="w-full format-center text-2xl bg-orange-500 p-2 text-0"
                    style="border-radius: 3px 3px 0px 0px"
                  >
                    <i class="pi pi-bell mx-2 text-2xl" /> Thông báo khác
                  </div>
                </template>
                <div
                  class="overflow-design p-0"
                  style="height: calc(100vh - 150px)"
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
                      <div
                        class="w-full"
                        v-if="slotProps.data.news_id != news.news_id"
                      >
                        <Panel
                          class="w-full h-full p-panel-unset"
                          :toggleable="false"
                        >
                          <template #header>
                            <div
                              class="h-full align-items-center justify-content-center"
                            >
                              <div class="text-justify">
                                <span
                                  class="font-bold text-color-secondary text-justify text-4line cursor-pointer hyper-link"
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
                                      new Date(slotProps.data.approved_date),
                                    ).format("HH:mm DD/MM/YYYY")
                                  }}</span
                                >
                                <span v-if="slotProps.data.url_file != null">
                                  <i
                                    class="pi pi-paperclip px-2 text-600 cursor-pointer"
                                    aria:haspopup="true"
                                    aria-controls="overlay_panelMonth"
                                    @click="toggleFilterMonth"
                                  ></i>

                                  <OverlayPanel
                                    ref="filterMonth"
                                    appendTo="body"
                                    class="p-0 m-0"
                                    :showCloseIcon="false"
                                    id="overlay_panelMonth"
                                    style="width: 300px"
                                  >
                                    <div
                                      v-for="(item, index) in slotProps.data
                                        .url_file"
                                      :key="index"
                                    >
                                      <div
                                        class="flex align-items-center cursor-pointer"
                                      >
                                        <a
                                          :href="basedomainURL + item"
                                          download
                                          class="w-full no-underline flex align-items-center"
                                        >
                                          <img
                                            :src="
                                              basedomainURL +
                                              '/Portals/Image/file/' +
                                              (item
                                                ? item.substring(
                                                    item.indexOf('.') + 1,
                                                  )
                                                : 'filess') +
                                              '.png'
                                            "
                                            @error="
                                              $event.target.src =
                                                basedomainURL +
                                                '/Portals/Image/file/filess.png'
                                            "
                                            style="
                                              width: 32px;
                                              height: 32px;
                                              object-fit: contain;
                                            "
                                          />
                                          <div class="pl-2">
                                            {{ item.substring(14) }}
                                          </div>
                                        </a>
                                      </div>
                                    </div>
                                  </OverlayPanel>
                                </span>
                              </div>
                            </div>
                          </template>
                        </Panel>
                      </div>
                    </template>
                    <template #empty>
                      <div
                        class="align-items-center justify-content-center p-4 text-center"
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
    </div>
    <OverlayPanel
      class="p-0"
      ref="panelEmoij4"
      appendTo="body"
      :showCloseIcon="false"
      id="overlay_panelEmoij4"
    >
      <VuemojiPicker @emojiClick="handleEmojiClick" />
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
.d-del-comment {
  visibility: hidden;
}
.d-comment:hover .d-del-comment {
  visibility: visible;
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
