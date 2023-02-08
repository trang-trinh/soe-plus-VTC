<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { VuemojiPicker } from "vuemoji-picker";
import taskgroup from "../../components/task/taskgroup.vue";
import groupuser from "../../components/task/groupuser.vue";
import detailstask from "../../components/task/detailstask.vue";
import taskbug from "../../components/task/taskbug.vue";
import taskchart from "../../components/task/taskchart.vue";
import taskreport from "../../components/task/taskreport.vue";
import commentCheckList from "../../components/news/comment.vue";
import moment from "moment";
import {   checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");

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
    case "hideChartSidebar":
      userFilter.value = store.getters.user.user_id;
      onTaskFilter(
        userFilter.value,
        options.value.statusTask,
        options.value.Start_Date,
        options.value.End_Date,
        options.value.typeCheckList,
        options.value.Date_Filter,
        true
      );
      typeChart.value = 0;
      isShowChart.value = false;
      break;
    case "onTaskFilter":
      onTaskFilter(
        obj.data.userFilter,
        obj.data.statusTask,
        obj.data.Start_Date,
        obj.data.End_Date,
        obj.data.typeCheckList,
        obj.data.Date_Filter,
        true
      );
      break;
      case "detailsBugcomment":
      showDetails(obj.data);
      break;
  }
});
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const arrFiles = ref([]);
const listImgPast = ref([]);
const projectID_Save = ref();
const valImg = ref([]);
const comment = ref("");
const optionsCommentTask = ref({
  isShowInput: true,
  isUploadFile: true,
  isReply: true,
  isReaction: true,
});
const task = ref({
  category_id: null,
  task_name: "",
  test_user_ids: "",
  user_id: "",
  des: null,
  estimated_date: null,
  estimated_hours: null,
  status: 0,
  parent_id: null,
  is_plan: false,
  keywords: "",
});
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

const selectedTasks = ref();
const submitted = ref(false);
const isSaveTask = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const checkDelList = ref(false);
const userFilter = ref(store.getters.user.user_id);
const chartMore = ref([
  {
    label: "Thời gian",
    icon: "pi pi-chart-line",
    command: () => {
      showChart(0);
    },
  },
  {
    label: "Cá nhân",
    icon: "pi pi-chart-line",
    command: () => {
      showChart(1);
    },
  },
]);
const options = ref({
  IsNext: true,
  sort: "task_id",
  searchText: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
  finishedRecord: null,
  waitedRecord: null,
  tempClose: null,
  unFinishRecord: null,
  statusTask: null,
  err_work: null,
  SearchTextUser: "",
  typeCheckList: 0,
  Date_Filter: 0,
  Start_Date: null,
  End_Date: null,
  status: null,
});

const liUsers = ref();
//Lấy số bản ghi
const loadCount = (user_id, project_id, category_id) => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_checklist_count",
        par: [
          { par: "user_id", va: user_id },
          { par: "search", va: options.value.searchText },
          { par: "category_id", va: category_id ? category_id : null },
          { par: "typefilter", va: options.value.typeCheckList },
          { par: "datefilter", va: options.value.Date_Filter },
          { par: "task_id", va: options.value.task_id },
          { par: "bug_id", va: options.value.bug_id },
          { par: "project_id", va: project_id },
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "status", va: options.value.statusTask },
          { par: "start_date", va: options.value.Start_Date },
          { par: "end_date", va: options.value.End_Date },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        dataChart.value = data[0];
        options.value.totalRecords = data[0].totalRecords;
        totalRecords.value = data[0].totalRecords;
        options.value.finishedRecord = data[0].finishedRecord;
        options.value.waitedRecord = data[0].waitedRecord;
        options.value.tempClose = data[0].tempClose;
        options.value.unFinishRecord = data[0].unFinishRecord;
        options.value.err_work = data[0].err_work;
        sttTask.value = data[0].totalRecords + 1;
        totalRecords.value = data[0].unFinishRecord;
        if (options.value.statusTask == null)
          totalRecords.value = data[0].totalRecords;
        if (options.value.statusTask == 7)
          totalRecords.value = data[0].add_work;
        if (options.value.statusTask == 3)
          totalRecords.value = data[0].finishedRecord;
        if (options.value.statusTask == 4)
          totalRecords.value = data[0].tempClose;
        if (options.value.statusTask == 2)
          totalRecords.value = data[0].waitedRecord;
        if (options.value.statusTask == 0)
          totalRecords.value = data[0].err_work;
        if (totalRecords.value == 0 || totalRecords.value == null)
          checkPage.value = false;
        if (totalRecords.value > options.value.PageSize) checkPage.value = true;
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
const checkPage = ref(false);
const loadCountSave = (user_id, project_id, category_id) => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_checklist_count",
        par: [
          { par: "user_id", va: user_id },
          { par: "search", va: options.value.searchText },
          { par: "category_id", va: category_id },
          { par: "typefilter", va: options.value.typeCheckList },
          { par: "datefilter", va: options.value.Date_Filter },
          { par: "task_id", va: options.value.task_id },
          { par: "bug_id", va: options.value.bug_id },
          { par: "project_id", va: project_id },
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "status", va: options.value.statusTask },
          { par: "start_date", va: options.value.Start_Date },
          { par: "end_date", va: options.value.Start_Date },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        totalRecords.value = data[0].unFinishRecord;
        if (options.value.statusTask == null)
          totalRecords.value = data[0].totalRecords;
        if (options.value.statusTask == 7)
          totalRecords.value = data[0].add_work;
        if (options.value.statusTask == 3)
          totalRecords.value = data[0].finishedRecord;
        if (options.value.statusTask == 4)
          totalRecords.value = data[0].tempClose;
        if (options.value.statusTask == 2)
          totalRecords.value = data[0].waitedRecord;
        if (options.value.statusTask == 0)
          totalRecords.value = data[0].err_work;

        if (totalRecords.value == 0 || totalRecords.value == null)
          checkPage.value = false;
        if (options.value.totalRecords > options.value.PageSize)
          checkPage.value = true;
      }
    })
    .catch((error) => {
      console.log(error);
    });
};

