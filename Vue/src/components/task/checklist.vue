<script setup>
//Khai báo InJect và Import (import)
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { VuemojiPicker } from "vuemoji-picker";
import commentCheckList from "../news/comment.vue";
import vi from "date-fns/locale/vi";
import moment from "moment";
import { forEach } from "jszip";
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const emitter = inject("emitter");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
//Nơi nhận dữ liệu

emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "":
      break;
  }
});
const props = defineProps({
  isShow: Boolean,
  bug: Object,
  fromView: String,
  BugCommentID: Intl,
});

watch(props, () => {
  if (props.isShow == true) {
    showCommentBug(props.bug);
  }
});
//Khai báo biến (variable)

const dataCommentCheckList = ref();
const optionsCommentTask = ref({
  isShowInput: true,
  isUploadFile: true,
  isReply: true,
  isReaction: true,
});
const Bug = ref();
const bug = ref({
  bug_name: "",
  des: "",
  status: 0,
  keyword: "",
});
const basedomainURL = baseURL;
const toast = useToast();
const isFirst = ref(true);
const options = ref({
  IsNext: true,
  sort: "task_id",
  searchText: "",
  PageNo: 0,
  PageSize: 10,
  loading: true,
  totalRecords: null,
  finishedRecord: null,
  waitedRecord: null,
  tempClose: null,
  unFinishRecord: null,
  statusTask: null,
  outOfDate: null,
  SearchTextUser: "",
  Start_date: null,
  End_date: null,
});
const justifyOptions = ref([
  { icon: "pi pi-align-justify", value: false },
  { icon: "pi pi-list", value: true },
]);
const isSaveBug = ref(false);
const arrFiles = ref([]);
const activeIndexArc = ref();
const listBugCmtUn = ref([]);
const listBugsCmtCompleted = ref([]);
const isThumnFiles = ref(false);
const listThumFiles = ref();
const bugInComment = ref({
  bug_id: null,
  des: null,
  group_name: null,
  url_file: null,
  status: 0,
  start_date: new Date(),
  overtime:true
});
const isShowAddMore = ref(false);
const cmtBugId = ref();
const checkEditComment = ref(false);
const checkEditCommentChild = ref(false);
const checkFileComment = ref(false);
const listGroupBugComment = ref([]);
const listDropdownGroupBug = ref([]);
const comment = ref("");
const commentChild = ref("");
const replyCmt = ref();
const ckReplyCmt = ref(true);
const listCommentBugSave = ref();
const listCommentBugs = ref();
const panelEmoij4 = ref();
const panelEmoij3 = ref();
const comment_zone_child = ref();
const bugComment = ref();
const comment_zone_main = ref();
let filecoments = [];
const listFileComment = ref([]);
const typeBugComment = ref(false);
const checkPreCmtBug = ref(false);
const checkAddChildCmt = ref();
const checkAddBugComment = ref(false);
const isShowAddBugComment = ref(false);
const grpNameEd = ref();
const grpNameEdSend = ref();
const grpNameEdArr = ref([]);
const listImgPast = ref([]);
const valImg = ref([]);
let filebugcmt = [];
const comment_zone = ref();
const opBugs = ref();
const itemsAddMore = ref([
  {
    label: "Thêm nhiều",
    icon: "pi pi-plus",
    command: () => {
      addBugCommentMore();
    },
  },
]);
const listStatusBugComment = ref([
  {
    name: "Lỗi",
    code: 0,
    css: "p-button-raised p-button-danger ",
  },
  {
    name: "Đang xử lý",
    code: 1,
    css: "p-button-raised ",
  },
  {
    name: "Chuyển Test",
    code: 2,
    css: "p-button-raised p-button-secondary ",
  },
  {
    name: "Đã xử lý",
    code: 3,
    css: "p-button-raised p-button-success ",
  },
]);

const listStatusBugs = ref([
  {
    name: "Đề xuất",
    code: -4,
    css: "p-button-raised p-button-secondary",
  },
  {
    name: "Yêu cầu thêm",
    code: -3,
    css: "p-button-raised",
  },
  {
    name: "Lỗi",
    code: -2,
    css: "p-button-raised p-button-danger",
  },

  {
    name: "Đã sửa",
    code: 1,
    css: "p-button-raised p-button-success",
  },
  {
    name: "Đang sửa",
    code: -1,
    css: "p-button-raised ",
  },
  {
    name: "Đã đóng",
    code: 2,
    css: "p-button-raised p-button-warning",
  },
]);
const opbugcomment = ref();
const paste_zone = ref();
const bugCommentSave = ref();
const listCheckList = ref([]);
const groupNameSave = ref("");

//Hàm (Function)

///Thêm nhiều checklist

const addTempBug = () => {
  if (bugInComment.value.des == "" || bugInComment.value.des == null) return;
  else {
    listCheckList.value.push({
      value: bugInComment.value,
      fileUpload: filebugcmt,
      filesPaste: listImgPast.value,
      arrFiles: arrFiles.value,
    });
    filebugcmt = [];
    bugInComment.value = {
      bug_id: Bug.value.bug_id,
      des: "",
      group_name: null,
      url_file: null,
      status: 0,
    };
    arrFiles.value = [];
    listImgPast.value = [];
  }
};

