<script setup>
import { ref, inject, onMounted, onBeforeUnmount, watch } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import { VuemojiPicker } from "vuemoji-picker";
import DetailedChild from "../task_origin/DetailedWork.vue";
import membersVue from "./Detail_Task/membersInfo.vue";
import taskFileVue from "./Detail_Task/taskFile.vue";
import viewedMemberVue from "./Detail_Task/viewedMember.vue";
import taskActiveVue from "./Detail_Task/TaskActivate.vue";
import reviewTaskVue from "./Detail_Task/reviewTask.vue";
import taskExtendsVue from "./Detail_Task/taskExtends.vue";
import FileInfoVue from "./Detail_Task/FileInfo.vue";
import Task_FollowVue from "./Detail_Task/Task_Follow.vue";
import TaskCheckListDetailVue from "./Detail_Task/TaskCheckListDetail.vue";
import { encr } from "../../util/function.js";
import moment from "moment";
import DocLinkTaskVue from "./Detail_Task/DocTask.vue";`
import treeuser from "../../components/user/treeuser.vue";`

const cryoptojs = inject("cryptojs");
const options = ref({});
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const emitter = inject("emitter");

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const today = ref({});
today.value = new Date();
const basedomainURL = fileURL;
//Lấy size màn hình
const width1 = ref(window.screen.availWidth);
const height1 = ref(window.screen.height);
//Khai báo
const user = store.state.user;
const isShow = ref(false);
const isFirst = ref(true);
const datalists = ref({});
const members = ref();

const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);

