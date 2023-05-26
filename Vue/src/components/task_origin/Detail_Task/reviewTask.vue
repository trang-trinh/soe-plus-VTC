<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import { VuemojiPicker } from "vuemoji-picker";
import { encr } from "../../../util/function.js";
import moment from "moment";
import FileAction from "../Detail_Task/FileInfo.vue";
const cryoptojs = inject("cryptojs");
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const emitter = inject("emitter");

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const props = defineProps({
  id: Intl,
  task_name: String,
  member: Array,
  data: Object,
  isClose: Boolean,
});
const today = ref({});
today.value = new Date();
const basedomainURL = fileURL;

//Lấy size màn hình

//Khai báo
const members = ref();
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);

const memberType = ref();
const memberType1 = ref();
const memberType2 = ref();
const memberType3 = ref();
const loadMember = () => {
  members.value.forEach((element) => {
    if (store.state.user.user_id == element.user_id) {
      if (memberType.value == null) {
        memberType.value = element.is_type;
        return;
      }
      if (memberType1.value == null) {
        memberType1.value = element.is_type;
        return;
      }
      if (memberType2.value == null) {
        memberType2.value = element.is_type;
        return;
      }
      if (memberType3.value == null) {
        memberType3.value = element.is_type;
        return;
      }
    }
  });
  var byDate = members.value.slice(0);
  byDate.sort(function (a, b) {
    return a.is_type - b.is_type;
  });
  members.value = byDate;
};

const panelEmoij1 = ref();
let filecoments = [];
const listFileComment = ref([]);

const checkDel = ref(false);

const showEmoji = (event, check) => {
  if (check == 1) panelEmoij1.value.toggle(event);
};
const handleEmojiClick = (event) => {
  if (openReviewDialog.value == true) {
    Review.value.contents =
      (Review.value.contents ? Review.value.contents : "") + event.unicode;
  } else {
    ReportProgress.value.contents =
      (ReportProgress.value.contents ? ReportProgress.value.contents : "") +
      event.unicode;
  }
};

const chonanh = (id) => {
  document.getElementById(id).value = "";
  document.getElementById(id).click();
};

const delImgComment = (value, index) => {
  checkDel.value = true;
  let arrImg = [];
  let le = filecoments.length;
  for (let index = 0; index < le; index++) {
    const element = filecoments[index];
    if (element != value) {
      arrImg.push(element);
    }
  }
  filecoments = arrImg;
  listFileComment.value = listFileComment.value.filter((x) => x.data != value);
  listFileComment.value = listFileComment.value.filter((x) => x != value);
};

const showDetail1 = ref(false);

