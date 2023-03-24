<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { encr } from "../../../util/function.js";
import moment from "moment";
import TaskFollowDetailVue from "./follow/TaskFollowDetail.vue";
import { FilterMatchMode, FilterOperator } from "primevue/api";
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
const addLog = (log) => {
  // eslint-disable-next-line no-undef
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
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

const props = defineProps({
  componentKey: Intl,
  id: String,
  pj_id: String,
  listChild: Array,
  member: Array,
  data: Object,
  isClose: Boolean,
});
const listChildTask = ref([]);
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
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_follow_list",
            par: [{ par: "task_id", va: props.id }],
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
      if (data.length > 0)
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
            x.StepProgress =
              Math.floor(x.countStepFinished / x.countStep) * 100;
          }
          x.TaskProgress = 0;
          if (x.countTask > 0) {
            x.TaskProgress =
              Math.floor(x.countTaskFinished / x.countTask) * 100;
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
                y.TaskProgress =
                  Math.floor(y.countTaskFinished / y.countTask) * 100;
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
            x.task_follow_step = [];
          }
        });
      datalists.value = data;
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
    .catch((error) => {
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
// 0: tạo/giao 1,2: làm, 3:theo dõi
const TypeMember = ref();
const expandedRows = ref([]);
const expandedRows2 = ref([]);
onMounted(() => {
  loadData();
  if (props.listChild != null) {
    listChildTask.value = JSON.parse(JSON.stringify(props.listChild));
  }
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
      v-model:expandedRows="expandedRows"
    >
      <Toolbar class="w-full custoolbar">
        <template #end>
          <Button
            icon="pi pi-plus"
            label="Thêm quy trình"
            @click="openDialog()"
            v-if="user.is_admin == true || TypeMember == 0"
          ></Button>
        </template>
      </Toolbar>
      <Column
        expander
        class="max-w-4rem"
      />
      <Column
        header="Tên quy trình"
        field="follow_name"
        header-class="justify-content-center align-items-center text-center"
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
        class="justify-content-center align-items-center max-w-10rem"
      >
        <template #body="data">
          <div
            class="w-full justify-content-center align-items-center text-center max-w-10rem"
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
          >
            {{ data.data.status_display.label }}
          </span>
        </template>
      </Column>
      <Column
        header="Chức năng"
        field=""
        class="justify-content-center align-items-center max-w-10rem"
      >
        <template #body="data">
          <div class="flex">
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
          <Toolbar class="w-full custoolbar">
            <template #start>
              <span>
                Thông tin quy trình:
                <span class="px-1 font-bold line-height-3">{{
                  slotProps.data.follow_name
                }}</span></span
              >
            </template>
            <template #end>
              <Button
                icon="pi pi-plus"
                label="Thêm bước"
                @click="openStepDialog(slotProps.data)"
                v-if="user.is_admin == true || TypeMember == 0"
              ></Button>
            </template>
          </Toolbar>
          <Accordion
            :activeIndex="0"
            class="w-full"
          >
            <AccordionTab header="Mô tả quy trình">
              <span
                class="max-h-15rem"
                style="overflow-y: auto"
              >
                <span
                  v-html="slotProps.data.description"
                  v-if="slotProps.data.description != null"
                ></span>
                <b v-else>Không có mô tả cho quy trình này!</b>
              </span>
            </AccordionTab>
          </Accordion>

          <DataTable
            :value="slotProps.data.task_follow_step"
            v-model:expandedRows="expandedRows2"
          >
            <Column
              expander
              class="max-w-3rem"
            />
            <Column
              header="Thứ tự"
              field="is_step"
              class="justify-content-center align-items-center text-center max-w-5rem"
            >
            </Column>
            <Column
              header="Tên bước"
              field="step_name"
              header-class="justify-content-center align-items-center text-center"
            ></Column>
            <Column
              header="Công việc"
              field="step_name"
              class="justify-content-center align-items-center text-center max-w-10rem"
            >
              <template #body="data">
                <div
                  class="w-full justify-content-center align-items-center text-center max-w-10rem"
                >
                  <div>
                    {{ data.data.countTaskFinished }} /
                    {{ data.data.countTask }}
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
              field="task_id_follow"
              class="justify-content-center align-items-center text-center max-w-13rem"
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
                >
                  {{ data.data.status_display.label }}
                </span>
              </template></Column
            >
            <Column
              header="Chức năng"
              field=""
              class="justify-content-center align-items-center max-w-8rem"
            >
              <template #body="data">
                <div class="flex">
                  <Button
                    class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                    type="button"
                    icon="pi pi-pencil"
                    v-tooltip="'Sửa'"
                    @click="openEditStepDialog(data.data, slotProps.data)"
                    v-if="user.is_admin == true || TypeMember == 0"
                  >
                  </Button>
                  <Button
                    @click="DeleteStep(data.data, true)"
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
                <Toolbar class="w-full custoolbar">
                  <template #start>
                    <span>
                      Thông tin bước:
                      <span class="px-1 font-bold line-height-3">{{
                        slotProps.data.step_name
                      }}</span></span
                    >
                  </template>
                </Toolbar>
                <Accordion
                  :activeIndex="0"
                  class="w-full"
                >
                  <AccordionTab header="Mô tả bước">
                    <span
                      class="max-h-15rem"
                      style="overflow-y: auto"
                    >
                      {{ slotProps.data.description }}
                      <span
                        v-html="slotProps.data.description"
                        v-if="
                          slotProps.data.description != null &&
                          slotProps.data.description != ''
                        "
                      ></span>
                      <b v-else>Không có mô tả cho bước này!</b>
                    </span>
                  </AccordionTab>
                </Accordion>

                <DataTable
                  :value="slotProps.data.task_info"
                  @rowReorder="onRowReorder"
                >
                  <Column
                    v-if="TypeMember.value == 0"
                    rowReorder
                    class="max-w-2rem justify-content-center align-items-center text-center"
                  />
                  <Column
                    header="STT"
                    field="step"
                    class="justify-content-center align-items-center text-center max-w-5rem"
                  ></Column>
                  <Column
                    header="Tên công việc"
                    field="task_id_follow"
                    header-class="justify-content-center align-items-center text-center"
                  >
                    <template #body="data">
                      <div>
                        <span class="font-bold text-xl">
                          {{ data.data.task_name }}
                        </span>
                        <br />
                        <span>
                          {{
                            moment(new Date(data.data.start_date)).format(
                              "DD/MM/YYYY",
                            )
                          }}
                        </span>
                        -
                        <span v-if="data.data.is_deadline == true">
                          {{
                            moment(new Date(data.data.end_date)).format(
                              "DD/MM/YYYY",
                            )
                          }}
                        </span>
                      </div>
                    </template>
                  </Column>
                  <Column
                    header="Tiến độ"
                    field="task_id_follow"
                    class="justify-content-center align-items-center text-center max-w-8rem"
                  >
                    <template #body="data">
                      <div v-if="data.data.progress > 0">
                        <ProgressBar :value="data.data.progress" />
                      </div>
                      <div
                        class="pt-2"
                        v-else
                      >
                        0%
                      </div>
                    </template></Column
                  >
                  <Column
                    header="Trạng thái"
                    field="task_id_follow"
                    class="justify-content-center align-items-center text-center max-w-13rem"
                  >
                    <template #body="data">
                      <span
                        :style="{
                          background: data.data.status_display.bg_color,
                          color: data.data.status_display.text_color,
                          padding: '5px 10px',
                          border:
                            '1px solid' + data.data.status_display.bg_color,
                          borderRadius: '5px',
                        }"
                      >
                        {{ data.data.status_display.text }}
                      </span>
                    </template></Column
                  >

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
                </DataTable>
              </div>
            </template>
          </DataTable>
        </div>
      </template>
    </DataTable>
    <TaskFollowDetailVue
      :componentKey="componentKey"
      :data="round"
      :isOpen="isOpen"
      :closeDialogDetail="closeDialogDetail"
      :rowReorder="onRowReorder"
      :memberType="TypeMember"
    ></TaskFollowDetailVue>
  </div>
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
          :minDate="new Date(props.data.start_date)"
          :maxDate="new Date(props.data.end_date)"
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
          :minDate="
            taskfollow.start_date != null
              ? new Date(taskfollow.start_date)
              : new Date()
          "
          showButtonBar
          :maxDate="new Date(props.data.end_date)"
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
        <div class="col-8 p-0">
          <MultiSelect
            :filter="true"
            v-model="listTask"
            :options="listChildTask"
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
</template>

<style lang="scss" scoped>
.h-custom {
  height: calc(100vh - 5rem);
}
</style>
