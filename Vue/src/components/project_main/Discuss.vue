<script setup>
import { ref, inject, onMounted, onBeforeUnmount, watch } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import { encr } from "../../util/function.js";
import moment from "moment";
import { VuemojiPicker } from "vuemoji-picker";
import FileInfoVue from "../task_origin/Detail_Task/FileInfo.vue";

const cryoptojs = inject("cryptojs");
const first = ref(0);
const toast = useToast();
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios
const emitter = inject("emitter");
const basedomainURL = fileURL;
const user = store.state.user;

const props = defineProps({
  id: String,
});

const rules_discuss = {
  discuss_project_content: {
    required,
    $errors: [
      {
        $property: "discuss_project_content",
        $validator: "required",
        $message: "Nội dung thảo luận không được để trống!",
      },
    ],
  },
};
let line1 = "";
let line = "";
const panelEmoij1 = ref();
let filecoments = [];
const selectedDiscussProjectID = ref();
const selectedDiscussProjectContent = ref();
const comment = ref("");
const discussProject = ref({
  discuss_project_id: null,
  project_id: props.id,
  discuss_project_content: null,
  is_public: true,
  start_date: new Date(),
  end_date: null,
  is_order: null,
  members: [],
});
const v3$ = useVuelidate(rules_discuss, discussProject);

const opition = ref({
  type_chart: 1,
  PageNo: 0,
  PageSize: 20,
  sort: "created_date",
  ob: "DESC",
  totalRecords: 0,
});
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
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
const listProjectMainDiscuss = ref([]);
const headerAddDiscuss = ref();
const displayDiscuss = ref(false);
const issaveDiscuss = ref(false);
const listDropdownMembers = ref([]);
const DiscussMembers = ref([]);
const submitted = ref(false);

const showEmoji = (event, check) => {
  if (check == 1) panelEmoij1.value.toggle(event);
};

const fileInfo = ref();
const isViewFileInfo = ref(false);
const ViewFileInfo = (data) => {
  isViewFileInfo.value = true;
  fileInfo.value = data;
};
emitter.on("closeViewFile", (obj) => {
  isViewFileInfo.value = obj;
});

const loadData = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "project_main_get_list_discuss",
            par: [
              { par: "project_id", va: props.id },
              // { par: "ob", va: opition.value.ob },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      data[0].forEach(function (t, i) {
        t.STT = i + 1;
        t.Thanhviens = t.Thanhviens ? JSON.parse(t.Thanhviens) : [];
        t.ThanhvienShows = [];
        if (t.Thanhviens.length > 3) {
          t.ThanhvienShows = t.Thanhviens.slice(0, 3);
        } else {
          t.ThanhvienShows = [...t.Thanhviens];
        }
      });
      listProjectMainDiscuss.value = data[0];
      opition.value.totalRecords = data[0].length;
      listDropdownMembers.value = data[1].map((x) => ({
        name: x.full_name,
        code: x.user_id,
        avatar: x.avatar,
        ten: x.last_name,
      }));
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "LogsView.vue",
        log_content: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const listComments = ref([]);
