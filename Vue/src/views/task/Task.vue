<script setup>
//Khai báo InJect và Import (import)
import taskgroup from "../../components/task/taskgroup.vue";
import groupuser from "../../components/task/groupuser.vue";
import detailstask from "../../components/task/detailstask.vue";
import taskbug from "../../components/task/taskbug.vue";
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { VuemojiPicker } from "vuemoji-picker";
import vi from "date-fns/locale/vi";
import moment from "moment";
import { checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
//Nơi nhận EMIT từ component
const emitter = inject("emitter");
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "loadCategory":
      onNodeSelect(obj.data, true);
      break;
    case "listCategory":
      listCategory.value = obj.data;
      break;
    case "listCategorySave":
      listCategorySave.value = obj.data;
      break;
    case "listCateSelected":
      listCateSelected.value = obj.data;
      break;

    case "listDropdownUser":
      listDropdownUser.value = obj.data;
      break;
    case "listUsers":
      listUsers.value = obj.data;
      break;
    case "listDropdownUserCheck":
      listDropdownUserCheck.value = obj.data;
      break;
    case "UsersCount":
      UsersCount.value = obj.data;
      break;
    case "onTaskUserFilter":
      onTaskUserFilter(obj.data);
      break;
    case "userFilter":
      userFilter.value = obj.data;
      break;
    case "liUsers":
      liUsers.value = obj.data;

      break;
    case "reloadViewTask":
      datalists.value
        .filter((x) => x.active == true)
        .forEach((element) => {
          element.active = false;
        });
      isCheckTask.value = false;
      break;
    case "loadTask":
      loadTask(
        userFilter.value,
        nodeValue.value
          ? nodeValue.value.data.category_id
            ? nodeValue.value.data.category_id
            : null
          : null,
        nodeValue.value
          ? nodeValue.value.data.code
            ? nodeValue.value.data.code
            : null
          : null
      );
      isShowBug.value = false;

      break;
  }
});
//Khai báo biến (variable)
const rules = {
  task_name: {
    required,
    $errors: [
      {
        $property: "task_name",
        $validator: "required",
        $message: "Tên công việc không được để trống!",
      },
    ],
  },
  test_user_ids: {
    required,
    $errors: [
      {
        $property: "test_user_ids",
        $validator: "required",
        $message: "Người kiểm tra không được để trống!",
      },
    ],
  },
};
const taskDetails = ref();

const op = ref();
const task = ref({
  category_id: null,
  task_name: "",
  test_user_ids: "",
  partner_id: "",
  user_id: "",
  des: null,
  estimated_date: null,
  estimated_hours: null,
  status: 0,
  parent_id: null,
  is_plan: false,
  keywords: "",
});
const bug = ref({
  bug_name: "",
  des: "",
  status: 0,
  keyword: "",
});
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
const list_UserTest = ref();
const listCategory = ref([]);
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const isShowQickAdd = ref(false);
const opbugcomment = ref();
const listUsertest = ref([]);
const listBugsTest = ref([]);
const listCateSelected = ref();

const submitted = ref(false);
const v$ = useVuelidate(rules, task);
const isSaveTask = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const userFilter = ref(store.getters.user.user_id);
const listBugs = ref();
const sttTask = ref(1);
const checkPage = ref(false);
const itemButMoresTaskTest = ref([
  {
    label: "Cập nhật",
    icon: "pi pi-cog",
    command: () => {
      editTaskTest();
    },
  },

  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: () => {
      deleteTaskTest();
    },
  },
]);
const isDetailsTaskTest = ref(false);
const isDetailsTest = ref(false);
const task_PreView = ref();
const listTestChild = ref();
const checkAddChild = ref(false);
const projectSelected = ref();
const taskId = ref();
const qickBug = ref({
  project_id: projectSelected.value,
  task_id: taskId.value,
  bug_name: "",
  status: -2,
});
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
  overTime: null,
});
const totalRecoreds = ref(10);
const nodeSelected = ref();
const listCategorySave = ref([]);
const checkIsmain = ref(true);
const check_CateNull = ref();
const categoryIdSave = ref();
const layout = ref("list");
const nodeValue = ref();
const categoryName = ref();
const keyselected = ref(0);
const id_Khac = ref(1);
const selectedKey = ref([]);
const expandedKeys = ref({});
const listUsers = ref([]);
const listUserShow = ref([]);
const UsersCount = ref();
const listDropdownUser = ref([]);
const listDropdownUserCheck = ref([]);
const listTask = ref([]);
const headerDialog = ref();
const displayBasic = ref(false);
const isTypeAPI = ref(true);
const isFirst = ref(true);
const taskDateFilter = ref();
const monthPickerFilter = ref();
const filterMonth = ref();
const weekPickerFilter = ref();
const itemButMores = ref([
  {
    label: "Sửa công việc",
    icon: "pi pi-cog",
    command: () => {
      editTask(task.value);
    },
  },
  {
    label: "Xoá công việc",
    icon: "pi pi-trash",
    command: () => {
      deleteTask(task.value.task_id);
    },
  },
]);
const isCheckTask = ref(false);
const isCheckTest = ref(false);
const TaskSave = ref();
const panelEmoij1 = ref();
const panelEmoij2 = ref();
const panelEmoij3 = ref();
const panelEmoij4 = ref();
const panelCalendar = ref();

let files = [];
const dropdownSel = ref();
const isEditWork = ref(false);

