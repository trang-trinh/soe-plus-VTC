<script setup>
//Khai báo InJect và Import (import)
import checklist from "./checklist.vue";
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { VuemojiPicker } from "vuemoji-picker";
import vi from "date-fns/locale/vi";
import moment from "moment";
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
    case "preListBug":
      showBugs(obj.data.task_id, task.value);
      isDetailsBug.value = false;
      break;
    case "goToNoti":
      id_noti.value = obj.data;
      break;
  }
});

const props = defineProps({
  isShowBug: Boolean,
  task: Object,
  project_id: Intl,
});
watch(props, () => {
  if (props.isShowBug == true) {
    showBugs(props.task.task_id, props.task);
    isShowBug.value = true;
    task.value = props.task;
    isDetailsBug.value = false;
    options.value.searchTextBug = null;
  }
});
//Khai báo biến (variable)
const fromView = ref("bug");
const id_noti = ref();
const Bug = ref({
  bug_name: "",
  created_by: "",
  created_date: null,
  des: "",
  keyword: "",
  url_file: "",
  status: 0,
  date_now: "",
});
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
const isShowBug = ref(false);
const task = ref();
const listBugSave = ref([]);
const ruleBug = {
  bug_name: {
    required,
    $errors: [
      {
        $property: "bug_name",
        $validator: "required",
        $message: "Tên lỗi không được để trống!",
      },
    ],
  },
};
const itemButBugsMores = ref([
  {
    label: "Sửa",
    icon: "pi pi-cog",
    command: () => {
      editBug(Bug.value);
    },
  },

  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: () => {
      deleteBug(Bug.value);
    },
  },
]);
const bugFilter = ref(0);
const listFilter = ref([
  { name: "Tất cả", code: 0 },
  { name: "Cá nhân", code: 1 },
]);
const isDetailsBug = ref(false);
const submitted = ref(false);
const validateBug = useVuelidate(ruleBug, bug);
const projectSelected = ref();
const isSaveBug = ref(false);
const isShowAddBug = ref(false);
const headerAddBug = ref("");

