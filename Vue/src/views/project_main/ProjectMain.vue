<script setup>
import { ref, inject, onMounted, watch, onBeforeUnmount } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import moment from "moment";
import { concat } from "lodash";
import { encr } from "../../util/function.js";
import treeuser from "../../components/user/treeuser.vue";
import DetailProject from "../../components/project_main/DetailedProject.vue"
import DataTable from "primevue/datatable";
const cryoptojs = inject("cryptojs");
const basedomainURL = fileURL;

const toast = useToast();
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const opition = ref({
  IsNext: true,
  sort: "created_date",
  ob: "DESC",
  PageNo: 0,
  PageSize: 20,
  search: "",
  Filteruser_id: null,
  user_id: store.getters.user_id,
  type_view: 2,
  filter_type: 0,
  sdate: null,
  edate: null,
  loctitle: "Lọc",
  filter_date: null,
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

const listDropdownUser = ref([]);
const arrNhom = ref([]);
const listProjectMains = ref();
const treelistProjectMains = ref();
const sttProjectMain = ref();
const selectedProjectMainDel = ref([]);
const selectedProjectMain = ref();
const selectedKey = ref();
const selectedNodes = ref([]);
const listProjectGroups = ref();
const first = ref(0);
let files = {};
let fileAll = [];
const ProjectMainMember = ref();
const isDisplayAvt = ref(false);
const listDropdownStatus = ref([
  {
    value: 0,
    text: "Chưa bắt đầu",
    bg_color: "#bbbbbb",
    text_color: "#FFFFFF",
  },
  {
    value: 1,
    text: "Đang thực hiện",
    bg_color: "#2196f3",
    text_color: "#FFFFFF",
  },
  {
    value: 2,
    text: "Đã hoàn thành",
    bg_color: "#04D215",
    text_color: "#FFFFFF",
  },
  { value: 3, text: "Tạm dừng", bg_color: "#d87777", text_color: "#FFFFFF" },
  { value: 4, text: "Đóng", bg_color: "red", text_color: "#FFFFFF" },
]);
const headerAddProjectMain = ref();
const isAdd = ref(false);
const issaveProjectMain = ref(false);
const displayProjectMain = ref(false);
const submitted = ref(false);
const listDropdownParent = ref();
const selectcapcha = ref({});
const rules = {
  project_code: {
    required,
    $errors: [
      {
        $property: "project_code",
        $validator: "required",
        $message: "Mã dự án không được để trống!",
      },
    ],
  },
  project_name: {
    required,
    $errors: [
      {
        $property: "project_name",
        $validator: "required",
        $message: "Tên dự án không được để trống!",
      },
    ],
  },
};
const ProjectMain = ref({
  project_code: "",
  project_name: "",
  description: "",
  keywords: "",
  group_code: null,
  status: 0,
  is_order: null,
});
const v$ = useVuelidate(rules, ProjectMain);
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const listData = ref();

const onPage = (event) => {
  if (event.rows != opition.value.PageSize) {
    opition.value.PageSize = event.rows;
  }

  if (event.page == 0) {
    //Trang đầu
    opition.value.id = null;
    opition.value.IsNext = true;
  } else if (event.page > opition.value.PageNo + 1) {
    //Trang cuối
    opition.value.id = -1;
    opition.value.IsNext = false;
  } else if (event.page > opition.value.PageNo) {
    //Trang sau

    opition.value.id =
      listProjectMains.value[listProjectMains.value.length - 1].project_id;
    opition.value.IsNext = true;
  } else if (event.page < opition.value.PageNo) {
    //Trang trước
    opition.value.id = listProjectMains.value[0].project_id;
    opition.value.IsNext = false;
  }
  opition.value.PageNo = event.page;
  loadData(true);
};

const listThanhVien = ref([]);

const listUser = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_users_list_task_origin",
            par: [
              { par: "search", va: opition.value.SearchTextUser },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "role_id", va: null },
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "department_id", va: null },
              { par: "position_id", va: null },

              { par: "isadmin", va: null },
              { par: "status", va: null },
              { par: "start_date", va: null },
              { par: "end_date", va: null },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      listDropdownUser.value = data.map((x) => ({
        name: x.full_name,
        code: x.user_id,
        avatar: x.avatar,
        ten: x.last_name,
      }));
      if (listDropdownUser.value.length > 10) {
        listThanhVien.value = listDropdownUser.value.slice(0, 10);
      } else {
        listThanhVien.value = [...listDropdownUser.value];
      }
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const groupBy = (list, props) => {
  return list.reduce((a, b) => {
    (a[b[props]] = a[b[props]] || []).push(b);
    return a;
  }, {});
};

const loadData = (rf) => {
  if (rf) {
    opition.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "project_main_list",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "pageno", va: opition.value.PageNo },
              { par: "pagesize", va: opition.value.PageSize },
              { par: "search", va: opition.value.search },
              { par: "sort", va: opition.value.sort },
              { par: "ob", va: opition.value.ob },
              { par: "loc", va: opition.value.filter_type },
              { par: "sdate", va: opition.value.sdate },
              { par: "edate", va: opition.value.edate },
              { par: "filter_date", va: opition.value.filter_date },
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
      if (data.length > 0) {
        listData.value = concat(data[0], data[2]);
        // listData.value = data[0];
        listData.value.forEach((element, i) => {
          element.status_name = listDropdownStatus.value.filter(
            (x) => x.value == element.status,
          )[0].text;
          element.status_bg_color = listDropdownStatus.value.filter(
            (x) => x.value == element.status,
          )[0].bg_color;
          element.status_text_color = listDropdownStatus.value.filter(
            (x) => x.value == element.status,
          )[0].text_color;
          element.Thanhviens = element.Thanhviens
            ? JSON.parse(element.Thanhviens)
            : [];
          element.ThanhvienShows = [];
          if (element.Thanhviens.length > 3) {
            element.ThanhvienShows = element.Thanhviens.slice(0, 3);
          } else {
            element.ThanhvienShows = [...element.Thanhviens];
          }
          element.countThanhviens = element.Thanhviens.length;
          element.countThanhvienShows = element.ThanhvienShows.length;
          element.STT = opition.value.PageNo * opition.value.PageSize + i + 1;
          element.progress = element.count_task > 0 ? Math.floor((element.count_taskHT / element.count_task) * 100) : 0;
        });
        if (opition.value.type_view == 1) {
          listProjectMains.value = listData.value;
        } else if (opition.value.type_view == 2) {
          let obj = renderTreeDV(
            listData.value,
            "project_id",
            "project_name",
            "dự án",
          );
          listProjectMains.value = obj.arrChils;
          treelistProjectMains.value = obj.arrtreeChils;
        } else if (opition.value.type_view == 3) {
          var listCV = groupBy(listData.value, "status");
          var arrNew = [];
          for (let k in listCV) {
            var CVGroup = [];
            listCV[k].forEach(function (r) {
              CVGroup.push(r);
            });
            arrNew.push({
              status: k,
              group_view_name: listDropdownStatus.value.filter(
                (x) => x.value == k,
              )[0].text,
              group_view_bg_color: listDropdownStatus.value.filter(
                (x) => x.value == k,
              )[0].bg_color,
              CVGroup: CVGroup,
              countProject: CVGroup.length,
            });
          }
          listProjectMains.value = arrNew;
          // stt.value = data[1][0].total + 1;
        } else if (opition.value.type_view == 4 || opition.value.type_view == 5) {
          listProjectMains.value = listData.value;
          let date1 = new Date(
            opition.value.sdate ? opition.value.sdate : new Date(),
          );
          let date2 = new Date(
            opition.value.edate ? opition.value.edate : new Date(),
          );
          // var firstDay = new Date(date.getFullYear(), date.getMonth(), 1);
          // var lastDay = new Date(date.getFullYear(), date.getMonth() + 1, 0);
          var firstDay = new Date(date1.getFullYear(), date1.getMonth(), 1);
          var lastDay = new Date(date2.getFullYear(), date2.getMonth() + 1, 0);
          getDates(firstDay, lastDay);
        }
        opition.value.totalRecords = data[1][0].totalrecords;
      } else {
        listProjectMains.value = [];
      }
      if (rf) {
        opition.value.loading = false;
        swal.close();
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

const GrandsDate = ref();
const Grands = ref();
const WeekDay = ref([
  { value: "Monday", text: "T2", bg: "" },
  { value: "Tuesday", text: "T3", bg: "" },
  { value: "Wednesday", text: "T4", bg: "" },
  { value: "Thursday", text: "T5", bg: "" },
  { value: "Friday", text: "T6", bg: "" },
  { value: "Saturday", text: "T7", bg: "aliceblue" },
  { value: "Sunday", text: "CN", bg: "antiquewhite" },
]);

function getDaysInMonth(year, month) {
  return new Date(year, month, 0).getDate();
}

const getDates = (startDate, endDate) => {
  var dateArray = [];
  var currentDate = moment(startDate);
  var stopDate = moment(endDate);
  while (currentDate <= stopDate && currentDate) {
    var d = moment.utc(currentDate).toDate();
    var date = new Date();
    var currentYear = date.getFullYear();
    var currentMonth = date.getMonth() + 1;
    dateArray.push({
      DayN: moment(currentDate).format("DD"),
      DW: d.getDay(),
      Day: parseInt(moment(currentDate).format("DD")),
      DayName: WeekDay.value.filter(
        (x) => x.value == d.toLocaleString("default", { weekday: "long" }),
      )[0].text,
      bg: WeekDay.value.filter(
        (x) => x.value == d.toLocaleString("default", { weekday: "long" }),
      )[0].bg,
      color:
        parseInt(moment(currentDate).format("DD/MM/YYYY")) ==
          parseInt(moment(new Date()).format("DD/MM/YYYY"))
          ? "#ff0000"
          : "",
      totalDayCurrent: getDaysInMonth(currentYear, currentMonth),
      currentDate: currentDate,
      Month: d.getMonth(),
      Year: d.getFullYear(),
    });
    currentDate = moment(currentDate).add(1, "days");
  }
  listProjectMains.value.forEach(function (d) {
    var dates = [];
    var bd = new Date(d.start_date);
    bd.setHours(0, 0, 0, 0);

    dateArray.forEach(function (t, i) {
      var to = { DW: t.DW, Day: t.Day, totalDay: 0 };
      if (
        new Date(t.currentDate) >= bd &&
        new Date(t.currentDate) <=
        new Date(d.finish_date != null ? d.finish_date : new Date())
      ) {
        to.IsCheck = true;
        to.Name = d.progress + '% (' + d.count_taskHT + '/' + d.count_task + ')';
        to.progress = d.progress;
        to.totalDay = to.totalDay + 1;
        if (i > 0 && dates[i - 1].IsCheck) {
          to.IsHide = true;
        } else {
          to.IsHide = false;
        }
      } else {
        to.IsHide = false;
      }
      to.color = t.color;
      to.bg = t.bg;
      dates.push(to);
    });
    d.totalDay = dates.filter((x) => x.IsCheck == true).length;
    d.dateArray = dates.filter((x) => x.IsHide == false);
  });

  if (opition.value.type_view == 5) {
    var listData = [];
    listProjectMains.value.forEach(function (cv) {
      cv.Thanhviens.forEach(function (u) {
        if (
          listData.filter(
            (x) => x.user_id == u.user_id && x.project_id == cv.project_id,
          ).length == 0
        ) {
          listData.push({
            user_id: u.user_id,
            project_id: cv.project_id,
            user_name: u.fullName,
            dateArray: cv.dateArray,
            totalDay: cv.totalDay,
            time_bg: cv.status_bg_color,
            status_text_color: cv.status_text_color,
            avatar: u.avatar,
            last_name: u.ten,
            tenToChuc: u.tenToChuc,
            tenChucVu: u.tenChucVu,
            is_type: parseInt(u.is_type),
          });
        }
      });
    });
    let listCV = groupBy(listData, "user_id");
    var arrNew = [];
    for (let k in listCV) {
      listCV[k].forEach(function (r, i) {
        r.IsHienThi = i == 0 ? true : false;
        r.count_cv = listCV[k].length;
        r.count_istype_0 = listCV[k].filter((x) => x.is_type == 0).length;
        r.count_istype_1 = listCV[k].filter(
          (x) => x.is_type == 1 || x.is_type == 2,
        ).length;
        r.count_istype_3 = listCV[k].filter((x) => x.is_type == 3).length;
        arrNew.push(r);
      });
    }
    listProjectMains.value = arrNew;
  }

  GrandsDate.value = dateArray;

  var years = [];
  for (
    var i = new Date(startDate).getFullYear();
    i <= new Date(endDate).getFullYear();
    i++
  ) {
    for (var j = 0; j < 12; j++) {
      var Month = { Month: j + 1, Year: i, Dates: [], Time: new Date(startDate) };
      Month.Dates = dateArray.filter((x) => x.Month === j && x.Year === i);
      if (Month.Dates.length > 0) years.push(Month);
    }
  }
  Grands.value = years;
}

const addProjectMain = (str) => {
  submitted.value = false;
  arrNhom.value = [];
  ProjectMain.value = {
    project_code: "",
    project_name: "",
    description: "",
    keywords: "",
    parent_id: null,
    group_code: null,
    start_date: new Date(),
    end_date: null,
    status: 0,
    is_order: listProjectMains.value.length + 1,
    managers: [],
    participants: [],
  };
  if (store.state.user.is_super) {
    ProjectMain.value.organization_id = 0;
  } else {
    ProjectMain.value.organization_id = store.state.user.organization_id;
  }
  selectcapcha.value[-1] = true;
  isAdd.value = true;
  issaveProjectMain.value = false;
  headerAddProjectMain.value = str;
  displayProjectMain.value = true;
};
const addTreeProjectMain = (p) => {
  submitted.value = false;
  arrNhom.value = [];
  selectcapcha.value = [];
  ProjectMain.value = {
    project_code: "",
    project_name: "",
    description: "",
    keywords: "",
    parent_id: p.project_id,
    group_code: null,
    status: 0,
    start_date: new Date(),
    is_order: listProjectMains.value.length + 1,
    managers: [],
    participants: [],
  };
  if (store.state.user.is_super) {
    ProjectMain.value.organization_id = 0;
  } else {
    ProjectMain.value.organization_id = store.state.user.organization_id;
  }
  selectcapcha.value[p.project_id || -1] = true;
  isAdd.value = true;
  issaveProjectMain.value = false;
  headerAddProjectMain.value = "Thêm mới dự án";
  displayProjectMain.value = true;
};
const closeDialogProjectMain = () => {
  ProjectMain.value = {
    project_code: "",
    project_name: "",
    description: "",
    keywords: "",
    group_code: null,
    status: 0,
    is_order: sttProjectMain.valu1e,
  };
  displayProjectMain.value = false;
};
const editProjectMain = (dataProjectMain) => {
  selectcapcha.value = [];
  fileAll = [];
  arrNhom.value = [];
  if (dataProjectMain.parent_id) {
    selectcapcha.value[dataProjectMain.parent_id] = true;
  } else {
    selectcapcha.value[-1] = true;
  }
  submitted.value = false;
  isAdd.value = false;
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "project_main_get_edit",
            par: [{ par: "project_id", va: dataProjectMain.project_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      ProjectMain.value = data[0][0];
      if (
        ProjectMain.value.keywords != null &&
        ProjectMain.value.keywords.length > 1
      ) {
        if (!Array.isArray(ProjectMain.value.keywords)) {
          ProjectMain.value.keywords = ProjectMain.value.keywords.split(",");
        }
      }
      if (ProjectMain.value.group_code) {
        arrNhom.value.push(ProjectMain.value.group_code);
      }
      ProjectMain.value.start_date = ProjectMain.value.start_date
        ? new Date(ProjectMain.value.start_date)
        : null;
      ProjectMain.value.end_date = ProjectMain.value.end_date
        ? new Date(ProjectMain.value.end_date)
        : null;
      ProjectMain.value.files = data[1];
      ProjectMain.value.managers = [];
      ProjectMain.value.participants = [];
      if (data[2].length > 0) {
        data[2].forEach((t) => {
          if (t.is_type == 0) {
            ProjectMain.value.managers.push(t.user_id);
          } else if (t.is_type == 1) {
            ProjectMain.value.participants.push(t.user_id);
          }
        });
      }
      headerAddProjectMain.value = "Sửa dự án";
      issaveProjectMain.value = false;
      displayProjectMain.value = true;
      if (store.state.user.is_super) {
        ProjectMain.value.organization_id = 0;
      } else {
        ProjectMain.value.organization_id = store.state.user.organization_id;
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "ProjectMain.vue",
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
const DelProjectMain = (dataProjectMain) => {
  if (dataProjectMain.count_child == 0 && dataProjectMain.count_task == 0) {
    swal
      .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá dự án này không!",
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
          if (!dataProjectMain) {
            selectedNodes.value.forEach(function (pg) {
              listId.push(pg.project_id);
            });
          }
          axios
            .delete(baseURL + "/api/ProjectMain/Delete_ProjectMain", {
              headers: { Authorization: `Bearer ${store.getters.token}` },
              data:
                dataProjectMain != null ? [dataProjectMain.project_id] : listId,
            })
            .then((response) => {
              swal.close();
              if (response.data.err != "1") {
                swal.close();
                toast.success("Xoá dự án thành công!");
                //   checkDelList.value = false;
                loadData(true);
              } else {
                swal.fire({
                  title: "Thông báo!",
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
                  title: "Thông báo!",
                  text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                  icon: "error",
                  confirmButtonText: "OK",
                });
              }
            });
        }
      });
  } else {
    swal.fire({
      title: "Thông báo!",
      text: "Không thể xóa do tồn tại công việc hoặc dự án con thuộc dự án này!",
      icon: "info",
      confirmButtonText: "OK",
    });
  }
};

const saveProjectMain = (isFormValid) => {
  ProjectMainMember.value = [];
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (Object.keys(selectcapcha.value)[0] == "-1" || !selectcapcha.value) {
    selectcapcha.value = [];
  } else {
    ProjectMain.value.parent_id = Object.keys(selectcapcha.value)[0];
  }
  if (ProjectMain.value.keywords != null) {
    ProjectMain.value.keywords = ProjectMain.value.keywords.toString();
  }
  if (arrNhom.value != null) {
    if (arrNhom.value.length > 0) {
      ProjectMain.value.group_code = arrNhom.value[0];
    } else {
      ProjectMain.value.group_code = null;
    }
  } else {
    ProjectMain.value.group_code = null;
  }
  let formData = new FormData();
  if (files["LogoDonvi"]) {
    formData.append("LogoDonvi", JSON.stringify(files["LogoDonvi"].name));
    fileAll.push(files["LogoDonvi"]);
  } else {
    formData.append("LogoDonvi", JSON.stringify());
  }
  for (var i = 0; i < fileAll.length; i++) {
    let file = fileAll[i];
    formData.append("url", file);
  }
  if (ProjectMain.value.managers.length > 0) {
    ProjectMain.value.managers.forEach((t) => {
      let member = {
        project_id: null,
        task_id: null,
        user_id: t,
        is_type: 0, // 0: người quản lý, 1: người tham gia
        status: true,
      };
      member.user_id = t;
      ProjectMainMember.value.push(member);
    });
  }
  if (ProjectMain.value.participants.length > 0) {
    ProjectMain.value.participants.forEach((t) => {
      let member1 = {
        project_id: null,
        task_id: null,
        user_id: t,
        is_type: 1, // 0: người quản lý, 1: người tham gia
        status: true,
      };
      member1.user_id = t;
      ProjectMainMember.value.push(member1);
    });
  }

  // formData.append("url", files["LogoDonvi"]);
  formData.append("ProjectMain", JSON.stringify(ProjectMain.value));
  formData.append("projectmainmember", JSON.stringify(ProjectMainMember.value));
  if (!issaveProjectMain.value) {
    axios
      .post(
        baseURL +
        "/api/ProjectMain/" +
        (isAdd.value == true ? "Add_ProjectMain" : "Update_ProjectMain"),
        formData,
        config,
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Cập nhật dự án thành công!");
          isDisplayAvt.value = false;
          selectcapcha.value = [];
          arrNhom.value = [];
          listtreeProjectMain();
          loadData(true);
          closeDialogProjectMain();
        } else {
          let ms = response.data.ms;
          let title_ms = "";
          if (ms.includes("project_name") == true) {
            title_ms = "Tên dự án không quá 250 ký tự!";
          } else if (ms.includes("project_code") == true) {
            title_ms = "Mã dự án không quá 50 ký tự!";
          } else {
            title_ms = ms;
          }

          swal.fire({
            title: "Thông báo!",
            html: title_ms,
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
const emitter = inject("emitter");

const RenderData = (response) => {
  opition.value.allRecord = null;
  let list1 = [];
  let list2 = [];
  let list3 = [];
  let d1 = JSON.parse(response.data.data)[0];
  d1.forEach((element, i) => {
    let c = {
      key: element.project_id,
      data: {
        place_id: element.project_id,
        parent_id: element.parent_id,
        project_name: element.project_name,
        status: element.status,
        is_order: element.is_order,
        STT: null,
        created_by: element.created_by,
      },
      children: null,
    };
    if (opition.value.PageNo > 0) {
      c.data.STT = opition.value.PageNo * opition.value.PageSize + i + 1;
    } else {
      c.data.STT = i + 1;
    }
    if (d1[i].children) {
      list2 = JSON.parse(d1[i].children);
      if (list2 != null) {
        list2.forEach((element, i) => {
          //đổi dạng stt=> true/false
          if (element.data.status == 1) {
            element.data.status = true;
          } else {
            element.data.status = false;
          }
          //đổi is_order
          element.data.STT = c.data.STT + "." + (i + 1);
          let temp = list2[i].data.STT;
          if (list2[i].children != null) {
            list3 = list2[i].children;
            list3.forEach((element, i) => {
              element.data.STT = temp + "." + (i + 1);
              if (element.data.status == 1) {
                element.data.status = true;
              } else {
                element.data.status = false;
              }
            });
            list2[i].children = list3;
          }
        });
      }
      c.children = list2;
    }
    list1.push(c);
  });
  listProjectMains.value = list1;
  if (JSON.parse(response.data.data)[1]) {
    let data2 = JSON.parse(response.data.data)[1];
    opition.value.allRecord = data2[0].allRecord;
  } else {
    opition.value.allRecord = datalists.value.length;
  }
};
const ChangeCheckProjectMain = (model) => {
  let data = listData.value.filter((x) => x.project_id == model.project_id);
  if (model.is_check) {
    selectedProjectMainDel.value.push(model.project_id);
  }
};
const renderTreeDV = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  data
    .filter((x) => x.parent_id == null)
    .forEach((m, i) => {
      m.arr_del = [];
      m.arr_del.push(m.project_id);
      m.IsOrder = i + 1;
      if (opition.value.PageNo > 0) {
        m.STT = opition.value.PageNo * opition.value.PageSize + i + 1;
      } else {
        m.STT = i + 1;
      }
      let om = { key: m[id], data: m };
      const rechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em, index) => {
            m.arr_del.push(em.project_id);
            em.STT = mm.data.STT + "." + (index + 1);
            // em.label_order = mm.data.label_order + "." + em.is_order;
            em.is_check = false;
            let om1 = { key: em[id], data: em };
            rechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      rechildren(om, m[id]);
      arrChils.push(om);
      //
      om = { key: m[id], data: m[id], label: m[name] };
      const retreechildren = (mm, pid) => {
        let dts = data.filter((x) => x.parent_id == pid);
        if (dts.length > 0) {
          if (!mm.children) mm.children = [];
          dts.forEach((em) => {
            let om1 = { key: em[id], data: em[id], label: em[name] };
            retreechildren(om1, em[id]);
            mm.children.push(om1);
          });
        }
      };
      retreechildren(om, m[id]);
      arrtreeChils.push(om);
    });
  arrtreeChils.unshift({
    key: -1,
    data: -1,
    label: "-----Chọn " + title + "----",
  });
  return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const handleFileUpload = (event, ia) => {
  if (ia == "LogoDonvi") isDisplayAvt.value = true;
  else if (ia == "LogoNen") isDisplayNen.value = true;
  files[ia] = event.target.files[0];
  var output = document.getElementById(ia);
  output.src = URL.createObjectURL(event.target.files[0]);
  output.onload = function () {
    URL.revokeObjectURL(output.src); // free memory
  };
};
const chonanh = (id) => {
  document.getElementById(id).click();
};
const listtreeProjectMain = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "project_main_list_pb",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((m, i) => {
        m.STT = opition.value.PageNo * opition.value.PageSize + i + 1;
      });
      // console.log("heloo", data);
      let obj = renderTreeDV(data, "project_id", "project_name", "cấp cha");
      listDropdownParent.value = obj.arrtreeChils;
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
// const delLogo = () => {

// };
const delLogo = (datafile) => {
  files["LogoDonvi"] = [];
  isDisplayAvt.value = false;
  var output = document.getElementById("LogoDonvi");
  output.src = basedomainURL + "/Portals/Image/noimg.jpg";
  ProjectMain.value.logo = null;
  // if (isAdd.value == true) {

  // } else {
  //   swal
  //     .fire({
  //       title: "Thông báo",
  //       text: "Bạn có muốn xoá file này không!",
  //       icon: "warning",
  //       showCancelButton: true,
  //       confirmButtonColor: "#3085d6",
  //       cancelButtonColor: "#d33",
  //       confirmButtonText: "Có",
  //       cancelButtonText: "Không",
  //     })
  //     .then((result) => {
  //       if (result.isConfirmed) {
  //         swal.fire({
  //           width: 110,
  //           didOpen: () => {
  //             swal.showLoading();
  //           },
  //         });
  //         axios
  //           .delete(baseURL + "/api/ProjectMain/Delete_file", {
  //             headers: { Authorization: `Bearer ${store.getters.token}` },
  //             data: datafile,
  //           })
  //           .then((response) => {
  //             swal.close();
  //             if (response.data.err != "1") {
  //               swal.close();
  //               toast.success("Xoá file thành công!");
  //               ProjectMain.value.logo = null;
  //             } else {
  //               swal.fire({
  //                 title: "Thông báo!",
  //                 text: response.data.ms,
  //                 icon: "error",
  //                 confirmButtonText: "OK",
  //               });
  //             }
  //           })
  //           .catch((error) => {
  //             swal.close();
  //             if (error.status === 401) {
  //               swal.fire({
  //                 title: "Thông báo!",
  //                 text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
  //                 icon: "error",
  //                 confirmButtonText: "OK",
  //               });
  //             }
  //           });
  //       }
  //     });
  // }
};

const closeSildeBar = () => {
  showDetailProject.value = false;
}

const onRefersh = () => {
  opition.value = {
    IsNext: true,
    sort: "created_date",
    ob: "DESC",
    PageNo: 0,
    PageSize: 20,
    search: "",
    Filteruser_id: null,
    user_id: store.getters.user_id,
    type_view: 2,
    filter_type: 0,
    sdate: null,
    edate: null,
    loctitle: "Lọc",
    filter_date: null,
  };
  first.value = 0;
  itemSortButs.value.forEach((i) => {
    if (i.sort == opition.value.sort && i.ob == opition.value.ob) {
      i.active = true;
    } else {
      i.active = false;
    }
  });
  filterTime1.value = null;
  filterTime2.value = null;
  itemFilterButs.value.forEach((i) => {
    i.active = false;
  });
  loadData(true);
};
const removeVietnameseTones = (str) => {
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
  // loại bỏ tất cả khoảng trắng
  str = str.replace(/\s+/g, "");
  str = str.trim();
  // Remove punctuations
  // Bỏ dấu câu, kí tự đặc biệt
  str = str.replace(
    /!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'|\"|\&|\#|\[|\]|~|\$|_|`|-|{|}|\||\\/g,
    " ",
  );
  ProjectMain.value.project_code = str;
};
const onSort = (event) => {
  if (event.sortField == "STT") {
    opition.value.sort = "is_order";
    opition.value.ob = event.sortOrder == 1 ? "ASC" : "DESC";
  } else {
    opition.value.sort = "project_name";
    opition.value.ob = event.sortOrder == 1 ? "ASC" : "DESC";
  }
  opition.value.PageNo = 0;
  loadData(true);
};

const loadCountProjectGroup = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_ca_projectgroup_get_list",
            par: [{ par: "user_id", va: store.getters.user.user_id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        listProjectGroups.value = data.map((x) => ({
          name: x.group_name,
          code: x.group_id,
        }));
      }
    })
    .catch((error) => {
      addLog({
        title: "Lỗi Console loadCountProjectGroup",
        controller: "LogsView.vue",
        log_content: error.message,
        loai: 2,
      });
    });
};
const changeMaNhom = (event) => {
  arrNhom.value = [];
  if (event.value.length == 1) {
    arrNhom.value.push(event.value[0]);
  } else if (event.value.length > 1) {
    arrNhom.value.push(event.value[1]);
  } else {
    arrNhom.value = null;
  }
};

const displayDialogUser = ref(false);
const selectedUser = ref([]);
const headerDialogUser = ref();
const is_one = ref(false);
const is_type = ref();

const OpenDialogTreeUser = (one, type) => {
  selectedUser.value = [];
  if (type == 1) {
    ProjectMain.value.managers.forEach((t) => {
      let select = { user_id: t };
      selectedUser.value.push(select);
    });
    headerDialogUser.value = "Chọn người quản lý";
  } else if (type == 2) {
    ProjectMain.value.participants.forEach((t) => {
      let select = { user_id: t };
      selectedUser.value.push(select);
    });
    headerDialogUser.value = "Chọn người tham gia";
  }
  displayDialogUser.value = true;
  is_one.value = one;
  is_type.value = type;
};

const closeDialog = () => {
  displayDialogUser.value = false;
};
const choiceTreeUser = () => {
  switch (is_type.value) {
    case 1:
      if (selectedUser.value.length > 0) {
        ProjectMain.value.managers = [];
        selectedUser.value.forEach((t) => {
          ProjectMain.value.managers.push(t.user_id);
        });
      }
      break;
    case 2:
      if (selectedUser.value.length > 0) {
        ProjectMain.value.participants = [];
        selectedUser.value.forEach((t) => {
          ProjectMain.value.participants.push(t.user_id);
        });
      }
      break;
    default:
      break;
  }
  displayDialogUser.value = false;
};

const onUploadFile = (event) => {
  fileAll = [];
  event.files.forEach((element) => {
    element.is_type = 2;
    fileAll.push(element);
  });
};
const removeFile = (event) => {
  fileAll = fileAll.filter((a) => a != event.file);
};

const componentKey = ref(0);
const PositionSideBar = ref("right");

const forceRerender = () => {
  componentKey.value += 1;
};
const MaxMin = (m) => {
  PositionSideBar.value = m;
  emitter.emit("psb", m);
};

emitter.on("psb", (obj) => {
  PositionSideBar.value = obj;
  console.log(obj);
});

const showDetailProject = ref(false);
const selectedProjectMainID = ref();
const selectedKeys = ref();
const onNodeSelect = (id) => {
  forceRerender();
  showDetailProject.value = true;
  selectedProjectMainID.value = id.data.project_id;
};
const onRowSelect = (id) => {
  showDetailProject.value = true;
  selectedProjectMainID.value = id.project_id;
}
const onRowUnselect = (id) => { };
const menuListTypeButs = ref();
const toggleListType = (event) => {
  menuListTypeButs.value.toggle(event);
};
const itemSortButs = ref([
  {
    label: "Số thứ tự thấp đến cao",
    sort: "is_order",
    ob: "ASC",
    active: false,
    command: (event) => {
      ChangeSortProject("is_order", "ASC");
    },
  },
  {
    label: "Số thứ tự cao đến thấp",
    sort: "is_order",
    ob: "DESC",
    active: false,
    command: (event) => {
      ChangeSortProject("is_order", "DESC");
    },
  },
  {
    label: "Ngày tạo mới đến cũ",
    sort: "created_date",
    ob: "DESC",
    active: true,
    command: (event) => {
      ChangeSortProject("created_date", "DESC");
    },
  },
  {
    label: "Ngày tạo cũ đến mới",
    sort: "created_date",
    ob: "ASC",
    active: false,
    command: (event) => {
      ChangeSortProject("created_date", "ASC");
    },
  },
  {
    label: "Tên dự án A-Z",
    sort: "project_name",
    ob: "ASC",
    active: false,
    command: (event) => {
      ChangeSortProject("project_name", "ASC");
    },
  },
  {
    label: "Tên dự án Z-A",
    sort: "project_name",
    ob: "DESC",
    active: false,
    command: (event) => {
      ChangeSortProject("project_name", "DESC");
    },
  },
]);
const ChangeSortProject = (sort, ob) => {
  opition.value.sort = sort;
  opition.value.ob = ob;
  itemSortButs.value.forEach((i) => {
    if (i.sort == sort && i.ob == ob) {
      i.active = true;
    } else {
      i.active = false;
    }
  });
  menuSortButs.value.toggle();
  loadData(true);
}
const menuSortButs = ref();
const toggleSort = (event) => {
  menuSortButs.value.toggle(event);
};
const menuFilterButs = ref();
const itemFilterButs = ref([
  {
    label: "Trong tuần",
    icon: "pi pi-calendar",
    active: false,
    istype: 1,
    hasChildren: false,
  },
  {
    label: "Trong tháng",
    icon: "pi pi-calendar",
    active: false,
    istype: 2,
    hasChildren: false,
  },
  {
    label: "Trong năm",
    icon: "pi pi-calendar",
    active: false,
    istype: 3,
    hasChildren: false,
  },
  {
    label: "Theo thời gian",
    icon: "pi pi-calendar-times",
    active: false,
    istype: 4,
    hasChildren: true,
    groups: [
      {
        label: "Ngày bắt đầu",
        icon: "pi pi-calendar",
        children_id: true,
        is_change: 1,
      },
      {
        label: "Ngày kết thúc",
        icon: "pi pi-calendar",
        children_id: true,
        is_change: 2,
      },
    ],
  },
]);
const toggleFilter = (event) => {
  menuFilterButs.value.toggle(event);
};
const filterTime1 = ref();
const filterTime2 = ref();
const ChangeFilter = (type, act) => {
  opition.value.filter_type = type;
  itemFilterButs.value.forEach((i) => {
    if (i.istype == type) {
      i.active = true;
    } else {
      i.active = false;
    }
  });
  var date = new Date();
  switch (type) {
    case -1: //tất cả
      opition.value.sdate = null;
      filterTime1.value = null;
      opition.value.edate = null;
      filterTime2.value = null;
      opition.value.loctitle = "Lọc";
      break;
    case 1: //Trong tuần
      opition.value.sdate = moment().startOf("isoWeek").toDate();
      opition.value.edate = moment().endOf("isoWeek").toDate();
      opition.value.loctitle = "Trong tuần";
      break;
    case 5: //theo ngày nhận
      opition.value.sdate = null;
      opition.value.edate = null;
      debugger
      itemFilterButs.value
        .filter((x) => x.istype == 5)
        .forEach((t) => {
          t.groups
            .filter((y) => y.is_children == 1)
            .forEach((d) => {
              d.label =
                "Theo ngày nhận" +
                " (" +
                moment(t.filter_date).format("DD/MM/YYYY HH:mm") +
                ")";
              opition.value.filter_date = t.filter_date;
            });
        });

      itemFilterButs.value
        .filter((x) => x.istype == 6)
        .forEach((t) => {
          t.groups
            .filter((y) => y.is_children == 3)
            .forEach((d) => {
              d.label = "Ngày hoàn thành";
            });
        });
      opition.value.loctitle = "Theo ngày nhận";
      break;
    case 6: //theo ngày hoàn thành
      opition.value.sdate = null;
      opition.value.edate = null;
      itemFilterButs.value
        .filter((x) => x.istype == 6)
        .forEach((t) => {
          t.groups
            .filter((y) => y.is_children == 3)
            .forEach((d) => {
              d.label =
                "Ngày hoàn thành (" +
                moment(t.filter_date).format("DD/MM/YYYY HH:mm") +
                ")";
            });
          opition.value.filter_date = t.filter_date;
        });
      itemFilterButs.value
        .filter((x) => x.istype == 5)
        .forEach((t) => {
          t.groups
            .filter((y) => y.is_children == 1)
            .forEach((d) => {
              d.label = "Theo ngày nhận";
            });
        });
      opition.value.loctitle = "Theo ngày hoàn thành";
      break;
    case 2: //Trong tháng
      opition.value.sdate = new Date(date.getFullYear(), date.getMonth(), 1);
      opition.value.edate = new Date(
        date.getFullYear(),
        date.getMonth() + 1,
        0,
      );
      opition.value.loctitle = "Trong tháng";
      break;
    case 3: //Trong năm
      opition.value.sdate = new Date(date.getFullYear(), 1, 1);
      opition.value.edate = new Date(date.getFullYear(), 12, 31);
      opition.value.loctitle = "Trong năm";
      break;
    default:
      if (opition.value.sdate) {
        if (opition.value.edate) {
          opition.value.loctitle =
            "Từ " +
            moment(opition.value.sdate).format("DD/MM/YYYY") +
            " - " +
            moment(opition.value.edate).format("DD/MM/YYYY");
        } else {
          opition.value.loctitle =
            "Từ ngày " + moment(opition.value.sdate).format("DD/MM/YYYY");
        }
      } else {
        if (opition.value.edate) {
          opition.value.loctitle =
            "Đến ngày " + moment(opition.value.edate).format("DD/MM/YYYY");
        }
      }
      break;
  }
  filterTime1.value = opition.value.sdate
    ? moment(new Date(opition.value.sdate)).format("DD/MM/YYYY")
    : null;
  filterTime2.value = opition.value.edate
    ? moment(new Date(opition.value.edate)).format("DD/MM/YYYY")
    : null;
  if (type == 1 || type == 2 || type == 3) {
    menuFilterButs.value.toggle();
    loadData(true, opition.value.type_view);
  } else {
    if (act == true) {
      menuFilterButs.value.toggle();
      loadData(true, opition.value.type_view);
    }
  }
};
const ChangeTimeFilter = (type, value) => {
  opition.value.filter_type = 4;
  itemFilterButs.value.forEach((i) => {
    if (i.istype == 4) {
      i.active = true;
    } else {
      i.active = false;
    }
  });
  if (type == 1) {
    filterTime1.value = moment(new Date(value)).format("DD/MM/YYYY HH:mm");
    opition.value.sdate = value;
  } else {
    filterTime2.value = moment(new Date(value)).format("DD/MM/YYYY HH:mm");
    opition.value.edate = value;
  }
};
const Del_ChangeFilter = () => {
  opition.value.filter_duan = null;
  opition.value.filter_taskgroup = null;
  opition.value.filter_type = 0;
  filterTime1.value = null;
  filterTime2.value = null;
  opition.value.sdate = null;
  opition.value.edate = null;
  opition.value.filter_date = null;
  opition.value.loctitle = "Lọc";
  // itemFilterButs.value.forEach((i) => {
  //   if(i.istype == 5 || i.istype == 6){
  //     i.filter_date = new Date();
  //   }
  //   i.active = false;
  // });
  itemFilterButs.value = [
    {
      label: "Trong tuần",
      icon: "pi pi-calendar",
      active: false,
      istype: 1,
      hasChildren: false,
    },
    {
      label: "Trong tháng",
      icon: "pi pi-calendar",
      active: false,
      istype: 2,
      hasChildren: false,
    },
    {
      label: "Trong năm",
      icon: "pi pi-calendar",
      active: false,
      istype: 3,
      hasChildren: false,
    },
    {
      label: "Theo thời gian",
      icon: "pi pi-calendar-times",
      active: false,
      istype: 4,
      hasChildren: true,
      groups: [
        {
          label: "Ngày bắt đầu",
          icon: "pi pi-calendar",
          children_id: true,
          is_change: 1,
        },
        {
          label: "Ngày kết thúc",
          icon: "pi pi-calendar",
          children_id: true,
          is_change: 2,
        },
      ],
    },
  ];
  menuFilterButs.value.toggle();
  loadData(true);
};
const itemListTypeButs = ref([
  {
    label: "LIST",
    active: false,
    icon: "pi pi-list",
    type: 1,
    command: (event) => {
      ChangeView(1);
    },
  },
  {
    label: "TREE",
    active: true,
    icon: "pi pi-list",
    type: 2,
    command: (event) => {
      ChangeView(2);
    },
  },
  {
    label: "GRID",
    active: false,
    icon: "pi pi-table",
    type: 3,
    command: (event) => {
      ChangeView(3);
    },
  },
  {
    label: "GANTT",
    active: false,
    icon: "pi pi-calendar-plus",
    type: 4,
    command: (event) => {
      ChangeView(4);
    },
  },
  {
    label: "USER",
    active: false,
    icon: "pi pi-user-plus",
    type: 5,
    command: (event) => {
      ChangeView(5);
    },
  },
]);

const ChangeView = (data) => {
  if (data.type == 3) {
    opition.value.PageSize = 10000;
  } else {
    opition.value.PageSize = 20;
  }
  opition.value.type_view = data.type;
  loadData(true);
  itemListTypeButs.value.forEach((t) => {
    if (data.type != t.type) {
      t.active = false;
    } else {
      t.active = true;
    }
  });
  menuListTypeButs.value.toggle();
};
onMounted(() => {
  listUser();
  loadData(true);
  loadCountProjectGroup();
  listtreeProjectMain();

  return {};
});
</script>
<template>
  <!-- @nodeSelect="onNodeSelect" @nodeUnselect="onNodeUnselect" selectionMode="checkbox" -->
  <div v-if="store.getters.islogin" class="main-layout true flex-grow-1 p-2">
    <div class="flex justify-content-center align-items-center">
      <Toolbar class="w-full custoolbar">
        <template #start>
          <span class="p-input-icon-left">
            <i class="pi pi-search" />
            <InputText type="text" spellcheck="false" v-model="opition.search" placeholder="Tìm kiếm theo tên dự án"
              v-on:keyup.enter="loadData(true)" />
          </span>
        </template>

        <template #end>
          <Button label="Thêm dự án" icon="pi pi-plus" class="mr-2" @click="addProjectMain('Thêm mới dự án')" />
          <ul id="toolbar_right" style="padding: 0px; margin: 0px; display: flex">
            <li @click="toggleListType" aria-haspopup="true" :class="{ active: opition.type_view != 0 }"
              aria-controls="overlay_Export1">
              <a><i style="margin-right: 5px" class="pi pi-bars"></i>Kiểu hiển thị<i style="margin-left: 5px"
                  class="pi pi-angle-down"></i></a>
            </li>
            <li @click="toggleFilter" :class="{ active: opition.sort }" aria-haspopup="true"
              aria-controls="overlay_Export">
              <a><i class="pi pi-filter"></i> {{ opition.loctitle }}
                <i class="pi pi-angle-down"></i></a>
            </li>
            <li @click="toggleSort" :class="{ active: opition.sort }" aria-haspopup="true" aria-controls="overlay_Export">
              <a><i class="pi pi-sort"></i> Sắp xếp
                <i class="pi pi-angle-down"></i></a>
            </li>
          </ul>
          <Button class="mr-2 p-button-outlined p-button-secondary" icon="pi pi-refresh" @click="onRefersh" />
          <Button label="Xoá" icon="pi pi-trash" class="mr-2 p-button-danger" v-if="selectedNodes.length > 0"
            @click="DelProjectMain()" />
          <!-- <Button label="Export" icon="pi pi-file-excel" class="mr-2 p-button-outlined p-button-secondary"
                                        @click="toggleExport" aria-haspopup="true" aria-controls="overlay_Export" /> -->
          <Menu vị id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" />
          <Menu id="task_list_type" :model="itemListTypeButs" ref="menuListTypeButs" :popup="true">
            <template #item="{ item }">
              <div @click="ChangeView(item)">
                <a :class="{ active: item.active }"><i :class="item.icon"></i>{{ item.label }}</a>
              </div>
            </template>
          </Menu>
          <Menu id="task_sort" :model="itemSortButs" ref="menuSortButs" :popup="true">
            <template #item="{ item }">
              <a @click="ChangeSortProject(item.sort, item.ob)" :class="{ active: item.active }">{{ item.label }}</a>
            </template>
          </Menu>
          <OverlayPanel ref="menuFilterButs" id="task_filter" style="z-index: 10">
            <div style="
                  min-height: calc(100vh - 250px);
                  max-height: calc(100vh - 250px);
                  width: 100%;
                  overflow-x: scroll; ;
                ">
              <ul v-for="(item, index) in itemFilterButs" :key="index" style="padding: 0px; margin: 0px">
                <li v-if="item.istype == 5 || item.istype == 6" :class="{
                  children: item.hasChildren,
                  parent: !item.hasChildren,
                }" class="p-menuitem">
                  <ul style="padding: 0px; display: flex; flex-direction: row">
                    <li style="
                          list-style: none;
                          /* padding: 10px; */
                          /* display: flex; */
                          flex: 1;
                          align-items: center;
                        " v-for="(item1, index) in item.groups" :key="index">
                      <div v-if="item1.is_children == 1 || item1.is_children == 3">
                        <a @click="ChangeFilter(item.istype, false)" :class="{ active: item.active }">
                          <i style="padding-right: 5px" :class="item1.icon"></i>
                          {{ item1.label }}
                        </a>
                        <span style="margin-left: 10px">
                          <Calendar @date-select="ChangeFilter(item.istype, false)" inputId="icon"
                            v-model="item.filter_date" :showIcon="true" :manualInput="true" />
                        </span>
                      </div>
                      <div v-if="item1.is_children != 1 && item1.is_children != 3"
                        style="display: flex; align-items: center">
                        <a style="flex: 1" @click="ChangeFilter(item.istype, false)">{{ item1.label }}
                        </a>
                        <span style="margin-left: 10px; flex: auto">
                          <Dropdown @change="ChangeFilterAdvanced(item1.is_children)" v-if="item1.is_children == 2"
                            :filter="true" v-model="opition.filter_duan" panelClass="d-design-dropdown" selectionLimit="1"
                            :options="listDropdownProject" optionLabel="project_name" optionValue="project_id"
                            spellcheck="false" class="col-9 ip36 p-0" placeholder="Chọn">
                            <template #option="slotProps">
                              <div class="country-item flex">
                                <div class="pt-1">
                                  {{ slotProps.option.project_name }}
                                </div>
                              </div>
                            </template>
                          </Dropdown>
                          <Dropdown v-if="item1.is_children == 4" @change="ChangeFilterAdvanced(item1.is_children)"
                            :filter="true" v-model="opition.filter_taskgroup" panelClass="d-design-dropdown"
                            selectionLimit="1" :options="listDropdownTaskGroup" optionLabel="group_name"
                            optionValue="group_id" spellcheck="false" class="col-9 ip36 p-0" placeholder="Chọn">
                            <template #option="slotProps">
                              <div class="country-item flex">
                                <div class="pt-1">
                                  {{ slotProps.option.group_name }}
                                </div>
                              </div>
                            </template>
                          </Dropdown>
                        </span>
                      </div>
                    </li>
                  </ul>
                </li>
                <li v-if="item.istype == 4" :class="{
                  children: item.hasChildren,
                  parent: !item.hasChildren,
                }" class="p-menuitem">
                  <a :class="{ active: item.active }"><i style="padding-right: 5px" :class="item.icon"></i>{{ item.label
                  }}</a>
                  <ul style="padding: 0px; display: flex">
                    <li style="
                          list-style: none;
                          padding: 10px;
                          font-weight: bold;
                          display: flex;
                          flex-direction: column;
                        " v-for="(item1, index) in item.groups" :key="index">
                      <div style="padding-bottom: 10px">
                        <span>{{ item1.label }}</span>
                        <span style="
                              color: #2196f3;
                              font-weight: bold;
                              margin-left: 5px;
                              font-size: 14px;
                            " v-if="item1.is_change == 1">{{ filterTime1 }}
                          <i @click="removeTime(item1.is_change)" v-if="filterTime1" style="color: black"
                            class="pi pi-times-circle"></i></span>
                        <span style="
                              color: #2196f3;
                              font-weight: bold;
                              margin-left: 5px;
                              font-size: 14px;
                            " v-if="item1.is_change == 2">{{ filterTime2 }}
                          <i @click="removeTime(item1.is_change)" v-if="filterTime2" style="color: black"
                            class="pi pi-times-circle"></i></span>
                      </div>
                      <Calendar v-if="item1.is_change == 1" @date-select="
                        ChangeTimeFilter(item1.is_change, opition.sdate)
                      " v-model="opition.sdate" id="filterTime1" :inline="true"
                        :manualInput="true" />
                      <Calendar v-if="item1.is_change == 2" @date-select="
                        ChangeTimeFilter(item1.is_change, opition.edate)
                      " v-model="opition.edate" id="filterTime2" :inline="true" />
                    </li>
                  </ul>
                </li>
                <li v-if="
                  item.istype == 1 || item.istype == 2 || item.istype == 3
                " :class="{
  children: item.hasChildren,
  parent: !item.hasChildren,
}" class="p-menuitem" @click="ChangeFilter(item.istype, false)">
                  <a :class="{ active: item.active }"><i style="padding-right: 5px" :class="item.icon"></i>{{ item.label
                  }}</a>
                </li>
              </ul>
            </div>
            <div style="float: right; padding: 10px">
              <Button @click="ChangeFilter(opition.filter_type, true)" label="Thực hiện" />``
              <Button @click="Del_ChangeFilter" id="btn_huy" style="
                    background-color: #f2f4f6;
                    border: 1px solid #f2f4f6;
                    color: #333;
                    margin-left: 10px;
                  " label="Hủy lọc" />
            </div>
          </OverlayPanel>
        </template>
      </Toolbar>
    </div>
    <!-- dạng TREE -->
    <TreeTable v-if="opition.type_view == 2" id="project-main-list" :value="listProjectMains"
      v-model:selectionKeys="selectedKeys" v-model:first="first" :loading="opition.loading" @page="onPage($event)"
      @sort="onSort($event)" :paginator="true" :rows="opition.PageSize" :totalRecords="opition.totalRecords"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]" :filters="filters" :showGridlines="true" filterMode="strict"
      class="p-treetable-sm" :rowHover="true" responsiveLayout="scroll" :lazy="true" :scrollable="true"
      @nodeSelect="onNodeSelect" selectionMode="single" @nodeUnselect="onNodeUnselect" scrollHeight="flex">
      <Column field="STT" header="STT" class="align-items-center justify-content-center text-center font-bold"
        headerStyle="text-align:center;max-width:4rem;height:40px;" bodyStyle="text-align:center;max-width:4rem">
      </Column>
      <Column field="Logo" header="Logo" class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:80px;height:40px;" bodyStyle="text-align:center;max-width:80px">
        <template #body="md">
          <Avatar v-if="md.node.data.logo" :image="basedomainURL + md.node.data.logo" class="mr-2" size="large" />
        </template>
      </Column>
      <Column field="project_name" header="Tên dự án" :expander="true" :sortable="true"
        headerStyle="max-width:auto;height:40px;">
        <template #body="md">
          <div style="display: flex; align-items: center">
            <span style="margin-left: 5px">{{
              md.node.data.project_name
            }}</span>
          </div>
        </template>
      </Column>
      <Column field="project_code" header="Mã dự án" class="align-items-center justify-content-center text-center"
        headerStyle="max-width:100px;text-align:center;height:40px;" bodyStyle="max-width:100px;text-align:center;">
      </Column>
      <Column field="group_name" header="Nhóm dự án" class="align-items-center justify-content-center text-center"
        headerStyle="max-width:300px;text-align:center;height:40px;" bodyStyle="max-width:300px;text-align:center;">
      </Column>
      <Column field="status" header="Trạng thái" class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px;height:40px;" bodyStyle="text-align:center;max-width:120px">
        <template #body="md">
          <Chip :style="{
            background: md.node.data.status_bg_color,
            color: md.node.data.status_text_color,
          }" v-bind:label="md.node.data.status_name" />
        </template>
      </Column>
      <Column header="Chức năng" headerClass="text-center" class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px;height:40px;" bodyStyle="text-align:center;max-width:150px">
        <template #header> </template>
        <template #body="md">
          <div v-if="
            store.state.user.is_super == true ||
            store.state.user.user_id == md.node.data.created_by ||
            (store.state.user.role_id == 'admin' &&
              store.state.user.organization_id ==
              md.node.data.organization_id)
          ">
            <Button type="button" icon="pi pi-plus-circle" class="p-button-rounded p-button-secondary p-button-outlined"
              style="margin-right: 0.5rem" v-tooltip.top="'Thêm dự án'"
              @click="addTreeProjectMain(md.node.data)"></Button>
            <Button type="button" icon="pi pi-pencil" v-tooltip.top="'Chỉnh sửa'"
              class="p-button-rounded p-button-secondary p-button-outlined" style="margin-right: 0.5rem"
              @click="editProjectMain(md.node.data)"></Button>
            <Button type="button" icon="pi pi-trash" v-tooltip.top="'Xóa'"
              class="p-button-rounded p-button-secondary p-button-outlined"
              @click="DelProjectMain(md.node.data)"></Button>
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="align-items-center justify-content-center p-4 text-center m-auto" style="
                        min-height: calc(100vh - 220px);
                        max-height: calc(100vh - 220px);
                        display: flex;
                        flex-direction: column;
                      " v-if="!isFirst">
          <img src="../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </TreeTable>
    <!-- end -->
    <!-- dạng LIST -->
    <DataTable v-if="opition.type_view == 1" id="project-main-list" v-model:first="first" :rowHover="true"
      :value="listProjectMains" :paginator="true" :rows="opition.PageSize"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]" :scrollable="true" scrollHeight="flex"
      :totalRecords="opition.totalRecords" :row-hover="true" dataKey="project_id" v-model:selection="selectedProjectMain"
      @page="onPage($event)" @sort="onSort($event)" @filter="onFilter($event)" @rowSelect="onRowSelect($event.data)"
      @rowUnselect="onRowUnselect($event.data)" selectionMode="single" :lazy="true">
      <Column field="STT" header="STT" class="align-items-center justify-content-center text-center font-bold"
        headerStyle="text-align:center;max-width:4rem" bodyStyle="text-align:center;max-width:4rem">
      </Column>
      <Column field="Logo" header="Logo" class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:80px" bodyStyle="text-align:center;max-width:80px">
        <template #body="md">
          <Avatar v-if="md.data.logo" :image="basedomainURL + md.data.logo" class="mr-2" size="large" />
        </template>
      </Column>
      <Column field="project_name" header="Tên dự án" :expander="true" :sortable="true" headerStyle="max-width:auto;">
        <template #body="md">
          <div style="display: flex; align-items: center">
            <span style="margin-left: 5px">{{
              md.data.project_name
            }}</span>
          </div>
        </template>
      </Column>
      <Column field="project_code" header="Mã dự án" class="align-items-center justify-content-center text-center"
        headerStyle="max-width:100px;text-align:center;" bodyStyle="max-width:100px;text-align:center;">
      </Column>
      <Column field="group_name" header="Nhóm dự án" class="align-items-center justify-content-center text-center"
        headerStyle="max-width:300px;text-align:center;" bodyStyle="max-width:300px;text-align:center;">
      </Column>
      <Column field="status" header="Trạng thái" class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px" bodyStyle="text-align:center;max-width:120px">
        <template #body="md">
          <Chip :style="{
            background: md.data.status_bg_color,
            color: md.data.status_text_color,
          }" v-bind:label="md.data.status_name" />
        </template>
      </Column>
      <Column header="Chức năng" headerClass="text-center" class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px" bodyStyle="text-align:center;max-width:150px">
        <template #header> </template>
        <template #body="md">
          <div v-if="
            store.state.user.is_super == true ||
            store.state.user.user_id == md.data.created_by ||
            (store.state.user.role_id == 'admin' &&
              store.state.user.organization_id ==
              md.data.organization_id)
          ">
            <Button type="button" icon="pi pi-plus-circle" class="p-button-rounded p-button-secondary p-button-outlined"
              style="margin-right: 0.5rem" v-tooltip.top="'Thêm dự án'" @click="addTreeProjectMain(md.data)"></Button>
            <Button type="button" icon="pi pi-pencil" v-tooltip.top="'Chỉnh sửa'"
              class="p-button-rounded p-button-secondary p-button-outlined" style="margin-right: 0.5rem"
              @click="editProjectMain(md.data)"></Button>
            <Button type="button" icon="pi pi-trash" v-tooltip.top="'Xóa'"
              class="p-button-rounded p-button-secondary p-button-outlined" @click="DelProjectMain(md.data)"></Button>
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="align-items-center justify-content-center p-4 text-center m-auto" style="
                        min-height: calc(100vh - 220px);
                        max-height: calc(100vh - 220px);
                        display: flex;
                        flex-direction: column;
                      " v-if="!isFirst">
          <img src="../../assets/background/nodata.png" height="144" />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
    </DataTable>
    <!-- end -->
    <!-- dạng GRID -->
    <div id="project-main-list" v-if="opition.type_view == 3" style="
                /* height: 85%; */
                width: 100%;
                display: -webkit-box;
                overflow-x: auto;
                overflow-y: hidden;
              ">
      <div v-if="listProjectMains.length > 0" class="md:col-md-3" v-for="item in listProjectMains"
        style="width: 320px; height: 100%; margin: 0px 10px">
        <span style="
                    padding: 10px;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    font-weight: bold;
                    color: #ffffff;
                  " :style="{
                    background: item.group_view_bg_color + '!important',
                  }">{{ item.group_view_name }} ({{ item.countProject }})</span>
        <div style="width: 106%; height: 95%; overflow: hidden auto" id="task-grid" class="scroll-outer">
          <div class="scroll-inner" style="width: fit-content">
            <Card v-for="cv in item.CVGroup" style="width: 320px; margin-bottom: 2em;">
              <template #title>
                <span @click="onRowSelect(cv)" style="
                            overflow: hidden;
                            font-size: 14px;
                            font-weight: bold;
                            text-overflow: ellipsis;
                            width: 100%;
                            display: -webkit-box;
                            -webkit-line-clamp: 2;
                            -webkit-box-orient: vertical;
                          ">
                  {{ cv.project_name }}
                </span>
              </template>
              <template #content>
                <span v-if="cv.group_name" style="
                            margin: 0px auto;
                            text-align: center;
                            padding: 5px 15px;
                            background-color: #f2f4f6;
                            max-width: max-content;
                            border-radius: 5px;
                            overflow: hidden;
                            text-overflow: ellipsis;
                            white-space: nowrap;
                            max-width: 100%;
                            font-weight: 500;
                          ">{{ cv.group_name }}</span>
                <span v-if="cv.start_date || cv.end_date" style="color: #98a9bc"><i style="margin-right: 5px"
                    class="pi pi-calendar"></i>{{
                      cv.start_date
                      ? moment(new Date(cv.start_date)).format("DD/MM/YYYY")
                      : null
                    }}
                  {{
                    cv.end_date
                    ? '-' + moment(new Date(cv.end_date)).format("DD/MM/YYYY")
                    : null
                  }}</span>
                <span>
                  Công việc ({{ cv.count_taskHT + '/' + cv.count_task }})
                </span>
                <span>
                  <span v-if="cv.isQL" style="
                              background-color: #337ab7;
                              color: #ffffff;
                              display: inline;
                              padding: 0.4em 0.6em;
                              font-size: 75%;
                              font-weight: 700;
                              line-height: 1;
                              color: #fff;
                              text-align: center;
                              white-space: nowrap;
                              vertical-align: baseline;
                              border-radius: 0.25em;
                              margin-left: 10px;
                            ">Quản lý</span>
                  <span v-if="cv.isTT" style="
                              background-color: #5cb85c;
                              color: #ffffff;
                              display: inline;
                              padding: 0.4em 0.6em;
                              font-size: 75%;
                              font-weight: 700;
                              line-height: 1;
                              color: #fff;
                              text-align: center;
                              white-space: nowrap;
                              vertical-align: baseline;
                              border-radius: 0.25em;
                              margin-left: 5px;
                            ">Thực hiện</span>
                  <span v-if="cv.isTD" style="
                              background-color: #5bc0de;
                              color: #ffffff;
                              display: inline;
                              padding: 0.4em 0.6em;
                              font-size: 75%;
                              font-weight: 700;
                              line-height: 1;
                              color: #fff;
                              text-align: center;
                              white-space: nowrap;
                              vertical-align: baseline;
                              border-radius: 0.25em;
                              margin-left: 5px;
                            ">Theo dõi</span>
                </span>
                <span style="
                            display: flex;
                            justify-content: center;
                            align-items: center;
                          ">
                  <AvatarGroup>
                    <div v-for="(value, index) in cv.ThanhvienShows" :key="index">
                      <div>
                        <Avatar v-tooltip.bottom="{
                          value:
                            value.fullName +
                            '<br/>' +
                            (value.tenChucVu || '') +
                            '<br/>' +
                            (value.tenToChuc || ''),
                          escape: true,
                        }" v-bind:label="
  value.avatar
    ? ''
    : (value.ten ?? '').substring(0, 1)
