<script setup>
import { ref, inject, onMounted, onBeforeUnmount, watch } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../util/function.js";
import moment from "moment";

const cryoptojs = inject("cryptojs");
const opition = ref({});
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const emitter = inject("emitter");

const countTasks = ref(0);
const countMembers = ref(0);
const countComments = ref(0);
const countAllFile = ref(0);

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);

const listStatusProjectMain = ref([
    {value: 0, text: "Chưa bắt đầu" },
    {value: 1, text: "Đang thực hiện" },
    {value: 2, text: "Đã hoàn thành" },
    {value: 3, text: "Tạm dừng" },
    {value: 4, text: "Đóng" },
]);

const props = defineProps({
  isShow: Boolean,
  id: String,
  turn: Intl,
});

const ProjectMainDetail = ref({});
const ProjectMainMembers = ref();
const ProjectMainParticipants = ref();
const Thanhviens = ref([]);
const ThanhvienShows = ref([]);
const chartData = ref();

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

const optionsChart = {
  responsive: true,
  plugins: {
    title: {
      display: true,
      position: "bottom",
      text: "Thống kê công việc thuộc dự án",
    },
    legend: {
      position: "bottom",
    },
  },
};

const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
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
            proc: "project_main_get_detail",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "project_id", va: props.id },
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
      ProjectMainDetail.value = data[0][0];
      ProjectMainDetail.value.status_name = listStatusProjectMain.value.filter(x=>x.value == data[0][0].status)[0].text;
      Thanhviens.value = data[1];
      if(data[1].length > 5) {
        ThanhvienShows.value = data[1].slice(0, 5);
      } else {
        ThanhvienShows.value = [...data[1]];
      }
      ProjectMainMembers.value = data[1].filter(x=>x.is_type == 0);
      ProjectMainParticipants.value = data[1].filter(x=>x.is_type == 1);
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
}