const listStatusBugs = ref([
  {
    name: "Đề xuất",
    code: -4,
    css: "p-button-raised p-button-danger",
  },
  {
    name: "Yêu cầu thêm",
    code: -3,
    css: "p-button-raised p-button-danger",
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

const listImportant = ref([
  {
    name: "Không quan trọng",
    code: 0,
    css: "p-button-raised p-button-secondary ",
  },
  { name: "Bình thường", code: 1, css: "p-button-raised" },

  { name: "Gấp", code: 2, css: "p-button-raised p-button-warning" },
  { name: "Rất gấp", code: 3, css: "p-button-raised p-button-danger" },
]);
const listGroupBugComment = ref([]);
const menuButBugsMores = ref();

//Hàm (Function)
const showCommentBug = (value) => {
  Bug.value = value;
  isDetailsBug.value = true;
};
let filebugs = [];
const onUploadFileBug = (event) => {
  if (event.files.length > 0) bug.value.url_file = "";
  event.files.forEach((element) => {
    filebugs.push(element);
  });
};
const removeFileBug = () => {
  filebugs = [];
};

const editBug = (value) => {
  isSaveBug.value = true;
  if (value.keyword != null && value.keyword.length > 1) {
    if (!Array.isArray(value.keyword)) {
      value.keyword = value.keyword.split(",");
    }
  }
  bug.value = value;
  submitted.value = false;
  headerAddBug.value = "Sửa yêu cầu";
  isShowAddBug.value = true;
};
const deleteFileBug = () => {
  bug.value.url_file = "";
};
const arrFiles = ref([]);
let filebugcmt = [];
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
const reloadBug = () => {
  options.value.searchTextBug = null;
  bugFilter.value = 0;
  showBugs(task.value.task_id, task.value);
};
const addBug = () => {
  submitted.value = false;
  headerAddBug.value = "Thêm yêu cầu";
  isSaveBug.value = false;
  bug.value = {
    project_id:
      projectSelected.value != null && projectSelected.value != "allPr"
        ? projectSelected.value
        : props.project_id,
    task_id: task.value.task_id,
    bug_name: "",
    des: "",
    status: -2,
    keyword: "",
    is_overtime:false,
    start_overtime:new Date
  };
  isShowAddBug.value = true;
};
const onNewVersion = () => {
  toast.info("Chức năng bạn chọn sẽ sớm có ở phiên bản mới!");
};
const loadTask = () => {
  emitter.emit("emitData", { type: "loadTask", data: null });
  emitter.emit("emitData", { type: "reloadViewTask", data: id_noti.value });
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
const toggleBugsMores = (event, u) => {
  Bug.value = u;
  menuButBugsMores.value.toggle(event);
};

const closeBug = () => {
  bug.value = {
    bug_name: "",
    des: "",
    status: 0,
    keyword: "",
  };
  isShowAddBug.value = false;
};
const saveBug = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }

  let formData = new FormData();
  for (var i = 0; i < filebugs.length; i++) {
    let file = filebugs[i];
    formData.append("url", file);
  }
  if (bug.value.des) bug.value.des = bug.value.des.replace("\n", "<br>");
  submitted.value = true;
  if (bug.value.keyword != null) {
    bug.value.keyword = bug.value.keyword.toString();
  }
  formData.append("bug", JSON.stringify(bug.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  if (!isSaveBug.value) {
    axios
      .post(baseURL + "/api/api_bug/Add_bug", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm lỗi thành công!");
          listGroupBugComment.value
            .filter((x) => x.active == false)
            .forEach((item) => {
              item.active = true;
              item.icon = "p-accordion-toggle-icon pi pi-chevron-right";
            });

          showBugs(bug.value.task_id, task.value);
          closeBug();
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
        console.log(error);
        swal.close();
        swal.fire({
          title: "Error!",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    axios
      .put(baseURL + "/api/api_bug/Update_bug", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Cập nhật lỗi thành công!");
          showBugs(bug.value.task_id, task.value);

          loadTask();
          closeBug();
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

const deleteBug = (value) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xóa lỗi này không!",
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
          .delete(baseURL + "/api/api_bug/Delete_bug", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: value.bug_id != null ? [value.bug_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá lỗi thành công!");
              showBugs(task.value.task_id, task.value);
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

const showBugs = (value, dataLe) => {
  task.value = dataLe;
  //  swal.fire({
  //         width: 110,
  //         didOpen: () => {
  //           swal.showLoading();
  //         },
  //       });

  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_bug_list",
        par: [
          { par: "task_id", va: value },
          { par: "search", va: options.value.searchTextBug },
          { par: "user_id", va: store.getters.user.user_id },
          { par: "type", va: bugFilter.value },
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
      if (dataLe) {
        if (dataLe.test_user_ids.includes(store.getters.user.user_id))
          hideNewAction(value, true, false);

        if (store.getters.user.user_id == dataLe.user_id)
          hideNewAction(value, true, true);
      }
      // swal.close();
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
const printBug = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "ReportTask",
        par: [{ par: "task_id", va: task.value.task_id }],
      },
      config
    )
    .then((response) => {
      let tables = JSON.parse(response.data.data);
      swal.close();
      let style = `<style>
        body{padding:20px;font-family:'arial'}table{border-collapse: collapse;width:100%}table,th,td,tr{border:1px solid #999}th{
        position: sticky;
        z-index: 1;
        background-color: #ccc;
        color: #333;
        font-weight: bold;
        top: 0;
      }th,td{padding:10px}
      .text-center{text-align:center}
      .des{color:#333;font-style: italic;}
      span.status-1 {color: blue;display:block}
      span.status-2 {color: brown;display:block}
      span.status-3 {color: green;text-decoration: line-through;display:block}
      .h1title{color: #0078d4;
    font-size: 16pt;
    text-transform: uppercase;}
      .mw{min-width:300px}
      td.bg{background-color:#eee}
      .ghichu{
            display: flex;
          text-align: center;
          justify-content: center;
          margin: 5px;
          font-size: 13px;
      }
      </style>`;
      let html = '<html><head><meta charset="UTF-8">' + style + "</head>";
      html += "<body>";
      html += `<h1 class='h1title text-center'>${tables[0][0].task_name}</h1>`;
      html += `<div class='text-center des'>${tables[0][0].full_name}   ${tables[0][0].created_date}</div>`;
      html += `<div class='text-center ghichu'>
            <span class="status-0">Kế hoạch</span>&nbsp;|&nbsp;
            <span class="status-1">Đang xử lý</span>&nbsp;|&nbsp;
            <span class="status-2">Đang Test</span>&nbsp;|&nbsp;
            <span class="status-3">Đã xử lý</span>
      </div>`;
      html += '<table style="margin-top:10px">';
      html += "<thead>";
      html += "<tr>";
      html += '<th class="text-center" width=50>STT</th>';
      html +=
        '<th class="text-center" width=180 style="min-width:150px">Nhân viên</th>';
      let days = tables[1][0].days.split(",");
      for (let i = 0; i < days.length; i++) {
        html += `<th class="text-center">${days[i]}</th>`;
      }
      html += "</tr>";
      html += "</thead>";
      html += "<tbody>";
      for (let i = 0; i < tables[2].length; i++) {
        let r = tables[2][i];
        html += "<tr>";
        html += `<td class="bg text-center" width=50>${i + 1}</td>`;
        html += `<td class="bg text-center" width=180><b>${r.full_name}</b></td>`;
        for (let j = 0; j < days.length; j++) {
          let str = "";
          if (r[days[j]]) {
            let arr = r[days[j]].split("&lt;br/&gt;");
            for (let k = 0; k < arr.length; k++) {
              if (arr[k]) {
                let flag = arr[k].substring(0, 1);
                str +=
                  "<span style='margin-top:5px;display:block' class='status-" +
                  flag +
                  "'>" +
                  (arr[k].substring(1).indexOf("-") == 0 ? "" : "- ") +
                  arr[k].substring(1) +
                  "</span>";
              }
            }
          }
          if (str != "") html += `<td class="mw">${str}</td>`;
          else html += `<td></td>`;
        }
        html += "</tr>";
      }
      html += "</tbody>";
      html += "</table>";
      html += "</body>";
      html += "</html>";
      var win = window.open(
        "",
        "Kế hoạch công việc",
        "toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=yes,resizable=yes"
      );
      win.document.body.innerHTML = html;
    })
    .catch((error) => {
      console.log(error);
    });
};
//Các Func của CheckList
onMounted(() => {
  return {};
});
</script>
<template>
  <Sidebar
    v-model:visible="isShowBug"
    :baseZIndex="100"
    position="right"
    :showCloseIcon="false"
    class="p-sidebar-lg py-0 overflow-hidden"
    @hide="loadTask()"
  >
    <div v-if="!isDetailsBug">
      <div
        class="surface-0 pb-2"
        style="
          position: -webkit-sticky;
          position: sticky;
          top: 0;
          z-index: 1000;
        "
      >
        <h2>{{ props.task.task_name }}</h2>
        <DataView
          class="w-full h-full e-sm flex flex-column"
          responsiveLayout="scroll"
          :scrollable="true"
          layout="list"
          :lazy="true"
          :value="listBugSave"
        >
          <template #header>
            <div>
              <Toolbar class="w-full custoolbar p-0">
                <template #start>
                  <h3 class="m-0 flex">
                    <img
                      src="/src/assets/image/iconbug.png"
                      alt=""
                      width="20"
                      height="20"
                      class="cursor-pointer"
                    />
                    <span class="ml-1">Danh sách Bug</span>
                    <span v-if="listBugSave.length > 0"
                      >({{ listBugSave.length }})</span
                    >
                  </h3>
                </template>
                <template #end> </template>
              </Toolbar>
              <Toolbar class="w-full custoolbar pt-5">
                <template #start>
                  <span class="p-input-icon-left">
                    <i class="pi pi-search" />
                    <InputText
                      type="text"
                      class="p-inputtext-sm"
                      spellcheck="false"
                      placeholder="Tìm kiếm"
                      v-model="options.searchTextBug"
                      @keyup.enter="showBugs(task.task_id, task)"
                    />
                  </span>

                  <div>
                    <Dropdown
                      v-model="bugFilter"
                      :options="listFilter"
                      optionLabel="name"
                      optionValue="code"
                      placeholder="Chọn dự án"
                      class="w-12rem mx-2"
                      @change="showBugs(task.task_id, task)"
                    />
                  </div>
                </template>

                <template #end>
                  <!-- <DataViewLayoutOptions v-model="layout" class="mx-2" /> -->
                  <Button
                    v-if="
                      props.task.test_user_ids.filter(
                        (x) => x == store.getters.user.user_id
                      ).length > 0 ||
                      store.getters.user.user_id == props.task.user_id
                    "
                    @click="addBug"
                    class="p-button-sm mr-2"
                    label="Thêm mới"
                  ></Button>
                  <Button
                    class="
                      mr-2
                      p-button-sm p-button-outlined p-button-secondary
                    "
                    @click="reloadBug"
                    icon="pi pi-refresh"
                  />
                  <Button
                    label="Tiện ích"
                    icon="pi pi-file-excel"
                    class="mr-2 p-button-outlined p-button-secondary"
                    aria-haspopup="true"
                    aria-controls="overlay_Export"
                    @click="printBug"
                  />
                  <Menu id="overlay_Export" ref="serviceButs" :popup="true" />
                </template>
              </Toolbar>
            </div>
          </template>

          <template #list="slotProps">
            <div class="w-full">
              <div class="flex align-items-center justify-content-center">
                <div
                  class="
                    flex flex-column flex-grow-1
                    surface-0
                    m-2
                    border-round-xs
                    pl-3
                    pt-3
                  "
                >
                  <div class="col-12 field flex p-0 m-0">
                    <div
                      @click="showCommentBug(slotProps.data)"
                      class="col-9 p-0 cursor-pointer"
                    >
                      <div
                        class="col-12 p-0 font-bold text-xl flex"
                        style="font-size: 1rem"
                      >
                        <!-- <div class="mb-1 font-bold text-xl pt-2">
                        <Checkbox v-model="bugChecked" :binary="true" />
                      </div> -->
                        <div class="mb-1 font-bold text-xl pt-2">
                          <Tag
                            icon="pi pi-hashtag"
                            style="background-color: black; color: white"
                          >
                            {{ slotProps.data.bug_id }}
                          </Tag>
                        </div>
                        <div class="mb-1 font-bold text-xl pt-2 pl-1">
                          <Tag
                            v-if="slotProps.data.is_important == 0"
                            severity="info"
                            >Không quan trọng</Tag
                          >
                          <Tag
                            v-if="slotProps.data.is_important == 1"
                            severity="success"
                            >Bình thường</Tag
                          >
                          <Tag
                            v-if="slotProps.data.is_important == 2"
                            severity="warning"
                            >Gấp</Tag
                          >
                          <Tag
                            v-if="slotProps.data.is_important == 3"
                            severity="danger"
                            >Rất gấp</Tag
                          >
                        </div>
                        <div
                          class="mb-1 font-bold text-xl px-2 pt-2 pl-1"
                          :class="
                            slotProps.data.status == 1
                              ? 'line-through text-green-600'
                              : ''
                          "
                        >
                          {{ slotProps.data.bug_name }}
                        </div>
                        <div
                          v-if="slotProps.data.status"
                          class="mb-1 font-bold text-xl px-1 pt-2"
                        >
                          <div class="mb-1 font-italic text-color-secondary">
                            <Tag
                              :class="
                                slotProps.data.status == -4
                                  ? 'surface-400'
                                  : slotProps.data.status == -3
                                  ? 'bg-cyan-400'
                                  : ''
                              "
                              :severity="
                                slotProps.data.status == 1
                                  ? 'success'
                                  : slotProps.data.status == -1
                                  ? 'infor'
                                  : slotProps.data.status == -2
                                  ? 'danger'
                                  : slotProps.data.status == 2
                                  ? 'warning'
                                  : ''
                              "
                            >
                              {{
                                slotProps.data.status == -4
                                  ? "Đề xuất"
                                  : slotProps.data.status == -3
                                  ? "Yêu cầu thêm"
                                  : slotProps.data.status == 1
                                  ? "Đã sửa"
                                  : slotProps.data.status == -1
                                  ? "Đang sửa"
                                  : slotProps.data.status == -2
                                  ? "Lỗi"
                                  : slotProps.data.status == 2
                                  ? "Đã đóng"
                                  : "Trạng thái"
                              }}
                            </Tag>
                          </div>
                        </div>
                      
                      </div>
                    </div>
                    <div class="col-3 text-right flex">
                      <Toolbar
                        class="w-full surface-0 outline-none border-none p-0"
                      >
                        <template #start>
                          <div
                            v-if="
                              slotProps.data.test_user.indexOf(
                                store.getters.user.user_id
                              ) != -1 && slotProps.data.is_view_work
                            "
                          >
                            <img
                              src="/src/assets/image/notify.gif"
                              alt=""
                              width="40"
                              height="40"
                              class="cursor-pointer"
                            />
                          </div>

                          <div
                            v-if="
                              slotProps.data.work_user ==
                                store.getters.user.user_id &&
                              slotProps.data.is_view_test
                            "
                          >
                            <img
                              src="/src/assets/image/notify.gif"
                              alt=""
                              width="40"
                              height="40"
                              class="cursor-pointer"
                            />
                          </div>
                          <Button
                            v-tooltip.top="'Số lỗi đã xử lý!'"
                            v-if="slotProps.data.totalsCheck > 0"
                            :label="
                              slotProps.data.checkpass +
                              '/' +
                              slotProps.data.totalsCheck
                            "
                            :class="
                              slotProps.data.checkpass ==
                              slotProps.data.totalsCheck
                                ? 'p-button-rounded  p-button-success'
                                : (slotProps.data.checkpass /
                                    slotProps.data.totalsCheck) *
                                    100 >
                                  70
                                ? 'p-button-rounded  p-button-warning'
                                : 'p-button-rounded  p-button-danger'
                            "
                          />
                        </template>
                        <template #end>
                          <div
                            v-if="
                              store.getters.user.user_id ==
                                slotProps.data.created_by ||
                              store.getters.user.is_admin
                            "
                          >
                            <Button
                              icon="pi pi-ellipsis-h"
                              class="
                                p-button-outlined p-button-secondary
                                ml-2
                                border-none
                              "
                              @click="toggleBugsMores($event, slotProps.data)"
                              aria-haspopup="true"
                              aria-controls="overlay_BugsMore"
                            />
                            <Menu
                              id="overlay_BugsMore"
                              ref="menuButBugsMores"
                              :model="itemButBugsMores"
                              :popup="true"
                            />
                          </div>
                        </template>
                      </Toolbar>
                    </div>
                  </div>
                  <div
                    @click="showCommentBug(slotProps.data)"
                    class="col-12 field flex p-0 m-0 px-2 pb-2 cursor-pointer"
                  >
                    <div class="pl-0 pt-0">
                      <div>
                        Mở
                        {{
                          moment(slotProps.data.created_date).format(
                            "DD/MM/YYYY HH:mm:ss"
                          )
                        }}
                      </div>
                    </div>
                    <div class="pl-1 pt-0">
                      <div>
                        bởi
                        <span class="text-primary">
                          {{ slotProps.data.created_name }}</span
                        >
                      </div>
                    </div>
                    <div class="pl-1" v-if="slotProps.data.is_overtime">
                     <span class="font-bold">||</span> <span>
                              <font-awesome-icon class="mx-2" style="
    -moz-transform: scaleX(-1);
    color:red;
    -o-transform: scaleX(-1);
    -webkit-transform: scaleX(-1);
    transform: scaleX(-1);
    filter: FlipH;
    -ms-filter: 'FlipH'"  icon="fa-solid fa-clock" />
                            </span> ({{ moment(slotProps.data.start_overtime).format(
                            "HH:mm DD/MM/YYYY")}} -  {{ moment(slotProps.data.end_overtime).format(
                            "HH:mm DD/MM/YYYY ")}})
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
                style="height: 144px"
              />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </template>
        </DataView>
      </div>
    </div>
    <div class="relative comment-height">
      <checklist :isShow="isDetailsBug" :bug="Bug" :fromView="fromView" />
    </div>
  </Sidebar>

  <Dialog
    v-model:visible="isShowAddBug"
    :style="{ width: '40vw' }"
    :header="headerAddBug"
  >
 
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left p-0"
            >Tên yêu cầu <span class="redsao">(*)</span></label
          >
          <InputText
            class="col-10 ip36 p-0 px-2 m-0"
            v-model="bug.bug_name"
            required="true"
            autofocus
            :class="{
              'p-invalid': validateBug.bug_name.$invalid && submitted,
            }"
          />
        </div>

        <div style="display: flex" class="field col-12 md:col-12">
          <div class="col-2 text-left"></div>
          <small
            v-if="
              (validateBug.bug_name.$invalid && submitted) ||
              validateBug.bug_name.$pending.$response
            "
            class="col-8 p-error p-0"
          >
            <span class="col-12 p-0">{{
              validateBug.bug_name.required.$message
                .replace("Value", "Tên lỗi")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>

        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">Mô tả</label>
          <div class="col-10 p-0">
            <Textarea
              style="border-radius: 5px"
              class="w-full"
              spellcheck="false"
              :autoResize="true"
              rows="1"
              v-model="bug.des"
            />
            <!-- <Editor v-model="bug.des" editorStyle="height: 150px" /> -->
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0 flex">
            <label class="col-4 text-left p-0 pt-2">Mức độ</label>
            <Dropdown
              v-model="bug.is_important"
              :options="listImportant"
              optionLabel="name"
              optionValue="code"
              placeholder="Chọn mức độ"
              spellcheck="false"
              class="col-8 ip36 p-0"
            />
          </div>
          <div class="col-6 p-0 flex">
            <label class="col-4 text-center p-0 pt-2">Trạng thái</label>
            <Dropdown
              v-model="bug.status"
              :options="listStatusBugs"
              optionLabel="name"
              optionValue="code"
              placeholder="Chọn trạng thái"
              spellcheck="true"
              class="col-8 ip36 p-0"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">Từ khóa</label>
          <Chips v-model="bug.keyword" class="p-0 w-full m-0" />
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">Ngoài giờ</label>
          <InputSwitch class="col-1" v-model="bug.is_overtime" />
        </div>
        <div class="field col-12 md:col-12 flex"   v-if="bug.is_overtime">
          <div class="col-6 flex p-0">
            <label class="col-4 text-left p-0 pt-2">Ngày bắt đầu</label>
            <Calendar
              class="col-8 p-0"
              :showIcon="true"
              id="time24"
              :showTime="true"
              autocomplete="on"
              v-model="bug.start_overtime"
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
              v-model="bug.end_overtime"
            />
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">File lỗi</label>
          <div class="col-10 p-0 m-0">
            <FileUpload
              chooseLabel="Chọn File"
              :showUploadButton="false"
              :showCancelButton="false"
              :multiple="true"
              accept=".zip,.rar"
              :maxFileSize="10000000"
              @select="onUploadFileBug"
              @remove="removeFileBug"
            >
            </FileUpload>
          </div>
        </div>
        <div class="col-12 p-0 flex field">
          <label class="col-2 text-left"></label>
          <div class="col-10 p-0" v-if="bug.url_file">
            <Toolbar class="w-full py-3">
              <template #start>
                <div class="flex">
                  <img
                    src="/src/assets/image/rarimg.png"
                    style="object-fit: contain"
                    width="50"
                    height="50"
                    alt="logorar"
                  />
                  <span style="line-height: 50px">
                    {{ bug.url_file.substring(16) }}</span
                  >
                </div>
              </template>
              <template #end>
                <Button
                  icon="pi pi-times"
                  class="p-button-rounded p-button-danger"
                  @click="deleteFileBug(item)"
                />
              </template>
            </Toolbar>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeBug()"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveBug(!validateBug.$invalid)"
      />
    </template>
  </Dialog>
</template>
<style>
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