" v-bind:image="basedomainURL + value.avatar" style="
                                    background-color: #2196f3;
                                    color: #ffffff;
                                    width: 32px;
                                    height: 32px;
                                    font-size: 15px !important;
                                    margin-left: -10px;
                                  " :style="{
                                    background: bgColor[index % 7] + '!important',
                                  }" class="cursor-pointer" size="xlarge" shape="circle" />
                      </div>
                    </div>
                    <Avatar v-if="cv.Thanhviens.length - cv.ThanhvienShows.length > 0" :label="
                      '+' +
                      (cv.Thanhviens.length - cv.ThanhvienShows.length) +
                      ''
                    " class="cursor-pointer" shape="circle" style="
                                background-color: #e9e9e9 !important;
                                color: #98a9bc;
                                font-size: 14px !important;
                                width: 32px;
                                margin-left: -10px;
                                height: 32px;
                              " />
                  </AvatarGroup>
                </span>
                <span v-if="cv.title_time" style="
                            width: max-content;
                            font-size: 10px;
                            font-weight: bold;
                            padding: 5px;
                            border-radius: 5px;
                          " :style="{
                            background: cv.time_bg,
                            color: cv.status_text_color,
                          }">{{ cv.title_time }}</span>
                <div class="card-chucnang" style="
                            display: none;
                            flex-direction: column;
                            position: absolute;
                            right: 10px;
                            top:-20px;
                          " v-if="
                            store.state.user.is_super == true ||
                            store.state.user.user_id == cv.created_by ||
                            (store.state.user.role_id == 'admin' &&
                              store.state.user.organization_id ==
                              cv.organization_id)
                          ">
                  <Button @click="editProjectMain(cv)" style="margin-bottom: 5px"
                    class="p-button-rounded p-button-secondary p-button-outlined mx-1" type="button" icon="pi pi-pencil"
                    v-tooltip="'Sửa'"></Button>
                  <Button @click="DelProjectMain(cv)" class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                    type="button" icon="pi pi-trash" v-tooltip="'Xóa'"></Button>
                </div>
              </template>
              <template #footer>
                <!-- <span v-if="cv.progress == 0">{{ cv.progress }} %</span> -->
                <div style="width: 100%">
                  <ProgressBar :value="cv.progress" />
                </div>
              </template>
            </Card>
          </div>
        </div>
      </div>
      <div class="align-items-center justify-content-center p-4 text-center m-auto" style="
                  min-height: calc(100vh - 215px);
                  max-height: calc(100vh - 215px);
                  display: flex;
                  flex-direction: column;
                " v-if="listProjectMains.length == 0">
        <img src="../../assets/background/nodata.png" height="144" />
        <h3 class="m-1">Không có dữ liệu</h3>
      </div>
    </div>
    <!-- end -->
    <!-- dạng GANTT -->
    <div id="project-main-list" v-if="opition.type_view == 4" style="
                /* max-height: calc(100vh - 500px);
                min-height: calc(100vh - 150px); */
                display: -webkit-box;
                overflow-x: auto;
                overflow-y: hidden;
              " class="grid formgrid m-2">
      <div class="field col-12 md:col-12" style="
                  display: flex;
                  padding: 0px;
                  height: 100%;
                ">
        <div class="col-12 scrollbox_delayed" style="height: 100%; padding: 0px; overflow: auto">
          <table class="table table-border" style="
                      width: max-content;
                      table-layout: fixed;
                      min-width: 100%;
                      border-collapse: collapse;
                      overflow-x: scroll;
                    ">
            <thead style="
                        background-color: #f8f9fa;
                        position: sticky;
                        top: 0px;
                        z-index: 5;
                      ">
              <tr>
                <th class="fixcol left-0 p-3" rowspan="3" style="width: 200px; border: 1px solid #e9e9e9">
                  Dự án
                </th>
                <th class="fixcol left-200 p-3" rowspan="3" style="width: 150px; border: 1px solid #e9e9e9">
                  Thành viên
                </th>
                <th class="fixcol left-350 p-3" rowspan="3" style="width: 100px; border: 1px solid #e9e9e9">
                  Nhóm dự án
                </th>
                <!-- sa
                    <th class="fixcol left-450 p-3" rowspan="3" style="width: 100px; border: 1px solid #e9e9e9">
                      Kết thúc
                    </th> -->
                <th v-for="m in Grands" class="p-3" align="center" :width="m.Dates.length * 40" :colspan="m.Dates.length"
                  style="text-align: center; min-width: 100px; color: #2196f3">
                  Tháng {{ m.Month }}/{{ m.Year }}
                </th>
                <!-- <th class="p-3" style="border: 1px solid #e9e9e9;" :colspan="GrandsDate.length">Tháng 2</th> -->
              </tr>
              <tr>
                <th class="no-fixcol p-3" width="40" style="border: 1px solid #e9e9e9" :style="
                  (g.bg == ''
                    ? 'background-color: #fff;'
                    : 'background-color:' + g.bg + ';',
                    'color:' + g.color)
                " v-for="g in GrandsDate">
                  {{ g.DayName }}
                </th>
              </tr>
              <tr>
                <th class="no-fixcol p-3" width="40" style="border: 1px solid #e9e9e9" :style="
                  (g.bg == ''
                    ? 'background-color: #fff;'
                    : 'background-color:' + g.bg + ';',
                    'color:' + g.color)
                " v-for="g in GrandsDate">
                  {{ g.DayN }}
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="l in listProjectMains" @click="onRowSelect(l)">
                <td class="fixcol left-0 p-3" style="border: 1px solid #e9e9e9; background-color: #f8f9fa">
                  <label @click="onRowSelect(l)" style="font-weight: bold;">{{ l.project_name }}</label>
                  <div style="font-size: 12px;margin-top: 5px;display: flex;align-items: center;">
                    <span v-if="l.start_date || l.end_date" style="color: #98a9bc">{{
                      l.start_date
                      ? moment(new Date(l.start_date)).format(
                        "DD/MM/YYYY",
                      )
                      : null
                    }}
                      {{
                        l.end_date
                        ? '- ' + moment(new Date(l.end_date)).format("DD/MM/YYYY")
                        : null
                      }}</span>
                  </div>
                </td>
                <td class="fixcol left-200 p-3" style="border: 1px solid #e9e9e9; background-color: #f8f9fa">
                  <div style="display: flex; justify-content: center">
                    <AvatarGroup>
                      <div v-for="(value, index) in l.ThanhvienShows" :key="index">
                        <div>
                          <Avatar v-tooltip.bottom="{
                            value:
                              value.fullName +
                              '<br/>' +
                              (value.tenChucVu || '') +
                              '<br/>' +
                              (value.tenToChuc || ''),
                            escape: true,
                          }" v-bind:label="
  value.avatar
    ? ''
    : (value.ten ?? '').substring(0, 1)