const loadDiscuss = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "discuss_get_list",
            par: [
              { par: "discuss_project_id", va: selectedDiscussProjectID.value },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let commentJson = JSON.parse(response.data.data)[0];
      // let listCheckListJson = JSON.parse(response.data.data)[1];
      // countComments.value = listCheckListJson[0].count;
      commentJson.forEach((x) => {
        x.files = x.files != null ? JSON.parse(x.files) : null;
        if (x.files != null) {
          x.files.forEach((f) => {
            f.file_size = formatSize(f.file_size);
          });
        }
        x.tooltip =
          "Người tạo bình luận" +
          "<br/>" +
          x.full_name +
          "<br/>" +
          x.positions +
          "<br/>" +
          (x.department_name != null ? x.department_name : x.organiztion_name);
        x.contents = x.contents == "<body></body>" ? null : x.contents;
      });

      RenderComments(commentJson);
    })
    .catch((error) => {
      // toast.error("Tải dữ liệu không thành công!" + error);
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const comment_zone_main = ref();
const replyCmtValue = ref();
const DelReplyComment = () => {
  editComment.value = false;
  reply.value = false;
  replyCmtValue.value = null;
  listFileComment.value = [];
  comment.value = "";
  listSendFile.value = [];
  comment_zone_main.value.setHTML("<p>" + comment.value + "</p>");
};
const ReplyComment = (cmt) => {
  reply.value = true;
  editComment.value = false;
  replyCmtValue.value = [];
  listFileComment.value == [];
  listSendFile.value = [];
  comment.value = "";

  let comt = null;
  comt = cmt;
  comt.contents =
    comt.contents != null && comt.contents != "<body><p><br></p></body>"
      ? comt.contents.replace(
          'style="width:100%;max-width:30vw;object-fit:contain">',
          'style="width:100%;max-width:5vw;object-fit:contain">',
        )
      : null;
  replyCmtValue.value.push(comt);
  comment_zone_main.value.setHTML("<p>" + comment.value + "</p>");
};

const RenderComments = (data) => {
  let arrChils = [];
  data.forEach((cha) => {
    data.forEach((con) => {
      if (con.parent_id == cha.discuss_id) {
        con.parent = null;
        con.parent = cha;
      }
    });
  });
  data.forEach((z) => {
    arrChils.push(z);
  });

  arrChils.sort(function (a, b) {
    return new Date(a.created_date) - new Date(b.created_date);
  });
  listComments.value = arrChils;
};

const isAddDiscuss = ref(false);

const addDiscuss = (str) => {
  isAddDiscuss.value = true;
  submitted.value = false;
  discussProject.value = {
    discuss_project_id: -1,
    project_id: props.id,
    discuss_project_content: null,
    is_public: true,
    start_date: new Date(),
    end_date: null,
    is_order: listProjectMainDiscuss.value.length + 1,
    members: [],
  };
  issaveDiscuss.value = false;
  headerAddDiscuss.value = str;
  displayDiscuss.value = true;
};

const DelDiscuss = (model) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá thảo luận dự án này không!",
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
        var listId = [];
        axios
          .delete(baseURL + "/api/ProjectMain/Delete_DiscussProjectMain", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: model != null ? [model.discuss_project_id] : listId,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá thảo luận dự án thành công!");
              loadListDiscuss();
            } else {
              swal.fire({
                title: "Thông báo!",
                html: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
            if (error.status === 401) {
              swal.fire({
                title: "Thông báo!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

const editDiscuss = (model) => {
  isAddDiscuss.value = false;
  issaveDiscuss.value = false;
  headerAddDiscuss.value = "Sửa thảo luận";
  displayDiscuss.value = true;
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "discuss_project_get_edit",
            par: [{ par: "discuss_project_id", va: model.discuss_project_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      data[0].forEach((element, i) => {
        element.Thanhviens = element.Thanhviens
          ? JSON.parse(element.Thanhviens)
          : [];
        element.members = [];
      });
      discussProject.value = data[0][0];
      discussProject.value.start_date = discussProject.value.start_date
        ? new Date(discussProject.value.start_date)
        : null;
      discussProject.value.end_date = discussProject.value.end_date
        ? new Date(discussProject.value.end_date)
        : null;
      if (discussProject.value.Thanhviens.length > 0) {
        discussProject.value.Thanhviens.forEach(function (t) {
          discussProject.value.members.push(t.user_id);
        });
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "LogsView.vue",
        log_content: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const saveDiscussProjectMain = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  discussProject.value.start_date = discussProject.value.start_date
    ? new Date(discussProject.value.start_date)
    : null;
  discussProject.value.end_date = discussProject.value.end_date
    ? new Date(discussProject.value.end_date)
    : null;
  let formData = new FormData();
  if (discussProject.value.members.length > 0) {
    discussProject.value.members.forEach((t) => {
      let member = {
        discuss_member_id: -1,
        discuss_project_id: null,
        user_id: t,
        status: true,
      };
      member.user_id = t;
      DiscussMembers.value.push(member);
    });
  }
  formData.append("discussProject", JSON.stringify(discussProject.value));
  formData.append("discussMember", JSON.stringify(DiscussMembers.value));
  axios
    .post(
      baseURL +
        "/api/ProjectMain/" +
        (isAddDiscuss.value
          ? "Add_DiscussProjectMain"
          : "Update_DiscussProjectMain"),
      formData,
      config,
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật thảo luận dự án thành công!");
        loadData();
        displayDiscuss.value = false;
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
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};

const listFileComment = ref([]);
const ID_Comment = ref();
const editComment = ref(false);
const reply = ref(false);

const sending = ref(false);
const addComment = () => {
  if (
    (comment.value == "" ||
      comment.value == null ||
      comment.value == "<p><br></p>" ||
      comment.value == "<body><p><br></p></body>") &&
    listFileComment.value.length == 0
  ) {
    return;
  } else {
    let formData = new FormData();
    let bugComment = {
      contents: "<body>" + comment.value + "</body>",
    };
    if (listSendFile.value != null) {
      for (var i = 0; i < listSendFile.value.length; i++) {
        let file = listSendFile.value[i];
        formData.append("url_file", file);
      }
    }
    formData.append("discuss", JSON.stringify(bugComment));
    formData.append(
      "discuss_project_id",
      JSON.stringify(selectedDiscussProjectID.value),
    );
    formData.append(
      "is_reply",
      JSON.stringify(reply.value == true ? true : false),
    );
    if (reply.value == true) {
      formData.append(
        "parent_id",
        JSON.stringify(replyCmtValue.value[0].discuss_id),
      );
    }
    if (editComment.value == true) {
      formData.append("cmt_id", JSON.stringify(ID_Comment.value));
      formData.append(
        "Del_file_ID",
        JSON.stringify(listIdFileEditComments_Del.value),
      );
    }
    if (sending.value == true) {
      return;
    }
    sending.value = true;
    axios({
      method: editComment.value ? "put" : "post",
      url:
        baseURL +
        `/api/ProjectMain/${
          editComment.value ? "updateComments" : "add_Comments"
        }`,
      data: formData,
      headers: {
        Authorization: `Bearer ${store.getters.token}`,
      },
    })
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success(
            editComment.value
              ? "Cập nhật bình luận công việc thành công!"
              : "Thêm mới bình luận công việc thành công!",
          );
          comment.value = "";
          comment_zone_main.value.setHTML("<p>" + comment.value + "</p>");
          filecoments = [];
          listFileComment.value = [];
          listSendFile.value = [];
          editComment.value = false;
          sending.value = false;
          reply.value = false;
          loadDiscuss();
        } else {
          swal.fire({
            title: "Error!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
          sending.value = false;
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

const chonanh = (id) => {
  document.getElementById(id).value = "";
  document.getElementById(id).click();
};

const Change = (event) => {
  line = event.range.index ? event.range.index : null;
};

const onRowSelect = (id) => {
  selectedDiscussProjectID.value = id.discuss_project_id;
  selectedDiscussProjectContent.value = id.discuss_project_content;
  loadDiscuss();
};

const onRowUnselect = (id) => {};

const formatSize = (bytes) => {
  if (bytes === 0) {
    return "0 B";
  }

  let k = 1024,
    dm = 1,
    sizes = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"],
    i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
};

const listSendFile = ref([]);
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
const panel_file = ref();
const filefilefile = ref();
const file_Created = ref();
const FileAction = ref([
  {
    label: "Tải xuống tệp",
    icon: "pi pi-download",
    command: () => {
      download(filefilefile.value);
    },
  },
]);
const FileActionUploader = ref([
  {
    label: "Tải xuống tệp",
    icon: "pi pi-download",
    command: () => {
      download(filefilefile.value);
    },
  },
  {
    label: "Xóa tệp",
    icon: "pi pi-trash",
    command: (event) => {
      DelFile(filefilefile.value);
    },
  },
]);
const toggle_panel_file = (event, fileSelected, created) => {
  panel_file.value.toggle(event);
  filefilefile.value = fileSelected;
  file_Created.value = created;
};

const download = (file) => {
  panel_file.value.hide();
  var name = file.file_name || "file_download";
  const a = document.createElement("a");
  a.href =
    basedomainURL +
    "/Viewer/DownloadFile?url=" +
    file.file_path +
    "&title=" +
    name;
  a.download = name;

  a.click();
  a.remove();
};

const DelFile = (file) => {
  let id = [];
  id.push(file.file_id);
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá tệp tài liệu này không!",
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
          .delete(baseURL + "/api/ProjectMain/delete_Discuss_File", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: id,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá tệp tài liệu thành công!");
              loadDiscuss();
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
                title: "Error!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
  panel_file.value.hide();
};

const delImgComment = (value, index) => {
  if (editComment.value == true && value.data) {
    listIdFileEditComments_Del.value.push(value.data.file_id);
  }
  listSendFile.value.splice(index, 1);
  listFileComment.value = listFileComment.value.filter((x) => x.data != value);
  listFileComment.value = listFileComment.value.filter((x) => x != value);
};

const DelComment = (model) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá thảo luận này không!",
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
        var listId = [];
        axios
          .delete(baseURL + "/api/ProjectMain/Delete_Discuss", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: model != null ? [model] : listId,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá thảo luận thành công!");
              loadDiscuss();
            } else {
              swal.fire({
                title: "Thông báo!",
                html: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          })
          .catch((error) => {
            swal.close();
            if (error.status === 401) {
              swal.fire({
                title: "Thông báo!",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};

onMounted(() => {
  selectedDiscussProjectID.value = null;
  loadData();
  return {};
});
</script>
<template>
  <div class="tab-project-content h-full w-full col-md-12 p-0 m-0 flex">
    <div class="p-0 m-0 tab-project-content-left col-6">
      <div class="row">
        <div
          class="col-12"
          style="border-bottom: 1px solid #aaa; font-weight: 600; padding: 10px"
        >
          <Button
            label="Tạo thảo luận"
            icon="pi pi-plus"
            class="mr-2"
            @click="addDiscuss('Thêm mới thảo luận')"
          />
        </div>
        <div class="col-12">
          <DataTable
            id="projectmain-thaoluan"
            v-model:first="first"
            :rowHover="true"
            :value="listProjectMainDiscuss"
            :paginator="true"
            :rows="opition.PageSize"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
            :rowsPerPageOptions="[1, 20, 30, 50, 100, 200]"
            :scrollable="true"
            scrollHeight="flex"
            :totalRecords="opition.totalRecords"
            :row-hover="true"
            dataKey="discuss_project_id"
            v-model:selection="selectedProjectMains"
            @page="onPage($event)"
            @sort="onSort($event)"
            @filter="onFilter($event)"
            :lazy="true"
            selectionMode="single"
            @rowSelect="onRowSelect($event.data)"
            @rowUnselect="onRowUnselect($event.data)"
          >
            <Column
              field="STT"
              header="STT"
              class="align-items-center justify-content-center text-center font-bold"
              headerStyle="text-align:center;max-width:4rem"
              bodyStyle="text-align:center;max-width:4rem"
            >
            </Column>
            <Column
              field=""
              header="Người tạo"
              class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;max-width:100px"
              bodyStyle="text-align:center;max-width:100px"
            >
              <template #body="md">
                <Avatar
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                  v-tooltip.bottom="{
                    value:
                      md.data.full_name +
                      '<br/>' +
                      (md.data.tenChucVu || '') +
                      '<br/>' +
                      (md.data.tenToChuc || ''),
                    escape: true,
                  }"
                  v-bind:label="
                    md.data.avatar
                      ? ''
                      : (md.data.last_name ?? '').substring(0, 1)
                  "
                  v-bind:image="basedomainURL + md.data.avatar"
                  style="
                    background-color: #2196f3;
                    color: #ffffff;
                    width: 32px;
                    height: 32px;
                    font-size: 15px !important;
                    margin-left: -10px;
                  "
                  :style="{
                    background: bgColor[md.data.STT % 7] + '!important',
                  }"
                  class="cursor-pointer"
                  size="xlarge"
                  shape="circle"
                />
              </template>
            </Column>
            <Column
              field="discuss_project_content"
              header="Nội dung thảo luận"
              class="align-items-left justify-content-left text-left"
              headerStyle="text-align:left;max-width:auto;display: flex;align-items: left !important;"
              bodyStyle="text-align:left;max-width:auto"
            >
            </Column>
            <Column
              field=""
              header=""
              class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;max-width:100px"
              bodyStyle="text-align:center;max-width:100px;"
            >
              <template #body="data">
                <div v-if="data.data.is_public == true">
                  <span
                    style="
                      border: #2196f3;
                      background-color: #2196f3;
                      color: #ffffff;
                      padding: 5px;
                      border-radius: 5px;
                    "
                    >is public</span
                  >
                </div>
                <div
                  v-if="data.data.is_public == false"
                  style="display: flex; justify-content: center"
                >
                  <AvatarGroup>
                    <div
                      v-for="(value, index) in data.data.ThanhvienShows"
                      :key="index"
                    >
                      <div>
                        <Avatar
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                          v-tooltip.bottom="{
                            value:
                              value.fullName +
                              '<br/>' +
                              (value.tenChucVu || '') +
                              '<br/>' +
                              (value.tenToChuc || ''),
                            escape: true,
                          }"
                          v-bind:label="
                            value.avatar
                              ? ''
                              : (value.ten ?? '').substring(0, 1)
                          "
                          v-bind:image="basedomainURL + value.avatar"
                          style="
                            background-color: #2196f3;
                            color: #ffffff;
                            width: 32px;
                            height: 32px;
                            font-size: 15px !important;
                            margin-left: -10px;
                          "
                          :style="{
                            background: bgColor[index % 7] + '!important',
                          }"
                          class="cursor-pointer"
                          size="xlarge"
                          shape="circle"
                        />
                      </div>
                    </div>
                    <Avatar
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                      v-if="
                        data.data.Thanhviens.length -
                          data.data.ThanhvienShows.length >
                        0
                      "
                      :label="
                        '+' +
                        (data.data.Thanhviens.length -
                          data.data.ThanhvienShows.length) +
                        ''
                      "
                      class="cursor-pointer"
                      shape="circle"
                      style="
                        background-color: #e9e9e9 !important;
                        color: #98a9bc;
                        font-size: 14px !important;
                        width: 32px;
                        margin-left: -10px;
                        height: 32px;
                      "
                    />
                  </AvatarGroup>
                </div>
              </template>
            </Column>
            <Column
              class="align-items-center justify-content-center text-center"
              headerStyle="text-align:center;min-height:3.125rem;max-width:100px;"
              bodyStyle="text-align:center;max-width:100px;"
            >
              <template #body="data">
                <div
                  v-if="
                    store.state.user.is_super == true ||
                    store.state.user.user_id == data.data.created_by ||
                    data.data.isEdit == true ||
                    (store.state.user.role_id == 'admin' &&
                      store.state.user.organization_id ==
                        data.data.organization_id)
                  "
                >
                  <Button
                    @click="editDiscuss(data.data)"
                    class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                    type="button"
                    icon="pi pi-pencil"
                    v-tooltip="'Sửa'"
                  ></Button>
                  <Button
                    @click="DelDiscuss(data.data)"
                    class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                    type="button"
                    icon="pi pi-trash"
                    v-tooltip="'Xóa'"
                  ></Button>
                </div>
              </template>
            </Column>
            <template #empty>
              <div
                class="align-items-center justify-content-center p-4 text-center m-auto"
                style="
                  min-height: calc(100vh - 215px);
                  max-height: calc(100vh - 215px);
                  display: flex;
                  flex-direction: column;
                "
                v-if="listProjectMainDiscuss != null"
              >
                <img
                  src="../../assets/background/nodata.png"
                  height="144"
                />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </template>
          </DataTable>
        </div>
      </div>
    </div>
    <div
      class="col-6 p-0 m-0 tab-project-content-right"
      style="position: relative"
    >
      <div
        v-if="selectedDiscussProjectID"
        class="row"
        style="font-size: 13px; height: 85%; width: 98%"
      >
        <div
          class="row col-12"
          id="comments"
        >
          <div
            class="col-12 p-0 m-0"
            style="font-weight: 600; color: #888; font-size: 1.15rem"
          >
            <div class="col-12">
              <i
                class="pi pi-comments pr-2"
                style="font-size: 0.9rem !important"
              >
              </i>
              <span
                >Thảo luận về nội dung: ({{
                  selectedDiscussProjectContent
                }})</span
              >
            </div>
          </div>

          <div
            class="col-12 w-full scroll-outer"
            style="
              overflow: hidden auto;
              max-height: calc(100vh - 190px);
              min-height: calc(100vh - 190px);
            "
            v-if="listComments != null"
          >
            <div
              class="row col-12 pl-4 w-full cmt-hover relative scroll-inner discuss-element"
              style="padding-left: 0px !important"
              v-for="(cmt, index) in listComments"
              :key="index"
              :id="
                index == listComments.length - 1
                  ? 'comment_final'
                  : 'comment_' + index
              "
              ref="index"
            >
              <div
                class="right-0 absolute delete-button-hover"
                style="display: none"
                v-if="store.state.user.user_id == cmt.created_by"
              >
                <Button
                  icon="pi pi-pencil"
                  class="p-button-raised2 p-button-text"
                  @click="EditComment(cmt)"
                />
                <Button
                  icon=" pi pi-trash"
                  class="p-button-raised2 p-button-text"
                  @click="DelComment(cmt.discuss_id)"
                />
              </div>
              <div class="col-12 p-0 m-0 pb-2">
                <!-- delete-button-hover -->
                <div class="col-12 flex">
                  <div class="format-center">
                    <Avatar
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                      v-tooltip="{
                        value: cmt.tooltip,
                        escape: true,
                      }"
                      v-bind:label="
                        cmt.avatar
                          ? ''
                          : cmt.full_name.split(' ').at(-1).substring(0, 1)
                      "
                      v-bind:image="basedomainURL + cmt.avatar"
                      style="color: #ffffff; cursor: pointer"
                      :style="{
                        background: bgColor[index % 7],
                        border: '1px solid' + bgColor[index % 10],
                      }"
                      class="myTextAvatar p-0 m-0"
                      size="small"
                      shape="circle"
                    />
                  </div>
                  <div class="col-10 format-left">
                    <span
                      style="font-weight: 700; font-size: 16px; color: #385898"
                      >{{ cmt.full_name }}</span
                    >
                    <span class="ml-2">
                      {{
                        moment(new Date(cmt.created_date)).format(
                          "HH:mm DD/MM/YYYY",
                        )
                      }}
                    </span>
                  </div>
                </div>

                <div
                  class="row col-12 p-0 m-0 flex"
                  v-if="
                    (cmt.contents != null &&
                      cmt.contents != '<body><p><br></p></body>' &&
                      cmt.contents != '<body></body>') ||
                    cmt.children != null
                  "
                >
                  <div class="col-1"></div>
                  <div
                    class="pl-4 p-0 m-0 pr-4 bg-cmt-color border-1 border-round border-blue-100"
                    :class="cmt.parent != null ? 'w-full' : ''"
                  >
                    <div
                      class="w-full pl-4 p-0 m-0 pr-4 bg-reply border-bottom-comment"
                      style="padding-top: 5px !important"
                      v-if="cmt.parent != null"
                    >
                      <div class="col-12 p-0 m-0">
                        <!-- delete-button-hover -->
                        <div class="col-12 flex p-0 m-0">
                          <div class="format-center">
                            <Avatar
                              @error="
                                $event.target.src =
                                  basedomainURL + '/Portals/Image/nouser1.png'
                              "
                              v-tooltip="{
                                value: cmt.parent.tooltip,
                                escape: true,
                              }"
                              v-bind:label="
                                cmt.parent.avatar
                                  ? ''
                                  : cmt.parent.full_name
                                      .split(' ')
                                      .at(-1)
                                      .substring(0, 1)
                              "
                              v-bind:image="basedomainURL + cmt.parent.avatar"
                              style="color: #ffffff; cursor: pointer"
                              :style="{
                                background: bgColor[index % 7],
                                border: '1px solid' + bgColor[index % 10],
                              }"
                              class="myTextAvatar p-0 m-0"
                              size="small"
                              shape="circle"
                            />
                          </div>
                          <div class="col-10 format-left p-0 m-0">
                            <span
                              class="ml-2"
                              style="
                                font-weight: 700;
                                font-size: 16px;
                                color: #385898;
                              "
                              >{{ cmt.parent.full_name }}</span
                            >
                            <span class="ml-2">
                              {{
                                moment(
                                  new Date(cmt.parent.created_date),
                                ).format("HH:mm DD/MM/YYYY")
                              }}
                            </span>
                          </div>
                        </div>
                        <div
                          class="row col-12 flex p-0 m-0"
                          v-if="
                            cmt.parent.contents != null &&
                            cmt.parent.contents != '<body></body>'
                          "
                        >
                          <div class="col-1 p-0 m-0 text-3xl right-0"></div>

                          <div
                            class="pl-4 p-0 m-0 pr-4 flex"
                            style="
                              white-space: nowrap;
                              overflow: hidden;
                              display: -webkit-box;
                              -webkit-line-clamp: 3;
                              -webkit-box-orient: vertical;
                              text-overflow: ellipsis;
                            "
                          >
                            <font-awesome-icon
                              icon="fa-solid fa-quote-left"
                            /><span v-html="cmt.parent.contents"></span
                            ><font-awesome-icon
                              icon="fa-solid fa-quote-right"
                            />
                          </div>
                          <div class="col-1 p-0 m-0"></div>
                        </div>
                      </div>
                    </div>
                    <div
                      class="pl-4 p-0 m-0 pr-4"
                      v-html="cmt.contents"
                    ></div>
                  </div>

                  <div class="col-1"></div>
                </div>
                <div
                  class="row col-12 flex p-0 m-0 pt-2"
                  v-if="cmt.files != null"
                >
                  <div class="col-1"></div>
                  <div
                    class="col-10 p-0 m-0 bg-white-100 border-1 border-round border-blue-100"
                  >
                    <div class="col-12 flex flex-wrap">
                      <div
                        v-for="(slotProps, index) in cmt.files"
                        :key="index"
                        class="col-4 py-0 mb-0 h-full relative div-menu-file-hover"
                        v-on:dblclick="ViewFileInfo(slotProps)"
                        v-tooltip.top="{
                          value: 'Nháy chuột 2 lần để xem chi tiết',
                        }"
                      >
                        <div class="absolute right-0 top-0 div-menu-file">
                          <Button
                            icon="pi pi-ellipsis-h"
                            class="p-button-hover-file-menu p-button-text"
                            v-tooltip="{ value: '' }"
                            @click="
                              toggle_panel_file(
                                $event,
                                slotProps,
                                cmt.created_by,
                              )
                            "
                            aria-haspopup="true"
                            aria-controls="overlay_panel"
                          />
                        </div>
                        <div
                          class="col-12 p-0 m-0 py-2 format-default file-hover file-comments"
                          style="height: 8rem"
                        >
                          <div
                            class="col-12 p-0 m-0"
                            style="
                              display: flex;
                              flex-direction: column;
                              align-items: center;
                            "
                          >
                            <Image
                              :src="basedomainURL + slotProps.file_path"
                              :alt="slotProps.file_name"
                              preview
                              :imageStyle="'max-width: 50px; max-height: 50px; margin-top:5px'"
                              v-if="slotProps.is_image == 1"
                              style="
                                white-space: nowrap;
                                overflow: hidden;
                                text-overflow: ellipsis;
                              "
                            />
                            <img
                              v-else
                              :src="
                                basedomainURL +
                                '/Portals/Image/file/' +
                                slotProps.file_type +
                                '.png'
                                  ? basedomainURL +
                                    '/Portals/Image/file/' +
                                    slotProps.file_type +
                                    '.png'
                                  : basedomainURL +
                                    '/Portals/Image/file/iconga.png'
                              "
                              style="
                                width: 50px;
                                height: 50px;
                                object-fit: contain;
                                margintop: 5px;
                              "
                              :alt="slotProps.file_name"
                            />
                            <div
                              class="col-12 py-2 px-3 format-center file-comments-hover"
                              style="
                                overflow: hidden;
                                text-overflow: ellipsis;
                                display: block;
                                white-space: nowrap;
                              "
                            >
                              <span
                                class=""
                                v-tooltip.top="{
                                  value: slotProps.file_name,
                                }"
                              >
                                {{ " " + slotProps.file_name }}
                              </span>
                            </div>
                            <div class="col-12 p-0 m-0 format-center">
                              {{ slotProps.file_size }}
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-1"></div>
                </div>
                <div class="row col-12 flex p-0 m-0 pt-1">
                  <div class="col-1"></div>
                  <div class="col-3 p-0 m-0 format-left">
                    <Button
                      label="Trả lời"
                      icon="pi pi-reply"
                      class="p-button-text reply"
                      @click="ReplyComment(cmt)"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div
        class="row"
        style="padding: 15px; font-size: 13px; height: 15%"
      >
        <div
          class="grid formgrid m-2"
          style="position: relative"
        >
          <div
            v-if="selectedDiscussProjectID"
            class="field col-12 md:col-12 absolute border-1 format-center col-12 p-0 m-0 border-round-xs border-600"
            style="border-radius: 5px"
          >
            <div class="border-0 col-9 p-0 m-0">
              <QuillEditor
                ref="comment_zone_main"
                placeholder="Nhập nội dung thảo luận..."
                contentType="html"
                :content="comment"
                v-model:content="comment"
                theme="bubble"
                @selectionChange="Change($event)"
                style="height: 5rem"
                @keydown.enter.exact.prevent="addComment()"
              />
            </div>
            <div class="col-3 p-0 m-0">
              <div class="format-center flex col-12 p-0 m-0 h-full">
                <!-- v-clickoutside="onHideEmoji" -->

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
                  @click="chonanh('filediscuss')"
                  v-tooltip="{ value: 'Đính kèm tệp' }"
                >
                </Button>
                <Button
                  icon="pi pi-send pt-1 pr-0 font-bold"
                  class="p-button-text p-button-plain col-3 w-3rem h-3rem"
                  style="background-color: ; color: black"
                  @click="addComment()"
                  v-tooltip="{ value: 'Gửi bình luận' }"
                />
                <input
                  class="hidden"
                  id="filediscuss"
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

      <div
        class="row discuss-file-send"
        v-if="listFileComment.length > 0"
        style="position: absolute; bottom: 100px; width: 100%; display: flex"
      >
        <div
          class="col-12"
          style="background-color: #f5f5f5"
        >
          <div
            class="row"
            style="display: flex"
          >
            <div
              v-for="(item, index) in listFileComment"
              :key="index"
              class="col-2 relative format-center p-1"
              style=""
            >
              <div class="col-2 p-0 m-0 anh format-center file-hover">
                <Button
                  @click="delImgComment(item.data ? item.data : item, index)"
                  icon="pi pi-times-circle"
                  class="p-button-rounded p-button-danger p-button-text p-button-plain absolute top-0 right-0 pr-0 mr-0 p-button-hover"
                  v-tooltip="{ value: 'Xóa tệp' }"
                ></Button>

                <div
                  class=""
                  v-if="item.checkimg == true"
                >
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
                <div
                  class=""
                  v-else
                >
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

      <div
        class="row discuss-repley-content"
        v-if="replyCmtValue"
        style="
          position: absolute;
          bottom: 100px;
          background-color: #f5f5f5;
          width: 100%;
        "
      >
        <div class="col-12 w-full scroll-outer">
          <div
            class="row col-12 pl-4 w-full cmt-hover relative scroll-inner discuss-element"
            style="padding-left: 0px !important"
            ref="index"
          >
            <div class="right-0 absolute delete-button-hover">
              <Button
                icon=" pi pi-times"
                class="p-button-raised2 p-button-text"
                @click="DelReplyComment()"
              />
            </div>
            <div class="col-12 p-0 m-0 pb-2">
              <!-- delete-button-hover -->
              <div class="col-12 flex">
                <div class="format-center">
                  <Avatar
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/nouser1.png'
                    "
                    v-tooltip="{
                      value: replyCmtValue[0].tooltip,
                      escape: true,
                    }"
                    v-bind:label="
                      replyCmtValue[0].avatar
                        ? ''
                        : replyCmtValue[0].full_name
                            .split(' ')
                            .at(-1)
                            .substring(0, 1)
                    "
                    v-bind:image="basedomainURL + replyCmtValue[0].avatar"
                    style="color: #ffffff; cursor: pointer"
                    :style="{
                      background: bgColor[1],
                      border: '1px solid' + bgColor[1],
                    }"
                    class="myTextAvatar p-0 m-0"
                    size="small"
                    shape="circle"
                  />
                </div>
                <div class="col-10 format-left">
                  <span
                    style="font-weight: 700; font-size: 16px; color: #385898"
                    >{{ replyCmtValue[0].full_name }}</span
                  >
                  <span class="ml-2">
                    {{
                      moment(new Date(replyCmtValue[0].created_date)).format(
                        "HH:mm DD/MM/YYYY",
                      )
                    }}
                  </span>
                </div>
              </div>

              <div
                class="row col-12 p-0 m-0 flex"
                v-if="
                  (replyCmtValue[0].contents != null &&
                    replyCmtValue[0].contents != '<body><p><br></p></body>' &&
                    replyCmtValue[0].contents != '<body></body>') ||
                  replyCmtValue[0].children != null
                "
              >
                <div class="col-1"></div>
                <div
                  class="pl-4 p-0 m-0 pr-4 bg-cmt-color border-1 border-round border-blue-100"
                  :class="replyCmtValue[0].parent != null ? 'w-full' : ''"
                >
                  <div
                    class="w-full pl-4 p-0 m-0 pr-4 bg-reply border-bottom-comment"
                    style="padding-top: 5px !important"
                    v-if="replyCmtValue[0].parent != null"
                  >
                    <div class="col-12 p-0 m-0">
                      <!-- delete-button-hover -->
                      <div class="col-12 flex p-0 m-0">
                        <div class="format-center">
                          <Avatar
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                            v-tooltip="{
                              value: replyCmtValue[0].parent.tooltip,
                              escape: true,
                            }"
                            v-bind:label="
                              replyCmtValue[0].parent.avatar
                                ? ''
                                : replyCmtValue[0].parent.full_name
                                    .split(' ')
                                    .at(-1)
                                    .substring(0, 1)
                            "
                            v-bind:image="
                              basedomainURL + replyCmtValue[0].parent.avatar
                            "
                            style="color: #ffffff; cursor: pointer"
                            :style="{
                              background: bgColor[2],
                              border: '1px solid' + bgColor[2],
                            }"
                            class="myTextAvatar p-0 m-0"
                            size="small"
                            shape="circle"
                          />
                        </div>
                        <div class="col-10 format-left p-0 m-0">
                          <span
                            class="ml-2"
                            style="
                              font-weight: 700;
                              font-size: 16px;
                              color: #385898;
                            "
                            >{{ replyCmtValue[0].parent.full_name }}</span
                          >
                          <span class="ml-2">
                            {{
                              moment(
                                new Date(replyCmtValue[0].parent.created_date),
                              ).format("HH:mm DD/MM/YYYY")
                            }}
                          </span>
                        </div>
                      </div>
                      <div
                        class="row col-12 flex p-0 m-0"
                        v-if="
                          replyCmtValue[0].parent.contents != null &&
                          replyCmtValue[0].parent.contents != '<body></body>'
                        "
                      >
                        <div class="col-1 p-0 m-0 text-3xl right-0"></div>

                        <div
                          class="pl-4 p-0 m-0 pr-4 flex"
                          style="
                            white-space: nowrap;
                            overflow: hidden;
                            display: -webkit-box;
                            -webkit-line-clamp: 3;
                            -webkit-box-orient: vertical;
                            text-overflow: ellipsis;
                          "
                        >
                          <font-awesome-icon
                            icon="fa-solid fa-quote-left"
                          /><span
                            v-html="replyCmtValue[0].parent.contents"
                          ></span
                          ><font-awesome-icon icon="fa-solid fa-quote-right" />
                        </div>
                        <div class="col-1 p-0 m-0"></div>
                      </div>
                    </div>
                  </div>
                  <div
                    class="pl-4 p-0 m-0 pr-4"
                    v-html="replyCmtValue[0].contents"
                  ></div>
                </div>

                <div class="col-1"></div>
              </div>
              <div
                class="row col-12 flex p-0 m-0 pt-2"
                v-if="replyCmtValue[0].files != null"
              >
                <div class="col-1"></div>
                <div
                  class="col-10 p-0 m-0 bg-white-100 border-1 border-round border-blue-100"
                >
                  <div class="col-12 flex flex-wrap">
                    <div
                      v-for="(slotProps, index) in replyCmtValue[0].files"
                      :key="index"
                      class="col-4 py-0 mb-0 h-full relative div-menu-file-hover"
                      v-on:dblclick="ViewFileInfo(slotProps)"
                      v-tooltip.top="{
                        value: 'Nháy chuột 2 lần để xem chi tiết',
                      }"
                    >
                      <div class="absolute right-0 top-0 div-menu-file">
                        <Button
                          icon="pi pi-ellipsis-h"
                          class="p-button-hover-file-menu p-button-text"
                          v-tooltip="{ value: '' }"
                          @click="
                            toggle_panel_file(
                              $event,
                              slotProps,
                              replyCmtValue[0].created_by,
                            )
                          "
                          aria-haspopup="true"
                          aria-controls="overlay_panel"
                        />
                      </div>
                      <div
                        class="col-12 p-0 m-0 py-2 format-default file-hover file-comments"
                        style="height: 8rem"
                      >
                        <div
                          class="col-12 p-0 m-0"
                          style="
                            display: flex;
                            flex-direction: column;
                            align-items: center;
                          "
                        >
                          <Image
                            :src="basedomainURL + slotProps.file_path"
                            :alt="slotProps.file_name"
                            preview
                            :imageStyle="'max-width: 50px; max-height: 50px; margin-top:5px'"
                            v-if="slotProps.is_image == 1"
                            style="
                              white-space: nowrap;
                              overflow: hidden;
                              text-overflow: ellipsis;
                            "
                          />
                          <img
                            v-else
                            :src="
                              basedomainURL +
                              '/Portals/Image/file/' +
                              slotProps.file_type +
                              '.png'
                                ? basedomainURL +
                                  '/Portals/Image/file/' +
                                  slotProps.file_type +
                                  '.png'
                                : basedomainURL +
                                  '/Portals/Image/file/iconga.png'
                            "
                            style="
                              width: 50px;
                              height: 50px;
                              object-fit: contain;
                              margintop: 5px;
                            "
                            :alt="slotProps.file_name"
                          />
                          <div
                            class="col-12 py-2 px-3 format-center file-comments-hover"
                            style="
                              overflow: hidden;
                              text-overflow: ellipsis;
                              display: block;
                              white-space: nowrap;
                            "
                          >
                            <span
                              class=""
                              v-tooltip.top="{
                                value: slotProps.file_name,
                              }"
                            >
                              {{ " " + slotProps.file_name }}
                            </span>
                          </div>
                          <div class="col-12 p-0 m-0 format-center">
                            {{ slotProps.file_size }}
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div class="col-1"></div>
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
    <VuemojiPicker @emojiClick="handleEmojiClick" />
  </OverlayPanel>
  <Menu
    :model="user.user_id == file_Created ? FileActionUploader : FileAction"
    ref="panel_file"
    :popup="true"
    id="overlay_panel"
  >
    <template #item="{ item }">
      <a
        download
        style="text-decoration: none"
        class="a-hover format-center"
      >
        <Button
          :icon="item.icon"
          class="w-full p-button-text p-button-secondary p-button-hover-file"
          :label="item.label"
          @click="item.command"
        >
        </Button>
      </a>
    </template>
  </Menu>
  <FileInfoVue
    :data="fileInfo"
    v-if="isViewFileInfo"
  ></FileInfoVue>
  <Dialog
    :header="headerAddDiscuss"
    v-model:visible="displayDiscuss"
    :style="{ width: '40vw' }"
    :closable="true"
    :maximizable="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0"
            >Nội dung thảo luận<span class="redsao"> (*) </span></label
          >
          <InputText
            v-model="discussProject.discuss_project_content"
            spellcheck="false"
            class="col-9 ip36 px-2"
            :class="{
              'p-invalid': v3$.discuss_project_content.$invalid && submitted,
            }"
          />
        </div>
        <div
          style="display: flex"
          class="field col-12 md:col-12"
        >
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (v3$.discuss_project_content.$invalid && submitted) ||
              v3$.discuss_project_content.$pending.$response
            "
            class="col-9 p-error p-0"
          >
            <span class="col-12 p-0">{{
              v3$.discuss_project_content.required.$message
                .replace("Value", "Nội dung thảo luận")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div
          class="field col-12 md:col-12"
          style="display: flex; align-items: center"
        >
          <label class="col-3 text-left p-0">Ngày bắt đầu</label>
          <div
            class="col-9"
            style="display: flex; padding: 0px; align-items: center"
          >
            <Calendar
              :manualInput="true"
              :showIcon="true"
              :showTime="true"
              class="col-5 ip36 title-lable"
              style="margin-top: 5px; padding: 0px"
              id="time1"
              autocomplete="on"
              v-model="discussProject.start_date"
            />
            <div
              class="col-7"
              style="display: flex; padding: 0px; align-items: center"
            >
              <label class="col-5 text-center">Ngày kết thúc</label>
              <Calendar
                :manualInput="true"
                :showTime="true"
                :showIcon="true"
                class="col-7 ip36 title-lable"
                style="margin-top: 5px; padding: 0px"
                id="time2"
                placeholder="dd/MM/yy HH:mm"
                autocomplete="on"
                v-model="discussProject.end_date"
                @date-select="CheckDate($event)"
              />
            </div>
          </div>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0">STT</label>
          <InputNumber
            v-model="discussProject.is_order"
            style="padding: 0px !important"
            class="col-9 ip36 px-2"
          />
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-3 text-left p-0">Public thảo luận</label>
          <div
            class="col-9"
            style="position: relative"
          >
            <InputSwitch
              class="col-12"
              style="position: absolute; top: 0px; left: 0px"
              v-model="discussProject.is_public"
            />
          </div>
        </div>
        <div
          class="field col-12 md:col-12"
          v-if="discussProject.is_public == false"
        >
          <label class="col-3 text-left p-0"
            >Người tham gia
            <span
              @click="OpenDialogTreeUser(false, 1)"
              class="choose-user"
              ><i class="pi pi-user-plus"></i></span
          ></label>
          <MultiSelect
            :filter="true"
            v-model="discussProject.members"
            :options="listDropdownMembers"
            optionValue="code"
            optionLabel="name"
            class="col-9 ip36 p-0"
            placeholder="Người tham gia"
            display="chip"
          >
            <template #option="slotProps">
              <div
                class="country-item flex"
                style="align-items: center; margin-left: 10px"
              >
                <Avatar
                  @error="
                    $event.target.src =
                      basedomainURL + '/Portals/Image/nouser1.png'
                  "
                  v-bind:label="
                    slotProps.option.avatar
                      ? ''
                      : (slotProps.option.name ?? '').substring(0, 1)
                  "
                  v-bind:image="basedomainURL + slotProps.option.avatar"
                  style="
                    background-color: #2196f3;
                    color: #ffffff;
                    width: 32px;
                    height: 32px;
                    font-size: 15px !important;
                    margin-left: -10px;
                  "
                  :style="{
                    background: bgColor[slotProps.index % 7] + '!important',
                  }"
                  class="cursor-pointer"
                  size="xlarge"
                  shape="circle"
                />
                <div
                  class="pt-1"
                  style="padding-left: 10px"
                >
                  {{ slotProps.option.name }}
                </div>
              </div>
            </template>
          </MultiSelect>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogProjectMain"
        class="p-button-text"
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveDiscussProjectMain(!v3$.discuss_project_content.$invalid)"
      />
    </template>
  </Dialog>
</template>
<style scoped>
.tab-project-content {
  height: calc(100vh - 50px) !important;
  background-color: #f3f3f3;
}

.tab-project-content-left {
  background-color: #fff;
  margin: 5px 5px 0px 0px !important;
  height: 100%;
}

.tab-project-content-right {
  background-color: #fff;
  margin: 5px 0px 5px 5px !important;
  height: 100%;
  width: 50%;
  animation: 0.5s;
}

#projectmain-thaoluan {
  max-height: calc(100vh - 110px);
  min-height: calc(100vh - 110px);
}

.discuss-element:hover {
  cursor: pointer;
  background-color: #f5f5f5;
}

.discuss-element:hover .delete-button-hover {
  display: block !important;
}

.discuss-element:hover .file-hover {
  background-color: #fff;
}

.file-hover {
  margin: 5px;
}

.discuss-element .file-hover:hover {
  background-color: #d8edff;
}
</style>
