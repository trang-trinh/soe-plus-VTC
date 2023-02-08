<script setup>
import { ref, onMounted } from "vue";
import MyTaskInfo from "./dashboardComponent/MyTaskInfo.vue";
import MembersTask from "./dashboardComponent/MembersTask.vue";
import OrganizationTasks from "./dashboardComponent/OrganizationTask.vue";
import TaskReport from "./dashboardComponent/TaskReport.vue";
const ListButtonLabel = ref([
  { label: "Cá nhân", icon: "pi pi-user", code: "0", count: "", status: false },
  {
    label: "Thành viên",
    icon: "pi pi-users",
    code: "1",
    count: "",
    status: false,
  },
  {
    label: "Phòng ban",
    icon: "pi pi-sitemap",
    code: "2",
    count: "",
    status: false,
  },
  {
    label: "Công ty",
    icon: "pi pi-building",
    code: "3",
    count: "",
    status: false,
  },
  {
    label: "Báo cáo công việc",
    icon: "pi pi-check-circle",
    code: "4",
    count: "",
    status: false,
  },
  {
    label: "Đánh giá công việc",
    icon: "pi pi-thumbs-up",
    code: "5",
    count: "",
    status: false,
  },
  {
    label: "Gia hạn công việc",
    icon: "pi pi-clock",
    code: "6",
    count: "",
    status: false,
  },
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
onMounted(() => {
  ChangeView(4);
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
          :label="item.label"
          :icon="item.icon"
          class="font-bold m-1px"
          @click="ChangeView(item.code)"
        ></Button>
      </span>
    </div>
    <div class="div-info bg-white">
      <MyTaskInfo v-if="ListButtonLabel[0].status == true"></MyTaskInfo>
      <MembersTask v-if="ListButtonLabel[1].status == true"></MembersTask>
      <OrganizationTasks
        v-if="ListButtonLabel[3].status == true"
      ></OrganizationTasks>
      <TaskReport v-if="ListButtonLabel[4].status == true"></TaskReport>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.m-1px {
  margin: 1px;
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
</style>
