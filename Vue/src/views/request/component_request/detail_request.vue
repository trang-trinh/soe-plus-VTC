<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../util/function";
import moment from "moment";
import treeuser from "../../../components/user/treeuser.vue";

const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const basedomainURL = baseURL;
const baseUrlCheck = "http://localhost:8080/";
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

const props = defineProps({
    isShow: Boolean,
    id: String,
    key: Number,
    listStatusRequests: Array,
});
const loadData = (rf) => {
    if (rf) {
        loadDetailRequest();
    }
};
const detail_request = ref();
const TimeToDo = ref();
const loadDetailRequest = () => {
    axios
    .post(
        baseURL + "/api/request/getData",
        {
            str: encr(
                JSON.stringify({
                    proc: "request_detail_get",
                    par: [
                        { par: "request_id", va: props.id },
                        { par: "user_id", va: store.state.user.user_id },
                    ],
                }),
                SecretKey,
                cryoptojs,
            ).toString(),
        },
        config,
    )
    .then((response) => {
        if (response.data != null && response.data.data != null) {
            var data = JSON.parse(response.data.data);
            if (data.length > 0) {
                detail_request.value = data[0][0];                
                detail_request.value.objStatus = props.listStatusRequests.find(x => x.id == detail_request.value.status);
                detail_request.value.times_processing_max = detail_request.value.times_processing_max || 0;
                detail_request.value.IsViewXL = false;
                if (detail_request.value.is_security) {
                    is_viewSecurityRequest.value = true;
                }
                else {                    
                    // temp fake
                    is_viewSecurityRequest.value = true; // false;
                }
                
                let today = new Date();
                var d2 = datalists.value.completed_date ? new Date(datalists.value.completed_date) : new Date();
                var diff = d2.getTime() - today.getTime();
                var daydiff = diff / (1000 * 60 * 60 * 24);
                var stdate = new Date(detail_request.value.start_date);
                if (stdate == null || stdate > today) {
                    TimeToDo.value = "Chưa bắt đầu";
                }
                else {
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
                }
            }
            else {
                detail_request.value = null;
            }
        }
    })
    .catch((error) => {
        if (error && error.status === 401) {
            swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
            });
            store.commit("gologout");
        }
    });
};

// 
const openRecallRequest = (dataRequest, f) => {

};

const menuButMores = ref();
const itemButMores = ref([
    {
        label: "Thiết lập quy trình xử lý",
        icon: "pi pi-cog",
        class: "",
        command: (event) => {
            openFlow(detail_request);
        },
    },
    {
        label: "Chuyển bộ phận đề xuất xử lý",
        icon: "pi pi-send",
        class: "status-process-0",
        command: (event) => {
            openXLDX(detail_request, 1, 'Chuyển xử lý');
        },
    },
    {
        label: detail_request.created_by == store.getters.user.user_id ? 'Đánh giá đề xuất' : 'Gửi người lập đánh giá',
        icon: "pi pi-user",
        class: "status-process-1",
        command: (event) => {
            openXLDX(detail_request, 2, 'Gửi người lập đánh giá');
        },
    },
    {
        label: "Dừng xử lý",
        icon: "pi pi-stop-circle",
        class: "status-process-4",
        command: (event) => {
            openXLDX(detail_request, 4, 'Dừng xử lý');
        },
    },
    {
        label: "Xử lý tiếp",
        icon: "pi pi-play",
        class: "status-process-4",
        command: (event) => {
            openXLDX(detail_request, 1, 'Xử lý tiếp');
        },
    },
]);
const getFuncRequest = () => {
    if (detail_request.status_processing == 0) {
        return itemButMores.filter(x => x.class == "" || x.class.includes("status-process-0"));
    }
    else if (detail_request.status_processing == 1) {
        return itemButMores.filter(x => x.class == "" || x.class.includes("status-process-1"));
    }
    else if (detail_request.status_processing == 4) {
        return itemButMores.filter(x => x.class == "" || x.class.includes("status-process-4"));
    }
    if (detail_request.status_processing != 4) {
        return itemButMores.filter(x => !x.class.includes("status-process-4"));
    }
    return itemButMores;
};
const toggleMores = (event, item) => {
    menuButMores.value.toggle(event);
};

const RQJobs = ref([]);