" v-bind:image="basedomainURL + value.avatar" style="
                                      background-color: #2196f3;
                                      color: #ffffff;
                                      width: 32px;
                                      height: 32px;
                                      font-size: 15px !important;
                                      margin-left: -10px;
                                    " :style="{
                                      background: bgColor[index % 7] + '!important',
                                    }" class="cursor-pointer" size="xlarge" shape="circle" />
                        </div>
                      </div>
                      <Avatar v-if="l.countThanhviens - l.countThanhvienShows > 0" :label="
                        '+' +
                        (l.countThanhviens - l.countThanhvienShows) +
                        ''
                      " class="cursor-pointer" shape="circle" style="
                                  background-color: #e9e9e9 !important;
                                  color: #98a9bc;
                                  font-size: 14px !important;
                                  width: 32px;
                                  margin-left: -10px;
                                  height: 32px;
                                " />
                    </AvatarGroup>
                  </div>
                </td>
                <td class="fixcol left-350 p-3"
                  style="border: 1px solid #e9e9e9; background-color: #f8f9fa; text-align: center;">
                  {{
                    l.group_name
                  }}
                </td>
                <!-- <td class="fixcol left-450 p-3"
                      style="border: 1px solid #e9e9e9; background-color: #f8f9fa; text-align: center;">
                      {{
                        l.end_date
                        ? moment(new Date(l.end_date)).format("DD/MM/YYYY HH:mm")
                        : ""
                      }}
                    </td> -->
                <td class="no-fixcol-hover" style="background-color: #fff; border: 1px solid #e9e9e9" width="40"
                  :colspan="g.IsCheck ? l.totalDay : 1" :style="
                    (g.Name
                      ? 'background-color: #fff;'
                      : 'background-color:' + g.bg + ';',
                      'color:' + g.color)
                  " v-for="g in l.dateArray">
                  <div v-if="g.Name" class="divbg" :style="
                    'background-color:' +
                    l.status_bg_color +
                    '!important;color:' +
                    l.status_text_color
                  ">
                    {{ g.Name }}
                  </div>
                </td>
              </tr>
              <tr v-if="listProjectMains.length == 0">
                <td :colspan="GrandsDate.length + 4" style="text-align: center">
                  <div class="align-items-center justify-content-center p-4 text-center m-auto" style="
                              min-height: calc(100vh - 215px);
                              max-height: calc(100vh - 215px);
                              display: flex;
                              flex-direction: column;
                            " v-if="listProjectMains != null || opition.totalRecords == 0">
                    <img src="../../assets/background/nodata.png" height="144" />
                    <h3 class="m-1">Không có dữ liệu</h3>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
    <!-- end -->
    <!-- dạng User -->
    <div id="project-main-list" v-if="opition.type_view == 5" style="
            display: -webkit-box;
            overflow-x: auto;
            overflow-y: hidden;
          " class="grid formgrid m-2">
      <div class="field col-12 md:col-12" style="
              display: flex;
              padding: 0px;
              height: 100%;
            ">
        <div class="col-12 scrollbox_delayed" style="height: 100%; padding: 0px; overflow: auto">
          <table class="table table-border" style="
                  width: max-content;
                  table-layout: fixed;
                  min-width: 100%;
                  border-collapse: collapse;
                  overflow-x: scroll;
                ">
            <thead style="
                    background-color: #f8f9fa;
                    position: sticky;
                    top: 0px;
                    z-index: 5;
                  ">
              <tr>
                <th class="fixcol left-0 p-3" rowspan="3" style="width: 200px; border: 1px solid #e9e9e9">
                  <div style="
                          display: flex;
                          justify-content: center;
                          flex-direction: column;
                          align-items: center;
                        ">
                    <span style="margin-bottom: 5px">
                      Thành viên {{ "(" + listDropdownUser.length + ")" }}
                    </span>
                    <span>
                      <AvatarGroup>
                        <div v-for="(value, index) in listThanhVien" :key="index">
                          <div>
                            <Avatar v-bind:label="
                              value.avatar
                                ? ''
                                : (value.ten ?? '').substring(0, 1)
                            " v-bind:image="basedomainURL + value.avatar" 
                            style="
                                    background-color: #2196f3;
                                    color: #ffffff;
                                    width: 32px;
                                    height: 32px;
                                    font-size: 15px !important;
                                    margin-left: -10px;
                                  " :style="{
                                    background: bgColor[index % 7] + '!important',
                                  }" class="cursor-pointer" size="xlarge" shape="circle" />
                          </div>
                        </div>
                        <Avatar v-if="
                          listDropdownUser.length - listThanhVien.length > 0
                        " :label="
  '+' +
  (listDropdownUser.length - listThanhVien.length) +
  ''
