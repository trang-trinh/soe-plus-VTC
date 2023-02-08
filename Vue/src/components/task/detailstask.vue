<script setup>
//Khai báo InJect và Import (import)
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { VuemojiPicker } from "vuemoji-picker";
//tung
import bugdetail from "./bugdetail.vue";

import moment from "moment";
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const emitter = inject("emitter");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

//Nơi nhận dữ liệu
const props = defineProps({
  isCheckTask: Boolean,
  task: Object,
});
//Khai báo biến (variable)
//tung
const cache = ref("");
const basedomainURL = baseURL;
const toast = useToast();
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
const listCommentBugSave = ref();
const listCommentBugs = ref();
let filecoments = [];
const listFileComment = ref([]);
const comment = ref("");
const commentChild = ref("");
const panelEmoij1 = ref();
const panelEmoij2 = ref();
const panelEmoij3 = ref();
const panelEmoij4 = ref();
const panelCalendar = ref();
const checkFileComment = ref(false);
const isCheckTask = ref(false);
//tung
const isShowBug = ref(true);
const listBugSave = ref();
const taskName = ref();
const projectID_Save = ref();
const taskSave = ref();
const checkPreCmtBug = ref(false);
//end tung

const isDetailsBug = ref(false);
const checkAddChildCmt = ref();
const checkEditComment = ref(false);
const checkEditCommentChild = ref(false);
const bugComment = ref();
const checkFileCommentChild = ref(false);
const listFileCommentChild = ref([]);
// Khai báo hàm (function)
watch(props, () => {
  if (props.isCheckTask == true) {
    isCheckTask.value = true;
    console.log(props);
  }
});
//tung
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "goToNoti":
      id_noti.value = obj.data;
      break;
  }
});
function generateUUID() {
  // Public Domain/MIT
  var d = new Date().getTime(); //Timestamp
  var d2 =
    (typeof performance !== "undefined" &&
      performance.now &&
      performance.now() * 1000) ||
    0; //Time in microseconds since page-load or 0 if unsupported
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
    var r = Math.random() * 16; //random number between 0 and 16
    if (d > 0) {
      //Use timestamp until depleted
      r = (d + r) % 16 | 0;
      d = Math.floor(d / 16);
    } else {
      //Use microseconds since page-load if supported
      r = (d2 + r) % 16 | 0;
      d2 = Math.floor(d2 / 16);
    }
    return (c === "x" ? r : (r & 0x3) | 0x8).toString(16);
  });
}
//list bug from task detail
const showBugs = (value, dataLe) => {
  cache.value = generateUUID();
  taskSave.value = dataLe;
  projectID_Save.value = dataLe.project_id;
  checkPreCmtBug.value = false;
  taskName.value = [];
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_bug_list",
        par: [
          { par: "task_id", va: value },
          { par: "search", va: options.value.searchText },
          { par: "user_id", va: store.getters.user.user_id },
          { par: "type", va: 0 },
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
      listBugSave.value = data;
      isDetailsBug.value = false;
      isShowBug.value = true;
    })
    .catch((error) => {
       
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
//end tung
//BÌNH LUẬN CÔNG VIỆC

const reloadChildComment = (comment_id) => {
  (async () => {
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "task_comment_list",
          par: [{ par: "task_id", va: props.task.task_id }],
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
          proc: "task_comment_listwithid",
          par: [
            { par: "task_id", va: props.task.task_id },
            { par: "parent_id", va: comment_id },
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

const editComment = (value, check) => {
  bugComment.value = value;
  if (check) checkEditComment.value = value.comment_id;
  else checkEditCommentChild.value = value.comment_id;
};
const delEditFileComment = (url) => {
  bugComment.value.url_file = bugComment.value.url_file.filter((x) => x != url);
};
const saveEditComment = (item, check) => {
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
    .put(baseURL + "/api/task_comment/Update_comment", formData, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Sửa bình luận thành công!");
        isDetailsBug.value = true;
        if (check) reloadComment(props.task.task_id);
        else reloadChildComment(bugComment.value.parent_id);
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
const showAddChildCmt = (value) => {
  commentChild.value = "";
  (async () => {
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "task_comment_list",
          par: [{ par: "task_id", va: props.task.task_id }],
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
          proc: "task_comment_listwithid",
          par: [
            { par: "task_id", va: value.task_id },
            { par: "parent_id", va: value.comment_id },
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

const handleEmojiClickChild = (event) => {
  if (commentChild.value)
    commentChild.value = commentChild.value + event.unicode;
  else commentChild.value = event.unicode;
  commentChild.value = commentChild.value
    .replace("<p>", "")
    .replace("</p>", "");
  comment_zone1.value.setHTML("<p>" + comment.value + "</p>");
};

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
//Render Bình luận công việc

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

///DANH SÁCH NGƯỜI DÙNG

const comment_zone = ref();
const comment_zone1 = ref();
const comment_zone_main = ref();
const addComment = (check, comment_id) => {
  if (check) {
    if (
      (comment.value == "" || comment.value == null) &&
      filecoments.length == 0
    )
      return;
    else {
      let bugComment = {
        task_id: props.task.task_id,
        des: comment.value,
      };

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
        .post(baseURL + "/api/task_comment/Add_comment", formData, config)
        .then((response) => {
          if (response.data.err != "1") {
            swal.close();
            toast.success("Thêm bình luận thành công!");
            isDetailsBug.value = true;
            reloadComment(props.task.task_id);
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
  )
    return;
  else {
    let bugComment = {
      task_id: props.task.task_id,
      parent_id: comment_id,
      des: commentChild.value,
    };
    commentChild.value = "";
    let formData = new FormData();
    for (var i = 0; i < filecoments.length; i++) {
      let file = filecoments[i];
      formData.append("url_file", file);
    }
    filecoments = [];
    listFileCommentChild.value = [];
    formData.append("comment", JSON.stringify(bugComment));
    commentChild.value = "";
    axios
      .post(baseURL + "/api/task_comment/Add_comment", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm bình luận thành công!");
          isDetailsBug.value = true;
          reloadChildComment(comment_id);
          commentChild.value = "";
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

const deleteComment = (value, check) => {
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
          .delete(baseURL + "/api/task_comment/Delete_comment", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: value.comment_id != null ? [value.comment_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá bình luận thành công!");
              if (check) reloadComment(value.task_id);
              else reloadChildComment(value.parent_id);
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
//Lấy file logo
const chonanh = (id) => {
  document.getElementById(id).click();
};
//Load Bình luận công việc

const reloadComment = (id) => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_comment_list",
        par: [{ par: "task_id", va: id }],
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
      renderComment(data);
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
const id_noti = ref();
//Xóa active
const reloadViewTask = () => {
  emitter.emit("emitData", { type: "reloadViewTask", data: id_noti.value });
};

//Emoij

const showEmoji = (event, check) => {
  if (check == 1) panelEmoij1.value.toggle(event);
  if (check == 2) panelEmoij2.value.toggle(event);
  if (check == 3) panelEmoij3.value.toggle(event);
  if (check == 4) panelEmoij4.value.toggle(event);
  if (check == 5) panelCalendar.value.toggle(event);
};
const handleEmojiClick = (event) => {
  if (comment.value) comment.value = comment.value + event.unicode;
  else comment.value = event.unicode;
  comment.value = comment.value.replace("<p>", "").replace("</p>", "");
  comment_zone_main.value.setHTML("<p>" + comment.value + "</p>");
};
onMounted(() => {
  return {};
});
</script>
<template>
  <Sidebar
    v-model:visible="isCheckTask"
    :baseZIndex="100"
    :showCloseIcon="false"
    position="right"
    class="p-sidebar-lg"
    v-on:hide="reloadViewTask()"
    v-on:show="
      reloadComment(props.task.task_id);
      showBugs(props.task.task_id, props.task);
    "
  >
    <div>
      <h2>{{ props.task.task_name }}</h2>
    </div>
    <div class="grid formgrid m-2">
      <div class="field col-12 md:col-12 flex">
        <div class="col-8 p-0 flex">
          <Button
            :label="
              props.task.is_important == 0
                ? 'Không quan trọng'
                : props.task.is_important == 1
                ? 'Bình thường'
                : props.task.is_important == 2
                ? 'Gấp'
                : props.task.is_important == 3
                ? 'Rất gấp'
                : 'Bình thường'
            "
            :class="
              props.task.is_important == 0
                ? ' p-button-secondary'
                : props.task.is_important == 1
                ? ' p-button-success'
                : props.task.is_important == 2
                ? ' p-button-warning'
                : props.task.is_important == 3
                ? ' p-button-danger'
                : ' p-button-danger'
            "
          />

          <Button
            v-if="props.task.estimated_hours"
            :label="
              (props.task.taskTime / 60).toFixed() +
              '/' +
              props.task.estimated_hours +
              ' Giờ'
            "
            class="mx-2"
            :class="
              (props.task.taskTime / 60).toFixed() > props.task.estimated_hours
                ? 'p-button-danger'
                : (props.task.estimated_hours / 60).toFixed() -
                    props.task.taskTime <=
                  1
                ? 'p-button-warning'
                : 'p-button-success'
            "
          />
          <Button
            v-else
            :label="'Vô thời hạn'"
            class="mx-2"
            :class="'p-button-success'"
          />
        </div>
        <div class="col-4 p-0">
          <Button
            v-if="
              props.task.test_user_ids.filter(
                (x) => x == store.getters.user.user_id
              ).length > 0
            "
            :label="
              props.task.status == 0
                ? 'Đang lập kế hoạch'
                : props.task.status == 1
                ? 'Đang làm'
                : props.task.status == 2
                ? 'Đang đợi Test'
                : props.task.status == 3
                ? 'Đã Test OK'
                : props.task.status == 4
                ? 'Test Chưa OK'
                : props.task.status == 5
                ? 'Phát sinh thêm'
                : props.task.status == 6
                ? 'Tạm đóng'
                : 'Trạng thái'
            "
            @click="toggleStatusDetails(props.task, $event)"
            aria:haspopup="true"
            aria-controls="overlay_panelS"
            :class="
              props.task.status == 0
                ? 'p-button-raised  p-button-secondary'
                : props.task.status == 1
                ? 'p-button-raised'
                : props.task.status == 2
                ? 'p-button-raised p-button-help'
                : props.task.status == 3
                ? 'p-button-raised p-button-success'
                : props.task.status == 4
                ? 'p-button-raised p-button-danger'
                : props.task.status == 5
                ? 'p-button-raised p-button-secondary'
                : props.task.status == 6
                ? 'p-button-raised p-button-warning'
                : 'p-button-raised'
            "
          />
        </div>
        <OverlayPanel
          ref="opDetails"
          appendTo="body"
          :showCloseIcon="false"
          id="overlay_panelS11"
          style="width: 250px"
          :breakpoints="{ '960px': '20vw' }"
        >
          <div>
            <div v-for="item in listStatusDetails" :key="item.code">
              <Button
                :label="item.name"
                :class="item.css"
                class="w-full mb-1"
                @click="onProductSelect(item, taskDetails, true)"
              />
            </div>
          </div>
        </OverlayPanel>
      </div>
      <div class="field col-12 md:col-12 flex">
        <div class="col-6 md:col-6 p-0">
          <span class="font-bold"
            >Người thực hiện: {{ props.task.full_name }}</span
          >
        </div>
        <div class="col-6 md:col-6 p-0">
          <span class="font-bold"
            >Người cập nhật: {{ props.task.modified_name }}</span
          >
        </div>
      </div>
      <div class="field col-12 md:col-12 flex">
        <div class="col-6 md:col-6 p-0">
          <span v-if="props.task.estimated_date" class="font-bold"
            >Ngày thực hiện: {{ props.task.date_W }}
          </span>
          <span class="font-bold mx-5" v-if="props.task.estimated_hours > 0"
            >Số giờ: {{ props.task.estimated_hours }}</span
          >
        </div>
        <div class="col-6 md:col-6 p-0">
          <span v-if="props.task.modified_date" class="font-bold"
            >Ngày cập nhật: {{ props.task.date_U }}
          </span>
        </div>
      </div>
      <div class="field col-12 md:col-12">
        <span v-if="props.task.actual_date" class="font-bold"
          >Ngày thực tế: {{ props.task.date_A }}
        </span>
        <span v-if="props.task.actual_hours > 0" class="font-bold mx-5"
          >Số giờ: {{ props.task.actual_hours }}</span
        >
      </div>
      <div class="field col-12 md:col-12">
        <hr />
      </div>
      <div class="field col-12 md:col-12 format-center">
        <div class="col-12 p-0 cursor-pointer" v-if="props.task.url_file">
          <div
            v-for="(item, index) in props.task.url_file"
            :key="index"
            class="flex"
          >
            <a
              :href="basedomainURL + item"
              download
              class="w-full no-underline"
            >
              <Toolbar class="w-full py-3">
                <template #start>
                  <div class="flex">
                    <img
                      :src="
                        basedomainURL +
                        '/Portals/Image/file/' +
                        item.substring(item.lastIndexOf('.') + 1) +
                        '.png'
                      "
                      style="object-fit: contain"
                      width="50"
                      height="50"
                      alt="logorar"
                    />
                    <span style="line-height: 50px">
                      {{ item.substring(16) }}</span
                    >
                  </div>
                </template>
              </Toolbar>
            </a>
          </div>
        </div>
      </div>

      <div class="field col-12 md:col-12">
        <p v-html="props.task.des" style="font-size: 16px"></p>
        <!-- tung -->
      </div>
      <div class="field col-12 md:col-12">
        <hr />
      </div>
      <div class="field col-12 md:col-12">
        <bugdetail
          :project_id="projectID_Save"
          :isShowBug="isShowBug"
          :task="taskSave"
          :cache="cache"
        />
      </div>
      <div class="field col-12 md:col-12 flex">
        <div
          v-for="(item, index) in props.task.keywords"
          :key="index"
          class="mr-1"
        >
          <Chip>
            {{ item }}
          </Chip>
        </div>
      </div>
    </div>
    <div class="pt-3 pb-3">
      <div class="px-3 pb-3">Bình luận công việc:</div>

      <div class="flex px-3">
        <div class="grid w-full">
          <div class="col-12 p-0 flex">
            <div
              class="col-10 p-0 ml-2 border-1 border-round-xs border-600 flex"
              style="border-radius: 5px"
            >
              <div class="border-0 col-10 p-0">
                <QuillEditor
                  ref="comment_zone_main"
                  placeholder="Viết bình luận..."
                  contentType="html"
                  :content="comment"
                  v-model:content="comment"
                  theme="bubble"
                />
              </div>
              <div class="col-2 p-0 relative">
                <div class="format-center flex">
                  <!-- v-clickoutside="onHideEmoji" -->

                  <Button
                    class="
                      p-1 p-button-rounded p-button-text p-button-plain
                      m-1
                    "
                    @click="showEmoji($event, 1)"
                  >
                    <img
                      alt="logo"
                      src="/src/assets/image/smile.png"
                      width="20"
                      height="20"
                    />
                  </Button>

                  <Button
                    class="
                      p-1 p-button-rounded p-button-text p-button-plain
                      m-1
                    "
                    @click="chonanh('anhcongviec')"
                  >
                    <img
                      alt="logo1"
                      src="/src/assets/image/imageicon.png"
                      width="20"
                      height="20"
                    />
                  </Button>
                  <Button
                    @click="chonanh('FileCommentTask')"
                    class="
                      p-1 p-button-rounded p-button-text p-button-plain
                      m-1
                    "
                  >
                    <img
                      alt="logo2"
                      src="/src/assets/image/filesymbol.png"
                      width="20"
                      height="20"
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
            <div class="w-2 h-full px-3 format-center col-2 px-2 p-0">
              <Button
                icon="pi pi-send"
                @click="addComment(true, null)"
                class="w-4rem h-3rem mx-1"
              />
            </div>
          </div>

          <div class="col-12 flex" v-if="checkFileComment">
            <div
              v-for="(item, index) in listFileComment"
              :key="index"
              class="mr-2 relative"
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
              ></Button>
              <img
                v-if="item.checkimg"
                :src="item.src"
                :alt="item.data.name"
                style="width: 100px; height: 100px; object-fit: contain"
              />
              <div v-else>
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
                <div>{{ item.data.name }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div class="overflow-y-auto overflow-x-hidden comment-height-scroll">
      <div v-for="(item, index) in listCommentBugs" :key="index">
        <div class="w-full" :id="item.data.comment_id">
          <div class="grid formgrid m-2">
            <div class="field col-12 md:col-12 flex p-0">
              <div class="col-1 md:col-1 pl-2 pr-3">
                <Avatar
                  :image="basedomainURL + item.data.avatar"
                  class="w-full"
                  size="large"
                  shape="circle"
                />
              </div>
              <div class="col-11 md:col-11 p-0">
                <Panel class="w-full">
                  <template #header>
                    <div class="flex relative w-full p-3">
                      <span class="font-bold pr-1"
                        >{{ item.data.created_name }}
                      </span>
                      <span>
                        {{
                          moment(item.data.created_date).format(
                            "HH:mm DD/MM/YYYY"
                          )
                        }}
                      </span>

                      <div
                        v-if="item.data.user_id == store.getters.user.user_id"
                        class="absolute right-0 bottom-0 mb-2"
                      >
                        <Button
                          class="
                            p-button-rounded p-button-secondary p-button-text
                          "
                          @click="editComment(item.data, true)"
                          type="button"
                          icon="pi pi-pencil"
                        ></Button>
                        <Button
                          class="
                            p-button-rounded p-button-secondary p-button-text
                          "
                          @click="deleteComment(item.data, true)"
                          type="button"
                          icon="pi pi-trash"
                        ></Button>
                      </div>
                    </div>
                  </template>

                  <div v-if="item.data.comment_id != checkEditComment">
                    <div
                      style="margin-bottom: 10px"
                      v-html="item.data.des"
                    ></div>
                    <div class="flex">
                      <div
                        v-for="(element, index) in item.data.url_file"
                        :key="index"
                        class="mr-2"
                      >
                        <Image
                          class="pt-2"
                          v-if="element.checkimg"
                          :src="element.src"
                          :alt="element.data"
                          width="150"
                          height="150"
                          style="
                            object-fit: contain;
                            border: 1px solid #ccc;
                            width: 150px;
                            height: 150px;
                          "
                          preview
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                        />
                        <div v-else>
                          <a
                            :href="element.src"
                            download
                            class="w-full no-underline"
                          >
                            <img
                              :src="
                                basedomainURL +
                                '/Portals/Image/file/' +
                                element.data.substring(
                                  element.data.indexOf('.') + 1
                                ) +
                                '.png'
                              "
                              style="
                                width: 50px;
                                height: 50px;
                                object-fit: contain;
                              "
                              :alt="element.data"
                            />
                            <div>{{ element.data.substring(1) }}</div></a
                          >
                        </div>
                      </div>
                    </div>
                  </div>
                  <div v-else>
                    <div class="w-full h-full p-0 m-0 flex">
                      <QuillEditor
                        placeholder="Viết bình luận..."
                        class="w-full"
                        ref="comment_zone"
                        contentType="html"
                        :content="bugComment.des"
                        v-model:content="bugComment.des"
                        theme="bubble"
                      />

                      <Button
                        icon="pi pi-check"
                        @click="saveEditComment(item, true)"
                        class="w-2"
                      />
                    </div>
                    <div>
                      <div
                        v-for="(element, stt) in bugComment.url_file"
                        :key="stt"
                        class="mr-2 relative my-2"
                      >
                        <Button
                          icon="pi pi-times"
                          class="
                            p-button-rounded p-button-text p-button-plain
                            absolute
                            top-0
                            right-0
                            w-1rem
                            h-1rem
                          "
                          @click="delEditFileComment(element, item)"
                        ></Button>
                        <Image
                          class="pt-2"
                          v-if="element.checkimg"
                          :src="element.src"
                          :alt="element.data"
                          width="150"
                          height="150"
                          style="
                            object-fit: contain;
                            border: 1px solid #ccc;
                            width: 150px;
                            height: 150px;
                          "
                          preview
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                        />
                        <div v-else>
                          <a
                            :href="element.src"
                            download
                            class="w-full no-underline"
                          >
                            <img
                              :src="
                                basedomainURL +
                                '/Portals/Image/file/' +
                                element.data.substring(
                                  element.data.indexOf('.') + 1
                                ) +
                                '.png'
                              "
                              style="
                                width: 50px;
                                height: 50px;
                                object-fit: contain;
                              "
                              :alt="element.data"
                            />
                            <div>{{ element.data.substring(1) }}</div></a
                          >
                        </div>
                      </div>
                    </div>
                  </div>
                </Panel>
                <div
                  v-if="checkAddChildCmt != item.data.comment_id"
                  @click="showAddChildCmt(item.data)"
                  class="cursor-pointer font-bold pt-2"
                >
                  Phản hồi
                </div>
                <div
                  @click="showAddChildCmt(item.data)"
                  class="flex cursor-pointer ml-3"
                  v-if="checkAddChildCmt != item.data.comment_id"
                >
                  <div v-if="item.data.haschild > 0" class="flex">
                    <Button
                      class="p-button-rounded p-button-secondary p-button-text"
                      type="button"
                      icon="pi pi-reply"
                    ></Button>
                    <div style="line-height: 36px">Xem thêm</div>
                  </div>
                </div>
                <div v-else class="flex m-3">
                  <div class="grid w-full">
                    <div
                      class="field col-12 md:col-12 p-0"
                      v-if="item.children.length > 0"
                    >
                      <div
                        v-for="element in item.children"
                        :key="element.key"
                        class="col-12 m-0 p-0 flex py-1"
                      >
                        <div class="col-1 md:col-1 pl-2 pr-3">
                          <Avatar
                            :image="basedomainURL + element.data.avatar"
                            class="w-2rem h-2rem m-2"
                            size="large"
                            shape="circle"
                          />
                        </div>
                        <div class="col-11 md:col-11 p-0">
                          <Panel class="w-full">
                            <template #header>
                              <div class="flex relative w-full p-3">
                                <span class="font-bold pr-1"
                                  >{{ element.data.created_name }}
                                </span>
                                <span>
                                  {{
                                    moment(element.data.created_date).format(
                                      "HH:mm DD/MM/YYYY"
                                    )
                                  }}
                                </span>

                                <div
                                  v-if="
                                    element.data.user_id ==
                                    store.getters.user.user_id
                                  "
                                  class="absolute right-0 bottom-0 mb-2"
                                >
                                  <Button
                                    class="
                                      p-button-rounded
                                      p-button-secondary
                                      p-button-text
                                    "
                                    @click="editComment(element.data, false)"
                                    type="button"
                                    icon="pi pi-pencil"
                                  ></Button>
                                  <Button
                                    class="
                                      p-button-rounded
                                      p-button-secondary
                                      p-button-text
                                    "
                                    @click="deleteComment(element.data, false)"
                                    type="button"
                                    icon="pi pi-trash"
                                  ></Button>
                                </div>
                              </div>
                            </template>
                            <div
                              v-if="
                                element.data.comment_id != checkEditCommentChild
                              "
                            >
                              <span
                                class="pb-2"
                                v-html="element.data.des"
                              ></span>
                              <div class="flex">
                                <div
                                  v-for="(element1, index1) in element.data
                                    .url_file"
                                  :key="index1"
                                  class="mr-2"
                                >
                                  <Image
                                    class="pt-2"
                                    v-if="element1.checkimg"
                                    :src="element1.src"
                                    :alt="element1.data"
                                    width="150"
                                    height="150"
                                    style="
                                      object-fit: contain;
                                      border: 1px solid #ccc;
                                      width: 150px;
                                      height: 150px;
                                    "
                                    preview
                                    @error="
                                      $event.target.src =
                                        basedomainURL +
                                        '/Portals/Image/noimg.jpg'
                                    "
                                  />
                                  <div v-else>
                                    <a
                                      :href="element1.src"
                                      download
                                      class="w-full no-underline"
                                    >
                                      <img
                                        :src="
                                          basedomainURL +
                                          '/Portals/Image/file/' +
                                          element1.data.substring(
                                            element1.data.indexOf('.') + 1
                                          ) +
                                          '.png'
                                        "
                                        style="
                                          width: 50px;
                                          height: 50px;
                                          object-fit: contain;
                                        "
                                        :alt="element1.data"
                                      />
                                      <div>
                                        {{ element1.data }}
                                      </div></a
                                    >
                                  </div>
                                </div>
                              </div>
                            </div>
                            <div v-else>
                              <div class="w-full h-full p-0 m-0 flex">
                                <QuillEditor
                                  class="w-full"
                                  placeholder="Viết bình luận..."
                                  contentType="html"
                                  :content="bugComment.des"
                                  v-model:content="bugComment.des"
                                  theme="bubble"
                                />

                                <Button
                                  icon="pi pi-check"
                                  @click="saveEditComment(item.data, false)"
                                  class="w-2"
                                />
                              </div>
                              <div>
                                <div
                                  v-for="(element, stt) in bugComment.url_file"
                                  :key="stt"
                                  class="mr-2 relative my-2"
                                >
                                  <Button
                                    icon="pi pi-times"
                                    class="
                                      p-button-rounded
                                      p-button-text
                                      p-button-plain
                                      absolute
                                      top-0
                                      right-0
                                      w-1rem
                                      h-1rem
                                    "
                                    @click="
                                      delEditFileComment(element, item.data)
                                    "
                                  ></Button>
                                  <Image
                                    class="pt-2"
                                    v-if="element.checkimg"
                                    :src="element.src"
                                    :alt="element.data"
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
                                        basedomainURL +
                                        '/Portals/Image/noimg.jpg'
                                    "
                                  />
                                  <div v-else>
                                    <a
                                      :href="element.src"
                                      download
                                      class="w-full no-underline"
                                    >
                                      <img
                                        :src="
                                          basedomainURL +
                                          '/Portals/Image/file/' +
                                          element.data.substring(
                                            element.data.indexOf('.') + 1
                                          ) +
                                          '.png'
                                        "
                                        style="
                                          width: 50px;
                                          height: 50px;
                                          object-fit: contain;
                                        "
                                        :alt="element.data"
                                      />
                                      <div>
                                        {{ element.data }}
                                      </div></a
                                    >
                                  </div>
                                </div>
                              </div>
                            </div>
                          </Panel>
                        </div>
                      </div>
                    </div>

                    <div class="col-12 p-0 flex">
                      <div
                        class="
                          pt-2
                          col-10
                          p-0
                          ml-2
                          border-1 border-round-xs border-600
                          flex
                        "
                        style="border-radius: 5px"
                      >
                        <div class="border-0 col-10 p-0">
                          <QuillEditor
                            class="w-full"
                            ref="comment_zone1"
                            placeholder="Viết bình luận..."
                            contentType="html"
                            :content="commentChild"
                            v-model:content="commentChild"
                            theme="bubble"
                          />
                        </div>
                        <div class="col-2 p-0 relative">
                          <div class="format-center">
                            <Button
                              class="
                                p-1
                                p-button-rounded
                                p-button-text
                                p-button-plain
                                m-1
                              "
                              @click="showEmoji($event, 2)"
                            >
                              <img
                                alt="logo"
                                src="/src/assets/image/smile.png"
                                width="20"
                                height="20"
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
                              @click="chonanh('ImageChild')"
                            >
                              <img
                                alt="logo1"
                                src="/src/assets/image/imageicon.png"
                                width="20"
                                height="20"
                              />
                            </Button>
                            <Button
                              @click="chonanh('FileCommentChild')"
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
                                width="20"
                                height="20"
                              />
                            </Button>
                            <input
                              class="hidden"
                              id="ImageChild"
                              type="file"
                              multiple="true"
                              accept="image/*"
                              @change="handleFileUploadChild"
                            />
                            <input
                              class="hidden"
                              id="FileCommentChild"
                              type="file"
                              multiple="true"
                              @change="handleFileUploadChild"
                            />
                          </div>
                        </div>
                      </div>
                      <div class="w-2 h-full px-3 format-center col-2 px-2 p-0">
                        <Button
                          icon="pi pi-send"
                          @click="addComment(false, item.data.comment_id)"
                          class="w-4rem h-3rem mx-1"
                        />
                      </div>
                    </div>
                    <div class="col-12 flex" v-if="checkFileCommentChild">
                      <div
                        v-for="(item, index) in listFileCommentChild"
                        :key="index"
                        class="mr-2 relative"
                      >
                        <Button
                          @click="delImgCommentChild(item.data)"
                          icon="pi pi-times"
                          class="
                            p-button-rounded p-button-text p-button-plain
                            absolute
                            top-0
                            right-0
                            w-1rem
                            h-1rem
                          "
                        ></Button>
                        <img
                          v-if="item.checkimg"
                          :src="item.src"
                          :alt="item.data.name"
                          style="
                            width: 100px;
                            height: 100px;
                            object-fit: contain;
                          "
                        />
                        <div v-else>
                          <img
                            :src="
                              basedomainURL +
                              '/Portals/Image/file/' +
                              item.data.name.substring(
                                item.data.name.indexOf('.') + 1
                              ) +
                              '.png'
                            "
                            style="
                              width: 50px;
                              height: 50px;
                              object-fit: contain;
                            "
                            :alt="item.data.name"
                          />
                          <div>{{ item.data.name }}</div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Sidebar>
  <OverlayPanel
    class="p-0"
    ref="panelEmoij2"
    appendTo="body"
    :showCloseIcon="false"
    id="overlay_panelEmoij2"
  >
    <VuemojiPicker @emojiClick="handleEmojiClickChild" />
  </OverlayPanel>
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
  <OverlayPanel
    class="p-0"
    ref="panelEmoij1"
    appendTo="body"
    :showCloseIcon="false"
    id="overlay_panelEmoij1"
  >
    <VuemojiPicker @emojiClick="handleEmojiClick" />
  </OverlayPanel>
</template>
<style>
/* tung */
.list-bugs-item {
  border: solid #e9ecef;
  border-width: 0 0 1px 0;
  cursor: pointer;
}

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