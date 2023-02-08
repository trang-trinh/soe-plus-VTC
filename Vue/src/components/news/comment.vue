<script setup>
import { ref, inject, onMounted, watch, onUpdated } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";
import { VuemojiPicker } from "vuemoji-picker";
import "animate.css";
import { forEach } from "jszip";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const toast = useToast();
const router = inject("router");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const news = ref();
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

/*---------------- Giải thích Component
----objectData : Kiểu dữ liệu truyền đi
Example: video{
  video_id:1,
  user_id:store.getters.user.user_id,
  des:null
}
----Controller :Tên controller nhận dữ liệu xử lý
----dataComments: danh sách bình luận. cần tải ở component cha.
----refreshData: Truyền vào 1 hàm reload dữ liệu bình luận

-----options : lựa chọn component
 options.isShowInput :Type:Boolean : Hiển thị chỗ nhập bình luận
 options.isUploadFile :Type:Boolean :Bình luận cho phép UploadFile
  options.isReply :Type:Boolean : Cho phép trả lời bình luận
options.isReaction :Type:Boolean : Cho phép thả reaction
---- comment_count: số bình luận hiện có để hiển thị loadMore
--------Các cài đặt cần tuân thủ trong Controller
-Tên các Action: 
+ add_comment:Thêm
+ update_comment:Sửa
+ delete_comment:Xóa
-----
Lấy dữ liệu bằng call Proc cũng phải giống với mẫu ạ. Mẫu là task_bugcomment
*/
//Nơi nhận dữ liệu
const props = defineProps({
  objectData: Object,
  Controller: String,
  dataComments: Object,
  refreshData: Function,
  options: Object,
  comment_count: Intl,
});
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "renderComment":
      renderComment(props.dataComments);
      break;
  }
});

const options = ref({
  IsNext: true,

  SearchText: "",
  PageNo: 0,
  PageSize: 7,
  loading: true,
  totalRecords: null,
});
const emoteList = ref([]);
const loadEmote = () => {
  axios
    .post(
      baseUrlCheck + "api/law_comment/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "ca_emotes_list",
						par: [
              { par: "pageno", va: options.value.PageNo },
              { par: "pagesize", va: options.value.PageSize },
              { par: "search", va: options.value.SearchText },
              { par: "status", va: options.value.Status },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      emoteList.value = data;
      options.value.loading = false;
    })
    .catch((error) => {
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
  axios
    .post(
      baseUrlCheck + "api/law_comment/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "ca_emotes_count",
						par: [
              { par: "search", va: options.value.SearchText },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
      }
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCount",
        controller: "TemView.vue",
        log_content: error.message,
      });
    });
};
onMounted(() => {
  // comment_zone_main.value.focus()
  if (props.options.isReaction) loadEmote();
  if (props.options.isReply || props.options.isUploadFile) renderComment(props.dataComments);
  return {};
});
const comment = ref("");
const commentFile = ref("");
const commentReplyText = ref("");
const isShowEmoji = ref(false);

let filecoments = [];
const listFileComment = ref([]);
const commentChild = ref("");