" class="cursor-pointer" shape="circle" style="
                                background-color: #e9e9e9 !important;
                                color: #98a9bc;
                                font-size: 14px !important;
                                width: 32px;
                                margin-left: -10px;
                                height: 32px;
                              " />
                      </AvatarGroup>
                    </span>
                  </div>
                </th>
                <th v-for="m in Grands" class="p-3" align="center" :width="m.Dates.length * 40" :colspan="m.Dates.length"
                  style="text-align: center; min-width: 100px; color: #2196f3">
                  Tháng {{ m.Month }}/{{ m.Year }}
                </th>
              </tr>
              <tr>
                <th class="no-fixcol p-3" width="40" style="border: 1px solid #e9e9e9" :style="
                  (g.bg == ''
                    ? 'background-color: #fff;'
                    : 'background-color:' + g.bg + ';',
                    'color:' + g.color)
                " v-for="g in GrandsDate">
                  {{ g.DayName }}
                </th>
              </tr>
              <tr>
                <th class="no-fixcol p-3" width="40" style="border: 1px solid #e9e9e9" :style="
                  (g.bg == ''
                    ? 'background-color: #fff;'
                    : 'background-color:' + g.bg + ';',
                    'color:' + g.color)
                " v-for="g in GrandsDate">
                  {{ g.DayN }}
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(l, index) in listProjectMains">
                <td class="fixcol left-0 p-3" style="
                        height: 40px;
                        border: 1px solid #dedede;
                        background-color: #f8f9fa;
                      " v-if="l.IsHienThi" :rowspan="l.count_cv">
                  <div style="display: flex; align-items: center; padding: 10px">
                    <span>
                      <Avatar v-tooltip.bottom="{
                        value:
                          l.user_name +
                          '<br/>' +
                          (l.tenChucVu || '') +
                          '<br/>' +
                          (l.tenToChuc || ''),
                        escape: true,
                      }" v-bind:label="
  l.avatar ? '' : (l.last_name ?? '').substring(0, 1)
