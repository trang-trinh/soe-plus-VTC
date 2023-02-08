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
    case "preListDetail":
      showBugs(obj.data.task_id, task.value);
      isDetailsBug.value = false;
      break;
  }
});
const props = defineProps({
  isShowBug: Boolean,
  task: Object,
  project_id: Intl,
  cache:String,
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
const fromView = ref("detail");
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
  };
  isShowAddBug.value = true;
};
const onNewVersion = () => {
  toast.info("Chức năng bạn chọn sẽ sớm có ở phiên bản mới!");
};
const loadTask = () => {
  emitter.emit("emitData", { type: "loadTask", data: null });
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
          { par: "type", va:0 }
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

    //   if (dataLe.test_user_ids.includes(store.getters.user.user_id))
    //     hideNewAction(value, true, false);
    //   if (store.getters.user.user_id == dataLe.user_id)
    //     hideNewAction(value, true, true);
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
//Các Func của CheckList
onMounted(() => {
  return {};
});
</script>
<template>
<div v-if="!isDetailsBug">
  <h4>
    Danh sách bug
    <span>({{ listBugSave.length }})</span>
  </h4>
  <div
    class="w-full list-bugs-item"
    v-for="(item, index) in listBugSave"
    :key="index"
  >
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
          <div @click="showCommentBug(item)" class="col-9 p-0 cursor-pointer">
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
                  {{ item.bug_id }}
                </Tag>
              </div>
              <div class="mb-1 font-bold text-xl pt-2 pl-1">
                <Tag v-if="item.is_important == 0" severity="info"
                  >Không quan trọng</Tag
                >
                <Tag v-if="item.is_important == 1" severity="success"
                  >Bình thường</Tag
                >
                <Tag v-if="item.is_important == 2" severity="warning">Gấp</Tag>
                <Tag v-if="item.is_important == 3" severity="danger"
                  >Rất gấp</Tag
                >
              </div>
              <div
                class="mb-1 font-bold text-xl px-2 pt-2 pl-1"
                :class="item.status == 1 ? 'line-through text-green-600' : ''"
              >
                {{ item.bug_name }}
              </div>
              <div v-if="item.status" class="mb-1 font-bold text-xl px-1 pt-2">
                <div class="mb-1 font-italic text-color-secondary">
                  <Tag
                    :class="
                      item.status == -4
                        ? 'surface-400'
                        : item.status == -3
                        ? 'bg-cyan-400'
                        : ''
                    "
                    :severity="
                      item.status == 1
                        ? 'success'
                        : item.status == -1
                        ? 'infor'
                        : item.status == -2
                        ? 'danger'
                        : item.status == 2
                        ? 'warning'
                        : ''
                    "
                  >
                    {{
                      item.status == -4
                        ? "Đề xuất"
                        : item.status == -3
                        ? "Yêu cầu thêm"
                        : item.status == 1
                        ? "Đã sửa"
                        : item.status == -1
                        ? "Đang sửa"
                        : item.status == -2
                        ? "Lỗi"
                        : item.status == 2
                        ? "Đã đóng"
                        : "Trạng thái"
                    }}
                  </Tag>
                </div>
              </div>
            </div>
          </div>
          <div class="col-3 text-right flex">
            <Toolbar class="w-full surface-0 outline-none border-none p-0">
              <template #start>
                <div
                  v-if="
                    item.test_user.indexOf(store.getters.user.user_id) != -1 &&
                    item.is_view_work
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
                    item.work_user == store.getters.user.user_id &&
                    item.is_view_test
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
                  v-if="item.totalsCheck > 0"
                  :label="item.checkpass + '/' + item.totalsCheck"
                  :class="
                    item.checkpass == item.totalsCheck
                      ? 'p-button-rounded  p-button-success'
                      : (item.checkpass / item.totalsCheck) * 100 > 70
                      ? 'p-button-rounded  p-button-warning'
                      : 'p-button-rounded  p-button-danger'
                  "
                />
              </template>
              <template #end>
                <div
                  v-if="
                    store.getters.user.user_id == item.created_by ||
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
                    @click="toggleBugsMores($event, item)"
                    aria-haspopup="true"
                    aria-controls="overlay_BugsMore"
                  />
                  
                </div>
              </template>
            </Toolbar>
          </div>
        </div>
        <div
          @click="showCommentBug(item)"
          class="col-12 field flex p-0 m-0 px-2 pb-2 cursor-pointer"
        >
          <div class="pl-0 pt-0">
            <div>
              Mở
              {{ moment(item.created_date).format("DD/MM/YYYY HH:mm:ss") }}
            </div>
          </div>
          <div class="pl-1 pt-0">
            <div>
              bởi
              <span class="text-primary"> {{ item.created_name }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</div>
  <div class="relative comment-height">
    <checklist :isShow="isDetailsBug" :bug="Bug" :fromView="fromView" />
  </div>
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
  <Menu
                    id="overlay_BugsMore"
                    ref="menuButBugsMores"
                    :model="itemButBugsMores"
                    :popup="true"
                  />
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