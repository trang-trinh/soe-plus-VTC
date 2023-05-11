<script setup>
import { ref, inject, onMounted, watch, onBeforeUnmount } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import moment from "moment";
import { encr } from "../../util/function.js";
import treeuser from "../../components/task_origin/treeuser.vue/tree_user_task.vue";
import { useRoute } from "vue-router";
const cryoptojs = inject("cryptojs");
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const axios = inject("axios");
const store = inject("store");
const toast = useToast();
const swal = inject("$swal");
const router = inject("router");
const route = useRoute();
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

const selectedUser = ref([]);
const headerDialogUser = ref();
const is_one = ref(false);
const is_type = ref();
const basedomainURL = fileURL;
const componentKey = ref(0);
const headerAddTask = ref();
const displayTask = ref(false);
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
  { value: -1, text: "Bị xóa", bg_color: "red", text_color: "#FFFFFF" },
]);
let files = [];
const submitted = ref(false);
const displayDialogUser = ref(false);

const Task = ref({
  task_name: "",
  assign_user_id: [],
  work_user_ids: [],
  is_prioritize: false,
  is_deadline: true,
  is_review: true,
  is_security: false,
  weight: null,
  start_date: null,
  end_date: null,
  description: null,
  status: 0,
  target: null,
  request: null,
  is_XML: false,
  works_user_ids: [],
  follow_user_ids: [],
});
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
const listDropdownorganization = ref([]);
const listDropdownProject = ref([]);
const listDropdownUser = ref([]);
const listDropdownweight = ref([]);
const listDropdownTaskGroup = ref([]);
const props = defineProps({});
const v$ = useVuelidate(rules, Task);
const onUploadFile = (event) => {
  files = [];
  event.files.forEach((element) => {
    files.push(element);
  });
};
const removeFile = (event) => {
  files = files.filter((a) => a != event.file);
};
const closeDialogTask = () => {};

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

      listDropdownorganization.value = obj.arrtreeChils;
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
const listUser = () => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "[sys_user_list_tree]",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              {
                par: "filter_organization_id",
                va: null,
              },
              { par: "page_no", va: null },
              { par: "page_size", va: null },
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
      var data = response.data.data;
      if (data != null) {
        let tbs = JSON.parse(data);

        if (tbs[0] != null && tbs[0].length > 0) {
          tbs[0].forEach((element, i) => {
            if (element["created_date"] != null) {
              var ldate = element["created_date"].split(" ");
              element["created_date"] = ldate[0];
            }
          });
          if (tbs[0].length > 0) {
            tbs[0].forEach((element, i) => {
              element["STT"] = i + 1;
            });
          }
          listDropdownUser.value = tbs[0].map((x, i) => ({
            user_id: x.user_id,
            full_name: x.full_name,
            full_name_en: x.full_name_en,
            is_order: x.is_order,
            user_key: x.user_key,
            last_name: x.last_name,
            avatar: x.avatar,
            organization_id: x.organization_id,
            position_id: x.position_id,
            position_name: x.position_name,
            department_id: x.department_id,
            organization_name: x.organization_name,
            STT: x.STT,
          }));
        }
      }
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
const OpenDialogTreeUser = (one, type) => {
  selectedUser.value = [];
  if (type == 1) {
    Task.value.assign_user_id.forEach((t) => {
      let user = JSON.parse(JSON.stringify(t));
      selectedUser.value.push(user);
    });
    headerDialogUser.value = "Chọn người giao việc";
  } else if (type == 2) {
    Task.value.work_user_ids.forEach((t) => {
      let user = JSON.parse(JSON.stringify(t));
      selectedUser.value.push(user);
    });
    headerDialogUser.value = "Chọn người thực hiện";
  } else if (type == 3) {
    Task.value.works_user_ids.forEach((t) => {
      let user = JSON.parse(JSON.stringify(t));
      selectedUser.value.push(user);
    });
    headerDialogUser.value = "Chọn người đồng thực hiện";
  } else if (type == 4) {
    Task.value.follow_user_ids.forEach((t) => {
      let user = JSON.parse(JSON.stringify(t));
      selectedUser.value.push(user);
    });
    headerDialogUser.value = "Chọn người theo dõi";
  }
  displayDialogUser.value = true;
  is_one.value = one;
  is_type.value = type;
};
const closeDialog = () => {
  displayDialogUser.value = false;
};
const choiceTreeUser = (e) => {
  console.log(e, is_type.value);
  selectedUser.value = [];
  selectedUser.value = JSON.parse(JSON.stringify(e));
  switch (is_type.value) {
    case 1:
      if (selectedUser.value != null) {
        Task.value.assign_user_id = [];
        Task.value.assign_user_id.push(selectedUser.value);
      }
      break;
    case 2:
      if (selectedUser.value.length > 0) {
        Task.value.work_user_ids = [];
        selectedUser.value.forEach((t) => {
          Task.value.work_user_ids.push(t);
        });
      }
      break;
    case 3:
      if (selectedUser.value.length > 0) {
        Task.value.works_user_ids = [];
        selectedUser.value.forEach((t) => {
          Task.value.works_user_ids.push(t);
        });
      }
      break;
    case 4:
      if (selectedUser.value.length > 0) {
        Task.value.follow_user_ids = [];
        selectedUser.value.forEach((t) => {
          Task.value.follow_user_ids.push(t);
        });
      }
      break;
    default:
      break;
  }
  displayDialogUser.value = false;
};
onMounted(() => {
  displayTask.value = true;
  listProjectMain();
  listUser();
  listtreeOrganization();
});
</script>
<template>
  <Dialog
    :header="headerAddTask"
    v-model:visible="displayTask"
    :closable="true"
    :maximizable="true"
    :style="{ width: '45vw' }"
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
            :class="{ 'p-invalid': v$.task_name.$invalid && submitted }"
          />
        </div>
        <div
          style="display: flex"
          class="field col-12 md:col-12"
        >
          <div class="col-3 text-left"></div>
          <small
            v-if="
              (v$.task_name.$invalid && submitted) ||
              v$.task_name.$pending.$response
            "
            class="col-9 p-error p-0"
          >
            <span class="col-12 p-0">{{
              v$.task_name.required.$message
                .replace("Value", "Tên công việc")
                .replace("is required", "không được để trống")
            }}</span>
          </small>
        </div>

        <div class="field col-12 md:col-12 flex">
          <label class="col-3 text-left p-0">Công việc của phòng</label>
          <div class="col-3">
            <InputSwitch
              @change="ChangeIsDepartment(Task.is_department)"
              v-model="Task.is_department"
            />
          </div>

          <label class="col-3 text-left p-0">Công việc mẫu</label>
          <div class="col-3"><InputSwitch v-model="Task.is_template" /></div>
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
          <label class="col-3 text-left p-0">
            Người giao việc
            <Button
              icon="pi pi-user-plus"
              @click="OpenDialogTreeUser(true, 1)"
              class="p-button-text p-0"
              v-tooltip="'Chọn thành viên'"
            ></Button>
            <span class="redsao"> (*) </span></label
          >
          <MultiSelect
            :filter="true"
            v-model="Task.assign_user_id"
            :options="listDropdownUser"
            optionLabel="full_name"
            class="col-9 ip36 p-0"
            placeholder="Người giao việc"
            :class="{
              'p-invalid': Task.assign_user_id.length == 0 && submitted,
            }"
            :selectionLimit="1"
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
                      : (slotProps.option.last_name ?? '').substring(0, 1)
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
                  {{ slotProps.option.full_name }}
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
            <Button
              icon="pi pi-user-plus"
              @click="OpenDialogTreeUser(false, 2)"
              class="p-button-text p-0"
              v-tooltip="'Chọn thành viên'"
            ></Button>
            <span class="redsao"> (*) </span></label
          >
          <MultiSelect
            :filter="true"
            v-model="Task.work_user_ids"
            :options="listDropdownUser"
            optionLabel="full_name"
            class="col-9 ip36 p-0"
            placeholder="Người thực hiện"
            :class="{
              'p-invalid': Task.work_user_ids.length == 0 && submitted,
            }"
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
                      : (slotProps.option.last_name ?? '').substring(0, 1)
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
                  {{ slotProps.option.full_name }}
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
              (v$.work_user_ids.$invalid && submitted) ||
              v$.work_user_ids.$pending.$response
            "
            class="col-9 p-error p-0"
          >
            <span class="col-12 p-0">{{
              v$.work_user_ids.required.$message
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
            <Button
              icon="pi pi-user-plus"
              @click="OpenDialogTreeUser(false, 3)"
              class="p-button-text p-0"
              v-tooltip="'Chọn thành viên'"
            ></Button>
          </label>
          <MultiSelect
            :filter="true"
            v-model="Task.works_user_ids"
            :options="listDropdownUser"
            optionLabel="full_name"
            class="col-9 ip36 p-0"
            display="chip"
          >
            <template #option="slotProps">
              <div
                class="country-item flex"
                style="align-items: center; padding-left: 10px"
              >
                <Avatar
                  v-bind:label="
                    slotProps.option.avatar
                      ? ''
                      : (slotProps.option.last_name ?? '').substring(0, 1)
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
                  {{ slotProps.option.full_name }}
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
            <Button
              icon="pi pi-user-plus"
              @click="OpenDialogTreeUser(false, 4)"
              class="p-button-text p-0"
              v-tooltip="'Chọn thành viên'"
            ></Button
          ></label>
          <MultiSelect
            :filter="true"
            v-model="Task.follow_user_ids"
            :options="listDropdownUser"
            optionLabel="full_name"
            class="col-9 ip36 p-0"
            display="chip"
          >
            <template #option="slotProps">
              <div
                class="country-item flex"
                style="align-items: center; padding-left: 10px"
              >
                <Avatar
                  v-bind:label="
                    slotProps.option.avatar
                      ? ''
                      : (slotProps.option.last_name ?? '').substring(0, 1)
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
                  {{ slotProps.option.full_name }}
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
              :showIcon="true"
              id="time24"
              :showTime="true"
              autocomplete="on"
              class="col-5 ip36 title-lable"
              style="margin-top: 5px; padding: 0px"
              v-model="Task.start_date"
            />
            <!-- <Calendar
              :manualInput="true"
              :showIcon="true"
              class="col-5 ip36 title-lable"
              style="margin-top: 5px; padding: 0px"
              id="time1"
              hourFormat="12"
              autocomplete="on"
              v-model="Task.start_date"
            /> -->
            <div
              class="col-7"
              style="display: flex; padding: 0px; align-items: center"
            >
              <label class="col-5 text-center">Ngày kết thúc</label>

              <Calendar
                :showTime="true"
                :showIcon="true"
                class="col-7 ip36 title-lable"
                style="margin-top: 5px; padding: 0px"
                id="time2"
                placeholder="dd/MM/yy"
                autocomplete="on"
                v-model="Task.end_date"
                :class="{
                  'p-invalid':
                    v$.end_date.$invalid && submitted && Task.is_deadline,
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
              (v$.end_date.$invalid && submitted && Task.is_deadline) ||
              (v$.end_date.$pending.$response && Task.is_deadline)
            "
            class="col-9 p-error p-0"
          >
            <span class="col-12 p-0">{{
              v$.end_date.required.$message
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
        <!-- <div
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
        </div> -->
        <div class="field col-12 md:col-12">
          <Accordion :multiple="true">
            <AccordionTab header="THÔNG TIN KHÁC">
              <div
                v-if="Task.is_department"
                class="field col-12 md:col-12"
              >
                <label class="col-3 text-left p-0"
                  >Người thực hiện
                  <Button
                    icon="pi pi-user-plus"
                    @click="OpenDialogTreeUser(false, 2)"
                    class="p-button-text p-0"
                    v-tooltip="'Chọn thành viên'"
                  ></Button>
                </label>
                <MultiSelect
                  :filter="true"
                  v-model="Task.work_user_ids"
                  :options="listDropdownUser"
                  optionLabel="full_name"
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
                        v-bind:label="
                          slotProps.option.avatar
                            ? ''
                            : (slotProps.option.last_name ?? '').substring(0, 1)
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
                        {{ slotProps.option.full_name }}
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
                  <Button
                    icon="pi pi-user-plus"
                    @click="OpenDialogTreeUser(true, 3)"
                    class="p-button-text p-0"
                    v-tooltip="'Chọn thành viên'"
                  ></Button>
                </label>
                <MultiSelect
                  :filter="true"
                  v-model="Task.works_user_ids"
                  :options="listDropdownUser"
                  optionLabel="full_name"
                  class="col-9 ip36 p-0"
                  display="chip"
                >
                  <template #option="slotProps">
                    <div
                      class="country-item flex"
                      style="align-items: center; padding-left: 10px"
                    >
                      <Avatar
                        v-bind:label="
                          slotProps.option.avatar
                            ? ''
                            : (slotProps.option.last_name ?? '').substring(0, 1)
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
                        {{ slotProps.option.full_name }}
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
                  <Button
                    icon="pi pi-user-plus"
                    @click="OpenDialogTreeUser(false, 4)"
                    class="p-button-text p-0"
                    v-tooltip="'Chọn thành viên'"
                  ></Button>
                </label>
                <MultiSelect
                  :filter="true"
                  v-model="Task.follow_user_ids"
                  :options="listDropdownUser"
                  optionLabel="full_name"
                  class="col-9 ip36 p-0"
                  display="chip"
                >
                  <template #option="slotProps">
                    <div
                      class="country-item flex"
                      style="align-items: center; padding-left: 10px"
                    >
                      <Avatar
                        v-bind:label="
                          slotProps.option.avatar
                            ? ''
                            : (slotProps.option.last_name ?? '').substring(0, 1)
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
                        {{ slotProps.option.full_name }}
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
                    v-if="Task.files && Task.files.length > 0 && !isAdd"
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
        @click="saveTask(!v$.$invalid)"
      />
    </template>
  </Dialog>

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
<style lang="scss" scoped></style>
