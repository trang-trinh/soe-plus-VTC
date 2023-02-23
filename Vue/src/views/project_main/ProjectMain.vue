<script setup>
import { ref, inject, onMounted, watch, onBeforeUnmount } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import DetailedWork from "../../components/task_origin/DetailedWork.vue";
import moment from "moment";
import { concat } from "lodash";
import { encr } from "../../util/function.js";
import treeuser from "../../components/user/treeuser.vue";
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
              { par: "organization_id",va: store.getters.user.organization_id,},
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
      // if (listDropdownUser.value.length > 10) {
      //   listThanhVien.value = listDropdownUser.value.slice(0, 10);
      // } else {
      //   listThanhVien.value = [...listDropdownUser.value];
      // }
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
        });
        let obj = renderTreeDV(
          listData.value,
          "project_id",
          "project_name",
          "dự án",
        );
        listProjectMains.value = obj.arrChils;
        treelistProjectMains.value = obj.arrtreeChils;
        console.log("sk1", obj, data);
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
    is_order: listProjectMains.value.length + 1,
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
  if(files["LogoDonvi"]){
    formData.append("LogoDonvi", JSON.stringify(files["LogoDonvi"].name));
    fileAll.push(files["LogoDonvi"]);
  }else{
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
  };
  first.value = 0;
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

onMounted(() => {
  listUser();
  loadData(true);
  loadCountProjectGroup();
  listtreeProjectMain();

  return {};
});
</script>
<template><!-- @nodeSelect="onNodeSelect" @nodeUnselect="onNodeUnselect" selectionMode="checkbox" -->
  <div v-if="store.getters.islogin" class="main-layout true flex-grow-1 p-2">
    <TreeTable :value="listProjectMains" v-model:selectionKeys="selectedKey" v-model:first="first"
      :loading="opition.loading" @page="onPage($event)" @sort="onSort($event)" :paginator="true" :rows="opition.PageSize"
      :totalRecords="opition.totalRecords"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[20, 30, 50, 100, 200]" :filters="filters" :showGridlines="true" filterMode="strict"
      class="p-treetable-sm" :rowHover="true" responsiveLayout="scroll" :lazy="true" :scrollable="true"
      scrollHeight="flex">
      <template #header>
        <h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
          <i class="pi pi-microsoft"></i> Danh sách dự án ({{
            opition.totalRecords
          }})
        </h3>
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
            <Button class="mr-2 p-button-outlined p-button-secondary" icon="pi pi-refresh" @click="onRefersh" />
            <Button label="Xoá" icon="pi pi-trash" class="mr-2 p-button-danger" v-if="selectedNodes.length > 0"
              @click="DelProjectMain()" />
            <!-- <Button label="Export" icon="pi pi-file-excel" class="mr-2 p-button-outlined p-button-secondary"
                              @click="toggleExport" aria-haspopup="true" aria-controls="overlay_Export" /> -->
            <Menu vị id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" />
          </template>
        </Toolbar>
      </template>
      <Column field="STT" header="STT"
        class="align-items-center justify-content-center text-center font-bold"
        headerStyle="text-align:center;max-width:4rem" bodyStyle="text-align:center;max-width:4rem">
      </Column>
      <Column field="Logo" header="Logo" class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:80px" bodyStyle="text-align:center;max-width:80px">
        <template #body="md">
          <Avatar v-if="md.node.data.logo" :image="basedomainURL + md.node.data.logo" class="mr-2" size="large" />
        </template>
      </Column>
      <Column field="project_name" header="Tên dự án" :expander="true" :sortable="true" headerStyle="max-width:auto;">
        <template #body="md">
          <div style="display: flex; align-items: center">
            <span style="margin-left: 5px">{{
              md.node.data.project_name
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
            background: md.node.data.status_bg_color,
            color: md.node.data.status_text_color,
          }" v-bind:label="md.node.data.status_name" />
        </template>
      </Column>
      <Column header="Chức năng" headerClass="text-center" class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px" bodyStyle="text-align:center;max-width:150px">
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
            <label class="col-3 text-left p-0"
              >Người quản lý
              <span
                @click="OpenDialogTreeUser(false, 1)"
                class="choose-user"
                ><i class="pi pi-user-plus"></i></span
              ></label
            >
            <MultiSelect
              :filter="true"
              v-model="ProjectMain.managers"
              :options="listDropdownUser"
              optionValue="code"
              optionLabel="name"
              class="col-9 ip36 p-0"
              placeholder="Người quản lý"
              display="chip"
            >
              <template #option="slotProps">
                <div
                  class="country-item flex"
                  style="align-items: center; margin-left: 10px"
                >
                  <Avatar
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
          <div class="field col-12 md:col-12">
            <label class="col-3 text-left p-0"
              >Người tham gia
              <span
                @click="OpenDialogTreeUser(false, 2)"
                class="choose-user"
                ><i class="pi pi-user-plus"></i></span
              ></label
            >
            <MultiSelect
              :filter="true"
              v-model="ProjectMain.participants"
              :options="listDropdownUser"
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
  <treeuser
    v-if="displayDialogUser === true"
    :headerDialog="headerDialogUser"
    :displayDialog="displayDialogUser"
    :one="is_one"
    :selected="selectedUser"
    :closeDialog="closeDialog"
    :choiceUser="choiceTreeUser"
  />
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
</style>
