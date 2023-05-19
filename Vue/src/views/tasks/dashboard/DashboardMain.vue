<script setup>
import { ref, onMounted, inject } from "vue";
import MyTaskInfo from "./dashboardComponent/MyTaskInfo.vue";
import MembersTask from "./dashboardComponent/MembersTask.vue";
import OrganizationTasks from "./dashboardComponent/OrganizationTask.vue";
import TaskReport from "./dashboardComponent/TaskReport.vue";
import TaskReview from "./dashboardComponent/TaskReview.vue";
import TaskExtendDashboard from "./dashboardComponent/TaskExtendDashboard.vue";
import DepartmentTask from "./dashboardComponent/DepartmentTask.vue";

const emitter = inject("emitter");
emitter.on("count", (obj) => {
  ListButtonLabel.value[5].badgeCount = obj.data[0].report;
  ListButtonLabel.value[6].badgeCount = obj.data[0].extend;
});
const listDropdownProject = ref([]);
const listDropdownGroup = ref([]);
emitter.on("listDropdown", (obj) => {
  let temp1 = JSON.parse(obj.data[0].list_project);
  let temp2 = JSON.parse(obj.data[0].list_group);
  listDropdownProject.value = temp1;
  listDropdownGroup.value = temp2;
});
const ListButtonLabel = ref([
  {
    label: "Cá nhân",
    icon: "pi pi-user",
    code: "0",
    count: "",
    status: false,
    is_drd: false,
  },
  {
    label: "Thành viên",
    icon: "pi pi-users",
    code: "1",
    count: "",
    status: false,
    is_drd: true,
  },
  {
    label: "Phòng ban",
    icon: "pi pi-sitemap",
    code: "2",
    count: "",
    status: false,
    is_drd: false,
  },
  {
    label: "Công ty",
    icon: "pi pi-building",
    code: "3",
    count: "",
    status: false,
    is_drd: false,
  },
  {
    label: "Báo cáo công việc",
    icon: "pi pi-check-circle",
    code: "4",
    count: "",
    status: false,
    is_drd: false,
  },
  {
    label: "Đánh giá công việc",
    icon: "pi pi-thumbs-up",
    code: "5",
    count: "",
    status: false,
    badgeCount: null,
    is_drd: false,
  },
  {
    label: "Gia hạn công việc",
    icon: "pi pi-clock",
    code: "6",
    count: "",
    status: false,
    badgeCount: null,
    is_drd: false,
  },
  // {
  //   label: "Filter",
  //   icon: "pi pi-clock",
  //   code: "7",
  //   count: "",
  //   status: false,
  //   badgeCount: null,
  // },
]);
const ChangeView = (value) => {
  ListButtonLabel.value.forEach((x) => {
    if (x.code == value) {
      x.status = true;
    } else {
      x.status = false;
    }
  });
};
const selectedViewType = ref();
const item2s2 = ref([
  {
    label: "Xem theo thành viên",
    icon: "pi pi-users",
    command: () => {
      selectedViewType.value = 1;
      ChangeView(1);
    },
  },
  {
    label: "Xem theo dự án",
    icon: "pi pi-briefcase",
    command: () => {
      selectedViewType.value = 2;
      ChangeView(1);
    },
  },
]);
onMounted(() => {
  ChangeView(0);
  return;
});
</script>
<template>
  <div class="main-layout p-2">
    <div class="col-12 format-center div-button">
      <span
        class="format-center"
        v-for="item in ListButtonLabel"
        :key="item.label"
      >
        <Button
          v-if="item.is_drd == false"
          :label="item.label"
          :icon="item.icon"
          class="font-bold m-px"
          :class="item.status == true ? 'active' : ''"
          @click="ChangeView(item.code)"
          :badge="item.badgeCount"
          badgeClass="p-badge-danger"
        >
        </Button>
        <SplitButton
          v-if="item.is_drd == true"
          :label="item.label"
          :icon="item.icon"
          @click="ChangeView(item.code)"
          :model="item2s2"
          :class="{ 'p-splitbutton-item': item.status == true }"
        ></SplitButton>
      </span>
    </div>
    <div class="div-info bg-white">
      <MyTaskInfo v-if="ListButtonLabel[0].status == true"></MyTaskInfo>
      <MembersTask
        v-if="ListButtonLabel[1].status == true"
        :typeView="selectedViewType"
      ></MembersTask>
      <DepartmentTask v-if="ListButtonLabel[2].status == true"></DepartmentTask>
      <OrganizationTasks
        v-if="ListButtonLabel[3].status == true"
      ></OrganizationTasks>
      <TaskReport v-if="ListButtonLabel[4].status == true"></TaskReport>
      <TaskReview v-if="ListButtonLabel[5].status == true"></TaskReview>
      <TaskExtendDashboard
        v-if="ListButtonLabel[6].status == true"
      ></TaskExtendDashboard>
      <!-- <FilterTask v-if="ListButtonLabel[7].status == true"></FilterTask> -->
    </div>
  </div>
</template>

<style lang="scss" scoped>
.m-px {
  margin-left: 2px;
  margin-right: 2px;
}
.format-center {
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
  text-align: left;
}
.div-button {
  height: 45px;
}
/*.div-info {
  height: calc(100vh - 110px);
}*/
.active {
  background-color: #f18636;
  border: 1px solid #f18636;
}

::v-deep(.p-splitbutton-item) {
  .p-button:enabled {
    background: #f18636;
    color: #ffffff;
    border-color: #f18636;
    font-weight: 700;
  }
}
::v-deep(.p-splitbutton) {
  .p-button {
    font-weight: 700;
  }
}
</style>
