<script setup>
import { ref, inject, onMounted, onBeforeMount, watch } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { encr } from "../../../util/function.js";
import moment from "moment";
import TaskFollowDetailVue from "./follow/TaskFollowDetail.vue";
import DetailedWork from "../DetailedWork.vue";
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

const width1 = window.screen.width;
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

const props = defineProps({
  componentKey: Intl,
  id: String,
  pj_id: String,
  listChild: Array,
  member: Array,
  data: Object,
  isClose: Boolean,
  openAddTask: Function,
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
  start_real_date: null,
  end_real_date: null,
  type: null,
  weight: null,
  status: null,
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
    task_id: props.id,
    follow_name: null,
    description: null,
    start_date: null,
    end_date: null,
    start_real_date: null,
    end_real_date: null,
    type: 0,
    weight: 0,
    status: 0,
    is_step: 1,
  };
  DialogVisible.value = true;
  listTask.value = [];
  headerDialog.value = "Tạo quy trình công việc";
};
const OpenEditDialog = (data) => {
  submitted.value = false;
  isEdit.value = true;
  let edit = JSON.parse(JSON.stringify(data));
  edit.start_date = edit.start_date ? new Date(edit.start_date) : null;
  edit.end_date = edit.end_date ? new Date(edit.end_date) : null;
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
            proc: "task_follow_list",
            par: [
              { par: "task_id", va: props.id },
              { par: "user_id", va: null },
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
      if (data.length > 0) {
        data.forEach((x) => {
          let type = listDrdType.value.filter((xz) => xz.value == x.type)[0];
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
            x.task_follow_step.forEach((y) => {
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
                  let k = props.listChild.filter(
                    (a) => a.task_id == z.task_id_follow,
                  );
                  if (k.length > 0) {
                    let obj = Object.assign({}, z, k[0]);
                    y.task_info.push(obj);
                  }
                });
              }
            });
          } else {
            x.task_follow_step = null;
          }
        });
        datalists.value = data;

        if (datalists.value.length > 0) {
          expandAll(datalists.value);
        }
      } else {
        datalists.value = [];
      }
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
  let dataFilter = e.filter((x) => x.status == 1)[0];
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
const tempMinDate = ref();
const tempMaxDate = ref();
const StepDialogVisible = ref(false);
const headerStepDialog = ref();
const openStepDialog = (e) => {
  submitted.value = false;
  isEdit.value = false;
  taskStep.value = {
    follow_id: "",
    task_id: props.id,
    step_name: "",
    description: "",
    start_date: "",
    end_date: "",
    type: 0,
    status: 0,
    is_step: 1,
  };
  tempMinDate.value = e.start_date ? e.start_date : props.data.start_date;
  tempMaxDate.value = e.end_date ? e.end_date : props.data.end_date;
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
  tempMinDate.value = f.start_date ? f.start_date : props.data.start_date;
  tempMaxDate.value = f.end_date ? f.end_date : props.data.end_date;

  StepDialogVisible.value = true;
  listTask.value = [];
  e.task_info.forEach((x) => {
    let k = props.listChild.filter((a) => a.task_id == x.task_id_follow);
    if (k != null) {
      listTask.value = listTask.value.concat(k);
    }
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
const saveStep = (isFormValid) => {
  submitted.value = true;
  if (!isFormValid) {
    return;
  }
  let formData = new FormData();
  let listID = [];
  listTask.value.forEach((x) => {
    listID.push({ task_id_follow: x.task_id });
  });

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
};
const items = ref([
  {
    value: 0,
    label: "Chưa bắt đầu",
    bg_color: "#bbbbbb",
    text_color: "#FFFFFF",
    command: () => {},
  },
  {
    value: 1,
    label: "Đang làm",
    bg_color: "#2196f3",
    text_color: "#FFFFFF",
    command: () => {},
  },
  {
    value: 2,
    label: "Hoàn thành đúng hạn",
    bg_color: "#04D215",
    text_color: "#FFFFFF",
    command: () => {},
  },
  {
    value: 3,
    label: "Hoàn thành sau hạn",
    bg_color: "#ff8b4e",
    text_color: "#FFFFFF",
    command: () => {},
  },
  {
    value: 4,
    label: "Tạm ngưng",
    bg_color: "#d87777",
    text_color: "#FFFFFF",
    command: () => {},
  },
  {
    value: 5,
    label: "Đóng",
    bg_color: "#d87777",
    text_color: "#FFFFFF",
    command: () => {},
  },
]);
const menu = ref();
const menuPos = ref();
const menuData = ref();
const toggle = (event, e, data) => {
  menu.value.toggle(event);
  menuPos.value = e;
  menuData.value = data;
};
const changeStatus = (e) => {
  if (menuData.value.status == e) {
    toast.success(
      "Quy trình đang có trạng thái: " +
        listDrdStatus.value.filter((z) => z.value == e)[0].label,
    );
    return;
  } else {
    if (e == 5) {
      if (menuData.value.status == 3) {
        toast.success(
          menuPos.value == 1
            ? "Quy trình đang đóng!"
            : "Bước thực hiện đang đóng",
        );
        return;
      }
      swal
        .fire({
          title: "Thông báo",
          html:
            "Bạn có chắc chắn muốn đóng " +
            (menuPos.value == 1 ? "quy trình" : "bước thực hiện") +
            " này không!<br/>(Hành động này sẽ đóng tất cả " +
            (menuPos.value == 1 ? "quy trình" : "bước thực hiện") +
            " thuộc bước hiện tại!)",
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
            UpdateStatusTaksFunc(e);
          }
        });
    } else {
      UpdateStatusTaksFunc(e);
    }
  }
};
const UpdateStatusTaksFunc = (e) => {
  let formData = new FormData();
  menuData.value.status = e;
  formData.append("model", JSON.stringify(menuData.value));
  axios
    .put(
      baseURL +
        "/api/" +
        (menuPos.value == 1 ? "task_follow/" : "task_follow_step/") +
        (menuPos.value == 1 ? "Update_Status_Follow" : "Update_Status_Step"),
      formData,
      config,
    )
    .then((response) => {
      if (response.data.err != "1") {
        menu.value.hide();
        swal.close();
        toast.success("Cập nhật trạng thái công việc thành công!");
        loadData(true);
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
const expandedRows = ref([]);

const selectStep = (e, i) => {
  e.index = i;
};
const closeDetail = () => {
  showDetail.value = false;
  selectedTaskID.value = null;
  loadData();
};
const CopyFollow = ref(false);
const headerCopyFollow = ref();
const listDropdownFollow = ref([]);
const selectedFollowTemplate = ref();
const loadFollowTemplate = () => {
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
      listDropdownFollow.value = [];
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        data.forEach((x, i) => {
          let k = {
            label: x.follow_name,
            value: x.follow_id,
          };
          listDropdownFollow.value.push(k);
          if (i == 0) {
            selectedFollowTemplate.value = k.value;
          }
        });
      }
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
const AddTemplateToTask = () => {
  submitted2.value = true;
  if (selectedFollowTemplate.value == null) {
    return;
  }
};
const submitted2 = ref(false);
const openSelectFollow = () => {
  CopyFollow.value = true;
  headerCopyFollow.value = "Chọn quy trình công việc";
  loadFollowTemplate();
  submitted2.value = false;
};
onMounted(() => {
  let type = [];
  props.member.forEach((x) => {
    if (x.user_id == user.user_id) {
      type.push(x.is_type);
    }
  });
  if (
    props.data.created_by == user.user_id ||
    type.filter((x) => x == 0) != null
  ) {
    TypeMember.value = 0;
  } else if (type.filter((x) => x == 1) != null) {
    TypeMember.value = 1;
  } else if (type.filter((x) => x == 2) != null) {
    TypeMember.value = 1;
  } else if (type.filter((x) => x == 3) != null) {
    TypeMember.value = 3;
  } else {
    TypeMember.value = 4;
  }
});
</script>
<template>
  <div class="h-custom">
    <DataTable
      :value="datalists"
      scrollable
      scrollHeight="flex"
      dataKey="follow_id"
      v-model:expandedRows="expandedRows"
    >
      <Toolbar class="w-full custoolbar">
        <template #end>
          <Button
            icon="pi pi-plus"
            class="mx-1"
            label="Chọn quy trình mẫu"
            @click="openSelectFollow()"
            v-if="
              (user.is_admin == true || TypeMember == 0) &&
              props.isClose != true
            "
          ></Button>
          <Button
            icon="pi pi-plus"
            label="Thêm quy trình"
            @click="openDialog()"
            v-if="
              (user.is_admin == true || TypeMember == 0) &&
              props.isClose != true
            "
          ></Button>
        </template>
      </Toolbar>
      <Column
        expander
        class="max-w-3rem"
      />
      <Column
        header="Tên quy trình"
        field="follow_name"
        headerClass="justify-content-center align-items-center text-center "
        bodyClass="word-break-break-all"
      ></Column>
      <Column
        header="Bước"
        field="follow_name"
        class="justify-content-center align-items-center max-w-10rem"
      >
        <template #body="data">
          <div
            class="w-full justify-content-center align-items-center text-center max-w-10rem"
          >
            <div>
              {{ data.data.countStepFinished }} / {{ data.data.countStep }}
            </div>
            <div v-if="data.data.StepProgress > 0">
              <ProgressBar :value="data.data.StepProgress" />
            </div>
            <div
              class="pt-2"
              v-else
            >
              0%
            </div>
          </div>
        </template>
      </Column>

      <Column
        header="Công việc"
        field="follow_name"
        class="justify-content-center align-items-center max-w-8rem"
      >
        <template #body="data">
          <div
            class="w-full justify-content-center align-items-center text-center max-w-8rem"
          >
            <div>
              {{ data.data.countTaskFinished }} / {{ data.data.countTask }}
            </div>
            <div v-if="data.data.TaskProgress > 0">
              <ProgressBar :value="data.data.TaskProgress" />
            </div>
            <div
              class="pt-2"
              v-else
            >
              0%
            </div>
          </div>
        </template>
      </Column>
      <Column
        header="Trạng thái"
        field=""
        class="justify-content-center align-items-center max-w-13rem"
      >
        <template #body="data">
          <span
            :style="{
              background: data.data.status_display.bg_color,
              color: data.data.status_display.text_color,
              padding: '5px 10px',
              border: '1px solid' + data.data.status_display.bg_color,
              borderRadius: '5px',
            }"
            @click="
              toggle($event, 1, {
                follow_id: data.data.follow_id,
                status: data.data.status,
              })
            "
            aria-haspopup="true"
            aria-controls="overlay_menu"
          >
            {{ data.data.status_display.label }}
          </span>
        </template>
      </Column>
      <Column
        header="Chức năng"
        field=""
        class="justify-content-center align-items-center max-w-12rem"
      >
        <template #body="data">
          <div class="flex">
            <Button
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              icon="pi pi-plus"
              v-tooltip="'Thêm bước'"
              v-if="
                (user.is_admin == true || TypeMember == 0) &&
                data.data != null &&
                props.isClose != true
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
              v-if="
                (user.is_admin == true || TypeMember == 0) &&
                props.isClose != true
              "
            >
            </Button>
            <Button
              @click="DeleteItem(data.data, true)"
              class="p-button-rounded p-button-secondary p-button-outlined mx-1"
              type="button"
              v-tooltip="'Xóa'"
              icon="pi pi-trash"
              v-if="
                (user.is_admin == true || TypeMember == 0) &&
                props.isClose != true
              "
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
        <div>
          <div
            v-if="
              slotProps.data.task_follow_step != null &&
              slotProps.data.task_follow_step.length > 0
            "
          >
            <div class="multi-step numbered">
              <div class="buttonfunc">
                <!-- <Button
                  class="mx-1"
                  icon="pi pi-plus"
                  v-tooltip="'Thêm bước'"
                  v-if="
                    (user.is_admin == true || TypeMember == 0) &&
                    slotProps.data != null &&
                    props.isClose != true
                  "
                  @click="openStepDialog(slotProps.data)"
                ></Button> -->
                <Button
                  class="mx-1 p-button-outlined p-button-rounded"
                  icon="pi pi-pencil"
                  v-tooltip="'Sửa bước đang chọn'"
                  v-if="
                    (user.is_admin == true || TypeMember == 0) &&
                    (slotProps.data.index
                      ? slotProps.data.index
                      : indexSelected) != null &&
                    props.isClose != true
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
                      : indexSelected) != null &&
                    props.isClose != true
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
              <ul class="multi-step-list">
                <li
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
                  @click="selectStep(item, index)"
                >
                  <div
                    class="item-wrap flex align-items-center justify-content-center"
                  >
                    <p class="item-title text-center">{{ item.step_name }}</p>
                  </div>
                </li>
              </ul>
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
            <h3 class="m-1">Quy trình chưa có các bước thực hiện!</h3>
          </div>
          <div
            v-if="
              slotProps.data.task_follow_step != null &&
              slotProps.data.task_follow_step.length > 0
            "
          >
            <div
              v-if="
                slotProps.data.task_follow_step[indexSelected].task_info
                  .length > 0
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
                    <span class="font-bold text-xl">
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
                  <template #footer>
                    <div
                      v-if="item2.progress != 0"
                      style="width: 100%"
                    >
                      <ProgressBar :value="item2.progress ?? 0" /></div
                  ></template>
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
        <div class="col-4 flex align-items-center">Ngày bắt đầu</div>
        <Calendar
          v-model="taskfollow.start_date"
          :showIcon="true"
          :showTime="true"
          class="col-3 px-0"
          :manualInput="false"
          :minDate="new Date(props.data.start_date)"
          :maxDate="new Date(props.data.end_date)"
          showButtonBar
        >
        </Calendar>
        <div class="col-2 flex align-items-center justify-content-center">
          Ngày kết thúc
        </div>
        <Calendar
          v-model="taskfollow.end_date"
          :showIcon="true"
          :showTime="true"
          class="col-3 px-0"
          :manualInput="false"
          :minDate="
            taskfollow.start_date != null
              ? new Date(taskfollow.start_date)
              : new Date()
          "
          :maxDate="new Date(props.data.end_date)"
          showButtonBar
        >
        </Calendar>
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
      <div class="col-12 flex">
        <div class="col-4 flex align-items-center">Trạng thái</div>
        <Dropdown
          :filter="true"
          v-model="taskfollow.status"
          :options="listDrdStatus"
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
        <Calendar
          v-model="taskStep.start_date"
          :showIcon="true"
          :showTime="true"
          class="col-3 px-0"
          :manualInput="false"
          :minDate="new Date(tempMinDate)"
          :maxDate="new Date(tempMaxDate)"
          showButtonBar
        >
        </Calendar>
        <div class="col-2 flex align-items-center justify-content-center">
          Ngày kết thúc
        </div>
        <Calendar
          v-model="taskStep.end_date"
          :showIcon="true"
          :showTime="true"
          class="col-3 px-0"
          :manualInput="false"
          showButtonBar
          :minDate="new Date(tempMinDate)"
          :maxDate="new Date(tempMaxDate)"
        >
        </Calendar>
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
        <div class="col-4 flex align-items-center">Trạng thái</div>
        <Dropdown
          :filter="true"
          v-model="taskStep.status"
          :options="listDrdStatus"
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
            :options="props.listChild"
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
            @click="openAddTask(props.data)"
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
  <Menu
    ref="menu"
    id="overlay_menu"
    :model="items"
    :popup="true"
  >
    <template #item="{ item }">
      <div
        class="w-full p-2"
        @click="changeStatus(item.value)"
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
            :style="{ background: item.bg_color }"
          />
          {{ item.label }}
        </span>
      </div>
    </template>
  </Menu>
  <DetailedWork
    v-if="showDetail === true"
    :id="selectedTaskID"
    :turn="0"
    :closeDetail="closeDetail"
  >
  </DetailedWork>
  <Dialog
    v-model:visible="CopyFollow"
    :style="'width:40vw;'"
    :closable="false"
    :header="headerCopyFollow"
  >
    <div class="col-12 flex">
      <div class="col-4">Quy trình mẫu:</div>
      <Dropdown
        v-model="selectedFollowTemplate"
        :options="listDropdownFollow"
        optionLabel="label"
        placeholder="Chọn trình tự"
        panelClass="d-design-dropdown"
        class="col-8 py-0"
        optionValue="value"
        filter
        showClear
      >
      </Dropdown>
    </div>
    <div class="col-12 flex p-0">
      <div class="col-4 p-0"></div>
      <div class="col-8">
        <small
          v-if="submitted2 && selectedFollowTemplate == null"
          :class="{ 'p-error': submitted2 && selectedFollowTemplate == null }"
        >
          Mẫu quy trình không được để trống!
        </small>
      </div>
    </div>
    <template #footer>
      <div class="mt-2">
        <Button
          class="p-button-text"
          icon="pi pi-times"
          label="Đóng"
          @click="CopyFollow = false"
        />
        <Button
          icon="pi pi-check"
          label="Xác nhận"
          @click="AddTemplateToTask()"
        />
      </div>
    </template>
  </Dialog>
</template>
<style lang="scss" scoped>
// Variables
$base-margin: 2em;
$base-padding: 1em;
$base-border-radius: 0.2em;
$screen-xs-max: 786px;

$screen-qHD: 2048px;
$screen-fHD: 1920px;
$screen-Mac: 1440px;
$screen-HD: 1366px;

$text-color: #263238;
$text-color-inverted: #fff;
$clickable-hover: #d8f1ff;

$brand-primary: #2196f3;
$brand-success: #26a25f;
$brand-danger: #f82502;

$accent-dark: #2196f3;
$accent-light: #2196f3;
$accent-lighter: #ffffff;

$icon-danger: "!";
$icon-success: "✓";

$animation-time: 0.5s;

.multi-step {
  margin: 0;
}

.multi-step-list {
  position: relative;
  display: flex;
  flex-direction: row;
  justify-content: flex-start;
  list-style-type: none;
  padding: 10px 10px 0px 10px;
  overflow: auto;
  background: #efefef;

  @media only screen and (max-width: $screen-qHD) {
    max-width: 46.9vw;
  }
  @media only screen and (max-width: $screen-fHD) {
    max-width: 46.9vw;
  }
  @media only screen and (max-width: $screen-Mac) {
    max-width: 54vw;
  }
  @media only screen and (max-width: $screen-HD) {
    max-width: 53.9vw;
  }
  .multi-step-item:first-child {
    margin-left: 0;
  }
  .multi-step-item:last-child {
    margin-right: 0;
  }
}

// Defaults for each 'step'
.multi-step-item {
  position: relative;
  width: 100%;
  margin: 0 calc($base-margin / 6);
  @media only screen and (max-width: $screen-xs-max) {
    margin: 0 calc($base-margin / 6);
  }
  z-index: 2;
  border-radius: $base-border-radius;

  // Step title and subtitle defaults
  .item-title,
  .item-subtitle {
    position: relative;
    margin: 0;
    z-index: 2;
  }
  @media only screen and (max-width: $screen-xs-max) {
    .item-subtitle {
      display: none;
    }
  }
  .item-title {
    color: #72777a;
    font-weight: 600;
    margin: 0;
    white-space: nowrap;
    max-width: 10rem;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  // Different step states [ active, current, completed, error]
  &.active:hover {
    cursor: pointer;
  }
  &.current .item-title,
  &.current .item-subtitle {
    color: $text-color-inverted;
  }
  &.active.current:hover .item-title,
  &.active.current:hover .item-subtitle {
    color: $brand-primary;
  }
  &.error:after {
    position: absolute;
    top: 50%;
    z-index: 2;
    transform: translateY(-50%);
    right: 0.5em;

    content: $icon-danger;
    color: $brand-danger;
  }
  :hover .item-title {
    color: $brand-primary;
    font-weight: 600;
    margin: 0;
    white-space: nowrap;
    max-width: max-content;
  }
}

.item-wrap {
  padding: $base-padding;
  position: relative;
  height: 100%;
  &:before,
  &:after {
    position: absolute;
    left: 0;
    content: " ";
    width: 100%;
    height: 50.5%;
    z-index: 1;
    background-color: $accent-lighter;
  }

  // Top of the arrow
  &:before {
    top: 0;
    transform: skew(20deg);
    border-radius: 0.2em 0.2em 0 0;
  }
  // Bottom of the arrow
  &:after {
    bottom: 0;
    transform: skew(-20deg);
    border-radius: 0 0 0.2em 0.2em;
  }
}

// Changing arrow colors based on state
.current .item-wrap:before,
.current .item-wrap:after {
  background-color: $brand-primary;
}

.active:hover .item-wrap:before,
.active:hover .item-wrap:after {
  background-color: $clickable-hover;
}

.multi-step-item.error {
  .item-title,
  .item-subtitle {
    padding-right: ($base-padding * 2);
  }
}

// Changing step styles based on :first/:last step
.multi-step-item:first-child .item-wrap,
.multi-step-item:last-child .item-wrap {
  width: 100%;
  border-radius: $base-border-radius;
  &:before,
  &:after {
    width: 50%;
  }
}

// If first step, only point on the right
.multi-step-item:first-child .item-wrap {
  background: linear-gradient(to right, $accent-lighter 95%, transparent 5%);
  &:before,
  &:after {
    left: 50%;
  }
}
.active.multi-step-item:first-child:hover .item-wrap {
  background: linear-gradient(to right, $clickable-hover 95%, transparent 5%);
}
.current.multi-step-item:first-child .item-wrap {
  background: linear-gradient(to right, $brand-primary 95%, transparent 5%);
}

// If last step, only indent on the left
.multi-step-item:last-child .item-wrap {
  background: linear-gradient(to left, $accent-lighter 95%, transparent 5%);
  &:before,
  &:after {
    right: 50%;
  }
}
.active.multi-step-item:last-child:hover .item-wrap {
  background: linear-gradient(to left, $clickable-hover 95%, transparent 5%);
}
.current.multi-step-item:last-child .item-wrap {
  background: linear-gradient(to left, $brand-primary 95%, transparent 5%);
}

// MSI Checked & Complete
.checked .multi-step-item.completed:after {
  position: absolute;
  top: 50%;
  z-index: 2;
  transform: translateY(-50%);
  right: 0.5em;
  content: $icon-success;
  color: $brand-success;
}

// MSI Numbered
.numbered .multi-step-item {
  counter-increment: step-counter;
  max-width: max-content;
  .item-wrap {
    padding-left: ($base-padding * 5);
  }

  // Adds number to step
  &:before {
    content: counter(step-counter);
    position: absolute;
    top: 50%;
    left: 0.75em;
    transform: translateY(-50%);
    min-width: fit-content;
    padding: calc($base-padding / 2) $base-padding;
    z-index: 2;
    font-size: 0.85em;
    //background-color: $accent-light;
    //color: $text-color-inverted;
    background-color: $accent-light;
    color: $text-color-inverted;
    font-weight: 600;
    text-align: unset;
    border-radius: $base-border-radius;
  }
}

// MSI w/ badge counts
.item-wrap .badge {
  position: absolute;
  right: 0.5em;
  top: 50%;
  transform: translateY(-50%);
  z-index: 3;
}
.error .item-wrap .badge {
  right: 2em;
  ~ .item-title,
  ~ .item-subtitle {
    padding-right: 3em;
  }
}

// MSI CSS Loader
.multi-step-loading {
  opacity: 0.75;
}

.current.multi-step-loading:before {
  border-color: $text-color-inverted;
  border-top-color: transparent;
  opacity: 1;
}

.busy-css {
  z-index: 3;
  content: "";
  position: absolute;
  top: 50%;
  left: 50%;
  margin-top: -0.5em;
  margin-left: -0.5em;
  border-radius: 50%;
  width: 1em;
  height: 1em;
  border: 0.25em solid $accent-dark;
  border-top-color: transparent;
  animation: spin ($animation-time * 2) infinite linear;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
::-webkit-scrollbar-thumb {
  background-color: #fff;
}
.p-card :hover {
  background: #c5e5ff !important;
}
.buttonfunc {
  position: absolute;
  right: 0px;
  max-width: 12rem;
  z-index: 100;
  display: none;
  padding: 0 0.5rem 0 0;
}
.multi-step {
  .multi-step-list {
    padding-top: 3rem;
  }
}
.multi-step:hover {
  .buttonfunc {
    display: unset;
  }
}
</style>
