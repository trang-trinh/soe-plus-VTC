<script setup>
import { ref, inject, onMounted, onBeforeMount, watch } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { encr } from "../../../util/function.js";
import moment from "moment";
import TaskFollowDetailVue from "../../../components/task_origin/Detail_Task/follow/TaskFollowDetail.vue";
import DetailedWork from "../../../components/task_origin/DetailedWork.vue";
import DialogTask from "../../../components/task_origin/DialogTask.vue";
import useVuelidate from "@vuelidate/core";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const axios = inject("axios");
const store = inject("store");
const toast = useToast();
const swal = inject("$swal");
// eslint-disable-next-line no-undef
const basedomainURL = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

const bgColor = ref([
  "#F4B2A3",
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
let user = store.state.user;
const datalists = ref([]);
const options = ref({
  loading: true,
});

const DialogVisible = ref();
const headerDialog = ref();
const length = ref(false);
const checklength = () => {
  length.value = false;
  const textbox = document.getElementById("follow_name");
  if (textbox.value.length > 500) {
    length.value = true;
  }
  return length.value;
};
const taskfollow = ref({
  follow_id: null,
  task_id: null,
  project_id: null,
  follow_name: "",
  description: null,
  start_date: null,
  end_date: null,
  type: null,
  weight: null,
  status: null,
  is_template: true,
  process_time: null,
  organization_id: user.organization_id,
});
const rules = {
  follow_name: {
    required,
  },
};
const v$ = useVuelidate(rules, taskfollow);
const listDrdType = ref([
  {
    value: 0,
    label: "Mặc định",
    bg_color: "#2196f3",
    text_color: "#FFFFFF",
  },
  {
    value: 1,
    label: "Thực hiện tuần tự",
    bg_color: "#d87777",
    text_color: "#FFFFFF",
  },
  {
    value: 2,
    label: "Thực hiện song song",
    bg_color: "#04D215",
    text_color: "#FFFFFF",
  },
]);

const listDrdStatus = ref([
  {
    value: 0,
    label: "Chưa bắt đầu",
    bg_color: "#bbbbbb",
    text_color: "#FFFFFF",
  },
  {
    value: 1,
    label: "Đang làm",
    bg_color: "#2196f3",
    text_color: "#FFFFFF",
  },
  {
    value: 2,
    label: "Hoàn thành đúng hạn",
    bg_color: "#04D215",
    text_color: "#FFFFFF",
  },
  {
    value: 3,
    label: "Hoàn thành sau hạn",
    bg_color: "#ff8b4e",
    text_color: "#FFFFFF",
  },
  {
    value: 4,
    label: "Tạm ngưng",
    bg_color: "#d87777",
    text_color: "#FFFFFF",
  },
  {
    value: 5,
    label: "Đóng",
    bg_color: "#d87777",
    text_color: "#FFFFFF",
  },
]);
const isEdit = ref(false);
const openDialog = () => {
  submitted.value = false;
  isEdit.value = false;
  taskfollow.value = {
    follow_id: null,
    task_id: null,
    project_id: null,
    follow_name: "",
    description: null,
    start_date: null,
    end_date: null,
    type: 0,
    weight: 0,
    status: 0,
    is_template: true,
    process_time: null,
    organization_id: user.organization_id,
  };

  DialogVisible.value = true;
  listTask.value = [];
  headerDialog.value = "Tạo quy trình công việc";
};
const OpenEditDialog = (data) => {
  submitted.value = false;
  isEdit.value = true;
  let edit = JSON.parse(JSON.stringify(data));
  taskfollow.value.process_time =
    (taskfollow.value.day != null ? taskfollow.value.day : 0) * 24 * 60 +
    (taskfollow.value.hour != null ? taskfollow.value.hour : 0) * 60 +
    (taskfollow.value.minutes != null ? taskfollow.value.minutes : 0);
  taskfollow.value = edit;
  DialogVisible.value = true;
  headerDialog.value = "Sửa quy trình công việc";
};
const listTask = ref([]);
const submitted = ref(false);
const saveData = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  if (taskfollow.value.is_template == true) {
    taskfollow.value.process_time =
      (taskfollow.value.day != null ? taskfollow.value.day : 0) * 24 * 60 +
      (taskfollow.value.hour != null ? taskfollow.value.hour : 0) * 60 +
      (taskfollow.value.minutes != null ? taskfollow.value.minutes : 0);
  }
  let formData = new FormData();
  formData.append("task_follow", JSON.stringify(taskfollow.value));
  axios({
    method: isEdit.value == false ? "post" : "put",
    url:
      baseURL +
      "/api/task_follow/" +
      (isEdit.value == false ? "addFollows" : "UpdateFollow"),
    data: formData,
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        loadData();
        toast.success(
          isEdit.value == false
            ? "Thêm quy trình thành công!"
            : "Sửa quy trình thành công",
        );
        DialogVisible.value = false;
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
const listChild = ref([]);
const loadChildTaskOrigin = (listid) => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "Task_Origin_children_list_template",
            par: [{ par: "list_id", va: listid }],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let listChildjson = JSON.parse(response.data.data)[0];
      let listUser = JSON.parse(response.data.data)[1];

      listChildjson.forEach((c) => {
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

      listChild.value = [];
      listChild.value = JSON.parse(JSON.stringify(listChildjson));
      listTask.value = JSON.parse(JSON.stringify(listChildjson));
      return listChildjson;
    })
    .catch((error) => {
      // toast.error("Tải dữ liệu không thành công4!");
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
const loadData = () => {
  indexSelected.value = 0;
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
            proc: "task_follow_list_template",
            par: [
              { par: "task_id", va: null },
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
      datalists.value = [];
      let data = JSON.parse(response.data.data)[0];
      let listChildjson = JSON.parse(response.data.data)[1];
      let listUser = JSON.parse(response.data.data)[2];

      listChildjson.forEach((c) => {
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
      if (data.length > 0) {
        data.forEach((x) => {
          let type = listDrdType.value.filter((xz) => xz.value == x.type)[0];
          if (x.process_time > 0) {
            x.minutes = x.process_time % 60;
            x.hour = Math.floor(x.process_time / 60);
            let temp = x.hour;
            if (temp > 24) {
              x.hour = Math.floor(temp % 24);
              x.day = Math.floor(temp / 24);
            }
          }
          x.type_display = {};
          x.type_display = type;
          let filterstatus = listDrdStatus.value.filter(
            (xz) => xz.value == x.status,
          )[0];
          x.status_display = {};
          x.status_display = filterstatus;
          x.StepProgress = 0;
          if (x.countStep > 0) {
            x.StepProgress = Math.floor(
              (x.countStepFinished / x.countStep) * 100,
            );
          }
          x.TaskProgress = 0;
          if (x.countTask > 0) {
            x.TaskProgress = Math.floor(
              (x.countTaskFinished / x.countTask) * 100,
            );
          }
          if (x.task_follow_step != null) {
            x.task_follow_step = JSON.parse(x.task_follow_step);
            if (x.task_follow_step.length > 0) {
              x.task_follow_step.forEach((y) => {
                if (y.time_process > 0) {
                  let zminutes = null;
                  zminutes = y.time_process % 60;
                  let zhour = null;
                  zhour = Math.floor(y.time_process / 60);
                  let zday = null;
                  let tem2 = zhour;
                  if (tem2 > 24) {
                    zhour = Math.floor(tem2 % 24);
                    zday = Math.floor(tem2 / 24);
                  }
                  y = Object.assign(y, {
                    day: zday,
                    hour: zhour,
                    minutes: zminutes,
                  });
                }
                let type = listDrdType.value.filter(
                  (xz) => xz.value == y.type,
                )[0];

                y.type_display = {};
                y.type_display = type;
                let filterstatuszz = listDrdStatus.value.filter(
                  (xz) => xz.value == y.status,
                )[0];
                y.status_display = {};
                y.status_display = filterstatuszz;
                y.TaskProgress = 0;
                if (y.countTask > 0) {
                  y.TaskProgress = Math.floor(
                    (y.countTaskFinished / y.countTask) * 100,
                  );
                }
                y.task_info = [];
                if (y.task_id_follow != null) {
                  y.task_id_follow.forEach((z) => {
                    let k = listChildjson.filter(
                      (a) => a.task_id == z.task_id_follow,
                    );
                    if (k.length > 0) {
                      let obj = Object.assign({}, z, k[0]);
                      y.task_info.push(obj);
                    }
                  });
                }
              });
            }
          } else {
            x.task_follow_step = [];
          }
        });
        datalists.value = data;
        if (datalists.value.length > 0) {
          expandAll(datalists.value);
        }
      } else {
        datalists.value = [];
      }
      options.value.totalRecords = data.length;
      swal.close();
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
};
const indexSelected = ref(0);
const expandAll = (e) => {
  let dataFilter = e[0];
  if (dataFilter != null) {
    let add = [];
    if (
      dataFilter.task_follow_step != null &&
      dataFilter.task_follow_step.length > 0
    ) {
      let findIndex = dataFilter.task_follow_step.findIndex((x) => {
        return x.status === 1;
      });
      if (findIndex >= 0) {
        indexSelected.value = 0;
      } else indexSelected.value = 0;
    }
    add.push(dataFilter);
    expandedRows.value = add;
  }
};
const round = ref();
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
const isOpen = ref(false);
const openDetail = (data) => {
  round.value = null;
  forceRerender();
  isOpen.value = true;
  round.value = data;
};
const closeDialogDetail = () => {
  isOpen.value = false;
};
const onRowReorder = (event) => {
  let formData = new FormData();
  console.log(event.value);
  formData.append("task_follow", JSON.stringify(event.value));
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url: baseURL + "/api/task_follow_step/ReOdersFollow",
    data: formData,
    headers: { Authorization: `Bearer ${store.getters.token}` },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        loadData();
        toast.success("Sửa thứ tự thực hiện công việc thành công");
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
    .catch(() => {
      swal.close();

      swal.fire({
        title: "Thông báo",
        html: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const DeleteItem = (vl) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá quy trình này không!",
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
        let id = [];
        id.push(vl.follow_id);
        axios
          .delete(baseURL + "/api/task_follow/DeleteFollow", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: vl != null ? id : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá quy trình thành công!");
              loadData();
            } else {
              swal.fire({
                title: "Thông báo",
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
                title: "Thông báo",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const DeleteStep = (vl) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá bước này không!",
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
        let id = [];
        id.push(vl.follow_step_id);
        axios
          .delete(baseURL + "/api/task_follow_step/DeleteStep", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: vl != null ? id : 1,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá bước thành công!");

              loadData();
            } else {
              swal.fire({
                title: "Thông báo",
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
                title: "Thông báo",
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                icon: "error",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const taskStep = ref({
  follow_id: "",
  task_id: "",
  step_name: "",
  description: "",
  start_date: "",
  end_date: "",
  type: 0,
  status: 0,
  is_step: "",
});
const rulesStep = {
  step_name: { required },
};
const vStep$ = useVuelidate(rulesStep, taskStep);

const StepDialogVisible = ref(false);
const headerStepDialog = ref();
const openStepDialog = (e) => {
  submitted.value = false;
  isEdit.value = false;
  taskStep.value = {
    follow_id: "",
    task_id: null,
    step_name: "",
    description: "",
    start_date: null,
    end_date: null,
    type: 0,
    status: 0,
    is_step: 1,
  };
  taskStep.value.follow_id = e.follow_id;
  taskStep.value.is_step = e.countStep > 0 ? e.countStep + 1 : 1;
  StepDialogVisible.value = true;
  listTask.value = [];
  headerStepDialog.value = "Thêm bước";
};
const openEditStepDialog = (e, f) => {
  submitted.value = false;
  isEdit.value = true;
  let template = JSON.parse(JSON.stringify(e));
  taskStep.value = template;
  console.table(template);
  StepDialogVisible.value = true;
  listTask.value = [];
  e.task_info.forEach((x) => {
    listTask.value.push(x);
    listChild.value.push(x);
  });
  headerStepDialog.value = "Cập nhật bước";
};
const length2 = ref(false);
const checklength2 = () => {
  length2.value = false;
  const textbox = document.getElementById("step_name");
  if (textbox.value.length > 500) {
    length2.value = true;
  }
  return length2.value;
};
const DeleteTask = (x) => {
  var listId = [];
  listId.push(x);
  axios
    .delete(baseURL + "/api/task_origin/Delete_task_origin", {
      headers: { Authorization: `Bearer ${store.getters.token}` },
      data: listId,
    })
    .then((response) => {})
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
};
const saveStep = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  let formData = new FormData();
  let listID = [];
  let dontUseID = listChild.value.filter((x) => {
    listTask.value.includes(x) != true;
  });
  listTask.value.forEach((x) => {
    listID.push({ task_id_follow: x.task_id });
  });
  taskStep.value.is_template = true;
  if (taskStep.value.is_template == true) {
    taskStep.value.time_process =
      (taskStep.value.day != null ? taskStep.value.day : 0) * 24 * 60 +
      (taskStep.value.hour != null ? taskStep.value.hour : 0) * 60 +
      (taskStep.value.minutes != null ? taskStep.value.minutes : 0);
  }
  formData.append("task_step", JSON.stringify(taskStep.value));
  formData.append("task_follow_task", JSON.stringify(listID));
  axios({
    method: isEdit.value == false ? "post" : "put",
    url:
      baseURL +
      "/api/task_follow_step/" +
      (isEdit.value == false ? "addStep" : "UpdateStep"),
    data: formData,
    headers: { Authorization: `Bearer ${store.getters.token}` },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        loadData();
        toast.success(
          isEdit.value == false
            ? "Thêm bước thành công!"
            : "Sửa bước thành công!",
        );
        StepDialogVisible.value = false;
        listTask.value = [];
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
  if (dontUseID.length > 0) {
    dontUseID.forEach((x) => {
      DeleteTask(x.task_id);
    });
  }
};

// 0: tạo/giao 1,2: làm, 3:theo dõi
const TypeMember = ref();

const showDetail = ref(false);
const selectedTaskID = ref();
const onNodeSelect = (id) => {
  showDetail.value = false;
  showDetail.value = true;
  selectedTaskID.value = id;
};

onBeforeMount(() => {
  loadData();
});

onMounted(() => {});
const expandedRows = ref([]);

const selectStep = (e, i) => {
  e.index = i;
};
const closeDetail = () => {
  showDetail.value = false;
  selectedTaskID.value = null;
  loadData();
};
const headerAddTask = ref("Tạo công việc mẫu");
const displayTask = ref(false);
const is_Add = ref(true);
const is_Template = ref(true);
const DialogData = ref();
const closeDialogTask = () => {
  displayTask.value = false;
};
const list_id = ref("");
const afterSave = (e) => {
  debugger;
  list_id.value += e + ",";
  if (list_id.value != "") {
    loadChildTaskOrigin(list_id.value);
  }
};
const openAddTask = () => {
  displayTask.value = true;
};
</script>
<template>
  <div class="main-layout true flex-grow-1 p-2">
    <DataTable
      :value="datalists"
      scrollable
      scrollHeight="flex"
      dataKey="follow_id"
      v-model:expandedRows="expandedRows"
      showGridlines
      responsiveLayout="scroll"
      :rowHover="true"
      :globalFilterFields="['full_name', 'user_id']"
    >
      <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
          <i class="pi pi-sliders-v"></i> Danh sách quy trình mẫu ({{
            options.totalRecords
          }})
        </h3>
        <Toolbar class="w-full custoolbar">
          <!-- <template #start>
            <span class="p-input-icon-left">
              <i class="pi pi-search" />
              <InputText
                v-model="options.SearchText"
                @keyup.enter="loadData()"
                type="text"
                spellcheck="false"
                placeholder="Tìm kiếm"
              />
            </span>
            <Button
              type="button"
              class="ml-2 p-button-outlined p-button-secondary"
              icon="pi pi-filter"
              @click="toggle"
              aria:haspopup="true"
              aria-controls="overlay_panel"
              v-tooltip.bottom="'Bộ lọc'"
              :style="[styleObj]"
            />
            <OverlayPanel
              ref="op"
              appendTo="body"
              class="w-30rem p-0 m-0"
              :showCloseIcon="false"
              id="overlay_panel"
              style="z-index: 999"
            >
            </OverlayPanel>
          </template> -->

          <template #end>
            <Button
              icon="pi pi-plus"
              label="Thêm quy trình"
              @click="openDialog()"
              v-if="user.is_admin == true || TypeMember == 0"
              class="mx-2"
            ></Button>
          </template>
        </Toolbar>
      </template>

      <Column
        expander
        class="justify-content-center align-items-center text-center max-w-4rem"
      />
      <Column
        header="Tên quy trình"
        field="follow_name"
        headerClass="justify-content-center align-items-center text-center w-custom  "
        bodyClass="word-break-break-all w-custom "
      ></Column>
      <Column
        header="Bước"
        field="follow_name"
        class="justify-content-center align-items-center max-w-14rem"
      >
        <template #body="data">
          <div
            class="w-full justify-content-center align-items-center text-center max-w-10rem"
          >
            <div>
              {{ data.data.countStepFinished }} / {{ data.data.countStep }}
            </div>
          </div>
        </template>
      </Column>

      <Column
        header="Công việc"
        field="follow_name"
        class="justify-content-center align-items-center max-w-14rem"
      >
        <template #body="data">
          <div
            class="w-full justify-content-center align-items-center text-center max-w-8rem"
          >
            <div>
              {{ data.data.countTaskFinished }} / {{ data.data.countTask }}
            </div>
          </div>
        </template>
      </Column>
      <Column
        header="Thời gian xử lý"
        field="process_time_string"
        class="justify-content-center align-items-center max-w-20rem"
      >
      </Column>
      <Column
        header="Chức năng"
        field=""
        class="justify-content-center align-items-center max-w-15rem"
      >
        <template #body="data">
          <div class="flex">
            <Button
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              icon="pi pi-plus"
              v-tooltip="'Thêm bước'"
              v-if="
                (user.is_admin == true || TypeMember == 0) && data.data != null
              "
              @click="openStepDialog(data.data)"
            ></Button>
            <Button
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-info"
              v-tooltip="'Chi tiết'"
              @click="openDetail(data.data)"
            >
            </Button>
            <Button
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              icon="pi pi-pencil"
              v-tooltip="'Sửa'"
              @click="OpenEditDialog(data.data)"
              v-if="user.is_admin == true || TypeMember == 0"
            >
            </Button>
            <Button
              @click="DeleteItem(data.data, true)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              v-tooltip="'Xóa'"
              icon="pi pi-trash"
              v-if="user.is_admin == true || TypeMember == 0"
            ></Button>
          </div>
        </template>
      </Column>
      <template #empty>
        <div
          class="row col-12 align-items-center justify-content-center p-4 text-center m-auto"
        >
          <img
            src="../../../assets/background/nodata.png"
            height="144"
          />
          <h3 class="m-1">Không có dữ liệu</h3>
        </div>
      </template>
      <template #expansion="slotProps">
        <div class="w-full">
          <div
            class="w-full"
            v-if="slotProps.data.task_follow_step.length > 0"
          >
            <div class="multi-step numbered">
              <div class="buttonfunc">
                <Button
                  class="mx-1 p-button-outlined p-button-rounded"
                  icon="pi pi-pencil"
                  v-tooltip="'Sửa bước đang chọn'"
                  v-if="
                    (user.is_admin == true || TypeMember == 0) &&
                    (slotProps.data.index
                      ? slotProps.data.index
                      : indexSelected) != null
                  "
                  @click="
                    openEditStepDialog(
                      slotProps.data.task_follow_step[
                        slotProps.data.index
                          ? slotProps.data.index
                          : indexSelected
                      ],
                      slotProps.data,
                    )
                  "
                ></Button>
                <Button
                  class="mx-1 p-button-danger p-button-outlined p-button-raised p-button-rounded"
                  icon="pi pi-trash"
                  v-tooltip="'Xóa bước đang chọn'"
                  v-if="
                    (user.is_admin == true || TypeMember == 0) &&
                    (slotProps.data.index
                      ? slotProps.data.index
                      : indexSelected) != null
                  "
                  @click="
                    DeleteStep(
                      slotProps.data.task_follow_step[
                        slotProps.data.index
                          ? slotProps.data.index
                          : indexSelected
                      ],
                      true,
                    )
                  "
                ></Button>
              </div>
              <div class="multi-step-list">
                <div
                  class="multi-step-item active"
                  v-for="(item, index) in slotProps.data.task_follow_step"
                  :key="index"
                  :class="[
                    {
                      current: slotProps.data.index
                        ? index == slotProps.data.index
                        : index == indexSelected,
                    },
                  ]"
                  @click="selectStep(slotProps.data, index)"
                  v-tooltip.bottom="{ value: item.step_name }"
                >
                  <div
                    class="item-wrap flex align-items-center justify-content-center"
                  >
                    <p class="item-title text-center">{{ item.step_name }}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div
            v-else
            class="w-full col-12 align-items-center justify-content-center p-4 text-center flex"
          >
            <div class="block">
              <img
                src="../../../assets/background/nodata.png"
                height="144"
              />
              <h3 class="m-1">Quy trình chưa có các bước thực hiện!</h3>
            </div>
          </div>
          <div
            v-if="
              slotProps.data.task_follow_step != [] &&
              slotProps.data.task_follow_step.length > 0
            "
          >
            <div
              v-if="
                slotProps.data.task_follow_step[
                  slotProps.data.index ? slotProps.data.index : indexSelected
                ] != null &&
                slotProps.data.task_follow_step[
                  slotProps.data.index ? slotProps.data.index : indexSelected
                ].task_info.length > 0
              "
            >
              <div
                class="m-2 grid align-items-center justify-content-center flex-column"
                v-for="(item2, index2) in slotProps.data.task_follow_step[
                  slotProps.data.index ? slotProps.data.index : indexSelected
                ].task_info"
                :key="index2"
              >
                <Card
                  class="bg-bluegray-50 w-30rem card-hover"
                  @click="onNodeSelect(item2.task_id)"
                >
                  <template #title>
                    <span
                      class="flex justify-content-center align-items-center text-center font-bold text-xl"
                    >
                      Công việc:
                      <span class="text-blue-700"> {{ item2.task_name }}</span>
                    </span>
                  </template>
                  <template #subtitle>
                    <div class="flex justify-content-center align-items-center">
                      <span
                        v-if="item2.start_date || item2.end_date"
                        style="color: #98a9bc"
                      >
                        <i
                          style="margin-right: 5px"
                          class="pi pi-calendar"
                        >
                        </i>
                        {{
                          item2.start_date
                            ? moment(new Date(item2.start_date)).format(
                                "DD/MM/YYYY",
                              )
                            : null
                        }}
                        -
                        {{
                          item2.end_date
                            ? moment(new Date(item2.end_date)).format(
                                "DD/MM/YYYY",
                              )
                            : null
                        }}
                      </span>
                    </div>
                  </template>
                  <template #content>
                    <div
                      class="w-50 flex justify-content-center align-items-center"
                    >
                      <span
                        class=""
                        :style="{
                          background: item2.status_display.bg_color,
                          color: item2.status_display.text_color,
                          padding: '2px 8px',
                          border: '1px solid' + item2.status_display.bg_color,
                          borderRadius: '5px',
                        }"
                      >
                        {{ item2.status_display.text }}
                      </span>
                    </div>
                  </template>
                  <template #footer> </template>
                </Card>

                <icon
                  v-tooltip="'Tuần tự'"
                  class="py-2 pi pi-arrow-down font-bold text-2xl flex justify-content-center"
                  v-if="
                    slotProps.data.task_follow_step[
                      slotProps.data.index
                        ? slotProps.data.index
                        : indexSelected
                    ].type == 1 &&
                    index2 <
                      slotProps.data.task_follow_step[
                        slotProps.data.index
                          ? slotProps.data.index
                          : indexSelected
                      ].task_info.length -
                        1
                  "
                ></icon>
                <icon
                  class="py-2 pi pi-sort-alt font-bold text-2xl flex justify-content-center"
                  v-tooltip="'Song song'"
                  v-if="
                    slotProps.data.task_follow_step[
                      slotProps.data.index
                        ? slotProps.data.index
                        : indexSelected
                    ].type == 2 &&
                    index2 <
                      slotProps.data.task_follow_step[
                        slotProps.data.index
                          ? slotProps.data.index
                          : indexSelected
                      ].task_info.length -
                        1
                  "
                >
                </icon>
              </div>
            </div>
            <div
              v-else
              class="top-50 block row col-12 align-items-center justify-content-center p-4 text-center"
            >
              <img
                src="../../../assets/background/nodata.png"
                height="144"
              />
              <h3 class="m-1">Chưa có công việc được giao!</h3>
            </div>
          </div>
        </div>
      </template>
    </DataTable>
  </div>

  <TaskFollowDetailVue
    :componentKey="componentKey"
    :data="round"
    :isOpen="isOpen"
    :closeDialogDetail="closeDialogDetail"
    :rowReorder="onRowReorder"
    :memberType="TypeMember"
  ></TaskFollowDetailVue>
  <Dialog
    v-model:visible="DialogVisible"
    :style="'width:40vw;'"
    :closable="false"
    :header="headerDialog"
  >
    <form action="">
      <div class="col-12 flex">
        <div class="col-4">Tên quy trình<span class="redsao">(*)</span></div>
        <InputText
          id="follow_name"
          v-model="taskfollow.follow_name"
          spellcheck="false"
          class="col-8"
          :class="{
            'p-invalid': v$.follow_name.$invalid && submitted,
          }"
          autocomplete="off"
          @input="checklength()"
        />
      </div>
      <div
        style="display: flex"
        class="col-12 py-0"
        v-if="length == true"
      >
        <div class="col-4 p-0 text-left"></div>
        <small class="col-8 p-0 p-error">
          <span class="col-12">Tên quy trình không quá 500 kí tự!</span>
        </small>
      </div>
      <div
        style="display: flex"
        class="col-12 py-0"
        v-if="
          (v$.follow_name.$invalid && submitted) ||
          v$.follow_name.$pending.$response
        "
      >
        <div class="col-4 p-0 text-left"></div>
        <small class="col-8 p-0 p-error">
          <span class="col-12">{{
            v$.follow_name.required.$message
              .replace("Value", "Tên quy trình")
              .replace("is required", "không được để trống!")
          }}</span>
        </small>
      </div>
      <div class="col-12 flex">
        <div class="col-4 flex align-items-center">Mô tả</div>
        <Textarea
          v-model="taskfollow.description"
          spellcheck="false"
          class="col-8"
          rows="3"
          autocomplete="off"
        />
      </div>
      <div class="col-12 flex">
        <div class="col-4 flex align-items-center">Thời gian xử lý</div>
        <div class="col-8 p-0 flex align-items-center">
          <InputNumber
            class="col-4 pl-0"
            suffix=" ngày"
            mode="decimal"
            :min="0"
            :useGrouping="false"
            placeholder="ngày"
            v-model="taskfollow.day"
          ></InputNumber>

          <InputNumber
            class="col-4"
            suffix=" giờ"
            :min="0"
            :max="23"
            :useGrouping="false"
            placeholder="giờ"
            v-model="taskfollow.hour"
          ></InputNumber>
          <InputNumber
            class="col-4 pr-0"
            suffix=" phút"
            :min="0"
            :max="59"
            :useGrouping="false"
            placeholder="phút"
            v-model="taskfollow.minutes"
          ></InputNumber>
        </div>
      </div>
      <div class="col-12 flex">
        <div class="col-4 flex align-items-center">Trình tự thực hiện</div>
        <Dropdown
          :filter="true"
          v-model="taskfollow.type"
          :options="listDrdType"
          optionLabel="label"
          placeholder="Chọn trình tự"
          panelClass="d-design-dropdown"
          class="col-8 py-0"
          optionValue="value"
        >
        </Dropdown>
      </div>
    </form>

    <template #footer>
      <div class="mt-2">
        <Button
          class="p-button-text"
          icon="pi pi-times"
          label="Đóng"
          @click="DialogVisible = false"
        />
        <Button
          icon="pi pi-check"
          label="Xác nhận"
          @click="saveData(!v$.$invalid)"
        />
      </div>
    </template>
  </Dialog>
  <Dialog
    v-model:visible="StepDialogVisible"
    :style="'width:40vw;'"
    :closable="false"
    :header="headerStepDialog"
  >
    <form action="">
      <div class="col-12 flex">
        <div class="col-4">Tên bước<span class="redsao">(*)</span></div>
        <InputText
          id="step_name"
          v-model="taskStep.step_name"
          spellcheck="false"
          class="col-8"
          :class="{
            'p-invalid': vStep$.step_name.$invalid && submitted,
          }"
          autocomplete="off"
          @input="checklength2()"
        />
      </div>
      <div
        style="display: flex"
        class="col-12 py-0"
        v-if="length2 == true"
      >
        <div class="col-4 p-0 text-left"></div>
        <small class="col-8 p-0 p-error">
          <span class="col-12">Tên bước không quá 500 kí tự!</span>
        </small>
      </div>
      <div
        style="display: flex"
        class="col-12 py-0"
        v-if="
          (vStep$.step_name.$invalid && submitted) ||
          vStep$.step_name.$pending.$response
        "
      >
        <div class="col-4 p-0 text-left"></div>
        <small class="col-8 p-0 p-error">
          <span class="col-12">{{
            vStep$.step_name.required.$message
              .replace("Value", "Tên bước")
              .replace("is required", "không được để trống!")
          }}</span>
        </small>
      </div>
      <div class="col-12 flex">
        <div class="col-4 flex align-items-center">Mô tả</div>
        <Textarea
          v-model="taskStep.description"
          spellcheck="false"
          class="col-8"
          rows="3"
          autocomplete="off"
        />
      </div>
      <div class="col-12 flex">
        <div class="col-4 flex align-items-center">Ngày bắt đầu</div>
        <div class="col-8 p-0 flex align-items-center">
          <InputNumber
            class="col-4 pl-0"
            suffix=" ngày"
            mode="decimal"
            :min="0"
            :useGrouping="false"
            v-model="taskStep.day"
            placeholder="ngày"
          ></InputNumber>

          <InputNumber
            class="col-4"
            suffix=" giờ"
            :min="0"
            :max="23"
            :useGrouping="false"
            placeholder="giờ"
            v-model="taskStep.hour"
          ></InputNumber>
          <InputNumber
            class="col-4 pr-0"
            suffix=" phút"
            :min="0"
            :max="59"
            :useGrouping="false"
            v-model="taskStep.minutes"
            placeholder="phút"
          ></InputNumber>
        </div>
      </div>
      <div class="col-12 flex">
        <div class="col-4 flex align-items-center">Trình tự thực hiện</div>
        <Dropdown
          :filter="true"
          v-model="taskStep.type"
          :options="listDrdType"
          optionLabel="label"
          placeholder="Chọn trình tự"
          panelClass="d-design-dropdown"
          class="col-8 py-0"
          optionValue="value"
        >
        </Dropdown>
      </div>

      <div class="col-12 flex">
        <div class="col-4 flex align-items-center">Chọn công việc</div>
        <div class="col-7 p-0">
          <MultiSelect
            :filter="true"
            v-model="listTask"
            :options="listChild"
            placeholder="Chọn công việc"
            display="chip"
            optionLabel="task_name"
            :filterFields="['task_name', 'task_name_en']"
            class="d-design-dropdown w-full"
          >
            <template #option="slotProps">
              <div class="row col-12 flex">
                <div class="col-7 p-0 m-0">
                  <span class="font-bold text-xl">
                    {{ slotProps.option.task_name }}
                  </span>
                  <br />
                  <span>
                    {{
                      moment(new Date(slotProps.option.start_date)).format(
                        "DD/MM/YYYY",
                      )
                    }}
                  </span>
                  -
                  <span v-if="slotProps.option.is_deadline == true">
                    {{
                      moment(new Date(slotProps.option.end_date)).format(
                        "DD/MM/YYYY",
                      )
                    }}
                  </span>
                </div>
                <div class="col-4 p-0 m-0 format-center">
                  <AvatarGroup>
                    <div
                      v-for="(user, index) in slotProps.option.users"
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
                        v-if="user.is_type == 1 && user.STTTH == 0"
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
                        v-if="user.is_type == 2 && user.STTDTH == 0"
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
                        v-if="user.is_type == 3 && user.STTTD == 0"
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
                      v-if="slotProps.option.users.length > 4"
                      v-tooltip.right="{
                        value:
                          'và ' +
                          (slotProps.option.users.length - 4) +
                          ' người khác tham gia',
                      }"
                      :label="'+' + (slotProps.option.users.length - 4)"
                      style="color: #ffffff; cursor: pointer; font-size: 1rem"
                      :style="{
                        background: bgColor[((bgColor.length - 1) * 2) % 11],
                        border:
                          '2px solid' + bgColor[(bgColor.length - 1) % 10],
                      }"
                      class=""
                      size="normal"
                      shape="circle"
                    ></Avatar>
                  </AvatarGroup>
                </div>
                <div
                  class="col-1 p-0 m-0 flex align-items-center justify-content-center"
                >
                  {{ slotProps.option.progress }}%
                </div>
              </div>
            </template>
          </MultiSelect>
        </div>
        <div class="col-1 py-0 flex justify-content-center">
          <Button
            icon="pi pi-plus"
            class="p-button-raised"
            v-tooltip="'Tạo công việc con'"
            @click="openAddTask()"
          >
          </Button>
        </div>
      </div>
      <div>
        <OrderList
          v-model="listTask"
          listStyle="height:auto"
          dataKey="task_id"
          class="col-12"
          ><template #header> Thứ tự công việc</template>
          <template #item="slotProps">
            <div class="row col-12 flex">
              <div class="col-7 p-0 m-0">
                <span class="font-bold text-xl">
                  {{ slotProps.item.task_name }}
                </span>
                <br />
                <span>
                  {{
                    moment(new Date(slotProps.item.start_date)).format(
                      "DD/MM/YYYY",
                    )
                  }}
                </span>
                -
                <span v-if="slotProps.item.is_deadline == true">
                  {{
                    moment(new Date(slotProps.item.end_date)).format(
                      "DD/MM/YYYY",
                    )
                  }}
                </span>
              </div>
              <div class="col-4 p-0 m-0 format-center">
                <AvatarGroup>
                  <div
                    v-for="(user, index) in slotProps.item.users"
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
                      v-if="user.is_type == 1 && user.STTTH == 0"
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
                      v-if="user.is_type == 2 && user.STTDTH == 0"
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
                      v-if="user.is_type == 3 && user.STTTD == 0"
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
                    v-if="slotProps.item.users.length > 4"
                    v-tooltip.right="{
                      value:
                        'và ' +
                        (slotProps.item.users.length - 4) +
                        ' người khác tham gia',
                    }"
                    :label="'+' + (slotProps.item.users.length - 4)"
                    style="color: #ffffff; cursor: pointer; font-size: 1rem"
                    :style="{
                      background: bgColor[((bgColor.length - 1) * 2) % 11],
                      border: '2px solid' + bgColor[(bgColor.length - 1) % 10],
                    }"
                    class=""
                    size="normal"
                    shape="circle"
                  ></Avatar>
                </AvatarGroup>
              </div>
              <div
                class="col-1 p-0 m-0 flex align-items-center justify-content-center"
              >
                {{ slotProps.item.progress }}%
              </div>
            </div>
          </template>
        </OrderList>
      </div>
    </form>

    <template #footer>
      <div class="mt-2">
        <Button
          class="p-button-text"
          icon="pi pi-times"
          label="Đóng"
          @click="StepDialogVisible = false"
        />
        <Button
          icon="pi pi-check"
          label="Xác nhận"
          @click="saveStep(!vStep$.$invalid)"
        />
      </div>
    </template>
  </Dialog>

  <DetailedWork
    v-if="showDetail === true"
    :id="selectedTaskID"
    :turn="0"
    :closeDetail="closeDetail"
  >
  </DetailedWork>
  <DialogTask
    v-if="displayTask == true"
    :header="headerAddTask"
    :visible="displayTask"
    :is_add="is_Add"
    :is_template="is_Template"
    :data="DialogData"
    :closeDialogTask="closeDialogTask"
    :afterSave="afterSave"
  >
  </DialogTask>
</template>
<style lang="scss" scoped>
@import "./style/followTemplate.scss";
</style>
<style lang="scss" scoped>
.w-custom {
  max-width: calc(100% - 67rem);
}
</style>
