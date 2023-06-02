<script setup>
import { ref, inject, onMounted, watch, onUpdated } from "vue";
import { useToast } from "vue-toastification";
import videoview from "../../components/videos/videoview.vue"
import { useRouter, useRoute } from "vue-router";
import moment from "moment";
import { VuemojiPicker } from "vuemoji-picker";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const toast = useToast();
const route = useRoute();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const video = ref();
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const datalistViews = ref([]);
const listVideosRelate = ref([]);
const countComment = ref(0);
var old_video;
const countRelate = ref(0);
const dataComments = ref([]);
const commentSize = ref(10);
const relateSize = ref(15);
const isFirst = ref(true);
const videoId = ref("J-A8MkwjbVM");
const options = ref({
  IsNext: true,
  sort: "video_id DESC",
  search: "",
  pageno: 1,
  pagesize: 10,
  pageno1: 0,
  pagesize1: 10,
  loading: true,
  totalRecords: null,
  totalNotiRecords: null,
  datefilter: 1,
});
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
// Bình luận
// display , hidden icon edit, delete
const ShowMore = (item) => {
  dataComments.value.find(
    (x) => x.comment_id == item.comment_id
  ).is_show_more = true;
};
const HidenMore = (item) => {
  dataComments.value.find(
    (x) => x.comment_id == item.comment_id
  ).is_show_more = false;
};
const deleteComment = (data) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá bình luận này không!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Có",
      cancelButtonText: "Không",
    })
    .then((result) => {
      if (result.isConfirmed) {
        swal.fire({
          width: 110,
          didOpen: () => {
            swal.showLoading();
          },
        });
        axios
          .delete(baseURL + "/api/video_comment/delete_comment", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: data.comment_id != null ? [data.comment_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá bình luận thành công!");
              loadComment();
            } else {
              swal.fire({
                title: "Error!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
            if (error.status === 401) {
              swal.fire({
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const menuButMoresAloha = ref();
const onMoreCmt = () => {
  commentSize.value += 10;
  loadComment();
};
const onMoreRelate = () => {
  relateSize.value += 10;
  loadVideorelate(video.value.video_id, old_video);
};
const commentEdit = ref({ comment_id: null, des: "" });
const editComment = (value_coppy) => {
  let value = JSON.parse(JSON.stringify(value_coppy));
  value.des = value.des.replaceAll("<br/>", "\n");
  commentEdit.value = value;
  // document.getElementById('stextcmt').focus();
};
const cancelEditComment = () => {
  commentEdit.value = { comment_id: null, des: "" };
};
const saveEditComment = (item) => {
  commentEdit.value.des = commentEdit.value.des.replaceAll("\n", "<br/>");
  axios
    .put(
      baseURL + "/api/video_comment/update_comment",
      commentEdit.value,
      config
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Sửa bình luận thành công!");
        loadComment();
        commentEdit.value = { comment_id: null, des: "" };
      } else {
        console.log("LỖI A:", response);
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
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
const comment = ref("");
const isShowEmoji = ref(false);
const loadComment = () => {
  axios
    .post(
        baseURL + "/api/video_main/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "video_comment_list",
        par: [
          { par: "video_id", va: video.value.video_id },
          { par: "pagesize", va: commentSize.value },
        ],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        dataComments.value = data;
      }
      countComment.value = JSON.parse(response.data.data)[1][0].c;
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const addComment = () => {
  if (comment.value == "" || comment.value == null) return;
  else {
    let bugComment = {
      video_id: video.value.video_id,
      des: comment.value,
      user_id: store.getters.user.user_id,
    };

    bugComment.des = bugComment.des.replaceAll("\n", "<br/>");

    comment.value = "";
    axios
      .post(baseURL + "/api/video_comment/add_comment", bugComment, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm bình luận thành công!");
          loadComment();
        } else {
          console.log("LỖI A:", response);
          swal.fire({
            title: "Error!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  }
};
const panelEmoij4 = ref();
const panelEmoij3 = ref();
const checkEditEmoij = ref(true);
const showEmoji = (event, check) => {
  if (check) checkEditEmoij.value = true;
  else checkEditEmoij.value = false;
  panelEmoij4.value.toggle(event);
};
const handleEmojiClick = (event) => {
  if (checkEditEmoij.value)
    if (comment.value) comment.value = comment.value + event.unicode;
    else comment.value = event.unicode;
  else if (commentEdit.value.des)
    commentEdit.value.des = commentEdit.value.des + event.unicode;
  else commentEdit.value.des = event.unicode;
};
//listen event end video
const onEnded = () => {
  if (video.value.show_relate) {
    showDetails(listVideosRelate.value[0]);
  }
};
const onEnd = () => {
  if (video.value.show_relate) {
    showDetails(listVideosRelate.value[0]);
  }
};
const filterMonth = ref();
const idVideoLoaded = ref(
  window.location.href.substring(
    window.location.href.lastIndexOf("orient-") + 7
  )
);
const toggleFilterMonth = (event) => {
  filterMonth.value.toggle(event);
};
const loadVideorelate = (id, old_id) => {
  axios
    .post(
        baseURL + "/api/video_main/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "video_relate_list",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "key_words", va: options.value.key_words },
          { par: "search", va: options.value.search },
          { par: "status", va: null },
          { par: "video_id", va: id },
          { par: "old_video", va: old_id },
          { par: "relateSize", va: relateSize.value },
        ],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        data.forEach((item) => {
          if (!item.is_file_upload)
            item.thumbnail = get_youtube_thumbnail(item.link, "medium");
        });
        listVideosRelate.value = data;
      }
      countRelate.value = JSON.parse(response.data.data)[1][0].c;
      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
//get thumbnail from ytb
function get_youtube_thumbnail(url, quality) {
  if (url) {
    var video_id, thumbnail, result;
    if ((result = url.match(/youtube\.com.*(\?v=|\/embed\/)(.{11})/))) {
      video_id = result.pop();
    } else if ((result = url.match(/youtu.be\/(.{11})/))) {
      video_id = result.pop();
    }

    if (video_id) {
      if (typeof quality == "undefined") {
        quality = "high";
      }

      var quality_key = "maxresdefault"; // Max quality
      if (quality == "low") {
        quality_key = "sddefault";
      } else if (quality == "medium") {
        quality_key = "mqdefault";
      } else if (quality == "high") {
        quality_key = "hqdefault";
      }

      var thumbnail =
        "http://img.youtube.com/vi/" + video_id + "/" + quality_key + ".jpg";
      return thumbnail;
    }
  }
  return false;
}
const onloadVideo = () => {
  axios
    .post(
        baseURL + "/api/video_main/GetDataProc",
        {
          str: encr(JSON.stringify({
            proc: "video_main_get",
        par: [{ par: "video_id", va: idVideoLoaded.value }],
          }), SecretKey, cryoptojs
          ).toString()
        },
        config
      )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        old_video = parseInt(localStorage.getItem("old_video")) || null;
        video.value = data[0];
        if (video.value.key_words)
          video.value.key_words = video.value.key_words.replaceAll(",", ", ");
        video.value.show_relate = true;
        loadVideorelate(data[0].video_id, old_video);
        updateVideoViews(data[0]);
        if (video.value.is_comment) {
          loadComment();
        }
      } else video.value = store.getters.video;
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const updateVideoViews = (data) => {
  axios.put(baseURL + "/api/video_comment/update_views", data, config);
};
const datalistsDiff = ref();

const showDetails = (data) => {
  localStorage.setItem("old_video", video.value.video_id);
  localStorage.setItem("video", data.video_id);
  let srcMs = removeVietnameseTones(data.title);
  store.commit("setvideo", data);
  location.href =
    "/news/videosview/" +
    srcMs.replace(/','|'.'/g, "").replace(/\s+/g, "-") +
    "-orient-" +
    data.video_id;
};
const goKeywords = (item)=>{
  location.href =
    "/news/videossearch/" +
    "keywords-" +
    removeVietnameseTones(item).replace(/','|'.'/g, "").replace(/\s+/g, "-");
}
const pageUp = () => {
  document.getElementById("scrollTop").animate({ scrollTop: 0 }, 2000);
};
const goBack = () => {
  history.back();
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

onMounted(() => {
  onloadVideo();
  let id = route.params.name;
  //   onloadVideoDiff();
  return {};
});
</script>

<template>
  <div class="bg-gray">
    <div
      id="scrollTop"
      class="px-2 pt-2 bg-gray col-12 flex"
      style="max-height: calc(100vh - 50px); overflow-y: auto"
      v-if="video"
    >
      <div style="flex: 5">
        <div class="col-12">
          <Button
            label="Quay lại"
            icon="pi pi-arrow-left"
            class="p-button-outlined"
            @click="goBack()"
          />
        </div>
        <div class="col-12">
          <!-- <video
            v-if="video.is_file_upload"
            style="width: 100%; height: 60vh"
            controls
            :src="basedomainURL + video.path"
            @ended="onEnd()"
          ></video> -->
        <video  v-if="video.is_file_upload" id="mainPlayer" style="width: 100%; height: 60vh"
          autoplay="autoplay" controls="controls" onloadeddata="onLoad()">
          <source :src="basedomainURL+'/api/Media/Play?f='+video.path.toString()" />
            </video>
          <YoutubeVue3
            v-if="!video.is_file_upload"
            ref="youtube"
            :videoid="video.id_youtube"
            :autoplay="false"
            controls="1"
            width="100%"
            height="100%"
            :style="'width: 100%; height: 34vw'"
            @ended="onEnded"
            @paused="onPaused"
            @played="onPlayed"
          />
        </div>
        <div class="col-12 font-bold text-2xl">
          {{ video.title }}
        </div>
        <div class="col-12 flex">
          <div class="col-6 p-0">
            <span
              ><i class="pi pi-eye pr-1" style="font-size: 13px"></i
              >{{ video.is_visitor || 0 }} người xem</span
            >
          </div>
          <div class="col-6 p-0 flex justify-content-end">
            <span
              ><i class="pi pi-clock pr-1" style="font-size: 13px"></i>Ngày tải:
              {{
                moment(new Date(video.modified_date)).format("HH:mm DD/MM/YYYY")
              }}</span
            >
          </div>
        </div>
        <div class="col-12 flex" v-if="video.key_words != null">
          <span class="font-bold py-1">Từ khóa:&nbsp;</span>
          <span class="font-italic" v-if=" video.key_words">
          <Chip @click="goKeywords(item)" v-for="(item, index) in video.key_words.split(',')" :key ="index" class="p-ripple cursor-pointer py-1" v-ripple>{{item}}&nbsp;</Chip>
          </span>
        </div>
        <div class="col-12 md:col-12">
          <hr />
        </div>
        <!-- Bình luận -->
        <div v-if="video.is_comment">
          <div class="col-12 flex pt-0 pb-3">
            <span style="color: #007ad4; font-weight: bold; font-size: 16px"
              ><i class="pi pi-comments pr-1" style="font-size: 16px"></i>Bình
              luận ({{ countComment }})</span
            >
          </div>
          <div class="col-12 pr-3 flex">
            <div class="flex-1 p-0 format-center" style="align-items: baseline">
              <Avatar
                v-bind:label="
                  store.getters.user.avatar
                    ? ''
                    : store.getters.user.full_name
                        .split(' ')
                        .at(-1)
                        .substring(0, 1)
                "
                v-bind:image="basedomainURL + store.getters.user.avatar"
                class="w-3rem"
                size="large"
                shape="circle"
                style="color: #ffffff"
                :style="
                  store.getters.user.avatar
                    ? 'background-color: #2196f3'
                    : 'background:' +
                      bgColor[store.getters.user.full_name.length % 7]
                "
                @error="
                  $event.target.src =
                    basedomainURL + '/Portals/Image/nouser.png'
                "
              />
            </div>
            <div style="flex: 18" class="flex">
              <div
                class="col-10 p-0 ml-2 border-1 border-round-xs border-600 flex"
                style="border-radius: 5px"
              >
                <Textarea
                  style="border-radius: 5px; padding-top: 10px"
                  class="border-0 col-11 pb-0"
                  placeholder="Viết bình luận..."
                  :autoResize="true"
                  rows="1"
                  v-model="comment"
                />
                <div class="col-1 p-0 relative">
                  <div class="bottom-0 flex format-center">
                    <Button
                      class="
                        p-1 p-button-rounded p-button-text p-button-plain
                        m-1
                      "
                      @click="showEmoji($event, true)"
                    >
                      <img
                        alt="logo"
                        src="/src/assets/image/smile.png"
                        width="20"
                        height="20"
                      />
                    </Button>

                    <!-- Kiểm tra xem file là ảnh hoặc File rồi y -->
                  </div>
                </div>
              </div>
              <div class="w-2 pl-3 format-center col-2">
                <Button
                  label="Gửi"
                  @click="addComment()"
                  class="w-full ml-3"
                  style="
                    border-radius: 6px;
                    bacground-color: #386076 !important;
                    height: 30px;
                  "
                />
              </div>
            </div>
          </div>
          <div class="col-12 p-0 mt-5"></div>
          <div
            class="col-12 pt-2 pr-3 flex d-comment"
            v-for="(item, index) in dataComments"
            :key="index"
            @mouseover="ShowMore(item)"
            @mouseleave="HidenMore(item)"
          >
            <div class="flex-1 p-0 format-center" style="align-items: baseline">
              <Avatar
                v-bind:label="
                  item.avatar
                    ? ''
                    : item.full_name.split(' ').at(-1).substring(0, 1)
                "
                :image="basedomainURL + item.avatar"
                class="w-3rem"
                size="large"
                shape="circle"
                style="color: #ffffff"
                :style="
                  item.avatar
                    ? 'background-color: #2196f3'
                    : 'background:' + bgColor[item.full_name.length % 7]
                "
                @error="
                  $event.target.src =
                    basedomainURL + '/Portals/Image/nouser1.png'
                "
              />
            </div>
            <div class="p-0 box-comment">
              <div class="col-11 flex align-items-center pt-0">
                <div class="font-bold">
                  {{ item.full_name }}
                </div>
                <div class="text-sm px-2">
                  ({{
                    moment(new Date(item.created_date)).format(
                      "HH:mm DD/MM/YYYY"
                    )
                  }})
                </div>

                <div
                  class="d-del-comment"
                  v-if="
                    (item.created_by == store.getters.user.user_id ||
                      store.getters.user.is_super ||
                      (!store.getters.user.is_super &&
                        store.getters.user.is_admin &&
                        !item.is_super)) &&
                    item.is_show_more
                  "
                >
                  <Button
                    icon="pi pi-pencil"
                    style="color: #475057; padding: 0px !important"
                    v-tooltip.top="'Sửa bình luận'"
                    class="p-button-outlined ml-2 border-none"
                    @click="editComment(item)"
                  />
                  <Button
                    icon="pi pi-trash"
                    style="color: #475057; padding: 0px !important"
                    v-tooltip.top="'Xóa bình luận'"
                    class="p-button-outlined border-none"
                    @click="deleteComment(item)"
                  />
                </div>
              </div>

              <div class="col-12 pb-2 pt-0">
                <div
                  class="col-12 p-0 flex"
                  v-if="commentEdit.comment_id == item.comment_id"
                >
                  <div
                    class="
                      col-10
                      p-0
                      ml-2
                      border-1 border-round-xs border-600
                      flex
                    "
                    style="border-radius: 5px"
                  >
                    <Textarea
                      style="border-radius: 5px; padding-top: 10px"
                      class="border-0 col-11 pb-0"
                      placeholder="Viết bình luận..."
                      :autoResize="true"
                      rows="1"
                      id="stextcmt"
                      v-model="commentEdit.des"
                    />
                    <div
                      class="col-1 p-0 relative"
                      style="
                        background-color: #fff;
                        border-radius: 0px 8px 8px 0px;
                      "
                    >
                      <div class="bottom-0 flex format-center">
                        <Button
                          class="
                            p-1 p-button-rounded p-button-text p-button-plain
                            m-1
                          "
                          @click="showEmoji($event, false)"
                        >
                          <img
                            alt="logo"
                            src="/src/assets/image/smile.png"
                            width="20"
                            height="20"
                          />
                        </Button>
                        <div
                          v-if="isShowEmoji"
                          class="absolute right-0 bottom-100"
                          style="z-index: 10001"
                        ></div>
                      </div>
                      <!-- Kiểm tra xem file là ảnh hoặc File rồi y -->
                    </div>
                  </div>
                  <div class="col-2 p-0 flex">
                    <Button
                      icon="pi pi-check"
                      style="
                        background-color: rgb(71 80 87 / 52%);
                        border: none;
                      "
                      class="mx-2 p-button-rounded"
                      @click="saveEditComment"
                    ></Button>
                    <Button
                      icon="pi pi-times"
                      style="
                        background-color: rgb(71 80 87 / 52%);
                        border: none;
                      "
                      class="p-button-rounded"
                      @click="cancelEditComment"
                    ></Button>
                  </div>
                </div>
                <div v-else v-html="item.des"></div>
                <div></div>
              </div>
            </div>
          </div>
          <div class="col-12 p-2 flex" v-if="countComment > commentSize">
            <div class="col-1 p-0"></div>
            <div class="col-11 p-0">
              <span
                class="font-bold text-600 text-lg cursor-pointer"
                @click="onMoreCmt()"
              >
                Xem thêm...</span
              >
            </div>
          </div>
        </div>
      </div>
      <div class="px-3" style="flex: 2">
        <div class="col-12 flex p-0">
          <div class="col-10 p-0 flex font-bold align-items-center text-xl">
            Danh sách phát
          </div>
          <div class="col-2 cursor-pointer">
            <InputSwitch
              class="col-3"
              v-model="video.show_relate"
              v-tooltip.top="'Tự động phát'"
            />
          </div>
        </div>
        <div class="col-12 p-0" style="position: relative">
          <div
            class="col-12 p-0 flex item-relate cursor-pointer"
            v-for="(v, index) in listVideosRelate"
            :key="index"
            @click="showDetails(v)"
          >
            <div class="col-4 px-0" style="position: relative">
              <Image
                :src="
                  v.is_file_upload
                    ? v.image
                      ? basedomainURL + v.image
                      : basedomainURL + '/Portals/Image/youtube.png'
                    : v.image
                    ? basedomainURL + v.image
                    : v.thumbnail
                "
                @error="
                  $event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'
                "
              />
              <span v-if="v.image" class="video-duration text-xs">{{
                v.video_duration
              }}</span>
            </div>
            <div class="col-8 pr-0">
              <div class="relate-title text-2line font-bold line-height-3">
                {{ v.title }}
              </div>
              <div class="relate-created line-height-3">
                {{ v.created_name }}
              </div>
              <div class="relate-bottom line-height-3">
                {{ v.is_visitor || 0 }} người xem |
                {{
                  moment(new Date(v.modified_date)).format("HH:mm DD/MM/YYYY")
                }}
              </div>
            </div>
          </div>
          <div
            class="col-12 flex show-more"
            v-if="countRelate > relateSize"
            style="justify-content: center"
          >
            <span
              class="font-bold text-600 text-lg cursor-pointer"
              style="line-height: 50px"
              @click="onMoreRelate()"
            >
              Xem thêm...</span
            >
          </div>
        </div>
      </div>
      <div class="overflow-y-auto overflow-x-hidden"></div>
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
</template>

<style  scoped>
.show-more {
  position: absolute;
  height: 50px;
  background: linear-gradient(
    to bottom,
    rgb(245 245 245 / 17%) 10%,
    #f5f5f5 80%
  );
  bottom: -15px;
}
.text-2line {
  text-overflow: ellipsis;
  overflow: hidden;
  column-gap: initial;
  -webkit-line-clamp: 2;
  display: -webkit-box;
  -webkit-box-orient: vertical;
}
.hyper-link {
  color: black;
}
.hyper-link:hover {
  color: #1c80cf !important;
}
.item-relate {
  height: 6vw;
}
.item-relate:hover {
  color: #1c80cf;
  cursor: pointer;
}
.video-duration {
  position: absolute;
  background: rgba(0, 0, 0, 0.4);
  font-size: 1rem;
  color: #fff;
  border-radius: 15%;
  padding: 1 3px;
  bottom: 8px;
  right: 5px;
  z-index: 99;
}
.border-bottom {
  border-bottom: 1px solid #000000;
  padding-bottom: 15px !important;
}
.box-comment {
  flex: 18;
}
.bg-gray {
  /* background-color: #f9f9f9; */
  background-color: #fff;
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
::v-deep(.item-relate) {
  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }
}
</style>