const saveBug = () => {
  let statusUpdate = {
    IntID: Bug.value.bug_id,
    TextID: Bug.value.bug_id + "",
    IntTrangthai: Bug.value.status,
  };
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  axios
    .put(baseURL + "/api/api_bug/Update_TrangthaiBug", statusUpdate, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật lỗi thành công!");
        preListBug(Bug.value, props.fromView);
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
const addBugCommentMore = () => {
  bugInComment.value = {
    bug_id: Bug.value.bug_id,
    des: "",
    group_name: null,
    url_file: null,
    status: 0,
    start_date: new Date(),
  };
  groupNameSave.value = "";
  listCheckList.value = [];
  isShowAddMore.value = true;
};
const onChangeStatusBug = (status) => {
  opbugcomment.value.hide();
  bugCommentSave.value.status = status.code;
  let data = {
    IntID: bugCommentSave.value.bugcomment_id,
    TextID: bugCommentSave.value.bugcomment_id + "",
    IntTrangthai: status.code,
    BitTrangthai: false,
  };
  axios
    .put(baseURL + "/api/task_bugcomment/Update_StatusBugcomment", data, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật lỗi thành công!");

        if (!typeBugComment.value) showCommentBug(Bug.value);
        else onChangeViewBug();
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
};

const toggleStatusBugComment = (event, item) => {
  listStatusBugComment.value = [];
  if (store.getters.user.user_id == item.created_by) {
    listStatusBugComment.value = [
      {
        name: "Lỗi",
        code: 0,
        css: "p-button-raised p-button-danger ",
      },
      {
        name: "Đang xử lý",
        code: 1,
        css: "p-button-raised ",
      },
      {
        name: "Chuyển Test",
        code: 2,
        css: "p-button-raised p-button-secondary ",
      },
      {
        name: "Đã xử lý",
        code: 3,
        css: "p-button-raised p-button-success ",
      },
      {
        name: "Làm lại",
        code: 4,
        css: "p-button-raised p-button-warning ",
      },
    ];
  } else if (
    item.status == 0 &&
    (store.getters.user.user_id == item.user_id ||
      item.partner_id.includes(store.getters.user.user_id))
  ) {
    listStatusBugComment.value = [
      {
        name: "Đang xử lý",
        code: 1,
        css: "p-button-raised ",
      },
      {
        name: "Chuyển Test",
        code: 2,
        css: "p-button-raised p-button-secondary ",
      },
    ];
  } else if (
    item.status != 0 &&
    (store.getters.user.user_id == item.check_by || item.check_by == null)
  ) {
    listStatusBugComment.value = [
      {
        name: "Đang xử lý",
        code: 1,
        css: "p-button-raised ",
      },
      {
        name: "Chuyển Test",
        code: 2,
        css: "p-button-raised p-button-secondary ",
      },
    ];
  } else {
    return;
  }

  opbugcomment.value.toggle(event);
  bugCommentSave.value = item;
};
const saveBugCommentMore = () => {
  try {
    listCheckList.value.forEach((element) => {
      let formData = new FormData();
      for (var i = 0; i < element.fileUpload.length; i++) {
        let file = element.fileUpload[i];
        formData.append("url_file", file);
      }

      let strImg = "";
      let detached = "";
      if (element.filesPaste.length > 0)
        element.filesPaste.forEach((element12) => {
          strImg += detached + element12.substring(22);
          detached = ",";
        });
      if (strImg != "") element.value.strImg = strImg;
      else element.value.strImg = null;
      element.value.group_name = groupNameSave.value;
      element.value.status = bugInComment.value.status;
      if (element.value.group_name == null || element.value.group_name == "")
        element.value.group_name = "Khác";
      formData.append("bugcomment", JSON.stringify(element.value));

      filebugcmt = [];
      valImg.value = null;
      listImgPast.value = [];
      axios
        .post(baseURL + "/api/task_bugcomment/Add_bugcomment", formData, config)
        .then((response) => {
          if (response.data.err != "1") {
            swal.close();
            showCommentBug(Bug.value, true);
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
          console.log(error);
          swal.fire({
            title: "Error!",
            text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
            icon: "error",
            confirmButtonText: "OK",
          });
        });
    });
    toast.success("Thêm lỗi thành công!");

    closeBugComment();
  } catch (error) {
    console.log(error);
  }
};
const saveBugComment = () => {
  if (checkAddBugComment.value) {
    let formData = new FormData();
    for (var i = 0; i < filebugcmt.length; i++) {
      let file = filebugcmt[i];
      formData.append("url_file", file);
    }
    listFileComment.value = [];
    let strImg = "";
    let detached = "";
    if (listImgPast.value.length > 0)
      listImgPast.value.forEach((element) => {
        strImg += detached + element.substring(22);
        detached = ",";
      });
    if (strImg != "") bugInComment.value.strImg = strImg;
    else bugInComment.value.strImg = null;
    if (
      bugInComment.value.group_name == null ||
      bugInComment.value.group_name == ""
    )
      bugInComment.value.group_name = "Khác";
    formData.append("bugcomment", JSON.stringify(bugInComment.value));

    filebugcmt = [];
    valImg.value = null;
    listImgPast.value = [];
    axios
      .post(baseURL + "/api/task_bugcomment/Add_bugcomment", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm lỗi thành công!");

          showCommentBug(Bug.value, true);

          closeBugComment();
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
        console.log(error);
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    editBugCommentSave(bugInComment.value);
    closeBugComment();
  }
};

const onEditStatusBug = (value) => {
  Bug.value.status = value.code;

  saveBug();
};
const editBugWithGroupname = (grp) => {
  grpNameEdArr.value.push(grp.group_name);
  grpNameEd.value = grp.group_name;
  grpNameEdSend.value = grp.group_name;
};
const reDataBugcmt = () => {
  arrFiles.value = [];
  filebugcmt = [];
  valImg.value = null;
  listImgPast.value = [];
};

const isNumberKey = (e) => {
  e.preventDefault();
};

const onPasteImg = (event) => {
  valImg.value.ops.forEach((element) => {
    if (element.insert)
      if (
        element.insert.image &&
        !listImgPast.value.includes(element.insert.image)
      ) {
        listImgPast.value.push(element.insert.image);
      }
  });
  paste_zone.value.setHTML("");
};

const saveBugcmtGroupname = () => {
  grpNameEdArr.value.push(grpNameEdSend.value);
  grpNameEdArr.value.push(Bug.value.bug_id);

  axios
    .put(
      baseURL + "/api/task_bugcomment/Update_GroupName",
      grpNameEdArr.value,
      config
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật lỗi thành công!");

        if (!typeBugComment.value) showCommentBug(Bug.value);
        else onChangeViewBug();
        grpNameEdArr.value = [];
        grpNameEd.value = null;
        grpNameEdSend.value = null;
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
};
const cancelBugcmtGroupname = () => {
  grpNameEdArr.value = [];
  grpNameEd.value = null;
  grpNameEdSend.value = null;
};
const preListBug = (value, fromView) => {
  if (checkPreCmtBug.value == true) isShowBug.value = false;
  else {
    if (fromView == "detail") {
      emitter.emit("emitData", { type: "preListDetail", data: value });
    } else if (fromView == "bug") {
      emitter.emit("emitData", { type: "preListBug", data: value });
    }
  }
};

const hideNewAction = (id, stt, gt) => {
  let arr = [];
  arr.push(id);
  axios
    .put(
      baseURL +
        `/api/${stt ? "task_main" : "api_bug"}${
          gt ? "/Update_IsViewTest" : "/Update_IsViewWork"
        }`,
      arr,
      config
    )
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
const onChangeViewBug = () => {
  if (!typeBugComment.value) {
    showCommentBug(Bug.value);
  } else {
    axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "task_bugcomment_list",
          par: [
            { par: "bug_id", va: Bug.value.bug_id },
            { par: "search", va: null },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        let arrU = [];
        let arrC = [];
        data.forEach((element) => {
          if (element.url_file) {
            element.url_file = element.url_file.split(",");
            let arrFile = [];
            let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
            element.url_file.forEach((element) => {
              // Kiểm tra định dạng
              if (allowedExtensions.exec(element)) {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: true,
                  allsrc: element,
                });
                URL.revokeObjectURL(element);
              } else {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: false,
                  allsrc: element,
                });
              }
            });
            element.url_file = arrFile;
          }
          if (element.created_date)
            element.created_date = new Date(
              moment(element.created_date).format("YYYY/MM/DD HH:mm:ss")
            );
          else
            element.created_date = new Date(
              moment(new Date()).format("YYYY/MM/DD HH:mm:ss")
            );
          let arrPn = [];
          if (element.partner_id != null && element.partner_id.length > 1) {
            if (!Array.isArray(element.partner_id)) {
              arrPn = element.partner_id.split(",");
            }
          }
          element.partner_id = arrPn.filter((x) => x.length > 0);

          if (element.status == 3) arrC.push(element);
          else arrU.push(element);
        });
        listBugsCmtCompleted.value = arrC;
        listBugCmtUn.value = arrU;
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
  }
};
const renderComment = (listComment) => {
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
  listCommentBugs.value = arrChils;
};
const onCancelRepCmtB = () => {
  replyCmt.value = null;
  ckReplyCmt.value = true;
  cmtBugId.value = null;
};
const onSendCmtBug = (item) => {
  replyCmt.value = item.des;
  ckReplyCmt.value = false;
  cmtBugId.value = item.bugcomment_id;
};

const reloadCommentBug = () => {
  listCommentBugSave.value = null;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_commentbug_list",
        par: [
          { par: "bug_id", va: Bug.value.bug_id },
          { par: "commentbug_id", va: null },
          { par: "user_id", va: store.getters.user.user_id },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element) => {
        if (element.created_date)
          element.created_date = new Date(
            moment(element.created_date).format("YYYY/MM/DD HH:mm:ss")
          );
        else
          element.created_date = new Date(
            moment(new Date()).format("YYYY/MM/DD HH:mm:ss")
          );
      });
      listCommentBugSave.value = data;
     
    })
    .catch((error) => {
      console.log(error);
       
      options.value.loading = false;
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

const showEmoji = (event, check) => {
  if (check == 3) panelEmoij3.value.toggle(event);
  if (check == 4) panelEmoij4.value.toggle(event);
};
const chonanh = (id) => {
  document.getElementById(id).click();
};
const handleEmojiClick = (event) => {
  if (comment.value) comment.value = comment.value + event.unicode;
  else comment.value = event.unicode;
  comment.value = comment.value.replace("<p>", "").replace("</p>", "");
  comment_zone_main.value.setHTML("<p>" + comment.value + "</p>");
};
const handleEmojiClickChild = (event) => {
  if (commentChild.value)
    commentChild.value = commentChild.value + event.unicode;
  else commentChild.value = event.unicode;
  commentChild.value = commentChild.value
    .replace("<p>", "")
    .replace("</p>", "");
  comment_zone_child.value.setHTML("<p>" + comment.value + "</p>");
};

const toggleStatusBugs = () => {
  opBugs.value.toggle(event);
};
const addCommentBug = (check, comment_id) => {
  if (check) {
    if (
      comment.value == "" ||
      comment.value == null ||
      comment.value == "<p><br></p>"
    )
      return;
    else {
      let bugComment = {
        bug_id: Bug.value.bug_id,
        des: "<body>" + comment.value + "</body>",
      };
      if (cmtBugId.value != null) {
        bugComment.bugcomment_id = cmtBugId.value;
        cmtBugId.value = null;
      }

      let formData = new FormData();
      for (var i = 0; i < filecoments.length; i++) {
        let file = filecoments[i];
        formData.append("url_file", file);
      }
      filecoments = [];
      listFileComment.value = [];
      formData.append("comment", JSON.stringify(bugComment));
      comment.value = "";
      axios
        .post(baseURL + "/api/api_commentbug/Add_commentbug", formData, config)
        .then((response) => {
          if (response.data.err != "1") {
            swal.close();
            toast.success("Thêm bình luận thành công!");

            reloadCommentBug();
            comment.value = "";
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
  } else if (commentChild.value == "" || commentChild.value == null) return;
  else {
    let bugComment = {
      bug_id: Bug.value.bug_id,
      parent_id: comment_id,
      des: commentChild.value,
    };
    commentChild.value = "";
    let formData = new FormData();
    for (let i = 0; i < filecoments.length; i++) {
      let file = filecoments[i];
      formData.append("url_file", file);
    }
    filecoments = [];
    listFileCommentChild.value = [];
    formData.append("comment", JSON.stringify(bugComment));
    commentChild.value = "";

    axios
      .post(baseURL + "/api/api_commentbug/Add_commentbug", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm bình luận thành công!");

          reloadChildCommentBug(comment_id);
          commentChild.value = "";
          comment_zone_child.value[0].setHTML("");
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
  ckReplyCmt.value = true;
};

const reloadChildCommentBug = (comment_id) => {
  (async () => {
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_commentbug_list",
          par: [
            { par: "bug_id", va: Bug.value.bug_id },
            { par: "commentbug_id", va: null },
            { par: "user_id", va: store.getters.user.user_id },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        data.forEach((element) => {
          if (element.url_file) {
            element.url_file = element.url_file.split(",");
            let arrFile = [];
            let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
            element.url_file.forEach((element) => {
              //Kiểm tra định dạng
              if (allowedExtensions.exec(element)) {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: true,
                });
                URL.revokeObjectURL(element);
              } else {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: false,
                });
              }
            });
            element.url_file = arrFile;
          }
          if (element.created_date)
            element.created_date = new Date(
              moment(element.created_date).format("YYYY/MM/DD HH:mm:ss")
            );
          else
            element.created_date = new Date(
              moment(new Date()).format("YYYY/MM/DD HH:mm:ss")
            );
        });
        listCommentBugSave.value = data;
      });
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_commentbug_listwithid",
          par: [
            { par: "bug_id", va: Bug.value.bug_id },
            { par: "parent_id", va: comment_id },
            { par: "commentbug_id", va: null },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        data.forEach((element) => {
          if (element.url_file) {
            element.url_file = element.url_file.split(",");
            let arrFile = [];
            let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
            element.url_file.forEach((element) => {
              //Kiểm tra định dạng
              if (allowedExtensions.exec(element)) {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: true,
                });
                URL.revokeObjectURL(element);
              } else {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: false,
                });
              }
            });
            element.url_file = arrFile;
          }
          if (element.created_date)
            element.created_date = new Date(
              moment(element.created_date).format("YYYY/MM/DD HH:mm:ss")
            );
          else
            element.created_date = new Date(
              moment(new Date()).format("YYYY/MM/DD HH:mm:ss")
            );

          listCommentBugSave.value.push(element);
        });
        renderComment(listCommentBugSave.value);
        checkAddChildCmt.value = comment_id;
        //Cần làm cái Render Child ở đây
      })
      .catch((error) => {
        console.log(error);
         
        options.value.loading = false;
        console.log(error);
        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  })();
};
const delImgCommentBug = (value) => {
  let arrImg = [];
  for (let index = 0; index < filecoments.length; index++) {
    const element = filecoments[index];
    if (element != value) {
      arrImg.push(element);
    }
  }
  filecoments = arrImg;
  listFileComment.value = listFileComment.value.filter((x) => x.data != value);
};

const delEditFileCommentBug = (url) => {
  bugComment.value.url_file = bugComment.value.url_file.filter((x) => x != url);
};

const showAddChildCmtBug = (value) => {
  commentChild.value = "";
  (async () => {
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_commentbug_list",
          par: [
            { par: "bug_id", va: Bug.value.bug_id },
            { par: "commentbug_id", va: null },
            { par: "user_id", va: store.getters.user.user_id },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        data.forEach((element) => {
          if (element.url_file) {
            element.url_file = element.url_file.split(",");
            let arrFile = [];
            let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
            element.url_file.forEach((element) => {
              //Kiểm tra định dạng
              if (allowedExtensions.exec(element)) {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: true,
                });
                URL.revokeObjectURL(element);
              } else {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: false,
                });
              }
            });
            element.url_file = arrFile;
          }
          if (element.created_date)
            element.created_date = new Date(
              moment(element.created_date).format("YYYY/MM/DD HH:mm:ss")
            );
          else
            element.created_date = new Date(
              moment(new Date()).format("YYYY/MM/DD HH:mm:ss")
            );
        });
        listCommentBugSave.value = data;
      });
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_commentbug_listwithid",
          par: [
            { par: "bug_id", va: value.bug_id },
            { par: "parent_id", va: value.comment_id },
            { par: "bugcomment_id", va: null },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        data.forEach((element) => {
          if (element.url_file) {
            element.url_file = element.url_file.split(",");
            let arrFile = [];
            let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
            element.url_file.forEach((element) => {
              //Kiểm tra định dạng
              if (allowedExtensions.exec(element)) {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: true,
                });
                URL.revokeObjectURL(element);
              } else {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: false,
                });
              }
            });
            element.url_file = arrFile;
          }
          if (element.created_date)
            element.created_date = new Date(
              moment(element.created_date).format("YYYY/MM/DD HH:mm:ss")
            );
          else
            element.created_date = new Date(
              moment(new Date()).format("YYYY/MM/DD HH:mm:ss")
            );

          listCommentBugSave.value.push(element);
        });
        renderComment(listCommentBugSave.value);

        checkAddChildCmt.value = value.comment_id;
      })
      .catch((error) => {
        console.log(error);
         
        options.value.loading = false;
        console.log(error);
        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  })();
};
const saveEditCommentBug = (item, check) => {
  let url_files = "";
  let detached = "";
  if (Array.isArray(bugComment.value.url_file))
    bugComment.value.url_file.forEach((element) => {
      url_files += detached + "/Portals/CommentBug/" + element.data;
      detached = ",";
    });
  bugComment.value.url_file = url_files;
  checkEditComment.value = null;
  checkEditCommentChild.value = null;
  let formData = new FormData();
  formData.append("comment", JSON.stringify(bugComment.value));
  axios
    .put(baseURL + "/api/api_commentbug/Update_commentbug", formData, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Sửa bình luận thành công!");

        if (check) reloadCommentBug();
        else reloadChildCommentBug(bugComment.value.parent_id);
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
const editCommentBug = (value, check) => {
  bugComment.value = value;
  if (check) checkEditComment.value = value.comment_id;
  else checkEditCommentChild.value = value.comment_id;
};

const deleteCommentBug = (value, check) => {
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
          .delete(baseURL + "/api/api_commentbug/Delete_commentbug", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: value.comment_id != null ? [value.comment_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá bình luận thành công!");
              if (check) reloadCommentBug();
              else reloadChildCommentBug(value.parent_id);
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

const closeBugComment = () => {
  bugInComment.value = {
    des: "",
    group_name: null,
    url_file: "",
    status: 0,
  };
  isShowAddMore.value = false;
  isShowAddBugComment.value = false;
};
const addBugWithGroupname = (grp, index) => {
  bugInComment.value = {
    bug_id: Bug.value.bug_id,
    des: "",
    group_name: grp,
    url_file: null,
    status: 0,
  };

  checkAddBugComment.value = true;
  isShowAddBugComment.value = true;
  activeIndexArc.value = index + 1;
};
const deleteBugComment = (value) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá Lỗi này không!",
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
          .delete(baseURL + "/api/task_bugcomment/Delete_Bugcomment", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: value.bugcomment_id != null ? [value.bugcomment_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá lỗi thành công!");

              showCommentBug(Bug.value);

              listGroupBugComment.value
                .filter((x) => x.group_name == value.group_name)
                .forEach((item) => {
                  item.active = false;
                  item.icon = "p-accordion-toggle-icon pi pi-chevron-down";
                });
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
const editBugComment = (value) => {
  checkAddBugComment.value = false;
  bugInComment.value = value;
  isShowAddBugComment.value = true;
};
const editBugCommentSave = (value) => {
  let str = "";
  let detached = "";

  if (Array.isArray(value.url_file)) {
    value.url_file.forEach((element) => {
      str += detached + element.allsrc;
      detached = ",";
    });
  }
  if (str == "") {
    value.url_file = null;
  } else value.url_file = str;

  let formData = new FormData();
  for (var i = 0; i < filebugcmt.length; i++) {
    let file = filebugcmt[i];
    formData.append("url_file", file);
  }
  listFileComment.value = [];
  let strImg = "";
  detached = "";
  listImgPast.value.forEach((element) => {
    strImg += detached + element.substring(22);
    detached = ",";
  });
  value.strImg = strImg;
  formData.append("bugcomment", JSON.stringify(value));
  filebugcmt = [];
  listImgPast.value = [];
  valImg.value = null;
  axios
    .put(baseURL + "/api/task_bugcomment/Update_Bugcomment", formData, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật lỗi thành công!");
        showCommentBug(Bug.value);
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
const showCommentBug = (value) => {
  dataCommentCheckList.value = {
    bug_id: value.bug_id,
    des: null,
    user_id: store.getters.user.user_id,
  };
  Bug.value = value;
  activeIndexArc.value = null;
  typeBugComment.value = false;
  let arrActive = [];
  if (listGroupBugComment.value.length > 0) {
    listGroupBugComment.value.forEach((element) => {
      if (element.group_name.active == false)
        arrActive.push(element.group_name.group_name);
    });
  }
  listGroupBugComment.value = [];
  listDropdownGroupBug.value = [];

  if (Bug.value.keyword)
    if (!Array.isArray(Bug.value.keyword))
      Bug.value.keyword = Bug.value.keyword.split(",");
  if (Bug.value.url_file)
    if (!Array.isArray(Bug.value.url_file))
      Bug.value.url_file = Bug.value.url_file.split(",");
  (async () => {
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "task_bugcomment_list",
          par: [
            { par: "bug_id", va: Bug.value.bug_id },
            { par: "search", va: null },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        let arrU = [];
        let arrC = [];
        data.forEach((element) => {
          if (element.url_file) {
            element.url_file = element.url_file.split(",");
            let arrFile = [];
            let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
            element.url_file.forEach((element) => {
              // Kiểm tra định dạng
              if (allowedExtensions.exec(element)) {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: true,
                  allsrc: element,
                });
                URL.revokeObjectURL(element);
              } else {
                arrFile.push({
                  data: element.substring(20),
                  src: baseURL + element,
                  checkimg: false,
                  allsrc: element,
                });
              }
            });
            element.url_file = arrFile;
          }
          if (element.created_date)
            element.created_date = new Date(
              moment(element.created_date).format("YYYY/MM/DD HH:mm:ss")
            );
          else
            element.created_date = new Date(
              moment(new Date()).format("YYYY/MM/DD HH:mm:ss")
            );
          let arrPn = [];
          if (element.partner_id != null && element.partner_id.length > 1) {
            if (!Array.isArray(element.partner_id)) {
              arrPn = element.partner_id.split(",");
            }
          }
          element.partner_id = arrPn.filter((x) => x.length > 0);
          if (element.status == 3) arrC.push(element);
          else arrU.push(element);
        });
        listBugsCmtCompleted.value = arrC;
        listBugCmtUn.value = arrU;
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

    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "task_groupname_list",
          par: [
            { par: "bug_id", va: Bug.value.bug_id },
            { par: "search", va: null },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        listDropdownGroupBug.value = [];

        data.forEach((element) => {
          if (element.group_name == null || element.group_name == "")
            element.group_name = "Khác";
          if (element.active == null) element.active = true;
          if (arrActive.includes(element.group_name)) element.active = false;
          if (element.icon == null)
            element.icon = "p-accordion-toggle-icon pi pi-chevron-right";

          let check = false;
          listGroupBugComment.value.forEach((elemd) => {
            if (element.group_name == elemd.group_name.group_name) {
              check = true;
            }
          });
          if (!check) {
            listGroupBugComment.value.push({
              group_name: element,
              listBugsCmtCompleted: listBugsCmtCompleted.value.filter(
                (x) => x.group_name == element.group_name
              ),
              listBugCmtUn: listBugCmtUn.value.filter(
                (x) => x.group_name == element.group_name
              ),
            });
            listDropdownGroupBug.value.push({ name: element.group_name });
          }
        });
        if (props.BugCommentID){
          let obj = listBugsCmtCompleted.value.find(x => x.bugcomment_id == props.BugCommentID);
          if(obj){
            let index = data.findIndex(x => x.group_name == obj.group_name);
            if(index !== -1) showBugPanel(index);
          }
        }
      })
      .catch((error) => {
        console.log(error);
         
        options.value.loading = false;
        console.log(error);
        if (error && error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
  })();
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_commentbug_list",
        par: [
          { par: "bug_id", va: Bug.value.bug_id },
          { par: "commentbug_id", va: null },
          { par: "user_id", va: store.getters.user.user_id },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element) => {
        if (element.url_file) {
          element.url_file = element.url_file.split(",");
          let arrFile = [];
          let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép
          element.url_file.forEach((element) => {
            //Kiểm tra định dạng
            if (allowedExtensions.exec(element)) {
              arrFile.push({
                data: element.substring(20),
                src: baseURL + element,
                checkimg: true,
              });
              URL.revokeObjectURL(element);
            } else {
              arrFile.push({
                data: element.substring(20),
                src: baseURL + element,
                checkimg: false,
              });
            }
          });
          element.url_file = arrFile;
        }
        if (element.created_date)
          element.created_date = new Date(
            moment(element.created_date).format("YYYY/MM/DD HH:mm:ss")
          );
        else
          element.created_date = new Date(
            moment(new Date()).format("YYYY/MM/DD HH:mm:ss")
          );
      });

      listCommentBugSave.value = data;
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
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_commentbug_count",
        par: [
          { par: "bug_id", va: Bug.value.bug_id },
          { par: "bugcomment_id", va: null },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      commentCount.value = data[0].totalRecords;
    })
    .catch((error) => {
      console.log(error);
    });
  if (value.test_user.indexOf(store.getters.user.user_id) != -1)
    hideNewAction(value.bug_id, false, false);

  if (store.getters.user.user_id == value.work_user)
    hideNewAction(value.bug_id, false, true);
};
const commentCount = ref(0);
const checkFileCommentChild = ref(false);
const listFileCommentChild = ref([]);

const handleFileUploadComment = (event) => {
  listFileComment.value = [];
  filecoments = [];
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
const showBugPanel = (index) => {
  if (listGroupBugComment.value[index].group_name.active == false) {
    listGroupBugComment.value[index].group_name.active = true;
    listGroupBugComment.value[index].group_name.icon =
      "p-accordion-toggle-icon pi pi-chevron-right";
  } else {
    listGroupBugComment.value
      .filter((x) => x.active == false)
      .forEach((item) => {
        item.active = true;
        item.icon = "p-accordion-toggle-icon pi pi-chevron-right";
      });
    listGroupBugComment.value[index].group_name.active = false;
    listGroupBugComment.value[index].group_name.icon =
      "p-accordion-toggle-icon pi pi-chevron-down";
  }
};

const addBugComment = (value) => {
  bugInComment.value = {
    bug_id: value.bug_id,
    des: "",
    group_name: null,
    url_file: null,
    status: 0,
    start_date: new Date(),
    overtime:Bug.value.is_overtime==true?true:false
  };
 
  checkAddBugComment.value = true;
  isShowAddBugComment.value = true;
};

const onUploadFileBugComment = (event) => {
  filebugcmt = [];
  arrFiles.value = [];
  event.files.forEach((element) => {
    filebugcmt.push(element);
    let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép

    //Kiểm tra định dạng
    if (allowedExtensions.exec(element.name)) {
      arrFiles.value.push({
        data: element.name,
        src: URL.createObjectURL(element),
        checkimg: true,
      });
      URL.revokeObjectURL(element);
    } else {
      arrFiles.value.push({
        data: element.name,
        src: URL.createObjectURL(element),
        checkimg: false,
      });
    }
  });
  arrFiles.value.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
const removeFileBase = (element) => {
  listImgPast.value = listImgPast.value.filter((x) => x != element);
};
const removeFileBugComment = (item) => {
  arrFiles.value = arrFiles.value.filter((x) => x.data != item.data);
  filebugcmt = filebugcmt.filter((x) => x.name != item.data);
};
const deleImgBugcmt = (value) => {
  bugInComment.value.url_file = bugInComment.value.url_file.filter(
    (x) => x.data != value.data
  );
  if (bugInComment.value.url_file == "") bugInComment.value.url_file = null;
};

const showThumnFiles = (value) => {
  if (value.length > 1) {
    listThumFiles.value = value;
    isThumnFiles.value = true;
  }
};
onMounted(() => {
  return {};
});
</script>

<template>
  <div class="relative comment-height" v-if="props.isShow">
    <div
      class="surface-0 pb-2"
      style="position: -webkit-sticky; position: sticky; top: 0; z-index: 1000"
    >
      <div class="surface-0">
        <div
          style="
            justify-content: left;
            align-items: center;
            vertical-align: middle;
          "
          class="flex pb-3 relative grid"
        >
          <Button
            icon="pi
pi-arrow-left"
            class="h-2rem col-1"
            @click="preListBug(Bug, props.fromView)"
            v-if="fromView !== 'Noti'"
          ></Button>
          <h2 class="ml-2 my-0 col-8">{{ Bug.bug_name }}</h2>
          <Button
            v-if="store.getters.user.user_id == Bug.created_by"
            class="ml-3 w-2 col-3"
            :label="
              Bug.status == -2
                ? 'Lỗi'
                : Bug.status == -1
                ? 'Đang sửa'
                : Bug.status == 1
                ? 'Đã sửa'
                : Bug.status == 2
                ? 'Đã đóng'
                : 'Trạng thái'
            "
            @click="toggleStatusBugs($event)"
            aria:haspopup="true"
            aria-controls="overlay_panelS"
            :class="
              Bug.status == -2
                ? 'p-button-raised p-button-danger'
                : Bug.status == -1
                ? 'p-button-raised'
                : Bug.status == 1
                ? 'p-button-raised p-button-success'
                : Bug.status == 2
                ? 'p-button-raised p-button-warning'
                : 'p-button-raised'
            "
          />

          <OverlayPanel
            ref="opBugs"
            appendTo="body"
            :showCloseIcon="false"
            style="width: 250px"
            :breakpoints="{ '960px': '20vw' }"
          >
            <div>
              <div v-for="item in listStatusBugs" :key="item.code">
                <Button
                  :label="item.name"
                  :class="item.css"
                  class="w-full mb-1"
                  @click="onEditStatusBug(item)"
                />
              </div>
            </div>
          </OverlayPanel>
        </div>

        <div>
          Người tạo:
          <span class="font-semibold pr-1">{{ Bug.created_name }} </span>

          <!-- <timeago :datetime="Bug.created_date" :locale="vi" /> -->
        </div>

        <hr />
        <div class="pl-2" v-html="Bug.des"></div>
      </div>
    </div>

    <div>
      <div v-if="typeBugComment == false">
        <Toolbar class="mt-2">
          <template #start>
            <div class="format-center font-bold">
              Danh sách nhóm lỗi ({{
                listBugsCmtCompleted.length + listBugCmtUn.length
              }})
            </div>
          </template>
          <template #end>
            <SelectButton
              @change="onChangeViewBug"
              class="px-2"
              v-model="typeBugComment"
              :options="justifyOptions"
              optionValue="value"
              dataKey="value"
            >
              <template #option="slotProps">
                <i :class="slotProps.option.icon"></i>
              </template>
            </SelectButton>

            <!-- v-if="taskName.test_user_ids.filter((x)=>x==store.getters.user.user_id).length>0||store.getters.user.is_admin" -->

            <SplitButton
              @click="addBugComment(Bug)"
              label="Thêm lỗi"
              :model="itemsAddMore"
            ></SplitButton>
          </template>
        </Toolbar>
        <div class="grid w-full p-0 pt-2 m-0">
          <div class="col-12 p-0 relative">
            <Panel
              v-for="(elem, index) in listGroupBugComment"
              :key="index"
              v-model:collapsed="elem.group_name.active"
              class="p-0 cursor-pointer"
            >
              <template #header>
                <div
                  :style="
                    elem.listBugsCmtCompleted.length ==
                    elem.listBugsCmtCompleted.length + elem.listBugCmtUn.length
                      ? 'font-weight:600;color: #689F38; border-left:8px solid #689F38'
                      : ''
                  "
                  class="p-3 py-4 w-full flex"
                  v-if="grpNameEd != elem.group_name.group_name"
                  @click="showBugPanel(index)"
                >
                  <i
                    :class="elem.group_name.icon"
                    class="format-center text-secondary pr-2"
                  ></i>
                  <div style="line-height: 16px">
                    {{ elem.group_name.group_name }} ({{
                      elem.listBugsCmtCompleted.length
                    }}/{{
                      elem.listBugsCmtCompleted.length +
                      elem.listBugCmtUn.length
                    }})
                  </div>
                </div>
                <div v-else class="p-3 py-4 w-full flex">
                  <i
                    :class="elem.group_name.icon"
                    class="format-center text-secondary pr-2"
                  ></i>
                  <div class="format-center">
                    <InputText v-model="grpNameEdSend" />
                  </div>
                </div>
              </template>
              <template #icons>
                <div
                  class="relative flex"
                  v-if="grpNameEd != elem.group_name.group_name"
                >
                  <Button
                    v-if="Bug.created_by == store.getters.user.user_id"
                    class="py-1 mr-3"
                    icon="pi pi-plus"
                    @click="
                      addBugWithGroupname(elem.group_name.group_name, index)
                    "
                  ></Button>
                  <Button
                    v-if="Bug.created_by == store.getters.user.user_id"
                    class="py-1 mr-3"
                    icon="pi pi-pencil"
                    @click="editBugWithGroupname(elem.group_name)"
                  ></Button>
                </div>
                <div v-else class="flex">
                  <Button
                    v-if="Bug.created_by == store.getters.user.user_id"
                    class="py-1 mr-3"
                    icon="pi pi-check"
                    @click="saveBugcmtGroupname()"
                  ></Button>
                  <Button
                    v-if="Bug.created_by == store.getters.user.user_id"
                    class="py-1 mr-3"
                    icon="pi pi-times"
                    @click="cancelBugcmtGroupname()"
                  ></Button>
                </div>
              </template>

              <div class="grid" v-if="elem.listBugCmtUn.length > 0">
                <div class="col-12 flex" style="padding-top: 1rem !important">
                  <div class="col-6 format-center font-bold">Nội dung</div>
                  <div class="col-2 format-center font-bold">Xử lý</div>
                  <div class="col-2 format-center font-bold">Trạng thái</div>
                  <div class="col-1 format-center font-bold">File</div>
                  <div class="col-1 format-center font-bold"></div>
                </div>

                <div
                  class="col-12 flex"
                  style="padding-top: 8px !important"
                  v-for="(item, index) in elem.listBugCmtUn"
                  :key="index"
                >
                  <div
                    style="border-radius: 8px 0px 0px 8px"
                    class="col-6 surface-100 align-items-center flex text-left"
                    :style="
                      item.status == 0
                        ? 'border-left:8px solid red'
                        : item.status == 1
                        ? 'border-left:8px solid #2196F3'
                        : item.status == 2
                        ? 'border-left:8px solid #607D8B'
                        : item.status == 3
                        ? 'border-left:8px solid #689F38'
                        : 'border-left:8px solid #FBC02D'
                    "
                  >
                    {{ item.des }}
                  </div>
                  <div class="col-2 format-center surface-100">
                    <Avatar
                      v-tooltip.top="
                        'Người tạo: ' +
                        item.created_name +
                        '<br> Ngày:' +
                        moment(item.created_date).format('HH:mm DD/MM/YYYY')
                      "
                      style="border: 3px solid cyan"
                      :image="
                        item.created_avatar
                          ? basedomainURL + item.created_avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      class="mr-2"
                      size="large"
                      shape="circle"
                    />
                    <Avatar
                      v-tooltip.top="
                        'Người xử lý: ' +
                        item.check_name +
                        '<br> Ngày:' +
                        moment(item.check_date).format('HH:mm DD/MM/YYYY')
                      "
                      v-if="item.check_avatar"
                      :image="basedomainURL + item.check_avatar"
                      class="mr-2"
                      size="large"
                      shape="circle"
                    />
                  </div>
                  <div class="col-2">
                    <Button
                      class="w-full h-full p-0"
                      :label="
                        item.status == 0
                          ? 'Lỗi'
                          : item.status == 1
                          ? 'Đang làm'
                          : item.status == 2
                          ? 'Đang đợi Test'
                          : item.status == 3
                          ? 'Đã Test OK'
                          : 'Test chưa OK'
                      "
                      @click="toggleStatusBugComment($event, item)"
                      aria:haspopup="true"
                      aria-controls="overlay_panelS1"
                      :class="
                        item.status == 0
                          ? 'p-button-raised p-button-danger'
                          : item.status == 1
                          ? 'p-button-raised '
                          : item.status == 2
                          ? 'p-button-raised  p-button-secondary'
                          : item.status == 3
                          ? 'p-button-raised p-button-success'
                          : 'p-button-raised p-button-warning'
                      "
                    />
                  </div>
                  <div v-if="item.url_file" class="col-1 format-center">
                    <div v-if="item.url_file.length > 1">
                      <div
                        class="flex"
                        v-if="item.url_file"
                        @click="showThumnFiles(item.url_file)"
                      >
                        <img
                          class="cursor-pointer px-2"
                          v-if="item.url_file[0].checkimg"
                          :src="item.url_file[0].src"
                          :alt="item.url_file[0].data"
                          style="
                            object-fit: contain;
                            border: 1px solid #ccc;
                            width: 50px;
                            height: 50px;
                          "
                        />
                        <div v-else>
                          <img
                            v-if="item.url_file.lenth > 0"
                            class="cursor-pointer"
                            :src="
                              basedomainURL +
                              '/Portals/Image/file/' +
                              item.url_file[0].data.substring(
                                item.url_file[0].data.lastIndexOf('.') + 1
                              ) +
                              '.png'
                            "
                            style="
                              width: 50px;
                              height: 50px;
                              object-fit: contain;
                            "
                            :alt="item.url_file[0].data"
                          />
                          <div>
                            {{ item.url_file[0].data }}
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-1 format-center" v-else-if="item.url_file">
                      <Image
                        v-if="item.url_file[0].checkimg"
                        :src="item.url_file[0].src"
                        :alt="item.url_file[0].data"
                        width="36"
                        height="36"
                        style="
                          object-fit: contain;
                          border: 1px solid #ccc;
                          width: 36px;
                          height: 36px;
                        "
                        preview
                        @error="
                          $event.target.src =
                            basedomainURL + '/Portals/Image/noimg.jpg'
                        "
                      />
                      <div v-else>
                        <a
                          v-if="
                            item.url_file[0].src != null &&
                            item.url_file[0].src != ''
                          "
                          :href="item.url_file[0].src"
                          download
                          class="w-full no-underline"
                        >
                          <img
                            :src="
                              basedomainURL +
                              '/Portals/Image/file/' +
                              item.url_file[0].data.substring(
                                item.url_file[0].data.lastIndexOf('.') + 1
                              ) +
                              '.png'
                            "
                            style="
                              width: 36px;
                              height: 36px;
                              object-fit: contain;
                            "
                            :alt="item.url_file[0].data"
                          />
                          <div>
                            {{ item.url_file[0].data }}
                          </div></a
                        >
                      </div>
                    </div>
                  </div>
                  <div class="col-1 format-center" v-else></div>
                  <div class="col-1 p-0 pt-3 flex">
                    <Button
                      class="p-button-rounded p-button-secondary p-button-text"
                      @click="onSendCmtBug(item)"
                      type="button"
                      icon="pi pi-send"
                    ></Button>
                    <Button
                      v-if="item.created_by == store.getters.user.user_id"
                      class="p-button-rounded p-button-secondary p-button-text"
                      @click="editBugComment(item)"
                      type="button"
                      icon="pi pi-pencil"
                    ></Button>
                    <Button
                      v-if="item.created_by == store.getters.user.user_id"
                      class="p-button-rounded p-button-secondary p-button-text"
                      @click="deleteBugComment(item)"
                      type="button"
                      icon="pi pi-trash"
                    ></Button>
                  </div>
                </div>
              </div>
              <div v-if="elem.listBugsCmtCompleted.length > 0" class="grid">
                <div class="col-12 flex" style="padding-top: 1rem !important">
                  <div class="col-6 format-center font-bold">Nội dung</div>
                  <div class="col-2 format-center font-bold">Xử lý</div>
                  <div class="col-2 format-center font-bold">Trạng thái</div>
                  <div class="col-1 format-center font-bold">File</div>
                  <div class="col-1 format-center font-bold"></div>
                </div>
                <div
                  class="col-12 flex"
                  style="padding-top: 8px !important"
                  v-for="(item, index) in elem.listBugsCmtCompleted"
                  :key="index"
                >
                  <div
                    style="border-radius: 8px 0px 0px 8px"
                    class="col-6 surface-100 align-items-center flex text-left"
                    :style="
                      item.status == 0
                        ? 'border-left:8px solid red'
                        : item.status == 1
                        ? 'border-left:8px solid #2196F3'
                        : item.status == 2
                        ? 'border-left:8px solid #607D8B'
                        : item.status == 3
                        ? 'border-left:8px solid #689F38'
                        : 'border-left:8px solid #FBC02D'
                    "
                  >
                    {{ item.des }}
                  </div>
                  <div class="col-2 format-center surface-100">
                    <Avatar
                      v-tooltip.top="
                        'Người tạo: ' +
                        item.created_name +
                        '<br> Ngày:' +
                        moment(item.created_date).format('HH:mm DD/MM/YYYY')
                      "
                      style="border: 3px solid cyan"
                      :image="
                        item.created_avatar
                          ? basedomainURL + item.created_avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      class="mr-2"
                      size="large"
                      shape="circle"
                    />
                    <Avatar
                      v-tooltip.top="
                        'Người xử lý: ' +
                        item.check_name +
                        '<br> Ngày:' +
                        moment(item.check_date).format('HH:mm DD/MM/YYYY')
                      "
                      v-if="item.check_avatar"
                      :image="basedomainURL + item.check_avatar"
                      class="mr-2"
                      size="large"
                      shape="circle"
                    />
                  </div>
                  <div class="col-2">
                    <Button
                      class="w-full h-full p-0"
                      :label="
                        item.status == 0
                          ? 'Lỗi'
                          : item.status == 1
                          ? 'Đang làm'
                          : item.status == 2
                          ? 'Đang đợi Test'
                          : item.status == 3
                          ? 'Đã Test OK'
                          : 'Test chưa OK'
                      "
                      @click="toggleStatusBugComment($event, item)"
                      aria:haspopup="true"
                      aria-controls="overlay_panelS1"
                      :class="
                        item.status == 0
                          ? 'p-button-raised p-button-danger'
                          : item.status == 1
                          ? 'p-button-raised '
                          : item.status == 2
                          ? 'p-button-raised  p-button-secondary'
                          : item.status == 3
                          ? 'p-button-raised p-button-success'
                          : 'p-button-raised p-button-warning'
                      "
                    />
                  </div>
                  <div v-if="item.url_file" class="col-1 format-center">
                    <div v-if="item.url_file.length > 1">
                      <div
                        class="flex"
                        v-if="item.url_file"
                        @click="showThumnFiles(item.url_file)"
                      >
                        <img
                          class="cursor-pointer px-2"
                          v-if="item.url_file[0].checkimg"
                          :src="item.url_file[0].src"
                          :alt="item.url_file[0].data"
                          style="
                            object-fit: contain;
                            border: 1px solid #ccc;
                            width: 50px;
                            height: 50px;
                          "
                        />
                        <div v-else>
                          <img
                            class="cursor-pointer"
                            :src="
                              basedomainURL +
                              '/Portals/Image/file/' +
                              item.url_file[0].data.substring(
                                item.url_file[0].data.lastIndexOf('.') + 1
                              ) +
                              '.png'
                            "
                            style="
                              width: 50px;
                              height: 50px;
                              object-fit: contain;
                            "
                            :alt="item.url_file[0].data"
                          />
                          <div>
                            {{ item.url_file[0].data }}
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-1 format-center" v-else-if="item.url_file">
                      <Image
                        v-if="item.url_file[0].checkimg"
                        :src="item.url_file[0].src"
                        :alt="item.url_file[0].data"
                        width="50"
                        height="50"
                        style="
                          object-fit: contain;
                          border: 1px solid #ccc;
                          width: 50px;
                          height: 50px;
                        "
                        preview
                        @error="
                          $event.target.src =
                            basedomainURL + '/Portals/Image/noimg.jpg'
                        "
                      >
                      </Image>
                      <div v-else>
                        <a
                          v-if="
                            item.url_file[0].src != null &&
                            item.url_file[0].src != ''
                          "
                          :href="item.url_file[0].src"
                          download
                          class="w-full no-underline"
                        >
                          <img
                            :src="
                              basedomainURL +
                              '/Portals/Image/file/' +
                              item.url_file[0].data.substring(
                                item.url_file[0].data.lastIndexOf('.') + 1
                              ) +
                              '.png'
                            "
                            style="
                              width: 50px;
                              height: 50px;
                              object-fit: contain;
                            "
                            :alt="item.url_file[0].data"
                          />
                          <div>
                            {{ item.url_file[0].data }}
                          </div></a
                        >
                      </div>
                    </div>
                  </div>
                  <div class="col-1 format-center" v-else></div>
                  <div
                    class="col-1 flex"
                    v-if="Bug.created_by == store.getters.user.user_id"
                  >
                    <Button
                      class="p-button-rounded p-button-secondary p-button-text"
                      @click="onSendCmtBug(item)"
                      type="button"
                      icon="pi pi-send"
                    ></Button>
                    <Button
                      class="p-button-rounded p-button-secondary p-button-text"
                      @click="editBugComment(item)"
                      type="button"
                      icon="pi pi-pencil"
                    ></Button>
                    <Button
                      class="p-button-rounded p-button-secondary p-button-text"
                      @click="deleteBugComment(item)"
                      type="button"
                      icon="pi pi-trash"
                    ></Button>
                  </div>
                </div>
              </div>
            </Panel>
          </div>
        </div>
      </div>
      <div v-else>
        <Toolbar class="mt-2">
          <template #start>
            <div class="format-center font-bold">
              Danh sách lỗi ({{
                listBugsCmtCompleted.length + listBugCmtUn.length
              }})
            </div>
          </template>
          <template #end>
            <SelectButton
              @change="onChangeViewBug"
              class="px-2"
              v-model="typeBugComment"
              :options="justifyOptions"
              optionValue="value"
              dataKey="value"
            >
              <template #option="slotProps">
                <i :class="slotProps.option.icon"></i>
              </template>
            </SelectButton>
            <Button
              @click="addBugComment(Bug)"
              label="Thêm lỗi"
              icon="pi pi-plus"
            ></Button>
          </template>
        </Toolbar>
        <div class="grid w-full p-0 m-0">
          <div class="col-12 p-0">
            <div
              class="grid"
              v-if="listBugCmtUn.length > 0"
              style="padding-top: 16px !important"
            >
              <div class="col-12 flex" style="padding-top: 1rem !important">
                <div class="col-6 format-center font-bold">Nội dung</div>
                <div class="col-2 format-center font-bold">Xử lý</div>
                <div class="col-2 format-center font-bold">Trạng thái</div>
                <div class="col-1 format-center font-bold">File</div>
                <div class="col-1 format-center font-bold"></div>
              </div>
              <div
                class="col-12 flex"
                style="padding-top: 8px !important"
                v-for="(item, index) in listBugCmtUn"
                :key="index"
              >
                <div
                  style="border-radius: 8px 0px 0px 8px"
                  class="col-6 surface-100 align-items-center flex text-left"
                  :style="
                    item.status == 0
                      ? 'border-left:8px solid red'
                      : item.status == 1
                      ? 'border-left:8px solid #2196F3'
                      : item.status == 2
                      ? 'border-left:8px solid #607D8B'
                      : item.status == 3
                      ? 'border-left:8px solid #689F38'
                      : 'border-left:8px solid #FBC02D'
                  "
                >
                  {{ item.des }}
                </div>
                <div class="col-2 format-center surface-100">
                  <Avatar
                    v-tooltip.top="
                      'Người tạo: ' +
                      item.created_name +
                      '<br> Ngày:' +
                      moment(item.created_date).format('HH:mm DD/MM/YYYY')
                    "
                    style="border: 3px solid cyan"
                    :image="
                      item.created_avatar
                        ? basedomainURL + item.created_avatar
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    class="mr-2"
                    size="large"
                    shape="circle"
                  />
                  <Avatar
                    v-tooltip.top="
                      'Người xử lý: ' +
                      item.check_name +
                      '<br> Ngày:' +
                      moment(item.check_date).format('HH:mm DD/MM/YYYY')
                    "
                    v-if="item.check_avatar"
                    :image="basedomainURL + item.check_avatar"
                    class="mr-2"
                    size="large"
                    shape="circle"
                  />
                </div>
                <div class="col-2">
                  <Button
                    class="w-full h-full p-0"
                    :label="
                      item.status == 0
                        ? 'Lỗi'
                        : item.status == 1
                        ? 'Đang làm'
                        : item.status == 2
                        ? 'Đang đợi Test'
                        : item.status == 3
                        ? 'Đã Test OK'
                        : 'Test chưa OK'
                    "
                    @click="toggleStatusBugComment($event, item)"
                    aria:haspopup="true"
                    aria-controls="overlay_panelS1"
                    :class="
                      item.status == 0
                        ? 'p-button-raised p-button-danger'
                        : item.status == 1
                        ? 'p-button-raised '
                        : item.status == 2
                        ? 'p-button-raised  p-button-secondary'
                        : item.status == 3
                        ? 'p-button-raised p-button-success'
                        : 'p-button-raised p-button-warning'
                    "
                  />
                </div>
                <div v-if="item.url_file" class="col-1 format-center">
                  <div v-if="item.url_file.length > 1">
                    <div
                      class="flex"
                      v-if="item.url_file"
                      @click="showThumnFiles(item.url_file)"
                    >
                      <div v-if="item.url_file[0].checkimg">
                        <img
                          class="cursor-pointer px-2"
                          :src="item.url_file[0].src"
                          :alt="item.url_file[0].data"
                          style="
                            object-fit: contain;
                            border: 1px solid #ccc;
                            width: 50px;
                            height: 50px;
                          "
                        />
                      </div>
                      <div v-else>
                        <!-- <img
                            class="cursor-pointer"
                           
                            :src="
                                basedomainURL + '/Portals/Image/file/' +
                              item.url_file[0].data.substring(
                                item.url_file[0].data.lastIndexOf('.') + 1
                              ) +
                              '.png'
                            "
                            style="
                              width: 50px;
                              height: 50px;
                              object-fit: contain;
                            "
                            :alt="item.url_file[0].data"
                          /> -->
                      </div>
                    </div>
                  </div>
                  <div class="col-1 format-center" v-else-if="item.url_file">
                    <Image
                      v-if="item.url_file[0].checkimg"
                      :src="item.url_file[0].src"
                      :alt="item.url_file[0].data"
                      width="50"
                      height="50"
                      style="
                        object-fit: contain;
                        border: 1px solid #ccc;
                        width: 50px;
                        height: 50px;
                      "
                      preview
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                    />
                    <div v-else>
                      <a
                        v-if="
                          item.url_file[0].src != null &&
                          item.url_file[0].src != ''
                        "
                        :href="item.url_file[0].src"
                        download
                        class="w-full no-underline"
                      >
                        <img
                          :src="
                            basedomainURL +
                            '/Portals/Image/file/' +
                            item.url_file[0].data.substring(
                              item.url_file[0].data.lastIndexOf('.') + 1
                            ) +
                            '.png'
                          "
                          style="width: 50px; height: 50px; object-fit: contain"
                          :alt="item.url_file[0].data"
                        />
                        <div>
                          {{ item.url_file[0].data }}
                        </div></a
                      >
                    </div>
                  </div>
                </div>
                <div class="col-1 format-center" v-else></div>
                <div
                  class="col-1 flex"
                  v-if="Bug.created_by == store.getters.user.user_id"
                >
                  <Button
                    class="p-button-rounded p-button-secondary p-button-text"
                    @click="onSendCmtBug(item)"
                    type="button"
                    icon="pi pi-send"
                  ></Button>
                  <Button
                    class="p-button-rounded p-button-secondary p-button-text"
                    @click="editBugComment(item)"
                    type="button"
                    icon="pi pi-pencil"
                  ></Button>
                  <Button
                    class="p-button-rounded p-button-secondary p-button-text"
                    @click="deleteBugComment(item)"
                    type="button"
                    icon="pi pi-trash"
                  ></Button>
                </div>
              </div>
            </div>
            <div
              v-if="listBugsCmtCompleted.length > 0"
              class="grid"
              style="padding-top: 16px !important"
            >
              <div class="col-12 flex" style="padding-top: 1rem !important">
                <div class="col-6 format-center font-bold">Nội dung</div>
                <div class="col-2 format-center font-bold">Xử lý</div>
                <div class="col-2 format-center font-bold">Trạng thái</div>
                <div class="col-1 format-center font-bold">File</div>
                <div class="col-1 format-center font-bold"></div>
              </div>
              <div
                class="col-12 flex"
                style="padding-top: 8px !important"
                v-for="(item, index) in listBugsCmtCompleted"
                :key="index"
              >
                <div
                  style="border-radius: 8px 0px 0px 8px"
                  class="col-6 surface-100 align-items-center flex text-left"
                  :style="
                    item.status == 0
                      ? 'border-left:8px solid red'
                      : item.status == 1
                      ? 'border-left:8px solid #2196F3'
                      : item.status == 2
                      ? 'border-left:8px solid #607D8B'
                      : item.status == 3
                      ? 'border-left:8px solid #689F38'
                      : 'border-left:8px solid #FBC02D'
                  "
                >
                  {{ item.des }}
                </div>
                <div class="col-2 format-center surface-100">
                  <Avatar
                    v-tooltip.top="
                      'Người tạo: ' +
                      item.created_name +
                      '<br> Ngày:' +
                      moment(item.created_date).format('HH:mm DD/MM/YYYY')
                    "
                    style="border: 3px solid cyan"
                    :image="
                      item.created_avatar
                        ? basedomainURL + item.created_avatar
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    class="mr-2"
                    size="large"
                    shape="circle"
                  />
                  <Avatar
                    v-tooltip.top="
                      'Người xử lý: ' +
                      item.check_name +
                      '<br> Ngày:' +
                      moment(item.check_date).format('HH:mm DD/MM/YYYY')
                    "
                    v-if="item.check_avatar"
                    :image="basedomainURL + item.check_avatar"
                    class="mr-2"
                    size="large"
                    shape="circle"
                  />
                </div>
                <div class="col-2">
                  <Button
                    class="w-full h-full p-0"
                    :label="
                      item.status == 0
                        ? 'Lỗi'
                        : item.status == 1
                        ? 'Đang làm'
                        : item.status == 2
                        ? 'Đang đợi Test'
                        : item.status == 3
                        ? 'Đã Test OK'
                        : 'Test chưa OK'
                    "
                    @click="toggleStatusBugComment($event, item)"
                    aria:haspopup="true"
                    aria-controls="overlay_panelS1"
                    :class="
                      item.status == 0
                        ? 'p-button-raised p-button-danger'
                        : item.status == 1
                        ? 'p-button-raised '
                        : item.status == 2
                        ? 'p-button-raised  p-button-secondary'
                        : item.status == 3
                        ? 'p-button-raised p-button-success'
                        : 'p-button-raised p-button-warning'
                    "
                  />
                </div>
                <div v-if="item.url_file" class="col-1 format-center">
                  <div v-if="item.url_file.length > 1">
                    <div
                      class="flex"
                      v-if="item.url_file"
                      @click="showThumnFiles(item.url_file)"
                    >
                      <img
                        class="cursor-pointer px-2"
                        v-if="item.url_file[0].checkimg"
                        :src="item.url_file[0].src"
                        :alt="item.url_file[0].data"
                        style="
                          object-fit: contain;
                          border: 1px solid #ccc;
                          width: 50px;
                          height: 50px;
                        "
                      />
                      <div v-else>
                        <img
                          class="cursor-pointer"
                          :src="
                            basedomainURL +
                            '/Portals/Image/file/' +
                            item.url_file[0].data.substring(
                              item.url_file[0].data.lastIndexOf('.') + 1
                            ) +
                            '.png'
                          "
                          style="width: 50px; height: 50px; object-fit: contain"
                          :alt="item.url_file[0].data"
                        />
                        <div>
                          {{ item.url_file[0].data }}
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-1 format-center" v-else-if="item.url_file">
                    <Image
                      v-if="item.url_file[0].checkimg"
                      :src="item.url_file[0].src"
                      :alt="item.url_file[0].data"
                      width="50"
                      height="50"
                      style="
                        object-fit: contain;
                        border: 1px solid #ccc;
                        width: 50px;
                        height: 50px;
                      "
                      preview
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                    />
                    <div v-else>
                      <a
                        v-if="
                          item.url_file[0].src != null &&
                          item.url_file[0].src != ''
                        "
                        :href="item.url_file[0].src"
                        download
                        class="w-full no-underline"
                      >
                        <img
                          :src="
                            basedomainURL +
                            '/Portals/Image/file/' +
                            item.url_file[0].data.substring(
                              item.url_file[0].data.lastIndexOf('.') + 1
                            ) +
                            '.png'
                          "
                          style="width: 50px; height: 50px; object-fit: contain"
                          :alt="item.url_file[0].data"
                        />
                        <div>
                          {{ item.url_file[0].data }}
                        </div></a
                      >
                    </div>
                  </div>
                </div>
                <div class="col-1 format-center" v-else></div>
                <div
                  class="col-1 flex"
                  v-if="Bug.created_by == store.getters.user.user_id"
                >
                  <Button
                    class="p-button-rounded p-button-secondary p-button-text"
                    @click="onSendCmtBug(item)"
                    type="button"
                    icon="pi pi-send"
                  ></Button>
                  <Button
                    class="p-button-rounded p-button-secondary p-button-text"
                    @click="editBugComment(item)"
                    type="button"
                    icon="pi pi-pencil"
                  ></Button>
                  <Button
                    class="p-button-rounded p-button-secondary p-button-text"
                    @click="deleteBugComment(item)"
                    type="button"
                    icon="pi pi-trash"
                  ></Button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-10 p-0" v-if="Bug.url_file">
        <div class="flex">
          <Toolbar class="w-full py-3">
            <template #start>
              <div class="flex">
                <img
                  src="/src/assets/image/filess.png"
                  style="object-fit: contain"
                  width="50"
                  height="50"
                  alt="logorar"
                />
                <span style="line-height: 50px">
                  {{ Bug.url_file.substring(16) }}</span
                >
              </div>
            </template>
            <!-- <template #end>
                            <Button
                              icon="pi pi-times"
                              class="p-button-rounded p-button-danger"
                              @click="deleteFileBug()"
                            />
                          </template> -->
          </Toolbar>
        </div>
      </div>
      <div v-for="item in Bug.keyword" class="m-1">
        <Chip>
          {{ item }}
        </Chip>
      </div>
      <div class="col-12 field p-0 flex" v-if="listCommentBugSave != null">
        <commentCheckList
          :options="optionsCommentTask"
          :refreshData="reloadCommentBug"
          :objectData="dataCommentCheckList"
          :comment_count="commentCount"
          :dataComments="listCommentBugSave"
          :Controller="'api_commentbug'"
        />
      </div>
    </div>
  </div>
  <Dialog
    v-model:visible="isShowAddBugComment"
    :style="{ width: '40vw' }"
    header="Cập nhật lỗi"
    @hide="reDataBugcmt"
  >
   
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">Mô tả</label>
          <div class="col-10 p-0">
            <Textarea
              style="border-radius: 5px"
              class="w-full"
              placeholder="Mô tả lỗi"
              rows="4"
              cols="30"
              v-model="bugInComment.des"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 flex p-0">
            <label class="col-4 text-left p-0 pt-2">Nhóm lỗi</label>
            <Dropdown
              v-model="bugInComment.group_name"
              :options="listDropdownGroupBug"
              :editable="true"
              optionLabel="name"
              optionValue="name"
              placeholder="Chọn hoặc nhập nhóm lỗi"
              spellcheck="true"
              class="col-8 ip36 p-0"
            />
          </div>
          <div class="col-6 flex p-0">
            <label class="col-4 text-left p-0 pl-2 pt-2">Trạng thái</label>
            <Dropdown
              v-model="bugInComment.status"
              :options="listStatusBugComment"
              optionLabel="name"
              optionValue="code"
              placeholder="Chọn trạng thái"
              spellcheck="true"
              class="col-8 ip36 p-0"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex"   v-if="checkAddBugComment">
          <div class="col-6 flex p-0">
            <label class="col-4 text-left p-0 pt-2">Ngày bắt đầu</label>
            <Calendar
              class="col-8 p-0"
              :showIcon="true"
              id="time24"
              :showTime="true"
              autocomplete="on"
              v-model="bugInComment.start_date"
            />
          </div>
          <div class="col-6 flex p-0">
            <label class="col-4 text-left pl-2 p-0 pt-2">Ngày kết thúc</label>
            <Calendar
              class="col-8 p-0"
              :showIcon="true"
              id="time24"
              :showTime="true"
              autocomplete="on"
              v-model="bugInComment.end_date"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex" v-if="checkAddBugComment">
          <div class="col-6 flex p-0">
            <label class="col-4 text-left p-0 pt-2">Ngoài giờ</label>
            <InputSwitch v-model="bugInComment.overtime" />
          </div>

        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">File lỗi</label>
          <div class="col-10 p-0 m-0">
            <Panel>
              <template #header>
                <div class="p-3">
                  <FileUpload
                    chooseLabel="Chọn File"
                    :auto="true"
                    mode="basic"
                    :multiple="true"
                    :maxFileSize="1000000"
                    @select="onUploadFileBugComment"
                  >
                  </FileUpload>
                </div>
              </template>
              <QuillEditor
                @text-change="onPasteImg($event)"
                placeholder="Nơi paste ảnh..."
                ref="paste_zone"
                @keypress="isNumberKey($event)"
                class="w-full"
                contentType="delta"
                :content="comment"
                v-model:content="valImg"
                theme="bubble"
              />
            </Panel>
          </div>
        </div>

        <div
          class="col-12 p-0 flex field"
          v-for="(element1, index1) in listImgPast"
          :key="index1"
        >
          <div class="col-2 text-left"></div>
          <div class="col-2">
            <Image
              :src="element1"
              :alt="'Ảnh' + index1"
              width="70"
              height="50"
              style="
                object-fit: contain;
                border: 1px solid #ccc;
                width: 70px;
                height: 50px;
              "
              preview
              @error="
                $event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'
              "
            />
          </div>
          <div
            class="col-7 flex"
            style="align-items: center; vertical-align: middle"
          >
            Ảnh {{ index1 + 1 }}
          </div>
          <div class="col-1">
            <Button
              @click="removeFileBase(element1)"
              icon="pi pi-times"
            ></Button>
          </div>
        </div>
        <div
          class="col-12 p-0 flex field"
          v-for="(element1, index1) in arrFiles"
          :key="index1"
        >
          <div class="col-2 text-left"></div>
          <div class="col-2">
            <Image
              v-if="element1.checkimg"
              :src="element1.src"
              :alt="element1.data"
              width="70"
              height="50"
              style="
                object-fit: contain;
                border: 1px solid #ccc;
                width: 70px;
                height: 50px;
              "
              preview
              @error="
                $event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'
              "
            />
            <div v-else>
              <a :href="element1.src" download class="w-full no-underline">
                <img
                  :src="
                    basedomainURL +
                    '/Portals/Image/file/' +
                    element1.data.substring(
                      element1.data.lastIndexOf('.') + 1
                    ) +
                    '.png'
                  "
                  style="width: 70px; height: 50px; object-fit: contain"
                  :alt="element1.data"
                />
              </a>
            </div>
          </div>
          <div
            class="col-7 flex"
            style="align-items: center; vertical-align: middle"
          >
            {{ element1.data }}
          </div>
          <div class="col-1">
            <Button
              @click="removeFileBugComment(element1)"
              icon="pi pi-times"
            ></Button>
          </div>
        </div>
        <div class="col-12 p-0 flex field">
          <label class="col-2 text-left"></label>

          <div class="col-10 p-0" v-if="bugInComment.url_file">
            <div style="display: grid; grid-template-columns: repeat(5, 1fr)">
              <div
                v-for="(element1, index1) in bugInComment.url_file"
                :key="index1"
                class="m-1 relative"
                style="border: 3px solid #ccc"
              >
                <Image
                  v-if="element1.checkimg"
                  :src="element1.src"
                  :alt="element1.data"
                  width="100"
                  height="75"
                  style="object-fit: contain; width: 100%; height: 100%"
                  preview
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                />
                <div v-else>
                  <a :href="element1.src" download class="w-full no-underline">
                    <img
                      :src="
                        basedomainURL +
                        '/Portals/Image/file/' +
                        element1.data.substring(
                          element1.data.lastIndexOf('.') + 1
                        ) +
                        '.png'
                      "
                      style="width: 50px; height: 50px; object-fit: contain"
                      :alt="element1.data"
                    />
                    <div>
                      {{ element1.data }}
                    </div></a
                  >
                </div>
                <Button
                  @click="deleImgBugcmt(element1)"
                  style="
                    top: -15%;
                    right: -10%;
                    height: 20px !important;
                    width: 20px !important;
                  "
                  class="absolute p-0 m-0 p-button-rounded"
                  icon="pi pi-times"
                >
                </Button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeBugComment()"
        class="p-button-text"
      />

      <Button label="Lưu" icon="pi pi-check" @click="saveBugComment()" />
    </template>
  </Dialog>
  <Dialog
    v-model:visible="isShowAddMore"
    :style="{ width: '45vw' }"
    header="Thêm nhanh lỗi"
    @hide="reDataBugcmt"
  >
 
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0 pt-2">Nhóm lỗi</label>
          <Dropdown
            v-model="groupNameSave"
            :options="listDropdownGroupBug"
            :editable="true"
            optionLabel="name"
            optionValue="name"
            placeholder="Chọn hoặc nhập nhóm lỗi"
            spellcheck="true"
            class="col-4 ip36 p-0"
          />

          <label class="col-2 text-left p-0 pt-2 px-2">Trạng thái</label>
          <Dropdown
            v-model="bugInComment.status"
            :options="listStatusBugComment"
            optionLabel="name"
            optionValue="code"
            placeholder="Chọn trạng thái"
            spellcheck="true"
            class="col-4 ip36 p-0"
          />
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0 pt-2">Ngày bắt đầu</label>
          <Calendar
            class="col-4 p-0"
            :showIcon="true"
            id="time24"
            :showTime="true"
            autocomplete="on"
            v-model="bugInComment.start_date"
          />
          <label class="col-2 text-left p-2 pt-2">Ngày kết thúc</label>
          <Calendar
            class="col-4 p-0"
            :showIcon="true"
            id="time24"
            :showTime="true"
            autocomplete="on"
            v-model="bugInComment.end_date"
          />
        </div>
        <div class="field col-12 md:col-12" v-if="listCheckList.length > 0">
          <div
            class="field col-12 p-0 md:col-12"
            v-for="(element11, index11) in listCheckList"
            :key="index11"
          >
            <div class="col-12 p-0 flex field">
              <div class="col-2 ip36 p-0 pt-2">Mô tả lỗi {{ index11 + 1 }}</div>
              <div class="col-10 p-0">
                <InputText
                  v-model="element11.value.des"
                  readonly
                  class="w-full"
                />
              </div>
            </div>
            <div
              class="col-12 p-0 flex field"
              v-for="(element1, index1) in element11.filesPaste"
              :key="index1"
            >
              <div class="col-2 text-left"></div>
              <div class="col-2 p-0">
                <Image
                  :src="element1"
                  :alt="'Ảnh' + index1"
                  width="70"
                  height="50"
                  style="
                    object-fit: contain;
                    border: 1px solid #ccc;
                    width: 70px;
                    height: 50px;
                  "
                  preview
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                />
              </div>
              <div
                class="col-7 flex"
                style="align-items: center; vertical-align: middle"
              >
                Ảnh lỗi {{ index11 + 1 }}.{{ index1 + 1 }}
              </div>
              <!-- <div class="col-1">
            <Button
              @click="removeFileBase(element1)"
              icon="pi pi-times"
            ></Button>
          </div> -->
            </div>
            <div
              class="col-12 p-0 flex field"
              v-for="(element1, index1) in element11.arrFiles"
              :key="index1"
            >
              <div class="col-2 text-left"></div>
              <div class="col-2 p-0">
                <Image
                  v-if="element1.checkimg"
                  :src="element1.src"
                  :alt="element1.data"
                  width="70"
                  height="50"
                  style="
                    object-fit: contain;
                    border: 1px solid #ccc;
                    width: 70px;
                    height: 50px;
                  "
                  preview
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                />
                <div v-else>
                  <a :href="element1.src" download class="w-full no-underline">
                    <img
                      :src="
                        basedomainURL +
                        '/Portals/Image/file/' +
                        element1.data.substring(
                          element1.data.lastIndexOf('.') + 1
                        ) +
                        '.png'
                      "
                      style="width: 70px; height: 50px; object-fit: contain"
                      :alt="element1.data"
                    />
                  </a>
                </div>
              </div>
              <div
                class="col-7 flex"
                style="align-items: center; vertical-align: middle"
              >
                {{ element1.data }}
              </div>
              <!-- <div class="col-1">
            <Button
              @click="removeFileBugComment(element1)"
              icon="pi pi-times"
            ></Button>
          </div> -->
            </div>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-2 ip36 p-0 pt-2">Mô tả</div>
          <div class="col-4 p-0">
            <InputText v-model="bugInComment.des" autofocus class="w-full" />
          </div>
          <div class="col-2 pt-2 text-left">File</div>

          <div
            class="
              col-4
              p-0
              flex
              border-1 border-solid border-round-md border-300
            "
          >
            <FileUpload
              class="h-full"
              chooseLabel="Files"
              :auto="true"
              mode="basic"
              :multiple="true"
              :maxFileSize="10000000"
              @select="onUploadFileBugComment"
            >
            </FileUpload>

            <QuillEditor
              style="padding: 0"
              @text-change="onPasteImg($event)"
              placeholder="Nơi paste ảnh..."
              ref="paste_zone"
              @keypress="isNumberKey($event)"
              class="w-full"
              contentType="delta"
              :content="comment"
              v-model:content="valImg"
              theme="bubble"
            />
          </div>
        </div>

        <div
          class="col-12 p-0 flex field"
          v-for="(element1, index1) in listImgPast"
          :key="index1"
        >
          <div class="col-2 text-left"></div>
          <div class="col-2 p-0">
            <Image
              :src="element1"
              :alt="'Ảnh' + index1"
              width="70"
              height="50"
              style="
                object-fit: contain;
                border: 1px solid #ccc;
                width: 70px;
                height: 50px;
              "
              preview
              @error="
                $event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'
              "
            />
          </div>
          <div
            class="col-7 flex"
            style="align-items: center; vertical-align: middle"
          >
            Ảnh {{ index1 + 1 }}
          </div>
          <div class="col-1">
            <Button
              @click="removeFileBase(element1)"
              icon="pi pi-times"
            ></Button>
          </div>
        </div>
        <div
          class="col-12 p-0 flex field"
          v-for="(element1, index1) in arrFiles"
          :key="index1"
        >
          <div class="col-2 text-left"></div>
          <div class="col-2 p-0">
            <Image
              v-if="element1.checkimg"
              :src="element1.src"
              :alt="element1.data"
              width="70"
              height="50"
              style="
                object-fit: contain;
                border: 1px solid #ccc;
                width: 70px;
                height: 50px;
              "
              preview
              @error="
                $event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'
              "
            />
            <div v-else>
              <a :href="element1.src" download class="w-full no-underline">
                <img
                  :src="
                    basedomainURL +
                    '/Portals/Image/file/' +
                    element1.data.substring(
                      element1.data.lastIndexOf('.') + 1
                    ) +
                    '.png'
                  "
                  style="width: 70px; height: 50px; object-fit: contain"
                  :alt="element1.data"
                />
              </a>
            </div>
          </div>
          <div
            class="col-7 flex"
            style="align-items: center; vertical-align: middle"
          >
            {{ element1.data }}
          </div>
          <div class="col-1">
            <Button
              @click="removeFileBugComment(element1)"
              icon="pi pi-times"
            ></Button>
          </div>
        </div>

        <Button
          label="Thêm mới"
          icon="pi pi-plus"
          @click="addTempBug()"
          class="p-button-text"
        />
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeBugComment()"
        class="p-button-text"
      />

      <Button label="Lưu" icon="pi pi-check" @click="saveBugCommentMore()" />
    </template>
  </Dialog>
  <OverlayPanel
    ref="opbugcomment"
    appendTo="body"
    :showCloseIcon="false"
    id="overlay_panelS1"
    style="width: 250px"
    :breakpoints="{ '960px': '20vw' }"
  >
    <div>
      <div v-for="item1 in listStatusBugComment" :key="item1.code">
        <Button
          :label="item1.name"
          :class="item1.css"
          class="w-full mb-1"
          @click="onChangeStatusBug(item1)"
        />
      </div>
    </div>
  </OverlayPanel>
  <Dialog
    v-model:visible="isThumnFiles"
    :style="{ width: '40vw' }"
    header="Danh sách File"
  >
 
    <div class="flex format-center">
      <div style="display: grid; grid-template-columns: repeat(5, 1fr)">
        <div
          v-for="(element1, index1) in listThumFiles"
          :key="index1"
          class="m-1"
          style="border: 3px solid #ccc"
        >
          <Image
            v-if="element1.checkimg"
            :src="element1.src"
            :alt="element1.data"
            width="100"
            height="100"
            style="object-fit: contain; width: 100px; height: 100px"
            preview
            @error="
              $event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'
            "
          />
          <div v-else>
            <a
              v-if="element1.src != null && element1.src != ''"
              :href="element1.src"
              download
              class="w-full no-underline"
            >
              <img
                :src="
                  basedomainURL +
                  '/Portals/Image/file/' +
                  element1.data.substring(element1.data.lastIndexOf('.') + 1) +
                  '.png'
                "
                style="width: 100px; height: 100px; object-fit: contain"
                :alt="element1.data"
              />
            </a>
          </div>
        </div>
      </div>
    </div>
  </Dialog>
  <OverlayPanel
    class="p-0"
    ref="panelEmoij3"
    appendTo="body"
    :showCloseIcon="false"
    id="overlay_panelEmoij3"
  >
    <VuemojiPicker @emojiClick="handleEmojiClickChild" />
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
</template>
<style scoped>
/* 
  .field.grid .col-fixed, .formgrid.grid .col-fixed, .field.grid .col, .formgrid.grid .col, .field.grid .col-1, .formgrid.grid .col-1, .field.grid .col-2, .formgrid.grid .col-2, .field.grid .col-3, .formgrid.grid .col-3, .field.grid .col-4, .formgrid.grid .col-4, .field.grid .col-5, .formgrid.grid .col-5, .field.grid .col-6, .formgrid.grid .col-6, .field.grid .col-7, .formgrid.grid .col-7, .field.grid .col-8, .formgrid.grid .col-8, .field.grid .col-9, .formgrid.grid .col-9, .field.grid .col-10, .formgrid.grid .col-10, .field.grid .col-11, .formgrid.grid .col-11, .field.grid .col-12, .formgrid.grid .col-12 {
    padding: 0.5rem !important;
  }    */
.row.true {
  background-color: rgb(190, 211, 245) !important;
}
.format-center {
  display: flex;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-treetable) {
  .p-treetable-tbody > tr > td {
    padding: 0;
  }
}
</style>
<style lang="scss" scoped>
::v-deep(.p-panel) {
  .p-panel-header {
    padding: 0;
  }
}
</style>