" v-bind:image="basedomainURL + l.avatar" style="
                              background-color: #2196f3;
                              color: #ffffff;
                              width: 3.5rem;
                              height: 3.5rem;
                              font-size: 15px !important;
                            " :style="{
                              background: bgColor[(index % 7) + 1] + '!important',
                            }" class="cursor-pointer" size="xlarge" shape="circle" />
                    </span>
                    <span style="margin-left: 10px">
                      <div style="
                              display: flex;
                              flex-direction: column;
                              line-height: 20px;
                            ">
                        <b style="font-size: 13px">{{ l.user_name }}</b>
                        <div style="
                                font-weight: 600;
                                color: #72777a;
                                font-size: 12px;
                              ">
                          {{ l.tenChucVu }}
                        </div>
                        <div style="font-weight: 500; font-size: 11px">
                          {{ l.tenToChuc }}
                        </div>
                        <div style="display: flex">
                          <span v-if="l.count_istype_0 > 0" style="
                                  background-color: #337ab7;
                                  color: #ffffff;
                                  display: inline;
                                  padding: 0.4em 0.6em;
                                  font-size: 75%;
                                  font-weight: 700;
                                  line-height: 1;
                                  color: #fff;
                                  text-align: center;
                                  white-space: nowrap;
                                  vertical-align: baseline;
                                  border-radius: 0.25em;
                                  margin-left: 10px;
                                ">Quản lý {{ l.count_istype_0 }}</span>
                          <span v-if="l.count_istype_1 > 0" style="
                                  background-color: #5cb85c;
                                  color: #ffffff;
                                  display: inline;
                                  padding: 0.4em 0.6em;
                                  font-size: 75%;
                                  font-weight: 700;
                                  line-height: 1;
                                  color: #fff;
                                  text-align: center;
                                  white-space: nowrap;
                                  vertical-align: baseline;
                                  border-radius: 0.25em;
                                  margin-left: 5px;
                                ">Tham gia {{ l.count_istype_1 }}</span>
                          <span v-if="l.count_istype_3 > 0" style="
                                  background-color: #5bc0de;
                                  color: #ffffff;
                                  display: inline;
                                  padding: 0.4em 0.6em;
                                  font-size: 75%;
                                  font-weight: 700;
                                  line-height: 1;
                                  color: #fff;
                                  text-align: center;
                                  white-space: nowrap;
                                  vertical-align: baseline;
                                  border-radius: 0.25em;
                                  margin-left: 5px;
                                ">Theo dõi {{ l.count_istype_3 }}</span>
                        </div>
                      </div>
                    </span>
                  </div>
                </td>
                <td @click="onRowSelect(l)" rowspan="1" class="no-fixcol-hover" style="
                        background-color: #fff;
                        border: 1px solid #e9e9e9;
                        height: 40px;
                      " width="40" :colspan="g.IsCheck ? l.totalDay : 1" :style="
                        (g.Name
                          ? 'background-color: #fff;'
                          : 'background-color:' + g.bg + ';',
                          'color:' + g.color)
                      " v-for="g in l.dateArray">
                  <div v-if="g.Name" class="divbg" :style="
                    'background-color:' +
                    l.time_bg +
                    '!important;color:' +
                    l.status_text_color
                  ">
                    {{ g.Name }}
                  </div>
                </td>
              </tr>
              <tr v-if="listProjectMains.length == 0">
                <td :colspan="GrandsDate.length + 4" style="text-align: center">
                  <div class="align-items-center justify-content-center p-4 text-center m-auto" style="
                          min-height: calc(100vh - 215px);
                          max-height: calc(100vh - 215px);
                          display: flex;
                          flex-direction: column;
                        " v-if="listProjectMains != null || opition.totalRecords == 0">
                    <img src="../../assets/background/nodata.png" height="144" />
                    <h3 class="m-1">Không có dữ liệu</h3>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
    <!-- end -->
    <Sidebar v-model:visible="showDetailProject" :position="PositionSideBar" :style="{
      width:
        PositionSideBar == 'right'
          ? width1 > 1800
            ? ' 60vw'
            : '80vw'
          : '100vw',
      'height': '100vh !important',
    }" :showCloseIcon="false">
      <div
        style="position: absolute;z-index: 10;left: 0px;padding-bottom: 10px;background-color: #fff;width: 66px;display: flex;">
        <Button icon="pi pi-times" class="p-button-rounded p-button-text" v-tooltip="{ value: 'Đóng' }"
          @click="closeSildeBar()" />
        <Button icon="pi pi-window-maximize" class="p-button-rounded p-button-text" v-tooltip="{ value: 'Phóng to' }"
          @click="MaxMin('full')" v-if="PositionSideBar == 'right'" />
        <Button icon="pi pi-window-minimize" class="p-button-rounded p-button-text" v-tooltip="{ value: 'Thu nhỏ' }"
          @click="MaxMin('right')" v-if="PositionSideBar == 'full'" />
      </div>
      <DetailProject :isShow="showDetailProject" :id="selectedProjectMainID" :turn="0">
      </DetailProject>
    </Sidebar>

    <Dialog :header="headerAddProjectMain" v-model:visible="displayProjectMain" :style="{ width: '40vw' }"
      :closable="true" :maximizable="true">
      <form>
        <div class="grid formgrid m-2">
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left p-0">Mã dự án<span class="redsao"> (*) </span></label>
            <InputText v-model="ProjectMain.project_code" @change="removeVietnameseTones(ProjectMain.project_code)"
              spellcheck="false" class="col-9 ip36 px-2"
              :class="{ 'p-invalid': v$.project_code.$invalid && submitted }" />
          </div>
          <div style="display: flex" class="field col-12 md:col-12">
            <div class="col-3 text-left"></div>
            <small v-if="
              (v$.project_code.$invalid && submitted) ||
              v$.project_code.$pending.$response
            " class="col-9 p-error p-0">
              <span class="col-12 p-0">{{
                v$.project_code.required.$message
                  .replace("Value", "Mã dự án")
                  .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left p-0">Tên dự án<span class="redsao"> (*) </span></label>
            <InputText v-model="ProjectMain.project_name" spellcheck="false" class="col-9 ip36 px-2" />
          </div>
          <div style="display: flex" class="field col-12 md:col-12">
            <div class="col-3 text-left"></div>
            <small v-if="
              (v$.project_name.$invalid && submitted) ||
              v$.project_name.$pending.$response
            " class="col-9 p-error p-0">
              <span class="col-12 p-0">{{
                v$.project_name.required.$message
                  .replace("Value", "Tên dự án")
                  .replace("is required", "không được để trống")
              }}</span>
            </small>
          </div>
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left p-0">Nhóm dự án</label>
            <MultiSelect :filter="true" v-model="arrNhom" :options="listProjectGroups" optionValue="code"
              optionLabel="name" class="col-9 ip36 p-0" placeholder="----Chọn nhóm dự án----"
              @change="changeMaNhom($event)" display="chip">
              <template #option="slotProps">
                <div class="country-item flex" style="align-items: center; margin-left: 10px">
                  <div class="pt-1" style="padding-left: 10px">
                    {{ slotProps.option.name }}
                  </div>
                </div>
              </template>
            </MultiSelect>
          </div>
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left p-0">Cấp cha</label>
            <TreeSelect class="col-9" v-model="selectcapcha" :options="listDropdownParent" :showClear="true"
              :max-height="200" placeholder="" optionLabel="project_name" optionValue="project_id" />
          </div>
          <div class="field col-12 md:col-12 flex">
            <label class="col-3 text-left p-0">Logo</label>
            <div class="col-9 p-0">
              <div class="inputanh relative">
                <img @click="chonanh('AnhDonvi')" id="LogoDonvi" style="height: 80px; width: 100px" v-bind:src="
                  ProjectMain.logo
                    ? basedomainURL + ProjectMain.logo
                    : basedomainURL + '/Portals/Image/noimg.jpg'
                " />
                <Button v-if="isDisplayAvt || ProjectMain.logo" style="width: 1.5rem; height: 1.5rem" icon="pi pi-times"
                  @click="delLogo(ProjectMain)" class="p-button-rounded absolute top-0 right-0 cursor-pointer" />
              </div>
              <input class="ipnone" style="display: none" id="AnhDonvi" type="file" accept="image/*"
                @change="handleFileUpload($event, 'LogoDonvi')" />
            </div>
          </div>
          <div class="field col-12 md:col-12" style="display: flex">
            <label class="col-3 text-left p-0">Mô tả</label>
            <Textarea style="margin-top: 5px; padding: 5px" v-model="ProjectMain.description" class="col-9 ip36"
              :autoResize="true" rows="5" cols="30" />
          </div>
          <div class="field col-12 md:col-12" style="display: flex; align-items: center">
            <label class="col-3 text-left p-0">Ngày bắt đầu</label>
            <div class="col-9" style="display: flex; padding: 0px; align-items: center">
              <Calendar :manualInput="true" :showIcon="true" class="col-5 ip36 title-lable"
                style="margin-top: 5px; padding: 0px" id="time1" autocomplete="on" v-model="ProjectMain.start_date" />
              <div class="col-7" style="display: flex; padding: 0px; align-items: center">
                <label class="col-5 text-center">Ngày kết thúc</label>
                <Calendar :manualInput="true" :showIcon="true" class="col-7 ip36 title-lable"
                  style="margin-top: 5px; padding: 0px" id="time2" placeholder="dd/MM/yy" autocomplete="on"
                  v-model="ProjectMain.end_date" @date-select="CheckDate($event)" />
              </div>
            </div>
          </div>
          <div class="field col-12 md:col-12" style="display: flex">
            <label style="display: flex; align-items: center" class="col-3 text-left p-0">Từ khóa</label>
            <Chips v-model="ProjectMain.keywords" spellcheck="false" class="col-9 ip36" style="padding: 0px"
              placeholder="Ấn Enter sau mỗi từ khóa!" />
          </div>
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left p-0">Trạng thái dự án</label>
            <Dropdown :filter="true" style="margin-top: 5px" v-model="ProjectMain.status" :options="listDropdownStatus"
              optionLabel="text" optionValue="value" placeholder="Trạng thái dự án" spellcheck="false"
              class="col-9 ip36 p-0">
              <template #option="slotProps">
                <div class="country-item flex">
                  <div class="pt-1">{{ slotProps.option.text }}</div>
                </div>
              </template>
            </Dropdown>
          </div>
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left p-0">STT</label>
            <InputNumber v-model="ProjectMain.is_order" style="padding: 0px !important" class="col-9 ip36 px-2" />
          </div>

          <div class="field col-12 md:col-12">
            <label class="col-3 text-left p-0">Người quản lý
              <span @click="OpenDialogTreeUser(false, 1)" class="choose-user"><i
                  class="pi pi-user-plus"></i></span></label>
            <MultiSelect :filter="true" v-model="ProjectMain.managers" :options="listDropdownUser" optionValue="code"
              optionLabel="name" class="col-9 ip36 p-0" placeholder="Người quản lý" display="chip">
              <template #option="slotProps">
                <div class="country-item flex" style="align-items: center; margin-left: 10px">
                  <Avatar v-bind:label="
                    slotProps.option.avatar
                      ? ''
                      : (slotProps.option.name ?? '').substring(0, 1)
                  " v-bind:image="basedomainURL + slotProps.option.avatar" style="
                                background-color: #2196f3;
                                color: #ffffff;
                                width: 32px;
                                height: 32px;
                                font-size: 15px !important;
                                margin-left: -10px;
                              " :style="{
                                background: bgColor[slotProps.index % 7] + '!important',
                              }" class="cursor-pointer" size="xlarge" shape="circle" />
                  <div class="pt-1" style="padding-left: 10px">
                    {{ slotProps.option.name }}
                  </div>
                </div>
              </template>
            </MultiSelect>
          </div>
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left p-0">Người tham gia
              <span @click="OpenDialogTreeUser(false, 2)" class="choose-user"><i
                  class="pi pi-user-plus"></i></span></label>
            <MultiSelect :filter="true" v-model="ProjectMain.participants" :options="listDropdownUser" optionValue="code"
              optionLabel="name" class="col-9 ip36 p-0" placeholder="Người tham gia" display="chip">
              <template #option="slotProps">
                <div class="country-item flex" style="align-items: center; margin-left: 10px">
                  <Avatar v-bind:label="
                    slotProps.option.avatar
                      ? ''
                      : (slotProps.option.name ?? '').substring(0, 1)
                  " v-bind:image="basedomainURL + slotProps.option.avatar" style="
                                background-color: #2196f3;
                                color: #ffffff;
                                width: 32px;
                                height: 32px;
                                font-size: 15px !important;
                                margin-left: -10px;
                              " :style="{
                                background: bgColor[slotProps.index % 7] + '!important',
                              }" class="cursor-pointer" size="xlarge" shape="circle" />
                  <div class="pt-1" style="padding-left: 10px">
                    {{ slotProps.option.name }}
                  </div>
                </div>
              </template>
            </MultiSelect>
          </div>
          <div class="field col-12 md:col-12">
            <Accordion :multiple="true">
              <AccordionTab header="TÀI LIỆU THAM KHẢO">
                <div class="field col-12 md:col-12" id="task_file" style="display: flex">
                  <label class="col-3 text-left p-0">File</label>
                  <div class="col-9 p-0">
                    <FileUpload chooseLabel="Chọn File" style="margin-top: 5px !important" :showUploadButton="false"
                      :showCancelButton="false" :multiple="true" accept="" :maxFileSize="10000000" @select="onUploadFile"
                      @remove="removeFile" />
                    <div class="col-12 p-0" style="border: 1px solid #e1e1e1; margin-top: -1px">
                      <DataView :lazy="true" :value="ProjectMain.files" :rowHover="true" :scrollable="true"
                        class="w-full h-full ptable p-datatable-sm flex flex-column col-10 ip36 p-0" layout="list"
                        responsiveLayout="scroll">
                        <template #list="slotProps">
                          <Toolbar class="w-full" style="display: flex">
                            <template #start>
                              <div class="flex align-items-center task-file-list">
                                <img class="mr-2" :src="
                                  basedomainURL +
                                  '/Portals/Image/file/' +
                                  slotProps.data.file_type +
                                  '.png'
                                " style="object-fit: contain" width="40" height="40" />
                                <span style="line-height: 1.5; word-break: break-all">
                                  {{ slotProps.data.file_name }}</span>
                              </div>
                            </template>
                            <template #end>
                              <Button icon="pi pi-times" class="p-button-rounded p-button-danger"
                                @click="deleteFile(slotProps.data)" />
                            </template>
                          </Toolbar>
                        </template>
                      </DataView>
                    </div>
                  </div>
                </div>
              </AccordionTab>
            </Accordion>
          </div>
        </div>
      </form>
      <template #footer>
        <Button label="Hủy" icon="pi pi-times" @click="closeDialogProjectMain" class="p-button-text" />
        <Button label="Lưu" icon="pi pi-check" @click="saveProjectMain(!v$.$invalid)" />
      </template>
    </Dialog>
  </div>
  <treeuser v-if="displayDialogUser === true" :headerDialog="headerDialogUser" :displayDialog="displayDialogUser"
    :one="is_one" :selected="selectedUser" :closeDialog="closeDialog" :choiceUser="choiceTreeUser" />
</template>
<style>
.p-treeselect-panel {
  max-width: 398px;
}

.p-treeselect-panel ul li .p-treenode-label {
  white-space: pre-line;
}

.p-chip {
  border-radius: 5px !important;
}

.choose-user {
  color: #2196f3;
}

.choose-user:hover {
  cursor: pointer;
}

#toolbar_right .active {
  background-color: #2196f3 !important;
  border: 1px solid #5ca7e3 !important;
  color: #fff;
}

#toolbar_right li {
  list-style: none;
  display: flex;
  justify-content: center;
  align-items: center;
  height: 30px;
  border: 1px solid;
  border-radius: 4px;
  margin: 0px 5px 0px 0px;
}