const addCommentFile = (check, comment_id) => {
  if (check) {
    if (
      (commentFile.value == "" || commentFile.value == null) &&
      filecoments.length == 0
    ) {
      return;
    }
    else {
      props.objectData.des = commentFile.value;

      let formData = new FormData();
      // for (var i = 0; i < filecoments.length; i++) {
      //   let file = filecoments[i];
      //   formData.append("url_file", file);
      // }
      for (var i = 0; i < listFileComment.value.length; i++) {
        let file = listFileComment.value[i].data;
        formData.append("url_file", file);
      }
      formData.append("comment", JSON.stringify(props.objectData));

      axios
        .post(
          //baseURL
          baseUrlCheck + "/api/" + props.Controller + "/add_comment",
          formData,
          config
        )
        .then((response) => {
          if (response.data.err != "1") {
            swal.close();
            toast.success("Thêm bình luận thành công!");
            filecoments = [];
            listFileComment.value = [];
            comment.value = "";
            props.refreshData();
            comment_zone_main.value.setHTML("");
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
          console.log(error);
          swal.close();
          swal.fire({
            title: "Error!",
            text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
            icon: "error",
            confirmButtonText: "OK",
          });
        });
    }
  } else if (
    (commentChild.value == "" || commentChild.value == null) &&
    filecoments.length == 0
  ) {
    return;
  }
  else {
    if (!props.objectData.parent_id) props.objectData.parent_id = comment_id;
    props.objectData.des = commentChild.value;
    let formData = new FormData();
    for (var i = 0; i < filecoments.length; i++) {
      let file = filecoments[i];
      formData.append("url_file", file);
    }
    formData.append("comment", JSON.stringify(props.objectData));
    axios
      .post(
        //baseURL
        baseUrlCheck + "/api/" + props.Controller + "/add_comment",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm bình luận thành công!");
          filecoments = [];
          listFileCommentChild.value = [];
          commentChild.value = "";
          props.refreshData();
          // comment_zone1.value.setHTML("");
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
      .catch((err) => {
        console.log(err);
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
const addCommentReply = (value) => {
  if (commentReplyText.value == "" || commentReplyText.value == null) return;
  else {
    props.objectData.parent_id = value.comment_id;
    props.objectData.des = commentReplyText.value;
    let formData = new FormData();
    // for (var i = 0; i < filesReply.length; i++) {
    //   let file = filesReply[i];
    //   formData.append("url_file", file);
    // }
    for (var i = 0; i < listFileCommentReply.value.length; i++) {
      let file = listFileCommentReply.value[i].data;
      formData.append("url_file", file);
    }
    formData.append("comment", JSON.stringify(props.objectData));
    axios
      .post(
        baseURL + "/api/" + props.Controller + "/add_comment",
        formData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm bình luận thành công!");
          filesReply = [];
          listFileCommentReply.value = [];
          commentReplyText.value = "";
          props.refreshData();
          comment_zone_reply.value[0].setHTML("");
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
        console.log(error);
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
const addComment = () => {
  if (comment.value == "" || comment.value == null) return;
  else {
    props.objectData.des = comment.value;

    props.objectData.des = props.objectData.des.replaceAll("\n", "<br/>");

    comment.value = "";
    axios
      .post(
        baseURL + "/api/" + props.Controller + "/add_comment",
        props.objectData,
        config
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm bình luận thành công!");
          props.refreshData();
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
function isJson(str) {
  try {
    JSON.parse(str);
  } catch (e) {
    return false;
  }
  return true;
}
const listCommentRender = ref();
const renderComment = (listComment) => {
  listComment.forEach((element) => {
    if (element.reaction != null && element.reaction != "") {
      if (isJson(element.reaction))
        element.reaction = JSON.parse(element.reaction);
    }
    if (!element.listemotes) element.listemotes = [];
    if (element.reaction)
      element.reaction.forEach((item, i = 0) => {
        if (i <= 2) {
          element.listemotes.push(item);
          i++;
        }
      });
    if (element.url_file && !Array.isArray(element.url_file)) {
      element.url_file = element.url_file.split(",");
      let arrFile = [];
      let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
      element.url_file.forEach((urlFile) => {
        //Kiểm tra định dạng
        if (allowedExtensions.exec(urlFile)) {
          arrFile.push({
            data: urlFile.substring(20),
            src: baseURL + urlFile,
            checkimg: true,
            url_file: urlFile,
            nameFile: urlFile.substring(urlFile.lastIndexOf('/') + 1)
          });
          URL.revokeObjectURL(urlFile);
        } else {
          arrFile.push({
            data: urlFile.substring(20),
            src: baseURL + urlFile,
            checkimg: false,
            url_file: urlFile,
            nameFile: urlFile.substring(urlFile.lastIndexOf('/') + 1)
          });
        }
      });
      element.url_file = arrFile;
    }
  });
  let arrChils = [];
  listComment
    .filter((x) => x.parent_id == null)
    .forEach((m) => {
      let om = { key: m.comment_id, data: m };
      const rechildren = (mm, comment_id) => {
        if (!mm.children) mm.children = [];
        let dts = listComment.filter((x) => x.parent_id == comment_id);
        if (dts.length > 0) {
          dts.forEach((em) => {
            let om1 = { key: em.comment_id, data: em };
            rechildren(om1, em.comment_id);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m.comment_id);
      arrChils.push(om);
    });
  listCommentRender.value = arrChils;
};
const panelEmoij4 = ref();
const checkEditEmoij = ref(1);
const showEmoji = (event, check) => {
  if (check == 1) checkEditEmoij.value = 1;
  else if (check == 2) {
    checkEditEmoij.value = 2;
  } else if (check == 3) {
    checkEditEmoij.value = 3;
  } else if (check == 4) {
    checkEditEmoij.value = 4;
  }
  panelEmoij4.value.toggle(event);
};
const dataEmote = ref({ comment_id: null, emote_id: null });
const showEmote = (event, data) => {
  checkHide.value = false;
  dataEmote.value.comment_id = data.comment_id;
  panelEmote.value.toggle(event);
};
const showEmoteR = (event, data) => {
  checkHide.value = false;
  dataEmote.value.comment_id = data.comment_id;
  panelEmote.value.toggle(event);
};
const holdEmote = () => {
  checkHide.value = false;
};
const hideEmote = () => {
  checkHide.value = true;

  setTimeout(() => {
    hideEmoteR();
  }, 1000);
};
const checkHide = ref(false);
const hideEmoteR = () => {
  if (checkHide.value == true) panelEmote.value.hide();
};
const checkHover = ref(0);
const onCheckHover = (item) => {
  checkHover.value = item.emote_id;
};
const hideCheckHover = () => {
  checkHover.value = null;
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
const panelEmote = ref();
const chonanh = (id) => {
  document.getElementById(id).value = "";
  document.getElementById(id).click();
};
const onNextEmote = () => {
  options.value.PageNo += 1;
  loadEmote();
};

const onPreEmote = () => {
  options.value.PageNo -= 1;
  loadEmote();
};
const checkReplyCmt = ref();
const checkAddChildCmt = ref();
const showAddChildCmt = (value) => {
  checkAddChildCmt.value = value.comment_id;
  checkReplyCmt.value = value.comment_id;
};
const addEmote = (emote_id) => {
  dataEmote.value.emote_id = emote_id;
  axios
    .post(
      baseURL + "/api/" + props.Controller + "_emotes/add_emote",
      dataEmote.value,
      config
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        props.refreshData();
        panelEmote.value.hide();
        // checkAddChildCmt.value = dataEmote.value.comment_id;
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
      console.log(error);
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const comment_zone_main = ref();
const comment_zone_edit = ref();
const handleEmojiClick = (event) => {
  if (checkEditEmoij.value == 1)
    if (comment.value) comment.value = comment.value + event.unicode;
    else comment.value = event.unicode;
  else if (checkEditEmoij.value == 3) {
    if (commentEdit.value.des)
      commentEdit.value.des = commentEdit.value.des + event.unicode;
    else commentEdit.value.des = event.unicode;
    //commentEdit.value.des = commentEdit.value.des.replace("<p>", "").replace("</p>", "");
    //comment_zone_edit.value[0].setHTML("<p>" + commentEdit.value.des + "</p>");
    let cmtFormat = commentEdit.value.des.replaceAll("<br>", "").replaceAll("</p><p>", "</p><br><p>");
    let cmtData = cmtFormat.replaceAll("<p>", "").replaceAll("</p>", "");
    let arrCmt = cmtData.split("<br>");
    let contentCmt = "";
    arrCmt.forEach((x) => {
      contentCmt += ("<p>" + x + "</p>");
    });
    commentEdit.value.des = contentCmt;
    comment_zone_edit.value[0].setHTML(commentEdit.value.des);
  } else if (checkEditEmoij.value == 2) {
    if (commentFile.value)
      commentFile.value = commentFile.value + event.unicode;
    else commentFile.value = event.unicode;
    //commentFile.value = commentFile.value.replace("<p>", "").replace("</p>", "");
    //comment_zone_main.value.setHTML("<p>" + commentFile.value + "</p>");
    let cmtFormat = commentFile.value.replaceAll("<br>", "").replaceAll("</p><p>", "</p><br><p>");
    let cmtData = cmtFormat.replaceAll("<p>", "").replaceAll("</p>", "");
    let arrCmt = cmtData.split("<br>");
    let contentCmt = "";
    arrCmt.forEach((x) => {
      contentCmt += ("<p>" + x + "</p>");
    });
    commentFile.value = contentCmt;
    comment_zone_main.value.setHTML(commentFile.value);
  } else if (checkEditEmoij.value == 4) {
    if (commentReplyText.value)
      commentReplyText.value = commentReplyText.value + event.unicode;
    else commentReplyText.value = event.unicode;
    //commentReplyText.value = commentReplyText.value.replace("<p>", "").replace("</p>", "");
    //comment_zone_reply.value[0].setHTML("<p>" + commentReplyText.value + "</p>");
    let cmtFormat = commentReplyText.value.replaceAll("<br>", "").replaceAll("</p><p>", "</p><br><p>");
    let cmtData = cmtFormat.replaceAll("<p>", "").replaceAll("</p>", "");
    let arrCmt = cmtData.split("<br>");
    let contentCmt = "";
    arrCmt.forEach((x) => {
      contentCmt += ("<p>" + x + "</p>");
    });
    commentReplyText.value = contentCmt;
    comment_zone_reply.value[0].setHTML(commentReplyText.value);
  }
};
const checkFileComment = ref(false);
const handleFileUploadComment = (event) => {
  // listFileComment.value = [];
  // filecoments = [];
  filecoments = event.target.files;
  if (filecoments) {
    checkFileComment.value = true;
    for (let index = 0; index < filecoments.length; index++) {
      const element = filecoments[index];

      var allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
      //Kiểm tra định dạng
      if (allowedExtensions.exec(element.name)) {
        listFileComment.value.push({
          data: element,
          src: URL.createObjectURL(element),
          checkimg: true,
        });
        URL.revokeObjectURL(element);
      } else {
        listFileComment.value.push({
          data: element,
          src: element.name,
          checkimg: false,
        });
      }
    }
  }
};
var filesEdit = [];
const listFileCommentEdit = ref([]);
var filesReply = [];
const listFileCommentReply = ref([]);

const handleFileUploadCommentReply = (event) => {
  // listFileComment.value = [];
  // filecoments = [];
  filesReply = event.target.files;
  if (filesReply) {
    checkFileComment.value = true;
    for (let index = 0; index < filesReply.length; index++) {
      const element = filesReply[index];
      var allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
      //Kiểm tra định dạng
      if (allowedExtensions.exec(element.name)) {
        listFileCommentReply.value.push({
          data: element,
          src: URL.createObjectURL(element),
          checkimg: true,
        });
        URL.revokeObjectURL(element);
      } else {
        listFileCommentReply.value.push({
          data: element,
          src: element.name,
          checkimg: false,
        });
      }
    }
  }
};
const handleFileUploadCommentEdit = (event) => {
  // listFileComment.value = [];
  // filecoments = [];
  filesEdit = event.target.files;
  if (filesEdit) {
    checkFileComment.value = true;
    for (let index = 0; index < filesEdit.length; index++) {
      const element = filesEdit[index];
      var allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
      //Kiểm tra định dạng
      if (allowedExtensions.exec(element.name)) {
        listFileCommentEdit.value.push({
          data: element,
          src: URL.createObjectURL(element),
          checkimg: true,
        });
        URL.revokeObjectURL(element);
      } else {
        listFileCommentEdit.value.push({
          data: element,
          src: element.name,
          checkimg: false,
        });
      }
    }
  }
};

const delImgCommentEdit = (value) => {
  let arrImg = [];
  for (let index = 0; index < filesEdit.length; index++) {
    const element = filesEdit[index];
    if (element != value) {
      arrImg.push(element);
    }
  }
  filesEdit = arrImg;
  listFileCommentEdit.value = listFileCommentEdit.value.filter(
    (x) => x.data != value
  );
};

const delImgCommentReply = (value) => {
  let arrImg = [];
  for (let index = 0; index < filesReply.length; index++) {
    const element = filesReply[index];
    if (element != value) {
      arrImg.push(element);
    }
  }
  filesReply = arrImg;
  listFileCommentReply.value = listFileCommentReply.value.filter(
    (x) => x.data != value
  );
};
const delImgCommentDetails = (data, value) => {
  data.url_file = data.url_file.filter((x) => x.data != value);
};
const delImgComment = (value) => {
  let arrImg = [];
  for (let index = 0; index < filecoments.length; index++) {
    const element = filecoments[index];
    if (element != value) {
      arrImg.push(element);
    }
  }
  filecoments = arrImg;
  listFileComment.value = listFileComment.value.filter((x) => x.data != value);
  if (listFileComment.value.length == 0){
    checkFileComment.value = false;
  }
};
const isShowEmotes = ref(false);
const listReaction = ref();
const onShowEmotes = (value) => {
  axios
    .post(
      baseUrlCheck + "api/law_comment/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "api_commentbug_reaction",
						par: [
							{ par: "comment_id", va: value.comment_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      listReaction.value = data;
    })
    .catch((error) => {
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
  isShowEmotes.value = true;
};
const handleFileUploadChild = (event) => {
  listFileCommentChild.value = [];
  filecoments = [];
  filecoments = event.target.files;
  if (filecoments) {
    checkFileCommentChild.value = true;
    for (let index = 0; index < filecoments.length; index++) {
      const element = filecoments[index];

      var allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
      //Kiểm tra định dạng
      if (allowedExtensions.exec(element.name)) {
        listFileCommentChild.value.push({
          data: element,
          src: URL.createObjectURL(element),
          checkimg: true,
        });
        URL.revokeObjectURL(element);
      } else {
        listFileCommentChild.value.push({
          data: element,
          src: element.name,
          checkimg: false,
        });
      }
    }
  }
};
const delImgCommentChild = (value) => {
  let arrImg = [];
  for (let index = 0; index < filecoments.length; index++) {
    const element = filecoments[index];
    if (element != value) {
      arrImg.push(element);
    }
  }
  filecoments = arrImg;
  listFileCommentChild.value = listFileCommentChild.value.filter(
    (x) => x.data != value
  );
};
const onMoreCmt = () => {
  commentSize.value += 10;
  emitter.emit("emitData", {
    type: "loadComment",
    data: commentSize.value,
  });
};
const comment_zone_reply = ref();
const commentEdit = ref({ comment_id: null, des: "" });
var desSave = "";
const editComment = (value) => {
  desSave = value.des;
  value.des = value.des.replaceAll("<br/>", "\n");
  commentEdit.value = value;
  listFileCommentEdit.value = [];
  // document.getElementById('stextcmt').focus();
};
const cancelEditCommentEdit = () => {
    props.dataComments
     .filter((x) => x.comment_id == commentEdit.value.comment_id)
     .forEach((x) => {
       x.des = desSave;
     });
   commentEdit.value = { comment_id: null, des: "" };
};
const cancelEditComment = () => {
  props.dataComments
    .filter((x) => x.comment_id == commentEdit.value.comment_id)
    .forEach((x) => {
      x.des = desSave;
    });
  commentEdit.value = { comment_id: null, des: "" };
};
const saveEditCommentEdit = (item) => {
  commentEdit.value.des = commentEdit.value.des.replaceAll("\n", "<br/>");
  let arc = "";
  let detached = "";
  if (item.url_file) {
    item.url_file.forEach((x) => {
      arc = arc + detached + x.url_file;
      detached = ",";
    });
    item.url_file = arc;
  }

  let formData = new FormData();
  // for (var i = 0; i < filesEdit.length; i++) {
  //   let file = filesEdit[i];
  //   formData.append("url_file", file);
  // }
  for (var i = 0; i < listFileCommentEdit.value.length; i++) {
    let file = listFileCommentEdit.value[i].data;
    formData.append("url_file", file);
  }
  formData.append("comment", JSON.stringify(item));
  
  axios
    .put(
      baseURL + "/api/" + props.Controller + "/update_comment",
      formData,
      config
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Sửa bình luận thành công!");
        listFileCommentEdit.value = [];
        filesEdit = [];
        props.refreshData();
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

  // axios
  //   .put(baseURL + "/api/task_bugcomment/Update_Bugcomment", formData, config)
  //   .then((response) => {
  //     if (response.data.err != "1") {
  //       swal.close();
  //       toast.success("Cập nhật lỗi thành công!");
  //       isShowAddBugComment.value = false;
  //     } else {
  //       console.log("LỖI A:", response);
  //       swal.fire({
  //         title: "Error!",
  //         text: response.data.ms,
  //         icon: "error",
  //         confirmButtonText: "OK",
  //       });
  //     }
  //   })
  //   .catch(() => {
  //     swal.close();
  //     swal.fire({
  //       title: "Error!",
  //       text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
  //       icon: "error",
  //       confirmButtonText: "OK",
  //     });
  //   });
};
const saveEditComment = (item) => {
  commentEdit.value.des = commentEdit.value.des.replaceAll("\n", "<br/>");
  axios
    .put(
      baseURL + "/api/" + props.Controller + "/update_comment",
      commentEdit.value,
      config
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Sửa bình luận thành công!");
        props.refreshData();
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
const ShowMore = (item) => {
  props.dataComments.find(
    (x) => x.comment_id == item.comment_id
  ).is_show_more = true;
};
const HidenMore = (item) => {
  props.dataComments.find(
    (x) => x.comment_id == item.comment_id
  ).is_show_more = false;
  checkHide.value = true;
  setTimeout(() => {
    hideEmoteR();
  }, 1000);
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
          .delete(baseURL + "/api/" + props.Controller + "/delete_comment", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: data.comment_id != null ? [data.comment_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá bình luận thành công!");
              props.refreshData();
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
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const commentSize = ref(10);
</script>
<template>
  <div class="grid w-full">
    <div class="col-12 flex pt-0 py-3">
      <span style="color: #007ad4; font-weight: bold; font-size: 16px"
        ><i class="pi pi-comments pr-1" style="font-size: 16px"></i>Bình luận
        ({{ props.comment_count }})</span
      >
    </div>
    <div class="col-12 pr-3 flex" v-if="props.options.isShowInput">
      <div class="flex-1 p-0 format-center" style="align-items: baseline">
        <Avatar
          v-bind:label="
            store.getters.user.avatar
              ? ''
              : store.getters.user.full_name.substring(
                  store.getters.user.full_name.lastIndexOf(' ') + 1,
                  store.getters.user.full_name.lastIndexOf(' ') + 2
                )
          "
          v-bind:image="basedomainURL + store.getters.user.avatar"
          class="w-3rem"
          size="large"
          shape="circle"
          :style="
            store.getters.user.avatar
              ? 'background-color: #2196f3'
              : 'background:' + bgColor[store.getters.user.full_name.length % 7]
          "
          @error="
            $event.target.src = basedomainURL + '/Portals/Image/nouser.png'
          "
        />
      </div>
      <div style="flex: 18 !important" class="flex">
        <div
          v-if="!props.options.isUploadFile"
          class="p-0 ml-2 border-1 border-round-xs border-600 flex flex-1"
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
                class="p-1 p-button-rounded p-button-text p-button-plain m-1"
                @click="showEmoji($event, 1)"
              >
                <img
                  alt="logo"
                  src="/src/assets/image/smile.png"
                  width="18"
                  height="18"
                />
              </Button>

              <!-- Kiểm tra xem file là ảnh hoặc File rồi y -->
            </div>
          </div>
        </div>
        <div
          v-else
          class="p-0 ml-2 border-1 border-round-xs border-600 flex flex-1"
          style="border-radius: 5px"
        >
          <!-- <div class="border-0 col-10 p-0"> -->
          <div class="border-0 p-0 flex" style="flex:1;">
            <QuillEditor
              ref="comment_zone_main"
              placeholder="Viết bình luận..."
              contentType="html"
              v-model:content="commentFile"
              theme="bubble"
              style="width: -webkit-fill-available;"
            />
          </div>
          <!-- <div class="col-2 p-0 relative flex" style="align-items:center;justify-content:center;"> -->
          <div class="p-0 relative flex" style="align-items:center;justify-content:center;">
            <div class="format-center flex">
              <!-- v-clickoutside="onHideEmoji" -->

              <Button
                class="p-1 p-button-rounded p-button-text p-button-plain m-1"
                @click="showEmoji($event, 2)"
              >
                <img
                  alt="logo"
                  src="/src/assets/image/smile.png"
                  width="18"
                  height="18"
                />
              </Button>

              <Button
                class="p-1 p-button-rounded p-button-text p-button-plain m-1"
                @click="chonanh('anhcongviec')"
              >
                <img
                  alt="logo1"
                  src="/src/assets/image/imageicon.png"
                  width="18"
                  height="18"
                />
              </Button>
              <Button
                @click="chonanh('FileCommentTask')"
                class="p-1 p-button-rounded p-button-text p-button-plain m-1"
              >
                <img
                  alt="logo2"
                  src="/src/assets/image/filesymbol.png"
                  width="18"
                  height="18"
                />
              </Button>
              <input
                class="hidden"
                id="anhcongviec"
                type="file"
                multiple="true"
                accept="image/*"
                @change="handleFileUploadComment"
              />
              <input
                class="hidden"
                id="FileCommentTask"
                type="file"
                multiple="true"
                @change="handleFileUploadComment"
              />
            </div>
          </div>
        </div>
        <div class="pl-2 py-0 format-center" style="min-width:5rem;">
          <Button
            v-if="!props.options.isUploadFile"
            label="Gửi"
            @click="addComment()"
            class="w-full ml-0 p-2"
            style="
              border-radius: 6px;
              bacground-color: #386076 !important;
              height: -webkit-fill-available; max-height: 50px;
            "
          />
          <Button
            v-else
            label="Gửi"
            @click="addCommentFile(true, null)"
            class="w-full ml-0 p-2"
            style="
              border-radius: 6px;
              bacground-color: #386076 !important;
              height: -webkit-fill-available; max-height: 50px;
            "
          />
        </div>
      </div>
    </div>
    <div class="col-12 flex" v-if="checkFileComment">
      <div
        v-for="(item, index) in listFileComment"
        :key="index"
        class="mr-3 relative c1"
      >
        <Button
          @click="delImgComment(item.data)"
          icon="pi pi-times"
          class="
            p-button-rounded p-button-text p-button-plain
            absolute
            top-0
            right-0
            w-1rem
            h-1rem
          "
          style="right: -10px !important; top: -10px !important;z-index: 1;"
        ></Button>
        <Image class="flex"
          v-if="item.checkimg"
          :src="item.src"
          :alt="item.data.name"
          preview
          style="max-width: 100%; max-height: 100px; object-fit: contain"
        />
        <div class="flex" style="flex-direction:column;align-items: center;width: 130px;" v-else>
          <img
            :src="
              basedomainURL +
              '/Portals/Image/file/' +
              item.data.name.substring(item.data.name.lastIndexOf('.') + 1) +
              '.png'
            "
            style="width: 50px; height: 50px; object-fit: contain"
            :alt="item.data.name"
          />
          <div class="mt-2 text-center">{{ item.data.name }}</div>
        </div>
      </div>
    </div>
    <div class="col-12 p-0 mt-3"></div>
    <div class="col-12 p-0 data-comment" v-if="props.options.isReply == false">
      <div
        class="col-12 pt-2 pr-3 flex d-comment"
        v-for="(item, index) in props.dataComments"
        :key="index"
        @mouseover="ShowMore(item)"
        @mouseleave="HidenMore(item)"
      >
        <div class="flex-1 p-0 format-center" style="align-items: baseline">
          <Avatar
            v-bind:label="
              item.avatar
                ? ''
                : item.full_name.substring(
                    item.full_name.lastIndexOf(' ') + 1,
                    item.full_name.lastIndexOf(' ') + 2
                  )
            "
            :image="basedomainURL + item.avatar"
            class="w-3rem"
            :style="
           'background-color:' + bgColor[item.full_name.length % 7]
            "
            size="large"
            shape="circle"
            @error="
              $event.target.src = basedomainURL + '/Portals/Image/nouser1.png'
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
                moment(new Date(item.created_date)).format("HH:mm DD/MM/YYYY")
              }})
            </div>

            <div
              class="d-del-comment cmt1"
              v-if="
                (item.created_by == store.getters.user.user_id ) &&
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
            <div class="col-12 p-0 my-2 flex"
              v-if="commentEdit.comment_id == item.comment_id"
            >
              <div
                class="flex-1 p-0 ml-0 border-1 border-round-xs border-600 flex"
                style="border-radius: 5px"
              >
                <!-- <Textarea
                  style="border-radius: 5px; padding-top: 10px"
                  class="border-0 col-11 pb-0"
                  placeholder="Viết bình luận..."
                  :autoResize="true"
                  rows="1"
                  id="stextcmt"
                  v-model="commentEdit.des"
                /> -->
                <div class="border-0 flex-1 p-0">
                  <QuillEditor
                    ref="comment_zone_edit"
                    contentType="html"
                    v-model:content="commentEdit.des"
                    theme="bubble"
                  />
                </div>
                <div
                  class="flex p-0 relative"
                  style="background-color: #fff; border-radius: 0px 8px 8px 0px"
                >
                  <div class="bottom-0 flex format-center">
                    <Button
                      class="
                        p-1 p-button-rounded p-button-text p-button-plain
                        m-1
                      "
                      @click="showEmoji($event, 3)"
                    >
                      <img
                        alt="logo"
                        src="/src/assets/image/smile.png"
                        width="18"
                        height="18"
                      />
                    </Button>
                    <div
                      v-if="isShowEmoji"
                      class="absolute right-0 bottom-100"
                      style="z-index: 10001"
                    ></div>
                    <Button class="p-1 p-button-rounded p-button-text p-button-plain m-1"
                      @click="chonanh('imageeditcmt')"
                      v-if="props.options.isUploadFile"
                    >
                      <img
                        alt="logo1"
                        src="/src/assets/image/imageicon.png"
                        width="18"
                        height="18"
                      />
                    </Button>
                    <Button class="p-1 p-button-rounded p-button-text p-button-plain m-1"
                      @click="chonanh('FileCommentEdit')"
                      v-if="props.options.isUploadFile"
                    >
                      <img
                        alt="logo2"
                        src="/src/assets/image/filesymbol.png"
                        width="18"
                        height="18"
                      />
                    </Button>

                    <input class="hidden"
                      id="imageeditcmt"
                      type="file"
                      multiple="true"
                      accept="image/*"
                      @change="handleFileUploadCommentEdit"
                      v-if="props.options.isUploadFile"
                    />
                    <input class="hidden"
                      id="FileCommentEdit"
                      type="file"
                      multiple="true"
                      @change="handleFileUploadCommentEdit"
                      v-if="props.options.isUploadFile"
                    />
                  </div>
                  <!-- Kiểm tra xem file là ảnh hoặc File rồi y -->
                </div>
              </div>
              <div class="p-0 flex" v-if="!props.options.isUploadFile">
                <Button
                  icon="pi pi-check"
                  style="background-color: rgb(71 80 87 / 52%); border: none"
                  class="mx-2 p-button-rounded"
                  @click="saveEditComment"
                ></Button>
                <Button
                  icon="pi pi-times"
                  style="background-color: rgb(71 80 87 / 52%); border: none"
                  class="p-button-rounded"
                  @click="cancelEditComment"
                ></Button>
              </div>
              <div class="p-0 flex" v-else>
                <Button
                  icon="pi pi-check"
                  style="background-color: rgb(71 80 87 / 52%); border: none"
                  class="mx-2 p-button-rounded"
                  @click="saveEditCommentEdit(item)"
                ></Button>
                <Button
                  icon="pi pi-times"
                  style="background-color: rgb(71 80 87 / 52%); border: none"
                  class="p-button-rounded"
                  @click="cancelEditCommentEdit"
                ></Button>
              </div>
            </div>
            <div class="cmtContent pt-2" v-else v-html="item.des"></div>
            <div></div>
          </div>
          <div class="col-12 flex py-0 mb-2" v-if="(item.url_file != null && item.url_file != '') || listFileCommentEdit.length > 0">
            <div class="mr-2 relative flex" style="align-items:center;justify-content:center;" v-if="item.url_file != null && item.url_file.length > 0">
              <div
                v-for="(ite, index) in item.url_file"
                :key="index"
                class="mr-3 relative c2"
              >
                <Button
                  v-if="commentEdit.comment_id == item.comment_id"
                  @click="delImgCommentDetails(item, ite.data)"
                  icon="pi pi-times"
                  class="
                    p-button-rounded p-button-text p-button-plain
                    absolute
                    top-0
                    right-0
                    w-1rem
                    h-1rem
                  "
                  style="right: -10px !important; top: -10px !important;z-index: 1;"
                ></Button>
                <Image class="flex"
                  v-if="ite.checkimg"
                  :src="ite.src"
                  :alt="ite.data"
                  preview
                  style="max-width: 100%; max-height: 100px; object-fit: contain"
                />
                <div class="flex" style="flex-direction:column;align-items: center;width: 130px;" v-else>
                  <img
                    v-if="ite.data"
                    :src="
                      basedomainURL +
                      '/Portals/Image/file/' +
                      ite.data.substring(ite.data.lastIndexOf('.') + 1) +
                      '.png'
                    "
                    style="width: 50px; height: 50px; object-fit: contain"
                    :alt="ite.data"
                  />
                  <div class="mt-2 text-center">
                    <a class="no-underline" download :href="ite.src">
                      {{ ite.nameFile }}
                    </a>
                  </div>
                </div>
              </div>
            </div>
            <div
              class="mr-2 relative flex"
              v-if="commentEdit.comment_id == item.comment_id && listFileCommentEdit.length > 0"
            >
              <div
                class="mr-3 relative c3"
                v-for="(itemEdit, index) in listFileCommentEdit"
                :key="index"
              >
                <Button
                  @click="delImgCommentEdit(itemEdit.data)"
                  icon="pi pi-times"
                  class="
                    p-button-rounded p-button-text p-button-plain
                    absolute
                    top-0
                    right-0
                    w-1rem
                    h-1rem
                  "
                  style="right: -10px !important; top: -10px !important;z-index: 1;"
                ></Button>
                <Image class="flex"
                  v-if="itemEdit.checkimg"
                  :src="itemEdit.src"
                  :alt="itemEdit.data.name"
                  preview
                  style="max-width: 100%; max-height: 100px; object-fit: contain"
                />
                <div class="flex" style="flex-direction:column;align-items: center;width: 130px;" v-else>
                  <img
                    :src="
                      basedomainURL +
                      '/Portals/Image/file/' +
                      itemEdit.data.name.substring(
                        itemEdit.data.name.lastIndexOf('.') + 1
                      ) +
                      '.png'
                    "
                    style="width: 50px; height: 50px; object-fit: contain"
                    :alt="itemEdit.data.name"
                  />
                  <div class="mt-2 text-center">{{ itemEdit.data.name }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-12 p-2 flex" v-if="props.comment_count > commentSize">
        <div class="flex-1 p-0"></div>
        <div class="p-0 pl-2" style="flex: 18">
          <span
            class="font-bold text-600 text-lg cursor-pointer"
            @click="onMoreCmt()"
          >
            Xem thêm...</span
          >
        </div>
      </div>
    </div>
    <div class="col-12 p-0" v-else>
      <div
        class="col-12 pt-2 pr-3 d-comment"
        @mouseover="ShowMore(item.data)"
        @mouseleave="HidenMore(item.data)"
        v-for="(item, index) in listCommentRender"
        :key="index"
      >
        <div
          class="col-12 p-0 align-items-center flex"
          style="align-items: baseline"
        >
          <div class="col-1 p-0 format-center" style="align-items: baseline">
            <Avatar
              v-bind:label="
                item.data.avatar
                  ? ''
                  : item.data.full_name.substring(
                      item.data.full_name.lastIndexOf(' ') + 1,
                      item.data.full_name.lastIndexOf(' ') + 2
                    )
              "
              :image="basedomainURL + item.data.avatar"
              class="w-3rem"
              size="large"
              :style="
                item.data.avatar
                  ? 'background-color: #2196f3'
                  : 'background:' + bgColor[item.data.full_name.length % 7]
              "
              shape="circle"
              @error="
                $event.target.src = basedomainURL + '/Portals/Image/nouser1.png'
              "
            />
          </div>
          <div class="col-11 p-0 align-items-center flex">
            <div class="font-bold">
              {{ item.data.full_name }}
            </div>
            <div class="text-sm px-2">
              ({{
                moment(new Date(item.data.created_date)).format("HH:mm DD/MM/YYYY")
              }})
            </div>

            <div
              class="d-del-comment cmt2"
              v-if="
                (item.data.created_by == store.getters.user.user_id) &&
                item.data.is_show_more &&
                commentEdit.comment_id != item.data.comment_id
              "
            >
              <Button
                icon="pi pi-pencil"
                style="color: #475057; padding: 0px !important"
                v-tooltip.top="'Sửa bình luận'"
                class="p-button-outlined ml-2 border-none"
                @click="editComment(item.data)"
              />
              <Button
                icon="pi pi-trash"
                style="color: #475057; padding: 0px !important"
                v-tooltip.top="'Xóa bình luận'"
                class="p-button-outlined border-none"
                @click="deleteComment(item.data)"
              />
            </div>
          </div>
        </div>

        <div class="col-12 p-0">
          <div
            class="col-12 p-0 my-2 flex"
            v-if="commentEdit.comment_id == item.data.comment_id"
          >
            <div class="col-1 p-0"></div>
            <div class="col-10 p-0 flex border-1" style="border-radius: 5px">
              <div class="border-0 col-10 p-0">
                <QuillEditor
                  ref="comment_zone_edit"
                  contentType="html"
                  v-model:content="commentEdit.des"
                  theme="bubble"
                />
              </div>
              <div class="col-2 p-0 relative">
                <div class="format-center flex">
                  <Button
                    class="
                      p-1 p-button-rounded p-button-text p-button-plain
                      m-1
                    "
                    @click="showEmoji($event, 3)"
                  >
                    <img
                      alt="logo"
                      src="/src/assets/image/smile.png"
                      width="18"
                      height="18"
                    />
                  </Button>

                  <Button
                    class="
                      p-1 p-button-rounded p-button-text p-button-plain
                      m-1
                    "
                    @click="chonanh('imageeditcmt')"
                  >
                    <img
                      alt="logo1"
                      src="/src/assets/image/imageicon.png"
                      width="18"
                      height="18"
                    />
                  </Button>
                  <Button
                    @click="chonanh('FileCommentEdit')"
                    class="
                      p-1 p-button-rounded p-button-text p-button-plain
                      m-1
                    "
                  >
                    <img
                      alt="logo2"
                      src="/src/assets/image/filesymbol.png"
                      width="18"
                      height="18"
                    />
                  </Button>

                  <input
                    class="hidden"
                    id="imageeditcmt"
                    type="file"
                    multiple="true"
                    accept="image/*"
                    @change="handleFileUploadCommentEdit"
                  />
                  <input
                    class="hidden"
                    id="FileCommentEdit"
                    type="file"
                    multiple="true"
                    @change="handleFileUploadCommentEdit"
                  />
                </div>
              </div>
            </div>
            <div class="col-2 p-0 flex">
              <Button
                icon="pi pi-check"
                style="background-color: rgb(71 80 87 / 52%); border: none"
                class="mx-2 p-button-rounded"
                @click="saveEditCommentEdit(item.data)"
              ></Button>
              <Button
                icon="pi pi-times"
                style="background-color: rgb(71 80 87 / 52%); border: none"
                class="p-button-rounded"
                @click="cancelEditCommentEdit"
              ></Button>
            </div>
          </div>
          <div class="col-12 flex relative p-0 mt-1" v-else-if="item.data.des != null && item.data.des != ''">
            <div class="col-1 p-0"></div>
            <div class="relative p-0 col-11 mb-0">
              <div class="cmtContent text-lg" v-html="item.data.des"></div>
            </div>
          </div>

          <div class="col-12 flex py-0 mb-2" v-if="item.data.url_file != null && item.data.url_file != ''">
            <div class="mr-2 relative">
              <div
                v-for="(ite, index) in item.data.url_file"
                :key="index"
                class="mr-3 relative c4"
              >
                <Button
                  v-if="commentEdit.comment_id == item.data.comment_id"
                  @click="delImgCommentDetails(item.data, ite.data)"
                  icon="pi pi-times"
                  class="
                    p-button-rounded p-button-text p-button-plain
                    absolute
                    top-0
                    right-0
                    w-1rem
                    h-1rem
                  "
                  style="right: -10px !important; top: -10px !important;z-index: 1;"
                ></Button>
                <Image class="flex"
                  v-if="ite.checkimg"
                  :src="ite.src"
                  :alt="ite.data"
                  preview
                  style="max-width: 100%; max-height: 100px; object-fit: contain"
                />
                <div class="flex" style="flex-direction:column;align-items: center;width: 130px;" v-else>
                  <img
                    v-if="ite.data"
                    :src="
                      basedomainURL +
                      '/Portals/Image/file/' + ite.data.substring(ite.data.lastIndexOf('.') + 1) + '.png'
                    "
                    style="width: 50px; height: 50px; object-fit: contain"
                    :alt="ite.data"
                  />
                  <div class="mt-2 text-center">
                    <a class="no-underline" download :href="ite.src">
                      {{ ite.nameFile }}
                    </a>
                  </div>
                </div>
              </div>
            </div>
            <div
              class="mr-2 relative flex"
              v-if="commentEdit.comment_id == item.data.comment_id"
            >
              <div
                class="mr-3 relative c5"
                v-for="(item, index) in listFileCommentEdit"
                :key="index"
              >
                <Button
                  @click="delImgCommentEdit(item.data)"
                  icon="pi pi-times"
                  class="
                    p-button-rounded p-button-text p-button-plain
                    absolute
                    top-0
                    right-0
                    w-1rem
                    h-1rem
                  "
                  style="right: -10px !important; top: -10px !important;z-index: 1;"
                ></Button>
                <Image class="flex"
                  v-if="item.checkimg"
                  :src="item.src"
                  :alt="item.data.name"
                  preview
                  style="max-width: 100%; max-height: 100px; object-fit: contain"
                />
                <div class="flex" style="flex-direction:column;align-items: center;width: 130px;" v-else>
                  <img
                    :src="
                      basedomainURL + '/Portals/Image/file/' + item.data.name.substring(item.data.name.lastIndexOf('.') + 1) + '.png'
                    "
                    style="width: 50px; height: 50px; object-fit: contain"
                    :alt="item.data.name"
                  />
                  <div class="mt-2 text-center">{{ item.data.name }}</div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-12 p-0 my-2 flex">
            <div class="col-1 p-0"></div>
            <div class="col-11 p-0">
              <div class="col-12  m-0 p-0 flex">
            <AvatarGroup
              @click="onShowEmotes(item.data)"
              class="cursor-pointer"
              style="border-radius: 100px"
              v-if="props.options.isReaction"
            >
              <Avatar
                v-for="(ele, index1) in item.data.listemotes"
                :key="index1"
                :image="basedomainURL + ele.emote_file"
                size="large"
                shape="circle"
                class="mr-1"
                style="width: 20px; height: 20px"
              />
            </AvatarGroup>

            <div
              v-if="props.options.isReaction"
              @click="
                addEmote(
                  item.data.emote_name
                    ? null
                    : emoteList.filter((x) => x.emote_name == 'Thích')[0]
                        .emote_id
                )
              "
              @mouseover="showEmote($event, item.data)"
              class="text-md mr-3 flex cursor-pointer like-reply format-center"
              :style="{ color: item.data.text_color }"
            >
              <!-- <img
                :src="basedomainURL + item.data.emote_file"
                :alt="item.data.emote_file"
                width="15"
                height="15"
                class="mr-2"
              /> -->
              {{ item.data.emote_name ? item.data.emote_name : "Thích" }}
            </div>
            <div
              v-if="props.options.isReply"
              class="text-md cursor-pointer like-reply format-center"
              @click="showAddChildCmt(item.data)"
            >
              Phản hồi
            </div>
          </div>
            </div>
          </div>
          <div class="col-12 p-0">
            <div
              v-if="
                checkAddChildCmt != item.data.comment_id &&
                item.data.haschild > 0
              "
              class="text-lg ml-4 text-600 cursor-pointer p-2"
              @click="showAddChildCmt(item.data)"
            >
              <div class="w-full" style="font-size:1rem;">
                <i class="pi pi-reply"
                  style="
                    font-size: 1rem;
                    -moz-transform: scaleY(-1);
                    -o-transform: scaleY(-1);
                    -webkit-transform: scaleY(-1);
                  "
                ></i>
                {{ item.data.haschild }} Phản hồi
              </div>
            </div>
            <div class="col-12 p-0" v-else>
              <div class="col-12 p-0">
                <div
                  class="col-12 pt-2 pr-3 flex d-comment"
                  @mouseover="ShowMore(etem.data)"
                  @mouseleave="HidenMore(etem.data)"
                  v-for="(etem, index) in item.children"
                  :key="index"
                >
                  <div
                    class="flex-1 p-0 format-center"
                    style="align-items: baseline"
                  >
                    <Avatar
                      v-bind:label="
                        etem.data.avatar
                          ? ''
                          : etem.data.full_name.substring(
                              etem.data.full_name.lastIndexOf(' ') + 1,
                              etem.data.full_name.lastIndexOf(' ') + 2
                            )
                      "
                      :image="basedomainURL + etem.data.avatar"
                      class="w-3rem"
                      size="large"
                      :style="
                        etem.data.avatar
                          ? 'background-color: #2196f3'
                          : 'background-color:' +
                            bgColor[etem.data.full_name.length % 7]
                      "
                      shape="circle"
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                    />
                  </div>
                  <div class="p-0 box-comment">
                    <div class="col-11 flex align-items-center pt-0">
                      <div class="font-bold">
                        {{ etem.data.full_name }}
                      </div>
                      <div class="text-sm px-2">
                        ({{
                          moment(new Date(etem.data.created_date)).format(
                            "HH:mm DD/MM/YYYY"
                          )
                        }})
                      </div>

                      <div
                        class="d-del-comment cmt3"
                        v-if="
                          (etem.data.created_by == store.getters.user.user_id) &&
                          etem.data.is_show_more &&
                          commentEdit.comment_id != etem.data.comment_id
                        "
                      >
                        <Button
                          icon="pi pi-pencil"
                          style="color: #475057; padding: 0px !important"
                          v-tooltip.top="'Sửa bình luận'"
                          class="p-button-outlined ml-2 border-none"
                          @click="editComment(etem.data)"
                        />
                        <Button
                          icon="pi pi-trash"
                          style="color: #475057; padding: 0px !important"
                          v-tooltip.top="'Xóa bình luận'"
                          class="p-button-outlined border-none"
                          @click="deleteComment(etem.data)"
                        />
                      </div>
                    </div>

                    <div class="col-12 py-0">
                      <div
                        class="col-12 p-0 my-2 flex"
                        v-if="commentEdit.comment_id == etem.data.comment_id"
                      >
                        <div
                          class="col-10 p-0 flex border-1"
                          style="border-radius: 5px"
                        >
                          <div class="border-0 col-10 p-0">
                            <QuillEditor
                              ref="comment_zone_edit"
                              contentType="html"
                              v-model:content="commentEdit.des"
                              theme="bubble"
                            />
                          </div>
                          <div class="col-2 p-0 relative">
                            <div class="format-center flex">
                              <Button
                                class="
                                  p-1
                                  p-button-rounded
                                  p-button-text
                                  p-button-plain
                                  m-1
                                "
                                @click="showEmoji($event, 3)"
                              >
                                <img
                                  alt="logo"
                                  src="/src/assets/image/smile.png"
                                  width="18"
                                  height="18"
                                />
                              </Button>

                              <Button
                                class="
                                  p-1
                                  p-button-rounded
                                  p-button-text
                                  p-button-plain
                                  m-1
                                "
                                @click="chonanh('imageeditcmt')"
                              >
                                <img
                                  alt="logo1"
                                  src="/src/assets/image/imageicon.png"
                                  width="18"
                                  height="18"
                                />
                              </Button>
                              <Button
                                @click="chonanh('FileCommentEdit')"
                                class="
                                  p-1
                                  p-button-rounded
                                  p-button-text
                                  p-button-plain
                                  m-1
                                "
                              >
                                <img
                                  alt="logo2"
                                  src="/src/assets/image/filesymbol.png"
                                  width="18"
                                  height="18"
                                />
                              </Button>

                              <input
                                class="hidden"
                                id="imageeditcmt"
                                type="file"
                                multiple="true"
                                accept="image/*"
                                @change="handleFileUploadCommentEdit"
                              />
                              <input
                                class="hidden"
                                id="FileCommentEdit"
                                type="file"
                                multiple="true"
                                @change="handleFileUploadCommentEdit"
                              />
                            </div>
                          </div>
                        </div>
                        <div class="col-2 p-0 flex">
                          <Button
                            icon="pi pi-check"
                            style="background-color: rgb(71 80 87 / 52%); border: none;"
                            class="mx-2 p-button-rounded"
                            @click="saveEditCommentEdit(etem.data)"
                          ></Button>
                          <Button
                            icon="pi pi-times"
                            style="background-color: rgb(71 80 87 / 52%); border: none;"
                            class="p-button-rounded"
                            @click="cancelEditCommentEdit"
                          ></Button>
                        </div>
                      </div>
                      <div class="col-12 flex relative pl-0" v-else>
                        <div class="relative p-0">
                          <div class="cmtContent text-lg" v-html="etem.data.des"></div>
                        </div>
                      </div>
                    </div>

                    <div class="col-12 flex py-0">
                      <div
                        class="mr-2 relative"
                        v-if="
                          etem.data.url_file != null && etem.data.url_file != ''
                        "
                      >
                        <div
                          v-for="(ite, index) in etem.data.url_file"
                          :key="index"
                          class="mr-3 relative c6"
                        >
                          <Button
                            v-if="
                              commentEdit.comment_id == etem.data.comment_id
                            "
                            @click="delImgCommentDetails(etem.data, ite.data)"
                            icon="pi pi-times"
                            class="
                              p-button-rounded p-button-text p-button-plain
                              absolute
                              top-0
                              right-0
                              w-1rem
                              h-1rem
                            "
                            style="right: -10px !important; top: -10px !important;z-index: 1;"
                          ></Button>
                          <Image class="flex"
                            v-if="ite.checkimg"
                            :src="ite.src"
                            :alt="ite.data"
                            preview
                            style="
                              max-width: 100%;
                              max-height: 100px;
                              object-fit: contain;
                            "
                          />
                          <div class="flex" style="flex-direction:column;align-items: center;width: 130px;" v-else>
                            <img
                              v-if="ite.data"
                              :src="
                                basedomainURL +
                                '/Portals/Image/file/' + ite.data.substring(ite.data.lastIndexOf('.') + 1) + '.png'
                              "
                              style="width: 50px; height: 50px; object-fit: contain;"
                              :alt="ite.data"
                            />
                            <div class="mt-2 text-center">
                              <a class="no-underline" download :href="ite.src" >
                                {{ ite.nameFile }}
                              </a>
                            </div>
                          </div>
                        </div>
                      </div>
                      <div
                        class="mr-2 relative flex"
                        v-if="commentEdit.comment_id == etem.data.comment_id"
                      >
                        <div
                          class="mr-3 relative c7"
                          v-for="(etem, index) in listFileCommentEdit"
                          :key="index"
                        >
                          <Button
                            @click="delImgCommentEdit(etem.data)"
                            icon="pi pi-times"
                            class="
                              p-button-rounded p-button-text p-button-plain
                              absolute
                              top-0
                              right-0
                              w-1rem
                              h-1rem
                            "
                            style="right: -10px !important; top: -10px !important;z-index: 1;"
                          ></Button>
                          <Image class="flex"
                            v-if="etem.checkimg"
                            :src="etem.src"
                            :alt="etem.data.name"
                            preview
                            style="
                              max-width: 100%;
                              max-height: 100px;
                              object-fit: contain;
                            "
                          />
                          <div class="flex" style="flex-direction:column;align-items: center;width: 130px;" v-else>
                            <img
                              :src="
                                basedomainURL +
                                '/Portals/Image/file/' + etem.data.name.substring(etem.data.name.lastIndexOf('.') + 1) + '.png'
                              "
                              style="width: 50px; height: 50px; object-fit: contain;"
                              :alt="etem.data.name"
                            />
                            <div class="mt-2 text-center">{{ etem.data.name }}</div>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-12 flex">
                      <AvatarGroup
                        v-if="props.options.isReaction"
                        @click="onShowEmotes(etem.data)"
                        class="cursor-pointer"
                        style="border-radius: 100px"
                      >
                        <Avatar
                          v-for="(ele, index1) in etem.data.listemotes"
                          :key="index1"
                          :image="basedomainURL + ele.emote_file"
                          size="large"
                          shape="circle"
                          class="mr-1"
                          style="width: 20px; height: 20px"
                        />
                      </AvatarGroup>
                      <div
                        @click="
                          addEmote(
                            etem.data.emote_name
                              ? null
                              : emoteList.filter(
                                  (x) => x.emote_name == 'Thích'
                                )[0].emote_id
                          )
                        "
                        @mouseover="showEmote($event, etem.data)"
                        @onmousemove="showEmote($event, etem.data)"
                        class="
                          text-md
                          mr-3
                          flex
                          cursor-pointer
                          like-reply
                          format-center
                        "
                        :style="{ color: etem.data.text_color }"
                        v-if="props.options.isReaction"
                      >
                        {{
                          etem.data.emote_name ? etem.data.emote_name : "Thích"
                        }}
                      </div>
                      <!-- @click="showAddChildCmt(etem.data)" -->
                      <div
                        class="text-md cursor-pointer like-reply format-center"
                      >
                        Phản hồi
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div
              class="col-12 flex pt-2 p-0"
              v-if="checkReplyCmt == item.data.comment_id"
            >
              <div class="col-10 p-0 flex border-1" style="border-radius: 5px">
                <div class="border-0 col-10 p-0">
                  <QuillEditor
                    ref="comment_zone_reply"
                    contentType="html"
                    placeholder="Viết bình luận..."
                    v-model:content="commentReplyText"
                    theme="bubble"
                  />
                </div>
                <div class="col-2 p-0 relative">
                  <div class="format-center flex">
                    <Button
                      class="
                        p-1 p-button-rounded p-button-text p-button-plain
                        m-1
                      "
                      @click="showEmoji($event, 4)"
                    >
                      <img
                        alt="logo"
                        src="/src/assets/image/smile.png"
                        width="18"
                        height="18"
                      />
                    </Button>

                    <Button
                      class="
                        p-1 p-button-rounded p-button-text p-button-plain
                        m-1
                      "
                      @click="chonanh('imagereplycmt')"
                    >
                      <img
                        alt="logo1"
                        src="/src/assets/image/imageicon.png"
                        width="18"
                        height="18"
                      />
                    </Button>
                    <Button
                      @click="chonanh('ImageFileCmt')"
                      class="
                        p-1 p-button-rounded p-button-text p-button-plain
                        m-1
                      "
                    >
                      <img
                        alt="logo2"
                        src="/src/assets/image/filesymbol.png"
                        width="18"
                        height="18"
                      />
                    </Button>

                    <input
                      class="hidden"
                      id="imagereplycmt"
                      type="file"
                      multiple="true"
                      accept="image/*"
                      @change="handleFileUploadCommentReply"
                    />
                    <input
                      class="hidden"
                      id="ImageFileCmt"
                      type="file"
                      multiple="true"
                      @change="handleFileUploadCommentReply"
                    />
                  </div>
                </div>
              </div>
              <div class="col-2 p-0 format-center">
                <div class="col-12 format-center">
                  <Button
                    label="Gửi"
                    @click="addCommentReply(item.data)"
                    class="w-full ml-3"
                    style="
                      border-radius: 6px;
                      bacground-color: #386076 !important;
                      height: 30px;
                    "
                  />
                  <!-- <Button
                      v-else
                      label="Gửi"
                      @click="addCommentFile(true, null)"
                      class="w-full ml-3"
                      style="
                        border-radius: 6px;
                        bacground-color: #386076 !important;
                        height: 30px;
                      "
                    /> -->
                </div>
              </div>
            </div>
            <div class="col-12 flex p-0 pt-4" v-if="listFileCommentReply.length > 0">
              <div
                class="pr-2 relative mr-4"
                v-for="(item, index) in listFileCommentReply"
                :key="index"
              >
                <Button
                  @click="delImgCommentReply(item.data)"
                  icon="pi pi-times"
                  class="
                    p-button-rounded p-button-text p-button-plain
                    absolute
                    top-0
                    right-0
                    w-1rem
                    h-1rem
                  "
                  style="right: -10px !important; top: -10px !important;"
                ></Button>
                <Image class="flex"
                  v-if="item.checkimg"
                  :src="item.src"
                  :alt="item.data.name"
                  preview
                  style="max-width: 100%; max-height: 100px; object-fit: contain"
                />
                <div class="flex" style="flex-direction:column;align-items: center;width: 130px;" v-else>
                  <img
                    :src="
                      basedomainURL +
                      '/Portals/Image/file/' +
                      item.data.name.substring(
                        item.data.name.lastIndexOf('.') + 1
                      ) +
                      '.png'
                    "
                    style="width: 50px; height: 50px; object-fit: contain"
                    :alt="item.data.name"
                  />
                  <div class="mt-2 text-center">{{ item.data.name }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="col-12 p-2 flex" v-if="props.comment_count > commentSize">
        <div class="flex-1 p-0"></div>
        <div class="p-0 pl-2" style="flex: 18">
          <span
            class="font-bold text-600 text-lg cursor-pointer"
            @click="onMoreCmt()"
          >
            Xem thêm...</span
          >
        </div>
      </div>
    </div>

    <OverlayPanel
      @mousemove="holdEmote"
      @mouseleave="hideEmote()"
      class="p-0 p-overlaypanel-setup"
      ref="panelEmote"
      appendTo="body"
      :showCloseIcon="false"
      id="overlay_panelEmote"
    >
      <div class="flex p-0">
        <div
          @click="onPreEmote"
          v-if="options.PageNo > 0"
          class="p-0 pl-2 cursor-pointer format-center"
        >
          <i class="pi pi-angle-left" style="font-size: 2rem"></i>
        </div>

        <div
          @mouseover="onCheckHover(item)"
          @mouseleave="hideCheckHover()"
          class="p-0 cursor-pointer format-center"
          style="width: 40px; height: 40px"
          v-for="(item, index) in emoteList"
          :key="index"
        >
          <img
            v-tooltip.top="item.emote_name"
            :src="basedomainURL + item.emote_file"
            :alt="item.emote_file"
            @click="addEmote(item.emote_id)"
            :class="
              checkHover == item.emote_id
                ? 'animate__animated  animate__pulse emote-lg'
                : 'emote-md'
            "
          />
        </div>

        <div
          @click="onNextEmote"
          v-if="options.totalRecords > options.PageSize * (options.PageNo + 1)"
          class="p-0 pr-2 cursor-pointer format-center"
        >
          <i class="pi pi-angle-right" style="font-size: 2rem"></i>
        </div>
      </div>
    </OverlayPanel>
    <OverlayPanel
      class="p-0"
      ref="panelEmoij4"
      appendTo="body"
      :showCloseIcon="false"
      id="overlay_panelEmoij4"
    >
      <VuemojiPicker @emojiClick="handleEmojiClick" />
    </OverlayPanel>
    <Dialog
      :style="{ width: '30vw' }"
      header="Tất cả"
      v-model:visible="isShowEmotes"
    >
      <div v-for="(item, index) in listReaction" :key="index">
        <div class="flex w-full p-2 align-content-center align-items-center">
          <div class="relative">
            <Avatar
              v-bind:label="
                item.avatar
                  ? ''
                  : item.full_name.substring(
                      item.full_name.lastIndexOf(' ') + 1,
                      item.full_name.lastIndexOf(' ') + 2
                    )
              "
              :image="basedomainURL + item.avatar"
              :style="
                item.avatar
                  ? 'background-color: #2196f3'
                  : 'background:' + bgColor[item.full_name.length % 7]
              "
              class="w-3rem"
              size="large"
              shape="circle"
              @error="
                $event.target.src = basedomainURL + '/Portals/Image/nouser1.png'
              "
            >
            </Avatar>
            <div class="absolute right-0 bottom-0">
              <img
                :src="basedomainURL + item.emote_file"
                :alt="item.emote_file"
                width="15"
                height="15"
              />
            </div>
          </div>
          <Toolbar class="w-full custoolbar">
            <template #start>
              <div class="align-content-center pl-2 text-lg font-bold">
                {{ item.full_name }}
              </div>
            </template>
            <template #end>
              <div>
                {{ moment(item.created_date).format("HH:mm DD/MM/YYYY ") }}
              </div>
            </template>
          </Toolbar>
        </div>
      </div>
    </Dialog>
  </div>
</template>
<style scoped>
.box-comment {
  flex: 18;
}
.like-reply:hover {
  text-decoration: underline;
}
.emote-lg {
  width: 35px;
  height: 35px;
  padding-right: 0px;
  padding-left: 0px;
}
.emote-md {
  width: 30px;
  height: 30px;
}
.comp-law .data-comment {
  max-height: calc(100vh - 19rem);
  overflow-y: auto;
}
.sidebar-law .data-comment {
  max-height: calc(100vh - 12rem);
  overflow-y: auto;
}
</style>

<style lang="scss" scoped>
  ::v-deep(.p-menu) {
    .p-menu-list {
      display: flex !important;
    }
  }
  ::v-deep(.cmtContent) {
    p {
      margin-top: 0rem;
      margin-bottom: 0.5rem;
      line-height: 1.5;
      text-align: justify;
    }
    img {
      max-width: 100%;
      max-height: 150px !important;
      object-fit: contain;
    }
  }
  ::v-deep(.ql-bubble) {
    .ql-editor img {
      max-width: 100%;
      max-height: 150px !important;
      object-fit: contain;
    }
  }
  ::v-deep(.p-image) {
    img {
      max-width: 100%;
      max-height: 100px !important;
      object-fit: contain;
    }
  }
  ::v-deep(.comp-law) {
    .ql-bubble .ql-editor {
      padding-left: 0.5rem;
      padding-right: 0.5rem;
    }
  }
</style>

  