const listOrganization = ref([]);
const listDropdownorganization = ref([]);
const listCheckList = ref();
const listTaskCheckList = ref();
const countChecklists = ref();
const countMembers = ref();
const countComments = ref();
const countFiles = ref();
const ngv = ref();
const nth = ref();
const ndth = ref();
const ntd = ref();
const countChecked = ref();
const countUnChecked = ref();
const ExpireTimeTask = ref();
const TimeToDo = ref();
//
const props = defineProps({
  isShow: Boolean,
  id: String,
  turn: Intl,
});
//Tải dữ liệu
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
//Load Main Task Detail
const listtreeOrganization = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_list_pb",
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
      let obj = renderTreeDV(
        data.filter((x) => x.organization_type != 0),
        "organization_id",
        "organization_name",
        "phòng ban",
      );
      listOrganization.value = data;
      listDropdownorganization.value = obj.arrtreeChils;
      let id = parseInt(Object.keys(selectcapcha.value)[0]);
      if (id != -1) {
        listOrganization.value
          .filter((x) => x.organization_id == id)
          .forEach((t) => {
            if (t.user_id) {
              Task.value.assign_user_id.push(store.state.user.user_id);
              Task.value.work_user_ids.push(t.user_id);
            } else {
              selectcapcha.value = [];
              selectcapcha.value[-1] = true;
              Task.value.assign_user_id.push(store.state.user.user_id);
              Task.value.work_user_ids.push(store.state.user.user_id);
              swal.fire({
                title: "Thông báo!",
                text: "Phòng ban này chưa tồn tại người chủ trì!",
                icon: "info",
                confirmButtonText: "OK",
              });
            }
          });
      }
    })
    .catch((error) => {
      //// toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const renderTreeDV = (data, id, name, title) => {
  let arrChils = [];
  let arrtreeChils = [];
  // .filter((x) => x.parent_id == null)
  data.forEach((m, i) => {
    m.IsOrder = i + 1;
    let om = { key: m[id], data: m };
    const rechildren = (mm, pid) => {
      let dts = data.filter((x) => x.parent_id == pid);
      if (dts.length > 0) {
        if (!mm.children) mm.children = [];
        dts.forEach((em) => {
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

const listFile = ref();
const countAllFile = ref();
const loadFile = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_file_get",
            par: [
              { par: "id", va: props.id },
              { par: "type", va: 1 },
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
      let count = JSON.parse(response.data.data)[1];
      let count2 = JSON.parse(response.data.data)[2];
      data.forEach((element) => {
        element.filesize_display = formatSize(element.file_size);
        element.creator = JSON.parse(element.creator);
        element.creator_tooltip =
          "Người tạo <br/>" +
            element.creator.full_name +
            "<br/>" +
            element.creator.positions ??
          "" +
            "<br/>" +
            (element.creator.department_name != null
              ? element.creator.department_name
              : element.creator.organiztion_name);
      });
      listFile.value = [];
      listFile.value = data;
      countFiles.value = count[0].count;
      countAllFile.value = count2[0].countALLFile;
    })
    .catch((error) => {
      // toast.error("Tải dữ liệu không thành công!");
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const listWeights = ref();
let ColorWeight = ref([
  "#00cdff",
  "#00ffcd",
  "#00ff17",
  "#fff000",
  "#ff00bd",
  "#ff0000",
]);
const countExtend = ref();
const newReport = ref();
const isClose = ref(false);
const loadTaskMain = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_get",
            par: [
              { par: "id", va: props.id },
              { par: "user_id", va: user.user_id },
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
      if (data.length == 0) {
        is_viewSecurityTask.value = false;
        return;
      } else {
        is_viewSecurityTask.value = true;
      }
      let data1 = JSON.parse(response.data.data)[1];
      let data2 = JSON.parse(response.data.data)[2];
      let data3 = JSON.parse(response.data.data)[3];
      if (data2[0].totalExtend > 0 && data[0].status != 3) {
        data[0].status = 9;
      }

      countExtend.value = data2[0].totalExtend;
      newReport.value = data3[0].newReport;
      listDropdownStatus.value.forEach((x) => {
        if (data[0].status == x.value) {
          data[0].statuss = x.text;
          data[0].bgColor = x.bg_color;
          data[0].text_color = x.text_color;
        }
      });
      let status = data[0].status;
      if (
        status == 0 ||
        status == 2 ||
        status == 4 ||
        status == 7 ||
        status == 3
      ) {
        isClose.value = true;
      }
      datalists.value = data[0];
      listWeights.value = data1;
      listDropdownweight.value = [];
      let drd = data1.filter((x) => x.status == true);
      drd.forEach((x) => {
        listDropdownweight.value.push(x);
      });

      let weigth = [];
      data1.forEach((x, i) => {
        let w =
          "<span style='color:" +
          ColorWeight.value[i] +
          "; font-weight: 700;margin-left: 5px;margin-right: 5px;'>" +
          x.weight_name +
          "</span>";
        weigth.push(w);
      });
      datalists.value.weights =
        datalists.value.weight != null ? weigth[datalists.value.weight] : null;
      if (datalists.value.description != null) {
        if (datalists.value.description.includes("\n")) {
          datalists.value.description = datalists.value.description.replace(
            /\n/g,
            "<br/>",
          );
          datalists.value.description =
            datalists.value.description.split("<br/>");
        } else {
          datalists.value.description += "<br/>";
          datalists.value.description =
            datalists.value.description.split("<br/>");
        }
      }
      if (datalists.value.target != null) {
        if (datalists.value.target.includes("\n")) {
          datalists.value.target = datalists.value.target.replace(
            /\n/g,
            "<br/>",
          );
          datalists.value.target = datalists.value.target.split("<br/>");
        } else {
          datalists.value.target += "<br/>";
          datalists.value.target = datalists.value.target.split("<br/>");
        }
      }
      if (datalists.value.difficult != null) {
        if (datalists.value.difficult.includes("\n")) {
          datalists.value.difficult = datalists.value.difficult.replace(
            /\n/g,
            "<br/>",
          );
          datalists.value.difficult = datalists.value.difficult.split("<br/>");
        } else {
          datalists.value.difficult += "<br/>";
          datalists.value.difficult = datalists.value.difficult.split("<br/>");
        }
      }
      if (datalists.value.request != null) {
        if (datalists.value.request.includes("\n")) {
          datalists.value.request = datalists.value.request.replace(
            /\n/g,
            "<br/>",
          );
          datalists.value.request = datalists.value.request.split("<br/>");
        } else {
          datalists.value.request += "<br/>";
          datalists.value.request = datalists.value.request.split("<br/>");
        }
      }

      datalists.value.progress = datalists.value.progress
        ? datalists.value.progress
        : 0;
      var now = new Date();
      var d1 = new Date(now + 1);
      // data.start_real_date != null
      //   ? data.start_real_date
      //   : data.start_date,
      var d2 = new Date(datalists.value.end_date);

      var diff = d2.getTime() - d1.getTime();

      var daydiff = diff / (1000 * 60 * 60 * 24);

      var stdate = new Date(datalists.value.start_date);
      if (stdate > today.value) {
        TimeToDo.value = "Chưa bắt đầu";
        return;
      }
      if (0 < daydiff + 1 && daydiff + 1 < 1) {
        TimeToDo.value =
          "<div class='flex format-center font-bold text-xl' style='background-color: #fffbd8;color: #6DD230'> Đến hạn hoàn thành </div>";
        return;
      }
      let displayTime = Math.abs(Math.floor(daydiff + 1));
      TimeToDo.value =
        daydiff + 1 < 0
          ? "<div class='flex format-center font-bold text-xl' style='background-color: #fffbd8;color: red'> Quá hạn " +
            displayTime +
            " ngày</div>"
          : "<div class='flex format-center font-bold text-xl' style='background-color: #fffbd8;color: #6DD230'> Còn " +
            displayTime +
            " ngày</div>";
    })
    .catch((error) => {
      //toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "DetailedWork.vue",
        logcontent: error.message,
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
const countAllMember = ref();
const memberType = ref();
const memberType1 = ref();
const memberType2 = ref();
const memberType3 = ref();
const loadMember = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_member_get",
            par: [
              { par: "id", va: props.id },
              { par: "type", va: null },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let mem = JSON.parse(response.data.data)[0];
      let countMem = JSON.parse(response.data.data)[1];
      let countAllMem = JSON.parse(response.data.data)[2];
      countAllMember.value = countAllMem[0].countAll;
      countMembers.value = countMem[0].count;
      members.value = mem;
      // member;
      ngv.value = 0;
      nth.value = 0;
      ndth.value = 0;
      ntd.value = 0;
      let sttgv = 0;
      let sttth = 0;
      let sttdth = 0;
      let stttd = 0;

      members.value.forEach((element) => {
        if (store.state.user.user_id == element.user_id) {
          isViewTask(element.member_id);
        }
        element.STTGV = null;
        element.STTTH = null;
        element.STTDTH = null;
        element.STTTD = null;
        element.tooltip =
          (element.is_type == 0
            ? "Người giao việc"
            : element.is_type == 1
            ? "Người xử lý chính"
            : element.is_type == 2
            ? "Người đồng xử lý"
            : element.is_type == 3
            ? "Người theo dõi"
            : "") +
          "<br/>" +
          element.full_name +
          "<br/>" +
          (element.positions ?? "") +
          (element.positions ? "<br/>" : "") +
          (element.department_name != null
            ? element.department_name
            : element.organiztion_name);

        if (element.is_type == 0) {
          element.STTGV = sttgv;
          sttgv++;
        }
        if (element.is_type == 1) {
          element.STTTH = sttth;
          sttth++;
        }
        if (element.is_type == 2) {
          element.STTDTH = sttdth;
          sttdth++;
        }
        if (element.is_type == 3) {
          element.STTTD = stttd;
          stttd++;
        }

        ngv.value += element.is_type == 0 ? 1 : 0;
        nth.value += element.is_type == 1 ? 1 : 0;
        ndth.value += element.is_type == 2 ? 1 : 0;
        ntd.value += element.is_type == 3 ? 1 : 0;
        if (store.state.user.user_id == element.user_id) {
          if (memberType.value == null) {
            memberType.value = element.is_type;
            return;
          }
          if (memberType1.value == null) {
            memberType1.value = element.is_type;
            return;
          }
          if (memberType2.value == null) {
            memberType2.value = element.is_type;
            return;
          }
          if (memberType3.value == null) {
            memberType3.value = element.is_type;
            return;
          }
        }
      });
      var byDate = members.value.slice(0);
      byDate.sort(function (a, b) {
        return a.is_type - b.is_type;
      });
      members.value = byDate;
    })
    .catch((error) => {
      // toast.error("Tải dữ liệu không thành công2!");
      options.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "DetailedWork.vue",
        logcontent: error.message,
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
const listComments = ref();
const loadComments = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_Comments_get",
            par: [{ par: "id", va: props.id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let commentJson = JSON.parse(response.data.data)[0];
      let listCheckListJson = JSON.parse(response.data.data)[1];
      countComments.value = listCheckListJson[0].count;
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
      options.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "DetailedWork.vue",
        logcontent: error.message,
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
const loadTaskCheckList = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_checklist",
            par: [
              { par: "id", va: props.id },
              { par: "type", va: 0 },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let listCheckListJson = JSON.parse(response.data.data)[0];
      let count = JSON.parse(response.data.data)[1];
      countChecklists.value = count[0].count;
      let count1 = JSON.parse(response.data.data)[2];
      let count2 = JSON.parse(response.data.data)[3];
      let count3 = JSON.parse(response.data.data)[4];
      countChecked.value = count1[0].countChecked ? count1[0].countChecked : 0;
      countUnChecked.value = count2[0].countUnChecked
        ? count2[0].countUnChecked
        : 0;
      ExpireTimeTask.value = count3[0].countExTime ? count3[0].countExTime : 0;
      let listChecklist = JSON.parse(response.data.data)[5];
      RenderData(listChecklist, listCheckListJson);
    })
    .catch((error) => {
      // toast.error("Tải dữ liệu không thành công3!");
      options.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "DetailedWork.vue",
        logcontent: error.message,
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
const ListChildTask = ref();
const AllChild = ref();
const NotStartedChild = ref();
const DoingChild = ref();
const FinishedChild = ref();
const loadChildTaskOrigin = (type) => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "Task_Origin_children_list",
            par: [
              { par: "id", va: props.id },
              { par: "type", va: type },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let listChild = JSON.parse(response.data.data)[0];
      let listUser = JSON.parse(response.data.data)[1];
      let count1 = JSON.parse(response.data.data)[2];
      let count2 = JSON.parse(response.data.data)[3];
      let count3 = JSON.parse(response.data.data)[4];
      let count4 = JSON.parse(response.data.data)[5];
      AllChild.value = count1[0].totalChild;
      NotStartedChild.value = count2[0].NotStartedYet;
      FinishedChild.value = count3[0].Finished;
      DoingChild.value = count4[0].Doing;
      listChild.forEach((c) => {
        c.users = [];
        let sttus = listDropdownStatus.value.filter((a) => a.value == c.status);
        c.status_display = {
          text: sttus[0].text,
          bg_color: sttus[0].bg_color,
          text_color: sttus[0].text_color,
        };
        let sttgv = 0;
        let sttth = 0;
        let sttdth = 0;
        let stttd = 0;
        listUser.forEach((u) => {
          if (c.task_id == u.task_id) {
            c.progress = c.progress != null ? c.progress : 0;
            u.tooltip =
              (u.is_type == 0
                ? "Người giao việc"
                : u.is_type == 1
                ? "Người xử lý chính"
                : u.is_type == 2
                ? "Người đồng xử lý"
                : u.is_type == 3
                ? "Người theo dõi"
                : "") +
              "<br/>" +
              u.full_name +
              "<br/>" +
              (u.positions ?? "") +
              "<br/>" +
              (u.department_name != null
                ? u.department_name
                : u.organiztion_name);
            c.users.push(u);
          }
        });
        c.users.forEach((u) => {
          if (u.is_type == 0) {
            u.STTGV = sttgv;
            sttgv++;
          }
          if (u.is_type == 1) {
            u.STTTH = sttth;
            sttth++;
          }
          if (u.is_type == 2) {
            u.STTDTH = sttdth;
            sttdth++;
          }
          if (u.is_type == 3) {
            u.STTTD = stttd;
            stttd++;
          }
        });
        var byDate = c.users.slice(0);
        byDate.sort(function (a, b) {
          return a.is_type - b.is_type;
        });
        c.users = byDate;
      });

      ListChildTask.value = [];
      ListChildTask.value = JSON.parse(JSON.stringify(listChild));
    })
    .catch((error) => {
      // toast.error("Tải dữ liệu không thành công4!");
      options.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "DetailedWork.vue",
        logcontent: error.message,
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

const loadData = (rf) => {
  if (rf) {
    loadTaskMain();

    loadTaskCheckList();
    loadMember();
    loadComments();
    loadChildTaskOrigin(0);
    loadFile();
    loadTaskDoc();
  }
};
const RenderComments = (data) => {
  let arrChils = [];
  data.forEach((cha) => {
    data.forEach((con) => {
      if (con.parent_id == cha.comment_id) {
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
const RenderData = (cgr, c) => {
  listTaskCheckList.value = c;
  cgr.forEach((lcl) => {
    let i = 1;
    lcl.task = [];
    c.forEach((ltlc) => {
      ltlc.task_names = ltlc.task_name;
      ltlc.task_names = ltlc.task_names.replace(/\n/g, "<br/>");
      ltlc.actor_tooltip =
        "Người thực hiện <br/>" +
        ltlc.actor_full_name +
        "<br/>" +
        ltlc.actor_positions +
        "<br/>" +
        (ltlc.actor_department_name != null
          ? ltlc.actor_department_name
          : ltlc.actor_organiztion_name);
      ltlc.creator_tooltip =
        "Người tạo <br/>" +
        ltlc.creator_full_name +
        "<br/>" +
        ltlc.creator_positions +
        "<br/>" +
        (ltlc.creator_department_name != null
          ? ltlc.creator_department_name
          : ltlc.creator_organiztion_name);
      ltlc.workTime = Math.abs(
        (
          ((ltlc.close_date ? new Date(ltlc.close_date) : new Date()) -
            new Date(ltlc.start_date)) /
          (1000 * 24 * 60 * 60)
        ).toFixed(0),
      );
      if (ltlc.checklist_id == lcl.checklist_id) {
        ltlc.STT = i;
        i++;
        lcl.task.push(ltlc);
      }
    });
  });
  listCheckList.value = [];
  listCheckList.value = cgr;
};
//Khác
//Bình luận
const panelEmoij1 = ref();
let filecoments = [];
const listFileComment = ref([]);
const comment = ref("");
const comment_zone_main = ref();
let line1 = "";
let line = "";
const showEmoji = (event, check) => {
  if (check == 1) panelEmoij1.value.toggle(event);
};
const Change = (event) => {
  line = event.range.index ? event.range.index : null;
};
const handleEmojiClick = (event) => {
  if (ThongTinChung.value == true) {
    comment.value = comment.value.replace("<p>", "").replace("</p>", "");
    line1 = line ? line : comment.value.length;
    let str1 = comment.value.slice(0, line1);
    let str2 = comment.value.slice(line1);
    if (comment.value)
      comment.value =
        line1 > 0 ? str1 + event.unicode + str2 : comment.value + event.unicode;
    else comment.value = comment.value + event.unicode;
    comment.value = comment.value.replace("<p>", "").replace("</p>", "");
    comment_zone_main.value.setHTML("<p>" + comment.value + "</p>");
    line1 += 1;
  }
};

const chonanh = (id) => {
  document.getElementById(id).value = "";
  document.getElementById(id).click();
};

const delImgComment = (value, index) => {
  if (editComment.value == true && value.data) {
    listIdFileEditComments_Del.value.push(value.data.file_id);
  }
  listSendFile.value.splice(index, 1);
  listFileComment.value = listFileComment.value.filter((x) => x.data != value);
  listFileComment.value = listFileComment.value.filter((x) => x != value);
};
//----------------------------------Nhóm Checklist--------------------------------------------------------
const checkList = ref({
  checklist_id: null,
  project_id: null,
  task_id: null,
  checklist_name: null,
  description: null,
  status: null,
  is_order: null,
});
const rules = {
  checklist_name: {
    required,
  },
};
const openDialog = ref(false);
const submitted = ref(false);
const edit = ref(false);
const headerDialog = ref();
const v$ = useVuelidate(rules, checkList);
const addCheckList = () => {
  textboxLength.value = 0;
  textboxLength2.value = 0;
  checkList.value = {
    checklist_id: null,
    project_id: null,
    task_id: datalists.value.task_id,
    checklist_name: null,
    description: null,
    status: true,
    is_order: listCheckList.value.length + 1,
  };
  headerDialog.value = "Thêm mới Checklist";
  openDialog.value = true;
  submitted.value = false;
};
const editCheckList = (c) => {
  textboxLength.value = 0;
  textboxLength2.value = 0;
  checkList.value = c;
  headerDialog.value = "Sửa Checklist";
  openDialog.value = true;
  submitted.value = false;
  edit.value = true;
};
const deleteCheckList = (c) => {
  let id = [];
  id.push(c);

  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá Checklist này không!",
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
          .delete(baseURL + "/api/checkList/deleteChecklist", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: id,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá Checklist thành công!");
              loadTaskCheckList();
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
const textboxLength = ref();
const focusInput = () => {
  textboxLength.value = 0;
  const textbox = document.getElementById("textbox");
  textboxLength.value = textbox.value.length;
};
const textboxLength2 = ref();
const focusInput2 = () => {
  textboxLength2.value = 0;
  const textbox = document.getElementById("descript");
  textboxLength2.value = textbox.value.length;
};
const saveData = (isFormValid) => {
  if (textboxLength.value > 500 || textboxLength2.value > 500) {
    return;
  }
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: edit.value ? "put" : "post",
    url:
      baseURL +
      `/api/checkList/${edit.value ? "updateChecklist" : "addCheckList"}`,
    data: checkList.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        loadTaskCheckList();
        toast.success(
          response.config.method == "put"
            ? "Sửa Checklist thành công!"
            : "Thêm Checklist thành công",
        );
        swal.close();
        edit.value = false;
        submitted.value = false;
        openDialog.value = false;
        closeDialog();
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html:
            ms.includes("group_name") == true
              ? "Tên nhóm công việc không quá 250 ký tự!"
              : ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};

//----------------------------------Công việc Checklist--------------------------------------------------
const Task = ref({
  STT: null,
  task_code: null,
  parent_id: null,
  project_id: null,
  checklist_id: null,
  department_id: null,
  group_id: null,
  task_name: null,
  task_name_en: null,
  description: null,
  keywords: null,
  start_date: null,
  end_date: null,
  start_real_date: null,
  end_real_date: null,
  weight: null,
  difficult: null,
  target: null,
  result: null,
  request: null,
  is_prioritize: null,
  is_deadline: null,
  is_review: null,
  progress: null,
  is_todo: null,
  is_public: null,
  is_security: null,
  close_by: null,
  close_date: null,
  status: null,
  is_order: null,
  organization_id: null,
  assign_user_id: "",
  work_user_ids: [],
  works_user_ids: [],
  follow_user_ids: [],
});
const rulesTaskCheckList = {
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
};
const editTaskCheckList = ref(false);
const submittedTaskCheckList = ref(false);
const v1$ = useVuelidate(rulesTaskCheckList, Task);
const editTaskCheckListText = ref(false);
const addNewTaskCheckList = ref(false);

const addTaskCheckList = (event) => {
  submittedTaskCheckList.value = false;
  addNewTaskCheckList.value = true;
  editTaskCheckList.value == false;
  Task.value = {
    STT: event.totalRecords + 1,
    checklist_id: event.checklist_id,
    parent_id: datalists.value.task_id,
    is_todo: true,
    is_check: false,
    is_order: event.totalRecords + 1,
  };
};

const editTaskCheckListFunc = (t) => {
  submittedTaskCheckList.value = false;
  editTaskCheckListText.value = true;
  Task.value = t;
};
const deteleTaskCheckList = (t) => {
  let id = [];
  id.push(t.task_id);

  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá công việc Checklist này không!",
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
          .delete(baseURL + "/api/ChecklistDetail/deleteTaskChecklist", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: id,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá công việc checklist thành công!");
              loadTaskCheckList();
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
//Chung Dialog
const closeDialog = () => {
  openDialog.value = false;
  openDialog1.value = false;
  submitted.value = false;
  edit.value = false;
  addNewTaskCheckList.value = false;
  editTaskCheckListText.value = false;
  editTaskCheckList.value = false;
  submittedTaskCheckList.value = false;
  editTaskCheckList.value = false;
  Task.value.task_id = null;
  loadData(true);
};

const saveTaskCheckList = (isFormValid) => {
  submittedTaskCheckList.value = true;
  Task.value.parent_id = datalists.value.task_id;
  if (Task.value.end_real_date != null) Task.value.is_check = true;
  if (!isFormValid) {
    return;
  }
  if (Task.value.task_name.lastIndexOf("\n") == Task.value.task_name.length) {
    Task.value.task_name = Task.value.task_name.substring(
      0,
      Task.value.task_name.lastIndexOf("\n"),
    );
  }
  if (Task.value.end_date != null) {
    Task.value.is_deadline = true;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method:
      editTaskCheckList.value || editTaskCheckListText.value ? "put" : "post",
    url:
      baseURL +
      `/api/ChecklistDetail/${
        editTaskCheckList.value || editTaskCheckListText.value
          ? "UpdateTaskChecklists"
          : "addTaskCheckList"
      }`,
    data: Task.value,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        closeDialog();
        toast.success(
          response.config.method == "put"
            ? "Cập nhật công việc thành công!"
            : "Thêm mới công việc thành công!",
        );
        loadTaskCheckList();
        editTaskCheckList.value = false;
        editTaskCheckListText.value = false;
        addNewTaskCheckList.value = false;
        submittedTaskCheckList.value = false;
        openDialog1.value = true;
        closeDialog();
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html:
            ms.includes("group_name") == true
              ? "Tên nhóm công việc không quá 250 ký tự!"
              : ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const onCheckboxTask = (t) => {
  if (editTaskCheckListText.value) {
    return;
  }
  let data = {
    TextID: t.task_id,
  };

  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .put(baseURL + "/api/CheckListDetail/Update_StatusTaskChecklist", data, {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái công việc thành công!");
        loadTaskCheckList();
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html:
            ms.includes("group_name") == true
              ? "Tên nhóm công việc không quá 250 ký tự!"
              : ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
//Button Hover
const copyToClipboard = (event) => {
  navigator.clipboard.writeText(event);
  toast.success("Đã sao chép vào bộ nhớ tạm");
};
const maxDate = ref();
const minDate = ref();
const openDialog1 = ref(false);
const headerDialog1 = ref();
const editTime = (t) => {
  Task.value = null;
  minDate.value = datalists.value.start_date;
  maxDate.value = datalists.value.end_date;
  editTaskCheckList.value = true;
  headerDialog1.value = "Cập nhật ngày checklist";
  Task.value = t;
  Task.value.start_date = new Date(Task.value.start_date);
  Task.value.end_date = Task.value.end_date
    ? new Date(Task.value.end_date)
    : null;
  Task.value.close_date = Task.value.close_date
    ? new Date(Task.value.close_date)
    : null;
  openDialog1.value = true;
};
const cancel = (str) => {
  if (str == "add") {
    addNewTaskCheckList.value = false;
  } else {
    editTaskCheckListText.value = false;
    Task.value = {};
    loadTaskCheckList();
  }
};
const selectcapcha = ref({});
//ChildTask
const ruleChildTask = {
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
  assign_user_id: {
    required,
    $errors: [
      {
        $property: "assign_user_id",
        $validator: "required",
        $message: "Người giao việc không được để trống!",
      },
    ],
  },
  work_user_ids: {
    required,
    $errors: [
      {
        $property: "work_user_ids",
        $validator: "required",
        $message: "Người thực hiện không được để trống!",
      },
    ],
  },
  end_date: {
    required,
    $errors: [
      {
        $property: "end_date",
        $validator: "required",
        $message: "Ngày kết thúc không được để trống!",
      },
    ],
  },
};
let files = [];
const v3$ = useVuelidate(ruleChildTask, Task);
const headerAddTask = ref();
const displayTask = ref(false);
const addNewChildTaskOrigin = (task) => {
  sbm.value = false;
  headerAddTask.value = "Thêm công việc con";
  Task.value = {
    project_id: task.project_id,
    task_name: "",
    assign_user_id: [],
    works_user_ids: [],
    work_user_ids: [],
    follow_user_ids: [],
    is_prioritize: false,
    is_deadline: true,
    is_review: true,
    is_security: false,
    weight: 0,
    start_date: new Date(),
    end_date: null,
    description: null,
    status: 0,
    target: null,
    request: null,
    organization_id: store.state.user.organization_id,
    files: [],
  };
  Task.value.parent_id = task.task_id;
  if (task.department_id && task.department_id != -1) {
    Task.value.is_department = true;
  }
  selectcapcha.value[task.department_id || -1] = true;
  listtreeOrganization();
  listProjectMain();
  Task.value.assign_user_id.push(store.state.user.user_id);
  Task.value.work_user_ids.push(store.state.user.user_id);
  isAdd.value = true;
  displayTask.value = true;
};
//loadDropdown
const listDropdownUser = ref([]);
const listDropdownProject = ref([]);
const listDropdownTaskGroup = ref([]);
const listDropdownStatus = ref([
  {
    value: 0,
    text: "Chưa bắt đầu",
    bg_color: "#bbbbbb",
    text_color: "#FFFFFF",
  },
  { value: 1, text: "Đang làm", bg_color: "#2196f3", text_color: "#FFFFFF" },
  { value: 2, text: "Tạm ngừng", bg_color: "#d87777", text_color: "#FFFFFF" },
  { value: 3, text: "Đã đóng", bg_color: "#d87777", text_color: "#FFFFFF" },
  { value: 4, text: "HT đúng hạn", bg_color: "#04D215", text_color: "#FFFFFF" },
  {
    value: 5,
    text: "Chờ đánh giá",
    bg_color: "#33c9dc",
    text_color: "#FFFFFF",
  },
  { value: 6, text: "Bị trả lại", bg_color: "#ffa500", text_color: "#FFFFFF" },
  { value: 7, text: "HT sau hạn", bg_color: "#ff8b4e", text_color: "#FFFFFF" },
  { value: 8, text: "Đã đánh giá", bg_color: "#51b7ae", text_color: "#FFFFFF" },
  { value: 9, text: "Xin gia hạn", bg_color: "#F18636", text_color: "#FFFFFF" },
  { value: -1, text: "Bị xóa", bg_color: "red", text_color: "#FFFFFF" },
]);
const listDropdownweight = ref();
const listDropdownUserAssign = ref();
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "listDropdownUserAssign":
      listDropdownUserAssign.value = obj.data;
      break;
  }
});
emitter.on("addMember", (obj) => {
  if (obj == true) {
    loadMember();
  }
});
emitter.on("closeDoc", (obj) => {
  if (obj == false) {
    openDocDialog.value = false;
    loadTaskDoc();
  }
});
emitter.on("delMember", (obj) => {
  if (obj == true) {
    loadMember();
  }
});
const listUser = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "getUserByOrganization",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "search", va: null },
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
      data.forEach((element, i) => {
        // if (i < 5) listUserShow.value.push({ data: element, active: false });
        listDropdownUser.value.push({
          name: element.full_name,
          code: element.user_id,
          avatar: element.avatar,
        });
      });
    })
    .catch((error) => {
      // toast.error("Tải dữ liệu không thành công6!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};

const closeDialogTask = () => {
  displayTask.value = false;
  loadTaskMain();
};

const listProjectMain = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_get_list_init",
            par: [
              {
                par: "user_id",
                va: store.getters.user.user_id,
              },
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
      listDropdownProject.value = data[0];
      listDropdownTaskGroup.value = data[1];
      listDropdownweight.value = data[2];
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const onUploadFile = (event) => {
  files = [];
  event.files.forEach((element) => {
    files.push(element);
  });
};
const removeFile = (event) => {
  files = files.filter((a) => a != event.file);
};

const TaskMembers = ref([]);
const sbm = ref(false);
const issaveTask = ref(false);
const isAdd = ref(false);
const ChangeTaskDepartment = () => {
  let id = parseInt(Object.keys(selectcapcha.value)[0]);
  Task.value.assign_user_id = [];
  Task.value.work_user_ids = [];
  if (id != -1) {
    listOrganization.value
      .filter((x) => x.organization_id == id)
      .forEach((t) => {
        if (t.user_id) {
          Task.value.assign_user_id.push(store.state.user.user_id);
          Task.value.work_user_ids.push(t.user_id);
        } else {
          selectcapcha.value = [];
          selectcapcha.value[-1] = true;
          Task.value.assign_user_id.push(store.state.user.user_id);
          Task.value.work_user_ids.push(store.state.user.user_id);
          swal.fire({
            title: "Thông báo!",
            text: "Phòng ban này chưa tồn tại người chủ trì!",
            icon: "info",
            confirmButtonText: "OK",
          });
        }
      });
  }
};
const ChangeIsDepartment = (model) => {
  selectcapcha.value = [];
  Task.value.assign_user_id = [];
  Task.value.work_user_ids = [];
  selectcapcha.value[-1] = true;
  if (store.state.user.is_super) {
    Task.value.organization_id = 0;
  } else {
    Task.value.organization_id = store.state.user.organization_id;
  }
  Task.value.assign_user_id.push(store.state.user.user_id);
  Task.value.work_user_ids.push(store.state.user.user_id);
};
const saveTask = (isFormValid) => {
  sbm.value = true;
  if (Task.value.is_deadline) {
    if (!isFormValid) {
      return;
    }
  }
  if (!selectcapcha.value) {
    selectcapcha.value = [];
  }
  TaskMembers.value = [];
  Task.value.department_id = parseInt(Object.keys(selectcapcha.value)[0]);
  let formData = new FormData();
  for (var i = 0; i < files.length; i++) {
    let file = files[i];
    formData.append("url", file);
  }
  if (Task.value.assign_user_id.length > 0) {
    Task.value.assign_user_id.forEach((t) => {
      let member = {
        project_id: null,
        task_id: null,
        user_id: t,
        is_type: 0,
        status: true,
      };
      member.user_id = t;
      TaskMembers.value.push(member);
    });
  }
  if (Task.value.work_user_ids.length > 0) {
    Task.value.work_user_ids.forEach((t) => {
      let member1 = {
        project_id: null,
        task_id: null,
        user_id: t,
        is_type: 1,
        status: true,
      };
      member1.user_id = t;
      TaskMembers.value.push(member1);
    });
  }
  if (Task.value.works_user_ids.length > 0) {
    Task.value.works_user_ids.forEach((t) => {
      let member2 = {
        project_id: null,
        task_id: null,
        user_id: t,
        is_type: 2,
        status: true,
      };
      TaskMembers.value.push(member2);
    });
  }
  if (Task.value.follow_user_ids.length > 0) {
    Task.value.follow_user_ids.forEach((t) => {
      let member3 = {
        project_id: null,
        task_id: null,
        user_id: t,
        is_type: 3,
        status: true,
      };
      TaskMembers.value.push(member3);
    });
  }
  if (isAdd.value == false) {
    if (Task.value.created_date) {
      Task.value.created_date = new Date(Task.value.created_date);
    }
    if (Task.value.update_date) {
      Task.value.update_date = new Date(Task.value.update_date);
    }
    if (Task.value.start_date) {
      Task.value.start_date = new Date(Task.value.start_date);
    }
    if (Task.value.end_date) {
      Task.value.end_date = new Date(Task.value.end_date);
    }
  }
  formData.append("isXML", JSON.stringify(Task.value.is_XML));
  formData.append("taskmember", JSON.stringify(TaskMembers.value));
  formData.append("taskOrigin", JSON.stringify(Task.value));
  if (!issaveTask.value) {
    axios
      .post(
        baseURL +
          "/api/task_origin/" +
          (isAdd.value == true ? "Add_TaskOrigin" : "Update_TaskOrigin"),
        formData,
        config,
      )
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success(
            isAdd.value == true
              ? "Thêm công việc thành công!"
              : "Chỉnh sửa công việc thành công!",
          );
          displayTask.value = false;
          // loadData(true);
          loadChildTaskOrigin(0);
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
}; //recall component
const showDetail1 = ref(false);
const selectedTaskID = ref();
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
const show = (ch) => {
  forceRerender();
  selectedTaskID.value = null;
  showDetail1.value = true;
  selectedTaskID.value = ch.task_id;
};
emitter.on("SideBar1", (obj) => {
  showDetail1.value = obj;
  showDetail1.value = false;
  selectedTaskID.value = null;
});
const close = () => {
  emitter.emit("SideBar1", false);
  showDetail1.value = false;
};

//----Xóa Task----
const DelTask = (task) => {
  displayTask.value = false;

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
        var listId = [];
        listId.push(task.task_id);
        axios
          .delete(baseURL + "/api/task_origin/Delete_task_origin", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá công việc thành công!");
              isShow.value = false;
              hideall();
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
//Edit Task
const EditTask = (task) => {
  listProjectMain();
  listtreeOrganization();
  // listUser();
  Task.value = {
    STT: null,
    task_code: null,
    parent_id: null,
    project_id: null,
    checklist_id: null,
    department_id: null,
    group_id: null,
    task_name: null,
    task_name_en: null,
    description: null,
    keywords: null,
    start_date: null,
    end_date: null,
    start_real_date: null,
    end_real_date: null,
    weight: null,
    difficult: null,
    target: null,
    result: null,
    request: null,
    is_prioritize: null,
    is_deadline: null,
    is_review: null,
    progress: null,
    is_todo: null,
    is_public: null,
    is_security: null,
    close_by: null,
    close_date: null,
    status: null,
    is_order: null,
    organization_id: null,
    assign_user_id: [],
    work_user_ids: [],
    works_user_ids: [],
    follow_user_ids: [],
  };
  selectcapcha.value = [];
  //Task.value = task;
  // members.value.forEach((x) => {
  //   if (x.is_type == 0) {
  //     Task.value.assign_user_id.push(x.user_id);
  //   }
  //   if (x.is_type == 1) {
  //     Task.value.work_user_ids.push(x.user_id);
  //   }
  //   if (x.is_type == 2) {
  //     Task.value.works_user_ids.push(x.user_id);
  //   }
  //   if (x.is_type == 3) {
  //     Task.value.follow_user_ids.push(x.user_id);
  //   }
  // });

  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_get_edit",
            par: [{ par: "task_id", va: props.id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      debugger;
      if (data.length > 0) {
        data[0].forEach((element, i) => {
          element.Thanhviens = element.Thanhviens
            ? JSON.parse(element.Thanhviens)
            : [];
          element.files = element.files ? JSON.parse(element.files) : [];
          element.files = element.files ? element.files : [];
        });
      }
      Task.value = data[0][0];
      Task.value.is_XML = false;
      selectcapcha.value[Task.value.department_id || -1] = true;
      Task.value.is_department =
        Task.value.department_id && Task.value.department_id != -1
          ? true
          : false;
      Task.value.start_date = Task.value.start_date
        ? new Date(Task.value.start_date)
        : null;
      Task.value.end_date = Task.value.end_date
        ? new Date(Task.value.end_date)
        : null;
      Task.value.assign_user_id = [];
      Task.value.work_user_ids = [];
      Task.value.works_user_ids = [];
      Task.value.follow_user_ids = [];
      if (Task.value.Thanhviens.length > 0) {
        Task.value.Thanhviens.forEach((t) => {
          if (t.is_type == "0") {
            Task.value.assign_user_id.push(t.user_id);
          } else if (t.is_type == "1") {
            Task.value.work_user_ids.push(t.user_id);
          } else if (t.is_type == "2") {
            Task.value.works_user_ids.push(t.user_id);
          } else if (t.is_type == "3") {
            Task.value.follow_user_ids.push(t.user_id);
          }
        });
      }
      Task.value.is_order = Task.value.STT;
      headerAddTask.value = "Sửa công việc";
      issaveTask.value = false;
      displayTask.value = true;
      Task.value.organization_id = store.state.user.organization_id;
    })
    .catch((error) => {
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
  displayTask.value = true;
  headerAddTask.value = "Chỉnh sửa công việc";
};
///Hide and seek right side
const ThongTinChung = ref(true);
const DanhGiaCongViec = ref(false);
const GiaHanXuLy = ref(false);
const CongViecCon = ref(false);
const QuanLyThanhVien = ref(false);
const QuanLyTaiLieu = ref(false);
const HoatDong = ref(false);
const NguoiDaXem = ref(false);
const PhanQuyen = ref(false);
const ThongKe = ref(false);
const QuyTrinh = ref(false);
const Switch = (e) => {
  switch (e) {
    case "1":
      ThongTinChung.value = true;
      DanhGiaCongViec.value = false;
      GiaHanXuLy.value = false;
      CongViecCon.value = false;
      QuanLyThanhVien.value = false;
      QuanLyTaiLieu.value = false;
      HoatDong.value = false;
      NguoiDaXem.value = false;
      PhanQuyen.value = false;
      ThongKe.value = false;
      loadData(true);
      displayTask.value = false;
      QuyTrinh.value = false;
      break;
    case "2":
      ThongTinChung.value = false;
      DanhGiaCongViec.value = true;
      GiaHanXuLy.value = false;
      CongViecCon.value = false;
      QuanLyThanhVien.value = false;
      QuanLyTaiLieu.value = false;
      HoatDong.value = false;
      NguoiDaXem.value = false;
      PhanQuyen.value = false;
      displayTask.value = false;
      ThongKe.value = false;
      QuyTrinh.value = false;
      break;
    case "3":
      ThongTinChung.value = false;
      DanhGiaCongViec.value = false;
      GiaHanXuLy.value = true;
      CongViecCon.value = false;
      QuanLyThanhVien.value = false;
      QuanLyTaiLieu.value = false;
      HoatDong.value = false;
      NguoiDaXem.value = false;
      displayTask.value = false;
      PhanQuyen.value = false;
      ThongKe.value = false;
      QuyTrinh.value = false;

      break;
    case "4":
      ThongTinChung.value = false;
      DanhGiaCongViec.value = false;
      GiaHanXuLy.value = false;
      CongViecCon.value = true;
      QuanLyThanhVien.value = false;
      QuanLyTaiLieu.value = false;
      HoatDong.value = false;
      NguoiDaXem.value = false;
      displayTask.value = false;
      PhanQuyen.value = false;
      ThongKe.value = false;
      QuyTrinh.value = false;

      break;
    case "5":
      ThongTinChung.value = false;
      DanhGiaCongViec.value = false;
      GiaHanXuLy.value = false;
      CongViecCon.value = false;
      QuanLyThanhVien.value = true;
      QuanLyTaiLieu.value = false;
      HoatDong.value = false;
      NguoiDaXem.value = false;
      PhanQuyen.value = false;
      displayTask.value = false;
      ThongKe.value = false;
      QuyTrinh.value = false;

      break;
    case "6":
      ThongTinChung.value = false;
      DanhGiaCongViec.value = false;
      GiaHanXuLy.value = false;
      CongViecCon.value = false;
      QuanLyThanhVien.value = false;
      QuanLyTaiLieu.value = true;
      HoatDong.value = false;
      NguoiDaXem.value = false;
      displayTask.value = false;
      PhanQuyen.value = false;
      ThongKe.value = false;
      QuyTrinh.value = false;

      break;
    case "7":
      ThongTinChung.value = false;
      DanhGiaCongViec.value = false;
      GiaHanXuLy.value = false;
      CongViecCon.value = false;
      QuanLyThanhVien.value = false;
      QuanLyTaiLieu.value = false;
      HoatDong.value = true;
      NguoiDaXem.value = false;
      PhanQuyen.value = false;
      displayTask.value = false;
      ThongKe.value = false;
      QuyTrinh.value = false;

      break;
    case "8":
      ThongTinChung.value = false;
      DanhGiaCongViec.value = false;
      GiaHanXuLy.value = false;
      CongViecCon.value = false;
      QuanLyThanhVien.value = false;
      QuanLyTaiLieu.value = false;
      HoatDong.value = false;
      NguoiDaXem.value = true;
      PhanQuyen.value = false;
      displayTask.value = false;
      ThongKe.value = false;
      QuyTrinh.value = false;

      break;
    case "9":
      ThongTinChung.value = false;
      DanhGiaCongViec.value = false;
      GiaHanXuLy.value = false;
      CongViecCon.value = false;
      QuanLyThanhVien.value = false;
      QuanLyTaiLieu.value = false;
      HoatDong.value = false;
      NguoiDaXem.value = false;
      PhanQuyen.value = true;
      displayTask.value = false;
      ThongKe.value = false;
      QuyTrinh.value = false;
      break;
    case "10":
      ThongTinChung.value = false;
      DanhGiaCongViec.value = false;
      GiaHanXuLy.value = false;
      CongViecCon.value = false;
      QuanLyThanhVien.value = false;
      QuanLyTaiLieu.value = false;
      HoatDong.value = false;
      NguoiDaXem.value = false;
      PhanQuyen.value = false;
      displayTask.value = false;
      ThongKe.value = true;
      QuyTrinh.value = false;
      break;
    case "11":
      ThongTinChung.value = false;
      DanhGiaCongViec.value = false;
      GiaHanXuLy.value = false;
      CongViecCon.value = false;
      QuanLyThanhVien.value = false;
      QuanLyTaiLieu.value = false;
      HoatDong.value = false;
      NguoiDaXem.value = false;
      PhanQuyen.value = false;
      displayTask.value = false;
      ThongKe.value = false;
      QuyTrinh.value = true;
      forceRerender();
      loadChildTaskOrigin(0);
      break;
  }
};
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
///Ẩn side bar
const interval = ref(null);
const startProgress = () => {
  interval.value = setInterval(() => {
    let newValue =
      datalists.value.progress + Math.floor(Math.random() * 10) + 1;
    if (newValue >= datalists.value.progress) {
      newValue = datalists.value.progress;
    }
  }, 600);
};
const endProgress = () => {
  clearInterval(interval.value);
  interval.value = null;
};

const hideall = () => {
  isShow.value = false;
  emitter.emit("SideBar", false);
};
const PositionSideBar = ref("right");
const MaxMin = (m) => {
  PositionSideBar.value = m;
  emitter.emit("psb", m);
};
const isViewTask = (e) => {
  let id = [];
  id.push(e);
  axios
    .put(baseURL + "/api/task_Member/Update_Task_Member", id, config)
    .then((response) => {
      if (response.data.err != "1") {
        console.log("");
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html: ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const changeNguoiGiaoViec = (event) => {
  Task.value.assign_user_id = [];
  Task.value.assign_user_id.push(event.value[1]);
};
const deleteFile = (datafile) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá file này không!",
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
          .delete(baseURL + "/api/task_origin/Delete_file", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: [datafile.file_id],
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              //  toast.success("Xoá file thành công!");
              var idx = Task.value.files.findIndex(
                (x) => x.file_id == datafile.file_id,
              );
              if (idx != -1) {
                Task.value.files.splice(idx, 1);
              }
              emitter.emit("deleteFile", true);
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
};
const headerAddLinkTask = ref();
const displayLinkTask = ref(false);
const listTaskLink = ref();
const optionsLinkTask = ref({
  search: "",
  PageNo: 0,
  PageSize: 20,
  loading: true,
  totalRecords: null,
});
const addLinkTaskOrigin = (tasklink) => {
  headerAddLinkTask.value = "Liên kết công việc con";
  displayLinkTask.value = true;
  LoadLinkTaskOrigin(tasklink);
};
const LoadLinkTaskOrigin = (tasklink) => {
  optionsLinkTask.value.loading = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_origin_get_list_addlink",
            par: [
              { par: "user_id", va: tasklink.created_by },
              { par: "task_id", va: tasklink.task_id },
              { par: "pageno", va: optionsLinkTask.value.PageNo },
              { par: "pagesize", va: optionsLinkTask.value.PageSize },
              { par: "search", va: optionsLinkTask.value.search },
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
      data[0].forEach((element, i) => {
        element.progress = element.progress == null ? 0 : element.progress;
        element.update_date = element.modified_date
          ? element.modified_date
          : element.created_date;
        element.status_name = listDropdownStatus.value.filter(
          (x) => x.value == element.status,
        )[0].text;
        element.status_bg_color = listDropdownStatus.value.filter(
          (x) => x.value == element.status,
        )[0].bg_color;
        element.status_text_color = listDropdownStatus.value.filter(
          (x) => x.value == element.status,
        )[0].text_color;
        element.is_check = false;
        //thời gian xử lý
        if (element.end_date != null) {
          if (element.thoigianquahan < 0) {
            if (element.thoigianxuly > 0) {
              element.title_time = element.thoigianxuly + " ngày";
              element.time_bg = element.status_bg_color;
              element.time_color = "color: #fff;";
            }
          } else {
            if (element.thoigianquahan > 0) {
              element.title_time =
                "Quá hạn " + element.thoigianquahan + " ngày";
              element.time_bg = "red;";
              element.time_color = "color: #fff;";
            }
          }
        } else if (element.thoigianxuly) {
          element.title_time = element.thoigianxuly + " ngày";
          element.time_bg = element.status_bg_color;
          element.time_color = "color: #fff;";
        }
        element.Thanhviens = element.Thanhviens
          ? JSON.parse(element.Thanhviens)
          : [];
        element.ThanhvienShows = [];
        if (element.Thanhviens.length > 3) {
          element.ThanhvienShows = element.Thanhviens.slice(0, 3);
        } else {
          element.ThanhvienShows = [...element.Thanhviens];
        }
        element.files = element.files ? JSON.parse(element.files) : [];
        element.files = element.files ? element.files : [];
        element.STT = options.value.PageNo * options.value.PageSize + i + 1;
      });
      listTaskLink.value = data[0];
      optionsLinkTask.value.totalRecords = data[1][0].total;
      optionsLinkTask.value.loading = false;
      swal.close();
    })
    .catch((error) => {
      //   toast.error("Tải dữ liệu không thành công6!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const closeDialogLinkTask = () => {
  displayLinkTask.value = false;
};
const listLinkTaskCheck = ref();
const saveAddLinkTask = () => {
  var list = listTaskLink.value.filter((x) => x.is_check);
  list.forEach(function (l) {
    l.parent_id = datalists.value.task_id;
  });
  listLinkTaskCheck.value = list;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL + "/api/task_origin/Add_LinkTask",
      listLinkTaskCheck.value,
      config,
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        loadChildTaskOrigin(0);
        displayLinkTask.value = false;
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
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
          .delete(baseURL + "/api/task_Files/delete_Task_File", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: id,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá tệp tài liệu thành công!");
              loadFile();
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
let filesTaiLieu = [];
const removefilesTaiLieu = (event) => {
  filesTaiLieu = [];
};
const selectfilesTaiLieu = (event) => {
  filesTaiLieu = [];
  event.files.forEach((element) => {
    filesTaiLieu.push(element);
  });
};
const openFileDialog = ref(false);
const OpenFileDialog = () => {
  openFileDialog.value = true;
  filesTaiLieu = [];
};
const Upload = () => {
  let checkFile;
  openFileDialog.value = false;
  let formData = new FormData();
  if (filesTaiLieu.length == 0) {
    checkFile = "Chưa có tệp tải lên!";
  }
  for (var i = 0; i < filesTaiLieu.length; i++) {
    let file = filesTaiLieu[i];
    formData.append("url_file", file);
  }
  formData.append("task_id", JSON.stringify(datalists.value.task_id));

  if (checkFile == null) {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
    axios
      .post(baseURL + "/api/task_Files/add_Task_File", formData, config)
      .then((response) => {
        if (response.data.err != "1") {
          swal.close();
          toast.success("Tải tệp tài liệu lên thành công!");
          loadFile();
          filesTaiLieu = [];
        } else {
          swal.close();
          swal.fire({
            title: "Thông báo",
            html: "Vui lòng kiểm tra lại: <br>" + response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
          });
        }
      })
      .catch((error) => {
        swal.close();
        swal.fire({
          title: "Thông báo",
          text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
          icon: "error",
          confirmButtonText: "OK",
        });
      });
  } else {
    swal.close();
    swal.fire({
      title: "Thông báo",
      text: checkFile,
      icon: "error",
      confirmButtonText: "OK",
    });
  }
};

const menu = ref();
const items = ref([
  {
    label: "Chưa bắt đầu",
    color: "#C5A598",
    code: 0,
    command: () => {
      ChangeStatusTask(0);
    },
  },
  {
    label: "Đang làm",
    color: "#2196F3",
    code: 1,
    command: () => {
      ChangeStatusTask(1);
    },
  },
  {
    label: "Tạm ngừng",
    color: "#D87777",
    code: 2,
    command: () => {
      ChangeStatusTask(2);
    },
  },
  {
    label: "Đã đóng",
    color: "#FF0000",
    code: 3,
    command: () => {
      ChangeStatusTask(3);
    },
  },
  {
    label: "Hoàn thành đúng hạn",
    color: "#5Cb85c",
    code: 4,
    command: () => {
      ChangeStatusTask(4);
    },
  },
  // { label: "Đợi đánh giá", color: "#C5A598" },
  // { label: "Bị trả lại", color: "#C5A598" },
  {
    label: "Hoàn thành sau hạn",
    color: "#FF8b4E",
    code: 7,
    command: () => {
      ChangeStatusTask(7);
    },
  },
  // { label: "Đã đánh giá", color: "#C5A598" },
]);

const toggle = (event) => {
  menu.value.toggle(event);
};
const ChangeStatusTask = (stt) => {
  menu.value.hide();
  if (stt == datalists.value.status) {
    toast.success("Công việc đang có trạng thái: " + datalists.value.statuss);
    return;
  }
  end_date.value.stt = stt;
  if (stt == 4 || stt == 7) {
    minDate.value = datalists.value.start_date;
    maxDate.value = datalists.value.end_date;
    end_date.value.end_date = null;
    openStatusTask.value = true;
    headerStatusTask.value = "Cập nhật ngày hoàn thành";
  } else {
    if (stt == 3) {
      displayTask.value = false;
      if (datalists.value.status == 3) {
        toast.success("Công việc đang đóng!");
        return;
      }
      swal
        .fire({
          title: "Thông báo",
          html: "Bạn có chắc chắn muốn đóng công việc này không!<br/>(Hành động này sẽ đóng tất cả các công việc con của công việc hiện tại!)",
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
            UpdateStatusTaksFunc(stt, null, null);
          }
        });
    } else {
      UpdateStatusTaksFunc(stt, null, null);
    }
  }
};
const headerStatusTask = ref();
const openStatusTask = ref(false);
const end_date = ref({ end_date: null, stt: null });
const StatusTaskRule = {
  end_date: { required },
};
const closeStatusTask = () => {
  openStatusTask.value = false;
};
const sbmStatusTask = ref(false);
const validateStatusTask = useVuelidate(StatusTaskRule, end_date);
const UpdateStatusTaksFunc = (stt, end_date, isFormValid) => {
  if (stt == 4 || stt == 7) {
    sbmStatusTask.value = true;
    if (!isFormValid) {
      return;
    }
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .put(
      baseURL + "/api/task_origin/" + "Update_Status_TaskOrigin",
      { task_id: datalists.value.task_id, stt: stt, end_date: end_date },
      config,
    )
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái công việc thành công!");
        loadData(true);
        sbmStatusTask.value = false;
        openStatusTask.value = false;
        emitter.emit("update_status_task", true);
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html: ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
        icon: "error",
        confirmButtonText: "OK",
      });
    });
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
const closeFileUpload = () => {
  openFileDialog.value = false;
};
//Thêm bình luận
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
    formData.append("comment", JSON.stringify(bugComment));
    formData.append("task_id", JSON.stringify(datalists.value.task_id));
    formData.append(
      "is_reply",
      JSON.stringify(reply.value == true ? true : false),
    );
    if (reply.value == true) {
      formData.append(
        "parent_id",
        JSON.stringify(replyCmtValue.value[0].comment_id),
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
        `/api/task_Comments/${
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
          comment.value = "<p></p>";
          comment_zone_main.value.setHTML("<p>" + comment.value + "</p>");
          filecoments = [];
          listFileComment.value = [];
          listSendFile.value = [];
          editComment.value = false;
          sending.value = false;
          reply.value = false;
          closeReplyOrEditCmt();
          loadComments();
          GotoView("comment_final");
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
const reply = ref(false);
const replyCmtValue = ref();
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
const DelComment = (id) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xóa bình luận này không!",
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
        sending.value = true;
        var listId = [];
        listId.push(id);
        axios
          .delete(baseURL + "/api/task_comments/deleteTaskComments", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: listId,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá bình luận công việc thành công!");
              loadComments();
              sending.value = false;
            } else {
              swal.fire({
                title: "Thông báo!",
                text: response.data.ms,
                icon: "error",
                confirmButtonText: "OK",
              });
              sending.value = false;
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
  sending.value = false;
};
const editComment = ref(false);
const listIdFileEditComments_Del = ref();
const ID_Comment = ref();
const EditComment = (cmt) => {
  closeReplyOrEditCmt();
  editComment.value = true;
  ID_Comment.value = cmt.comment_id;
  if (cmt.parent_id != null) {
    reply.value = true;
    replyCmtValue.value = [];
    let comt = null;
    comt = cmt.parent;
    comt.contents = comt.contents.replace(
      'style="width:100%;max-width:30vw;object-fit:contain">',
      'style="width:100%;max-width:5vw;object-fit:contain">',
    );
    replyCmtValue.value.push(comt);
  }
  comment.value = cmt.contents.replace("<body>", "").replace("</body>", "");
  comment_zone_main.value.setHTML("<p>" + comment.value + "</p>");
  listIdFileEditComments_Del.value = [];
  if (cmt.files != null) {
    for (let index = 0; index < cmt.files.length; index++) {
      const element = cmt.files[index];
      listFileComment.value.push({
        data: { data: element, name: element.file_name },
        src:
          element.is_image == 0
            ? element.file_name
            : basedomainURL + element.file_path,
        size: element.file_size,
        checkimg: element.is_image == 0 ? false : true,
      });
    }
  }
};
const closeReplyOrEditCmt = () => {
  loadComments();
  editComment.value = false;
  reply.value = false;
  replyCmtValue.value = [];
  listFileComment.value = [];
  comment.value = "";
  listSendFile.value = [];
  comment_zone_main.value.setHTML("<p>" + comment.value + "</p>");
};
const is_Type = ref();
const GoToMemberView = (isType) => {
  is_Type.value = isType;
  Switch("5");
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
emitter.on("closeTaskChecklists", (obj) => {
  ViewTaskChecklists.value = obj;
  loadData(true);
});
emitter.on("deleteFile", (obj) => {
  loadFile();
});
emitter.on("reload", (obj) => {
  loadData(true);
});
const GotoView = (id) => {
  if (id == "Checklist") {
    Switch("1");
    OpenViewTaskChecklists(0);
  } else if (id == "members") {
    Switch("5");
  } else if (id == "file") {
    Switch("6");
  } else if (id == "comment_final") {
    Switch("1");
    setTimeout(() => {
      const objDiv = document.getElementById(id);
      if (objDiv != null) {
        objDiv.scrollIntoView();
      }
    }, 500);
  } else {
    Switch("1");
    if (countComments.value != null && countComments.value > 0)
      setTimeout(() => {
        const objDiv = document.getElementById(id);
        if (objDiv != null) {
          objDiv.scrollIntoView();
        }
      }, 500);
  }
  // setTimeout(() => {
  //   const objDiv = document.getElementById(id);
  //   if (objDiv != null) {
  //     objDiv.scrollIntoView();
  //   }
  // }, 500);
};
const ViewTaskChecklists = ref(false);
const ChecklistType = ref();
const OpenViewTaskChecklists = (number) => {
  ViewTaskChecklists.value = true;
  ChecklistType.value = number;
};
//LINk Doc
const countDocMaster = ref();
const ListDocMaster = ref();
const LinkDoc = ref();
const openDocDialog = ref(false);
const loadTaskDoc = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_linkdoc_listToTask",
            par: [{ par: "id", va: props.id }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let Count = JSON.parse(response.data.data)[1];
      countDocMaster.value = Count[0].totalRecord;
      data.forEach((x) => {
        x.filesize_display = formatSize(x.file_size);
        x.creator_tooltip =
          "Người liên kết <br/>" +
          x.full_name +
          "<br/>" +
          x.positions +
          "<br/>" +
          (x.department_name != null ? x.department_name : x.organiztion_name);
      });
      ListDocMaster.value = data;
    })
    .catch((error) => {
      //// toast.error("Tải dữ liệu không thành công!");

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const TaskLinkDOC = ref({
  organization_id: null,
  task_id: null,
  doc_master_id: null,
  is_main: null,
});

const OpenLinkDocToTask = () => {
  LinkDoc.value = "Gắn văn bản liên quan từ Văn bản";
  openDocDialog.value = true;
  TaskLinkDOC.value = {
    organization_id: user.organization_id,
    task_id: props.id,
    doc_master_id: null,
    is_main: datalists.value.parent_id != null ? true : false,
  };
};
const DelLink = (item) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá liên kết công việc này không!",
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
        var ids = [];
        if (item != null) {
          ids = [item.linkdoc_id];
        }
        axios
          .delete(baseURL + "/api/task_origin/Delete_task_linkdoc", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: ids,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá liên kết công việc thành công!");
              loadTaskDoc();
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
};

onMounted(() => {
  //isViewTask();
  Switch("1");
  if (props.id == null) {
    hideall();
  } else {
    isShow.value = true;
  }
  loadData(true);
  startProgress();
  listUser();
  return {};
});
onBeforeUnmount(() => {
  endProgress();
});
const router = inject("router");

const closeSildeBar = () => {
  isShow.value = false;
  emitter.emit("SideBar", false);
};
const selectedUser = ref([]);
const headerDialogUser = ref();
const displayDialogUser = ref();
const is_one = ref();
const is_type = ref();
const OpenDialogTreeUser = (one, type) => {
  selectedUser.value = [];
  if (type == 1) {
    Task.value.assign_user_id.forEach((t) => {
      let select = { user_id: t };
      selectedUser.value.push(select);
    });
    headerDialogUser.value = "Chọn người giao việc";
  } else if (type == 2) {
    Task.value.work_user_ids.forEach((t) => {
      let select = { user_id: t };
      selectedUser.value.push(select);
    });
    headerDialogUser.value = "Chọn người thực hiện";
  } else if (type == 3) {
    Task.value.works_user_ids.forEach((t) => {
      let select = { user_id: t };
      selectedUser.value.push(select);
    });
    headerDialogUser.value = "Chọn người đồng thực hiện";
  } else if (type == 4) {
    Task.value.follow_user_ids.forEach((t) => {
      let select = { user_id: t };
      selectedUser.value.push(select);
    });
    headerDialogUser.value = "Chọn người theo dõi";
  }
  displayDialogUser.value = true;
  is_one.value = one;
  is_type.value = type;
};
const closeDialogUser = () => {
  displayDialogUser.value = false;
};
const choiceTreeUser = () => {
  switch (is_type.value) {
    case 1:
      if (selectedUser.value.length > 0) {
        selectedUser.value.forEach((t) => {
          Task.value.assign_user_id = [];
          Task.value.assign_user_id.push(t.user_id);
        });
      }
      break;
    case 2:
      if (selectedUser.value.length > 0) {
        Task.value.work_user_ids = [];
        selectedUser.value.forEach((t) => {
          Task.value.work_user_ids.push(t.user_id);
        });
      }
      break;
    case 3:
      if (selectedUser.value.length > 0) {
        Task.value.works_user_ids = [];
        selectedUser.value.forEach((t) => {
          Task.value.works_user_ids.push(t.user_id);
        });
      }
      break;
    case 4:
      if (selectedUser.value.length > 0) {
        Task.value.follow_user_ids = [];
        selectedUser.value.forEach((t) => {
          Task.value.follow_user_ids.push(t.user_id);
        });
      }
      break;
    default:
      break;
  }
  displayDialogUser.value = false;
};
const is_viewSecurityTask = ref(true);
</script>
<template>
  <div
    class="overflow-hidden h-full w-full col-md-12 p-0 m-0 flex"
    v-if="is_viewSecurityTask == true"
  >
    <div class="col-9 p-0 m-0">
      <div
        class="row col-12 flex w-full justify-content-center px-0 mx-0 format-center"
      >
        <div class="col-1 p-0 m-0 flex">
          <Button
            v-if="props.turn == 0"
            icon="pi pi-times"
            class="p-button-rounded p-button-text"
            v-tooltip="{ value: 'Đóng' }"
            @click="closeSildeBar()"
          />
          <!-- <Button
            icon="pi pi-window-maximize"
            class="p-button-rounded p-button-text"
            v-tooltip="{ value: 'Phóng to' }"
            @click="MaxMin('full')"
            v-if="PositionSideBar == 'right'"
          /> -->

          <Button
            icon="pi pi-window-minimize"
            class="p-button-rounded p-button-text"
            v-tooltip="{ value: 'Thu nhỏ' }"
            @click="MaxMin('right')"
            v-if="PositionSideBar == 'full'"
          />
          <Button
            v-if="props.turn == 1"
            icon="pi pi-times"
            class="p-button-rounded p-button-text"
            @click="close()"
            v-tooltip="{ value: 'Quay lại công việc trước' }"
          />
        </div>
        <div class="col-11 p-0 m-0 flex pr-55">
          <div class="col-3 p-0 m-0 format-center pt-1">
            <Button
              class="p-button-success py-0 w-full"
              style="
                font-weight: 500;
                background-color: #59d05d !important;
                font-size: 1rem !important;
                box-shadow: 0px 1px 15px 1px rgb(69 65 78 / 8%);
              "
              @click="GotoView('Checklist')"
            >
              <i class="pi pi-list px-0"></i>
              <Badge
                class="px-1"
                size="large"
                style="background-color: #59d05d !important; color: white"
              >
                {{ countChecklists }}
              </Badge>
              <span class="px-2"> Checklist </span>
            </Button>
          </div>
          <div class="col-3 p-0 m-0 format-center pt-1">
            <Button
              class="p-button-success py-0 w-full"
              style="
                font-weight: 500;
                background-color: #fbad4c !important;
                font-size: 1rem !important;
                box-shadow: 0px 1px 15px 1px rgb(69 65 78 / 8%);
              "
              @click="GotoView('members')"
            >
              <i class="pi pi-user px-0"></i>
              <Badge
                class="px-1"
                size="large"
                style="background-color: #fbad4c !important; color: white"
              >
                {{ countMembers }}
              </Badge>
              <span class="px-2"> Tham Gia </span>
            </Button>
          </div>
          <div class="col-3 p-0 m-0 format-center pt-1">
            <Button
              class="p-button-success py-0 w-full"
              style="
                font-weight: 500;
                background-color: #ff646d !important;
                font-size: 1rem !important;
                box-shadow: 0px 1px 15px 1px rgb(69 65 78 / 8%);
              "
              @click="GotoView('comments')"
            >
              <i class="pi pi-comments px-0"></i>
              <Badge
                class="px-1"
                size="large"
                style="background-color: #ff646d !important; color: white"
              >
                {{ countComments }}
              </Badge>
              <span class="px-2"> Bình luận </span>
            </Button>
          </div>
          <div class="col-3 p-0 m-0 format-center pt-1">
            <Button
              class="p-button-success py-0 w-full"
              style="
                font-weight: 500;
                background-color: #1d62f0 !important;
                font-size: 1rem !important;
                box-shadow: 0px 1px 15px 1px rgb(69 65 78 / 8%);
              "
              @click="GotoView('file')"
            >
              <i class="pi pi-file px-0"></i>
              <Badge
                class="px-1"
                size="large"
                style="background-color: #1d62f0 !important; color: white"
              >
                {{ countAllFile }}
              </Badge>
              <span class="px-2"> Tài liệu </span>
            </Button>
          </div>
        </div>
      </div>
      <div
        v-if="ThongTinChung == true"
        class="flex flex-wrap align-items-center justify-content-center"
      >
        <div
          class="relative"
          style="width: 100%; height: calc(100vh - 5rem)"
        >
          <ScrollPanel
            class="thongtinchungscroll"
            style="width: 100%; height: calc(100% - 6.5rem)"
          >
            <div
              id="thongtinchung "
              style="margin-top: 12px"
            >
              <div class="row col-12 font-bold text-xl pb-1">
                <i class="pi pi-check-square pr-2"></i>
                <span>
                  {{ datalists.task_name }}
                </span>
              </div>
              <div
                class="row col-12"
                style="font-size: 1.15rem !important"
              >
                <div class="flex row col-12 m-0 pt-1 pb-1">
                  <div class="col-8 py-0 m-0 pl-3">
                    Tạo bởi:
                    <span
                      style="
                        color: #2196f3;
                        font-weight: 700;
                        margin-left: 5px;
                        margin-right: 5px;
                      "
                    >
                      {{ datalists.creator }}</span
                    >-
                    {{
                      moment(new Date(datalists.created_date)).format(
                        "HH:mm DD/MM",
                      )
                    }}
                  </div>

                  <div
                    class="col-4 py-0 m-0"
                    v-if="datalists.weights"
                  >
                    Trọng số:
                    <span v-html="datalists.weights"></span>
                  </div>
                </div>
                <div class="flex row col-12 m-0 pt-2">
                  <div class="col-8 py-0 m-0 pl-3">
                    <span v-if="datalists.project_name">
                      Thuộc dự án:
                      <span
                        style="
                          font-style: italic;
                          font-weight: lighter;
                          margin-left: 5px;
                          margin-right: 5px;
                        "
                      >
                        {{ datalists.project_name }}
                      </span></span
                    >
                  </div>
                  <div class="col-4 py-0 m-0">
                    Tiến độ công việc:<span
                      style="
                        font-weight: 700;
                        margin-left: 5px;
                        margin-right: 5px;
                      "
                      >{{ datalists.progress }}%</span
                    >
                  </div>
                </div>
              </div>
              <div id="members">
                <div class="row col-12">
                  <div
                    class="flex row col-12 p-0 m-0"
                    style="font-weight: 600; color: #888; font-size: 1.15rem"
                  >
                    <div
                      class="col-6 p-0 m-0"
                      v-if="ngv > 0"
                    >
                      <div class="row col-12">
                        <div class="col-12 p-0 m-0">
                          <i class="pi pi-user pr-2"></i>
                          <span>Người giao việc</span>
                        </div>
                      </div>
                      <div class="row col-12 p-0 m-0">
                        <div
                          class="flex col-12 p-0 m-0 pl-5"
                          style="flex-wrap: wrap"
                        >
                          <div
                            class="flex p-0 m-0"
                            v-for="(m, index) in members"
                            :key="m"
                          >
                            <Avatar
                              v-if="m.is_type == 0"
                              v-tooltip.right="{
                                value: m.tooltip,
                                escape: true,
                              }"
                              v-bind:label="
                                m.avt
                                  ? ''
                                  : m.full_name
                                      .split(' ')
                                      .at(-1)
                                      .substring(0, 1)
                              "
                              v-bind:image="basedomainURL + m.avt"
                              style="color: #ffffff; cursor: pointer"
                              :style="{
                                background: bgColor[index % 7],
                                border: '2px solid' + bgColor[index % 7],
                              }"
                              class="flex p-0 m-0"
                              size="normal"
                              shape="circle"
                            />
                          </div>
                        </div>
                      </div>
                    </div>
                    <div
                      class="col-6 p-0 m-0"
                      v-if="ntd > 0"
                    >
                      <div class="row col-12">
                        <div class="col-12 p-0 m-0">
                          <i class="pi pi-user pr-2"></i>
                          <span>Người theo dõi</span>
                        </div>
                      </div>
                      <div class="row col-12 p-0 m-0">
                        <div
                          class="flex col-12 p-0 m-0 pl-5"
                          style="flex-wrap: wrap"
                        >
                          <AvatarGroup>
                            <div
                              class="flex p-0 m-0"
                              v-for="(m, index) in members"
                              :key="m"
                            >
                              <Avatar
                                @error="
                                  $event.target.src =
                                    basedomainURL + '/Portals/Image/nouser1.png'
                                "
                                v-if="m.is_type == 3 && m.STTTD < 4"
                                v-tooltip.right="{
                                  value: m.tooltip,
                                  escape: true,
                                }"
                                v-bind:label="
                                  m.avt
                                    ? ''
                                    : m.full_name
                                        .split(' ')
                                        .at(-1)
                                        .substring(0, 1)
                                "
                                v-bind:image="basedomainURL + m.avt"
                                style="color: #ffffff; cursor: pointer"
                                :style="{
                                  background: bgColor[index % 7],
                                  border: '2px solid' + bgColor[index % 7],
                                }"
                                class="flex p-0 m-0"
                                size="normal"
                                shape="circle"
                              />
                            </div>
                            <Avatar
                              @error="
                                $event.target.src =
                                  basedomainURL + '/Portals/Image/nouser1.png'
                              "
                              v-if="ntd > 4"
                              v-tooltip.right="{
                                value:
                                  'và ' + (ntd - 4) + ' người khác tham gia',
                              }"
                              :label="'+' + (ntd - 4)"
                              style="color: #ffffff; cursor: pointer"
                              :style="{
                                background: bgColor[ntd % 7],
                                border: '2px solid' + bgColor[ntd % 7],
                              }"
                              class="flex p-0 m-0"
                              size="normal"
                              shape="circle"
                              @click="GoToMemberView(4)"
                            />
                          </AvatarGroup>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div
                    class="flex row col-12 p-0 m-0"
                    style="font-weight: 600; color: #888; font-size: 1.15rem"
                  >
                    <div
                      class="col-6 p-0 m-0 pt-1"
                      v-if="nth > 0"
                    >
                      <div class="row col-12">
                        <div class="col-12 p-0 m-0">
                          <i class="pi pi-user-edit pr-2"></i>
                          <span>Người xử lý chính</span>
                        </div>
                      </div>
                      <div class="row col-12 p-0 m-0">
                        <div
                          class="flex col-12 p-0 m-0 pl-5"
                          style="flex-wrap: wrap"
                        >
                          <AvatarGroup>
                            <div
                              class="flex p-0 m-0"
                              v-for="(m, index) in members"
                              :key="m"
                            >
                              <Avatar
                                @error="
                                  $event.target.src =
                                    basedomainURL + '/Portals/Image/nouser1.png'
                                "
                                v-if="m.is_type == 1 && m.STTTH < 4"
                                v-tooltip.right="{
                                  value: m.tooltip,
                                  escape: true,
                                  index: 190000,
                                }"
                                v-bind:label="
                                  m.avt
                                    ? ''
                                    : m.full_name
                                        .split(' ')
                                        .at(-1)
                                        .substring(0, 1)
                                "
                                v-bind:image="basedomainURL + m.avt"
                                style="color: #ffffff; cursor: pointer"
                                :style="{
                                  background: bgColor[index % 7],
                                  border: '2px solid' + bgColor[index % 7],
                                }"
                                class="flex p-0 m-0"
                                size="normal"
                                shape="circle"
                              />
                            </div>
                            <Avatar
                              @error="
                                $event.target.src =
                                  basedomainURL + '/Portals/Image/nouser1.png'
                              "
                              v-if="nth > 4"
                              v-tooltip.right="{
                                value:
                                  'và ' + (nth - 4) + ' người khác tham gia',
                              }"
                              :label="'+' + (nth - 4)"
                              style="color: #ffffff; cursor: pointer"
                              :style="{
                                background: bgColor[nth % 7],
                                border: '2px solid' + bgColor[nth % 7],
                              }"
                              class="flex p-0 m-0"
                              size="normal"
                              shape="circle"
                              @click="GoToMemberView(2)"
                            />
                          </AvatarGroup>
                        </div>
                      </div>
                    </div>
                    <div
                      class="col-6 p-0 m-0 pt-1"
                      v-if="ndth > 0"
                    >
                      <div class="row col-12">
                        <div class="col-12 p-0 m-0">
                          <i class="pi pi-user-edit pr-2"></i>
                          <span>Người đồng xử lý</span>
                        </div>
                      </div>
                      <div class="row col-12 p-0 m-0">
                        <div
                          class="flex col-12 p-0 m-0 pl-5"
                          style="flex-wrap: wrap"
                        >
                          <AvatarGroup>
                            <div
                              class="flex p-0 m-0"
                              v-for="(m, index) in members"
                              :key="m"
                            >
                              <Avatar
                                @error="
                                  $event.target.src =
                                    basedomainURL + '/Portals/Image/nouser1.png'
                                "
                                v-if="m.is_type == 2 && m.STTDTH < 4"
                                v-tooltip.right="{
                                  value: m.tooltip,
                                  escape: true,
                                }"
                                v-bind:label="
                                  m.avt
                                    ? ''
                                    : m.full_name
                                        .split(' ')
                                        .at(-1)
                                        .substring(0, 1)
                                "
                                v-bind:image="basedomainURL + m.avt"
                                style="color: #ffffff; cursor: pointer"
                                :style="{
                                  background: bgColor[index % 7],
                                  border: '2px solid' + bgColor[index % 7],
                                }"
                                class="flex p-0 m-0"
                                size="normal"
                                shape="circle"
                              />
                            </div>
                            <Avatar
                              @error="
                                $event.target.src =
                                  basedomainURL + '/Portals/Image/nouser1.png'
                              "
                              v-if="ndth > 4"
                              v-tooltip.right="{
                                value:
                                  'và ' + (ndth - 4) + ' người khác tham gia',
                              }"
                              :label="'+' + (ndth - 4)"
                              style="color: #ffffff; cursor: pointer"
                              :style="{
                                background: bgColor[ndth % 7],
                                border: '2px solid' + bgColor[ndth % 7],
                              }"
                              class="flex p-0 m-0"
                              size="normal"
                              shape="circle"
                              @click="GoToMemberView(3)"
                            />
                          </AvatarGroup>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div class="row col-12">
                <div
                  class="flex row col-12 p-0 m-0"
                  style="font-weight: 600; color: #888; font-size: 1.15rem"
                >
                  <div
                    class="col-6 p-0 m-0"
                    v-if="datalists.start_date"
                  >
                    <div class="col-12">
                      <i class="pi pi-calendar pr-2"></i>
                      <span>Ngày bắt đầu (Dự kiến)</span>
                    </div>
                    <div
                      class="col-12 flex p-0 m-0 pl-5 font-bold"
                      style="color: #72777a"
                    >
                      {{
                        moment(new Date(datalists.start_date)).format(
                          "DD/MM/YYYY HH:mm",
                        )
                      }}
                    </div>
                  </div>
                  <div
                    class="col-6 p-0 m-0"
                    v-if="datalists.end_date"
                  >
                    <div class="col-12">
                      <i class="pi pi-calendar-times pr-2"></i>
                      <span>Ngày kết thúc (Dự kiến)</span>
                    </div>
                    <div
                      class="col-12 flex p-0 m-0 pl-5"
                      style="color: #2196f3"
                    >
                      {{
                        moment(new Date(datalists.end_date)).format(
                          "DD/MM/YYYY HH:mm",
                        )
                      }}
                    </div>
                  </div>
                </div>
                <div
                  class="flex row col-12 p-0 m-0"
                  style="font-weight: 600; color: #888; font-size: 1.15rem"
                  v-if="datalists.start_real_date || datalists.end_real_date"
                >
                  <div
                    class="col-6 p-0 m-0"
                    v-if="datalists.start_real_date"
                  >
                    <div class="col-12">
                      <i class="pi pi-clock pr-2"></i>
                      <span>Ngày bắt đầu</span>
                    </div>
                    <div class="col-12 flex p-0 m-0 pl-5">
                      {{
                        moment(new Date(datalists.start_real_date)).format(
                          "DD/MM/YYYY",
                        )
                      }}
                    </div>
                  </div>
                  <div
                    class="col-6 p-0 m-0"
                    v-if="datalists.end_real_date"
                  >
                    <div class="col-12">
                      <i class="pi pi-check-circle pr-2"></i>
                      <span>Ngày kết thúc</span>
                    </div>
                    <div
                      class="col-12 flex p-0 m-0 pl-5"
                      style="color: #2196f3"
                    >
                      {{
                        moment(new Date(datalists.end_real_date)).format(
                          "DD/MM/YYYY",
                        )
                      }}
                    </div>
                  </div>
                </div>
              </div>
              <div
                class="row col-12"
                v-if="datalists.description"
              >
                <div
                  class="col-12 p-0 m-0"
                  style="font-weight: 600; color: #888; font-size: 1.15rem"
                >
                  <div class="col-12">
                    <i class="pi pi-info-circle pr-2"></i>
                    <span>Mô tả </span>
                  </div>
                </div>
                <div class="col-12 p-0 m-0">
                  <div class="col-12 p-0 m-0">
                    <div
                      class="p-0 m-0 pl-5"
                      style="line-height: 1.5"
                      v-for="(m, index) in datalists.description"
                      :key="index"
                    >
                      <span v-if="datalists.description.length > 1 && m != ''"
                        >- </span
                      >{{ m }}
                    </div>
                  </div>
                </div>
              </div>
              <div
                class="row col-12 p-0"
                v-if="datalists.target"
              >
                <div
                  class="col-12"
                  style="font-weight: 600; color: #888; font-size: 1.15rem"
                >
                  <div class="col-12">
                    <i class="pi pi-info-circle pr-2"></i>
                    <span>Mục tiêu</span>
                  </div>
                </div>

                <div class="col-12 p-0 m-0">
                  <div class="col-12 p-0 m-0">
                    <div
                      class="p-0 m-0 pl-5"
                      style="line-height: 1.5"
                      v-for="(m, index) in datalists.target"
                      :key="index"
                    >
                      <span v-if="datalists.target.length > 1 && m != ''"
                        >- </span
                      >{{ m }}
                    </div>
                  </div>
                </div>
              </div>
              <div
                class="row col-12"
                v-if="datalists.difficult"
              >
                <div
                  class="col-12 p-0 m-0"
                  style="font-weight: 600; color: #888; font-size: 1.15rem"
                >
                  <div class="col-12">
                    <i class="pi pi-info-circle pr-2"></i>
                    <span>Khó khăn vướng mắc</span>
                  </div>
                </div>
                <div class="col-12 p-0 m-0">
                  <div class="col-12 p-0 m-0">
                    <div
                      class="p-0 m-0 pl-5"
                      style="line-height: 1.5"
                      v-for="(m, index) in datalists.difficult"
                      :key="index"
                    >
                      <span v-if="datalists.difficult.length > 1 && m != ''"
                        >- </span
                      >{{ m }}
                    </div>
                  </div>
                </div>
              </div>
              <div
                class="row col-12"
                v-if="datalists.request"
              >
                <div
                  class="col-12 p-0 m-0"
                  style="font-weight: 600; color: #888; font-size: 1.15rem"
                >
                  <div class="col-12">
                    <i class="pi pi-info-circle pr-2"></i>
                    <span>Đề xuất</span>
                  </div>
                </div>
                <div class="col-12 p-0 m-0">
                  <div class="col-12 p-0 m-0">
                    <div
                      class="p-0 m-0 pl-5"
                      style="line-height: 1.5"
                      v-for="(m, index) in datalists.request"
                      :key="index"
                    >
                      <span v-if="datalists.request.length > 1 && m != ''"
                        >- </span
                      >{{ m }}
                    </div>
                  </div>
                </div>
              </div>
              <div
                class="row col-12"
                v-if="datalists.result"
              >
                <div
                  class="col-12 p-0 m-0"
                  style="font-weight: 600; color: #888; font-size: 1.15rem"
                >
                  <div class="col-12">
                    <i class="pi pi-check-circle pr-2"></i>
                    <span>Kết quả đạt được</span>
                  </div>
                </div>
                <div class="col-12 p-0 m-0">
                  <div class="col-12 p-0 m-0">
                    <div
                      class="p-0 m-0 pl-5"
                      style="line-height: 1.5"
                      v-for="(m, index) in datalists.result"
                      :key="index"
                    >
                      <span v-if="datalists.result.length > 1 && m != ''"
                        >- </span
                      >{{ m }}
                    </div>
                  </div>
                </div>
              </div>
              <div class="row col-12">
                <div
                  class="col-12 p-0 m-0"
                  style="font-weight: 600; color: #888; font-size: 1.15rem"
                >
                  <div class="col-12">
                    <i
                      class="pi pi-percentage pr-2"
                      style="font-size: 0.9rem !important"
                    ></i>
                    <span>Tiến độ</span>
                  </div>
                </div>
                <div class="col-12 p-0 m-0">
                  <div class="col-12 p-0 m-0">
                    <div class="p-0 m-0 pl-5">
                      <ProgressBar
                        :value="
                          datalists.progress != null ? datalists.progress : 0
                        "
                        :show-value="true"
                      />
                    </div>
                  </div>
                </div>
              </div>
              <div class="row col-12">
                <div
                  class="flex col-12 p-0 m-0"
                  style="font-weight: 600; color: #888; font-size: 1.15rem"
                >
                  <div class="col-4">
                    <i
                      class="pi pi-list pr-2"
                      style="font-size: 0.9rem !important"
                    ></i>
                    <span>
                      Checklist
                      <Button
                        v-if="isClose == false"
                        icon="pi pi-plus-circle"
                        class="p-button-secondary p-button-text p-0 m-0"
                        v-tooltip.top="{ value: 'Thêm Checklist' }"
                        @click="addCheckList()"
                      />
                    </span>
                  </div>
                  <div
                    class="col-8 format-right"
                    style="font-weight: normal !important; font-size: 1rem"
                  >
                    <span>
                      <span
                        style="color: black"
                        class="checklist-hover"
                        @click="OpenViewTaskChecklists(0)"
                        >Tất cả ({{ countChecklists }})
                      </span>
                      <span> | </span>
                      <span
                        style="color: #6dd230"
                        class="checklist-hover"
                        @click="OpenViewTaskChecklists(1)"
                        >Đã check ({{ countChecked }})
                      </span>
                      <span> | </span>
                      <span
                        style="color: #ffa500"
                        class="checklist-hover"
                        @click="OpenViewTaskChecklists(2)"
                      >
                        Chưa check ({{ countUnChecked }})
                      </span>
                      <span> | </span>
                      <span
                        style="color: #ff0000"
                        class="checklist-hover"
                        @click="OpenViewTaskChecklists(3)"
                      >
                        Quá hạn ({{ ExpireTimeTask }})</span
                      >
                    </span>
                  </div>
                </div>

                <div
                  class="col-12 p-0 m-0"
                  id="Checklist"
                >
                  <div v-if="listCheckList != null">
                    <div
                      class="col-12"
                      v-for="c in listCheckList"
                      :key="c"
                    >
                      <div
                        class="col-12 font-bold p-0 m-0 pl-4 text-left checklist-gr-hv-p"
                      >
                        <div
                          class="p-0 m-0 p-1"
                          v-tooltip="{ value: c.description }"
                        >
                          {{ c.checklist_name }} ({{ c.totalRecords }})
                        </div>
                        <div
                          class="p-0 m-0 font-bold checklist-gr-hv-c"
                          v-if="isClose == false"
                        >
                          <Button
                            icon="pi pi-pencil"
                            class="p-button-secondary p-button-text format-center p-0 m-0 btn-c-hover"
                            style="width: 1.5rem"
                            v-tooltip.top="{ value: 'Sửa Checklist' }"
                            @click="editCheckList(c)"
                          />
                          <Button
                            icon="pi pi-trash"
                            class="p-button-secondary p-button-text format-center p-0 m-0 p-danger-hover"
                            style="width: 1.5rem"
                            v-tooltip.top="{ value: 'Xóa Checklist' }"
                            @click="deleteCheckList(c.checklist_id)"
                          />
                        </div>
                      </div>
                      <div
                        class="col-12 p-0 m-0 pl-3 p-hover py-2"
                        v-for="(t, index) in c.task"
                        :key="t"
                      >
                        <div class="col-12 p-0 m-0 pl-3">
                          <div
                            class="flex col-12 p-0 m-0"
                            v-if="t.task_id != Task.task_id"
                            :class="
                              t.is_check == true
                                ? 'checked-p'
                                : t.thoigianquahan > 0
                                ? 'expTime'
                                : ''
                            "
                          >
                            <div class="col-10 p-0 m-0 format-center font-bold">
                              <div class="col-1 p-0 m-0">
                                {{ t.STT }}
                              </div>
                              <div
                                class="col-1 p-0 m-0"
                                v-if="isClose == false"
                              >
                                <Checkbox
                                  :binary="true"
                                  v-model="t.is_check"
                                  @click="onCheckboxTask(t)"
                                />
                              </div>
                              <div
                                class="col-1 p-0 m-0"
                                v-else
                              >
                                <Checkbox
                                  :binary="true"
                                  v-model="t.is_check"
                                  disabled="true"
                                />
                              </div>
                              <div class="col-10 p-0 m-0 format-default">
                                <div class="col-12 p-0 m-0 flex">
                                  <div class="col-8 py-0 m-0 format-left">
                                    <span
                                      v-tooltip="'Ưu tiên'"
                                      v-if="t.is_prioritize"
                                      style="margin-right: 5px"
                                    >
                                      <i
                                        style="color: orange"
                                        class="pi pi-star-fill"
                                      ></i>
                                    </span>
                                    <span
                                      class="text-left text-black"
                                      style="line-height: 1.75"
                                      v-html="t.task_names"
                                    ></span>
                                  </div>
                                  <div
                                    class="format-center col-2 p-0 m-0 font-bold"
                                    style="align-items: center"
                                  >
                                    <span v-if="t.end_date"
                                      >{{
                                        moment(new Date(t.end_date)).format(
                                          "DD/MM/YYYY",
                                        )
                                      }}
                                    </span>
                                  </div>
                                  <div class="format-center col-2 p-0 m-0">
                                    <Avatar
                                      @error="
                                        $event.target.src =
                                          basedomainURL +
                                          '/Portals/Image/nouser1.png'
                                      "
                                      v-tooltip.right="{
                                        value: t.creator_tooltip,
                                        escape: true,
                                      }"
                                      v-bind:label="
                                        t.creator_avt
                                          ? ''
                                          : t.creator_full_name
                                              .split(' ')
                                              .at(-1)
                                              .substring(0, 1)
                                      "
                                      v-bind:image="
                                        basedomainURL + t.creator_avt
                                      "
                                      style="color: ; cursor: pointer"
                                      :style="{
                                        background: bgColor[index % 7],
                                        border:
                                          '1px solid' + bgColor[index % 7],
                                      }"
                                      class=""
                                      size=""
                                      shape="circle"
                                    />
                                    <Avatar
                                      @error="
                                        $event.target.src =
                                          basedomainURL +
                                          '/Portals/Image/nouser1.png'
                                      "
                                      v-if="t.close_by != null"
                                      v-tooltip.right="{
                                        value: t.actor_tooltip,
                                        escape: true,
                                      }"
                                      v-bind:label="
                                        t.avt
                                          ? ''
                                          : t.actor_full_name
                                              .split(' ')
                                              .at(-1)
                                              .substring(0, 1)
                                      "
                                      v-bind:image="basedomainURL + t.avt"
                                      style="color: ; cursor: pointer"
                                      :style="{
                                        background: bgColor[index % 7],
                                        border:
                                          '1px solid' + bgColor[index % 7],
                                      }"
                                      class=""
                                      size=""
                                      shape="circle"
                                    />
                                  </div>
                                </div>
                              </div>
                            </div>
                            <div
                              class="col-2 p-0 pr-1 m-0 format-center c-hover"
                              v-if="isClose == false"
                            >
                              <Button
                                icon="pi pi-copy"
                                class="p-button-secondary p-button-text px-0 m-0 btn-c-hover"
                                style="color: black; font-size: 0.5rem"
                                @click="copyToClipboard(t.task_name)"
                                v-tooltip.top="{
                                  value: 'Sao chép vào bộ nhớ tạm',
                                }"
                                :style="
                                  t.is_check == true
                                    ? 'color:#6DD230;text-decoration: unset !important'
                                    : ''
                                "
                              />
                              <Button
                                icon="pi pi-clock"
                                class="p-button-secondary p-button-text px-0 m-0 btn-c-hover"
                                style="color: black; font-size: 0.5rem"
                                v-tooltip.top="{
                                  value: 'Cập nhật hạn xử lý',
                                }"
                                @click="editTime(t)"
                                :style="
                                  t.is_check == true
                                    ? 'color:#6DD230;text-decoration: unset !important'
                                    : ''
                                "
                                v-if="
                                  t.is_check == false &&
                                  (t.created_by == store.state.user.user_id ||
                                    memberType == 0 ||
                                    memberType1 == 0 ||
                                    memberType2 == 0 ||
                                    memberType3 == 0)
                                "
                              />
                              <Button
                                icon="pi pi-pencil"
                                class="p-button-secondary p-button-text px-0 m-0 btn-c-hover"
                                style="color: black; font-size: 0.5rem"
                                :style="
                                  t.is_check == true
                                    ? 'color:#6DD230;text-decoration: unset !important'
                                    : ''
                                "
                                v-if="
                                  t.is_check == false &&
                                  (t.created_by == store.state.user.user_id ||
                                    memberType == 0 ||
                                    memberType1 == 0 ||
                                    memberType2 == 0 ||
                                    memberType3 == 0)
                                "
                                v-tooltip.top="{ value: 'Sửa thông tin' }"
                                @click="editTaskCheckListFunc(t)"
                              />
                              <Button
                                icon="pi pi-trash"
                                class="p-button-secondary p-button-text px-0 m-0 btn-c-hover"
                                style="color: black; font-size: 0.5rem"
                                :style="
                                  t.is_check == true
                                    ? 'color:#6DD230;text-decoration: unset !important'
                                    : ''
                                "
                                v-tooltip.top="{
                                  value: 'Xóa công việc checklist',
                                }"
                                v-if="
                                  t.created_by == store.state.user.user_id ||
                                  memberType == 0 ||
                                  memberType1 == 0 ||
                                  memberType2 == 0 ||
                                  memberType3 == 0
                                "
                                @click="deteleTaskCheckList(t)"
                              />
                            </div>
                          </div>
                          <div
                            class="col-12 p-0 m-0 pl-3"
                            v-if="
                              editTaskCheckListText == true &&
                              t.task_id == Task.task_id
                            "
                          >
                            <div
                              class="col-12 p-0 m-0 pl-3 format-center font-bold"
                            >
                              <div class="flex col-12 p-0 m-0 format-center">
                                <div class="col-10 p-0 m-0 format-center">
                                  <div class="col-1 p-0 m-0">
                                    {{ Task.STT }}
                                  </div>
                                  <div class="col-1">
                                    <Checkbox
                                      :binary="true"
                                      v-model="Task.is_check"
                                    />
                                  </div>
                                  <div class="col-10 p-0 m-0 format-default">
                                    <Textarea
                                      rows="1"
                                      :autoResize="true"
                                      v-model="Task.task_name"
                                      spellcheck="false"
                                      class="w-full"
                                      :class="{
                                        'p-invalid':
                                          v1$.task_name.$invalid &&
                                          submittedTaskCheckList,
                                      }"
                                      placeholder="Nhập tên công việc..."
                                      @keydown.enter.exact.prevent="
                                        saveTaskCheckList(!v1$.$invalid)
                                      "
                                    />
                                  </div>
                                </div>

                                <div class="col-2 p-0 m-0 format-center">
                                  <Button
                                    icon="pi pi-trash"
                                    class="p-button-secondary p-button-text p-button-danger p-danger-hover"
                                    style="font-size: 0.5rem"
                                    v-tooltip.top="{
                                      value: 'Xóa công việc checklist',
                                    }"
                                    v-if="
                                      t.created_by ==
                                        store.state.user.user_id ||
                                      memberType == 0 ||
                                      memberType1 == 0 ||
                                      memberType2 == 0 ||
                                      memberType3 == 0
                                    "
                                    @click="deteleTaskCheckList(t)"
                                  />
                                  <Button
                                    icon="pi pi-times"
                                    class="p-button-secondary p-button-text p-button-danger p-danger-hover"
                                    style="font-size: 0.5rem"
                                    v-tooltip.top="{
                                      value: 'Hủy',
                                    }"
                                    @click="cancel('')"
                                  />
                                </div>
                                <div class="col-1 p-0 m-0 format-center"></div>
                              </div>
                            </div>
                            <div class="col-12 p-0 m-0 pl-3 format-center">
                              <div class="flex col-12 p-0 m-0 format-center">
                                <div class="col-10 p-0 m-0 format-center">
                                  <div class="col-2 p-0 m-0"></div>
                                  <div class="col-10 p-0 m-0 format-default">
                                    <div
                                      class="format-default col-12 px-0 p-0 m-0"
                                      v-if="
                                        submittedTaskCheckList == true &&
                                        v1$.task_name.$invalid
                                      "
                                      style="
                                        text-align: left !important;
                                        font-size: medium;
                                      "
                                    >
                                      <small
                                        v-if="
                                          (v1$.task_name.$invalid &&
                                            submittedTaskCheckList) ||
                                          v1$.task_name.$pending.$response
                                        "
                                        class="col-12 p-error p-0 m-0"
                                      >
                                        <span class="col-12 p-0">{{
                                          v1$.task_name.required.$message
                                            .replace(
                                              "Value",
                                              "Tên công việc checklist",
                                            )
                                            .replace(
                                              "is required",
                                              "không được để trống",
                                            )
                                        }}</span>
                                      </small>
                                    </div>
                                  </div>
                                </div>

                                <div class="col-2 p-0 m-0 format-center"></div>
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                      <div class="col-12 font-bold p-0 m-0 pl-3">
                        <div
                          class="col-12 p-0 m-0 pl-3 format-center"
                          v-if="
                            addNewTaskCheckList == true &&
                            Task.checklist_id == c.checklist_id
                          "
                        >
                          <div class="flex col-12 p-0 m-0 format-center">
                            <div class="col-10 p-0 m-0 format-center">
                              <div class="col-1 p-0 m-0">
                                {{ Task.STT }}
                              </div>
                              <div class="col-1">
                                <Checkbox
                                  :binary="true"
                                  v-model="Task.is_check"
                                />
                              </div>
                              <div class="col-10 p-0 m-0 format-default">
                                <Textarea
                                  rows="1"
                                  :autoResize="true"
                                  v-model="Task.task_name"
                                  spellcheck="false"
                                  class="w-full"
                                  :class="{
                                    'p-invalid':
                                      v1$.task_name.$invalid &&
                                      submittedTaskCheckList,
                                  }"
                                  placeholder="Nhập tên công việc..."
                                  @keydown.enter.exact.prevent="
                                    saveTaskCheckList(!v1$.$invalid)
                                  "
                                />
                              </div>
                            </div>

                            <div class="col-1 p-0 m-0 format-center">
                              <Button
                                icon="pi pi-times"
                                class="p-button-secondary p-button-text p-button-danger p-danger-hover"
                                style="font-size: 0.5rem"
                                @click="cancel('add')"
                              />
                            </div>
                            <div class="col-1 p-0 m-0 format-center"></div>
                          </div>
                        </div>
                        <div
                          class="col-12 p-0 m-0 pl-3 format-center"
                          v-if="
                            addNewTaskCheckList == true &&
                            Task.checklist_id == c.checklist_id
                          "
                        >
                          <div class="flex col-12 p-0 m-0 format-center">
                            <div class="col-10 p-0 m-0 format-center">
                              <div class="col-2 p-0 m-0"></div>
                              <div class="col-10 p-0 m-0 format-default">
                                <div
                                  class="format-default col-12 px-0 p-0 m-0"
                                  v-if="
                                    submittedTaskCheckList == true &&
                                    v1$.task_name.$invalid
                                  "
                                  style="
                                    text-align: left !important;
                                    font-size: medium;
                                  "
                                >
                                  <small
                                    v-if="
                                      (v1$.task_name.$invalid &&
                                        submittedTaskCheckList) ||
                                      v1$.task_name.$pending.$response
                                    "
                                    class="col-12 p-error p-0 m-0"
                                  >
                                    <span class="col-12 p-0">{{
                                      v1$.task_name.required.$message
                                        .replace(
                                          "Value",
                                          "Tên công việc checklist",
                                        )
                                        .replace(
                                          "is required",
                                          "không được để trống",
                                        )
                                    }}</span>
                                  </small>
                                </div>
                              </div>
                            </div>

                            <div class="col-2 p-0 m-0 format-center"></div>
                          </div>
                        </div>
                        <Button
                          icon="pi pi-plus-circle"
                          class="p-button-secondary p-button-text"
                          @click="addTaskCheckList(c)"
                          label="Thêm công việc checklist"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div class="row col-12">
                <div
                  class="flex col-12 p-0 m-0"
                  style="font-weight: 600; color: #888; font-size: 1.15rem"
                >
                  <div class="col-4">
                    <i
                      class="pi pi-tags pr-2"
                      style="font-size: 0.9rem !important"
                    >
                    </i>
                    <span>Công việc con</span>
                    <span>
                      <Button
                        icon="pi pi-plus-circle"
                        class="p-button-secondary p-button-text p-0 m-0"
                        @click="addNewChildTaskOrigin(datalists)"
                        v-if="isClose == false"
                      />
                    </span>
                  </div>
                  <div
                    class="col-8 format-right"
                    style="font-weight: normal !important; font-size: 1rem"
                  >
                    <span>
                      <span
                        style="color: black"
                        class="checklist-hover"
                        @click="loadChildTaskOrigin(0)"
                        >Tất cả ({{ AllChild }})
                      </span>
                      <span> | </span>
                      <span
                        style="color: ff0000"
                        class="checklist-hover"
                        @click="loadChildTaskOrigin(1)"
                        >Chưa bắt đầu ({{ NotStartedChild }})
                      </span>
                      <span> | </span>
                      <span
                        style="color: #2196f3"
                        class="checklist-hover"
                        @click="loadChildTaskOrigin(2)"
                      >
                        Đang thực hiện ({{ DoingChild }})
                      </span>
                      <span> | </span>
                      <span
                        style="color: #6dd230"
                        class="checklist-hover"
                        @click="loadChildTaskOrigin(3)"
                      >
                        Hoàn thành ({{ FinishedChild }})</span
                      >
                    </span>
                  </div>
                </div>

                <div
                  class="col-12 p-0 m-0 pt-1 pb-1 pl-6 child-task-hover"
                  v-for="(ch, index) in ListChildTask"
                  :key="index"
                >
                  <div
                    class="row col-12 flex p-0 m-0"
                    @click="show(ch)"
                  >
                    <div class="col-7 p-0 m-0">
                      <span class="font-bold text-xl">
                        {{ ch.task_name }}
                      </span>
                      <br />
                      <span>
                        {{
                          moment(new Date(ch.start_date)).format("DD/MM/YYYY")
                        }}
                      </span>
                      -
                      <span v-if="ch.is_deadline == true">
                        {{ moment(new Date(ch.end_date)).format("DD/MM/YYYY") }}
                      </span>
                    </div>
                    <div class="col-4 p-0 m-0 format-center">
                      <AvatarGroup>
                        <div
                          v-for="(user, index) in ch.users"
                          :key="index"
                        >
                          <Avatar
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                            v-if="user.is_type == 0 && user.STTGV == 0"
                            v-tooltip.right="{
                              value: user.tooltip,
                              escape: true,
                            }"
                            v-bind:label="
                              user.avt
                                ? ''
                                : user.full_name
                                    .split(' ')
                                    .at(-1)
                                    .substring(0, 1)
                            "
                            v-bind:image="basedomainURL + user.avt"
                            style="color: #ffffff; cursor: pointer"
                            :style="{
                              background: bgColor[index % 7],
                              border: '2px solid' + bgColor[index % 10],
                            }"
                            class=""
                            size="normal"
                            shape="circle"
                          />
                          <Avatar
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                            v-if="user.is_type == 1 && user.STTTH == 0"
                            v-tooltip.right="{
                              value: user.tooltip,
                              escape: true,
                            }"
                            v-bind:label="
                              user.avt
                                ? ''
                                : user.full_name
                                    .split(' ')
                                    .at(-1)
                                    .substring(0, 1)
                            "
                            v-bind:image="basedomainURL + user.avt"
                            style="color: #ffffff; cursor: pointer"
                            :style="{
                              background: bgColor[index % 7],
                              border: '2px solid' + bgColor[index % 10],
                            }"
                            class=""
                            size="normal"
                            shape="circle"
                          />
                          <Avatar
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                            v-if="user.is_type == 2 && user.STTDTH == 0"
                            v-tooltip.right="{
                              value: user.tooltip,
                              escape: true,
                            }"
                            v-bind:label="
                              user.avt
                                ? ''
                                : user.full_name
                                    .split(' ')
                                    .at(-1)
                                    .substring(0, 1)
                            "
                            v-bind:image="basedomainURL + user.avt"
                            style="color: #ffffff; cursor: pointer"
                            :style="{
                              background: bgColor[index % 7],
                              border: '2px solid' + bgColor[index % 10],
                            }"
                            class=""
                            size="normal"
                            shape="circle"
                          />
                          <Avatar
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                            v-if="user.is_type == 3 && user.STTTD == 0"
                            v-tooltip.right="{
                              value: user.tooltip,
                              escape: true,
                            }"
                            v-bind:label="
                              user.avt
                                ? ''
                                : user.full_name
                                    .split(' ')
                                    .at(-1)
                                    .substring(0, 1)
                            "
                            v-bind:image="basedomainURL + user.avt"
                            style="color: #ffffff; cursor: pointer"
                            :style="{
                              background: bgColor[index % 7],
                              border: '2px solid' + bgColor[index % 10],
                            }"
                            class=""
                            size="normal"
                            shape="circle"
                          />
                        </div>
                        <Avatar
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                          v-if="ch.users.length > 4"
                          v-tooltip.right="{
                            value:
                              'và ' +
                              (ch.users.length - 4) +
                              ' người khác tham gia',
                          }"
                          :label="'+' + (ch.users.length - 4)"
                          style="
                            color: #ffffff;
                            cursor: pointer;
                            font-size: 1rem;
                          "
                          :style="{
                            background: bgColor[index % 7],
                            border: '2px solid' + bgColor[index % 10],
                          }"
                          class=""
                          size="normal"
                          shape="circle"
                        ></Avatar>
                      </AvatarGroup>
                    </div>
                    <div class="col-1 p-0 m-0 format-center">
                      {{ ch.progress }}%
                    </div>
                  </div>
                </div>
              </div>

              <div
                class="row col-12"
                id="file"
              >
                <div
                  class="col-12 p-0 m-0"
                  style="font-weight: 600; color: #888; font-size: 1.15rem"
                >
                  <div class="col-12">
                    <i
                      class="pi pi-file pr-2"
                      style="font-size: 0.9rem !important"
                    >
                    </i>
                    <span
                      >Tài liệu công việc ({{ countFiles }})
                      <Button
                        icon="pi pi-plus-circle"
                        class="p-button-secondary p-button-text p-0 m-0"
                        v-tooltip.top="{ value: 'Thêm tệp tài liệu' }"
                        @click="OpenFileDialog()"
                        v-if="isClose == false"
                      />
                    </span>
                  </div>
                </div>
                <div
                  class="col-12 p-0 m-0"
                  v-if="countFiles > 0"
                >
                  <div
                    v-for="(slotProps, index) in listFile"
                    :key="index"
                    class="col-12 p-0 m-0 pl-5 flex file-hover"
                    v-on:dblclick="ViewFileInfo(slotProps)"
                    v-tooltip.top="{
                      value: 'Nháy chuột 2 lần để xem chi tiết',
                    }"
                  >
                    <div class="col-4 format-left">
                      <div
                        class=""
                        v-if="slotProps.is_image == 1"
                      >
                        <Image
                          :src="basedomainURL + slotProps.file_path"
                          :alt="slotProps.file_name"
                          width="24"
                          preview
                          style="
                            max-width: 24px;
                            white-space: nowrap;
                            overflow: hidden;
                            text-overflow: ellipsis;
                          "
                        />
                      </div>
                      <div
                        class=""
                        v-else
                      >
                        <img
                          :src="
                            basedomainURL +
                            '/Portals/Image/file/' +
                            slotProps.file_name.substring(
                              slotProps.file_name.lastIndexOf('.') + 1,
                            ) +
                            '.png'
                          "
                          style="width: 24px; object-fit: contain"
                          :alt="' '"
                          class="pt-1"
                        />
                      </div>
                      <div
                        class="format-left"
                        style="
                          max-width: 30rem;
                          white-space: nowrap;
                          overflow: hidden;
                          text-overflow: ellipsis;
                        "
                      >
                        <span
                          class="pl-2 w-full"
                          v-tooltip.top="{ value: slotProps.file_name }"
                        >
                          {{ " " + slotProps.file_name }}
                        </span>
                      </div>
                    </div>

                    <div class="col-2 format-center">
                      {{ slotProps.filesize_display }}
                    </div>
                    <div class="col-3 format-center">
                      {{
                        moment(new Date(slotProps.created_date)).format(
                          "HH:mm DD/MM/YYYY",
                        )
                      }}
                    </div>
                    <div class="col-1 format-center">
                      <Avatar
                        @error="
                          $event.target.src =
                            basedomainURL + '/Portals/Image/nouser1.png'
                        "
                        v-tooltip.right="{
                          value: slotProps.creator_tooltip,
                          escape: true,
                        }"
                        v-bind:label="
                          slotProps.creator.avt
                            ? ''
                            : slotProps.creator.full_name
                                .split(' ')
                                .at(-1)
                                .substring(0, 1)
                        "
                        v-bind:image="basedomainURL + slotProps.creator.avt"
                        style="color: #ffffff; cursor: pointer"
                        :style="{
                          background:
                            bgColor[Math.floor(Math.random() * 10) % 7],
                          border:
                            '2px solid' + bgColor[(Math.random() * 11) % 7],
                        }"
                        class="p-0 m-0"
                        size="small"
                        shape="circle"
                      />
                    </div>
                    <div class="flex col-2 format-default ml-3">
                      <a
                        download
                        style="text-decoration: none"
                        class="a-hover format-center"
                      >
                        <Button
                          icon="pi pi-download "
                          class="p-button-text p-button-secondary p-button-hover"
                          v-tooltip="{ value: 'Tải tệp xuống' }"
                          @click="download(slotProps)"
                        >
                        </Button
                      ></a>

                      <a
                        v-if="isClose == false"
                        style="text-decoration: none"
                        class="a-hover format-center"
                      >
                        <Button
                          icon="pi pi-trash"
                          class="p-button-text p-button-secondary p-button-hover"
                          @click="DelFile(slotProps)"
                          v-tooltip="{ value: 'Xóa' }"
                          v-if="
                            store.state.user.user_id == slotProps.created_by
                          "
                      /></a>
                    </div>
                  </div>
                </div>
              </div>
              <div class="row col-12">
                <div
                  class="col-12 p-0 m-0"
                  style="font-weight: 600; color: #888; font-size: 1.15rem"
                >
                  <div class="col-12">
                    <i
                      class="pi pi-link pr-2"
                      style="font-size: 0.9rem !important"
                    >
                    </i>
                    <span
                      >Văn bản liên quan (từ Văn bản) ({{ countDocMaster }})
                      <Button
                        icon="pi pi-plus-circle"
                        class="p-button-secondary p-button-text p-0 m-0"
                        v-tooltip.top="{
                          value: 'Gắn văn bản liên quan từ S.Doc',
                        }"
                        v-if="isClose == false"
                        @click="OpenLinkDocToTask()"
                    /></span>
                  </div>
                </div>

                <div
                  class="col-12 w-full"
                  v-if="countDocMaster > 0"
                >
                  <div
                    v-for="(slotProps, index) in ListDocMaster"
                    :key="index"
                    class="col-12 p-0 m-0 pl-5 flex file-hover"
                    v-on:dblclick="ViewFileInfo(slotProps)"
                    v-tooltip.top="{
                      value: 'Nháy chuột 2 lần để xem nhanh văn bản',
                    }"
                  >
                    <div class="col-4 format-left">
                      <div class="">
                        <img
                          :src="
                            basedomainURL +
                              '/Portals/Image/file/' +
                              slotProps.file_type +
                              '.png' ??
                            slotProps.file_name.substring(
                              slotProps.file_name.lastIndexOf('.') + 1,
                            ) + '.png'
                          "
                          style="width: 24px; object-fit: contain"
                          :alt="' '"
                          class="pt-1"
                        />
                      </div>
                      <div
                        class="format-left"
                        style="
                          max-width: 30rem;
                          white-space: nowrap;
                          overflow: hidden;
                          text-overflow: ellipsis;
                        "
                      >
                        <span
                          class="pl-2 w-full"
                          v-tooltip.top="{ value: slotProps.file_name }"
                        >
                          {{ " " + slotProps.file_name }}
                        </span>
                      </div>
                    </div>

                    <div class="col-2 format-center">
                      {{ slotProps.filesize_display }}
                    </div>
                    <div class="col-3 format-center">
                      {{
                        moment(new Date(slotProps.created_date)).format(
                          "HH:mm DD/MM/YYYY",
                        )
                      }}
                    </div>
                    <div class="col-1 format-center">
                      <Avatar
                        @error="
                          $event.target.src =
                            basedomainURL + '/Portals/Image/nouser1.png'
                        "
                        v-tooltip.right="{
                          value: slotProps.creator_tooltip,
                          escape: true,
                        }"
                        v-bind:label="
                          slotProps.avt
                            ? ''
                            : slotProps.full_name
                                .split(' ')
                                .at(-1)
                                .substring(0, 1)
                        "
                        v-bind:image="basedomainURL + slotProps.avt"
                        style="color: #ffffff; cursor: pointer"
                        :style="{
                          background:
                            bgColor[Math.floor(Math.random() * 10) % 7],
                          border:
                            '2px solid' + bgColor[(Math.random() * 11) % 7],
                        }"
                        class="p-0 m-0"
                        size="small"
                        shape="circle"
                      />
                    </div>
                    <div class="flex col-2 format-default ml-3">
                      <a
                        download
                        style="text-decoration: none"
                        class="a-hover format-center"
                      >
                        <Button
                          icon="pi pi-download "
                          class="p-button-text p-button-secondary p-button-hover"
                          v-tooltip="{ value: 'Tải tệp xuống' }"
                          @click="download(slotProps)"
                        >
                        </Button
                      ></a>
                      <a
                        v-if="isClose == false"
                        style="text-decoration: none"
                        class="a-hover format-center"
                      >
                        <Button
                          icon="pi pi-trash"
                          class="p-button-text p-button-secondary p-button-hover"
                          @click="DelLink(slotProps)"
                          v-tooltip="{ value: 'Xóa liên kết' }"
                          v-if="
                            store.state.user.user_id == slotProps.created_by
                          "
                      /></a>
                    </div>
                  </div>
                </div>
              </div>
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
                    <span>Bình luận công việc ({{ countComments }})</span>
                  </div>
                </div>

                <div
                  class="col-12 w-full"
                  v-if="listComments != null"
                >
                  <div
                    class="row col-12 pl-4 w-full cmt-hover relative"
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
                      v-if="user.user_id == cmt.created_by"
                    >
                      <Button
                        icon="pi pi-pencil"
                        class="p-button-raised2 p-button-text"
                        @click="EditComment(cmt)"
                      />
                      <Button
                        icon=" pi pi-trash"
                        class="p-button-raised2 p-button-text"
                        @click="DelComment(cmt.comment_id)"
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
                                : cmt.full_name
                                    .split(' ')
                                    .at(-1)
                                    .substring(0, 1)
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
                            style="
                              font-weight: 700;
                              font-size: 16px;
                              color: #385898;
                            "
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
                            v-if="cmt.parent != null"
                          >
                            <div class="col-12 p-0 m-0">
                              <!-- delete-button-hover -->
                              <div class="col-12 flex p-0 m-0">
                                <div class="format-center">
                                  <Avatar
                                    @error="
                                      $event.target.src =
                                        basedomainURL +
                                        '/Portals/Image/nouser1.png'
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
                                    v-bind:image="
                                      basedomainURL + cmt.parent.avatar
                                    "
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
                                <div
                                  class="col-1 p-0 m-0 text-3xl right-0"
                                ></div>

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
                              class="col-3 py-0 mb-2 h-full relative div-menu-file-hover"
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
                                <div class="col-12 p-0 m-0">
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
                      <div
                        class="row col-12 flex p-0 m-0 pt-1"
                        v-if="isClose == false"
                      >
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
              <div
                class="sticky bottom-0 left-0 w-full bg-white"
                :class="
                  reply == true
                    ? listFileComment.length > 0
                      ? listFileComment.length > 6
                        ? 'h-27rem'
                        : 'h-19rem'
                      : 'h-9rem'
                    : listFileComment.length > 0
                    ? listFileComment.length > 6
                      ? 'h-18rem'
                      : 'h-9rem'
                    : 'h-9rem'
                "
                v-if="listFileComment.length > 0 || reply == true"
              >
                <div class="absolute col-12 h-full bottom-0 p-0 m-0">
                  <div>
                    <div
                      class="col-12 m-0 p-0 w-full px-6 relative"
                      v-if="
                        editComment == true ||
                        reply == true ||
                        (editComment == true && reply == true) ||
                        listFileComment.length > 0
                      "
                    >
                      <div class="left-0 col-12 h-full absolute">
                        <Button
                          icon="pi pi-times-circle"
                          class="p-0 m-0 p-button-rounded p-button-danger p-button-text p-button-plain absolute top-0 right-0 p-button-hover"
                          style="top: 0 !important; right: 0 !important"
                          @click="closeReplyOrEditCmt()"
                          v-tooltip="{ value: 'Hủy' }"
                        >
                        </Button>
                      </div>
                    </div>
                    <div
                      class="col-12 m-0 p-0 w-full px-6"
                      v-if="replyCmtValue != null"
                    >
                      <div
                        class="row col-12 p-0 m-0 px-3 pt-1 w-full cmt-reply"
                        v-for="(cmt, index) in replyCmtValue"
                        :key="index"
                      >
                        <div class="col-12 p-0 m-0 pb-2">
                          <div class="col-12 flex p-0 m-0 pt-1">
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
                                    : cmt.full_name
                                        .split(' ')
                                        .at(-1)
                                        .substring(0, 1)
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
                            <div class="pl-2 col-10 format-left">
                              <span
                                style="
                                  font-weight: 700;
                                  font-size: 16px;
                                  color: #385898;
                                "
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
                            class="row col-12 flex p-0 m-0"
                            v-if="
                              cmt.contents != null &&
                              cmt.contents != '<body><p><br></p></body>'
                            "
                          >
                            <div class="col-1"></div>
                            <div
                              class="col pl-4 p-0 m-0 pr-4 bg-cmt-color border-1 border-round border-blue-100"
                              :style="{
                                height: height1 < 1000 ? '6rem' : '6rem',
                              }"
                              style="
                                white-space: nowrap;
                                overflow: hidden;
                                display: -webkit-box;
                                -webkit-line-clamp: 2;
                                -webkit-box-orient: vertical;
                                text-overflow: ellipsis;
                                line-height: 1;
                                font-size: 15px;
                              "
                              v-html="cmt.contents"
                            ></div>
                            <div class="col-1"></div>
                          </div>
                          <div
                            class="row col-12 flex p-0 m-0"
                            v-else
                          >
                            <div class="col-1"></div>
                            <div
                              class="col pl-4 p-0 m-0 pr-4 bg-cmt-color border-1 border-round border-blue-100 format-center text-black text-4xl"
                              :style="{
                                height: height1 < 1000 ? '6rem' : '6rem',
                              }"
                            >
                              <i class="pi pi-link text-black text-4xl m-2" />
                              Tệp đính kèm
                            </div>
                            <div class="col-1"></div>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div
                      class="col-12 p-0 m-0 font-bold pl-2 bg-white"
                      style="
                        font-weight: bold;
                        font-size: 16px;
                        margin: 10px 0;
                        border-top: 1px solid #f5f5f5;
                        padding-top: 15px;
                        color: #2196f3;
                      "
                      v-if="listFileComment.length > 0"
                    >
                      Tệp đính kèm
                    </div>
                    <div
                      class="col-12 m-0 flex format-center bg-white"
                      v-if="listFileComment.length > 0"
                      style="
                        max-width: 70vw;
                        height: auto;
                        display: flex;
                        flex-wrap: wrap;
                      "
                    >
                      <div
                        v-for="(item, index) in listFileComment"
                        :key="index"
                        class="col-2 relative format-center p-1"
                        style=""
                      >
                        <div class="col-2 p-0 m-0 anh format-center file-hover">
                          <Button
                            @click="
                              delImgComment(item.data ? item.data : item, index)
                            "
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
                                item.src.substring(
                                  item.src.lastIndexOf('.') + 1,
                                ) +
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
              </div>
            </div>
          </ScrollPanel>
          <div
            v-if="isClose == false"
            class="absolute border-1 format-center col-12 p-0 m-0 border-round-xs border-600 flex bottom-0"
            style="border-radius: 5px"
          >
            <div class="border-0 col-9 p-0 m-0">
              <QuillEditor
                ref="comment_zone_main"
                placeholder="Nhập nội dung bình luận..."
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
                  @click="chonanh('anhcongviec')"
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
                  id="anhcongviec"
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

      <reviewTaskVue
        v-if="DanhGiaCongViec == true"
        :id="props.id"
        :task_name="datalists.task_name"
        :member="members"
        :data="datalists"
        :isClose="isClose"
      >
      </reviewTaskVue>

      <div v-if="GiaHanXuLy == true">
        <taskExtendsVue
          :id="props.id"
          :data="datalists"
          :member="members"
          :isClose="isClose"
        ></taskExtendsVue>
      </div>
      <div v-if="QuanLyThanhVien == true">
        <membersVue
          :id="props.id"
          :isType="is_Type"
          :isClose="isClose"
        ></membersVue>
      </div>
      <div v-if="QuanLyTaiLieu == true">
        <taskFileVue
          :id="props.id"
          :psb="PositionSideBar"
          :isClose="isClose"
        ></taskFileVue>
      </div>
      <div v-if="NguoiDaXem == true">
        <viewedMemberVue :id="props.id"></viewedMemberVue>
      </div>
      <div v-if="QuyTrinh == true">
        <Task_FollowVue
          :componentKey="componentKey"
          :id="props.id"
          :pj_id="datalists.project_id"
          :listChild="ListChildTask"
          :member="members"
          :data="datalists"
          :isClose="isClose"
        ></Task_FollowVue>
      </div>
      <div v-if="CongViecCon == true">
        <div class="row col-12">
          <div class="col-12 p-0 m-0">
            <div class="row col-12 p-0 m-0 font-bold text-xl">
              <!-- <i class="pi pi-check-square pr-2"></i> -->
              <div style="float: right">
                <ul
                  id="task-child"
                  style="display: flex; padding: 0px"
                >
                  <li
                    v-if="isClose == false"
                    @click="addLinkTaskOrigin(datalists)"
                    style="list-style: none; margin-right: 20px; color: #0d89ec"
                  >
                    <a style="display: flex; font-size: 12px"
                      ><i
                        style="margin-right: 5px"
                        class="p-custom pi pi-link"
                      ></i>
                      Liên kết công việc con</a
                    >
                  </li>
                  <li
                    v-if="isClose == false"
                    @click="addNewChildTaskOrigin(datalists)"
                    style="list-style: none; margin-right: 20px; color: #0d89ec"
                  >
                    <a style="display: flex; font-size: 12px"
                      ><i
                        style="margin-right: 5px"
                        class="p-custom pi pi-plus-circle"
                      ></i>
                      Tạo công việc con</a
                    >
                  </li>
                </ul>
              </div>
            </div>
          </div>
          <div
            class="col-12 p-0 m-0"
            style="height: 100%; overflow-y: auto"
          >
            <div
              v-if="ListChildTask && ListChildTask.length == 0"
              style="
                text-align: center;
                display: flex;
                flex-direction: column;
                align-items: center;
              "
              class="row col-12 p-0 m-0 pt-1 pb-1 pl-5"
            >
              <img
                style="width: 500px"
                v-bind:src="basedomainURL + '/Portals/Image/noproject.png'"
              />
              <span style="font-size: 20px; font-weight: bold; margin-top: 25px"
                >Hiện chưa có công việc con nào</span
              >
            </div>
            <div v-if="ListChildTask && ListChildTask.length > 0">
              <div
                class="row col-12 p-0 m-0 pt-1 pb-1 pl-5 child-task-hover"
                v-for="(ch, index) in ListChildTask"
                :key="index"
              >
                <div
                  class="row col-12 flex p-0 m-0"
                  @click="show(ch)"
                >
                  <div class="col-7 p-0 m-0">
                    <span class="font-bold text-xl">
                      {{ ch.task_name }}
                    </span>
                    <br />
                    <span>
                      {{ moment(new Date(ch.start_date)).format("DD/MM/YYYY") }}
                    </span>
                    -
                    <span v-if="ch.is_deadline == true">
                      {{ moment(new Date(ch.end_date)).format("DD/MM/YYYY") }}
                    </span>
                  </div>
                  <div class="col-4 p-0 m-0 format-center">
                    <AvatarGroup>
                      <div
                        v-for="(user, index) in ch.users"
                        :key="index"
                      >
                        <Avatar
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                          v-if="user.is_type == 0"
                          v-tooltip.right="{
                            value: user.tooltip,
                            escape: true,
                          }"
                          v-bind:label="
                            user.avt
                              ? ''
                              : user.full_name.split(' ').at(-1).substring(0, 1)
                          "
                          v-bind:image="basedomainURL + user.avt"
                          style="color: #ffffff; cursor: pointer"
                          :style="{
                            background: bgColor[index % 7],
                            border: '2px solid' + bgColor[index % 10],
                          }"
                          class=""
                          size="normal"
                          shape="circle"
                        />
                        <Avatar
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                          v-if="user.is_type == 1"
                          v-tooltip.right="{
                            value: user.tooltip,
                            escape: true,
                          }"
                          v-bind:label="
                            user.avt
                              ? ''
                              : user.full_name.split(' ').at(-1).substring(0, 1)
                          "
                          v-bind:image="basedomainURL + user.avt"
                          style="color: #ffffff; cursor: pointer"
                          :style="{
                            background: bgColor[index % 7],
                            border: '2px solid' + bgColor[index % 10],
                          }"
                          class=""
                          size="normal"
                          shape="circle"
                        />
                        <Avatar
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                          v-if="user.is_type == 2"
                          v-tooltip.right="{
                            value: user.tooltip,
                            escape: true,
                          }"
                          v-bind:label="
                            user.avt
                              ? ''
                              : user.full_name.split(' ').at(-1).substring(0, 1)
                          "
                          v-bind:image="basedomainURL + user.avt"
                          style="color: #ffffff; cursor: pointer"
                          :style="{
                            background: bgColor[index % 7],
                            border: '2px solid' + bgColor[index % 10],
                          }"
                          class=""
                          size="normal"
                          shape="circle"
                        />
                        <Avatar
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                          v-if="user.is_type == 3"
                          v-tooltip.right="{
                            value: user.tooltip,
                            escape: true,
                          }"
                          v-bind:label="
                            user.avt
                              ? ''
                              : user.full_name.split(' ').at(-1).substring(0, 1)
                          "
                          v-bind:image="basedomainURL + user.avt"
                          style="color: #ffffff; cursor: pointer"
                          :style="{
                            background: bgColor[index % 7],
                            border: '2px solid' + bgColor[index % 10],
                          }"
                          class=""
                          size="normal"
                          shape="circle"
                        />
                      </div>
                      <Avatar
                        @error="
                          $event.target.src =
                            basedomainURL + '/Portals/Image/nouser1.png'
                        "
                        v-if="ch.users.length > 4"
                        v-tooltip.right="{
                          value:
                            'và ' +
                            (ch.users.length - 4) +
                            ' người khác tham gia',
                        }"
                        :label="'+' + (ch.users.length - 1)"
                        style="color: #ffffff; cursor: pointer; font-size: 1rem"
                        :style="{
                          background: bgColor[index % 7],
                          border: '2px solid' + bgColor[index % 10],
                        }"
                        class=""
                        size="normal"
                        shape="circle"
                      ></Avatar>
                    </AvatarGroup>
                  </div>
                  <div class="col-1 p-0 m-0 format-center">
                    {{ ch.progress }}%
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- Hoạt động -->
      <div v-if="HoatDong == true">
        <taskActiveVue
          :id="props.id"
          :psb="PositionSideBar"
        ></taskActiveVue>
      </div>
    </div>

    <div class="col-3 p-0 m-0">
      <div class="row flex col-12 p-0 m-0">
        <div :class="'col-12 format-right p-0 m-0 pt-2'">
          <AvatarGroup>
            <div
              v-for="(user, index) in members"
              :key="index"
            >
              <Avatar
                @error="
                  $event.target.src =
                    basedomainURL + '/Portals/Image/nouser1.png'
                "
                v-if="user.is_type == 0 && user.STTGV == 0"
                v-tooltip.left="{
                  value: user.tooltip,
                  escape: true,
                }"
                v-bind:label="
                  user.avt
                    ? ''
                    : user.full_name.split(' ').at(-1).substring(0, 1)
                "
                v-bind:image="basedomainURL + user.avt"
                style="color: #ffffff; cursor: pointer"
                :style="{
                  background: bgColor[index % 7],
                  border: '2px solid' + bgColor[index % 10],
                }"
                class=""
                size="large"
                shape="circle"
              />
              <Avatar
                @error="
                  $event.target.src =
                    basedomainURL + '/Portals/Image/nouser1.png'
                "
                v-if="user.is_type == 1 && user.STTTH == 0"
                v-tooltip.left="{
                  value: user.tooltip,
                  escape: true,
                }"
                v-bind:label="
                  user.avt
                    ? ''
                    : user.full_name.split(' ').at(-1).substring(0, 1)
                "
                v-bind:image="basedomainURL + user.avt"
                style="color: #ffffff; cursor: pointer"
                :style="{
                  background: bgColor[index % 7],
                  border: '2px solid' + bgColor[index % 10],
                }"
                class=""
                size="large"
                shape="circle"
              />
              <Avatar
                @error="
                  $event.target.src =
                    basedomainURL + '/Portals/Image/nouser1.png'
                "
                v-if="user.is_type == 2 && user.STTDTH == 0"
                v-tooltip.left="{
                  value: user.tooltip,
                  escape: true,
                }"
                v-bind:label="
                  user.avt
                    ? ''
                    : user.full_name.split(' ').at(-1).substring(0, 1)
                "
                v-bind:image="basedomainURL + user.avt"
                style="color: #ffffff; cursor: pointer"
                :style="{
                  background: bgColor[index % 7],
                  border: '2px solid' + bgColor[index % 10],
                }"
                class=""
                size="large"
                shape="circle"
              />
              <Avatar
                @error="
                  $event.target.src =
                    basedomainURL + '/Portals/Image/nouser1.png'
                "
                v-if="user.is_type == 3 && user.STTTD == 0"
                v-tooltip.left="{
                  value: user.tooltip,
                  escape: true,
                }"
                v-bind:label="
                  user.avt
                    ? ''
                    : user.full_name.split(' ').at(-1).substring(0, 1)
                "
                v-bind:image="basedomainURL + user.avt"
                style="color: #ffffff; cursor: pointer"
                :style="{
                  background: bgColor[index % 7],
                  border: '2px solid' + bgColor[index % 10],
                }"
                class=""
                size="large"
                shape="circle"
              />
            </div>
            <Avatar
              @error="
                $event.target.src = basedomainURL + '/Portals/Image/nouser1.png'
              "
              v-if="countAllMember > 4"
              v-tooltip.left="{
                value: 'và ' + (members.length - 4) + ' người khác tham gia',
              }"
              :label="'+' + (members.length - 4)"
              style="color: #ffffff; cursor: pointer; font-size: 1rem"
              :style="{
                background: bgColor[4 % 20],
                border: '2px solid' + bgColor[5 % 10],
              }"
              class=""
              size="large"
              shape="circle"
              @click="GoToMemberView(1)"
            ></Avatar>
          </AvatarGroup>
        </div>
      </div>

      <div class="row col-12 flex">
        <div
          v-if="datalists.is_deadline == true && datalists.status == 1"
          class="col-12 p-button-warning format-center font-bold py-3 my-1 text-xl border-round flex"
          style="background-color: #fffbd8; color: #857a1f"
        >
          <i
            class="pi pi-clock pr-2"
            v-if="TimeToDo != 'Chưa bắt đầu'"
          /><span
            class="flex"
            v-html="TimeToDo"
          ></span>
        </div>
      </div>
      <ScrollPanel
        style="width: 100%; height: 86vh"
        class="custombar2 pl-2"
      >
        <div
          class="row col-12 p-0 m-0 py-2 my-1 pl-2"
          v-if="
            store.state.user.user_id == datalists.created_by ||
            memberType == 0 ||
            memberType1 == 0 ||
            memberType2 == 0 ||
            memberType3 == 0
          "
        >
          <div
            class="col-12 p-0 m-0"
            style=""
          >
            <Button
              icon=" pi pi-caret-down"
              iconPos="right"
              :label="'Công việc: ' + datalists.statuss"
              class="font-bold w-full py-3 text-left"
              aria-haspopup="true"
              aria-controls="overlay_menu"
              @click="toggle"
              type="button"
              :style="{ 'background-color': datalists.bgColor }"
            />
            <Menu
              id="overlay_menu"
              ref="menu"
              :model="items"
              :popup="true"
            >
              <template #item="{ item }">
                <div
                  class="menu-hover text-xl w-full p-2"
                  @click="ChangeStatusTask(item.code)"
                >
                  <span>
                    <i
                      class="pi pi-circle"
                      style="
                        border: 1px hidden #ffffff;
                        border-radius: 50%;
                        color: #ffffff;
                        border-style: hidden;
                      "
                      :style="{ background: item.color }"
                    />
                    {{ item.label }}
                  </span>
                </div>
              </template>
            </Menu>
          </div>
        </div>
        <div class="row col-12 p-0 m-0 py-2 my-1 pl-1">
          <div
            class="col-12 p-0 m-0"
            style=""
          >
            <Button
              icon="p-custom pi pi-info-circle"
              label="Thông tin chung"
              class="p-button-raised p-button-text w-full py-3 text-left"
              @click="Switch('1')"
              :class="ThongTinChung == true ? 'activated' : ''"
            />
          </div>
        </div>

        <div
          class="row col-12 p-0 m-0 py-2 my-1 pl-1"
          v-if="
            isClose == false &&
            (store.state.user.user_id == datalists.created_by ||
              memberType == 0 ||
              memberType1 == 0 ||
              memberType2 == 0 ||
              memberType3 == 0)
          "
        >
          <div
            class="col-12 p-0 m-0"
            style=""
          >
            <Button
              icon="p-custom pi pi-pencil"
              label="Chỉnh sửa công việc"
              class="p-button-raised p-button-text w-full py-3 text-left"
              @click="EditTask(datalists)"
            />
          </div>
        </div>
        <div class="row col-12 p-0 m-0 py-2 my-1 pl-1">
          <div
            class="col-12 p-0 m-0"
            style="position: relative"
          >
            <Button
              class="p-button-raised p-button-text w-full py-3 text-left"
              @click="Switch('2')"
              :class="DanhGiaCongViec == true ? 'activated' : ''"
            >
              <font-awesome-icon
                icon="fa-solid fa-clipboard-check"
                class="p-custom"
                style="padding-right: 0.75rem"
              />
              <span
                v-if="
                  memberType == 0 ||
                  memberType1 == 0 ||
                  memberType2 == 0 ||
                  memberType3 == 0
                "
              >
                Đánh giá công việc
              </span>
              <span v-else> Báo cáo công việc </span>
            </Button>
            <span
              v-if="newReport > 0"
              style="
                position: absolute;
                top: 0px;
                right: 0px;
                height: 25px;
                width: 25px;
                background-color: red;
                color: #fff;
                border-radius: 50%;
                text-align: center;
                padding-top: 5px;
                font-size: 11px;
                font-weight: bold;
              "
              >{{ newReport }}</span
            >
          </div>
        </div>

        <div
          class="row col-12 p-0 m-0 py-2 my-1 pl-1"
          v-if="datalists.is_deadline == true"
        >
          <div
            class="col-12 p-0 m-0"
            style="position: relative"
          >
            <Button
              class="p-button-raised p-button-text w-full py-3 text-left"
              @click="Switch('3')"
              :class="GiaHanXuLy == true ? 'activated' : ''"
            >
              <font-awesome-icon
                icon="fa-solid fa-user-clock"
                class="p-custom"
                style="padding-right: 0.75rem"
              />
              Gia hạn xử lý
            </Button>
            <span
              v-if="countExtend > 0"
              style="
                position: absolute;
                top: 0px;
                right: 0px;
                height: 25px;
                width: 25px;
                background-color: red;
                color: #fff;
                border-radius: 50%;
                text-align: center;
                padding-top: 5px;
                font-size: 11px;
                font-weight: bold;
              "
              >{{ countExtend }}</span
            >
          </div>
        </div>
        <div class="row col-12 p-0 m-0 py-2 my-1 pl-1">
          <div
            class="col-12 p-0 m-0"
            style="position: relative"
          >
            <Button
              icon="p-custom pi pi-list"
              label="Công việc con"
              class="p-button-raised p-button-text w-full py-3 text-left"
              @click="Switch('4')"
              :class="CongViecCon == true ? 'activated' : ''"
            />
            <span
              v-if="ListChildTask && ListChildTask.length > 0"
              style="
                position: absolute;
                top: 0px;
                right: 0px;
                height: 25px;
                width: 25px;
                background-color: red;
                color: #fff;
                border-radius: 50%;
                text-align: center;
                padding-top: 5px;
                font-size: 11px;
                font-weight: bold;
              "
              >{{ ListChildTask.length }}</span
            >
          </div>
        </div>
        <div class="row col-12 p-0 m-0 py-2 my-1 pl-1">
          <div
            class="col-12 p-0 m-0"
            style="position: relative"
          >
            <Button
              icon="p-custom pi  pi-sync"
              label="Thiết lập quy trình"
              class="p-button-raised p-button-text w-full py-3 text-left"
              @click="Switch('11')"
              :class="QuyTrinh == true ? 'activated' : ''"
            />
          </div>
        </div>
        <div class="row col-12 p-0 m-0 py-2 my-1 pl-1">
          <div
            class="col-12 p-0 m-0"
            style=""
          >
            <Button
              icon="p-custom pi pi-users"
              label="Quản lý thành viên"
              class="p-button-raised p-button-text w-full py-3 text-left"
              @click="Switch('5')"
              :class="QuanLyThanhVien == true ? 'activated' : ''"
            />
          </div>
        </div>

        <div class="row col-12 p-0 m-0 py-2 my-1 pl-1">
          <div
            class="col-12 p-0 m-0"
            style=""
          >
            <Button
              icon="p-custom pi pi-folder"
              label="Quản lý tài liệu"
              class="p-button-raised p-button-text w-full py-3 text-left"
              @click="Switch('6')"
              :class="QuanLyTaiLieu == true ? 'activated' : ''"
            />
          </div>
        </div>
        <div class="row col-12 p-0 m-0 py-2 my-1 pl-1">
          <div
            class="col-12 p-0 m-0"
            style=""
          >
            <Button
              icon="p-custom pi pi-history"
              label="Hoạt động"
              class="p-button-raised p-button-text w-full py-3 text-left"
              @click="Switch('7')"
              :class="HoatDong == true ? 'activated' : ''"
            />
          </div>
        </div>
        <div class="row col-12 p-0 m-0 py-2 my-1 pl-1">
          <div
            class="col-12 p-0 m-0"
            style=""
          >
            <Button
              class="p-button-raised p-button-text w-full py-3 text-left"
              @click="Switch('8')"
              :class="NguoiDaXem == true ? 'activated' : ''"
            >
              <font-awesome-icon
                icon="fa-solid fa-user-check"
                class="p-custom"
                style="padding-right: 0.75rem"
              />
              Người đã xem
            </Button>
          </div>
        </div>
        <!-- <div class="row col-12 p-0 m-0 py-2 my-1 pl-1">
              <div class="col-12 p-0 m-0" style="">
                <Button
                  icon="p-custom pi pi-tags"
                  label="Quản lý Tags"
                  class="p-button-raised p-button-text w-full py-3 text-left"
                />
              </div>
            </div>
            <div class="row col-12 p-0 m-0 py-2 my-1 pl-1">
              <div class="col-12 p-0 m-0" style="">
                <Button
                  icon="p-custom pi pi-shield"
                  label="Phân quyền"
                  class="p-button-raised p-button-text w-full py-3 text-left"
                  @click="Switch('9')"
                  :class="PhanQuyen == true ? 'activated' : ''"
                />
              </div>
            </div> -->
        <!-- <div class="row col-12 p-0 m-0 py-2 my-1 pl-1">
              <div class="col-12 p-0 m-0" style="">
                <Button
                  icon="p-custom pi pi-chart-line"
                  label="Thống kê"
                  class="p-button-raised p-button-text w-full py-3 text-left"
                  @click="Switch('10')"
                  :class="ThongKe == true ? 'activated' : ''"
                />
              </div>
            </div> -->
        <div
          class="row col-12 p-0 m-0 py-2 my-1 pl-1"
          v-if="
            datalists.status != 3 &&
            isClose == false &&
            (store.state.user.user_id == datalists.created_by ||
              memberType == 0 ||
              memberType1 == 0 ||
              memberType2 == 0 ||
              memberType3 == 0)
          "
        >
          <div
            class="col-12 p-0 m-0"
            style=""
          >
            <Button
              icon="p-custom pi pi-lock"
              label="Đóng công việc"
              class="p-button-raised p-button-text w-full py-3 text-left"
              @click="ChangeStatusTask(3)"
            />
          </div>
        </div>
        <div
          class="row col-12 p-0 m-0 py-2 my-1 pl-1"
          v-if="
            store.state.user.user_id == datalists.created_by ||
            memberType == 0 ||
            memberType1 == 0 ||
            memberType2 == 0 ||
            memberType3 == 0
          "
        >
          <div
            class="col-12 p-0 m-0"
            style=""
          >
            <Button
              icon="p-custom pi pi-trash"
              label="Xóa công việc"
              class="p-button-raised p-button-text p-button-danger w-full py-3 text-left p-danger-hover"
              style="color: red !important; background: #f5f5f5"
              @click="DelTask(datalists)"
            />
          </div>
        </div>
      </ScrollPanel>
    </div>
  </div>
  <div
    class="overflow-hidden w-full"
    style="
      display: grid;
      align-content: center;
      justify-content: center;
      align-items: center;
      justify-items: center;
      height: 98vh;
    "
    v-else
  >
    <img
      src="../../assets/background/nodata.png"
      height="300"
    />
    <h2 class="m-1">Công việc bảo mật, đã bị xóa hoặc không tồn tại.</h2>
  </div>
  <!-- //OverlayPanel -->
  <OverlayPanel
    class="p-0"
    ref="panelEmoij1"
    append-to="body"
    :show-close-icon="false"
    id="overlay_panelEmoij1"
  >
    <VuemojiPicker @emojiClick="handleEmojiClick" />
  </OverlayPanel>
  <Dialog
    :header="headerDialog"
    v-model:visible="openDialog"
    :style="{ width: '40vw' }"
    :closable="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 format-center">
          <label class="col-3 text-left p-0"
            >Tên Checklist <span class="redsao">(*)</span></label
          >
          <InputText
            id="textbox"
            v-model="checkList.checklist_name"
            spellcheck="false"
            class="col-9 ip36 px-2"
            :class="{
              'p-invalid':
                (v$.checklist_name.$invalid && submitted) ||
                textboxLength > 500,
            }"
            @change="focusInput()"
          />
        </div>

        <div
          style="display: flex"
          class="field col-12 md:col-12 px-0"
          v-if="textboxLength > 500"
        >
          <div class="col-3 text-left"></div>
          <small class="col-9 p-error">
            <span class="col-12 p-0">Tên checklist không quá 500 kí tự!</span>
          </small>
        </div>
        <div
          style="display: flex"
          class="field col-12 md:col-12 px-0"
          v-if="
            (v$.checklist_name.$invalid && submitted) ||
            v$.checklist_name.$pending.$response
          "
        >
          <div class="col-3 text-left"></div>
          <small class="col-9 p-error">
            <span class="col-12 p-0">{{
              v$.checklist_name.required.$message
                .replace("Value", "Tên checklist")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div class="field col-12 md:col-12 format-center">
          <label class="col-3 text-left p-0">Mô tả </label>
          <InputText
            id="descript"
            v-model="checkList.description"
            spellcheck="false"
            class="col-9 ip36 px-2"
            @change="focusInput2()"
            :class="{
              'p-invalid': textboxLength2 > 500,
            }"
          />
        </div>
        <div
          style="display: flex"
          class="field col-12 md:col-12 px-0"
          v-if="textboxLength2 > 500"
        >
          <div class="col-3 text-left"></div>
          <small class="col-9 p-error">
            <span class="col-12 p-0">Mô tả không quá 500 kí tự!</span>
          </small>
        </div>

        <div
          style="display: flex"
          class="col-12 field md:col-12"
        >
          <div class="field col-6 md:col-6 p-0">
            <label class="col-6 text-left p-0">STT </label>

            <InputNumber
              v-model="checkList.is_order"
              class="col-6 ip36 p-0"
            />
          </div>
          <!-- <div class="field col-6 md:col-6 p-0 flex align-items-center">
            <label class="col-6 text-center p-0">Trạng thái </label>
            <InputSwitch
              v-model="checkList.status"
              style="justify-content: center; align-items: center"
            />
          </div> -->
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog()"
        class="p-button-text"
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveData(!v$.$invalid)"
      />
    </template>
  </Dialog>
  <Dialog
    :header="headerDialog1"
    v-model:visible="openDialog1"
    :style="{ width: '20vw' }"
    :closable="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 p-0 align-items-center">
          <label class="col-6 p-0 m-0">Ngày bắt đầu (Dự kiến) </label>
          <Calendar
            inputId="icon"
            v-model="Task.start_date"
            :showIcon="true"
            class="col-6 p-0 m-0"
            dateFormat="dd/mm/yy"
            :maxDate="new Date(maxDate)"
            :minDate="new Date(minDate)"
          />
        </div>
        <div class="field col-12 p-0 align-items-center">
          <label class="col-6 p-0 m-0">Ngày kết thúc (Dự kiến) </label>
          <Calendar
            inputId="icon"
            v-model="Task.end_date"
            :showIcon="true"
            class="col-6 p-0 m-0"
            dateFormat="dd/mm/yy"
            :maxDate="new Date(maxDate)"
            :minDate="new Date(minDate)"
          />
        </div>
        <div class="field col-12 p-0 align-items-center">
          <label class="col-6 p-0 m-0">Ngày hoàn thành </label>
          <Calendar
            inputId="icon"
            v-model="Task.close_date"
            :showIcon="true"
            class="col-6 p-0 m-0"
          />
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialog()"
        class="p-button-text"
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveTaskCheckList(!v1$.$invalid, $event)"
      />
    </template>
  </Dialog>

  <Dialog
    :header="headerAddTask"
    style="z-index: 10"
    v-model:visible="displayTask"
    :closable="true"
    :maximizable="true"
    :style="{ width: '700px' }"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0"
            >Tên công việc<span class="redsao"> (*) </span></label
          >
          <InputText
            v-model="Task.task_name"
            spellcheck="false"
            class="col-9 ip36 px-2"
            :class="{ 'p-invalid': v3$.task_name.$invalid && sbm }"
          />
        </div>
        <div
          style="display: flex"
          class="field col-12 md:col-12"
        >
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (v3$.task_name.$invalid && sbm) ||
              v3$.task_name.$pending.$response
            "
            class="col-9 p-error p-0"
          >
            <span class="col-12 p-0">{{
              v3$.task_name.required.$message
                .replace("Value", "Tên công việc")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <!-- update tráng -->
        <div
          class="field col-12 md:col-12"
          style="position: relative"
        >
          <label class="col-3 text-left p-0">Công việc của phòng</label>
          <InputSwitch
            @change="ChangeIsDepartment(Task.is_department)"
            class="col-6"
            style="position: absolute; top: 0px; left: 200px"
            v-model="Task.is_department"
          />
          <!-- <TreeSelect class="col-9" v-model="selectcapcha" :options="listDropdownorganization" :showClear="true"
            :max-height="200" placeholder="" optionLabel="organization_name" optionValue="department_id"
            @change="ChangeTaskDepartment()" /> -->
        </div>
        <div
          class="field col-12 md:col-12"
          v-if="Task.is_department"
        >
          <label class="col-3 text-left p-0">Phòng ban</label>
          <TreeSelect
            class="col-9"
            v-model="selectcapcha"
            :options="listDropdownorganization"
            :showClear="true"
            :max-height="200"
            placeholder=""
            optionLabel="organization_name"
            optionValue="department_id"
            @change="ChangeTaskDepartment()"
          />
        </div>
        <!-- end -->
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0">Thuộc dự án</label>
          <Dropdown
            :filter="true"
            v-model="Task.project_id"
            panelClass="d-design-dropdown"
            selectionLimit="1"
            :options="listDropdownProject"
            optionLabel="project_name"
            optionValue="project_id"
            spellcheck="false"
            class="col-9 ip36 p-0"
          >
            <template #option="slotProps">
              <div class="country-item flex">
                <div class="pt-1">{{ slotProps.option.project_name }}</div>
              </div>
            </template>
          </Dropdown>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0"
            >Người giao việc
            <span
              @click="OpenDialogTreeUser(true, 1)"
              class="choose-user"
              ><i class="pi pi-user-plus"></i></span
            ><span class="redsao"> (*) </span></label
          >
          <MultiSelect
            :filter="true"
            v-model="Task.assign_user_id"
            :options="listDropdownUser"
            optionValue="code"
            optionLabel="name"
            class="col-9 ip36 p-0"
            placeholder="Người giao việc"
            @change="changeNguoiGaoViec($event)"
            :class="{
              'p-invalid': Task.assign_user_id.length == 0 && sbm,
            }"
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
        <div
          v-if="!Task.is_department"
          class="field col-12 md:col-12"
        >
          <label class="col-3 text-left p-0"
            >Người thực hiện
            <span
              @click="OpenDialogTreeUser(false, 2)"
              class="choose-user"
              ><i class="pi pi-user-plus"></i></span
            ><span class="redsao"> (*) </span></label
          >
          <MultiSelect
            :filter="true"
            v-model="Task.work_user_ids"
            :options="listDropdownUser"
            optionValue="code"
            optionLabel="name"
            class="col-9 ip36 p-0"
            placeholder="Người thực hiện"
            :class="{
              'p-invalid': Task.work_user_ids.length == 0 && sbm,
            }"
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
        <div
          v-if="!Task.is_department"
          style="display: flex"
          class="field col-12 md:col-12"
        >
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (v3$.work_user_ids.$invalid && sbm) ||
              v3$.work_user_ids.$pending.$response
            "
            class="col-9 p-error p-0"
          >
            <span class="col-12 p-0">{{
              v3$.work_user_ids.required.$message
                .replace("Value", "Người thực hiện")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div
          v-if="!Task.is_department"
          class="field col-12 md:col-12"
        >
          <label class="col-3 text-left p-0"
            >Người đồng thực hiện
            <span
              @click="OpenDialogTreeUser(false, 3)"
              class="choose-user"
              ><i class="pi pi-user-plus"></i></span
          ></label>
          <MultiSelect
            :filter="true"
            v-model="Task.works_user_ids"
            :options="listDropdownUser"
            optionValue="code"
            optionLabel="name"
            class="col-9 ip36 p-0"
            display="chip"
          >
            <template #option="slotProps">
              <div
                class="country-item flex"
                style="align-items: center; padding-left: 10px"
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
        <div
          v-if="!Task.is_department"
          class="field col-12 md:col-12"
        >
          <label class="col-3 text-left p-0"
            >Người theo dõi
            <span
              @click="OpenDialogTreeUser(false, 4)"
              class="choose-user"
              ><i class="pi pi-user-plus"></i></span
          ></label>
          <MultiSelect
            :filter="true"
            v-model="Task.follow_user_ids"
            :options="listDropdownUser"
            optionValue="code"
            optionLabel="name"
            class="col-9 ip36 p-0"
            display="chip"
          >
            <template #option="slotProps">
              <div
                class="country-item flex"
                style="align-items: center; padding-left: 10px"
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
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0">Nhóm</label>
          <Dropdown
            :filter="true"
            v-model="Task.group_id"
            panelClass="d-design-dropdown"
            selectionLimit="1"
            :options="listDropdownTaskGroup"
            optionLabel="group_name"
            optionValue="group_id"
            spellcheck="false"
            class="col-9 ip36 p-0"
          >
            <template #option="slotProps">
              <div class="country-item flex">
                <div class="pt-1">{{ slotProps.option.group_name }}</div>
              </div>
            </template>
          </Dropdown>
        </div>
        <div
          class="field col-12 md:col-12"
          style="display: flex"
        >
          <div class="col-3"></div>
          <div
            class="col-9"
            style="display: flex"
          >
            <div class="col-5">
              <Checkbox
                style="margin-right: 5px"
                v-model="Task.is_review"
                :binary="true"
              />
              YC đánh giá công việc
            </div>
            <div class="col-4">
              <Checkbox
                style="margin-right: 5px"
                v-model="Task.is_deadline"
                :binary="true"
              />
              Có hạn xử lý
            </div>
            <div class="col-3">
              <Checkbox
                style="margin-right: 5px"
                v-model="Task.is_prioritize"
                :binary="true"
              />
              Ưu tiên
            </div>
          </div>
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
              class="col-5 ip36 title-lable"
              style="margin-top: 5px; padding: 0px"
              id="time1"
              autocomplete="on"
              :showTime="true"
              v-model="Task.start_date"
            />
            <div
              class="col-7"
              style="display: flex; padding: 0px; align-items: center"
            >
              <label class="col-5 text-center">Ngày kết thúc</label>
              <Calendar
                :manualInput="true"
                :showIcon="true"
                class="col-7 ip36 title-lable"
                style="margin-top: 5px; padding: 0px"
                id="time2"
                placeholder="dd/MM/yy"
                autocomplete="on"
                v-model="Task.end_date"
                :showTime="true"
                :class="{
                  'p-invalid': v3$.end_date.$invalid && sbm && Task.is_deadline,
                }"
                @date-select="CheckDate($event)"
              />
            </div>
          </div>
        </div>
        <div
          style="display: flex"
          class="field col-12 md:col-12"
        >
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (v3$.end_date.$invalid && sbm && Task.is_deadline) ||
              (v3$.end_date.$pending.$response && Task.is_deadline)
            "
            class="col-9 p-error p-0"
          >
            <span class="col-12 p-0">{{
              v3$.end_date.required.$message
                .replace("Value", "Ngày kết thúc")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>
        <div
          class="field col-12 md:col-12"
          style="display: flex; align-items: center"
        >
          <label class="col-3 text-left p-0">STT</label>
          <div
            class="col-9"
            style="display: flex; padding: 0px; align-items: center"
          >
            <InputText
              style="margin-top: 5px"
              v-model="Task.is_order"
              spellcheck="false"
              class="col-4 ip36 px-2"
            />
            <div
              class="col-8"
              style="
                display: flex;
                padding: 0px;
                align-items: center;
                position: relative;
              "
            >
              <label class="col-6 text-center">Kích hoạt bảo mật</label>
              <InputSwitch
                class="col-6"
                style="position: absolute; top: 0px; left: 200px"
                v-model="Task.is_security"
              />
            </div>
          </div>
        </div>
        <div class="field col-12 md:col-12">
          <label class="col-3 text-left p-0">Trạng thái công việc</label>
          <Dropdown
            :filter="true"
            style="margin-top: 5px"
            panelClass="d-design-dropdown"
            v-model="Task.status"
            :options="listDropdownStatus"
            optionLabel="text"
            optionValue="value"
            placeholder="Trạng thái công việc"
            spellcheck="false"
            class="col-9 ip36 p-0"
          >
            <template #option="slotProps">
              <div class="country-item flex">
                <div class="pt-1">{{ slotProps.option.text }}</div>
              </div>
            </template>
          </Dropdown>
        </div>
        <div
          class="field col-12 md:col-12"
          style="
            display: flex;
            /* padding: 0px; */
            align-items: center;
            position: relative;
          "
        >
          <label class="col-3 text-left p-0">Xuất XML</label>
          <InputSwitch
            class="col-9"
            style="position: absolute; top: 0px; left: 160px"
            v-model="Task.is_XML"
          />
        </div>
        <div class="field col-12 md:col-12">
          <Accordion :multiple="true">
            <AccordionTab header="THÔNG TIN KHÁC">
              <div
                v-if="Task.is_department"
                class="field col-12 md:col-12"
              >
                <label class="col-3 text-left p-0"
                  >Người thực hiện
                  <span
                    @click="OpenDialogTreeUser(false, 2)"
                    class="choose-user"
                    ><i class="pi pi-user-plus"></i></span
                ></label>
                <MultiSelect
                  :filter="true"
                  v-model="Task.work_user_ids"
                  :options="listDropdownUser"
                  optionValue="code"
                  optionLabel="name"
                  class="col-9 ip36 p-0"
                  placeholder="Người thực hiện"
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
                          background:
                            bgColor[slotProps.index % 7] + '!important',
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
              <div
                v-if="Task.is_department"
                class="field col-12 md:col-12"
              >
                <label class="col-3 text-left p-0"
                  >Người đồng thực hiện
                  <span
                    @click="OpenDialogTreeUser(false, 3)"
                    class="choose-user"
                    ><i class="pi pi-user-plus"></i></span
                ></label>
                <MultiSelect
                  :filter="true"
                  v-model="Task.works_user_ids"
                  :options="listDropdownUser"
                  optionValue="code"
                  optionLabel="name"
                  class="col-9 ip36 p-0"
                  display="chip"
                >
                  <template #option="slotProps">
                    <div
                      class="country-item flex"
                      style="align-items: center; padding-left: 10px"
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
                          background:
                            bgColor[slotProps.index % 7] + '!important',
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
              <div
                v-if="Task.is_department"
                class="field col-12 md:col-12"
              >
                <label class="col-3 text-left p-0"
                  >Người theo dõi
                  <span
                    @click="OpenDialogTreeUser(false, 4)"
                    class="choose-user"
                    ><i class="pi pi-user-plus"></i></span
                ></label>
                <MultiSelect
                  :filter="true"
                  v-model="Task.follow_user_ids"
                  :options="listDropdownUser"
                  optionValue="code"
                  optionLabel="name"
                  class="col-9 ip36 p-0"
                  display="chip"
                >
                  <template #option="slotProps">
                    <div
                      class="country-item flex"
                      style="align-items: center; padding-left: 10px"
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
                          background:
                            bgColor[slotProps.index % 7] + '!important',
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
                <label class="col-3 text-left p-0">Chọn trọng số</label>
                <Dropdown
                  :filter="true"
                  v-model="Task.weight"
                  panelClass="d-design-dropdown"
                  selectionLimit="1"
                  :options="listDropdownweight"
                  optionLabel="weight_name"
                  optionValue="weight_id"
                  spellcheck="false"
                  class="col-9 ip36 p-0"
                >
                  <template #option="slotProps">
                    <div class="country-item flex">
                      <div class="pt-1">{{ slotProps.option.weight_name }}</div>
                    </div>
                  </template>
                </Dropdown>
              </div>
              <div
                class="field col-12 md:col-12"
                style="display: flex; align-items: center"
              >
                <label class="col-3 text-left p-0">Mô tả</label>
                <Textarea
                  style="margin-top: 5px; padding: 5px; min-height: 100px"
                  v-model="Task.description"
                  class="col-9 ip36"
                  :autoResize="true"
                  rows="5"
                  cols="30"
                />
              </div>
              <div
                class="field col-12 md:col-12"
                style="display: flex; align-items: center"
              >
                <label class="col-3 text-left p-0">Mục tiêu</label>
                <Textarea
                  style="margin-top: 5px; padding: 5px; min-height: 100px"
                  v-model="Task.target"
                  class="col-9 ip36"
                  :autoResize="true"
                  rows="5"
                  cols="30"
                />
              </div>
              <div
                class="field col-12 md:col-12"
                style="display: flex; align-items: center"
              >
                <label class="col-3 text-left p-0">Khó khăn vướng mắc</label>
                <Textarea
                  style="margin-top: 5px; padding: 5px; min-height: 100px"
                  v-model="Task.difficult"
                  class="col-9 ip36"
                  :autoResize="true"
                  rows="5"
                  cols="30"
                />
              </div>
              <div
                class="field col-12 md:col-12"
                style="display: flex; align-items: center"
              >
                <label class="col-3 text-left p-0">Đề xuất</label>
                <Textarea
                  style="margin-top: 5px; padding: 5px; min-height: 50px"
                  v-model="Task.request"
                  class="col-9 ip36"
                  :autoResize="true"
                  rows="5"
                  cols="30"
                />
              </div>
              <div
                class="field col-12 md:col-12"
                id="task_file"
                style="display: flex"
              >
                <label class="col-3 text-left p-0">File</label>
                <div class="col-9 p-0">
                  <FileUpload
                    chooseLabel="Chọn File"
                    style="margin-top: 5px !important"
                    :showUploadButton="false"
                    :showCancelButton="false"
                    :multiple="true"
                    accept=""
                    :maxFileSize="10000000"
                    @select="onUploadFile"
                    @remove="removeFile"
                  />
                  <div
                    class="col-12 p-0"
                    style="border: 1px solid #e1e1e1; margin-top: -1px"
                    v-if="Task.files.length > 0 && !isAdd"
                  >
                    <DataView
                      :lazy="true"
                      :value="Task.files"
                      :rowHover="true"
                      :scrollable="true"
                      class="w-full h-full ptable p-datatable-sm flex flex-column col-10 ip36 p-0"
                      layout="list"
                      responsiveLayout="scroll"
                    >
                      <template #list="slotProps">
                        <Toolbar
                          class="w-full"
                          style="display: flex"
                        >
                          <template #start>
                            <div class="flex align-items-center task-file-list">
                              <img
                                class="mr-2"
                                :src="
                                  basedomainURL +
                                  '/Portals/Image/file/' +
                                  slotProps.data.file_type +
                                  '.png'
                                "
                                style="object-fit: contain"
                                width="40"
                                height="40"
                              />
                              <span
                                style="line-height: 1.5; word-break: break-all"
                              >
                                {{ slotProps.data.file_name }}</span
                              >
                            </div>
                          </template>
                          <template #end>
                            <Button
                              icon="pi pi-times"
                              class="p-button-rounded p-button-danger"
                              @click="deleteFile(slotProps.data)"
                            />
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
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogTask"
        class="p-button-text"
      />

      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveTask(!v3$.$invalid)"
      />
    </template>
  </Dialog>

  <Dialog
    header="Tải lên tệp tài liệu"
    v-model:visible="openFileDialog"
    :style="{ width: '40vw' }"
    :closable="true"
  >
    <form>
      <FileUpload
        @remove="removefilesTaiLieu"
        @select="selectfilesTaiLieu"
        :show-upload-button="false"
        choose-label="Chọn tệp"
        cancel-label="Hủy"
        :multiple="true"
        :maxFileSize="104857600"
        :invalidFileSizeMessage="'Tệp tải lên không quá 100MB!'"
      >
        <template #empty>
          <p>Kéo và thả tệp vào đây để tải lên.</p>
        </template>
      </FileUpload>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeFileUpload"
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="Upload"
      />
    </template>
    <!-- Chức năng đang chỉnh sửa vui lòng liên hệ quản trị viên phần mềm -->
  </Dialog>
  <Dialog
    :header="headerStatusTask"
    v-model:visible="openStatusTask"
    :style="{ width: '30vw' }"
    :closable="true"
  >
    <div
      style="display: flex"
      class="field col-12 md:col-12 px-0"
    >
      <small
        v-if="
          (validateStatusTask.end_date.$invalid && sbmStatusTask) ||
          validateStatusTask.end_date.$pending.$response
        "
        class="col-12 p-error p-0 m-0"
      >
        <span class="col-12 p-0">{{
          validateStatusTask.end_date.required.$message
            .replace("Value", "Ngày hoàn thành")
            .replace("is required", "không được để trống")
        }}</span>
      </small>
    </div>
    <Calendar
      v-model="end_date.end_date"
      :showIcon="false"
      class="w-full"
      placeholder="Thời gian hoàn thành"
      :minDate="new Date(minDate)"
      :class="{
        'p-invalid': validateStatusTask.end_date.$invalid && sbmStatusTask,
      }"
    />
    <Calendar
      inputId="icon"
      v-model="end_date.end_date"
      class="w-full h-full"
      placeholder="Thời gian hoàn thành"
      :minDate="new Date(minDate)"
      :inline="true"
    />
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeStatusTask()"
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="
          UpdateStatusTaksFunc(
            end_date.stt,
            end_date.end_date,
            !validateStatusTask.$invalid,
          )
        "
      />
    </template>
    <!-- Chức năng đang chỉnh sửa vui lòng liên hệ quản trị viên phần mềm -->
  </Dialog>

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
  <TaskCheckListDetailVue
    :id="props.id"
    :member="members"
    :type="ChecklistType"
    :data="datalists"
    :weight="listDropdownweight"
    :isClose="isClose"
    v-if="ViewTaskChecklists"
  ></TaskCheckListDetailVue>

  <DocLinkTaskVue
    v-if="openDocDialog == true"
    :visible="openDocDialog"
    :id="props.id"
    :header="LinkDoc"
    :main="datalists.parent_id != null ? true : false"
  ></DocLinkTaskVue>
  <Sidebar
    v-model:visible="showDetail1"
    position="right"
    :style="{
      width: width1 > 1800 ? ' 65vw' : '75vw',
      'min-height': '100vh !important',
    }"
    :showCloseIcon="false"
  >
    <DetailedChild
      :key="componentKey"
      :isShow="showDetail1"
      :id="selectedTaskID"
      :turn="1"
    >
    </DetailedChild>
  </Sidebar>
  <treeuser
    v-if="displayDialogUser === true"
    :headerDialog="headerDialogUser"
    :displayDialog="displayDialogUser"
    :one="is_one"
    :selected="selectedUser"
    :closeDialog="closeDialogUser"
    :choiceUser="choiceTreeUser"
  />
  <Dialog
    contentClass="task_list"
    :header="headerAddLinkTask"
    v-model:visible="displayLinkTask"
    style="overflow-y: hidden !important"
    :style="{ width: '1000px' }"
    :closable="true"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <div
            v-if="store.getters.islogin"
            style="background-color: #fff !important"
            class="flex-grow-1 p-2"
          >
            <DataTable
              v-model:first="first"
              :rowHover="true"
              :value="listTaskLink"
              :scrollable="true"
              scrollHeight="flex"
              :totalRecords="optionsLinkTask.totalRecords"
              :row-hover="true"
              dataKey="task_id"
              :paginator="true"
              :rows="optionsLinkTask.PageSize"
              paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
              :rowsPerPageOptions="[20, 30, 50, 100, 200]"
              v-model:selection="selectedTasks"
              @page="onPage($event)"
              @sort="onSort($event)"
              @filter="onFilter($event)"
              :lazy="true"
              selectionMode="single"
            >
              <template #header>
                <div class="flex justify-content-center align-items-center">
                  <Toolbar class="w-full custoolbar">
                    <template #start>
                      <span class="p-input-icon-left">
                        <i class="pi pi-search" />
                        <InputText
                          style="min-width: 300px"
                          type="text"
                          spellcheck="false"
                          v-model="optionsLinkTask.search"
                          placeholder="Tìm kiếm"
                          @keyup.enter="LoadLinkTaskOrigin(datalists)"
                        />
                      </span>
                    </template>
                  </Toolbar>
                </div>
              </template>
              <Column
                headerStyle="text-align:center;max-width:4rem;min-height:3.125rem"
                bodyStyle="text-align:center;max-width:4rem;"
                class="align-items-center justify-content-center text-center"
              >
                <template #body="md">
                  <div style="display: flex; align-items: center">
                    <Checkbox
                      v-model="md.data.is_check"
                      :binary="true"
                    />
                  </div>
                </template>
              </Column>
              <Column
                headerStyle="text-align:center;max-width:50px;min-height:3.125rem"
                bodyStyle="text-align:center;max-width:50px; "
                class="align-items-center justify-content-center text-center"
              >
                <template #body="value">
                  <Avatar
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/nouser1.png'
                    "
                    :key="index"
                    v-tooltip.bottom="{
                      value:
                        value.data.full_name +
                        '<br/>' +
                        (value.data.tenChucVu || '') +
                        '<br/>' +
                        (value.data.tenToChuc || ''),
                      escape: true,
                    }"
                    v-bind:label="
                      value.data.avatar
                        ? ''
                        : (value.data.last_name ?? '').substring(0, 1)
                    "
                    v-bind:image="basedomainURL + value.data.avatar"
                    style="
                      background-color: #2196f3;
                      color: #ffffff;
                      width: 2.5rem;
                      height: 2.5rem;
                      font-size: 15px !important;
                    "
                    :style="{
                      background: bgColor[0] + '!important',
                    }"
                    class="cursor-pointer"
                    size="xlarge"
                    shape="circle"
                  />
                </template>
              </Column>
              <Column
                header="Tên công việc"
                headerStyle="min-height:3.125rem"
                bodyStyle=" "
              >
                <template #body="data">
                  <div
                    style="display: flex; flex-direction: column; padding: 5px"
                  >
                    <div style="min-height: 25px">
                      <span style="font-weight: bold; font-size: 14px">{{
                        data.data.task_name
                      }}</span>
                    </div>
                    <div style="font-size: 12px">
                      <span
                        v-if="data.data.start_date || data.data.end_date"
                        style="color: #98a9bc"
                        >{{
                          data.data.start_date
                            ? moment(new Date(data.data.start_date)).format(
                                "DD/MM/YYYY",
                              )
                            : null
                        }}
                        -
                        {{
                          data.data.end_date
                            ? moment(new Date(data.data.end_date)).format(
                                "DD/MM/YYYY",
                              )
                            : null
                        }}</span
                      >
                    </div>
                    <div
                      v-if="data.data.project_name"
                      style="
                        min-height: 25px;
                        display: flex;
                        align-items: center;
                      "
                    >
                      <i class="pi pi-tag"></i>
                      <span
                        class="duan"
                        style="
                          font-size: 13px;
                          font-weight: 400;
                          margin-left: 5px;
                          color: #0078d4;
                        "
                        >{{ data.data.project_name }}</span
                      >
                    </div>
                  </div>
                </template>
              </Column>
              <Column
                field=""
                header="Thành viên"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:150px;min-height:3.125rem"
                bodyStyle="text-align:center;max-width:150px;"
              >
                <template #body="data">
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
                          :key="index"
                          v-tooltip.bottom="{
                            value:
                              value.type_name +
                              ': ' +
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
                </template>
              </Column>
              <Column
                class="align-items-center justify-content-center text-center"
                header="Tiến độ"
                headerStyle="text-align:center;max-width:100px;min-height:3.125rem"
                bodyStyle="text-align:center;max-width:100px;"
              >
                <template #body="data">
                  <span v-if="data.data.progress == 0"
                    >{{ data.data.progress }} %</span
                  >
                  <div
                    v-if="data.data.progress != 0"
                    style="width: 100%"
                  >
                    <ProgressBar :value="data.data.progress" />
                  </div>
                </template>
              </Column>
              <Column
                class="align-items-center justify-content-center text-center"
                header="Thời gian xử lý"
                headerStyle="text-align:center;max-width:150px;min-height:3.125rem"
                bodyStyle="text-align:center;max-width:150px;"
              >
                <template #body="data">
                  <div v-if="data.data.title_time">
                    <span
                      style="
                        font-size: 10px;
                        font-weight: bold;
                        padding: 5px;
                        border-radius: 5px;
                      "
                      :style="
                        ('background-color:' + data.data.time_bg,
                        'color:' + data.data.status_text_color)
                      "
                      >{{ data.data.title_time }}</span
                    >
                  </div>
                </template>
              </Column>
              <Column
                class="align-items-center justify-content-center text-center"
                header="Ngày cập nhật"
                headerStyle="text-align:center;max-width:150px;min-height:3.125rem"
                bodyStyle="text-align:center;max-width:150px;"
              >
                <template #body="data">
                  <div
                    style="
                      background-color: #fff8ee;
                      padding: 10px 20px;
                      border-radius: 5px;
                    "
                  >
                    <span
                      style="color: #ffab2b; font-size: 13px; font-weight: bold"
                      >{{
                        moment(new Date(data.data.update_date)).format(
                          "DD/MM/YYYY",
                        )
                      }}</span
                    >
                  </div>
                </template>
              </Column>
              <Column
                class="align-items-center justify-content-center text-center"
                header="Trạng thái"
                headerStyle="text-align:center;max-width:120px;min-height:3.125rem"
                bodyStyle="text-align:center;max-width:120px;"
              >
                <template #body="data">
                  <Chip
                    :style="{
                      background: data.data.status_bg_color,
                      color: data.data.status_text_color,
                    }"
                    v-bind:label="data.data.status_name"
                  />
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
                  v-if="!isFirst"
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
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="closeDialogLinkTask"
        class="p-button-text"
      />
      <Button
        label="Lưu"
        icon="pi pi-check"
        @click="saveAddLinkTask()"
      />
    </template>
  </Dialog>
</template>
<style scoped>
.task_list {
  overflow-y: hidden !important;
}

.task_list .p-datatable-tbody {
  overflow-y: auto !important;
}

.anh {
  width: 8rem;
  height: 7rem;
  border: 1px solid #f5f5f5;
  border-radius: 5px;
}

.p-badge.p-badge-lg {
  font-size: 1rem !important;
  min-width: 0rem !important;
}

.p-avatar {
  border: 1px solid;
}

.p-avatar-text {
  font-size: 1rem !important;
}

.format-center {
  display: flex;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-right {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-left {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}
</style>
<style scoped>
.c-hover {
  display: none;
  text-decoration: none !important;
}

.checklist-gr-hv-c {
  display: none !important;
}

/*hover checkbox */

.myTextAvatar .p-avatar-text {
  font-size: 2rem !important;
}
</style>
<style lang="scss" scoped>
.p-button.p-button-raised {
  box-shadow: none;
  color: #72777a;
  background-color: #f5f5f5;
  font-size: 14px !important;
  font-weight: 600;
}

.child-task-hover:hover {
  background-color: var(--blue-100) !important;
}

.p-hover:hover {
  background-color: var(--blue-100) !important;
}

.p-hover:hover .c-hover {
  background-color: var(--blue-100);
  cursor: pointer;
  display: flex;
}

.checklist-gr-hv-p:hover {
  cursor: pointer;
  display: flex !important;
}

.checklist-gr-hv-p:hover .checklist-gr-hv-c {
  cursor: pointer;
  display: flex !important;
}

::v-deep(.product-grid-item) {
  margin: 0.5rem;
  border: 1px solid var(--surface-border);

  .product-grid-item-top,
  .product-grid-item-bottom {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  img {
    box-shadow: 0 3px 6px rgba(0, 0, 0, 0.16), 0 3px 6px rgba(0, 0, 0, 0.23);
    margin: 0rem 0;
  }

  .product-grid-item-content {
    text-align: center;
  }

  .product-price {
    font-size: 1.5rem;
    font-weight: 600;
  }
}

//new
.p-button {
  margin-right: 0.5rem;
}

.p-buttonset {
  .p-button {
    margin-right: 0;
  }
}

.sizes {
  .button {
    margin-bottom: 0.5rem;
    display: block;

    &:last-child {
      margin-bottom: 0;
    }
  }
}

@media screen and (max-width: 640px) {
  .p-button {
    margin-bottom: 0.5rem;

    &:not(.p-button-icon-only) {
      display: flex;
      width: 100%;
    }
  }

  .p-buttonset {
    .p-button {
      margin-bottom: 0;
    }
  }
}

.checked-p {
  color: #6dd230;
  text-decoration: line-through;
  text-decoration-color: #6dd230;
}

.expTime {
  color: #ff0000;
}

.p-checkbox .p-checkbox-box.p-highlight {
  background: #6dd230;
  border: #6dd230;
}
</style>
<!-- style của ScrollPanel -->
<style lang="scss" scoped>
::v-deep(.p-progressbar) {
  &.working {
    .p-progressbar-value {
      border: 0 none;
      margin: 0;
      background: #f44336;
    }
  }

  &.worked {
    .p-progressbar-value {
      border: 0 none;
      margin: 0;
      background: #6dd230;
    }
  }
}

::v-deep(.p-scrollpanel) {
  &.custombar2 {
    .p-scrollpanel-bar-y {
      width: 3px;
    }

    .p-scrollpanel-wrapper {
      border-right: 1px solid var(--surface-ground);
    }

    .p-scrollpanel-bar {
      background-color: var(--primary-color);
      opacity: 1;
      transition: background-color 0.2s;

      &:hover {
        background-color: #007ad9;
      }
    }
  }
}

.text-xl {
  font-size: 14px;
}

.activated {
  background: #f5f5f5 !important;
  color: #2196f3 !important;
  border-color: transparent;
}

.contents {
  text-align: justify;
  color: #333;
  font-size: 14px;
  width: fit-content;
  padding: 1rem 1rem;
  background-color: #f5f5f5;
  border-radius: 20px;
  margin-left: 4rem;
}

.text-dark {
  color: #000000;
}

.contents2 {
  list-style-type: none;
  display: inline-block;
  width: 10rem;
  text-align: center;
  border: 1px solid #eee;
  border-radius: 5px;
  cursor: pointer;
}

::v-deep(.p-treeselect-items-wrapper) {
  max-height: 200px !important;
  min-width: calc(100vw - 1100px) !important;
  max-width: calc(100vw - 1100px) !important;
}

#task-child li:hover {
  cursor: pointer;
}

.file-hover:hover {
  background-color: #d8edff;
}

.menu-hover:hover {
  background-color: #d8edff;
}

a {
  text-decoration: none;
}

.p-button-hover:hover {
  color: #0025f8 !important;
  background: #ffffff !important;
}

.delete-button-hover {
  display: none;
}

.cmt-hover:hover .delete-button-hover {
  display: block !important;
}

.cmt-hover:hover {
  background-color: #f5f5f5;
}

.p-button.p-button-raised2 {
  box-shadow: none;
  color: black;
  background-color: #ffffff;
  font-size: 14px !important;
  font-weight: 600;
}

.file-comments {
  list-style-type: none;
  display: inline-block;
  text-align: center;
  border: 1px solid #eee;
  border-radius: 5px;
  cursor: pointer;
  background: #ffffff;
}

.file-comments-hover:hover {
  color: #0025f8 !important;
}

.reply {
  background-color: #fff5f5;
  padding-top: 0.25rem;
  padding-bottom: 0.25rem;
}

.reply:hover {
  background-color: #ffffff !important;
}

.p-button-hover-file:hover {
  color: #0025f8 !important;
  background: #f5f5f5 !important;
}

.p-button-hover-file-menu:hover {
  color: #ffffff !important;
  background-color: #5eb4fa !important;
}

.cmt-reply {
  background-color: #ffe9e9;
  border: 1px solid #ffe9e9;
  border-radius: 10px;
}

.border-bottom-comment {
  border-bottom: 1px solid black !important;
}

.bg-cmt-color {
  background-color: #ffffff !important;
}

.checklist-hover:hover {
  background-color: #dbf4fd !important;
  cursor: pointer;
}

.thongtinchungscroll .p-scrollpanel-content {
  height: calc(100% - 6.5rem);
}

.btn-c-hover:hover {
  color: #0025f8 !important;
  background: #f5f5f5 !important;
}

.p-danger-hover:hover {
  color: #ff0000 !important;
  background: #ffe9e9 !important;
}

.bg-reply {
  background: #ffe9e9 !important;
  border: 1px radius #ffe9e9;
  border-radius: 5px;
}
.p-avatar {
  font-size: inherit !important;
}
</style>