emitter.on("SideBar1", (obj) => {
  showDetail1.value = obj;
});
const ReportProgress = ref({
  project_id: null,
  task_id: null,
  review_Id: null,
  request_progress: null,
  progress: null,
  contents: null,
  difficult: null,
  request: null,
  status: null,
  is_send_email: false,
});
const isHaveReport = ref(false);
const ListReport = ref();
const LoadReview = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_reportprogress_list",
            par: [{ par: "id", va: props.id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data0 = JSON.parse(response.data.data)[0];
      ListReport.value = [];

      if (data0 != null)
        data0.forEach((x, i) => {
          let file =
            data0[i].reportFiles != null
              ? JSON.parse(data0[i].reportFiles)
              : null;
          x.reportFiles = file;
          x.creator_tooltip =
            "Người tạo báo cáo <br/>" +
            x.creator_full_name +
            "<br/>" +
            x.creator_positions +
            "<br/>" +
            (x.creator_department_name != null
              ? x.creator_department_name
              : x.creator_organiztion_name);

          x.review = x.review != null ? JSON.parse(x.review) : null;
          if (x.review != null) {
            x.review.contents = x.review.contents[0].contents;
            x.review.reviewer.tooltip =
              "Người đánh giá <br/>" +
              x.review.reviewer.creator_full_name +
              "<br/>" +
              x.review.reviewer.creator_positions +
              "<br/>" +
              (x.review.reviewer.creator_department_name != null
                ? x.review.reviewer.creator_department_name
                : x.review.reviewer.creator_organiztion_name);
          }
        });
      isHaveReport.value = data0.length > 0 ? true : false;
      ListReport.value = data0;
    })
    .catch(() => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const formatSize = (bytes) => {
  if (bytes === 0) {
    return "0 B";
  }

  let k = 1024,
    dm = 3,
    sizes = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"],
    i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
};

const handleFileUploadReport = (event) => {
  filecoments = [];
  filecoments = event.target.files;
  filecoments.forEach((x) => {
    if (x.size > 104857600) {
      swal.fire({
        title: "Thông báo",
        text: "Tệp tải lên không quá 100MB!",
        icon: "error",
        confirmButtonText: "OK",
      });
      return;
    }
  });
  if (
    filecoments.length > 12 ||
    listSendFile.value.length == 12 ||
    listSendFile.value.length + filecoments.length > 12
  ) {
    swal.fire({
      title: "Thông báo",
      text: "Bạn chỉ có thể chọn tối đa 12 tệp!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  let fileIndex = [];
  if (filecoments && listSendFile.value.length < 12) {
    if (listSendFile.value.length == 0) {
      filecoments.forEach((f, i) => {
        listSendFile.value.push(f);
      });
    } else {
      filecoments.forEach((x, i) => {
        let fi = listSendFile.value.filter((z) => x.name == z.name);
        if (fi.length > 0) {
          fileIndex.push(i);
        } else {
          listSendFile.value.push(x);
        }
      });
    }

    filecoments.forEach((filecomentIndex, index) => {
      let check =
        fileIndex.length > 0 ? fileIndex.filter((k) => index == k) : 0;
      if (check.length > 0) {
        return;
      } else {
        const element = filecomentIndex;
        let size = formatSize(element.size);
        var allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i;
        //Kiểm tra định dạng
        if (allowedExtensions.exec(element.name)) {
          listFileComment.value.push({
            data: element,
            src: URL.createObjectURL(element),
            size: size,
            checkimg: true,
          });
          URL.revokeObjectURL(element);
        } else {
          listFileComment.value.push({
            data: element.data,
            src: element.name,
            size: size,
            checkimg: false,
          });
        }
      }
    });
  }
};
const headerReport = ref();
const openReport = ref(false);
const addReport = () => {
  openReport.value = true;
  headerReport.value = "Thêm nội dung báo cáo";
  ReportProgress.value.difficult =
    ReportProgress.value.difficult != null
      ? ReportProgress.value.difficult.replace(/<br\s*\/?>/gi, "\n")
      : "";
  ReportProgress.value.request =
    ReportProgress.value.request != null
      ? ReportProgress.value.request.replace(/<br\s*\/?>/gi, "\n")
      : "";
};
const closeReport = (e) => {
  openReport.value = false;
  if (e == 0) {
    if (ReportProgress.value.difficult || ReportProgress.value.request) {
      console.log("Có dữ liệu");
    } else {
      ReportProgress.value.difficult = "";
      ReportProgress.value.request = "";
    }
  }
  ReportProgress.value.difficult =
    ReportProgress.value.difficult != null
      ? ReportProgress.value.difficult.replace(/\n/g, "<br/>")
      : "";
  ReportProgress.value.request =
    ReportProgress.value.request != null
      ? ReportProgress.value.request.replace(/\n/g, "<br/>")
      : "";
};

const sendReport = () => {
  if (ReportProgress.value.contents == null) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng cập nhật nội dung báo cáo!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  if (ReportProgress.value.request_progress == null) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng cập nhật tiến độ công việc!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  ReportProgress.value.contents =
    ReportProgress.value.contents != null
      ? ReportProgress.value.contents.replace(/\n/g, "<br/>")
      : "";
  ReportProgress.value.task_id = props.id;
  ReportProgress.value.status = 0;
  let formData = new FormData();
  if (filecoments != null)
    for (var i = 0; i < filecoments.length; i++) {
      let file = filecoments[i];
      formData.append("url_file", file);
    }
  formData.append("comment", JSON.stringify(ReportProgress.value));
  axios({
    method: "post",
    url: baseURL + `/api/ReportProgress/${"addReportProgress"}`,
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Thêm mới báo cáo công việc thành công!");
        ReportProgress.value = {
          task_id: props.id,
          review_Id: null,
          request_progress: null,
          progress: null,
          contents: null,
          difficult: null,
          request: null,
          status: null,
          is_send_email: false,
        };
        filecoments = [];
        listFileComment.value = [];
        LoadReview();
      } else {
        swal.fire({
          title: "Error!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch(() => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
//review
const Review = ref({
  project_id: null,
  task_id: null,
  report_id: null,
  contents: "",
  point: null,
  progress: null,
  is_type: null,
});
const openReviewDialog = ref(false);
const isRemake = ref(false);
const headerReview = ref();
const openReview = (e, id) => {
  listFileComment.value = [];
  if (e == 0) {
    Review.value = {
      project_id: null,
      task_id: props.id,
      report_id: id.report_id,
      contents: "",
      point: null,
      progress: null,
      is_type: 1,
    };
    isRemake.value = true;
    headerReview.value = "Yêu cầu làm lại";
  } else {
    Review.value = {
      project_id: null,
      task_id: props.id,
      report_id: id.report_id,
      contents: "",
      point: null,
      progress: id.request_progress,
      is_type: 0,
    };
    headerReview.value = "Đánh giá công việc";
    isRemake.value = false;
  }
  openReviewDialog.value = true;
  submittedReview.value = false;
};
const ruleReview = {
  contents: { required },
};
const v$Review = useVuelidate(ruleReview, Review);
const submittedReview = ref(false);
const sendReview = (e, isFormValid) => {
  if (e == 0) {
    isRemake.value = false;
    openReviewDialog.value = false;
    listFileComment.value = [];
    filecoments = [];
    listSendFile.value = [];
  } else {
    submittedReview.value = true;
    if (!isFormValid) {
      return;
    }
    Review.value.contents =
      Review.value.contents != null
        ? Review.value.contents.replace(/\n/g, "<br/>")
        : "";
    let formData = new FormData();
    if (filecoments != null)
      for (var i = 0; i < filecoments.length; i++) {
        let file = filecoments[i];
        formData.append("url_file", file);
      }
    formData.append("comment", JSON.stringify(Review.value));
    axios({
      method: "post",
      url: baseURL + `/api/Review/${"addReview"}`,
      data: formData,
      headers: {
        Authorization: `Bearer ${store.getters.token}`,
      },
    })
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm mới báo cáo công việc thành công!");
          Review.value = {
            project_id: null,
            task_id: null,
            report_id: null,
            contents: "",
            point: null,
            progress: null,
            is_type: null,
          };
          filecoments = [];
          listFileComment.value = [];
          openReviewDialog.value = false;
          isRemake.value = false;
          emitter.emit("reload", true);
          LoadReview();
        } else {
          swal.fire({
            title: "Error!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch(() => {
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
//
const listSendFile = ref([]);

const ViewFileInfo = (data) => {
  isViewFileInfo.value = true;
  fileInfo.value = data;
};
emitter.on("closeViewFile", (obj) => {
  isViewFileInfo.value = obj;
});
const isViewFileInfo = ref(false);
const fileInfo = ref();

//Thêm bình luận
onMounted(() => {
  members.value = props.member;
  loadMember();
  LoadReview();
});
</script>
<template>
  <div class="h-custom">
    <div class="relative card-container blue-container w-full h-full">
      <div class="relative p-4 border-round w-full h-full">
        <ScrollPanel style="height: calc(100vh - 12rem)" class="relative">
          <div class="col-12 p-0 m-0 font-bold text-xl">
            <i class="pi pi-check-square pr-2"></i>
            <span>
              {{ props.task_name }}
            </span>
          </div>
          <div style="height: auto">
            <div
              class="col-12 p-1 m-1 border-bottom-1 border-bluegray-100 hover-delete"
              v-for="(rp, index) in ListReport"
              :key="index"
            >
              <div class="col-12">
                <div class="flex col-12 p-0 m-0">
                  <div class="flex col-1 p-0 format-center">
                    <Avatar
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                      v-tooltip="{
                        value: rp.creator_tooltip,
                        escape: true,
                      }"
                      v-bind:label="
                        rp.creator_avt
                          ? ''
                          : rp.creator_full_name
                              .split(' ')
                              .at(-1)
                              .substring(0, 1)
                      "
                      v-bind:image="basedomainURL + rp.creator_avt"
                      style="color: #ffffff; cursor: pointer"
                      :style="{
                        background: bgColor[index % 7],
                        border: '1px solid' + bgColor[index % 10],
                      }"
                      class="myTextAvatar p-0 m-0"
                      size="large"
                      shape="circle"
                    />
                  </div>

                  <div
                    class="flex col-11 p-0 m-0 format-left pl-1"
                    style="font-weight: 700; font-size: 16px; color: #385898"
                  >
                    <div class="col-12 p-0 m-0">
                      <div class="flex col-12 p-0 m-0 pl-2 format-left">
                        <div class="col p-0 text-left">
                          {{ rp.creator_full_name }}
                        </div>
                        <div class="col-2 p-0 m-0 pl-2 h-50">
                          <ProgressBar
                            :value="
                              rp.request_progress != null
                                ? rp.request_progress
                                : 0
                            "
                            :showValue="false"
                            style="height: 8px !important"
                            :class="
                              rp.request_progress >= 50 ? 'worked' : 'working'
                            "
                          />
                        </div>

                        <div class="col-1 p-0 m-0 pl-2 font-bold">
                          <Chip
                            :style="{
                              color:
                                rp.request_progress >= 50 ? '#6DD230' : 'red',
                            }"
                            style="background-color: #ffffff"
                            class="font-bold text-xl"
                          >
                            {{ rp.request_progress }}%
                          </Chip>
                        </div>
                        <div class="col-3 p-0 m-0 pl-2 format-left" :style="{}">
                          <Chip
                            :style="{
                              color: '#FFFFFF',
                              background:
                                rp.status == 0
                                  ? '#3355F3'
                                  : rp.status == 1
                                  ? '#59D05D'
                                  : '#FF0000',
                            }"
                            class="font-bold py-1 px-4"
                          >
                            <span v-if="rp.status == 0">Đang đợi đánh giá</span>
                            <span v-if="rp.status == 1">Đã đánh giá</span>
                            <span v-if="rp.status == 2"
                              >Yêu cầu báo cáo lại</span
                            >
                            <span v-if="rp.status == 3">Hủy</span>
                            <span v-if="rp.status == 4"
                              >Không tính kết quả báo cáo</span
                            >
                          </Chip>
                        </div>
                      </div>
                      <div
                        class="flex col-12 text-sm p-0 m-0 pl-2 text-dark font-light mt-1"
                      >
                        {{
                          moment(new Date(rp.created_date)).format(
                            "HH:mm DD/MM/YYYY"
                          )
                        }}
                      </div>
                    </div>
                  </div>
                </div>
                <div class="col-12 contents">
                  <span v-html="rp.contents"></span>
                </div>
                <div class="col-12 contents mt-2" v-if="rp.difficult">
                  <label class="font-bold">Khó khăn, vướng mắc</label>
                  <br />
                  <span v-html="rp.difficult"></span>
                </div>
                <div class="col-12 contents mt-2" v-if="rp.request">
                  <label class="font-bold">Đề xuất giải quyết</label>
                  <br />
                  <span v-html="rp.request"></span>
                </div>
                <div
                  class="col-12 flex format-left mt-1"
                  v-if="rp.reportFiles != null"
                  style="
                    max-width: 100%;
                    height: auto;
                    display: flex;
                    margin-left: 4rem !important;
                  "
                  :style="{
                    'flex-wrap': 'wrap',
                  }"
                >
                  <div
                    v-for="(item, index) in rp.reportFiles"
                    :key="index"
                    class="relative format-left contents2 border-1 file-comments-hover"
                    style=""
                  >
                    <div class="anh2 format-center pt-1">
                      <div class="" v-if="item.data.is_image == 1">
                        <Image
                          :src="basedomainURL + item.data.file_path"
                          :alt="item.data.name"
                          width="90"
                          height="90"
                          preview
                          class="pt-1"
                        />
                        <div
                          class="p-1"
                          style="
                            width: 95px;
                            font-size: 13px;
                            overflow: hidden;
                            text-overflow: ellipsis;
                            display: block;
                            font-weight: 500;
                            white-space: nowrap;
                          "
                          @click="ViewFileInfo(item.data)"
                        >
                          <a download> {{ item.data.file_name }}</a>
                          <br />
                          {{ (item.data.file_size / 1024 / 1024).toFixed(2) }}
                          MB
                        </div>
                      </div>
                      <div class="" v-else>
                        <img
                          :src="
                            basedomainURL +
                            '/Portals/Image/file/' +
                            item.data.file_name.substring(
                              item.data.file_name.lastIndexOf('.') + 1
                            ) +
                            '.png'
                          "
                          style="
                            width: 100px;
                            height: 100px;
                            object-fit: contain;
                          "
                          :alt="''"
                          class="pt-1"
                        />
                        <div
                          class="p-1"
                          style="
                            width: 95px;
                            font-size: 13px;
                            overflow: hidden;
                            text-overflow: ellipsis;
                            display: block;
                            font-weight: 500;
                            white-space: nowrap;
                          "
                          @click="ViewFileInfo(item.data)"
                        >
                          <a download> {{ item.data.file_name }}</a>
                          <br />
                          {{ (item.data.file_size / 1024 / 1024).toFixed(2) }}
                          MB
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div
                  class="col-12"
                  style="padding-left: 6rem !important"
                  v-if="
                    props.isClose == false &&
                    rp.status != 1 &&
                    rp.review == null &&
                    (memberType == 0 ||
                      memberType1 == 0 ||
                      memberType2 == 0 ||
                      memberType3 == 0)
                  "
                >
                  <Button
                    class="p-button-text p-button-danger p-0 m-0 mr-4"
                    @click="openReview(0, rp)"
                    >Yêu cầu làm lại
                  </Button>
                  <Button
                    class="p-button-text p-button-success p-0 m-0"
                    @click="openReview(1, rp)"
                    >Đánh giá</Button
                  >
                </div>
                <div
                  class="col-12 p-0 m-0 pt-2"
                  style="padding-left: 4rem !important"
                  v-if="rp.review != null"
                >
                  <div class="contents3">
                    <div class="flex contents3 col-12">
                      <div class="flex col-1 m-2 p-0 format-center">
                        <Avatar
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                          v-tooltip="{
                            value: rp.review.reviewer.tooltip,
                            escape: true,
                          }"
                          v-bind:label="
                            rp.review.reviewer.creator_avt
                              ? ''
                              : rp.review.reviewer.creator_full_name
                                  .split(' ')
                                  .at(-1)
                                  .substring(0, 1)
                          "
                          v-bind:image="
                            basedomainURL + rp.review.reviewer.creator_avt
                          "
                          style="color: #ffffff; cursor: pointer"
                          :style="{
                            background: bgColor[index % 7],
                            border: '1px solid' + bgColor[index % 10],
                          }"
                          class="myTextAvatar p-0 m-0"
                          size="large"
                          shape="circle"
                        />
                      </div>
                      <div
                        class="flex col-11 p-0 m-0 format-left pl-1"
                        style="
                          font-weight: 700;
                          font-size: 16px;
                          color: #385898;
                        "
                      >
                        <div class="col-12 p-0 m-0">
                          <div class="flex col-12 p-0 m-0 pl-2 format-left">
                            {{ rp.review.reviewer.creator_full_name }}
                            <div class="col-3 p-0 m-0 pl-2 h-50">
                              <Rating
                                :modelValue="rp.review.point"
                                :readonly="true"
                                :stars="5"
                                :cancel="false"
                              />
                            </div>

                            <div
                              class="col-1 p-0 m-0 pl-2 font-bold"
                              v-if="rp.review.progress"
                            >
                              <Chip
                                :style="{
                                  color:
                                    rp.review.progress >= 50
                                      ? '#6DD230'
                                      : 'red',
                                }"
                                style="background-color: #ffffff"
                                class="font-bold text-xl"
                                >{{ rp.review.progress }}%</Chip
                              >
                            </div>
                          </div>
                          <div
                            class="flex col-12 text-sm p-0 m-0 pl-2 text-dark font-light mt-1"
                          >
                            {{
                              moment(new Date(rp.review.created_date)).format(
                                "HH:mm DD/MM/YYYY"
                              )
                            }}
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-12 flex pt-1">
                      <div class="col-1"></div>
                      <div
                        class="col-11 pl-4 div-contents"
                        v-html="rp.review.contents"
                      ></div>
                    </div>
                  </div>
                  <div
                    class="col-12 flex format-left mt-1"
                    v-if="rp.review.reviewFiles != null"
                    style="
                      max-width: 90%;
                      height: auto;
                      display: flex;
                      flex-wrap: wrap;
                      margin-left: 3rem !important;
                    "
                  >
                    <div
                      v-for="(item, index) in rp.review.reviewFiles"
                      :key="index"
                      class="mr-2 relative format-left contents2 border-1 m-1"
                      style=""
                    >
                      <div class="anh2 format-center">
                        <div class="" v-if="item.data.is_image == 1">
                          <Image
                            :src="basedomainURL + item.data.file_path"
                            :alt="''"
                            width="90"
                            height="90"
                            preview
                            class="pt-1"
                          />
                          <div
                            class="p-1"
                            style="
                              width: 95px;
                              font-size: 13px;
                              overflow: hidden;
                              text-overflow: ellipsis;
                              display: block;
                              font-weight: 500;
                              white-space: nowrap;
                            "
                            @click="ViewFileInfo(item.data)"
                          >
                            <a download> {{ item.data.file_name }}</a>
                            <br />
                            {{ (item.data.file_size / 1024 / 1024).toFixed(2) }}
                            MB
                          </div>
                        </div>
                        <div class="" v-else>
                          <img
                            :src="
                              basedomainURL +
                              '/Portals/Image/file/' +
                              item.data.file_name.substring(
                                item.data.file_name.lastIndexOf('.') + 1
                              ) +
                              '.png'
                            "
                            style="
                              width: 100px;
                              height: 100px;
                              object-fit: contain;
                            "
                            :alt="''"
                            class="pt-1"
                          />
                          <div
                            class="p-1"
                            style="
                              width: 95px;
                              font-size: 13px;
                              overflow: hidden;
                              text-overflow: ellipsis;
                              display: block;
                              font-weight: 500;
                              white-space: nowrap;
                            "
                            @click="ViewFileInfo(item.data)"
                          >
                            <a download> {{ item.data.file_name }}</a>
                            <br />
                            {{ (item.data.file_size / 1024 / 1024).toFixed(2) }}
                            MB
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div
              v-if="isHaveReport == false"
              class="col-12 align-items-center justify-content-center p-4 text-center m-auto"
              style="height: calc(100vh - 30rem)"
            >
              <img src="../../../assets/background/nodata.png" height="300" />
              <h3 class="m-1">Chưa có đánh giá</h3>
            </div>
          </div>
          <div
            class="sticky bottom-0 left-0 w-full bg-white"
            :class="
              listFileComment.length > 0
                ? listFileComment.length > 6
                  ? 'h-17rem'
                  : 'h-8rem'
                : ''
            "
            v-if="listFileComment.length > 0 && openReviewDialog == false"
          >
            <div class="absolute col-12 h-full bottom-0 p-0 m-0">
              <div
                class="col-12 p-0 m-0 font-bold pl-2 bg-white"
                style="
                  font-weight: bold;
                  font-size: 16px;
                  margin: 10px 0;
                  border-top: 1px solid #f5f5f5;
                  padding-top: 15px;
                  color: #2196f3;
                "
                v-if="listFileComment.length > 0 && openReviewDialog == false"
              >
                Tệp đính kèm
              </div>
              <div
                class="col-12 m-0 flex format-center bg-white"
                v-if="listFileComment.length > 0 && openReviewDialog == false"
                style="
                  max-width: 70vw;
                  height: auto;
                  display: flex;
                  flex-wrap: wrap;
                "
              >
                <div
                  v-for="(item, index) in listFileComment"
                  :key="index"
                  class="col-2 relative format-center file-hover"
                >
                  <span class="absolute h-8rem right-0 col-12 p-0">
                    <Button
                      @click="
                        delImgComment(item.data ? item.data : item, index)
                      "
                      icon="pi pi-times-circle"
                      class="absolute p-button-danger p-button-text p-button-rounded top-0 right-0 pr-0 mr-0 p-button-hover"
                      v-tooltip="{ value: 'Xóa tệp' }"
                    ></Button>
                  </span>

                  <div class="h-8rem" v-if="item.checkimg == true">
                    <img
                      :src="item.src"
                      :alt="' '"
                      style="
                        max-width: 80px;
                        max-height: 50px;
                        object-fit: contain;
                        margin-top: 5px;
                      "
                      class="pt-1"
                    />
                    <div
                      class="p-1"
                      style="
                        width: 95px;
                        font-size: 13px;
                        overflow: hidden;
                        text-overflow: ellipsis;
                        display: block;
                        font-weight: 500;
                        white-space: nowrap;
                      "
                    >
                      {{ item.data.name }}
                      <br />
                      {{ item.size }}
                    </div>
                  </div>
                  <div class="h-8rem" v-else>
                    <img
                      :src="
                        basedomainURL +
                        '/Portals/Image/file/' +
                        item.src.substring(item.src.lastIndexOf('.') + 1) +
                        '.png'
                      "
                      style="
                        max-width: 80px;
                        max-height: 50px;
                        object-fit: contain;
                        margin-top: 5px;
                      "
                      :alt="' '"
                      class="pt-1"
                    />
                    <div
                      class="p-1"
                      style="
                        width: 95px;
                        font-size: 13px;
                        overflow: hidden;
                        text-overflow: ellipsis;
                        display: block;
                        font-weight: 500;
                        white-space: nowrap;
                      "
                    >
                      {{ item.src }}
                      <br />
                      {{ item.size }}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </ScrollPanel>

        <div
          v-if="isClose == false"
          class="absolute bottom-0 left-0 border-round w-full font-bold text-white"
        >
          <div
            class="col-12 border-1 border-round-xs border-600 flex"
            style="border-radius: 5px"
          >
            <div class="col-9 flex p-0 m-0">
              <div class="border-0 col-8 p-0 m-0 pr-2">
                <Textarea
                  class="w-full h-full"
                  placeholder="Cập nhật kết quả công việc"
                  v-model="ReportProgress.contents"
                >
                </Textarea>
              </div>
              <div class="border-0 col-4 p-0 m-0 pr-3">
                <InputNumber
                  inputId="minmax-buttons"
                  mode="decimal"
                  showButtons
                  :min="0"
                  :max="100"
                  suffix=" %"
                  class="w-full pr-3 h-full"
                  v-tooltip.top="{
                    escape: true,
                    value: 'Tiến độ công việc <br/> (0<= x <=100)',
                  }"
                  v-model="ReportProgress.request_progress"
                />
              </div>
            </div>
            <div class="col-3 p-0 m-0">
              <div class="format-center flex col-12 p-0 m-0 h-full">
                <Button
                  icon="p-custom pi pi-plus-circle"
                  class="p-button-text p-button-plain col-3 format-center w-3rem h-3rem"
                  style="background-color: ; color: black"
                  v-tooltip="{ value: 'Thêm khó khăn/đề xuất' }"
                  @click="addReport()"
                ></Button>
                <Button
                  class="p-button-text p-button-plain col-3 format-center w-3rem h-3rem"
                  @click="showEmoji($event, 1)"
                  v-tooltip="{ value: 'Biểu cảm' }"
                >
                  <img
                    alt="logo"
                    src="/src/assets/image/smile.png"
                    width="20"
                    height="20"
                  />
                </Button>

                <Button
                  class="p-button-text p-button-plain col-3 w-3rem h-3rem"
                  style="background-color: ; color: black"
                  icon="pi pi-paperclip pt-1 pr-0 font-bold"
                  @click="chonanh('anhcongviec')"
                  v-tooltip="{ value: 'Đính kèm tệp' }"
                >
                </Button>
                <Button
                  icon="pi pi-send pt-1 pr-0 font-bold"
                  class="p-button-text p-button-plain col-3 w-3rem h-3rem"
                  style="background-color: ; color: black"
                  @click="sendReport()"
                  v-tooltip="{ value: 'Gửi' }"
                />
                <input
                  class="hidden"
                  id="anhcongviec"
                  type="file"
                  multiple="true"
                  accept="*"
                  @change="handleFileUploadReport"
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <OverlayPanel
    class="p-0"
    ref="panelEmoij1"
    append-to="body"
    :show-close-icon="false"
    id="overlay_panelEmoij1"
  >
    <VuemojiPicker @emojiClick="handleEmojiClick($event)" />
  </OverlayPanel>
  <Dialog
    :header="headerReport"
    v-model:visible="openReport"
    :style="{ width: '40vw' }"
    :closable="false"
  >
    <form>
      <div class="col-12 p-0 m-0">
        <div class="flex col-12">
          <div class="col-2 format-left">Khó khăn</div>
          <div class="col-10">
            <Textarea
              v-model="ReportProgress.difficult"
              :autoResize="true"
              class="w-full"
            />
          </div>
        </div>
        <div class="col-12 flex">
          <div class="col-2 format-left">Đề xuất</div>
          <div class="col-10">
            <Textarea
              v-model="ReportProgress.request"
              :autoResize="true"
              class="w-full"
            />
          </div>
        </div>
      </div>
    </form>

    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeReport('0')"
        class="p-button-text"
      />

      <Button label="Lưu" icon="pi pi-check" @click="closeReport('1')" />
    </template>
  </Dialog>
  <Dialog
    :header="headerReview"
    v-model:visible="openReviewDialog"
    :style="{ width: '60vw', 'z-index': '100000' }"
    :closable="false"
  >
    <form>
      <div class="col-12 p-0 m-0">
        <div class="col-12">
          <div class="left-0 w-full bg-white" v-if="listFileComment.length > 0">
            <div class="col-12 h-full bottom-0 p-0 m-0">
              <div
                class="col-12 p-0 m-0 font-bold pl-2 bg-white"
                style="
                  font-weight: bold;
                  font-size: 16px;
                  margin: 10px 0;
                  border-top: 1px solid #f5f5f5;
                  padding-top: 15px;
                  color: #2196f3;
                "
                v-if="listFileComment.length > 0"
              >
                Tệp đính kèm
              </div>
              <div
                class="col-12 flex format-center bg-white"
                v-if="listFileComment.length > 0"
                style="
                  max-width: 70vw;
                  height: auto;
                  display: flex;
                  flex-wrap: wrap;
                "
              >
                <div
                  v-for="(item, index) in listFileComment"
                  :key="index"
                  class="col-2 relative format-center file-hover"
                >
                  <Button
                    @click="delImgComment(item.data ? item.data : item, index)"
                    icon="pi pi-times-circle"
                    class="absolute p-button-danger p-button-text p-button-rounded top-0 right-0 pr-0 mr-0 p-button-hover"
                    v-tooltip="{ value: 'Xóa tệp' }"
                  ></Button>

                  <div class="" v-if="item.checkimg == true">
                    <img
                      :src="item.src"
                      :alt="' '"
                      style="
                        max-width: 80px;
                        max-height: 50px;
                        object-fit: contain;
                        margin-top: 5px;
                      "
                      class="pt-1"
                    />
                    <div
                      class="p-1"
                      style="
                        width: 95px;
                        font-size: 13px;
                        overflow: hidden;
                        text-overflow: ellipsis;
                        display: block;
                        font-weight: 500;
                        white-space: nowrap;
                      "
                    >
                      {{ item.data.name }}
                      <br />
                      {{ item.size }}
                    </div>
                  </div>
                  <div class="" v-else>
                    <img
                      :src="
                        basedomainURL +
                        '/Portals/Image/file/' +
                        item.src.substring(item.src.lastIndexOf('.') + 1) +
                        '.png'
                      "
                      style="
                        max-width: 80px;
                        max-height: 50px;
                        object-fit: contain;
                        margin-top: 5px;
                      "
                      :alt="' '"
                      class="pt-1"
                    />
                    <div
                      class="p-1"
                      style="
                        width: 95px;
                        font-size: 13px;
                        overflow: hidden;
                        text-overflow: ellipsis;
                        display: block;
                        font-weight: 500;
                        white-space: nowrap;
                      "
                    >
                      {{ item.src }}
                      <br />
                      {{ item.size }}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="flex col-12" v-if="isRemake == false">
          <div class="col-2 format-left">Tiến độ</div>
          <div class="col-10">
            <InputNumber
              inputId="minmax-buttons"
              mode="decimal"
              showButtons
              :min="0"
              :max="100"
              suffix=" %"
              class="col-12 p-0"
              v-tooltip.top="{
                value: 'Tiến độ công việc <br/> (0<= x <=100)',
              }"
              v-model="Review.progress"
            />
            <Slider v-model="Review.progress" class="p-0 m-0 col-12" />
          </div>
        </div>
        <div class="col-12 flex pb-0 mb-0">
          <div class="col-2 format-left">
            Nội dung đánh giá<span class="redsao">(*)</span>:
          </div>
          <div class="col-10">
            <div
              class="col-12 border-1 border-round-xs border-600 flex"
              style="border-radius: 5px"
            >
              <div class="col-10 flex p-0 m-0">
                <div class="border-0 col-12 p-0 m-0">
                  <Textarea
                    class="w-full"
                    :autoResize="true"
                    placeholder="Nội dung đánh giá..."
                    v-model="Review.contents"
                    :class="{
                      'p-invalid':
                        v$Review.contents.$invalid && submittedReview,
                    }"
                  >
                  </Textarea>
                </div>
              </div>

              <div class="col-2 p-0 m-0">
                <div class="format-center flex col-12 p-0 m-0">
                  <!-- v-clickoutside="onHideEmoji" -->

                  <Button
                    class="p-button-text p-button-plain col-3 format-center w-3rem h-4rem"
                    @click="showEmoji($event, 1)"
                    v-tooltip="{ value: 'Biểu cảm', escape: true }"
                  >
                    <img
                      alt="logo"
                      src="/src/assets/image/smile.png"
                      width="20"
                      height="20"
                    />
                  </Button>

                  <Button
                    icon="p-custom pi pi-paperclip"
                    class="p-button-text p-button-plain col-3 w-3rem h-4rem"
                    style="background-color: ; color: black"
                    @click="chonanh('anhcongviec')"
                    v-tooltip="{ value: 'Tải tệp lên' }"
                  >
                  </Button>

                  <input
                    class="hidden"
                    id="anhcongviec"
                    type="file"
                    multiple="true"
                    accept="*"
                    @change="handleFileUploadReport"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
        <div style="display: flex" class="col-12">
          <div class="col-2 text-left"></div>
          <small
            v-if="
              (v$Review.contents.$invalid && submittedReview) ||
              v$Review.contents.$pending.$response
            "
            class="col-10 p-error p-0 m-0"
          >
            <span class="col-12 p-0 pl-3">{{
              v$Review.contents.required.$message
                .replace("Value", "Nội dung đánh giá")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div class="col-12 flex" v-if="isRemake == false">
          <div class="col-2 format-left">Điểm đánh giá</div>
          <Rating
            v-model="Review.point"
            :stars="5"
            style="text-align: left"
            class="col-10"
          />
        </div>
      </div>
    </form>

    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="sendReview('0', !v$Review.$invalid)"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="sendReview('1', !v$Review.$invalid)"
      />
    </template>
  </Dialog>
  <FileAction :data="fileInfo" v-if="isViewFileInfo"></FileAction>
</template>
<style scoped></style>

<style lang="scss" scoped>
.anh {
  width: 8rem;
  height: 7rem;
  border: 1px solid #f5f5f5;
  border-radius: 5px;
}

.anh2 {
  height: 10rem;
}
.format-center {
  display: flex;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-left {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}
</style>
<!-- //Style của gif picker -->
<style scoped>
.myTextAvatar .p-avatar-text {
  font-size: 2rem !important;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-progressbar) {
  &.working {
    .p-progressbar-value {
      border: 0 none;
      margin: 0;
      background: #f44336;
    }
  }

  &.worked {
    .p-progressbar-value {
      border: 0 none;
      margin: 0;
      background: #6dd230;
    }
  }
}

::v-deep(.p-scrollpanel) {
  &.custombar2 {
    .p-scrollpanel-bar-y {
      width: 3px;
    }

    .p-scrollpanel-wrapper {
      border-right: 1px solid var(--surface-ground);
    }

    .p-scrollpanel-bar {
      background-color: var(--primary-color);
      opacity: 1;
      transition: background-color 0.2s;

      &:hover {
        background-color: #007ad9;
      }
    }
  }
}

.text-xl {
  font-size: 14px;
}

.contents {
  text-align: justify;
  color: #333;
  font-size: 14px;
  width: fit-content;
  padding: 1rem 1rem;
  background-color: #f5f5f5;
  border-radius: 10px;
  margin-left: 6rem;
}

.text-dark {
  color: #000000;
}

.contents2 {
  list-style-type: none;
  display: inline-block;
  width: 10rem;
  text-align: center;
  border: 1px solid #eee;
  border-radius: 5px;
  cursor: pointer;
}

::v-deep(.p-treeselect-items-wrapper) {
  max-height: 200px !important;
  min-width: calc(100vw - 1100px) !important;
  max-width: calc(100vw - 1100px) !important;
}

.file-hover:hover {
  background-color: #d8edff;
}

a {
  text-decoration: none;
}

.p-button-hover:hover {
  color: #0025f8 !important;
  background: #ffffff !important;
}
.file-comments-hover:hover {
  background-color: #2196f3 !important;
  color: #ffffff !important;
}

.h-custom {
  height: calc(100vh - 5rem);
}
.p-button-hover:hover {
  color: #0025f8 !important;
  background: #ffffff !important;
}

.hover-delete:hover .hover-delete-child {
  display: block;
}
.file-hover {
  border: 1px solid rgb(73, 73, 73);
  border-radius: 5px;
}
.contents3 {
  text-align: justify;
  color: #333;
  font-size: 14px;
  width: 100%;
  background-color: #f5f5f5;
  border-radius: 10px;
}
.div-contents {
  background-color: #e4fcff;
  border-radius: 10px;
}
</style>