const typeTime = ref(2);
const listWorkTime = ref([
  { name: "Giờ", code: 2 },

  { name: "Ngày", code: 3 },
]);
const listStatus = ref([
  {
    name: "Đang lập kế hoạch",
    code: 0,
    css: "p-button-raised p-button-secondary ",
  },
  { name: "Đang làm", code: 1, css: "p-button-raised" },

  { name: "Tạm đóng", code: 6, css: "p-button-raised p-button-warning" },
  { name: "Chuyển sang Test", code: 2, css: "p-button-raised p-button-help" },
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

const isShowBug = ref(false);

const isDetailsBug = ref(false);

const taskName = ref();
const projectID_Save = ref();
const taskSave = ref();
const checkPreCmtBug = ref(false);

const menuButMores = ref();
const menuTask2 = ref();
const menuButTaskTest = ref();

const listStatusDetails = ref([
  { name: "Đã Test OK", code: 3, css: "p-button-raised p-button-success " },
  { name: "Test Chưa OK", code: 4, css: "p-button-raised p-button-danger " },
  { name: "Phát sinh thêm", code: 5, css: "p-button-raise " },
  { name: "Tạm đóng", code: 6, css: "p-button-raised p-button-warning" },
]);

const opDetails = ref();

// Khai báo hàm (function)
const toggleStatusDetails = (dl, event) => {
  opDetails.value.toggle(event);
};
const deleteFileCode = (value) => {
  task.value.url_file = task.value.url_file.filter((a) => a != value);
};

const reloadListBugs = (id) => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_bug_list",
        par: [
          { par: "task_id", va: id },
          { par: "search", va: options.value.searchTextBug },

          { par: "user_id", va: store.getters.user.user_id },
          { par: "type", va: 0 },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      listBugsTest.value = [];

      data.forEach((element) => {
        listBugsTest.value.push({
          name: element.bug_name,
          code: element.bug_id + "",
        });
      });
    })
    .catch((error) => {
      options.value.loading = false;
      console.log(error);
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const showChildTest = (data) => {
  isDetailsTaskTest.value = false;
  task_PreView.value = data;

  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_test_child_list",
        par: [{ par: "test_group_id", va: data.test_group_id }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      listTestChild.value = data;
    });

  isDetailsTest.value = true;
};
const preListTask = () => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_test_list",
        par: [{ par: "task_id", va: task_PreView.value.task_id }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element) => {
        element.users = JSON.parse(element.users);
      });
      listUsertest.value = data;
      isDetailsTest.value = false;
    });
};
const addTestChild = () => {
  task_test.value = {
    test_group_id: task_PreView.value.test_group_id,
    task_id: task_PreView.value.task_id,
    test_user_id: store.getters.user.user_id,
    test_content: "",
    test_pass: null,
    is_main: false,
    is_delete: false,
  };
  isShowAddTest.value = true;
  checkAddChild.value = true;
};
const toggleTaskTest = (event, u) => {
  task_test.value = u;
  taskId.value = u.task_id;
  menuButTaskTest.value.toggle(event);
};
//Cập nhật nội dung test
const editTaskTest = () => {
  listQickBug.value = [];
  headerAddTest.value = "Cập nhật Test";
  let arrCheck = "";
  listBugsTest.value.forEach((element) => {
    arrCheck += element.code + ",";
  });
  if (
    !Array.isArray(task_test.value.test_bugs) &&
    task_test.value.test_bugs != null
  ) {
    task_test.value.test_bugs = task_test.value.test_bugs.split(",");
  }
  let arrSave = [];
  task_test.value.test_bugs.forEach((element) => {
    if (arrCheck.indexOf(element) != -1 && element != "") arrSave.push(element);
  });
  task_test.value.test_bugs = arrSave;

  isShowAddTest.value = true;
};
const deleteTaskTest = () => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá Test này không!",
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
          .delete(baseURL + "/api/task_main/Delete_TaskTest", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data:
              task_test.value.test_id != null ? [task_test.value.test_id] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá Test thành công!");

              loadProject(false);
              reloadTask(store.getters.user.user_id, true);
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
const changeCateSelect = () => {
  for (var j in dropdownSel.value) {
    if (j == 0) task.value.category_id = null;
    else task.value.category_id = j;
  }
};

const showEmoji = (event, check) => {
  if (check == 1) panelEmoij1.value.toggle(event);
  if (check == 2) panelEmoij2.value.toggle(event);
  if (check == 3) panelEmoij3.value.toggle(event);
  if (check == 4) panelEmoij4.value.toggle(event);
  if (check == 5) panelCalendar.value.toggle(event);
};
const deleteTask = (value) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá công việc này không!",
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
          .delete(baseURL + "/api/task_main/Delete_task", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: value != null ? [value] : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá công việc thành công!");

              loadProject(false);
              reloadTask(store.getters.user.user_id, true);
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

//Thêm Test
const isShowAddTest = ref(false);
const headerAddTest = ref();
const task_test = ref({
  test_content: "",
  test_pass: null,
});
const showAddTest = (value) => {
  let check = false;
  list_UserTest.value.test_user_ids.forEach((element) => {
    if (store.getters.user.user_id == element || store.getters.user.is_admin)
      check = true;
  });
  if (value.test_user_id == null && check) {
    task_test.value = value;
    task_test.value.test_user_id = store.getters.user.user_id;
    task_test.value.test_pass = false;
    isShowAddTest.value = true;
    isDetailsTaskTest.value = false;
    headerAddTest.value = "Cập nhật Test";
  } else {
    axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "task_bug_list",
          par: [
            { par: "task_id", va: value.task_id },
            { par: "search", va: options.value.searchTextBug },
            { par: "user_id", va: store.getters.user.user_id },
            { par: "type", va: 0 },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        listBugsDetails.value = [];
        if (!Array.isArray(value.test_bugs) && value.test_bugs != null) {
          value.test_bugs = value.test_bugs.split(",");
        }
        data.forEach((element) => {
          value.test_bugs.forEach((item) => {
            if (item == element.bug_id) listBugsDetails.value.push(element);
          });
        });
        task_PreView.value = value;
        isDetailsTest.value = true;
        isDetailsTaskTest.value = true;
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

const closeTestTask = () => {
  // task_test.value.test_content = "";
  isShowAddTest.value = false;
};

const showQickAddBug = () => {
  qickBug.value = {
    project_id: projectSelected.value,
    task_id: taskId.value,
    bug_name: "",
    status: -2,
  };
  isShowQickAdd.value = true;
};

const listQickBug = ref([]);

const listBugsDetails = ref([]);
const saveTestTask = () => {
  let modelTask_Test = {
    task_Test: task_test.value,
    api_Bugs: listQickBug.value,
  };
  if (Array.isArray(task_test.value.test_bugs))
    task_test.value.test_bugs = task_test.value.test_bugs.toString();
  if (!checkAddChild.value) {
    (async () => {
      await axios
        .put(baseURL + "/api/task_main/Update_TaskTest", modelTask_Test, config)
        .then((response) => {
          if (response.data.err != "1") {
            swal.close();

            closeTestTask();
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

      await axios
        .post(
          baseURL + "/api/Proc/CallProc",
          {
            proc: "task_test_list",
            par: [{ par: "task_id", va: task_test.value.task_id }],
          },
          config
        )
        .then((response) => {
          let data = JSON.parse(response.data.data)[0];
          data.forEach((element) => {
            element.users = JSON.parse(element.users);
          });
          listUsertest.value = data;
          listTestChild.value = data;
          reloadListBugs(task_test.value.task_id);
          toast.success("Cập nhật test thành công!");
        });
    })();
  } else {
    axios
      .post(baseURL + "/api/task_main/Add_TaskTest", modelTask_Test, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm Test thành công!");
          showChildTest(task_test.value);
          reloadListBugs(task_test.value.task_id);
          closeTestTask();
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
const toggleStatus = (dl, event) => {
  if (dl.status == 2 || dl.status == 3) {
    checkPreCmtBug.value = true;

    taskDetails.value = dl;
    list_UserTest.value = dl;
    axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "task_test_list",
          par: [{ par: "task_id", va: dl.task_id }],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        data.forEach((element) => {
          element.users = JSON.parse(element.users);
          if (element.test_bugs)
            element.test_bugs = element.test_bugs.split(",");

          if (element.users) {
            if (element.users.length > 3) {
              element.totalUser = element.users.length - 3;
              element.users = element.users.slice(0, 3);
            }
          }
        });

        listUsertest.value = data;
      })
      .catch((error) => {
        console.log(error);
      });
    axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "task_bug_list",
          par: [
            { par: "task_id", va: dl.task_id },
            { par: "search", va: options.value.searchTextBug },
            { par: "user_id", va: store.getters.user.user_id },
            { par: "type", va: 0 },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        listBugsTest.value = [];

        data.forEach((element) => {
          listBugsTest.value.push({
            name: element.bug_name,
            code: element.bug_id + "",
          });
        });
      })
      .catch((error) => {
        options.value.loading = false;
        console.log(error);
        if (error && error.status === 401) {
          swal.fire({
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
    isCheckTest.value = true;
  } else {
    taskDetails.value = dl;
    op.value.toggle(event);
  }
};
const saveData = () => {
  submitted.value = true;
  if (
    task.value.task_name == null ||
    task.value.task_name == "" ||
    task.value.test_user_ids == null
  ) {
    return;
  }
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url", file);
  }
  if (task.value.keywords != null) {
    task.value.keywords = task.value.keywords.toString();
  }
  if (Array.isArray(task.value.url_file) && task.value.url_file != null) {
    task.value.url_file = task.value.url_file.toString();
  }
  if (
    Array.isArray(task.value.test_user_ids) &&
    task.value.test_user_ids != null
  ) {
    task.value.test_user_ids = task.value.test_user_ids.toString();
  }
  if (Array.isArray(task.value.partner_id) && task.value.partner_id != null) {
    task.value.partner_id = task.value.partner_id.toString();
  }
  if (typeTime.value == 3)
    task.value.estimated_hours = task.value.estimated_hours * 24;
  let dtt = "";
  let detached = "";

  if (Array.isArray(task.value.next_date)) {
    if (task.value.next_date.length > 0) {
      task.value.next_date.forEach((element) => {
        if ((element != null, element != "")) {
          element = element.toJSON();
          dtt += detached + element;
        }
        detached = ",";
      });
      task.value.next_date = dtt;
    } else {
      task.value.next_date = null;
    }
  }
  console.log("yas", task.value);
  if (isSaveTask.value) task.value.update_dealine = false;
  formData.append("task", JSON.stringify(task.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });

  if (!isSaveTask.value) {
    axios
      .post(baseURL + "/api/task_main/Add_task", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm công việc thành công!");
          loadProject(false);

          closeDialog();
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
  } else {
    axios
      .put(baseURL + "/api/task_main/Update_task", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Sửa công việc thành công!");

          closeDialog();
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
};
//Sửa bản ghi
const editTask = (dataTem) => {
  if (
    (task.value.status == 2 || task.value.status == 3) &&
    store.getters.user.is_admin == false
  ) {
    toast.warning("Không thể cập nhật công việc lúc này!");
    return;
  }
  if (
    (task.value.status == 2 || task.value.status == 3) &&
    store.getters.user.is_admin == true
  ) {
    listStatus.value = [
      {
        name: "Đang lập kế hoạch",
        code: 0,
        css: "p-button-raised p-button-secondary ",
      },
      { name: "Đang làm", code: 1, css: "p-button-raised " },
      { name: "Đang đợi Test", code: 2, css: "p-button-raised p-button-help" },
      { name: "Đã Test OK", code: 3, css: "p-button-raised p-button-success" },
      { name: "Test Chưa OK", code: 4, css: "p-button-raised p-button-danger" },
      {
        name: "Phát sinh thêm",
        code: 5,
        css: "p-button-raised p-button-secondary",
      },

      { name: "Tạm đóng", code: 6, css: "p-button-raised p-button-warning" },
    ];
  }

  files = [];
  submitted.value = false;
  if (dataTem.category_id == null) {
    dropdownSel.value = { 0: true };
  } else {
    dropdownSel.value = { [dataTem.category_id]: true };
  }
  typeTime.value = 2;
  if (dataTem.keywords != null && dataTem.keywords.length > 1) {
    if (!Array.isArray(dataTem.keywords)) {
      dataTem.keywords = dataTem.keywords.split(",");
    }
  }
  if (!Array.isArray(task.value.url_file) && task.value.url_file != null) {
    dataTem.url_file = dataTem.url_file.split(",");
  }
  if (
    !Array.isArray(task.value.test_user_ids) &&
    task.value.test_user_ids != null &&
    task.value.test_user_ids != ""
  ) {
    dataTem.test_user_ids = dataTem.test_user_ids.split(",");
  }
  if (
    !Array.isArray(task.value.parent_id) &&
    task.value.parent_id != null &&
    task.value.parent_id != ""
  ) {
    dataTem.parent_id = dataTem.parent_id.split(",");
  }
  let arrN = [];
  if (!Array.isArray(task.value.next_date) && task.value.next_date != null) {
    dataTem.next_date = dataTem.next_date.split(",").forEach((element) => {
      arrN.push(new Date(element));
    });
    dataTem.next_date = arrN;
  }
  task.value.estimated_date = new Date(
    moment(task.value.estimated_date).format("YYYY/MM/DD HH:mm:ss")
  );
  if (dataTem.test_user_ids == null || task.value.test_user_ids == "")
    dataTem.test_user_ids = null;

  task.value = dataTem;
  listDropdownUserCheck.value = listDropdownUser.value.filter(
    (x) => x.code != task.value.user_id
  );
  if (task.value.test_user_ids == null) task.value.test_user_ids = "";
  isEditWork.value = true;
  headerDialog.value = "Sửa công việc";
  isSaveTask.value = true;
  displayBasic.value = true;
};
const toggleMores = (event, u, check) => {
  task.value = u;
  if (check) menuTask2.value.toggle(event);
  else menuButMores.value.toggle(event);
};
//Bug công việc

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
const showBugs = (value, dataLe) => {
  taskSave.value = dataLe;
  projectID_Save.value = dataLe.project_id;
  checkPreCmtBug.value = false;
  taskName.value = [];
  taskName.value = listTask.value.filter((x) => x.task_id == value);
  if (taskName.value.length > 0) taskName.value = taskName.value[0];
  else taskName.value = "";
  taskId.value = value;
  if (dataLe.test_user_ids.includes(store.getters.user.user_id))
    hideNewAction(value, true, false);

  if (store.getters.user.user_id == dataLe.user_id)
    hideNewAction(value, true, true);
  isDetailsBug.value = false;
  isShowBug.value = true;
};
// Xóa File

const onUploadFile = (event) => {
  event.files.forEach((element) => {
    files.push(element);
  });
};
const removeFile = (event) => {
  files = files.filter((a) => a != event.file);
};
const changeWorkUser = () => {
  console.log("sos1", task.value);
  if (task.value.test_user_ids)
    if (task.value.test_user_ids.indexOf(task.value.user_id) != -1)
      task.value.test_user_ids = "";

  listDropdownUserCheck.value = listDropdownUser.value.filter(
    (x) => x.code != task.value.user_id
  );
};
//Phân trang dữ liệu
const onPage = (event) => {
  options.value.PageNo = event.page;
  options.value.PageSize = event.rows;
  reloadTask(store.getters.user.user_id, false);
};
//Hiển thị dialog

const addTask = (str) => {
  files = [];

  submitted.value = false;

  task.value = {
    category_id: nodeSelected.value
      ? nodeSelected.value.category_id
        ? nodeSelected.value.category_id
        : id_Khac.value
      : id_Khac.value,
    task_name: "",
    test_user_ids: null,
    user_id: store.getters.user.user_id,
    des: "",
    estimated_date: new Date(),
    estimated_hours: null,
    status: 0,
    parent_id: null,
    is_order: options.value.totalRecords + 1,
    is_plan: false,
    keywords: "",
    next_date: [],
    is_important: 1,
  };
  listDropdownUserCheck.value = listDropdownUser.value.filter(
    (x) => x.code != task.value.user_id
  );
  isEditWork.value = false;
  checkIsmain.value = false;
  isSaveTask.value = false;
  headerDialog.value = str;
  displayBasic.value = true;
};

const closeDialog = () => {
  task.value = {
    task_name: "",
    test_user_ids: "",
    user_id: "",
    des: "",
    estimated_date: new Date(),
    estimated_hours: "",
    status: 0,
    parent_id: null,
    is_plan: false,
    keywords: "",
  };

  displayBasic.value = false;
};
//Hiển thị chi tiết công việc
const showDetails = (value) => {
  TaskSave.value = value.data;
  selectedKey.value[keyselected.value] = false;
  if (!value.data.task_id) {
    value.key = value.data.category_id;
    selectedKey.value[value.key] = true;
    keyselected.value = value.key;
    datalists.value
      .filter((x) => x.active)
      .forEach(function (d) {
        d.active = false;
      });
    value.data.active = true;
    nodeValue.value = value;
    onNodeSelect(value);
  } else {
    if (!Array.isArray(value.data.url_file) && value.data.url_file != null)
      value.data.url_file = value.data.url_file.split(",");
    value.key = value.data.task_name;
    selectedKey.value[value.key] = true;
    keyselected.value = value.key;
    datalists.value
      .filter((x) => x.active)
      .forEach(function (d) {
        d.active = false;
      });

    value.data.active = true;

    value.data.modified_name = liUsers.value.filter(
      (x) => x.user_id == value.data.modified_by
    )[0].full_name;

    value.data.date_W = moment(value.data.estimated_date).format(
      "HH:mm:ss DD/MM/YYYY"
    );
    if (value.data.actual_date)
      value.data.date_A = moment(value.data.actual_date).format(
        "HH:mm:ss DD/MM/YYYY"
      );
    value.data.date_U = moment(value.data.modified_date).format(
      "HH:mm:ss DD/MM/YYYY"
    );
    console.log("uas", value.data);
    task.value = value.data;
    isCheckTask.value = true;
  }
};
//Phiên bản mới
const onNewVersion = () => {
  toast.info("Chức năng bạn chọn sẽ sớm có ở phiên bản mới!");
};
//Lấy ngày nghỉ trong khoảng

function getDates(startDate, endDate) {
  const dates = [];
  let currentDate = startDate;
  const addDays = function (days) {
    const date = new Date(this.valueOf());
    date.setDate(date.getDate() + days);
    return date;
  };
  while (currentDate <= endDate) {
    dates.push(currentDate.toJSON());
    currentDate = addDays.call(currentDate, 1);
  }
  return dates;
}

const onProductSelect = (item, value, check) => {
  op.value.hide();
  if (check) {
    opDetails.value.hide();
    taskDetails.value.status = item.code;
  }
  let data = {
    IntID: value.task_id,
    TextID: value.task_id + "",
    IntTrangthai: item.code,
    BitTrangthai: value.status,
  };
  axios
    .put(baseURL + "/api/task_main/Update_statusTask", data, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái thành công!");
        reloadTask(store.getters.user.user_id, true);
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

//Chọn node category

const onNodeSelect = (node, check) => {
  keyselected.value = node.key;
  if (selectedKey.value) {
    selectedKey.value[keyselected.value] = false;
    selectedKey.value[node.key] = true;
  }
  if (check) {
    if (expandedKeys.value[node.key] == true) {
      expandedKeys.value[node.key] = false;
    } else {
      expandedKeys.value[node.key] = true;
    }
  }

  nodeValue.value = node;

  options.value.loading = true;
  categoryName.value = node.data.category_name;

  if (node.data.category_id == null) {
    nodeSelected.value = null;

    check_CateNull.value = node.data;
    let arrT = listCategorySave.value.filter(
      (x) => x.project_id == node.data.code && x.category_name == "Khác"
    );
    if (arrT.length > 0) id_Khac.value = arrT[0].category_id;
    loadCount(store.getters.user.user_id, node.data.code, null);
    loadTask(store.getters.user.user_id, null, node.data.code);
    return;
  } else {
    check_CateNull.value = null;
    categoryIdSave.value = node.data.category_id;
    isTypeAPI.value = true;
    nodeSelected.value = node.data;

    datalists.value = [];
    options.value.TaskUnfinished = 0;
    options.value.TaskFinished = 0;
    options.value.TaskTempClose = 0;
    loadCount(store.getters.user.user_id, null, categoryIdSave.value);
    //Lọc theo Tuần
    // options.value.Start_date = new Date(
    //   new Date().getFullYear(),
    //   new Date().getMonth(),
    //   new Date().getDate() + 1 - new Date().getDay()
    // );
    // options.value.End_date = new Date(
    //   new Date().getFullYear(),
    //   new Date().getMonth(),
    //   new Date().getDate() + (7 - new Date().getDay())
    // );  taskDateFilter.value=[];
    // taskDateFilter.value.push(options.value.Start_date);
    // taskDateFilter.value.push(options.value.End_date);
    loadCountSave(store.getters.user.user_id, null);
    loadTask(store.getters.user.user_id, node.data.category_id, null);
  }
};
//Load số trang

const loadCountSave = (user_id, project_id) => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_main_count",
        par: [
          { par: "user_id", va: user_id },
          { par: "search", va: options.value.searchText },
          { par: "category_id", va: categoryIdSave.value },
          { par: "project_id", va: project_id },
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "status", va: options.value.statusTask },
          { par: "start_date", va: options.value.Start_date },
          { par: "end_date", va: options.value.End_date },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        totalRecoreds.value = data[0].totalRecords;
      }
    })
    .catch((error) => {
      console.log(error);
    });
};

//Load lại công việc
const reloadTask = (user_id, check) => {
  if (check) loadCount(store.getters.user.user_id, null, categoryIdSave.value);
  options.value.loading = true;
  datalists.value = [];
  loadTask(
    user_id,
    nodeValue.value
      ? nodeValue.value.data.category_id
        ? nodeValue.value.data.category_id
        : null
      : null,
    nodeValue.value
      ? nodeValue.value.data.code
        ? nodeValue.value.data.code
        : null
      : null
  );
};

//Filter công việc

const onTaskUserFilter = (value) => {
  if (value.active) {
    listUserShow.value.forEach((element) => {
      if (element.data == value.data) element.active = false;
    });
    listUsers.value.forEach((element) => {
      if (element.data == value.data) element.active = false;
    });
    userFilter.value = store.getters.user.user_id;
    loadCount(store.getters.user.user_id, null, categoryIdSave.value);

    reloadTask(store.getters.user.user_id, false);

    return;
  } else {
    options.value.loading = true;
    datalists.value = [];
    listUserShow.value.forEach((element) => {
      if (element.data == value.data) element.active = true;
      else element.active = false;
    });
    listUsers.value.forEach((element) => {
      if (element.data == value.data) element.active = true;
      else element.active = false;
    });
    userFilter.value = value.data.user_id;
    loadCount(value.data.user_id, null, categoryIdSave.value);
    datalists.value = [];

    loadTask(value.data.user_id, categoryIdSave.value, null);
  }
};

const toggleFilterMonth = (event) => {
  filterMonth.value.toggle(event);
};
const todayClick = () => {
  taskDateFilter.value = [];
  taskDateFilter.value.push(new Date());
};
const delDayClick = () => {
  taskDateFilter.value = [];
  onTaskFilter(null);
};
const onDayClick = () => {
  if (taskDateFilter.value == null) taskDateFilter.value = [];
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  if (weekPickerFilter.value) weekPickerFilter.value = null;

  onTaskFilter(null);
};
const onCleanFilterMonth = () => {
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  if (weekPickerFilter.value) weekPickerFilter.value = null;
  options.value.Start_date = null;
  options.value.End_date = null;
  loadCountSave(userFilter.value, null);
  reloadTask(userFilter.value, false);
};
const onClearMonth = () => {
  options.value.Start_date = null;
  weekPickerFilter.value = [];

  options.value.End_date = null;

  monthPickerFilter.value = null;
};
const onFilterMonth = (check, e) => {
  if (check) {
    if (weekPickerFilter.value) {
      options.value.Start_date = weekPickerFilter.value[0];
      options.value.End_date = weekPickerFilter.value[1];
    } else {
      options.value.Start_date = null;
      options.value.End_date = null;
    }
  } else {
    if (monthPickerFilter.value) {
      weekPickerFilter.value = [];
      let day = new Date(
        monthPickerFilter.value.year,
        monthPickerFilter.value.month + 1,
        0
      ).getDate();
      options.value.Start_date = new Date(
        monthPickerFilter.value.month +
          1 +
          "/01" +
          "/" +
          monthPickerFilter.value.year
      );
      options.value.End_date = new Date(
        monthPickerFilter.value.month +
          1 +
          "/" +
          day +
          "/" +
          monthPickerFilter.value.year
      );
    } else {
      options.value.Start_date = null;
      options.value.End_date = null;
    }
  }
  options.value.PageNo = 0;
  loadCountSave(userFilter.value, null);
  reloadTask(userFilter.value, false);
};
const onTaskFilter = (status) => {
  datalists.value = [];
  options.value.PageNo = 0;
  if (status == null) {
    if (taskDateFilter.value) {
      options.value.Start_date = taskDateFilter.value[0];
      options.value.End_date = taskDateFilter.value[1];
    } else {
      options.value.Start_date = null;
      options.value.End_date = null;
    }

    options.value.statusTask = null;
    loadCountSave(userFilter.value, null);
    reloadTask(userFilter.value, false);
    totalRecoreds.value = options.value.totalRecords;
  } else {
    if (status == 7) {
      options.value.statusTask = 7;
      loadCountSave(userFilter.value, null);
      reloadTask(userFilter.value, false);
      totalRecoreds.value = options.value.outOfDate;
    } else if (status == 3) {
      totalRecoreds.value = options.value.finishedRecord;
      options.value.statusTask = 3;
      loadCountSave(userFilter.value, null);
      reloadTask(userFilter.value, false);
    } else if (status == 6) {
      totalRecoreds.value = options.value.tempClose;
      options.value.statusTask = 6;
      loadCountSave(userFilter.value, null);
      reloadTask(userFilter.value, false);
    } else if (status == 2) {
      totalRecoreds.value = options.value.waitedRecord;
      options.value.statusTask = 2;
      loadCountSave(userFilter.value, null);
      reloadTask(userFilter.value, false);
    } else {
      totalRecoreds.value = options.value.unFinishRecord;
      options.value.statusTask = status;
      loadCountSave(userFilter.value, null);
      reloadTask(userFilter.value, false);
    }
  }
};

//Load các công việc

const loadTask = (user_id, category_id, project_id) => {
  options.value.loading = true;
  datalists.value = [];
  (async () => {
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_bug_list_all",
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        listBugs.value = data;
      })
      .catch((error) => {
        options.value.loading = false;
        console.log(error);
        if (error && error.status === 401) {
          swal.fire({
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "task_main_list",
          par: [
            { par: "user_id", va: user_id },
            { par: "category_id", va: category_id },
            { par: "project_id", va: project_id },
            { par: "organization_id", va: store.getters.user.organization_id },

            { par: "pageno", va: options.value.PageNo },
            { par: "pagesize", va: options.value.PageSize },
            { par: "search", va: options.value.searchText },
            { par: "status", va: options.value.statusTask },
            { par: "start_date", va: options.value.Start_date },
            { par: "end_date", va: options.value.End_date },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];

        data.forEach((element) => {
          let timeSpan = new Date(element.estimated_date);
          let timeNow = new Date();

          if (!element.taskTime) element.taskTime = 0;
          let arrDateBw = getDates(
            new Date(moment(element.estimated_date).format("YYYY/MM/DD")),
            new Date()
          );
          let arrN = [];
          if (!Array.isArray(element.next_date) && element.next_date != null) {
            element.next_date = element.next_date
              .split(",")
              .forEach((item1) => {
                arrN.push(new Date(item1));
              });
            element.next_date = arrN;
          }
          let check = 0;
          if (element.next_date) {
            element.next_date.forEach((item) => {
              let dateR = new Date(moment(item).format("YYYY/MM/DD")).toJSON();
              if (arrDateBw.includes(dateR)) {
                if (
                  dateR ==
                  new Date(moment(new Date()).format("YYYY/MM/DD")).toJSON()
                )
                  check +=
                    Math.floor(timeNow.getTime() / 60000) -
                    Math.floor(
                      new Date(moment(item).format("YYYY/MM/DD")).getTime() /
                        60000
                    );
                else check += 1440;
              }
            });
          }
          element.taskTime =
            Math.floor(timeNow.getTime() / 60000) -
            check -
            Math.floor(timeSpan.getTime() / 60000);
          if (element.taskTime < 0) element.taskTime = 0;
          if (
            (element.taskTime / 60).toFixed() > element.estimated_hours &&
            element.is_deadline == false
          )
            upDeadlineTask(element, true);
          if (
            element.is_deadline == true &&
            (element.taskTime / 60).toFixed() < element.estimated_hours
          )
            upDeadlineTask(element, false);

          if (element.keywords != null && element.keywords.length > 1) {
            if (!Array.isArray(element.keywords)) {
              element.keywords = element.keywords.split(",");
            }
          }
          if (
            element.test_user_ids != null &&
            element.test_user_ids.length > 1
          ) {
            if (!Array.isArray(element.test_user_ids)) {
              element.test_user_ids = element.test_user_ids.split(",");
            }
          }
          if (element.partner_id != null && element.partner_id.length > 1) {
            if (!Array.isArray(element.partner_id)) {
              element.partner_id = element.partner_id.split(",");
            }
          }
          if (!element.checkbug) element.checkbug = 0;
          listBugs.value.forEach((item) => {
            if (element.task_id == item.task_id && item.status == 2)
              element.checkbug = 2;
          });
          listBugs.value.forEach((item) => {
            if (element.task_id == item.task_id && item.status < 0)
              element.checkbug = 1;
          });
          if (!element.arrUsers) element.arrUsers = [];
          if (!element.userCount) element.userCount = 0;

          element.arrUsers.push({ avatar: element.avatar, iswork: true });
          if (Array.isArray(element.test_user_ids))
            element.test_user_ids.forEach((item1, index) => {
              if (index < 1) {
                let us = listUsers.value.filter((x) => x.data.user_id == item1);
                if (us.length > 0)
                  element.arrUsers.push({
                    avatar: us[0].data.avatar,
                    iswork: false,
                  });
              } else {
                element.userCount++;
              }
            });
          datalists.value.push(element);
        });
        listTask.value = datalists.value;
        console.log(datalists.value);
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
  })();
};
//Đếm các công việc

const loadCount = (user_id, project_id, category_id) => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_main_count",
        par: [
          { par: "user_id", va: user_id },
          { par: "search", va: options.value.searchText },
          { par: "category_id", va: category_id ? category_id : null },
          { par: "project_id", va: project_id },
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "status", va: options.value.statusTask },
          { par: "start_date", va: null },
          { par: "end_date", va: null },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        options.value.totalRecords = data[0].totalRecords;
        totalRecoreds.value = data[0].totalRecords;
        options.value.finishedRecord = data[0].finishedRecord;
        options.value.waitedRecord = data[0].waitedRecord;
        options.value.tempClose = data[0].tempClose;
        options.value.unFinishRecord = data[0].unFinishRecord;
        options.value.outOfDate = data[0].outOfDate;
        options.value.overTime = data[0].overTime;
        sttTask.value = data[0].totalRecords + 1;

        if (options.value.totalRecords > options.value.PageSize)
          checkPage.value = true;
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
//Reload Dữ liệu

const onRefreshTask = () => {
  options.value.searchText = null;
  taskDateFilter.value = null;
  weekPickerFilter.value = [];
  monthPickerFilter.value = null;
  userFilter.value = store.getters.user.user_id;
  onTaskFilter(null);
};
//Load Các user trong cty
const liUsers = ref();
//Sidebar người dùng để lọc

//Load dữ liệu khi bắt đầu

const loadProject = (check) => {
  if (check) {
    loadCount(store.getters.user.user_id, null, categoryIdSave.value);
    loadTask(store.getters.user.user_id, null, null);
  }
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
        proc: "ReportTaskAll",
        par: [
          { par: "sdate", va: options.value.Start_date },
          { par: "edate", va: options.value.End_date },
        ],
      },
      config
    )
    .then((response) => {
      let tables = JSON.parse(response.data.data);
      swal.close();
      console.log("dqsssssssssssssssssss", tables);
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
                  "<span style='margin-top:5px' class='status-" +
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
onMounted(() => {  if (!checkURL(window.location.pathname, store.getters.listModule)) {
    //  router.back();
  }
  loadProject(true);
  return {
    // datalists,
    // options,
    // onPage,
    // loadCount,
    // addTask,
    // closeDialog,
    // basedomainURL,
    // handleFileUpload,
    // saveData,
    // isFirst,
    // searchTask,
    // selectedTasks,
    // deleteList,
  };
});
</script>

<template>
  

  <div class="surface-100">
    <Splitter class="w-full">
      <SplitterPanel :size="20">
        <taskgroup />
      </SplitterPanel>
      <SplitterPanel :size="80">
        <div class="d-lang-table">
          <DataView
            class="w-full h-full e-sm flex flex-column"
            @page="onPage($event)"
            :rowsPerPageOptions="[10, 20, 50, 100]"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink  RowsPerPageDropdown"
            responsiveLayout="scroll"
            :scrollable="true"
            :layout="layout"
            :lazy="true"
            :value="datalists"
            :loading="options.loading"
            :paginator="totalRecoreds > options.PageSize || checkPage == true"
            :rows="options.PageSize"
            :totalRecords="totalRecoreds"
            :rowHover="true"
            :showGridlines="true"
            :pageLinkSize="options.PageSize"
            currentPageReportTemplate=""
          >
            <template #header>
              <div>
                <div class="flex pt-1">
                  <div class="w-2">
                    <h3 class="m-0">
                      <i class="pi pi-book"></i> Công việc
                      <span v-if="options.totalRecords > 0"
                        >({{ options.totalRecords }})</span
                      >
                    </h3>
                  </div>
                  <div class="w-8">
                    <div class="flex format-center w-full">
                      <Button
                        @click="onTaskFilter(null)"
                        :label="
                          'Tất cả: ' +
                          (options.totalRecords ? options.totalRecords : 0)
                        "
                        class="mx-2 text-0 p-button-secondary"
                        :style="
                          options.statusTask == null
                            ? 'border:3px solid cyan'
                            : ''
                        "
                      />
                      <Button
                        @click="onTaskFilter(2)"
                        :label="
                          'Đang đợi test: ' +
                          (options.waitedRecord ? options.waitedRecord : 0)
                        "
                        class="p-button-help mx-2 text-0"
                        :style="
                          options.statusTask == 2 ? 'border:3px solid cyan' : ''
                        "
                      />

                      <Button
                        @click="onTaskFilter(3)"
                        :label="
                          'Hoàn thành: ' +
                          (options.finishedRecord ? options.finishedRecord : 0)
                        "
                        class="p-button-success mx-2 text-0"
                        :style="
                          options.statusTask == 3 ? 'border:3px solid cyan' : ''
                        "
                      />
                      <Button
                        @click="onTaskFilter(-1)"
                        :label="
                          'Đang làm: ' +
                          (options.unFinishRecord ? options.unFinishRecord : 0)
                        "
                        class="mx-2 text-0"
                        :style="
                          options.statusTask == -1
                            ? 'border:3px solid cyan'
                            : ''
                        "
                      />
                      <Button
                        @click="onTaskFilter(6)"
                        :label="
                          'Tạm đóng: ' +
                          (options.tempClose ? options.tempClose : 0)
                        "
                        class="p-button-warning mx-2 text-0"
                        :style="
                          options.statusTask == 6 ? 'border:3px solid cyan' : ''
                        "
                      />
                      <Button
                        @click="onTaskFilter(7)"
                        :label="
                          'Qúa hạn: ' +
                          (options.outOfDate ? options.outOfDate : 0)
                        "
                        class="p-button-danger mx-2 text-0"
                        :style="
                          options.statusTask == 7 ? 'border:3px solid cyan' : ''
                        "
                      />
                      <Button
                        @click="onTaskFilter(8)"
                        :label="
                          'Ngoài giờ: ' +
                          (options.overTime ? options.overTime : 0)
                        "
                        class="bg-blue-600 mx-2 text-0"
                        :style="
                          options.statusTask == 8 ? 'border:3px solid cyan' : ''
                        "
                      />
                    </div>
                  </div>
                  <div class="w-2 relative">
                    <groupuser />
                  </div>
                </div>

                <Toolbar class="w-full custoolbar pt-5">
                  <template #start>
                    <span class="p-input-icon-left mr-2">
                      <i class="pi pi-search" />
                      <InputText
                        type="text"
                        class="p-inputtext-sm"
                        spellcheck="false"
                        placeholder="Tìm kiếm"
                        v-model="options.searchText"
                        @keyup.enter="onTaskFilter(null)"
                      />
                    </span>

                    <Calendar
                      placeholder="Lọc theo ngày"
                      id="range"
                      v-model="taskDateFilter"
                      :showIcon="true"
                      selectionMode="range"
                      :manualInput="false"
                    >
                      <template #footer>
                        <div class="w-full flex">
                          <div class="w-4 format-center">
                            <span
                              @click="todayClick"
                              class="cursor-pointer text-primary"
                              >Hôm nay</span
                            >
                          </div>
                          <div class="w-4 format-center">
                            <Button
                              @click="onDayClick"
                              label="Thực hiện"
                            ></Button>
                          </div>
                          <div class="w-4 format-center">
                            <span
                              @click="delDayClick"
                              class="cursor-pointer text-primary"
                              >Xóa</span
                            >
                          </div>
                        </div>
                      </template>
                    </Calendar>

                    <Button
                      type="button"
                      class="ml-2 p-button-outlined p-button-secondary"
                      icon="pi pi-filter"
                      @click="toggleFilterMonth($event)"
                      aria-haspopup="true"
                      aria-controls="overlay_panelMonth"
                    />
                  </template>

                  <template #end>
                    <DataViewLayoutOptions @click="onNewVersion" class="mr-2" />
                    <Button
                      v-if="nodeSelected || check_CateNull"
                      @click="addTask('Thêm công việc')"
                      label="Thêm mới"
                      icon="pi pi-plus"
                      class="p-button-sm mr-2"
                    />
                    <Button
                      class="
                        mr-2
                        p-button-sm p-button-outlined p-button-secondary
                      "
                      @click="onRefreshTask(null)"
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
                    <Menu id="overlay_Export" ref="projectButs" :popup="true" />
                  </template>
                </Toolbar>
              </div>
            </template>
            <template #grid="slotProps">
              <div class="col-12 md:col-3 p-2">
                <Card class="no-paddcontent">
                  <template #title> </template>

                  <template #content>
                    <div
                      class="text-center cursor-pointer"
                      @click="showDetails(slotProps)"
                    >
                      <div>
                        <div v-if="slotProps.data.task_id">
                          <div
                            class="text-lg text-blue-400 font-bold pb-2"
                            style="word-break: break-all"
                          >
                            {{ slotProps.data.task_name }}
                          </div>
                          <div v-html="slotProps.data.title"></div>
                        </div>
                        <div v-else>
                          <div
                            class="mb-1 text-lg text-blue-400 font-bold"
                            style="word-break: break-all"
                          >
                            {{ slotProps.data.category_name }}
                          </div>
                        </div>
                      </div>
                    </div>
                  </template>
                </Card>
              </div>
            </template>
            <template #list="slotProps">
              <div class="w-full" :class="'row ' + slotProps.data.active">
                <div
                  :style="
                    slotProps.data.checkbug == 1
                      ? 'border-left:8px solid red'
                      : slotProps.data.status == 2
                      ? 'border-left:8px solid #9C27B0'
                      : slotProps.data.status == 3
                      ? 'border-left:8px solid #689f38'
                      : 'border-left:8px solid white'
                  "
                  class="flex align-items-center justify-content-center"
                >
                  <div
                    class="
                      flex flex-column flex-grow-1
                      surface-0
                      m-2
                      border-round-xs
                    "
                    :class="'row ' + slotProps.data.active"
                  >
                    <div class="col-12 field flex p-0 m-0 px-2">
                      <div
                        class="col-8 px-0 m-0 cursor-pointer"
                        @click="showDetails(slotProps)"
                      >
                        <div class="col-12 p-0 m-0 flex">
                          <div class="col-1 p-0">
                            <AvatarGroup>
                              <Avatar
                                v-for="(item, index) in slotProps.data.arrUsers"
                                :key="index"
                                :image="
                                  item.avatar
                                    ? basedomainURL + item.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                :style="
                                  item.iswork == false
                                    ? 'border: 3px solid white'
                                    : 'border: 3px solid green'
                                "
                                shape="circle"
                                class="cursor-pointer"
                              />
                              <Avatar
                                v-if="slotProps.data.userCount > 0"
                                :label="'+' + slotProps.data.userCount + ''"
                                shape="circle"
                                style="
                                  background-color: green;
                                  color: #ffffff;
                                  font-size: 9px;
                                "
                              />
                            </AvatarGroup>
                          </div>
                          <div class="col-11 p-0 font-bold text-xl">
                            {{ slotProps.data.task_name }}
                            <span v-if="slotProps.data.is_outtime">
                              <font-awesome-icon
                                class="ml-3"
                                style="
                                  -moz-transform: scaleX(-1);
                                  color: red;
                                  -o-transform: scaleX(-1);
                                  -webkit-transform: scaleX(-1);
                                  transform: scaleX(-1);
                                  filter: FlipH;
                                  -ms-filter: 'FlipH';
                                "
                                icon="fa-solid fa-clock"
                              />
                            </span>
                          </div>
                        </div>
                        <div class="col-12 p-0 m-0 flex pt-4">
                          <div class="col-4 p-0">
                            <div>
                              <Button
                                :label="
                                  slotProps.data.is_important == 0
                                    ? 'Không quan trọng'
                                    : slotProps.data.is_important == 1
                                    ? 'Bình thường'
                                    : slotProps.data.is_important == 2
                                    ? 'Gấp'
                                    : slotProps.data.is_important == 3
                                    ? 'Rất gấp'
                                    : 'Bình thường'
                                "
                                class="p-button-raised"
                                style="height: 2em"
                                :class="
                                  slotProps.data.is_important == 0
                                    ? ' p-button-secondary'
                                    : slotProps.data.is_important == 1
                                    ? ' p-button-success'
                                    : slotProps.data.is_important == 2
                                    ? ' p-button-warning'
                                    : slotProps.data.is_important == 3
                                    ? ' p-button-danger'
                                    : ' p-button-danger'
                                "
                              />
                            </div>
                          </div>
                          <div
                            class="col-4 p-0"
                            style="
                              display: flex;
                              justify-content: left;
                              align-items: center;
                              vertical-align: middle;
                            "
                          >
                            <div v-if="slotProps.data.created_date">
                              <i
                                class="pi pi-calendar text-color-secondary"
                                v-tooltip.top="'Ngày cập nhật'"
                              ></i>
                              {{
                                moment(
                                  new Date(slotProps.data.modified_date)
                                ).format("DD/MM/YYYY")
                              }}
                            </div>
                          </div>
                          <div
                            class="col-4 p-0"
                            style="
                              display: flex;
                              justify-content: left;
                              align-items: center;
                              vertical-align: middle;
                            "
                          >
                            <div v-if="slotProps.data.created_by">
                              <i
                                class="pi pi-user text-color-secondary"
                                v-tooltip.top="'Người thực hiện'"
                              ></i>
                              {{ slotProps.data.full_name }}
                            </div>
                          </div>
                        </div>
                      </div>
                      <div class="col-4 p-0 m-0 flex">
                        <div class="col-6 p-0 format-center">
                          <div class="format-center w-full">
                            <Button
                              class="w-9 p-0 h-2rem"
                              :label="
                                slotProps.data.status == 0
                                  ? 'Đang lập kế hoạch'
                                  : slotProps.data.status == 1
                                  ? 'Đang làm'
                                  : slotProps.data.status == 2
                                  ? 'Đang đợi Test'
                                  : slotProps.data.status == 3
                                  ? 'Đã Test OK'
                                  : slotProps.data.status == 4
                                  ? 'Test Chưa OK'
                                  : slotProps.data.status == 5
                                  ? 'Phát sinh thêm'
                                  : slotProps.data.status == 6
                                  ? 'Tạm đóng'
                                  : 'Trạng thái'
                              "
                              @click="toggleStatus(slotProps.data, $event)"
                              aria:haspopup="true"
                              aria-controls="overlay_panelS"
                              :class="
                                slotProps.data.status == 0
                                  ? 'p-button-raised  p-button-secondary'
                                  : slotProps.data.status == 1
                                  ? 'p-button-raised'
                                  : slotProps.data.status == 2
                                  ? 'p-button-raised p-button-help'
                                  : slotProps.data.status == 3
                                  ? 'p-button-raised p-button-success'
                                  : slotProps.data.status == 4
                                  ? 'p-button-raised p-button-danger'
                                  : slotProps.data.status == 5
                                  ? 'p-button-raised p-button-secondary'
                                  : slotProps.data.status == 6
                                  ? 'p-button-raised p-button-warning'
                                  : 'p-button-raised'
                              "
                            />
                          </div>

                          <OverlayPanel
                            ref="op"
                            appendTo="body"
                            :showCloseIcon="false"
                            id="overlay_panelS"
                            style="width: 250px"
                            :breakpoints="{ '960px': '20vw' }"
                          >
                            <div>
                              <div v-for="item in listStatus" :key="item.code">
                                <Button
                                  :label="item.name"
                                  :class="item.css"
                                  class="w-full mb-1"
                                  @click="
                                    onProductSelect(item, taskDetails, false)
                                  "
                                />
                              </div>
                            </div>
                          </OverlayPanel>
                        </div>
                        <div class="col-5 p-0 flex">
                          <div
                            class="format-center w-5"
                            @click="
                              showBugs(slotProps.data.task_id, slotProps.data)
                            "
                          >
                            <div
                              v-if="
                                slotProps.data.checkbug == 1 ||
                                slotProps.data.checkbug == null
                              "
                              class="relative"
                            >
                              <img
                                src="/src/assets/image/bug.png"
                                alt=""
                                width="32"
                                height="32"
                                class="cursor-pointer"
                              />
                            </div>
                            <div v-else>
                              <img
                                v-if="slotProps.data.checkbug == 2"
                                src="/src/assets/image/closebug.png"
                                alt=""
                                width="32"
                                height="32"
                                class="cursor-pointer"
                              />
                              <img
                                v-if="slotProps.data.checkbug == 0"
                                src="/src/assets/image/nobug.png"
                                alt=""
                                width="32"
                                height="32"
                                class="cursor-pointer"
                              />
                            </div>
                            <div
                              v-if="
                                store.getters.user.user_id ==
                                  slotProps.data.user_id &&
                                slotProps.data.is_view_test
                              "
                            >
                              <img
                                src="/src/assets/image/notify.gif"
                                alt=""
                                width="32"
                                height="32"
                                class="cursor-pointer"
                              />
                            </div>

                            <div
                              v-if="
                                slotProps.data.test_user_ids.includes(
                                  store.getters.user.user_id
                                ) && slotProps.data.is_view_work
                              "
                            >
                              <img
                                src="/src/assets/image/notify.gif"
                                alt=""
                                width="32"
                                height="32"
                                class="cursor-pointer"
                              />
                            </div>
                          </div>
                          <div
                            class="w-7 format-center"
                            v-if="
                              slotProps.data.taskTime != null &&
                              slotProps.data.estimated_hours != null &&
                              slotProps.data.status != 3
                            "
                          >
                            <Button
                              :label="
                                (slotProps.data.taskTime / 60).toFixed() +
                                '/' +
                                slotProps.data.estimated_hours
                              "
                              class="p-button-rounded format-center h-2rem"
                              :class="
                                (slotProps.data.taskTime / 60).toFixed() >
                                slotProps.data.estimated_hours
                                  ? 'p-button-danger'
                                  : slotProps.data.estimated_hours -
                                      (
                                        slotProps.data.taskTime / 60
                                      ).toFixed() <=
                                    1
                                  ? 'p-button-warning'
                                  : 'p-button-success'
                              "
                            />
                          </div>
                        </div>
                        <div class="col-1 p-0">
                          <div
                            v-if="
                              (slotProps.data.status == 0 ||
                                slotProps.data.status == 6) &&
                              store.getters.user.user_id ==
                                slotProps.data.created_by
                            "
                            class="w-2"
                          >
                            <Button
                              icon="pi pi-ellipsis-h"
                              class="
                                p-button-outlined p-button-secondary
                                ml-2
                                border-none
                              "
                              @click="
                                toggleMores($event, slotProps.data, false)
                              "
                              aria-haspopup="true"
                              aria-controls="overlay_More1"
                            />
                            <Menu
                              id="overlay_More1"
                              ref="menuButMores"
                              :popup="true"
                              :model="itemButMores"
                            />
                          </div>
                        </div>
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
                v-if="!isFirst"
              >
                <img src="../../assets/background/nodata.png" height="144" />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </template>
          </DataView>
          <div></div>
        </div>
      </SplitterPanel>
    </Splitter>

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
    <OverlayPanel
      ref="filterMonth"
      appendTo="body"
      class="p-0 m-0"
      :showCloseIcon="false"
      id="overlay_panelMonth"
      style="width: 300px"
    >
      <div class="grid formgrid m-0">
        <div class="flex field col-12 p-0">
          <Datepicker
            @closed="onFilterMonth(true, $event)"
            @open="onClearMonth()"
            selectText="Thực hiện"
            cancelText="Hủy"
            class="w-full"
            locale="vi"
            placeholder=" Lọc theo tuần"
            v-model="weekPickerFilter"
            weekPicker
          >
            <template #clear-icon>
              <Button
                @click="onCleanFilterMonth"
                icon="pi pi-times"
                class="p-button-rounded p-button-text"
              />
            </template>
            <template #input-icon>
              <Button class="mr-2 p-button-text" icon="pi pi-calendar" />
            </template>
          </Datepicker>
        </div>

        <div class="flex field col-12 p-0">
          <Datepicker
            @closed="onFilterMonth(false)"
            @open="onClearMonth()"
            class="w-full"
            locale="vi"
            selectText="Thực hiện"
            cancelText="Hủy"
            placeholder=" Lọc theo tháng"
            v-model="monthPickerFilter"
            monthPicker
            ><template #clear-icon>
              <Button
                @click="onCleanFilterMonth"
                icon="pi pi-times"
                class="p-button-rounded p-button-text"
              />
            </template>
            <template #input-icon>
              <Button icon="pi pi-calendar" class="p-button-text" />
            </template>
          </Datepicker>
        </div>
      </div>
    </OverlayPanel>
  </div>
  <detailstask :isCheckTask="isCheckTask" :task="task" />
  <taskbug
    :project_id="projectID_Save"
    :isShowBug="isShowBug"
    :task="taskSave"
  />
  <Dialog
    :header="headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '50vw' }"
   @hide="reloadTask(store.getters.user.user_id, true)"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left"
            >Tên công việc <span class="redsao">(*)</span></label
          >
          <InputText
            v-model="task.task_name"
            spellcheck="false"
            class="col-10 ip36 px-2"
            :class="{ 'p-invalid': v$.task_name.$invalid && submitted }"
          />
        </div>
        <div
          v-if="
            (v$.task_name.$invalid && submitted) ||
            v$.task_name.$pending.$response
          "
          class="field col-12 md:col-12 p-0 flex"
        >
          <div class="col-2 text-left p-0"></div>
          <small class="col-10 p-0 p-error">
            <span class="col-12 p-0">{{
              v$.task_name.required.$message
                .replace("Value", "Tên công việc")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div class="field col-12 md:col-12" v-if="isEditWork">
          <label class="col-2 text-left">Nhóm công việc</label>

          <TreeSelect
            @change="changeCateSelect"
            class="col-10"
            v-model="dropdownSel"
            :options="listCateSelected"
            placeholder="Chọn nhóm công việc"
          />
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left pt-2">Ngày bắt đầu</label>
          <div class="col-4 p-0 flex">
            <Calendar
              :showIcon="true"
              id="time24"
              :showTime="true"
              autocomplete="on"
              v-model="task.estimated_date"
            />
            <Button
              @click="showEmoji($event, 5)"
              v-tooltip.top="'Chọn ngày nghỉ!'"
              icon="pi pi-calendar"
              class="mx-1 p-button-danger"
            >
            </Button>
            <OverlayPanel
              class="p-0"
              ref="panelCalendar"
              appendTo="body"
              :showCloseIcon="false"
              id="overlay_panelCalendar"
            >
              <Calendar
                id="showNextDate"
                selectionMode="multiple"
                :manualInput="false"
                :showWeek="true"
                :inline="true"
                autocomplete="on"
                :minDate="
                  task.estimated_date
                    ? task.estimated_date
                    : new Date('1970/01/01')
                "
                v-model="task.next_date"
              >
              </Calendar>
            </OverlayPanel>
          </div>
          <label class="col-2 text-left pt-2">Trạng thái </label>
          <Dropdown
            v-model="task.status"
            :options="listStatus"
            optionLabel="name"
            optionValue="code"
            placeholder="Trạng thái"
            spellcheck="false"
            class="col-4 ip36 p-0"
          >
          </Dropdown>
        </div>
        <div class="field flex col-12 md:col-12">
          <label class="col-2 text-left pt-2">Thời gian làm</label>

          <div class="col-4 flex p-0">
            <InputNumber
              v-model="task.estimated_hours"
              class="col-6 ip36 p-0 pr-1"
            ></InputNumber>
            <Dropdown
              v-model="typeTime"
              :options="listWorkTime"
              optionLabel="name"
              optionValue="code"
              spellcheck="false"
              class="col-6 ip36 p-0"
            />
          </div>

          <label class="col-2 text-left pt-2">Mức độ</label>
          <Dropdown
            v-model="task.is_important"
            :options="listImportant"
            optionLabel="name"
            optionValue="code"
            placeholder="Chọn mức độ"
            spellcheck="false"
            class="col-4 ip36 p-0"
          />
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-6 p-0">
            <div class="col-12 flex align-items-center p-0">
              <label class="col-4 text-left">Người thực hiện</label>
              <Dropdown
                :filter="true"
                v-model="task.user_id"
                :options="listDropdownUser"
                optionLabel="name"
                optionValue="code"
                placeholder="Người thực hiện"
                spellcheck="false"
                class="col-8 ip36 p-0"
                @change="changeWorkUser"
              >
                <template #option="slotProps">
                  <div class="country-item flex">
                    <Avatar
                      :image="
                        slotProps.option.avatar
                          ? basedomainURL + slotProps.option.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      class="mr-2 w-2rem h-2rem"
                      size="large"
                      shape="circle"
                    />
                    <div class="pt-1">{{ slotProps.option.name }}</div>
                  </div>
                </template>
              </Dropdown>
            </div>
          </div>
          <div class="col-6 p-0">
            <div class="field col-12 flex align-items-center p-0">
              <label class="col-4 p-0 text-left pl-2"
                >Người kiểm tra <span class="redsao">(*)</span></label
              >

              <MultiSelect
                :filter="true"
                v-model="task.test_user_ids"
                :options="listDropdownUserCheck"
                optionValue="code"
                optionLabel="name"
                class="col-8 ip36 p-0"
                placeholder="Người kiểm tra"
                :class="{
                  'p-invalid': task.test_user_ids == null && submitted,
                }"
              >
                <template #option="slotProps">
                  <div class="country-item flex">
                    <Avatar
                      :image="
                        slotProps.option.avatar
                          ? basedomainURL + slotProps.option.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      class="mr-2 w-2rem h-2rem"
                      size="large"
                      shape="circle"
                    />
                    <div class="pt-1">{{ slotProps.option.name }}</div>
                  </div>
                </template>
              </MultiSelect>
            </div>
            <div
              v-if="task.test_user_ids == null && submitted"
              class="col-12 md:col-12 flex pt-2"
            >
              <div class="col-4 text-left"></div>
              <small class="col-8 p-0 p-error">
                <span class="col-12 p-0"
                  >Người kiểm tra không được để trống!</span
                >
              </small>
            </div>
          </div>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Đồng hành</label>

          <MultiSelect
            :filter="true"
            v-model="task.partner_id"
            :options="listDropdownUserCheck"
            optionValue="code"
            optionLabel="name"
            class="col-10 ip36 p-0"
            placeholder="Người cùng làm công việc!"
          >
            <template #option="slotProps">
              <div class="country-item flex">
                <Avatar
                  :image="
                    slotProps.option.avatar
                      ? basedomainURL + slotProps.option.avatar
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                  class="mr-2 w-2rem h-2rem"
                  size="large"
                  shape="circle"
                />
                <div class="pt-1">{{ slotProps.option.name }}</div>
              </div>
            </template>
          </MultiSelect>
        </div>
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left m-0">Mô tả</label>
          <div class="col-10 p-0">
            <QuillEditor
              class="w-full border-1 border-solid border-round-md border-400"
              ref="comment_zone"
              placeholder="Mô tả..."
              contentType="html"
              :content="task.des"
              v-model:content="task.des"
              theme="bubble"
            />

            <!-- <Editor
              spellcheck="false"
              v-model="task.des"
              editorStyle="height: 120px"
            /> -->
          </div>
        </div>
        <div class="col-12 flex field">
          <label class="col-2 text-left">File</label>
          <div class="col-10 p-0">
            <FileUpload
              chooseLabel="Chọn File"
              :showUploadButton="false"
              :showCancelButton="false"
              :multiple="true"
              accept=".zip,.rar"
              :maxFileSize="10000000"
              @select="onUploadFile"
              @remove="removeFile"
            />
          </div>
        </div>
        <div class="col-12 p-0 flex field">
          <label class="col-2 text-left"></label>
          <div class="col-10 p-0" v-if="task.url_file">
            <div
              v-for="(item, index) in task.url_file"
              :key="index"
              class="flex"
            >
              <Toolbar class="w-full py-3">
                <template #start>
                  <div class="flex">
                    <img
                      :src="basedomainURL + '/Portals/Image/rar.png'"
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
                <template #end>
                  <Button
                    icon="pi pi-times"
                    class="p-button-rounded p-button-danger"
                    @click="deleteFileCode(item)"
                  />
                </template>
              </Toolbar>
            </div>
          </div>
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="col-4 field md:col-4 p-0">
            <label class="col-6 text-left">STT</label>
            <InputNumber v-model="task.is_order" class="col-6 ip36 p-0" />
          </div>

          <div class="col-3 p-0 flex pt-1">
            <label style="vertical-align: text-bottom" class="col-5 text-center"
              >Kế hoạch
            </label>
            <InputSwitch class="col-3" v-model="task.is_plan" />
          </div>
          <div class="col-3 flex p-0">
            <label style="vertical-align: text-bottom" class="col-5 text-center"
              >Ngoài giờ
            </label>
            <InputSwitch class="col-3" v-model="task.is_outtime" />
          </div>
        </div>

        <div class="field col-12 md:col-12">
          <label class="col-2 text-left">Từ khóa</label>
          <Chips
            v-model="task.keywords"
            spellcheck="false"
            class="col-10 ip36 p-0"
            placeholder="Ấn Enter sau mỗi từ khóa!"
          />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog"
        class="p-button-text"
      />
      <Button label="Lưu" icon="pi pi-check" @click="saveData()" autofocus />
    </template>
  </Dialog>
  <Sidebar
    v-model:visible="isCheckTest"
    :baseZIndex="100"
    position="right"
    class="p-sidebar-lg"
    :showCloseIcon="false"
  >
    <div v-if="!isDetailsTest">
      <div class="grid w-full p-0">
        <div class="col-12 p-0 flex">
          <div class="col-10 p-0 px-2">
            <h2>Danh sách lần Test</h2>
          </div>
          <div class="col-2 p-0 format-center">
            <Button
              v-if="
                taskDetails.test_user_ids.filter(
                  (x) => x == store.getters.user.user_id
                ).length > 0
              "
              :label="
                taskDetails.status == 0
                  ? 'Đang lập kế hoạch'
                  : taskDetails.status == 1
                  ? 'Đang làm'
                  : taskDetails.status == 2
                  ? 'Đang đợi Test'
                  : taskDetails.status == 3
                  ? 'Đã Test OK'
                  : taskDetails.status == 4
                  ? 'Test Chưa OK'
                  : taskDetails.status == 5
                  ? 'Phát sinh thêm'
                  : taskDetails.status == 6
                  ? 'Tạm đóng'
                  : 'Trạng thái'
              "
              @click="toggleStatusDetails(taskDetails, $event)"
              aria:haspopup="true"
              aria-controls="overlay_panelS"
              :class="
                taskDetails.status == 0
                  ? 'p-button-raised  p-button-secondary'
                  : taskDetails.status == 1
                  ? 'p-button-raised'
                  : taskDetails.status == 2
                  ? 'p-button-raised p-button-help'
                  : taskDetails.status == 3
                  ? 'p-button-raised p-button-success'
                  : taskDetails.status == 4
                  ? 'p-button-raised p-button-danger'
                  : taskDetails.status == 5
                  ? 'p-button-raised p-button-secondary'
                  : taskDetails.status == 6
                  ? 'p-button-raised p-button-warning'
                  : 'p-button-raised'
              "
            />
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
        </div>
      </div>
      <DataView
        class="w-full h-full e-sm flex flex-column"
        responsiveLayout="scroll"
        :scrollable="true"
        :layout="layout"
        :lazy="true"
        :value="listUsertest"
        :loading="options.loading"
      >
        <template #header> </template>
        <template #list="slotProps">
          <div class="grid w-full p-0">
            <div class="field col-12 flex m-0 cursor-pointer">
              <div class="col-1">
                <Avatar
                  :image="
                    slotProps.data.avatar
                      ? basedomainURL + slotProps.data.avatar
                      : basedomainURL + '/Portals/Image/noimg.jpg'
                  "
                  class="w-4rem h-4rem"
                  size="large"
                  shape="circle"
                />
              </div>
              <div class="col-4 pt-2" @click="showAddTest(slotProps.data)">
                <div class="pt-2">
                  <div class="font-bold">
                    Người gửi: {{ slotProps.data.full_name }}
                  </div>
                  <div>
                    Ngày gửi:
                    {{
                      moment(new Date(slotProps.data.created_date)).format(
                        "DD/MM/YYYY HH:mm:ss"
                      )
                    }}
                  </div>
                  <div v-html="slotProps.data.test_content"></div>
                </div>
              </div>
              <div class="col-2" @click="showAddTest(slotProps.data)">
                <div class="pt-2">
                  <Button
                    class="w-7rem"
                    :label="
                      slotProps.data.test_pass == true
                        ? 'Đã pass'
                        : slotProps.data.test_pass == null
                        ? 'Đang test'
                        : 'Chưa pass'
                    "
                    :class="
                      slotProps.data.test_pass == true
                        ? 'p-button-rounded  p-button-success'
                        : slotProps.data.test_pass == null
                        ? 'p-button-rounded  p-button-warning'
                        : 'p-button-rounded  p-button-danger'
                    "
                  />
                </div>
              </div>
              <div class="col-4 flex">
                <AvatarGroup class="mb-3">
                  <Avatar
                    v-for="(item, index) in slotProps.data.users"
                    :key="index"
                    :image="basedomainURL + item.avatar"
                    size="large"
                    shape="circle"
                  />
                  <Avatar
                    class=""
                    v-if="slotProps.data.totalUser"
                    :label="slotProps.data.totalUser + ''"
                    shape="circle"
                    size="large"
                    style="background-color: cadetblue; color: #ffffff"
                  />
                  <Avatar
                    class="ml-3"
                    @click="showChildTest(slotProps.data)"
                    :label="slotProps.data.CountChild + ''"
                    shape="circle"
                    size="large"
                    style="background-color: green; color: #ffffff"
                  />
                </AvatarGroup>
              </div>
              <div class="col-1">
                <Button
                  v-if="
                    store.getters.user.user_id == slotProps.data.test_user_id ||
                    store.getters.user.is_admin
                  "
                  icon="pi pi-ellipsis-h"
                  class="p-button-outlined p-button-secondary ml-2 border-none"
                  @click="toggleTaskTest($event, slotProps.data)"
                  aria-haspopup="true"
                  aria-controls="overlay_MoreTask"
                />

                <Menu
                  id="overlay_MoreTask"
                  ref="menuButTaskTest"
                  :model="itemButMoresTaskTest"
                  :popup="true"
                />
              </div>
            </div>
          </div>
        </template>
      </DataView>
    </div>
    <div v-else>
      <div v-if="!isDetailsTaskTest" class="relative comment-height">
        <div class="fixed top-0 flex">
          <div class="flex format-center">
            <Button
              icon="pi
pi-arrow-left"
              class="h-2rem pt-2"
              @click="preListTask()"
            ></Button>
            <h2 class="ml-2">Danh sách người Test</h2>
          </div>
        </div>

        <div>
          <Toolbar
            class="w-full surface-0 outline-none border-none p-0 pt-2 pr-3"
          >
            <template #end>
              <div>
                <Button
                  @click="addTestChild"
                  icon="pi pi-plus"
                  class="p-button-sm mr-2"
                  label="Thêm mới"
                ></Button>
              </div>
            </template>
          </Toolbar>
        </div>
        <div>
          <DataView
            class="w-full h-full e-sm flex flex-column"
            responsiveLayout="scroll"
            :scrollable="true"
            :layout="layout"
            :lazy="true"
            :value="listTestChild"
            :loading="options.loading"
          >
            <template #list="slotProps">
              <div class="grid w-full p-0">
                <div class="field col-12 flex m-0 cursor-pointer">
                  <div class="col-1">
                    <Avatar
                      :image="
                        slotProps.data.avatar
                          ? basedomainURL + slotProps.data.avatar
                          : basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      class="w-4rem h-4rem"
                      size="large"
                      shape="circle"
                    />
                  </div>
                  <div class="col-6 pt-2">
                    <div class="pt-2">
                      <div class="font-bold">
                        Người tạo: {{ slotProps.data.full_name }}
                      </div>
                      <div>
                        Ngày tạo:
                        {{
                          moment(new Date(slotProps.data.created_date)).format(
                            "DD/MM/YYYY HH:mm:ss"
                          )
                        }}
                      </div>
                      <div v-html="slotProps.data.test_content"></div>
                    </div>
                  </div>
                  <div class="col-2">
                    <div class="pt-2">
                      <Button
                        class="w-7rem"
                        :label="
                          slotProps.data.test_pass == true
                            ? 'Đã pass'
                            : slotProps.data.test_pass == null
                            ? 'Đang test'
                            : 'Chưa pass'
                        "
                        :class="
                          slotProps.data.test_pass == true
                            ? 'p-button-rounded  p-button-success'
                            : slotProps.data.test_pass == null
                            ? 'p-button-rounded  p-button-warning'
                            : 'p-button-rounded  p-button-danger'
                        "
                      />
                    </div>
                  </div>
                  <div class="col-2"></div>
                  <div class="col-1">
                    <Button
                      v-if="
                        store.getters.user.user_id ==
                          slotProps.data.test_user_id ||
                        store.getters.user.is_admin
                      "
                      icon="pi pi-ellipsis-h"
                      class="
                        p-button-outlined p-button-secondary
                        ml-2
                        border-none
                      "
                      @click="toggleTaskTest($event, slotProps.data)"
                      aria-haspopup="true"
                      aria-controls="overlay_MoreTask"
                    />

                    <Menu
                      id="overlay_MoreTask"
                      ref="menuButTaskTest"
                      :model="itemButMoresTaskTest"
                      :popup="true"
                    />
                  </div>
                </div>
              </div>
            </template>
          </DataView>
        </div>
      </div>
      <div v-else>
        <div class="fixed top-0 flex">
          <div class="flex format-center">
            <Button
              icon="pi
pi-arrow-left"
              class="h-2rem pt-2"
              @click="preListTask()"
            ></Button>
            <h2 class="ml-2">Chi tiết Test</h2>
          </div>
        </div>

        <div>
          Người tạo:
          <span class="font-semibold pr-1">{{ task_PreView.created_by }} </span>

          <timeago :datetime="task_PreView.created_date" :locale="vi" />
        </div>

        <hr />
        <div class="pl-2" v-html="task_PreView.test_content"></div>
        <div>
          <DataView
            class="w-full h-full e-sm flex flex-column"
            responsiveLayout="scroll"
            :scrollable="true"
            layout="list"
            :lazy="true"
            :value="listBugsDetails"
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
                      <span v-if="listBugsDetails.length > 0"
                        >({{ listBugsDetails.length }})</span
                      >
                    </h3>
                  </template>
                  <template #end> </template>
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
                        @click="showComment(slotProps.data)"
                        class="col-10 p-0 cursor-pointer"
                      >
                        <div
                          class="col-12 p-0 font-bold text-xl flex"
                          style="font-size: 1rem"
                        >
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
                            class="mb-1 font-bold text-xl px-2 pt-2"
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
                            <div
                              v-if="slotProps.data.status == 1"
                              class="mb-1 font-italic text-color-secondary"
                            >
                              <Tag severity="success">Đã sửa</Tag>
                            </div>
                            <div
                              v-else-if="slotProps.data.status == -1"
                              class="mb-1 font-italic text-color-secondary"
                            >
                              <Tag severity="info">Đang sửa</Tag>
                            </div>
                            <div>
                              <div
                                class="mb-1 font-italic text-color-secondary"
                              >
                                <Tag
                                  v-if="slotProps.data.status == -2"
                                  severity="danger"
                                  >Lỗi</Tag
                                >
                              </div>
                            </div>
                            <div>
                              <div
                                class="mb-1 font-italic text-color-secondary"
                              >
                                <Tag
                                  v-if="slotProps.data.status == 2"
                                  severity="warning"
                                  >Đã đóng</Tag
                                >
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                      <div class="col-2 text-right flex">
                        <Toolbar
                          class="w-full surface-0 outline-none border-none p-0"
                        >
                          <template #start> </template>
                          <!-- <template #end>
                              <div>
                                <Button
                                  icon="pi pi-ellipsis-h"
                                  class="
                                    p-button-outlined p-button-secondary
                                    ml-2
                                    border-none
                                  "
                                  @click="
                                    toggleBugsMores($event, slotProps.data)
                                  "
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
                            </template> -->
                        </Toolbar>
                      </div>
                    </div>
                    <div
                      @click="showComment(slotProps.data)"
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
    </div>
  </Sidebar>
  <Dialog
    v-model:visible="isShowAddTest"
    :style="{ width: '40vw' }"
    :header="headerAddTest"
    @hide="closeTestTask"
  >
     
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <label class="col-2 text-left p-0">Nội dung</label>
          <div class="col-10 p-0">
            <Textarea
              style="border-radius: 5px"
              class="w-full"
              spellcheck="false"
              :autoResize="true"
              rows="1"
              v-model="task_test.test_content"
            />
            <!-- <Editor
              v-model="task_test.test_content"
              editorStyle="height: 150px"
            /> -->
          </div>
        </div>
        <div class="field col-12 p-0 md:col-12 flex">
          <label class="col-2 text-left p-0 pt-2">Danh sách lỗi</label>
          <MultiSelect
            v-model="task_test.test_bugs"
            :options="listBugsTest"
            optionLabel="name"
            optionValue="code"
            placeholder="Chọn lỗi"
            class="col-7"
          >
          </MultiSelect>
          <label class="col-2 text-center p-0 pt-2">Trạng thái</label>
          <InputSwitch v-model="task_test.test_pass" class="pt-2" />
        </div>
        <div class="field col-12 md:col-12 flex">
          <div class="w-full">
            <DataTable
              :value="listQickBug"
              editMode="cell"
              class="editable-cells-table w-full"
              responsiveLayout="scroll"
            >
              <Column
                field="bug_name"
                header="Tên lỗi"
                headerStyle="padding-left:0px"
              >
                <template #editor="{ data, field }">
                  <InputText class="w-full" v-model="data[field]" />
                </template>
              </Column>
              <Column class="w-10rem" field="status" header="Trạng thái lỗi">
                <template #editor="{ data, field }">
                  <Dropdown
                    class="w-full"
                    v-model="data[field]"
                    :options="listStatusBugs"
                    optionLabel="name"
                    optionValue="code"
                    placeholder="Trạng thái"
                  >
                  </Dropdown>
                </template>
                <template #body="slotProps">
                  <div v-if="slotProps.data.status == -2">Lỗi</div>
                  <div v-if="slotProps.data.status == -1">Đang sửa</div>
                  <div v-if="slotProps.data.status == 1">Đã sửa</div>
                  <div v-if="slotProps.data.status == 2">Đã đóng</div>
                </template>
              </Column>
            </DataTable>
            <div v-if="isShowQickAdd" class="grid w-full pt-2">
              <div class="col-12 mt-2 flex">
                <div class="col-2 ip36 pt-2">Tên lỗi</div>
                <div class="col-6 p-0">
                  <InputText
                    @keyup.enter="addToListBug"
                    autofocus
                    v-model="qickBug.bug_name"
                    class="w-full"
                  />
                </div>
                <div class="col-2 p-0 pt-2 text-center">Trạng thái</div>
                <div class="col-2 p-0">
                  <Dropdown
                    class="w-full"
                    :options="listStatusBugs"
                    v-model="qickBug.status"
                    optionLabel="name"
                    optionValue="code"
                    placeholder="Trạng thái"
                  >
                  </Dropdown>
                </div>
                <!-- <div class="col-1 p-0 pl-2">
                  <Button
                    @click="addToListBug"
                    icon="pi
pi-arrow-right"
                  ></Button>
                </div> -->
              </div>
            </div>
            <Button
              label="Thêm mới lỗi"
              icon="pi pi-plus"
             @click="showQickAddBug"
              class="p-button-text"
            />
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeTestTask()"
        class="p-button-text"
      />

      <Button label="Lưu" icon="pi pi-check" @click="saveTestTask()" />
    </template>
  </Dialog>
</template>

<style scoped>
.d-lang-table {
  margin: 0px 8px 0px 8px;
  height: calc(100vh - 55px);
}
.inputanh {
  border: 1px solid #ccc;
  width: 224px;
  height: 128px;
  cursor: pointer;
  padding: 1px;
}
.ipnone {
  display: none;
}
.inputanh img {
  object-fit: cover;
  width: 100%;
  height: 100%;
}
.format-center {
  display: flex;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
}

.row.true {
  background-color: rgb(190, 211, 245) !important;
}
.comment-height-scroll {
  height: calc(100vh - 300px);
}
</style>
<style lang="scss" scoped>
::v-deep(.p-galleria-content) {
  .p-galleria-item-wrapper {
    height: 100%;
  }
  .p-galleria-thumbnail-container {
    padding: 4px 2px;
    background-color: rgb(195, 195, 195);
  }
  .p-galleria-thumbnail-next {
    display: block;
  }
  .p-galleria-thumbnail-prev {
    display: block;
  }
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