#toolbar_right li a {
  padding: 0px 10px;
}

#toolbar_right li:hover {
  cursor: pointer;
  background-color: #2196f3 !important;
  border: 1px solid #5ca7e3 !important;
  color: #fff;
}

#project-main-list {
  height: 94% !important;
}

#task_list_type .p-menuitem div {
  padding: 5px 10px;
  display: flex;
  align-items: center;
}

#task_list_type .p-menuitem:hover {
  cursor: pointer;
  background-color: #e9ecef;
}

#task_list_type .p-menuitem .active {
  color: #2196f3 !important;
}

#task_list_type .p-menuitem i {
  margin-right: 5px;
}

#project-main-list .table thead tr .fixcol {
  z-index: 5;
  color: #000;
  font-weight: 600;
  position: sticky;
  /* background: #f5f5f5; */
  background-color: #f8f9fa;
  outline: 1px solid #e9e9e9;
  border: none;
  vertical-align: middle;
}

#project-main-list .table thead tr th {
  outline: 1px solid #e9e9e9;
}

#project-main-list .table tbody tr .fixcol {
  position: sticky;
  z-index: 0;
  color: #000;
  font-weight: 400;
  /* background: #f5f5f5; */
  background-color: #f8f9fa;
  outline: 1px solid #e9e9e9;
  border: none;
  vertical-align: middle;
}

#project-main-list .left-0 {
  left: 0px;
}

#project-main-list .left-200 {
  left: 200px;
}

#project-main-list .left-350 {
  left: 350px;
}

#project-main-list .left-450 {
  left: 450px;
}

#task_sort .p-menuitem {
  padding: 5px 10px;
}

#task_sort .p-menuitem:hover {
  cursor: pointer;
  background-color: #e9ecef;
}

#task_sort .p-menuitem .active {
  color: #2196f3 !important;
}

#task_filter .p-menuitem {
  padding: 5px 10px;
  list-style: none;
}

#task_filter .parent:hover {
  cursor: pointer;
  background-color: #e9ecef;
}

#task_filter .parent .active {
  color: #2196f3 !important;
}

#task_filter .children .active {
  color: #2196f3 !important;
}
</style>