const opBugs = ref();
//Lấy dữ liệu công việc
//Phân trang dữ liệu
const onPage = (event) => {
  options.value.PageNo = event.page;
  options.value.PageSize = event.rows;
  reloadTask(userFilter.value, false);
};
//Hiển thị dialog
const listTask = ref([]);
const headerDialog = ref();
const displayBasic = ref(false);
const addTask = (str) => {
  files = [];

  submitted.value = false;

  task.value = {
    category_id: nodeSelected.value.category_id,
    task_name: "",
    test_user_ids: "",
    user_id: store.getters.user.user_id,
    des: "",
    estimated_date: new Date(),
    estimated_hours: null,
    status: 0,
    parent_id: null,
    is_order: options.value.totalRecords + 1,
    is_plan: false,
    keywords: "",
  };
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
//Lấy file logo
const chonanh = (id) => {
  document.getElementById(id).click();
};
const cmtBugId = ref();
const bugId = ref();
let filecoments = [];

const reloadCommentBug = (id) => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_commentbug_list",
        par: [
          { par: "bug_id", va: null },
          { par: "commentbug_id", va: id },
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
const checkFileCommentChild = ref(false);
const listFileCommentChild = ref([]);

//Thêm bản ghi
let files = [];
const sttTask = ref(1);
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
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

  if (typeTime.value == 3)
    task.value.estimated_hours = task.value.estimated_hours * 24;

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
const projectSelected = ref();
const listCategorySave = ref([]);
const listProject = ref([]);
const checkIsmain = ref(true);
//Sửa bản ghi
const editTask = () => {
  if (store.getters.user.user_id == bugInComment.value.created_by) {
    checkAddBugComment.value = false;
    arrFiles.value = [];
    isShowAddBugComment.value = true;
  } else {
    toast.warning("Bạn không có quyền cập nhật!");
  }
};
const isFirst = ref(true);

watch(selectedTasks, () => {
  if (selectedTasks.value.length > 0) {
    checkDelList.value = true;
  } else {
    checkDelList.value = false;
  }
});

const layout = ref("list");

const toggleMores = (event, u) => {
  bugInComment.value = u;
  menuButMores.value.toggle(event);
};

const menuButMores = ref();
const itemButMores = ref([
  {
    label: "Cập nhật",
    icon: "pi pi-cog",
    command: () => {
      editTask();
    },
  },

  {
    label: "Xoá",
    icon: "pi pi-trash",
    command: () => {
      deleteTask(bugInComment.value);
    },
  },
]);

const loadTask = (user_id, category_id, project_id) => {
  options.value.loading = true;
  datalists.value = [];
  (async () => {
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "task_checklist",
          par: [
            { par: "user_id", va: user_id },

            { par: "category_id", va: category_id },
            { par: "typefilter", va: options.value.typeCheckList },
            { par: "datefilter", va: options.value.Date_Filter },
            { par: "task_id", va: options.value.task_id },
          { par: "bug_id", va: options.value.bug_id },
            { par: "project_id", va: project_id },
            { par: "organization_id", va: store.getters.user.organization_id },
            { par: "pageno", va: options.value.PageNo },
            { par: "pagesize", va: options.value.PageSize },
            { par: "search", va: options.value.searchText },
            { par: "status", va: options.value.statusTask },
            { par: "start_date", va: options.value.Start_Date },
            { par: "end_date", va: options.value.End_Date },
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
              if (element != "")
                if (allowedExtensions.exec(element)) {
                  // Kiểm tra định dạng
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

          datalists.value.push(element);
        });

        listTask.value = datalists.value;

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
  })();
};

//Load lại công việc
const reloadTask = (user_id, check) => {
  if (check) loadCount(userFilter.value, null, categoryIdSave.value);
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

const deleteTask = (value) => {
  console.log("2s", bugInComment.value);

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

              loadTask(
                store.getters.user.user_id,
                nodeValue.value
                  ? nodeValue.value.category_id
                    ? nodeValue.value.category_id
                    : null
                  : null,
                nodeValue.value
                  ? nodeValue.value.code
                    ? nodeValue.value.code
                    : null
                  : null
              );

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

const listCategory = ref([]);
const database_name = ref();
const projectLogo = ref();


const renderCate = (listCate) => {
  let arrCate = [];
  listProject.value.forEach((element) => {
    if (element.code == "allPr") return;
    else {
      let arrChils = [];
      listCate
        .filter((x) => x.parent_id == null && x.project_id == element.code)
        .forEach((m) => {
          let om = { key: m.category_id, data: m, count: m.taskcount };
          const rechildren = (mm, category_id) => {
            if (!mm.children) mm.children = [];
            if (!mm.count) mm.count = 0;
            let dts = listCate.filter((x) => x.parent_id == category_id);
            if (dts.length > 0) {
              dts.forEach((em) => {
                om.count += em.taskcount;

                let om1 = {
                  key: em.category_id,
                  data: em,
                  count: em.taskcount,
                };
                rechildren(om1, em.category_id);

                mm.children.push(om1);
              });
            }
          };

          rechildren(om, m.category_id);
          arrChils.push(om);
        });

      arrCate.push({
        key: element.code,
        data: element,
        count: null,
        children: arrChils,
      });
    }

    listCategory.value = arrCate;
  });

  let arrReChild = [];
  listCate
    .filter((x) => x.parent_id == null)
    .forEach((m) => {
      let om = {
        key: m.category_id,
        label: m.category_name,
        data: m.category_id,
      };
      const rechildren = (mm, category_id) => {
        if (!mm.children) mm.children = [];
        let dts = listCate.filter((x) => x.parent_id == category_id);
        if (dts.length > 0) {
          dts.forEach((em) => {
            let om1 = {
              key: em.category_id,
              label: em.category_name,
              data: em.category_id,
            };
            rechildren(om1, em.category_id);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m.category_id);
      arrReChild.push(om);
    });
  listCateSelected.value = arrReChild;
};
const listCateSelected = ref([]);
const loadProject = () => {
  (async () => {
    listProject.value = [];
    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "api_project_list_api",
          par: [
            { par: "search", va: options.value.searchText },
            { par: "status", va: options.value.Status },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];

        let db1 = {
          name: "Tất cả dự án",
          code: "allPr",
          db_name: null,
          project_logo: "/Portals/Image/noimg.jpg",
        };
        projectSelected.value = db1.code;
        database_name.value = db1.db_name;
        projectLogo.value = db1.project_logo;
        listProject.value.push(db1);
        data.forEach((element) => {
          let db = {
            name: element.project_name,
            code: element.project_id,
            db_name: element.db_name,
            project_logo: element.project_logo,
          };
          listProject.value.push(db);
        });
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

    listCategory.value = [];
    listCategorySave.value = [];

    await axios
      .post(
        baseURL + "/api/Proc/CallProc",
        {
          proc: "task_category_list",
          par: [
            { par: "parent_id", va: options.value.parent_id },
            {
              par: "project_id",
              va:
                projectSelected.value == "allPr" ? null : projectSelected.value,
            },
            { par: "search", va: options.value.searchText },
            { par: "status", va: options.value.Status },
            { par: "user_id", va: store.getters.user.user_id },
          ],
        },
        config
      )
      .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        listCategorySave.value = data;
        renderCate(data);
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
    loadCount(store.getters.user.user_id, null, categoryIdSave.value);
    loadTask(store.getters.user.user_id, null, null);
  })();
};

const checkEditComment = ref(false);
const checkEditCommentChild = ref(false);
const bugComment = ref();
const dataCommentCheckList = ref();
const checkAddChildCmt = ref();

const onChangeStatusBug = (status) => {
  opbugcomment.value.hide();
  bugCommentSave.value.status = status.code;
  editBugCommentSave(bugCommentSave.value);
};
const bug = ref();
const showThumnFiles = (value) => {
  if (value.length > 1) {
    listThumFiles.value = value;
    isThumnFiles.value = true;
  }
};
const categoryIdSave = ref();
const selectedKey = ref([]);
const expandedKeys = ref({});
const nodeValue = ref();
const categoryName = ref();
const checkNode = ref(false);
const keyselected = ref(0);
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
  checkNode.value = true;
  nodeValue.value = node;
  options.value.loading = true;
  categoryName.value = node.data.category_name;

  if (
    node.data.category_id == categoryIdSave.value ||
    node.data.category_id == null
  ) {
    loadCount(store.getters.user.user_id, node.data.code, null);
    loadTask(store.getters.user.user_id, null, node.data.code);
    return;
  } else {
    categoryIdSave.value = node.data.category_id;

    nodeSelected.value = node.data;
    datalists.value = [];
    options.value.TaskUnfinished = 0;
    options.value.TaskFinished = 0;
    options.value.TaskTempClose = 0;
    loadCount(store.getters.user.user_id, null, categoryIdSave.value);
    //Lọc theo Tuần
    // options.value.Start_Date = new Date(
    //   new Date().getFullYear(),
    //   new Date().getMonth(),
    //   new Date().getDate() + 1 - new Date().getDay()
    // );
    // options.value.End_Date = new Date(
    //   new Date().getFullYear(),
    //   new Date().getMonth(),
    //   new Date().getDate() + (7 - new Date().getDay())
    // );  taskDateFilter.value=[];
    // taskDateFilter.value.push(options.value.Start_Date);
    // taskDateFilter.value.push(options.value.End_Date);

    loadCountSave(store.getters.user.user_id, null, node.data.category_id);

    loadTask(store.getters.user.user_id, node.data.category_id, null);
  }
};

const nodeSelected = ref();
const bugcomment = ref();
const isCheckCmtBug = ref(false);

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
const isCheckTask = ref(false);
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
const filterTypeChecklist = ref([
  { name: "Tất cả", code: 0 },

  { name: "Người thực hiện", code: 1 },
  { name: "Người Test", code: 2 },
]);
const filterDateChecklist = ref([
  { name: "Ngày cập nhật", code: 0 },

  { name: "Ngày tạo", code: 1 },
  { name: "Ngày thực hiện", code: 2 },
]);

const isShowBug = ref(false);
const showBug = (value) => {
  projectID_Save.value = value.project_id;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_main_get",
        par: [{ par: "task_id", va: value.task_id }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      if (data.length > 0) {
        TaskSave.value = data[0];
        TaskSave.value.test_user_ids = TaskSave.value.test_user_ids.split(",");
        isShowBug.value = true;
      }
    })
    .catch((error) => {
      console.log(error);
    });
};
const TaskSave = ref();
const showTask = (value) => {
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_main_get",
        par: [{ par: "task_id", va: value.data.task_id }],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0][0];
      let timeSpan = new Date(data.estimated_date);
      let timeNow = new Date();

      if (!data.taskTime) data.taskTime = 0;
      let arrDateBw = getDates(
        new Date(moment(data.estimated_date).format("YYYY/MM/DD")),
        new Date()
      );
      let arrN = [];
      if (!Array.isArray(data.next_date) && data.next_date != null) {
        data.next_date = data.next_date.split(",").forEach((item1) => {
          arrN.push(new Date(item1));
        });
        data.next_date = arrN;
      }
      let check = 0;
      if (data.next_date) {
        data.next_date.forEach((item) => {
          let dateR = new Date(moment(item).format("YYYY/MM/DD")).toJSON();
          if (arrDateBw.includes(dateR)) {
            if (
              dateR ==
              new Date(moment(new Date()).format("YYYY/MM/DD")).toJSON()
            )
              check +=
                Math.floor(timeNow.getTime() / 60000) -
                Math.floor(
                  new Date(moment(item).format("YYYY/MM/DD")).getTime() / 60000
                );
            else check += 1440;
          }
        });
      }
      data.taskTime =
        Math.floor(timeNow.getTime() / 60000) -
        check -
        Math.floor(timeSpan.getTime() / 60000);

      if (!Array.isArray(data.url_file) && data.url_file != null)
        data.url_file = data.url_file.split(",");

      data.estimated_date = moment(data.estimated_date).format(
        "DD/MM/YYYY HH:mm:ss"
      );
      if (data.actual_date)
        data.actual_date = moment(data.actual_date).format(
          "DD/MM/YYYY HH:mm:ss"
        );
      if (!data.full_name) data.full_name = "";
      data.full_name = listUsers.value.filter(
        (x) => x.data.user_id == data.user_id
      )[0].data.full_name;
      data.test_user_ids = data.test_user_ids.split(",");
      TaskSave.value = data;

      isCheckTask.value = true;
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
const checkLoadCmt=ref(false);
const showDetails = (value) => {
  checkLoadCmt.value=false;
  listCommentBugSave.value = [];
  dataCommentCheckList.value = {
    bug_id: value.data.bug_id,
    des: null,
    user_id: store.getters.user.user_id,
    bugcomment_id: value.data.bugcomment_id,
  };
  bugId.value = value.data.bug_id;
  cmtBugId.value = value.data.bugcomment_id;
  if (!Array.isArray(value.data.url_file) && value.data.url_file != null)
    value.data.url_file = value.data.url_file.split(",");

  value.data.active = true;
  value.data.estimated_date = moment(value.data.estimated_date).format(
    "DD/MM/YYYY HH:mm:ss"
  );
  if (value.data.actual_date)
    value.data.actual_date = moment(value.data.actual_date).format(
      "DD/MM/YYYY HH:mm:ss"
    );
  bugcomment.value = value.data;

  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_commentbug_list",
        par: [
          { par: "bug_id", va: null },
          { par: "commentbug_id", va: value.data.bugcomment_id },
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

        listCommentBugSave.value.push(element);
      });
      checkLoadCmt.value=true;
      // renderComment(listCommentBugSave.value);

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
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_commentbug_count",
        par: [
          { par: "bug_id", va: value.data.bug_id },
          { par: "bugcomment_id", va: value.data.bugcomment_id },
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
  isCheckCmtBug.value = true;
};
const onHideCheckTask = () => {
  bugId.value = null;
  datalists.value
    .filter((x) => x.active == true)
    .forEach((element) => {
      element.active = false;
    });
};
///DANH SÁCH NGƯỜI DÙNG
const commentCount = ref(0);
const listUsers = ref([]);
const listUserShow = ref([]);
const UsersCount = ref();
const listDropdownUser = ref([]);
const listDropdownUserCheck = ref([]);
const onProductSelect = (item) => {
  op.value.hide();

  if (commentTask.value != "") {
    let CommentBug = {
      bug_id: bugId.value,
      des: commentTask.value,
      cmtBugId: cmtBugId.value,
    };
    bugId.value = null;
    commentTask.value = "";
    cmtBugId.value = null;
    let formData1 = new FormData();
    formData1.append("commentTask", JSON.stringify(CommentBug));
    commentTask.value = "";
    axios
      .post(baseURL + "/api/api_commentbug/Add_commentbug", formData1, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
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

  bugComment.value.status = item.code;
  let data = {
    IntID: bugComment.value.bugcomment_id,
    TextID: bugComment.value.bugcomment_id + "",
    IntTrangthai: item.code,
    BitTrangthai: false,
  };
  axios
    .put(baseURL + "/api/task_bugcomment/Update_StatusBugcomment", data, config)
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật lỗi thành công!");

        onTaskFilter(
          userFilter.value,
          options.value.statusTask,
          options.value.Start_Date,
          options.value.End_Date,
          options.value.typeCheckList,
          options.value.Date_Filter,
          true
        );
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

const listStatus = ref([
  {
    name: "Lỗi",
    code: 0,
    css: "p-button-raised p-button-danger ",
  },
  { name: "Đang xử lý", code: 1, css: "p-button-raised" },

  {
    name: "Chuyển sang Test",
    code: 2,
    css: "p-button-raised p-button-secondary",
  },
]);

const listStatusCheck = ref([
  {
    name: "Đã xử lý",
    code: 3,
    css: "p-button-raised p-button-success ",
  },
  { name: "Test chưa OK", code: 4, css: "p-button-raised p-button-warning" },

  { name: "Yêu cầu thêm", code: 5, css: "p-button-raised" },

  { name: "Làm lại", code: 6, css: "p-button-raised p-button-danger" },
]);
const typeTime = ref(2);
const op = ref();
const opbugcomment = ref();
const bugCommentSave = ref();
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
        isShowAddBugComment.value = false;
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

const removeFileBase = (element) => {
  // valImg.value = valImg.value.replace('<img src="' + element + '">', "");
  listImgPast.value = listImgPast.value.filter((x) => x != element);
};
const onPasteImg = (event) => {
  // valImg.value.ops.forEach((element) => {
  //   if (element.insert)
  //     if (element.insert.image) {
  //       listImgPast.value = [];
  //     }
  // });
  valImg.value.ops.forEach((element) => {
    if (element.insert)
      if (element.insert.image) {
        listImgPast.value.push(element.insert.image);
      }
  });
  paste_zone.value.setHTML("");
};
const paste_zone = ref();
const toggleStatus = (dl, event) => {
  bugComment.value = dl;
  bugId.value = dl.bug_id;
  cmtBugId.value = dl.bugcomment_id;

  if (
    dl.user_id == store.getters.user.user_id &&
    dl.status != 2 &&
    dl.status != 3
  ) {
    op.value.toggle(event);
  } else if (
    dl.created_by == store.getters.user.user_id &&
    (dl.status == 2 || dl.status == 4)
  ) {
    ops.value.toggle(event);
  }
};
const ops = ref();

///Hiển thị bug
const taskId = ref();

const isDetailsBug = ref(false);

const closeBugComment = () => {
  bugInComment.value = {
    des: "",
    group_name: null,
    url_file: "",
    status: 0,
  };

  isShowAddBugComment.value = false;
};
const checkImg=(src)=>{
  let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i;
 return  allowedExtensions.exec(src);
}
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
const deleImgBugcmt = (value) => {
  bugInComment.value.url_file = bugInComment.value.url_file.filter(
    (x) => x.data != value.data
  );
  if (bugInComment.value.url_file == "") bugInComment.value.url_file = null;
};
const removeFileBugComment = (item) => {
  arrFiles.value = arrFiles.value.filter((x) => x.data != item.data);
  filebugcmt = filebugcmt.filter((x) => x.name != item.data);
};
const commentTask = ref("");
const commentChild = ref("");

const panelEmoij1 = ref();
const panelEmoij2 = ref();
const panelEmoij3 = ref();
const panelEmoij4 = ref();
const handleEmojiClick = (event) => {
  if (commentTask.value) commentTask.value = commentTask.value + event.unicode;
  else commentTask.value = event.unicode;
};
const listGroupBugComment = ref([]);
const listDropdownGroupBug = ref([]);
const listCommentBugSave = ref();
const listCommentBugs = ref();
const listFileComment = ref([]);
const refreshListComment = () => {
  listCommentBugSave.value =[];
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_commentbug_list",
        par: [
          { par: "bug_id", va: null },
          { par: "commentbug_id", va: cmtBugId.value },
          { par: "user_id", va: store.getters.user.user_id },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      // listCommentBugSave.value =[];
      data.forEach((element) => {
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
      
      emitter.emit("emitData", {
        type: "renderComment",
        data:  null
      });
      
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
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "api_commentbug_count",
        par: [
          { par: "bug_id", va: bugId.value },
          { par: "bugcomment_id", va: cmtBugId.value },
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
};
const renderComment = (listComment) => {
  listComment.forEach((element) => {
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
  listCommentBugs.value = arrChils;
  console.log("Dư liệu comment", listCommentBugs.value);
};
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

const checkAddBugComment = ref(false);
const saveBugComment = () => {
  if (checkAddBugComment.value) {
    let formData = new FormData();
    for (var i = 0; i < filebugcmt.length; i++) {
      let file = filebugcmt[i];
      formData.append("url_file", file);
    }
    listFileComment.value = [];

    formData.append("bugcomment", JSON.stringify(bugInComment.value));

    filebugcmt = [];

    axios
      .post(baseURL + "/api/task_bugcomment/Add_bugcomment", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Thêm lỗi thành công!");

          loadTask(
            store.getters.user.user_id,
            nodeValue.value
              ? nodeValue.value.category_id
                ? nodeValue.value.category_id
                : null
              : null,
            nodeValue.value
              ? nodeValue.value.code
                ? nodeValue.value.code
                : null
              : null
          );
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
  }
};
const onNewVersion = () => {
  toast.info("Chức năng bạn chọn sẽ sớm có ở phiên bản mới!");
};
const onRefreshTask = () => {
  userFilter.value = store.getters.user.user_id;
  taskDateFilter.value = [];
  monthPickerFilter.value = null;
  weekPickerFilter.value = null;
  options.value.typeCheckList = 0;
  options.value.Date_Filter = 0;
  options.value.PageNo = 0;
  options.value.PageSize = 20;
  options.value.searchText = null;
  onTaskFilter(store.getters.user.user_id, null, null, null, 0, 0, true);
};
const handleEmojiClickChild = (event) => {
  if (commentChild.value)
    commentChild.value = commentChild.value + event.unicode;
  else commentChild.value = event.unicode;
};
const filterMonth = ref();
const toggleFilterMonth = (event) => {
  filterMonth.value.toggle(event);
};

const isEditWork = ref(false);
// Filter Task
const onTaskUserFilter = (value) => {
  if (value.active) {
    listUserShow.value.forEach((element) => {
      if (element.data == value.data) element.active = false;
    });
    listUsers.value.forEach((element) => {
      if (element.data == value.data) element.active = false;
    });
    userFilter.value = store.getters.user.user_id;
    onTaskFilter(
      userFilter.value,
      options.value.statusTask,
      options.value.Start_Date,
      options.value.End_Date,
      options.value.typeCheckList,
      options.value.Date_Filter,
      true
    );
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
    onTaskFilter(
      userFilter.value,
      options.value.statusTask,
      options.value.Start_Date,
      options.value.End_Date,
      options.value.typeCheckList,
      options.value.Date_Filter,
      true
    );
  }
};

const taskDateFilter = ref();
const monthPickerFilter = ref();

const weekPickerFilter = ref();
const todayClick = () => {
  taskDateFilter.value = [];
  taskDateFilter.value.push(new Date());
};
const delDayClick = () => {
  taskDateFilter.value = [];
  onTaskFilter(
    userFilter.value,
    options.value.statusTask,
    null,
    null,
    options.value.typeCheckList,
    options.value.Date_Filter,
    true
  );
};
const onDayClick = () => {
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  if (weekPickerFilter.value) weekPickerFilter.value = null;
  if (taskDateFilter.value == null) taskDateFilter.value = [];
  else {
    options.value.Start_Date = taskDateFilter.value[0];
    options.value.End_Date = taskDateFilter.value[1];
    onTaskFilter(
      userFilter.value,
      options.value.statusTask,
      options.value.Start_Date,
      options.value.End_Date,
      options.value.typeCheckList,
      options.value.Date_Filter,
      true
    );
  }
};
const totalRecords = ref(10);
const onTaskFilter = (
  user_id,
  status,
  start_date,
  end_date,
  type_checklist,
  date_filter,
  check
) => {
  options.value.statusTask = status;
  options.value.Start_Date = start_date;
  options.value.End_Date = end_date;
  options.value.typeCheckList = type_checklist;
  options.value.Date_Filter = date_filter;
  userFilter.value = user_id;
  datalists.value = [];
  options.value.PageNo = 0;
  if (check) reloadTask(userFilter.value, true);
  else {
    if (status == null) {
      reloadTask(userFilter.value, false);
      loadCountSave(userFilter.value, null, categoryIdSave.value);

      totalRecords.value = options.value.totalRecords;
    } else {
      if (status == 7) {
        reloadTask(userFilter.value, false);
        loadCountSave(userFilter.value, null, categoryIdSave.value);

        totalRecords.value = options.value.add_work;
      } else if (status == 3) {
        reloadTask(userFilter.value, false);
        loadCountSave(userFilter.value, null, categoryIdSave.value);
        totalRecords.value = options.value.finishedRecord;
      } else if (status == 6) {
        reloadTask(userFilter.value, false);

        loadCountSave(userFilter.value, null, categoryIdSave.value);
        totalRecords.value = options.value.tempClose;
      } else if (status == 2) {
        reloadTask(userFilter.value, false);

        loadCountSave(userFilter.value, null, categoryIdSave.value);
        totalRecords.value = options.value.waitedRecord;
      } else if (status == 0) {
        reloadTask(userFilter.value, false);
        loadCountSave(userFilter.value, null, categoryIdSave.value);
        totalRecords.value = options.value.err_work;
      } else {
        // err_work
        reloadTask(userFilter.value, false);
        loadCountSave(userFilter.value, null, categoryIdSave.value);
        totalRecords.value = options.value.unFinishRecord;
      }
    }
  }
};
const dataChart = ref();
const isShowChart = ref(false);
const dataReport = ref([]);
const dateReport = ref();
const userReport=ref();
const showChart = (type) => {
  typeChart.value = type;
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_chart",
        par: [
          { par: "user_id", va: userFilter.value },
          { par: "search", va: options.value.searchText },
          {
            par: "category_id",
            va: categoryIdSave.value ? categoryIdSave.value : null,
          },
          { par: "typefilter", va: options.value.typeCheckList },
          { par: "datefilter", va: options.value.Date_Filter },
          { par: "project_id", va: projectID_Save.value },
          { par: "status", va: options.value.statusTask },
          { par: "start_date", va: options.value.Start_Date },
          { par: "end_date", va: options.value.End_Date },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      dataChart.value = data[0];
      isShowChart.value = true;
    })
    .catch((error) => {
      console.log(error);
    });
};
const onHideSidebar = () => {
  isShowDialog.value = false;
};
const showLoadDingRp=ref(false);
const showReport = () => {
  showLoadDingRp.value=true;
  dataReport.value=[];
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_checklist_report",
        par: [
          { par: "sdate", va:null},
          { par: "edate", va: null },
          { par: "users", va: null },
          { par: "user_id", va: store.getters.user.user_id },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let data1 = JSON.parse(response.data.data)[1];
      let data2 = JSON.parse(response.data.data)[2];
     
      userReport.value=data1;
      dateReport.value = data[0];
      data2.forEach((element) => {
        if( element.start_date!=null)
        element.start_date = moment(new Date(element.start_date)).format(
          "MM/DD/YYYY"
        );
        if( element.actual_date!=null)
        element.actual_date = moment(new Date(element.actual_date)).format(
          "MM/DD/YYYY"
        );
        if( element.created_date!=null)
        element.created_date = moment(new Date(element.created_date)).format(
          "MM/DD/YYYY"
        );
        if( element.finished_date!=null)
        element.finished_date = moment(new Date(element.finished_date)).format(
          "MM/DD/YYYY"
        );
        if( element.end_date!=null)
        element.end_date = moment(new Date(element.end_date)).format(
          "MM/DD/YYYY"
        );
        if( element.switch_test_date!=null)
        element.switch_test_date = moment(new Date(element.switch_test_date)).format(
          "MM/DD/YYYY"
        );
      });
      dataReport.value = data2;
      isShowDialog.value = true;
      showLoadDingRp.value=false;


    })
    .catch((error) => {
      console.log(error);
    });
};
const isShowDialog = ref(false);
const typeChart = ref(0);
const isThumnFiles = ref(false);
const listThumFiles = ref();

const bugInComment = ref({
  bug_id: null,
  des: null,
  group_name: null,
  url_file: null,
  status: 0,
});
const isShowAddBugComment = ref(false);

const onCleanFilterMonth = () => {
  if (monthPickerFilter.value) monthPickerFilter.value = null;
  if (weekPickerFilter.value) weekPickerFilter.value = null;
  options.value.Start_Date = null;
  options.value.End_Date = null;
  onTaskFilter(
    userFilter.value,
    options.value.statusTask,
    options.value.Start_Date,
    options.value.End_Date,
    options.value.typeCheckList,
    options.value.Date_Filter,
    true
  );
};
const onFilterMonth = (check) => {
  if (check) {
    if (weekPickerFilter.value) {
      options.value.Start_Date = weekPickerFilter.value[0];
      options.value.End_Date = weekPickerFilter.value[1];
    } else {
      options.value.Start_Date = null;
      options.value.End_Date = null;
    }
  } else {
    if (monthPickerFilter.value) {
      weekPickerFilter.value = [];
      let day = new Date(
        monthPickerFilter.value.year,
        monthPickerFilter.value.month + 1,
        0
      ).getDate();
      options.value.Start_Date = new Date(
        monthPickerFilter.value.month +
          1 +
          "/01" +
          "/" +
          monthPickerFilter.value.year
      );
      options.value.End_Date = new Date(
        monthPickerFilter.value.month +
          1 +
          "/" +
          day +
          "/" +
          monthPickerFilter.value.year
      );
    } else {
      options.value.Start_Date = null;
      options.value.End_Date = null;
    }
  }
  options.value.PageNo = 0;
  onTaskFilter(
    userFilter.value,
    options.value.statusTask,
    options.value.Start_Date,
    options.value.End_Date,
    options.value.typeCheckList,
    options.value.Date_Filter,
    true
  );
};
//Bình luận công việc

onMounted(() => {  if (!checkURL(window.location.pathname, store.getters.listModule)) {
    //  router.back();
  }
  loadProject();
  return {
    datalists,
    options,
    onPage,
    loadCount,
    addTask,
    closeDialog,
    basedomainURL,

    saveData,
    isFirst,

    selectedTasks,
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
        <div class="d-lang-table" v-if="!showLoadDingRp">
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
            :paginator="totalRecords > options.PageSize || checkPage"
            :rows="options.PageSize"
            :totalRecords="totalRecords"
            :rowHover="true"
            :showGridlines="true"
            :pageLinks="5"
            currentPageReportTemplate=""
          >
            <template #header>
              <div>
                <div class="flex">
                  <div class="w-2">
                    <h3 class="m-0">
                      <i class="pi pi-ticket"></i> Check List
                      <span v-if="options.totalRecords > 0"
                        >({{ options.totalRecords }})</span
                      >
                    </h3>
                  </div>
                  <div class="w-8">
                    <div class="flex format-center w-full">
                      <Button
                        @click="
                          onTaskFilter(
                            userFilter,
                            null,
                            taskDateFilter ? taskDateFilter[0] : null,
                            taskDateFilter ? taskDateFilter[1] : null,
                            options.typeCheckList,
                            options.Date_Filter,
                            false
                          )
                        "
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
                        @click="
                          onTaskFilter(
                            userFilter,
                            2,
                            taskDateFilter ? taskDateFilter[0] : null,
                            taskDateFilter ? taskDateFilter[1] : null,
                            options.typeCheckList,
                            options.Date_Filter,
                            false
                          )
                        "
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
                        @click="
                          onTaskFilter(
                            userFilter,
                            3,
                            taskDateFilter ? taskDateFilter[0] : null,
                            taskDateFilter ? taskDateFilter[1] : null,
                            options.typeCheckList,
                            options.Date_Filter,
                            false
                          )
                        "
                        :label="
                          'Đã xử lý: ' +
                          (options.finishedRecord ? options.finishedRecord : 0)
                        "
                        class="p-button-success mx-2 text-0"
                        :style="
                          options.statusTask == 3 ? 'border:3px solid cyan' : ''
                        "
                      />
                      <Button
                        @click="
                          onTaskFilter(
                            userFilter,
                            1,
                            taskDateFilter ? taskDateFilter[0] : null,
                            taskDateFilter ? taskDateFilter[1] : null,
                            options.typeCheckList,
                            options.Date_Filter,
                            false
                          )
                        "
                        :label="
                          'Đang xử lý: ' +
                          (options.unFinishRecord ? options.unFinishRecord : 0)
                        "
                        class="mx-2 text-0"
                        :style="
                          options.statusTask == 1 ? 'border:3px solid cyan' : ''
                        "
                      />
                      <Button
                        @click="
                          onTaskFilter(
                            userFilter,
                            4,
                            taskDateFilter ? taskDateFilter[0] : null,
                            taskDateFilter ? taskDateFilter[1] : null,
                            options.typeCheckList,
                            options.Date_Filter,
                            false
                          )
                        "
                        :label="
                          'Test chưa OK: ' +
                          (options.tempClose ? options.tempClose : 0)
                        "
                        class="p-button-warning mx-2 text-0"
                        :style="
                          options.statusTask == 4 ? 'border:3px solid cyan' : ''
                        "
                      />
                      <Button
                        @click="
                          onTaskFilter(
                            userFilter,
                            0,
                            taskDateFilter ? taskDateFilter[0] : null,
                            taskDateFilter ? taskDateFilter[1] : null,
                            options.typeCheckList,
                            options.Date_Filter,
                            false
                          )
                        "
                        :label="
                          'Lỗi: ' + (options.err_work ? options.err_work : 0)
                        "
                        class="p-button-danger mx-2 text-0"
                        :style="
                          options.statusTask == 0 ? 'border:3px solid cyan' : ''
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
                        @keyup.enter="
                          onTaskFilter(
                            userFilter,
                            null,
                            options.Start_Date,
                            options.End_Date,
                            options.typeCheckList,
                            options.Date_Filter,
                            true
                          )
                        "
                      />
                    </span>
                    <!-- tHÊM VIF để lọc theo tuần
                    v-if="taskDateFilter" -->
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
                    <Dropdown
                      v-model="options.typeCheckList"
                      :options="filterTypeChecklist"
                      @change="
                        onTaskFilter(
                          userFilter,
                          options.statusTask,
                          options.Start_Date,
                          options.End_Date,
                          options.typeCheckList,
                          options.Date_Filter,
                          true
                        )
                      "
                      optionLabel="name"
                      optionValue="code"
                      class="mx-2 w-12rem"
                      spellcheck="true"
                    />
                    <Dropdown
                      v-model="options.Date_Filter"
                      :options="filterDateChecklist"
                      @change="
                        onTaskFilter(
                          userFilter,
                          options.statusTask,
                          options.Start_Date,
                          options.End_Date,
                          options.typeCheckList,
                          options.Date_Filter,
                          true
                        )
                      "
                      optionLabel="name"
                      optionValue="code"
                      class="mx-2 w-12rem"
                      spellcheck="true"
                    />
                    <SplitButton
                      @click="showChart(2)"
                      label="Biểu đồ"
                      class="p-button-outlined p-button-text"
                      icon="pi pi-chart-bar"
                      :model="chartMore"
                    ></SplitButton>

                    <Button
                      @click="showReport"
                      label="Báo cáo"
                      class="mx-2"
                      icon="pi pi-map"
                      
                    ></Button>
                  </template>

                  <template #end>
                    <!-- <DataViewLayoutOptions v-model="layout" class="mr-2" /> -->

                    <Button
                      class="
                        mr-2
                        p-button-sm p-button-outlined p-button-secondary
                      "
                      @click="onRefreshTask()"
                      icon="pi pi-refresh"
                    />
                    <Button
                      label="Tiện ích"
                      icon="pi pi-file-excel"
                      class="mr-2 p-button-outlined p-button-secondary"
                      aria-haspopup="true"
                      aria-controls="overlay_Export"
                      @click="onNewVersion"
                    />
                    <Menu id="overlay_Export" ref="projectButs" :popup="true" />
                  </template>
                </Toolbar>
              </div>
            </template>

            <template #list="slotProps">
              <div class="w-full" :class="'row ' + slotProps.data.active">
                <div
                  :style="
                    slotProps.data.status == 0
                      ? 'border-left:8px solid red'
                      : slotProps.data.status == 2
                      ? 'border-left:8px solid #9C27B0'
                      : slotProps.data.status == 3
                      ? 'border-left:8px solid #689f38'
                      : slotProps.data.status == 4
                      ? 'border-left:8px solid #FBC02D'
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
                    <div class="col-12 field flex p-0 m-0">
                      <div class="col-9 p-0 cursor-pointer">
                        <div class="col-12 p-0 flex">
                          <div class="col-1 p-0">
                            <AvatarGroup>
                              <Avatar
                                :image="
                                  slotProps.data.task_user_avatar
                                    ? basedomainURL +
                                      slotProps.data.task_user_avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                style="
                                  border: 3px solid #2196f3;
                                  width: 2.5rem;
                                  height: 2.5rem;
                                "
                                shape="circle"
                                v-tooltip.bottom="'Người thực hiện'"
                                class="cursor-pointer"
                              />
                              <Avatar
                                :image="
                                  slotProps.data.check_user_avatar
                                    ? basedomainURL +
                                      slotProps.data.check_user_avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                style="
                                  border: 3px solid #ccc;
                                  width: 2.5rem;
                                  height: 2.5rem;
                                "
                                shape="circle"
                                v-tooltip.bottom="'Người tạo lỗi'"
                                class="cursor-pointer"
                              />
                              <Avatar
                                v-if="slotProps.data.fix_user_avatar"
                                :image="
                                  slotProps.data.fix_user_avatar
                                    ? basedomainURL +
                                      slotProps.data.fix_user_avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                style="
                                  border: 3px solid #9c27b0;
                                  width: 2.5rem;
                                  height: 2.5rem;
                                "
                                shape="circle"
                                v-tooltip.bottom="'Người sửa lỗi'"
                                class="cursor-pointer"
                              />
                            </AvatarGroup>
                          </div>
                          <div
                            v-tooltip.left="'Hiển thị chi tiết Check List'"
                            class="col-10 p-0 font-bold text-xl py-1"
                            style="
                              justify-content: center;
                              align-items: center;
                              vertical-align: middle;
                              text-align: left;
                            "
                            @click="showDetails(slotProps)"
                          >
                            <span class="mx-2">
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
                                ><span>Gấp</span></Tag
                              >
                              <Tag
                                v-if="slotProps.data.is_important == 3"
                                severity="danger"
                                >Rất gấp</Tag
                              ></span
                            >
                            <span
                              :class="
                                slotProps.data.status == 3
                                  ? 'line-through text-green-600'
                                  : ''
                              "
                              >{{ slotProps.data.des }}</span
                            >
                            <span v-if="slotProps.data.overtime">
                              <font-awesome-icon class="ml-3" style="
    -moz-transform: scaleX(-1);
    color:red;
    -o-transform: scaleX(-1);
    -webkit-transform: scaleX(-1);
    transform: scaleX(-1);
    filter: FlipH;
    -ms-filter: 'FlipH'"  icon="fa-solid fa-clock" />
                            </span>
                          </div>
                        </div>
                        <div
                          @click="showDetails(slotProps)"
                          class="col-12 p-0 flex"
                        >
                          <div class="col-1 p-0 font-bold text-xl flex"></div>
                          <div class="col-11 p-0 flex py-1">
                            <i
                              class="pi pi-angle-double-right pt-1 pr-1"
                              style="font-size: 1rem"
                            ></i>
                            Nhóm lỗi: {{ slotProps.data.group_name }}
                          </div>
                        </div>
                        <div class="col-12 p-0 flex py-1">
                          <div class="col-1 p-0 font-bold text-xl flex"></div>
                          <div
                            v-tooltip.left="'Hiển thị danh sách lỗi'"
                            class="
                              col-11
                              p-0
                              font-medium
                              cursor-pointer
                              text-lg
                            "
                            @click="showBug(slotProps.data)"
                          >
                            <div>
                              <i
                                class="pi pi-fast-forward pt-1"
                                style="font-size: 1rem"
                              ></i>
                              {{ slotProps.data.bug_name }}
                            </div>
                          </div>
                        </div>
                        <div class="col-12 p-0 flex">
                          <div class="col-1 p-0"></div>
                          <div
                            v-tooltip.left="'Hiển thị chi tiết công việc'"
                            class="col-11 p-0 font-italic cursor-pointer"
                            @click="showTask(slotProps)"
                          >
                            <i class="pi pi-tag"></i>
                            {{ slotProps.data.task_name }}
                          </div>
                        </div>
                        <div class="col-12 p-0 flex">
                          <div class="col-1 p-0"></div>
                          <div
                            class="pt-2 cursor-pointer col-3 flex p-0"
                            @click="showDetails(slotProps)"
                          >
                            <div v-if="slotProps.data.created_date">
                              <i
                                class="pi pi-calendar text-color-secondary"
                              ></i>
                              {{
                                moment(
                                  new Date(slotProps.data.created_date)
                                ).format("DD/MM/YYYY")
                              }}
                            </div>
                          </div>

                          <div
                            class="px-8 pt-2 cursor-pointer col-7"
                            @click="showDetails(slotProps)"
                          >
                            <div v-if="slotProps.data.task_user_avatar">
                              <i class="pi pi-user text-color-secondary"></i>
                              by: {{ slotProps.data.task_user_name }}
                            </div>
                          </div>
                        </div>
                      </div>
                      <div class="col-3 flex p-0">
                        <div class="col-5 p-0 format-center">
                          <Button
                            class="w-full p-0"
                            style="height: 2.5rem"
                            :label="
                              slotProps.data.status == 0
                                ? 'Lỗi'
                                : slotProps.data.status == 1
                                ? 'Đang xử lý'
                                : slotProps.data.status == 2
                                ? 'Chuyển Test'
                                : slotProps.data.status == 3
                                ? 'Đã xử lý'
                                : slotProps.data.status == 4
                                ? 'Test chưa OK'
                                : slotProps.data.status == 5
                                ? 'Yêu cầu thêm'
                                : slotProps.data.status == 6
                                ? 'Làm lại'
                                : 'Trạng thái'
                            "
                            @click="toggleStatus(slotProps.data, $event)"
                            aria:haspopup="true"
                            aria-controls="overlay_panelS"
                            :class="
                              slotProps.data.status == 5
                                ? 'p-button-raised  p-button-secondary'
                                : slotProps.data.status == 1
                                ? 'p-button-raised'
                                : slotProps.data.status == 2
                                ? 'p-button-raised p-button-help'
                                : slotProps.data.status == 3
                                ? 'p-button-raised p-button-success'
                                : slotProps.data.status == 0
                                ? 'p-button-raised p-button-danger'
                                : slotProps.data.status == 4
                                ? 'p-button-raised p-button-warning'
                                : slotProps.data.status == 6
                                ? 'p-button-raised p-button-danger'
                                : 'p-button-raised'
                            "
                          />

                          <OverlayPanel
                            ref="op"
                            appendTo="body"
                            :showCloseIcon="false"
                            id="overlay_panelS"
                            style="width: 250px"
                            :breakpoints="{ '960px': '20vw' }"
                          >
                            <div>
                              <div
                                class="border-1 border-solid border-round mb-2"
                              >
                                <QuillEditor
                                  class="w-full"
                                  ref="comment_zone"
                                  placeholder="Viết bình luận..."
                                  contentType="html"
                                  :content="comment"
                                  v-model:content="comment"
                                  theme="bubble"
                                />
                              </div>
                              <div v-for="item in listStatus" :key="item.code">
                                <Button
                                  :label="item.name"
                                  :class="item.css"
                                  class="w-full mb-1"
                                  @click="onProductSelect(item)"
                                />
                              </div>
                            </div>
                          </OverlayPanel>
                          <OverlayPanel
                            ref="ops"
                            appendTo="body"
                            :showCloseIcon="false"
                            id="overlay_panelSs"
                            style="width: 250px"
                            :breakpoints="{ '960px': '20vw' }"
                          >
                            <div>
                              <div
                                v-for="item in listStatusCheck"
                                :key="item.code"
                              >
                                <Button
                                  :label="item.name"
                                  :class="item.css"
                                  class="w-full mb-1"
                                  @click="onProductSelect(item)"
                                />
                              </div>
                            </div>
                          </OverlayPanel>
                        </div>
                        <div class="col-5 p-0 format-center">
                          <div class="format-center">
                            <div v-if="slotProps.data.url_file">
                              <div v-if="slotProps.data.url_file.length > 1">
                                <div
                                  class="flex"
                                  v-if="slotProps.data.url_file"
                                  @click="
                                    showThumnFiles(slotProps.data.url_file)
                                  "
                                >
                                  <img
                                    class="cursor-pointer px-2"
                                    v-if="slotProps.data.url_file[0].checkimg"
                                    :src="slotProps.data.url_file[0].src"
                                    :alt="slotProps.data.url_file[0].data"
                                    style="
                                      object-fit: contain;
                                      border: 3px solid #ccc;
                                      width: 67px;
                                      height: 67px;
                                    "
                                  />
                                  <div v-else>
                                    <img
                                      v-if="
                                        slotProps.data.url_file.length > 0 &&
                                        slotProps.data.url_file[0].data
                                      "
                                      class="cursor-pointer"
                                      :src="
                                        basedomainURL +
                                        '/Portals/Image/file/' +
                                        slotProps.data.url_file[0].data.substring(
                                          slotProps.data.url_file[0].data.indexOf(
                                            '.'
                                          ) + 1
                                        ) +
                                        '.png'
                                      "
                                      style="
                                        border: 3px solid #ccc;
                                        width: 67px;
                                        height: 67px;
                                        object-fit: contain;
                                      "
                                      :alt="slotProps.data.url_file[0].data"
                                    />
                                    <div>
                                      {{ slotProps.data.url_file[0].data }}
                                    </div>
                                  </div>
                                </div>
                              </div>
                              <div v-else-if="slotProps.data.url_file">
                                <div v-if="slotProps.data.url_file[0].checkimg">
                                  <Image
                                    :src="
                                      basedomainURL +
                                      '/Portals/CommentWork' +
                                      slotProps.data.url_file[0].data
                                    "
                                    :alt="slotProps.data.url_file[0].data"
                                    width="64"
                                    height="64"
                                    style="
                                      object-fit: contain;
                                      border: 3px solid #cccc;
                                    "
                                    @error="
                                      $event.target.src =
                                        basedomainURL +
                                        '/Portals/Image/noimg.jpg'
                                    "
                                    preview
                                  />
                                </div>
                                <div v-else>
                                  <a
                                    v-if="
                                      slotProps.data.url_file[0].src != null &&
                                      slotProps.data.url_file[0].src != ''
                                    "
                                    :href="slotProps.data.url_file[0].src"
                                    download
                                    class="w-full no-underline"
                                  >
                                    <img
                                      :src="
                                        basedomainURL +
                                        '/Portals/Image/file/' +
                                        slotProps.data.url_file[0].data.substring(
                                          slotProps.data.url_file[0].data.indexOf(
                                            '.'
                                          ) + 1
                                        ) +
                                        '.png'
                                      "
                                      style="
                                        border: 3px solid #ccc;
                                        width: 64px;
                                        height: 64px;
                                        object-fit: contain;
                                      "
                                      :alt="slotProps.data.url_file[0].data"
                                    />
                                  </a>
                                </div>
                              </div>
                            </div>
                            <div class="w-10 format-center" v-else></div>
                          </div>
                        </div>
                        <div class="col-2 p-0 text-center format-center">
                          <div
                            v-if="
                              slotProps.data.created_by ==
                              store.getters.user.user_id
                            "
                          >
                            <Button
                              icon="pi pi-ellipsis-h"
                              class="
                                p-button-outlined p-button-secondary
                                ml-2
                                border-none
                              "
                              @click="toggleMores($event, slotProps.data)"
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
                <img
                  :src="basedomainURL + '/Portals/Image/noimg.jpg'"
                  height="144"
                />
                <h3 class="m-1">Không có dữ liệu</h3>
              </div>
            </template>
          </DataView>
          <div></div>
        </div>
        <div class="w-full h-full surface-200"   v-else>
          <div
                    style="
                      translate: transform(-50%, -50%);
                      top: 50%;
                      right: 50%;
                      object-fit: cover;
                    "
                 
                    class="absolute "
                  >
                    <ProgressSpinner
                      style="width: 50px; height: 50px"
                      strokeWidth="8"
                      fill="var(--surface-ground)"
                      animationDuration=".5s"
                    />
                  </div>
        </div>
        
      </SplitterPanel>
    </Splitter>
  </div>
 
  <detailstask :isCheckTask="isCheckTask" :task="TaskSave" />
  <taskbug
    :project_id="projectID_Save"
    :isShowBug="isShowBug"
    :task="TaskSave"
  />
  <Dialog
    v-model:visible="isShowAddBugComment"
    :style="{ width: '40vw' }"
    header="Cập nhật lỗi"
   @hide="
      loadTask(
        store.getters.user.user_id,
        nodeValue
          ? nodeValue.category_id
            ? nodeValue.category_id
            : null
          : null,
        nodeValue ? (nodeValue.code ? nodeValue.code : null) : null
      )
    "
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
            <label class="col-4 text-center p-0 pt-2">Trạng thái</label>
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
                <div v-else-if="element1.data">
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
    v-model:visible="isThumnFiles"
    :style="{ width: '40vw' }"
    header="Danh sách File"
  >
    
    <div class="flex format-center">
      <div
        v-for="(element1, index1) in listThumFiles"
        :key="index1"
        class="mr-2"
      >
        <Image
          v-if="element1.checkimg"
          :src="element1.src"
          :alt="element1.data"
          width="100"
          height="100"
          style="object-fit: contain; border: 1px solid #ccc"
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
                element1.data.substring(element1.data.indexOf('.') + 1) +
                '.png'
              "
              style="
                width: 100px;
                height: 100px;
                border: 3px solid #ccc;
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
  </Dialog>
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
          @closed="onFilterMonth(true)"
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
      <div class="flex field col-12 p-0">
        <Dropdown  class="w-full"    v-model="bugInComment.status"
              :options="listStatusBugComment" optionLabel="name" placeholder="Lọc theo công việc" />
        </div>
        <div class="flex field col-12 p-0">
          <Dropdown  class="w-full"   v-model="bugInComment.status"
              :options="listStatusBugComment"  optionLabel="name" placeholder="Lọc theo lỗi" />
        </div>
    </div>
  </OverlayPanel>
  <Sidebar
    v-model:visible="isCheckCmtBug"
    :baseZIndex="100"
    position="right"
    class="p-sidebar-lg"
 @hide="onHideCheckTask()"
    :showCloseIcon="false"
  >
    <div class="pt-5">
      <h2 class="p-0 m-0">{{ bugcomment.des }}</h2>
    </div>
    <div class="grid formgrid m-2">
      <div class="field col-12 md:col-12">- Lỗi: {{ bugcomment.bug_name }}</div>
      <div class="field col-12 md:col-12">
        - Công việc: {{ bugcomment.task_name }}
      </div>
      <div class="field col-12 md:col-12">
        <Button
          :label="
            bugcomment.status == 0
              ? 'Lỗi'
              : bugcomment.status == 1
              ? 'Đang xử lý'
              : bugcomment.status == 2
              ? 'Chuyển Test'
              : bugcomment.status == 3
              ? 'Đã xử lý'
              : bugcomment.status == 4
              ? 'Test chưa OK'
              : bugcomment.status == 5
              ? 'Yêu cầu thêm'
              : bugcomment.status == 6
              ? 'Làm lại'
              : 'Trạng thái'
          "
          :class="
            bugcomment.status == 5
              ? 'p-button-raised  p-button-secondary'
              : bugcomment.status == 1
              ? 'p-button-raised'
              : bugcomment.status == 2
              ? 'p-button-raised p-button-help'
              : bugcomment.status == 3
              ? 'p-button-raised p-button-success'
              : bugcomment.status == 0
              ? 'p-button-raised p-button-danger'
              : bugcomment.status == 4
              ? 'p-button-raised p-button-warning'
              : bugcomment.status == 6
              ? 'p-button-raised p-button-danger'
              : 'p-button-raised'
          "
        />
      </div>
      <div class="field col-12 md:col-12">
        <span class="font-bold"
          >Người thực hiện: {{ bugcomment.task_user_name }}</span
        >
      </div>
      <div class="field col-12 md:col-12">
        <span v-if="bugcomment.modified_date" class="font-bold"
          >Ngày thực hiện:
          {{
            moment(bugcomment.modified_date).format("HH:mm DD/MM/YYYY")
          }}</span
        >
      </div>
      <div class="field col-12 md:col-12">
        <span class="font-bold"
          >Người tạo: {{ bugcomment.check_user_name }}</span
        >
      </div>
      <div class="field col-12 md:col-12" v-if="bugcomment.created_date">
        <span class="font-bold"
          >Ngày tạo:
          {{ moment(bugcomment.created_date).format("HH:mm DD/MM/YYYY") }}</span
        >
      </div>
      <div class="field col-12 md:col-12">
        <hr />
      </div>
      <div
        class="field col-12 md:col-12 format-center"
        v-if="bugcomment.url_file!=null"
      >
        <div class="col-12 p-0 cursor-pointer"  v-if="bugcomment.url_file!=null">
          <div>
            <Toolbar class="w-full py-3">
              <template #start>
                <div
                  style="display: grid; grid-template-columns: repeat(5, 1fr)"
                >
           
                  <div
                    class="flex m-1"
                    style="border: 3px solid #ccc"
                    v-for="(item, index) in bugcomment.url_file"
                    :key="index"
                  >
                    <Image
                      class="pt-2"
                      v-if="item.checkimg"
                      :src="item.src"
                      :alt="item.data"
                      width="100"
                      height="100"
                      style="object-fit: contain; border: 1px solid #ccc"
                      preview
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                    />
                    <div v-else>
                      <a
                        v-if="item.data"
                        :href="item.src"
                        class="w-full no-underline"
                      >
                        <img
                          :src="
                            basedomainURL + '/Portals/Image/file/' + item.data
                              ? item.data.substring(item.data.indexOf('.') + 1)
                              : 'filess' + '.png'
                          "
                          style="
                            border: 3px solid #ccc;
                            width: 32px;
                            height: 32px;
                            object-fit: contain;
                          "
                          :alt="item.data"
                        />
                      </a>
                    </div>
                  </div>
                </div>
              </template>
            </Toolbar>
          </div>
        </div>
      </div>

      <div
        class="col-12 field p-0 flex"
        v-if="listCommentBugSave != null && checkLoadCmt "
      >
        <commentCheckList
          :options="optionsCommentTask"
          :refreshData="refreshListComment"
          :objectData="dataCommentCheckList"
          :comment_count="commentCount"
          :dataComments="listCommentBugSave"
          :Controller="'api_commentbug'"
        />
      </div>
    </div>
  </Sidebar>
  <taskchart
    :isCheckTask="isShowChart"
    :dataChart="dataChart"
    :typeChart="typeChart"
    :typeDateFilter="options.Date_Filter"
    :categoryid="categoryIdSave"
    :projectid="projectID_Save"
  />
  <div v-if="dataReport.length>0&&listDropdownUser.length>0">
    <Sidebar
    v-model:visible="isShowDialog"
    style="width: 100% !important"
    :showCloseIcon="false"
    class="p-sidebar-report"
    position="right"
  >  <div>
        <Toolbar class="custoolbar mt-2">
          <template #start>
            <Button icon="pi pi-th-large" class="p-button-rounded" />
            <h2 class="px-2 m-0">Báo cáo công việc</h2>
          </template>
          <template #end>
            <Button
              @click="onHideSidebar"
              icon="pi pi-times"
              class="p-button-rounded"
            />
          </template>
        </Toolbar>
      </div>
   <taskreport
      :isShowDialog="isShowDialog"
      :dataReport="dataReport"
      :dateReport="dateReport"
      :listDropdownUser="listDropdownUser"
      :userReport="userReport"
    />
    </Sidebar>
  </div>
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
::v-deep(.p-sidebar) {
  .p-sidebar .p-sidebar-header {
    padding: 0 !important;
  }
}
</style>