//Bình luận
const panelEmoij1 = ref();
let filecoments = [];
const listFileComment = ref([]);
const comment = ref("");
const comment_zone_main = ref();
let line1 = "";
let line = "";
const showEmoji = (event, check) => {
    if (check == 1) {
        panelEmoij1.value.toggle(event);
    }
};
const handleEmojiClick = (event) => {  
    comment.value = comment.value.replace("<p>", "").replace("</p>", "");
    line1 = line ? line : comment.value.length;
    let str1 = comment.value.slice(0, line1);
    let str2 = comment.value.slice(line1);
    if (comment.value) {
      comment.value = line1 > 0 ? str1 + event.unicode + str2 : comment.value + event.unicode;
    }
    else {
        comment.value = comment.value + event.unicode;
    }
    comment.value = comment.value.replace("<p>", "").replace("</p>", "");
    comment_zone_main.value.setHTML("<p>" + comment.value + "</p>");
    line1 += 1;  
};

const hideall = () => {
    emitter.emit("SideBarRequest", false);
};
const PositionSideBar = ref("right");
const MaxMin = (m) => {
    PositionSideBar.value = m;
    emitter.emit("psbRequest", m);
};
const closeSildeBar = () => {
    emitter.emit("SideBarRequest", false);
};
const is_viewSecurityRequest = ref(true);
onMounted(() => {
    if (props.id != null) {
        loadData(true);
    }
    else {
        hideall();
    }
    return {};
});
</script>
<template>
    <div class="overflow-hidden h-full w-full col-md-12 p-0 m-0 flex"
        v-if="is_viewSecurityRequest == true"
    >
        <div class="col-8 md:col-8 p-0 pl-2 pr-3 m-0" 
            style="border-right: 5px solid #ededed;" 
            v-if="detail_request != null"
        >
            <div class="row col-12 flex justify-content-center px-0 mx-0 format-center">
                <div class="col-1 p-0 m-0 flex">
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
                        v-if="PositionSideBar == 'right'"
                    />
                    <Button
                        icon="pi pi-window-minimize"
                        class="p-button-rounded p-button-text"
                        v-tooltip="{ value: 'Thu nhỏ' }"
                        @click="MaxMin('right')"
                        v-if="PositionSideBar == 'full'"
                    />
                </div>
                <div class="col-11 p-0 pl-3 m-0 flex" style="justify-content: space-between;">
                    <div class="flex" style="align-items: center;">
                        <i class="pi pi-check-square pr-2"></i>
                        <span class="font-bold" style="font-size: 1.25rem;">{{ detail_request.request_name }}</span>
                        <span class="card-nhom text-left"
                            style="padding:0.25rem 0.5rem;margin-left:0.5rem !important;background-color:#0078d4;color:#ffffff;" 
                        >
                            {{ detail_request.request_code || '' }}
                        </span>
                    </div>
                    <div class="flex" style="align-items: center;">
                        <Button
                            icon="pi pi-print"
                            class="p-button-rounded p-button-text"
                            v-tooltip="{ value: 'In' }"
                            @click="printRequest()"
                            v-if="false"
                        />
                        <Button
                            icon="pi pi-pencil"
                            class="p-button-rounded p-button-text"
                            v-tooltip="{ value: 'Chỉnh sửa' }"
                            @click="editRequest()"
                            v-if="false"
                        />
                        <Button
                            icon="pi pi-trash"
                            class="p-button-rounded p-button-text"
                            style="color:red;"
                            v-tooltip="{ value: 'Xóa' }"
                            @click="delRequest()"
                            v-if="false"
                        />
                        <span class="card-nhom text-left"
                            style="padding:0.25rem 0.5rem;margin-left:0.5rem !important;background-color:#ff8b4e;color:#ffffff;"
                            v-if="detail_request.is_change_process"
                        >
                            Quy trình động
                        </span>
                    </div>
                </div>
            </div>
            <div class="row col-12 p-0 pl-2">
                <div class="col-12 flex">
                    <span class="pr-2" style="font-style: italic;">Loại đề xuất:</span>
                    <span class="font-bold" style="font-style: italic;">{{ detail_request.request_form_name || 'Đề xuất trực tiếp' }}</span>
                </div>
                <div class="col-12 flex">
                    <span class="pr-2">Tạo bởi:</span>
                    <span class="font-bold" style="color:#2196f3;">{{ (detail_request.full_name || '') }}</span>
                    <span class="pl-2">viewRequest
                        {{ " - " + (detail_request.created_date ? moment(new Date(detail_request.created_date)).format('HH:mm DD/MM/yyyy') : '') }}
                    </span>
                </div>
                <div class="col-12 flex">
                    <div class="requestbutton" 
                        style="max-width:30%" 
                        v-if="detail_request.is_create && !detail_request.is_func && detail_request.status == 1 && detail_request.Tiendo <= 0"
                    >
                        <Button class="p-button-warning" 
                            style="background-color:orange"
                            @click="openRecallRequest(detail_request, true)"
                            label="Thu hồi">
                        </Button>
                    </div>
                    <div class="requestbutton" 
                        style="max-width:100%;height:36px" 
                        v-if="detail_request.status == 2 && detail_request.is_evaluate == true">
                        <div style="flex:1">
                            <div style="display:flex">
                                <div style="margin-right:10px" class="btn-group requestbutton" 
                                    v-if="detail_request.status==2 && detail_request.status_processing>0">
                                    <Button label="Đang được xử lý" 
                                        @click="viewXLDX(detail_request)" 
                                        style="margin-right:0" 
                                        v-if="detail_request.status_processing == 1">
                                    </Button>
                                    <Button label="Đang đợi xử lý" 
                                        @click="viewXLDX(detail_request)" 
                                        style="margin-right:0" 
                                        v-if="detail_request.status_processing == 0" 
                                        class="p-button-secondary">
                                    </Button>
                                    <Button label="Đã hoàn thành xử lý" 
                                        @click="viewXLDX(detail_request)" 
                                        style="margin-right:0;border-top-right-radius:5px;border-bottom-right-radius:5px" 
                                        v-if="detail_request.status_processing == 3" 
                                        class="p-button-success">
                                    </Button>
                                    <Button label="Đánh giá đề xuất" 
                                        v-if="detail_request.is_create && detail_request.status_processing == 2" 
                                        @click="openXLDX(detail_request,2,'Đánh giá đề xuất')" 
                                        style="background-color:#00bcd4;border-top-right-radius:5px;border-bottom-right-radius:5px">
                                    </Button>
                                    <Button label="Chờ người lập đánh giá" 
                                        v-if="!detail_request.is_create && detail_request.status_processing == 2" 
                                        @click="viewXLDX(detail_request)" 
                                        style="margin-right:0;background-color:orange">
                                    </Button>
                                    <Button label="Dừng xử lý" 
                                        v-if="(detail_request.IsXL||detail_request.IsSignXL) && detail_request.status_processing == 4" 
                                        @click="viewXLDX(detail_request)" 
                                        style="margin-right:0;" 
                                        class="p-button-danger">
                                    </Button>
                                    <Button
                                        v-if="!detail_request.is_create && (detail_request.IsXL || detail_request.IsSignXL) && 
                                        ((detail_request.status_processing != 3 && detail_request.status_processing != 2) || (detail_request.status_processing == 2 && detail_request.created_by === store.getters.user.user_id))"
                                        icon="pi pi-ellipsis-h"
                                        :class="'p-button-' + (detail_request.status_processing == 0 ? 'secondary' 
                                                                : detail_request.status_processing == 1 ? 'primary' 
                                                                : detail_request.status_processing == 2 ? 'success'
                                                                : 'danger')"
                                        @click="toggleMores($event, detail_request)"
                                        aria-haspopup="true"
                                        aria-controls="overlay_More"
                                        v-tooltip.top="'Tác vụ'"
                                    />
                                    <Menu 
                                        class="menu-request"
                                        id="overlay_More"
                                        ref="menuButMores"
                                        :model="getFuncRequest()"
                                        :popup="true"
                                    />                                
                                </div>
                            </div>
                        </div>
                        <div class="wizard small ng-cloak" 
                            v-if="detail_request.status == 2 && RQJobs.length > 1">
                            <template v-for="(state, idxJob) in RQJobs">
                                <a v-tooltip.top="{ value: state.Job_Name }" 
                                    @click="setCurrent(state)" 
                                    :class="(state.isCurrent ? 'current': '') + ' ' + ('job' + state.status)">
                                    <span>{{ idxJob + 1 }}</span>
                                </a>
                            </template>
                        </div>
                        <a @click="viewXLDX(detail_request)" 
                            style="color:#2196f3 !important;font-size:12px;margin-left:20px" 
                            v-if="detail_request.Stask > 0"
                        >
                            <font-awesome-icon icon="fa-solid fa-list-check" /> ({{detail_request.StaskFinish + "/" + detail_request.Stask}})
                            <div v-tooltip.top="{ value: 'Tiến độ công việc: ' + detail_request.StaskTiendo + '%' }" class="radialProgressBar progress-{{detail_request.bgtiendo}}" style="width:24px;height:24px;display:inline-flex">
                                <div class="overlay" 
                                    style="font-size:6px;width:16px;height:16px;color:#000">
                                    {{detail_request.StaskTiendo}}%
                                </div>
                            </div>
                        </a>
                    </div>
                    <div class="requestbutton" 
                        v-if="!detail_request.is_create && detail_request.is_func && detail_request.status==1"
                    >
                        <Button label="Chấp thuận" 
                            @click="OpenSendRequest(detail_request,'Chấp thuận',1)" 
                            class="p-button-success">
                        </Button>
                        <Button label="Từ chối"
                            @click="OpenSendRequest(detail_request, 'Từ chối', -1)" 
                            class="p-button-danger">
                        </Button>
                        <Button v-if="!detail_request.IsLast" 
                            label="Chuyển tiếp"
                            @click="OpenSendRequest(detail_request,'Chuyển tiếp',2)">
                        </Button>
                        <Button v-if="detail_request.IsForward" 
                            label="Đồng ý & chuyển tiếp"
                            @click="OpenSendRequest(detail_request,'Đồng ý và chuyển tiếp',3)" 
                            class="p-button-success">
                        </Button>
                    </div>
                    <div class="requestbutton" 
                        v-if="detail_request.is_create && detail_request.is_func"
                    >
                        <Button v-if="!detail_request.IsHoanthanh" 
                            label="Gửi" 
                            @click="OpenSendRequest(detail_request,'Gửi')">
                        </Button>
                        <Button v-if="detail_request.status != -1 && detail_request.status != 0" 
                            label="Hủy"
                            @click="StopRequest(detail_request)" 
                            class="p-button-warning" 
                            style="background-color:orange">
                        </Button>
                        <Button v-if="detail_request.status==-1" 
                            label="Bỏ hủy"
                            @click="BackRequest(detail_request)" 
                            class="p-button-warning" 
                            style="background-color:#6fbf73">
                        </Button>
                        <Button label="Xóa"
                            @click="DelRequest(detail_request, 1)" 
                            class="p-button-danger">
                        </Button>
                    </div>
                </div>
                <div class="col-12 flex">
                    <div class="job-title" v-if="detail_request.IsViewXL">
                        <label>
                            <font-awesome-icon icon="fa-solid fa-list-check" />
                            {{RQJobs.length}} nhiệm vụ đang thực hiện
                        </label> 
                        <a v-if="(detail_request.IsXL || detail_request.IsSignXL) && detail_request.status_processing != 3" 
                            @click="openFlow(detail_request)">
                            <i class="pi pi-plus-circle pr-2"></i> 
                            Thêm nhiệm vụ
                        </a>
                    </div>
                </div>
                <div class="col-12 flex">
                    <!-- coding... -->
                    
                </div>
            </div>
        </div>
        <div class="col-4 md:col-4 p-0 px-3 m-0" v-if="detail_request != null">
            <div class="row col-12 p-0 flex">
                <!-- <div v-if="detail_request.times_processing_max > 0 && detail_request.status == 1" -->
                <div
                    class="col-12 p-button-warning format-center font-bold py-3 mt-3 mb-2 text-xl border-round flex"
                    style="background-color: #fffbd8; color: #857a1f"
                >
                    <i class="pi pi-clock pr-2" v-if="TimeToDo != 'Chưa bắt đầu'" />
                    <span class="flex" v-html="TimeToDo"></span>
                </div>
            </div>
            <div class="row col-12 p-0 flex">
                <span class="w-full rq"
                    :class="detail_request.objStatus.class || ''"
                    style="font-size: 14px;font-weight: bold;padding: 10px;border-radius:0;text-align: center;"
                >
                    {{ detail_request.objStatus.text || "" }}
                </span>
                            
            </div>
        </div>
    </div>
    <div class="overflow-hidden w-full"
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
            src="../../../assets/background/nodata.png"
            height="250"
        />
        <h2 class="m-1">Đề xuất bảo mật, đã bị xóa hoặc không tồn tại.</h2>
    </div>
    <!-- OverlayPanel Emoji -->
    <OverlayPanel
        class="p-0"
        ref="panelEmoij1"
        append-to="body"
        :show-close-icon="false"
        id="overlay_panelEmoij1"
    >
        <VuemojiPicker @emojiClick="handleEmojiClick" />
    </OverlayPanel>
</template>
<style scoped>
    @import url(../style_request.css);
</style>
<style lang="scss" scoped>

</style>