onMounted(() => {
  loadData(true);
  return {};
});
</script>
<template>
    <div class="overflow-hidden h-full w-full col-md-12 p-0 m-0 flex" style="height: 100%;">
        <div class="col-12 p-0 m-0" style="height: 100%;">
            <div class="row w-full px-0 mx-0" style="height: 100%;">
                <!-- <div class="col-1 p-0 m-0 flex">
                    <Button
                        icon="pi pi-times"
                        class="p-button-rounded p-button-text"
                        v-tooltip="{ value: 'Đóng' }"
                        @click="closeSildeBar()"
                    />
                    <Button
                        icon="pi pi-window-maximize"
                        class="p-button-rounded p-button-text"
                        v-tooltip="{ value: 'Phóng to' }"
                        @click="MaxMin('full')"
                    />
                    <Button
                        icon="pi pi-window-minimize"
                        class="p-button-rounded p-button-text"
                        v-tooltip="{ value: 'Thu nhỏ' }"
                        @click="MaxMin('right')"
                    />
                </div> -->
                <div class="col-12 p-0 m-0" style="height: 100%;">
                    <div style="position: absolute;top: 2px; right: 20px; z-index: 2;">
                        <AvatarGroup>
                            <div
                            v-for="(value, index) in ThanhvienShows"
                            :key="index"
                            >
                            <div>
                                <Avatar
                                v-tooltip.bottom="{
                                    value:
                                    value.fullName +
                                    '<br/>' +
                                    (value.tenChucVu || '') +
                                    '<br/>' +
                                    (value.tenToChuc || ''),
                                    escape: true,
                                }"
                                v-bind:label="
                                    value.avatar ? '' : (value.last_name ?? '').substring(0, 1)
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
                            v-if="
                                Thanhviens.length - ThanhvienShows.length >
                                0
                            "
                            :label="
                                '+' +
                                (Thanhviens.length -
                                ThanhvienShows.length) +
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
                    </div>
                    <TabView ref="tabview1" style="height: 100%;">
                        <TabPanel header="Thông tin chung">
                            <div class="tab-project-content h-full w-full col-md-12 p-0 m-0 flex">
                                <div class="col-6 p-0 m-0 tab-project-content-left">
                                    <div class="row">
                                        <div class="col-12" style="border-bottom: 1px solid #aaa; font-weight: 600;padding: 10px;"><i class="pi pi-chart-pie"></i> Thống kê chung</div>
                                        <div class="col-12">
                                            <div class="row flex">
                                                <div class="col-3 format-center pt-1">
                                                    <Button class="p-button-success py-0 w-full"
                                                    style="
                                                        font-weight: 500;
                                                        background-color: #59d05d !important;
                                                        font-size: 1rem !important;
                                                        box-shadow: 0px 1px 15px 1px rgb(69 65 78 / 8%);
                                                        height: 30px;
                                                        display: flex;
                                                        justify-content: center;
                                                    "
                                                    @click="GotoView('Checklist')"
                                                    >
                                                    <i class="pi pi-list px-0"></i>
                                                    <span class="pl-2">{{ countTasks }}</span>
                                                    <span class="pl-2"> Công việc </span>
                                                    </Button>
                                                </div>
                                                <div class="col-3 format-center pt-1">
                                                    <Button
                                                    class="p-button-success py-0 w-full"
                                                    style="
                                                        font-weight: 500;
                                                        background-color: #fbad4c !important;
                                                        font-size: 1rem !important;
                                                        box-shadow: 0px 1px 15px 1px rgb(69 65 78 / 8%);
                                                        height: 30px;
                                                        display: flex;
                                                        justify-content: center;
                                                    "
                                                    @click="GotoView('members')"
                                                    >
                                                    <i class="pi pi-user px-0"></i>
                                                    <span class="pl-2">{{ countMembers }}</span>
                                                    <span class="pl-2"> Tham Gia </span>
                                                    </Button>
                                                </div>
                                                <div class="col-3 format-center pt-1">
                                                    <Button
                                                    class="p-button-success py-0 w-full"
                                                    style="
                                                        font-weight: 500;
                                                        background-color: #ff646d !important;
                                                        font-size: 1rem !important;
                                                        box-shadow: 0px 1px 15px 1px rgb(69 65 78 / 8%);
                                                        height: 30px;
                                                        display: flex;
                                                        justify-content: center;
                                                    "
                                                    @click="GotoView('comments')"
                                                    >
                                                    <i class="pi pi-comments px-0"></i>
                                                    <span class="pl-2">{{ countComments }}</span>
                                                    <span class="pl-2"> Bình luận </span>
                                                    </Button>
                                                </div>
                                                <div class="col-3 format-center pt-1">
                                                    <Button
                                                    class="p-button-success py-0 w-full"
                                                    style="
                                                        font-weight: 500;
                                                        background-color: #1d62f0 !important;
                                                        font-size: 1rem !important;
                                                        box-shadow: 0px 1px 15px 1px rgb(69 65 78 / 8%);
                                                        height: 30px;
                                                        display: flex;
                                                        justify-content: center;
                                                    "
                                                    @click="GotoView('file')"
                                                    >
                                                    <i class="pi pi-file px-0"></i>
                                                    <span class="pl-2">{{ countAllFile }}</span>
                                                    <span class="pl-2"> Tài liệu </span>
                                                    </Button>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-12">
                                            <Chart
                                            type="pie"
                                            :data="chartData"
                                            :options="optionsChart"
                                            />
                                        </div>
                                    </div>
                                </div>
                                <div class="col-6 p-0 m-0 tab-project-content-right">
                                    <div class="row" style="padding: 15px; font-size: 14px;">
                                        <div class="col-12">Tên dự án: <span style="font-weight: 600;">{{ ProjectMainDetail.project_name }}</span></div>
                                        <div class="col-12">Tạo bởi: <span style="font-weight: 600;color:#2196F3;">{{ ProjectMainDetail.full_name }} - </span><span>{{ moment(new Date(ProjectMainDetail.created_date)).format("HH:mm DD/MM") }}</span></div>
                                        <div class="col-12" style="display: flex; flex-direction: column;">
                                            <span>Thành viên quản trị dự án</span>
                                            <div style="margin: 10px 0px 0px 10px;">
                                                <AvatarGroup>
                                                    <div
                                                        v-for="(value, index) in ProjectMainMembers"
                                                        :key="index"
                                                        >
                                                        <div>
                                                            <Avatar
                                                            v-tooltip.bottom="{
                                                                value:
                                                                value.full_name +
                                                                '<br/>' +
                                                                (value.tenChucVu || '') +
                                                                '<br/>' +
                                                                (value.tenToChuc || ''),
                                                                escape: true,
                                                            }"
                                                            v-bind:label="
                                                                value.avatar ? '' : (value.last_name ?? '').substring(0, 1)
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
                                                </AvatarGroup>
                                            </div>
                                        </div>
                                        <div class="col-12" style="display: flex; flex-direction: column;">
                                            <span>Thành viên tham gia dự án</span>
                                            <div style="margin: 10px 0px 0px 10px;">
                                                <AvatarGroup>
                                                    <div
                                                        v-for="(value, index) in ProjectMainParticipants"
                                                        :key="index"
                                                        >
                                                        <div>
                                                            <Avatar
                                                            v-tooltip.bottom="{
                                                                value:
                                                                value.full_name +
                                                                '<br/>' +
                                                                (value.tenChucVu || '') +
                                                                '<br/>' +
                                                                (value.tenToChuc || ''),
                                                                escape: true,
                                                            }"
                                                            v-bind:label="
                                                                value.avatar ? '' : (value.last_name ?? '').substring(0, 1)
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
                                                </AvatarGroup>
                                            </div>
                                        </div>
                                        <div class="col-12">Nhóm dự án: <span style="font-weight: 600;">{{ ProjectMainDetail.group_name }}</span></div>
                                        <div class="col-12">
                                            <div class="row flex">
                                                <div class="col-6 flex" style="flex-direction: column;padding: 0px;font-weight: 600;line-height: 25px;">
                                                    <span>Ngày bắt đầu</span>
                                                    <span v-if="ProjectMainDetail.start_date">{{ moment(new Date(ProjectMainDetail.start_date)).format("DD/MM/YYYY") }}</span>
                                                </div>
                                                <div class="col-6 flex" style="flex-direction: column;padding: 0px;font-weight: 600;line-height: 25px;">
                                                    <span>Ngày kết thúc</span>
                                                    <span v-if="ProjectMainDetail.end_date">{{ moment(new Date(ProjectMainDetail.end_date)).format("DD/MM/YYYY") }}</span>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-12">
                                            <div class="row flex">
                                                <div class="col-12 flex" style="flex-direction: column;padding: 0px;line-height: 25px;">
                                                    <span>Trạng thái dự án</span>
                                                    <span style="font-weight: 600;">{{ ProjectMainDetail.status_name }}</span>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </TabPanel>
                        <TabPanel header="Dự án con">
                            <p>Dự án con</p>
                        </TabPanel>
                        <TabPanel header="Giai đoạn">
                            <p>Giai đoạn</p>
                        </TabPanel>
                        <TabPanel header="Công việc">
                            <p>Công việc</p>
                        </TabPanel>
                        <TabPanel header="Tài liệu">
                            <p>Tài liệu</p>
                        </TabPanel>
                        <TabPanel header="Thảo luận">
                            <p>Thảo luận</p>
                        </TabPanel>
                        <TabPanel header="Lịch">
                            <p>Lịch</p>
                        </TabPanel>
                        <TabPanel header="Báo cáo">
                            <p>Báo cáo</p>
                        </TabPanel>
                        <TabPanel header="Hoạt động">
                            <p>Hoatrj động</p>
                        </TabPanel>
                    </TabView>
                </div>
            </div>
        </div>
    </div>
</template>
<style scoped>
.tab-project-content{
    height: calc(100vh - 50px) !important;
    background-color: #f3f3f3;
}
.tab-project-content-left{
    background-color: #fff;
    margin: 5px 5px 0px 0px !important;
    height: 100%;
}
.tab-project-content-right{
    background-color: #fff;
    margin: 5px 0px 5px 5px !important;
    height: 100%;
}
.pl-2{
    padding-left: 0.5rem !important;
}
</style>
<style>
.p-sidebar  .p-sidebar-content{
    height: 100%;
    background-color: #f3f3f3;
}
.p-tabview .p-tabview-panels{
    height: 96%;
    padding: 0px !important;
    /* background-color: #f3f3f3; */
}
</style>