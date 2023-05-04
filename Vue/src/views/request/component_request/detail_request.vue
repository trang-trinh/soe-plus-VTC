<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../util/function";
import moment from "moment";
import treeuser from "../../../components/user/treeuser.vue";

const toast = useToast();
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
const renderColorProgress = (value) => {
    if (value >= 75) {
        return "classOver75";
    }
    else if (value >= 50) {
        return "classOver50";
    }
    else if (value >= 30) {
        return "classOver30";
    }
    else if (value > 0) {
        return "classOver0";
    }
    return "";
};
const listStatusRequests = ref([
    { id: 0,  text: "Mới lập",    class: "rqlap" },
    { id: 1,  text: "Chờ duyệt",  class: "rqchoduyet" },
    { id: 2,  text: "Chấp thuận", class: "rqchapthuan" },
    { id: -2, text: "Từ chối",    class: "rqtuchoi" },
    { id: -1, text: "Hủy",        class: "rqhuy" },
    { id: 3,  text: "Thu hồi",    class: "rqthuhoi" },
    { id: -3, text: "Xóa",        class: "rqxoa" }
]);
const orderDatas = (dataJob, type) => {
    let resultOrder = dataJob.sort((a, b) => { 
        return a[type] - b[type];
    });
    return resultOrder;
};
const limitData = (dataGet, limit) => {
    return dataGet.slice(0, (limit >= 0 ? limit : 0));
};
const filterFileType = (dataFilter, typeFilter, value) => {
    return dataFilter.filter(x => x[typeFilter] == value);
};
const formatByte = ((bytes, precision) => {
	if (isNaN(parseFloat(bytes)) || !isFinite(bytes)) return '-';
	if (typeof precision === 'undefined') precision = 1;
	let units = ['bytes', 'KB', 'MB', 'GB', 'TB', 'PB'];
	if (typeof bytes === 'string' || bytes instanceof String){
		bytes = parseFloat(bytes);
	}
	let	number = Math.floor(Math.log(bytes) / Math.log(1024));
	return (bytes / Math.pow(1024, Math.floor(number))).toFixed(precision) + ' ' + units[number];
});

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
const isClose = ref(false);
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
                detail_request.value.objStatus = props.listStatusRequests.find(x => x.id == detail_request.value.status_processing);
                detail_request.value.times_processing_max = detail_request.value.times_processing_max || 0;
                // temp fake 1
                //detail_request.value.IsViewXL = false; // r.IsViewXL;
                //detail_request.value.Tiendo = 20;
                // end temp fake 1

                if (detail_request.value.is_security) {
                    is_viewSecurityRequest.value = true;
                }
                else {                    
                    // temp fake
                    is_viewSecurityRequest.value = true; // false;
                }
                
                let today = new Date();
                var d2 = detail_request.value.completed_date ? new Date(detail_request.value.completed_date) : new Date();
                var diff = d2.getTime() - today.getTime();
                var daydiff = diff / (1000 * 60 * 60 * 24);
                var stdate = new Date(detail_request.value.start_date);
                if (stdate == null || stdate > today) {
                    TimeToDo.value = "Chưa bắt đầu";
                }
                else {
                    if (0 < daydiff + 1 && daydiff + 1 < 1) {
                        TimeToDo.value =
                        "<div class='flex format-center font-bold' style='background-color: #fffbd8;color: #6DD230'> Đến hạn hoàn thành </div>";
                        return;
                    }
                    let displayTime = Math.abs(Math.floor(daydiff + 1));
                    TimeToDo.value =
                        daydiff + 1 < 0
                        ? "<div class='flex format-center font-bold' style='background-color: #fffbd8;color: red'> Quá hạn " +
                            displayTime +
                            " ngày</div>"
                        : "<div class='flex format-center font-bold' style='background-color: #fffbd8;color: #6DD230'> Còn " +
                            displayTime +
                            " ngày</div>";
                }
            }
            else {
                detail_request.value = null;
            }
            listFiles();
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

const LisFileAttachRQ = ref([]);
const listFiles = () => {
    axios
        .post(
            baseURL + "/api/request/getData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "request_files",
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
        if (response.data != null && response.data.data != null && response.data.err != '1') {
            var data = JSON.parse(response.data.data);
            LisFileAttachRQ.value = data[0];
            // fake data
            // LisFileAttachRQ.value = [
            //     { file_name: 'nabila-miah-happy-birthday-gif10774342.gif', file_path: '/Portals/Gif/nabila-miah-happy-birthday-gif10774342.gif', file_type: 'gif', file_size: 135792, is_image: true, is_type: 0, created_date: new Date() },
            //     { file_name: 'Mẫu Excel Phép năm.xlsx', file_path: '/Portals/Mau Excel/Mẫu Excel Phép năm.xlsx', file_type: 'xlsx', file_size: 895792, is_image: false, is_type: 0, created_date: new Date() },
            // ];
            // end fake
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

const datelines = ref([]);
const listDateline = () => {

};
const Comments = ref([]);
const listComments = () => {
    axios
        .post(
            baseURL + "/api/request/getData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "request_comment_list",
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
        if (response.data != null && response.data.data != null && response.data.err != '1') {
            var data = JSON.parse(response.data.data);
            Comments.value = data[0];
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

const RQJobs = ref([]);
const RelateRequests = ref([]);
const openRelate = (dataRelate, module, type) => {
    
};

// 
const openRecallRequest = (dataRequest, f) => {
    
};
const viewXLDX = function (dex) {
    dex.IsViewXL = !(dex.IsViewXL || false);
    for (var i = 0; i < RQJobs.value.length; i++) {
        RQJobs.value[i].isCurrent = null;
    }
    if (dex.IsViewXL === true) {
        RQJobs.value = [];
        listJob(dex);
    }
};
const setCurrent = function (state) {
    if (state.isCurrent == true) {
        for (var i = 0; i < RQJobs.value.length; i++) {
            RQJobs.value[i].isCurrent = null;
        }
    } else {
        for (var i = 0; i < RQJobs.value.length; i++) {
            RQJobs.value[i].isCurrent = false;
        }
        state.isCurrent = true;
    }
    detail_request.value.IsViewXL = true;
};

const listJob = (dex) => {

};

const downloadFile = (file) => {
    var name = file.file_name || ("file_download" + file.file_type);
    const a = document.createElement("a");
    a.href = basedomainURL + '/Viewer/DownloadFile?url='+ encodeURIComponent(file.file_path) + '&title=' + encodeURIComponent(name);
    a.download = name;
    // a.target = "_blank";
    a.click();
    a.remove();
}

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
const IsReply = ref(false);

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
    <div class="overflow-hidden h-full w-full col-12 p-0 m-0 flex"
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
            <div class="row col-12 p-0 pl-2" style="position: relative;flex-direction: column;height: calc(100vh - 4rem);">
                <div class="col-12 flex">
                    <span class="pr-2" style="font-style: italic;">Loại đề xuất:</span>
                    <span class="font-bold" style="font-style: italic;">{{ detail_request.request_form_name || 'Đề xuất trực tiếp' }}</span>
                </div>
                <div class="col-12 flex">
                    <span class="pr-2">Tạo bởi:</span>
                    <span class="font-bold" style="color:#2196f3;">{{ (detail_request.full_name || '') }}</span>
                    <span class="pl-2">
                        {{ " - " + (detail_request.created_date ? moment(new Date(detail_request.created_date)).format('HH:mm DD/MM/yyyy') : '') }}
                    </span>
                </div>
                <div class="col-12 flex">
                    <div class="requestbutton" 
                        style="max-width:30%" 
                        v-if="detail_request.is_create && !detail_request.is_func && detail_request.status_processing == 1 && detail_request.Tiendo <= 0"
                    >
                        <Button class="p-button-warning" 
                            style="background-color:orange"
                            @click="openRecallRequest(detail_request, true)"
                            label="Thu hồi">
                        </Button>
                    </div>
                    <div class="requestbutton" 
                        style="max-width:100%;height:36px" 
                        v-if="detail_request.status_processing == 2 && detail_request.is_evaluate == true">
                        <div style="flex:1">
                            <div style="display:flex">
                                <div style="margin-right:10px" class="btn-group requestbutton" 
                                    v-if="detail_request.status_processing == 2 && detail_request.status_processing > 0">
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
                            v-if="detail_request.status_processing == 2 && RQJobs.length > 1">
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
                            <font-awesome-icon icon="fa-solid fa-list-check" /> ({{ (detail_request.StaskFinish || 0) + "/" + (detail_request.Stask || 0) }})
                            <div v-tooltip.top="{ value: 'Tiến độ công việc: ' + (detail_request.StaskTiendo || 0) + '%' }" 
                                class="radialProgressBar"
                                :class="detail_request.bgtiendo ? ('progress-' + detail_request.bgtiendo) : ''" 
                                style="width:24px;height:24px;display:inline-flex">
                                <div class="overlay" 
                                    style="font-size:6px;width:16px;height:16px;color:#000">
                                    {{ detail_request.StaskTiendo || 0 }}%
                                </div>
                            </div>
                        </a>
                    </div>
                    <div class="requestbutton" 
                        v-if="!detail_request.is_create && detail_request.is_func && detail_request.status_processing == 1"
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
                        <Button v-if="detail_request.status_processing != -1 && detail_request.status_processing != 0" 
                            label="Hủy"
                            @click="StopRequest(detail_request)" 
                            class="p-button-warning" 
                            style="background-color:orange">
                        </Button>
                        <Button v-if="detail_request.status_processing == -1" 
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
                <div class="col-12 flex" v-if="detail_request.IsViewXL">
                    <div class="job-title">
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
                    <div class="task-content scrollbox_delayed w-full" 
                        style="padding:0.75rem 0;overflow-y: auto;overflow-x:hidden;"
                        :style="detail_request.IsViewXL ? 'height: calc(100vh - 225px)' : 
                                !IsReply ? 'height: calc(100vh - 250px)' : 
                                detail_request.IsComment ? 'height: calc(100vh - 315px)' : 'height: calc(100vh - 145px)'
                        "
                        id="request_message_panel"
                    >
                        <div v-show="!detail_request.IsViewXL" 
                            class="scrollbox-content"
                            style="margin-right:0;"
                        >
                            <form id="frRequest">
                                <div class="row">
                                    <div class="col-3 p-0" v-if="col-detail_request.modified_date">
                                        <div class="t-r">
                                            <div class="flex">
                                                <span class="cv-spicon flex" style="align-items:center;">
                                                    <i class="pi pi-clock"></i>
                                                </span>
                                                <span class="cv-request">Cập nhật</span>
                                            </div>
                                        </div>
                                        <p>
                                            <span class="datetime-request">
                                                {{ detail_request.modified_date ? moment(new Date(detail_request.modified_date)).format("HH:mm DD/MM/yyyy") : (detail_request.created_date ? moment(new Date(detail_request.created_date)).format("HH:mm DD/MM/yyyy") : '') }}
                                            </span>
                                        </p>
                                    </div>
                                    <div class="col-3 p-0">
                                        <div class="t-r">
                                            <div class="flex">
                                                <span class="cv-spicon flex" style="align-items:center;">
                                                    <i class="pi pi-calendar"></i>
                                                </span>
                                                <span class="cv-request">Ngày lập</span>
                                            </div>
                                        </div>
                                        <p>
                                            <span class="datetime-request">
                                                {{ detail_request.created_date ? moment(new Date(detail_request.created_date)).format("HH:mm DD/MM/yyyy") : '' }}
                                            </span>
                                        </p>
                                    </div>
                                    <div class="col-3 p-0" v-if="col-detail_request.deadline_date != null">
                                        <div class="t-r">
                                            <div class="flex">
                                                <span class="cv-spicon flex" style="align-items:center;">
                                                    <i class="pi pi-calendar-times"></i>
                                                </span>
                                                <span class="cv-request">Hạn xử lý</span>
                                            </div>
                                        </div>
                                        <p>
                                            <span class="datetime-request" style="color:#2196f3">
                                                {{ detail_request.deadline_date ? moment(new Date(detail_request.deadline_date)).format("HH:mm DD/MM/yyyy") : '' }}
                                            </span>
                                        </p>
                                    </div>
                                    <div class="col-3 p-0" v-if="detail_request.status_processing == 2">
                                        <div class="t-r">
                                            <div class="flex">
                                                <span class="cv-spicon flex" style="align-items:center;">
                                                    <i class="pi pi-clock"></i>
                                                </span>
                                                <span class="cv-request">Hoàn thành</span>
                                            </div>
                                        </div>
                                        <p>
                                            <span class="datetime-request">
                                                {{ detail_request.completed_date ? moment(new Date(detail_request.completed_date)).format("HH:mm DD/MM/yyyy") : '' }}
                                            </span>
                                        </p>
                                    </div>
                                </div>
                                <div class="row" 
                                    style="flex-direction: column;margin-top:0.5rem" 
                                    v-if="detail_request.content"
                                >
                                    <div class="t-r">
                                        <div class="flex">
                                            <span class="cv-spicon flex" style="align-items:center;">
                                                <i class="pi pi-align-left"></i>
                                            </span>
                                            <span class="cv-request">Nội dung</span>
                                        </div>
                                    </div>
                                    <p style="margin-left: 22px" v-html="detail_request.content"></p>
                                </div>
                                <div class="row" v-if="detail_request.Tiendo > 0">
                                    <div class="col-12 p-0">
                                        <div class="t-r">
                                            <div class="flex">
                                                <span class="cv-spicon flex" style="align-items:center;">
                                                    <font-awesome-icon icon="fa-solid fa-list-check" />
                                                </span>
                                                <span class="cv-request">Tiến độ</span>
                                            </div>
                                        </div>
                                        <div class="flex my-3">                                            
                                            <span class="flex font-bold mr-3" style="font-size:1.2rem;margin-left:22px;">
                                                {{ detail_request.Tiendo }}% 
                                            </span>
                                            <ProgressBar class="progress-bar-custom flex"
                                                :class="renderColorProgress(detail_request.Tiendo)"
                                                v-tooltip.top="{ value: (detail_request.Tiendo + '%') }" 
                                                :value="(detail_request.Tiendo || 0)"
                                                style="flex:1;">
                                            </ProgressBar>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="row mt-2" 
                                    style="flex-direction:column;"
                                    v-if="col-(detail_request.status_processing == 2 || detail_request.status_processing == 3)"
                                >
                                    <div class="t-r">
                                        <div class="flex">
                                            <span class="cv-spicon flex" style="align-items:center;">
                                                <i class="pi pi-star"></i>
                                            </span>
                                            <span class="cv-request">Đánh giá đề xuất</span>
                                        </div>
                                    </div>
                                    <div class="flex p-3" v-if="col-detail_request.avatar_completed_all">
                                        <div class="r-ava">
                                            <Avatar
                                                v-bind:label="
                                                    detail_request.avatar_completed_all
                                                    ? ''
                                                    : (detail_request.last_name_completed_all ?? '').substring(0, 1).toUpperCase()
                                                "
                                                v-bind:image="
                                                    detail_request.avatar_completed_all
                                                    ? basedomainURL + detail_request.avatar_completed_all
                                                    : basedomainURL + '/Portals/Image/nouser1.png'
                                                "
                                                v-tooltip.top="{ value: (detail_request.full_name_completed_all + '<br/>' + detail_request.position_name_completed_all + '<br/>' + detail_request.department_name_completed_all), escape: true }"
                                                style="color: #ffffff; font-size: 1rem !important;"
                                                :style="{ background: bgColor[0], }"
                                                size="large"
                                                shape="circle"
                                                class="border-radius"
                                            />
                                        </div>
                                        <div class="flex ml-3" style="flex-direction: column;">
                                            <div class="r-cname">
                                                <span class="font-bold mr-3">{{ detail_request.full_name_completed_all || '' }}</span>
                                                <span class="request-cdate">
                                                    {{ detail_request.completed_all_date ? moment(new Date(detail_request.completed_all_date)).format("HH:mm DD/MM/yyyy") : '' }}
                                                </span>
                                            </div>
                                            <div class="mt-2" style="word-break: break-word;">
                                                <div style="margin-bottom:0" v-html="detail_request.completed_all_content || ''"></div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="flex p-3" v-if="col-(detail_request.evaluated_score > 0)">
                                        <div class="r-ava">
                                            <Avatar
                                                v-bind:label="
                                                    detail_request.avatar_evaluated
                                                    ? ''
                                                    : (detail_request.last_name_evaluated ?? '').substring(0, 1).toUpperCase()
                                                "
                                                v-bind:image="
                                                    detail_request.avatar_evaluated
                                                    ? basedomainURL + detail_request.avatar_evaluated
                                                    : basedomainURL + '/Portals/Image/nouser1.png'
                                                "
                                                v-tooltip.top="{ value: (detail_request.full_name_evaluated + '<br/>' + detail_request.position_name_evaluated + '<br/>' + detail_request.department_name_evaluated), escape: true }"
                                                style="color: #ffffff; font-size: 1rem !important;"
                                                :style="{ background: bgColor[0], }"
                                                size="large"
                                                shape="circle"
                                                class="border-radius"
                                            />
                                        </div>
                                        <div class="flex ml-3" style="flex-direction: column;">
                                            <div class="r-cname">
                                                <span class="font-bold mr-3">{{ detail_request.full_name_evaluated || '' }}</span>
                                                <span class="request-cdate">
                                                    {{ detail_request.evaluated_date ? moment(new Date(detail_request.evaluated_date)).format("HH:mm DD/MM/yyyy") : '' }}
                                                </span>
                                            </div>
                                            <div class="mt-2" v-if="col-detail_request.status_processing == 3">
                                                <Rating class="star-rating-custom"
                                                    v-model="detail_request.evaluated_score"
                                                    v-tooltip.top="{ value: 'Ngày đánh giá: <br/>' + (detail_request.evaluated_date ? moment(new Date(detail_request.evaluated_date)).format('HH:mm DD/MM/yyyy') : ''), escape: true }"
                                                    :stars="5"
                                                    :cancel="false" 
                                                    :readonly="true"
                                                />
                                            </div>
                                            <div class="mt-2" style="word-break: break-word;">
                                                <div style="margin-bottom:0" v-html="evaluated_content || ''"></div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <!--
                                <div v-if="FormDS && FormDS.length>0">
                                    <script type="text/ng-template" id="FormD.html">
                                        <div v-if="d.IsType==3">
                                            <div class="form-group" style="margin-top:15px" v-if="d.IsLabel">
                                                <div class="form-group formlabel" style="margin-bottom:0"><label>{{d.TenTruong}} </label></div>
                                                <table class="table table-bordered">
                                                    <thead style="background-color:#eee">
                                                        <tr>
                                                            <th width={{th.IsWidth}} align="{{th.TextAlign}}" style="border:1px solid #ccc;{{renderStyle(th)}}" v-for="th in FormDS|filter:{IsParent_ID:d.FormD_ID}:true">{{th.TenTruong}}</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody ng-init="$pindex=$index;ths=(FormDS|filter:{IsParent_ID:d.FormD_ID}:true)">
                                                        <tr v-for="r in Ftables[$pindex] track by $index">
                                                            <td v-for="td in r track by $index" style="border:1px solid #ccc" ng-init="ths[$index].total=renderTotal(ths[$index],td);">
                                                                <div ng-switch="td.KieuTruong" v-if="!td.IsLabel">
                                                                    <div multiswitch-when="varchar|nvarchar|textarea|float|int|email" style="{{renderStyle(td)}}">
                                                                        {{td.IsGiatri}}
                                                                    </div>
                                                                    <div multiswitch-when="checkbox">
                                                                        <div class="checkbox checkbox-circle checkbox-info peers ai-c" style="margin-left:20px">
                                                                            <input onclick="return false;" ng-checked="td.IsGiatri=='true'" type="checkbox" class="peer" />
                                                                            <label></label>
                                                                        </div>
                                                                    </div>
                                                                    <div multiswitch-when="radio">
                                                                        <div class="radio radio-circle radio-info peers ai-c" style="margin-left:20px">
                                                                            <input onclick="return false;" ng-checked="detail_request.Radio==td.FormD_ID" type="radio" class="peer" />
                                                                            <label></label>
                                                                        </div>
                                                                    </div>
                                                                    <div multiswitch-when="date" class="dropdown datetimeinput" style="{{renderStyle(td)}}">
                                                                        <span ng-bind="td.IsGiatri.substring(0, 10)|date:'dd/MM/yyyy'"></span>
                                                                    </div>
                                                                    <div multiswitch-when="datetime" class="dropdown datetimeinput" style="{{renderStyle(td)}}">
                                                                        <span ng-bind="td.IsGiatri|date:'HH:mm dd/MM/yyyy'"></span>
                                                                    </div>
                                                                    <div multiswitch-when="time" class="dropdown datetimeinput" style="{{renderStyle(td)}}">
                                                                        <span ng-bind="td.IsGiatri|date:'HH:mm'"></span>
                                                                    </div>
                                                                </div>
                                                            </td>
                                                        </tr>
                                                        <tr v-if="Ftables[$pindex].length>1" style="font-weight:bold;background-color: aliceblue;color: blue;">
                                                            <td style="border:1px solid #ccc;{{renderStyle(td)}}" v-for="td in ths track by $index">
                                                                <span v-if="td.total>0" ng-bind="td.total|number : 0"></span>
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </div>
                                        </div>
                                        <div v-if="d.IsType!=3" class="formd {{d.IsClass}}">
                                            <div class="form-group formlabel" style="margin-top:15px" v-if="d.IsLabel" ng-bind="d.TenTruong"></div>
                                            <div class="form-group" v-if="!d.IsLabel">
                                                <div v-if="d.KieuTruong!='checkbox' && d.KieuTruong!='radio'">
                                                    <label class="fw-500" ng-bind="d.TenTruong"></label>
                                                </div>
                                                <div ng-switch="d.KieuTruong">
                                                    <div multiswitch-when="varchar|nvarchar|textarea|float|int|email">
                                                        {{d.IsGiatri}}
                                                    </div>
                                                    <div multiswitch-when="checkbox">
                                                        <div class="checkbox checkbox-circle checkbox-info peers ai-c mB-15">
                                                            <input onclick="return false;" ng-checked="d.IsGiatri=='true'" type="checkbox" class="peer" />
                                                            <label ng-bind="d.TenTruong"></label>
                                                        </div>
                                                    </div>
                                                    <div multiswitch-when="radio">
                                                        <div class="radio radio-circle radio-info peers ai-c mB-15" style="margin-left:7px">
                                                            <input onclick="return false;" ng-checked="detail_request.Radio==d.FormD_ID" type="radio" class="peer" />
                                                            <label ng-bind="d.TenTruong"></label>
                                                        </div>
                                                    </div>
                                                    <div multiswitch-when="date" class="dropdown datetimeinput">
                                                        <span ng-bind="d.IsGiatri.substring(0, 10)|date:'dd/MM/yyyy'"></span>
                                                    </div>
                                                    <div multiswitch-when="datetime" class="dropdown datetimeinput">
                                                        <span ng-bind="d.IsGiatri|date:'HH:mm dd/MM/yyyy'"></span>
                                                    </div>
                                                    <div multiswitch-when="time" class="dropdown datetimeinput">
                                                        <span ng-bind="d.IsGiatri|date:'HH:mm'"></span>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="formd">
                                                <div v-for="d in FormDS|filter:{IsParent_ID:d.FormD_ID}:true" ng-include="'FormD.html'"></div>
                                            </div>
                                        </div>
                                    </script>
                                    <div class="formd-0" style="margin-top:10px">
                                        <div v-for="d in FormDS|filter:{IsParent_ID:null}:true" ng-include="'FormD.html'"></div>
                                    </div>
                                </div>
                                -->
                                <div class="row mt-2 mb-1">
                                    <div class="t-r">
                                        <div class="flex" style="align-items: center;">
                                            <span class="cv-spicon flex" style="align-items:center;">
                                                <i class="pi pi-file"></i>
                                            </span>
                                            <span class="cv-request">Đề xuất liên quan ({{ RelateRequests.length || 0 }})</span>
                                            <span class="flex ml-2"
                                                v-if="detail_request.IsEdit" 
                                                v-tooltip.top="'Thêm đề xuất liên quan'" 
                                                @click="openRelate(detail_request,'srequest',1)">
                                                <i class="pi pi-plus-circle"></i>
                                            </span>
                                        </div>
                                    </div>
                                    <table class="table table-condensed" style="margin-left:0;"
                                        v-if="RelateRequests.length > 0">
                                        <tbody>
                                            <tr v-for="r in RelateRequests">
                                                <td class="" 
                                                    style="cursor:pointer;text-align:center;" 
                                                    @click="openURLRQ(r)" 
                                                    :class="r.status != 2 && r.is_overdue && r.Deadline && r.SoNgayHan <= 24 ? 'overdue-request' : ''"
                                                >
                                                    <span style="word-break: break-all;">{{ r.request_code }}</span>
                                                    <div class="mt-2" v-if="col-r.status_processing == 3">
                                                        <Rating class="star-rating-custom"
                                                            v-model="r.evaluated_score"
                                                            v-tooltip.top="{ value: 'Ngày đánh giá: <br/>' + (r.evaluated_date ? moment(new Date(r.evaluated_date)).format('HH:mm DD/MM/yyyy') : ''), escape: true }"
                                                            :stars="5"
                                                            :cancel="false" 
                                                            :readonly="true"
                                                        />
                                                    </div>
                                                    <span class="rq" 
                                                        :class="r.objStatus.class"
                                                        style="display:flex"
                                                    >
                                                        <strong style="text-align:center;flex:1">{{ r.objStatus.text }}</strong>
                                                        <i class="pi pi-check-circle" v-if="r.status_processsing == 3" style="margin-left:2px"></i>
                                                    </span>
                                                </td>
                                                <td style="cursor:pointer" @click="openURLRQ(r)">
                                                    <span class="uutien"
                                                        :class="'uutien' + (r.priority_level || 0)" 
                                                        v-if="r.priority_level > 0"
                                                    >
                                                        {{ r.priority_level == 1 ? 'Gấp' : 'Rất gấp' }}
                                                    </span>
                                                    <span class="card-nhom flex text-left" 
                                                        style="padding: 0.25rem 0.5rem;background-color: #ff8b4e;color: #fff;margin-right: 0.5rem !important;"
                                                        v-if="r.is_change_process">Quy trình động</span>
                                                    <span style="font-weight:bold">
                                                        {{ r.request_name }}
                                                    </span>
                                                    <p style="font-size:12px;margin:2px 0;line-height: 12px;font-weight:bold;font-weight:500">
                                                        {{ r.full_name }}
                                                    </p>
                                                    <p style="font-size:12px;margin:0;line-height: 12px;">
                                                        {{ r.department_name }}
                                                    </p>
                                                </td>
                                                <td>
                                                    <div class="card-users" 
                                                        @click="showChartSign(r)" 
                                                        style="cursor:pointer;margin:auto;text-align:center"
                                                    >
                                                        <div v-for="(g, idxSign) in limitData(r.Signs, 3)" 
                                                            style="display:inline-block">
                                                            <ul style="margin:0 5px">
                                                                <template v-for="u in g.USign">
                                                                    <li class="IsType0"
                                                                        :class="'IsSign' + u.is_sign + ' ' + 'iclose' + u.is_close + ' ' + 'Trangthai' + u.status"
                                                                        v-tooltip.top="{ value: (u.full_name+'<br/>'+u.position_name+'<br/>'+u.department_name), escape: true }">
                                                                        <Avatar
                                                                            v-bind:label="
                                                                                u.avatar
                                                                                ? ''
                                                                                : (u.last_name_completed_all ?? '').substring(0, 1).toUpperCase()
                                                                            "
                                                                            v-bind:image="
                                                                                u.avatar
                                                                                ? basedomainURL + u.avatar
                                                                                : basedomainURL + '/Portals/Image/nouser1.png'
                                                                            "
                                                                            v-tooltip.top="{ value: (u.full_name+'<br/>'+u.position_name+'<br/>'+u.department_name), escape: true }"
                                                                            style="color: #ffffff; width:2rem; height:2rem; font-size: 1rem !important;"
                                                                            :style="{ background: bgColor[0], }"
                                                                            size="large"
                                                                            shape="circle"
                                                                            class="border-radius"
                                                                        />
                                                                        <span class="IsSign" v-if="u.is_sign && u.is_sign != 0 && u.is_sign != 100">
                                                                            <i :class="'IsSign' + u.is_sign 
                                                                                + ' pi ' + (u.is_sign == 1 ? 'pi-check-circle' :
                                                                                            u.is_sign == -1 ? 'pi-stop-circle' :
                                                                                            u.is_sign == 2 ? 'pi-chevron-circle-right' : '')"
                                                                            ></i>
                                                                        </span>
                                                                    </li>
                                                                </template>
                                                                <template v-if="g.IsTypeDuyet!=0 || g.IsShowTV" v-for="u in limitData(g.Thanhviens,5)">
                                                                    <li
                                                                        class="IsType0"
                                                                        :class="'IsSign' + u.is_sign + ' ' + 'iclose' + u.is_close + ' ' + 'Trangthai' + u.status"
                                                                        v-tooltip.top="{ value: (u.full_name+'<br/>'+u.position_name+'<br/>'+u.department_name), escape: true }"
                                                                    >
                                                                        <Avatar
                                                                            v-bind:label="
                                                                                u.avatar
                                                                                ? ''
                                                                                : (u.last_name_completed_all ?? '').substring(0, 1).toUpperCase()
                                                                            "
                                                                            v-bind:image="
                                                                                u.avatar
                                                                                ? basedomainURL + u.avatar
                                                                                : basedomainURL + '/Portals/Image/nouser1.png'
                                                                            "
                                                                            v-tooltip.top="{ value: (u.full_name+'<br/>'+u.position_name+'<br/>'+u.department_name), escape: true }"
                                                                            style="color: #ffffff; width:2rem; height:2rem; font-size: 1rem !important;"
                                                                            :style="{ background: bgColor[0], }"
                                                                            size="large"
                                                                            shape="circle"
                                                                            class="border-radius"
                                                                        />
                                                                        <span class="IsSign" v-if="u.is_sign && u.is_sign != 0 && u.is_sign != 100">
                                                                            <i :class="'IsSign' + u.is_sign 
                                                                                + ' pi ' + (u.is_sign == 1 ? 'pi-check-circle' :
                                                                                            u.is_sign == -1 ? 'pi-stop-circle' :
                                                                                            u.is_sign == 2 ? 'pi-chevron-circle-right' : '')"
                                                                            ></i>
                                                                        </span>
                                                                    </li>
                                                                </template>
                                                                <li v-if="(g.IsTypeDuyet!=0 || g.IsShowTV) && g.Thanhviens.length > 5" class="IsType1">
                                                                    <div class="divav" 
                                                                        style="background-color:#f8fafb;color:#98a9bc!important;text-align:center;border-radius:50%;padding-top: 2px;margin-left:-16px;border: none;">
                                                                        <span>+{{g.Thanhviens.length - 5}}</span>
                                                                    </div>
                                                                </li>
                                                            </ul>
                                                        </div>
                                                    </div>
                                                </td>
                                                <td style="text-align:center">
                                                    <span>
                                                        {{ (r.created_date ? moment(new Date(r.created_date)).format('DD/MM/yyyy') : '') }}
                                                    </span>
                                                    <div class="mt-2" style="vertical-align:middle;width:120px" v-if="r.Tiendo && r.Tiendo > 0">
                                                        <ProgressBar class="progress-bar-custom" 
                                                            :class="renderColorProgress(r.Tiendo)"
                                                            :value="(r.Tiendo || 0)"
                                                        ></ProgressBar>
                                                    </div>
                                                </td>
                                                <td v-if="detail_request.IsEdit">
                                                    <span style="padding-top:10px;padding-right:5px" 
                                                        @click="Del_Relate('Đề xuất',r.RequestRelate_ID,RelateRequests)">
                                                        <i class="pi pi-trash"></i>
                                                    </span>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>                                
                                
                                <div class="form-group mb-0 mt-3" id="task-file">
                                    <div class="t-r">
                                        <div class="flex" style="align-items: center;">
                                            <span class="cv-spicon">
                                                <i class="pi pi-paperclip"></i>
                                            </span>
                                            <span class="cv-request">
                                                Tài liệu đính kèm ({{ filterFileType(LisFileAttachRQ, 'is_type', 0).length }})
                                            </span>
                                            <span class="flex ml-2"
                                                v-if="detail_request.IsEdit || detail_request.IsComment" 
                                                style="margin:5px 5px" 
                                                v-tooltip="'Thêm File đính kèm'"
                                                 @click="openModalAddFileCV(detail_request)">
                                                 <i class="pi pi-plus-circle"></i>
                                            </span>
                                        </div>
                                    </div>
                                    <div class="row" v-if="filterFileType(LisFileAttachRQ, 'is_type', 0).length>0">
                                        <div class="col-12 ListImagesFile" style="margin:5px 0;cursor:pointer">
                                            <table class="table table-condensed" style="table-layout:fixed;margin-left:0;width:100%;border-spacing: 0;">
                                                <tbody>
                                                    <tr v-for="ffile in filterFileType(LisFileAttachRQ, 'is_type', 0)">
                                                        <td class="td-table-file" width="50" style="text-align: center;">
                                                            <Image v-if="!ffile.is_image"
																class="flex image-type-file"
																:src="basedomainURL + '/Portals/Image/file/' + ffile.file_type + '.png'"
																@error="$event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'"
																style="height: 100%; width: 100%; object-fit: contain;justify-content: center;padding: 0.25rem;"
															/>
                                                            <Image v-else
																class="flex image-type-file"
																:src="basedomainURL + ffile.file_path"
																@error="$event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'"
																style="height: 100%; width: 100%; object-fit: contain;justify-content: center;padding: 0.25rem;"
																preview
															/>
                                                        </td>
                                                        <td class="td-table-file pl-1">
                                                            <a v-if="!ffile.is_image && ffile.file_type != 'pdf'">
                                                                {{ ffile.file_name }}
                                                            </a>
                                                            <a v-if="ffile.file_type == 'pdf'">
                                                                {{ ffile.file_name }}
                                                                <span v-if="ffile.file_path_ca" style="color:green">(Đã thêm chữ ký số)</span>
                                                            </a>
                                                            <a v-if="ffile.is_image">
                                                                {{ ffile.file_name || '' }}
                                                            </a>
                                                        </td>
                                                        <td class="td-table-file" width="120" style="text-align: center;">
                                                            {{ ffile.created_date ? moment(ffile.created_date || new Date()).format("DD/MM/YYYY") : "" }}
                                                        </td>
                                                        <td class="td-table-file" width="100" style="text-align: center;">
                                                            {{ formatByte(ffile.file_size) }}
                                                        </td>
                                                        <td class="td-table-file" 
                                                            :style="detail_request.IsEdit ? 'width:100px' : 'width:50px'"
                                                        >
                                                            <div class="flex" style="justify-content: center;">
                                                                <Button class="p-button-text p-button-secondary p-button-rounded"
                                                                    icon="pi pi-download"
                                                                    @click="downloadFile(ffile)"
                                                                />
                                                                <Button class="p-button-text p-button-danger p-button-rounded ml-1"
                                                                    icon="pi pi-trash"
                                                                    @click="Del_CommentFile(LisFileAttachRQ, $index, ffile)"
                                                                    v-if="detail_request.IsEdit"
                                                                />
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>                                
                            </form>
                            
                            <div class="t-r mt-3">
                                <div class="flex" style="align-items:center;">
                                    <span class="cv-spicon" style="margin-top:-2px">
                                        <i class="pi pi-comments"></i>
                                    </span>
                                    <span class="cv-request">Thảo luận ({{ Comments.length || 0 }})</span>
                                </div>
                            </div>
                            
                            <div class="task-comment" id="task-comment">
                                <div class="my-3"
                                    style="display:table;height:100%;width:100%;"
                                    v-if="Comments.length == 0"
                                >
                                    <div class="align-items-center justify-c p-0 text-center m-auto">
                                        <img src="../../../../src/assets/background/nodata.png" height="144" />                                        
                                        <h3 class="m-1">Hiện chưa có thảo luận nào cho yêu cầu - đề xuất này.</h3>
                                    </div>
                                </div>
                                
                                <div v-for="u in Comments">
                                    <div class="row-comment">
                                        <div class="r-ava">
                                            <Avatar
                                                v-bind:label="
                                                    u.avatar
                                                    ? ''
                                                    : (u.last_name ?? '').substring(0, 1).toUpperCase()
                                                "
                                                v-bind:image="
                                                    u.avatar
                                                    ? basedomainURL + u.avatar
                                                    : basedomainURL + '/Portals/Image/nouser1.png'
                                                "
                                                v-tooltip.top="{ value: (u.full_name + '<br/>' + u.position_name + '<br/>' + u.department_name), escape: true }"
                                                style="color: #ffffff; font-size: 1rem !important;"
                                                :style="{ background: bgColor[0], }"
                                                size="large"
                                                shape="circle"
                                                class="border-radius"
                                            />
                                        </div>
                                        <div class="r-cbox">
                                            <div class="r-cname">
                                                <span>{{ u.full_name }} </span>
                                                <span class="r-cdate ml-1">
                                                    {{ (u.created_date ? moment(new Date(u.created_date)).format('HH:mm DD/MM/yyyy') : '') }}
                                                </span>
                                            </div>
                                            <div class="r-cm" style="word-break: break-word;">
                                                <div class="reply-chat show-reply"
                                                    style="padding: 10px;border-bottom: 0.5px solid #ccc;margin-bottom: 10px;"
                                                    v-if="u.ParentComment"
                                                >
                                                    <div class="row">
                                                        <div class="content-reply flex">
                                                            <font-awesome-icon icon="fa-solid fa-quote-right" style="font-size: 1rem; color: gray;padding-bottom: 5px;" />
                                                            <div style="display: inline-block; padding: 0px 10px 5px;" 
                                                                class="bind-chat-html" 
                                                                v-html="u.ParentComment.content"
                                                                v-if="u.ParentComment.type_comment">
                                                            </div>
                                                            <div style="display: inline-block; padding: 0px 10px 5px;" class="bind-chat-html" v-else>
																<Image v-if="u.ParentComment.type_comment == 1 && u.ParentComment.files.length > 0"
																	class="flex"
																	:src="basedomainURL + (u.ParentComment.files[0].file_path ||'/Portals/Image/noimg.jpg')"
																	style="height: 3rem; object-fit: contain;"
																/>
																<div class="r-fbox image_file_chat flex" style="align-items: center;" v-else>
																	<img style="width:32px;" 
																		v-bind:src="basedomainURL+'/Portals/Image/file/'+u.ParentComment.files[0].file_type+'.png'" 
																		v-if="u.ParentComment.files.length > 0"
																	/>
																	<a class="ml-2" style="color:#a9a69e; font-size: 0.9rem;" v-if="u.ParentComment.files.length > 0">
																		<b>{{u.ParentComment.files[0].file_name}}</b>
																	</a>
																</div>
															</div>
                                                        </div>
                                                        <div class="name-date-reply" style="text-align: left; white-space: nowrap;">
                                                            <span style="color: #888;font-size: 12px;">
                                                                {{ u.ParentComment.full_name + ', ' + 
																	(moment(new Date(u.ParentComment.created_date)).format("DD/MM/YYYY") == moment(new Date()).format("DD/MM/YYYY") 
																	? ("Hôm nay lúc " + moment(new Date(u.ParentComment.created_date)).format("HH:mm"))
																	: moment(new Date(u.ParentComment.created_date)).format("DD/MM/YYYY"))
																}}
                                                            </span>
                                                        </div>
                                                    </div>
                                                </div>
                                                <a :href="u.content" target="_blank" v-if="u.content.includes('http://') || u.content.includes('https://')">
													<div v-html="u.content" style="display: inline-grid; text-align: -webkit-left; padding: 5px 10px; margin-bottom:0;word-break:break-word"></div>
												</a>
												<div v-else v-html="u.content" style="display: inline-grid; text-align: -webkit-left; padding: 5px 10px; margin-bottom:0;word-break:break-word"></div>
                                            </div>
                                            <!--
                                            <div class="r-file" v-if="u.files.length>0" style="padding:0">
                                                <ul>
                                                    <li v-for="f in u.files" class="border-none px-0 py-2">
                                                        <div class="r-fbox">
                                                            <a @click="$root.openFile(f.Tenfile,f.Duongdan)" v-if="f.IsImage!='1' && f.Dinhdang!='pdf'">
                                                                <img width="32" onerror="this.style.display = 'none'" ng-src="{{$root.fileUrl+'/Content/file/'+f.Dinhdang+'.png'}}" v-if="f.IsImage!='1' && f.Dinhdang!='pdf'" />
                                                            </a>
                                                            <a ng-href="{{$root.fileUrl+f.Duongdan}}" v-if="f.IsImage=='1'" data-fancybox data-caption="{{f.Tenfile}}">
                                                                <img width="32" ng-src="{{$root.fileUrl+f.Duongdan}}" />
                                                            </a>
                                                            <a ng-href="{{$root.fileUrl+f.Duongdan}}" v-if="f.Dinhdang=='pdf'" data-fancybox data-caption="{{f.Tenfile}}">
                                                                <img width="32" ng-src="{{$root.fileUrl+'/Content/file/'+f.Dinhdang+'.png'}}" v-if="f.IsImage!='1'" />
                                                            </a>
                                                            <a @click="$root.openFile(f.Tenfile,f.Duongdan)" v-if="f.IsImage!='1' && f.Dinhdang!='pdf'" style="color:inherit">
                                                                <b>{{f.Tenfile}}</b>
                                                            </a>
                                                            <a v-if="f.IsImage=='1'" ng-href="{{$root.fileUrl+f.Duongdan}}" data-fancybox data-caption="{{f.Tenfile}}" style="color:inherit">
                                                                <b>{{f.Tenfile}}</b>
                                                            </a>
                                                            <span>{{f.Dungluong|bytes:"MB"}}</span>
                                                            <div style="width:40px;position:absolute;top:-20px;right:-20px;" v-if="detail_request.IsEdit">
                                                                <div class="btn-group" role="group">
                                                                    <button type="button" title="Tùy chọn chức năng"
                                                                            class="btn cur-p no-after dropdown-toggle" data-toggle="dropdown"
                                                                            aria-haspopup="true" aria-expanded="false">
                                                                        <i class="ti-more-alt"></i>
                                                                    </button>
                                                                    <ul class="dropdown-menu fsz-sm trf" aria-labelledby="btnGroupDrop1" style="width: max-content;">
                                                                        <li> <a href="" @click="$root.openFile(f.Tenfile,f.Duongdan)" class="d-b td-n pY-10 pX-10 bgcH-grey-100"><i class="la la-download"></i> <span>Tải xuống</span></a></li>
                                                                        <li role="separator" class="divider"></li>
                                                                        <li v-if="detail_request.isdelcomment || u.NguoiTao==$root.login.u.NhanSu_ID"> <a href="" @click="Del_CommentFile(u.files,$index,f)" class="d-b td-n pY-10 pX-10 bgcH-grey-100 c-red-700"><i class="la la-trash-o"></i> <span>Xóa file này</span></a></li>
                                                                    </ul>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </li>
                                                </ul>
                                            </div>
                                            <div class="r-action pT-10">
                                                <div v-if="!u.IsEdit">
                                                    <ul v-if="u.countstisk>0" @click="getStick(u)">
                                                        <li v-for="st in u.sticks track by st.Stick_File">
                                                            <img ng-src="{{$root.fileUrl+st.Stick_File}}" />
                                                        </li>
                                                        <li v-if="u.countstisk - 3 > 0"><span> +{{u.countstisk - 3}}</span></li>
                                                    </ul>
                                                    <a v-if="u.NguoiTao !== $root.login.u.NhanSu_ID" @click="clickLike(u,1)">Thích</a>
                                                    <a v-if="!congviec.isclosett" @click="u.IsReply?HuyReply():Reply(u)">{{u.IsReply?'Hủy':'Trả lời ('+u.CountReply.length+')'}}</a>
                                                </div>
                                                <div v-if="u.IsEdit">
                                                    <a @click="u.IsEdit = !(u.IsEdit || false)">Hủy</a>
                                                </div>
                                            </div>
                                            <div v-if="u.IsEdit">
                                                <div class="r-file" v-if="u.FileAttach_Edit.length>0">
                                                    <h3 style="font-weight: bold;font-size: 16px;margin: 10px 0;border-top: 1px solid #f5f5f5;padding-top: 15px;color: #2196f3;">Danh sách file đã chọn</h3>
                                                    <ul>
                                                        <li v-for="f in u.FileAttach_Edit">
                                                            <div class="r-fbox">
                                                                <img width="32" ng-src="{{$root.fileUrl+'/Content/file/'+f.Dinhdang+'.png'}}" />
                                                                <b>{{f.Tenfile}}</b>
                                                                <span>{{f.Dungluong|bytes:"MB"}}</span>
                                                                <div style="width:40px;position:absolute;top:-10px;right:-20px;">
                                                                    <a @click="removeFilesComment_Edit(u.FileAttach_Edit,$index)"><i style="font-size:20px" class="la la-times-circle"></i></a>
                                                                </div>
                                                            </div>
                                                        </li>
                                                    </ul>
                                                </div>
                                                <div class="msb-reply" style="margin: 10px 0;border: 1px solid #eee;border-radius: 20px;">
                                                    <div style="height:75px; padding: 10px; padding-right: 200px; overflow: auto;" id="noiDungChat_Edit" ng-model="u.Noidung_Edit" contenteditable="true" my-enter="sendEditMS(u)" ng-keyup="completecm($event)" ng-mouseover="congviec.Focuscm=false" data-text="Nhập nội dung bình luận..."></div>
                                                    <div class="showiconchat">
                                                        <button data-toggle="dropdown" title="Gửi Sticker" style="right:150px;color:#777"><i class="las la-smile" style="font-size: 23px;"></i></button>
                                                        <ul class="dropdown-menu dropdown-menuiconchat" role="menu" style="width:340px;top: -33px!important;right: 0;left: auto; bottom: inherit;margin:0;padding:0;border:0">
                                                            <li>
                                                                <div id="chhome" class="tab-pane fade in active show">
                                                                    <ul class="emoji">
                                                                        <li v-for="e in emojis|filter:{category:cart[emojiIndex].name}:true"><a @click="setEmoji_Edit(u,e,false)"><span ng-bind="e.emoji"></span></a></li>
                                                                    </ul>
                                                                    <div class="emoji-tabs">
                                                                        <ul class="navbar-bottomleft">
                                                                            <li v-for="c in cart"><a title="{{c.name}}" class="{{emojiIndex==$index}}" @click="setemojiIndex_Edit($index)" data-keepOpenOnClick><span ng-bind="c.emoji"></span></a></li>
                                                                        </ul>
                                                                    </div>
                                                                </div>
                                                            </li>
                                                        </ul>
                                                        <button style="right:100px;color:#777" @click="openImageChat_Edit()" title="Gửi ảnh">
                                                            <i class="la la-image" style="font-size: 23px;"></i>
                                                        </button>
                                                        <input style="display:none" type="file" name="FileImagesChat_Edit" class="inputimgChats_Edit" accept=".png, .jpg, .jpeg, .gif" multiple="multiple" ngf-select="UploadFileChat_Edit(u,$files,1)" />
                                                        <button style="right:50px;color:#777" @click="openFileChat_Edit('inputimgChats_Edit')" title="Gửi file đính kèm">
                                                            <i class="la la-paperclip" style="font-size: 23px;"></i>
                                                        </button>
                                                        <input style="display:none" type="file" name="FileAttachChat_Edit" class="inputfileChat inputfileChatcm" multiple accept=".xlsx,.xls,image/*,.doc, .docx,.ppt, .pptx,.txt,.pdf" ngf-select="UploadFileChat_Edit(u,$files,2)" />
                                                        <button @click="sendEditMS(u)"><i class="la la-paper-plane" style="font-size: 23px;"></i></button>
                                                    </div>
                                                </div>
                                            </div>
                                            -->
                                        </div>
                                        <!--
                                        <div style="width:40px">
                                            <div class="btn-group" role="group">
                                                <button type="button" title="Tùy chọn chức năng"
                                                        class="btn cur-p no-after dropdown-toggle" data-toggle="dropdown"
                                                        aria-haspopup="true" aria-expanded="false">
                                                    <i class="ti-more-alt"></i>
                                                </button>
                                                <ul class="dropdown-menu fsz-sm trf" aria-labelledby="btnGroupDrop1" style="width: max-content;">
                                                    <li v-if="u.NguoiTao === $root.login.u.NhanSu_ID"> <a href="javascript:void(0);" @click="EditComment(u)" class="d-b td-n pY-10 pX-10 bgcH-grey-100"><i class="la la-edit"></i> <span>Chỉnh sửa thảo luận</span></a></li>
                                                    <li> <a href="" @click="Reply(u)" class="d-b td-n pY-10 pX-10 bgcH-grey-100"><i class="la la-copy"></i> <span>Trả lời</span></a></li>
                                                    <li v-if="detail_request.isdelcomment || u.NguoiTao==$root.login.u.NhanSu_ID" role="separator" class="divider"></li>
                                                    <li v-if="detail_request.isdelcomment || u.NguoiTao==$root.login.u.NhanSu_ID"> <a href="" @click="Del_Comment(u,$index)" class="d-b td-n pY-10 pX-10 bgcH-grey-100 c-red-700"><i class="la la-trash-o"></i> <span>Xóa thảo luận</span></a></li>
                                                </ul>
                                            </div>
                                        </div>
                                        -->
                                    </div>
                                </div>
                                <!--
                                <div class="r-file" v-if="FileAttach.length>0">
                                    <h3 style="font-weight: bold;font-size: 16px;margin: 10px 0;border-top: 1px solid #f5f5f5;padding-top: 15px;color: #2196f3;">Danh sách file đã chọn</h3>
                                    <ul>
                                        <li v-for="f in FileAttach">
                                            <div class="r-fbox">
                                                <img width="32" ng-src="{{$root.fileUrl+'/Content/file/'+f.Dinhdang+'.png'}}" />
                                                <span class="font-bold">{{ f.file_name }}</span>
                                                <span>{{ formatByte(f.file_size) }}</span>
                                                <div style="width:40px;position:absolute;top:-10px;right:-20px;">
                                                    <a @click="removeFilesComment(FileAttach,$index)"><i style="font-size:20px" class="la la-times-circle"></i></a>
                                                </div>
                                            </div>
                                        </li>
                                    </ul>
                                </div>
                            -->
                            </div>
                            
                        </div>
                        <div v-if="detail_request.IsViewXL">
                            <div class="scrollbox-content"
                                style="margin-right:0;"
                            >
                                <div class="box-jobStask">
                                    <template v-for="(job, idxJob) in orderDatas(RQJobs, 'is_order')">
                                        <div class="box-job" v-if="job.isCurrent == col-job.isCurrent == null">
                                            <div class="job-headder">
                                                <a style="padding:5px" data-toggle="collapse" data-target="#CollJob{{job.RequestJob_ID}}"><i class="la la-angle-down"></i></a>
                                                <div class="job-ava" data-toggle="collapse" data-target="#CollJob{{job.RequestJob_ID}}">
                                                    <Avatar
                                                        v-bind:label="
                                                            job.avatar
                                                            ? ''
                                                            : (job.last_name ?? '').substring(0, 1).toUpperCase()
                                                        "
                                                        v-bind:image="
                                                            job.avatar
                                                            ? basedomainURL + job.avatar
                                                            : basedomainURL + '/Portals/Image/nouser1.png'
                                                        "
                                                        v-tooltip.top="{ value: (job.full_name + '<br/>' + job.position_name + '<br/>' + job.department_name), escape: true }"
                                                        style="color: #ffffff; font-size: 1rem !important;"
                                                        :style="{ background: bgColor[0], }"
                                                        size="large"
                                                        shape="circle"
                                                        class="border-radius"
                                                    />
                                                </div>
                                                <div class="job-row" data-toggle="collapse" data-target="#CollJob{{job.RequestJob_ID}}">
                                                    <b ng-bind="job.Job_Name"></b>
                                                    <div class="card-date text-left" style="font-size:12px;flex:1">
                                                        <span ng-bind="job.Ngaybatdau|date:'HH:mm dd/MM/yyyy'"></span>
                                                        <span class="">-</span>
                                                        <span ng-bind="job.Ngayketthuc|date:'HH:mm dd/MM/yyyy'"></span>
                                                    </div>
                                                    <div class="duan-process-bg" style="flex:1;position:unset;font-size:11px;padding-top:8px;max-width:200px" v-if="job.Tiendo && job.Tiendo>0">
                                                        <div class="progress" style="height:16px;margin-bottom:0;background-color: #aaa;">
                                                            <div class="progress-bar" style="background-color:{{job.color}};max-width: {{job.Tiendo}}%" role="progressbar" aria-valuenow="{{job.Tiendo}}" aria-valuemin="0" aria-valuemax="100">
                                                                <span ng-bind="(job.Tiendo||0)+'%'"></span>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div style="height:32px" class="btn-group">
                                                    <button type="button" style="background-color:{{job.TrangthaiColor}};width:110px" class="btn btn-success dropdown-toggle dropdown-toggle-split {{(job.IsFun && job.Trangthai<2)?'':'no-after'}}" data-toggle="dropdown">
                                                        <span ng-bind="job.TrangthaiTen"></span>
                                                        <span v-if="job.IsFun && job.Trangthai<2" class="caret"></span>
                                                    </button>
                                                    <div class="dropdown-menu" v-if="job.IsFun && job.Trangthai<2">
                                                        <a @click="editJob(detail_request,job,2)" class="dropdown-item"><i class="la la-pencil"></i> Chỉnh sửa nhiệm vụ</a>
                                                        <a @click="editJob(detail_request,job,3)" class="dropdown-item"><i class="la la-tasks"></i> Thêm nhanh công việc cho nhiệm vụ</a>
                                                        <a @click="openModalAddCV(detail_request,job)" class="dropdown-item"><i class="la la-tasks"></i> Tạo công việc cho nhiệm vụ</a>
                                                        <a v-if="$index!==RQJobs.length-1" @click="MoveJob($index,false)" class="dropdown-item"><i class="la la-angle-down"></i> Chuyển xuống dưới</a>
                                                        <a v-if="$index!==0" @click="MoveJob($index,true)" class="dropdown-item"><i class="la la-angle-up"></i> Chuyển lên</a>
                                                        <a @click="DelJob(job)" class="dropdown-item"><i class="la la-trash"></i> Xóa</a>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="box-task collapse show" id="CollJob{{job.RequestJob_ID}}">
                                                <div class="job-task card" v-for="d in orderDatas(job.Congviecs, is_order)" select="select{{CongviecID==d.CongviecID}}">
                                                    <div class="card-ten text-left" style="cursor:pointer;-webkit-line-clamp:unset;font-size:14px;flex:1">
                                                        <div style="display:flex">
                                                            <div style="flex:1" @click="goInfoCV(d)">
                                                                {{d.CongviecTen}}
                                                            </div>
                                                            <div style="height:32px" class="btn-group" v-if="job.IsFun">
                                                                <a class="dropdown-toggle dropdown-toggle-split no-after" data-toggle="dropdown" style="padding:0"><i style="font-size:16px" class="las la-ellipsis-h"></i></a>
                                                                <div class="dropdown-menu">
                                                                    <a @click="editJob(detail_request,job,4,d)" class="dropdown-item"><i class="la la-pencil"></i> Chỉnh sửa công việc</a>
                                                                    <a v-if="$index!==job.Congviecs.length-1" @click="MoveJobTask(job,$index,false)" class="dropdown-item"><i class="la la-angle-down"></i> Chuyển xuống dưới</a>
                                                                    <a v-if="$index!==0" @click="MoveJobTask(job,$index,true)" class="dropdown-item"><i class="la la-angle-up"></i> Chuyển lên trên</a>
                                                                    <a @click="DelJobTask(job,d)" class="dropdown-item"><i class="la la-trash"></i> Xóa</a>
                                                                </div>
                                                            </div>
                                                        </div>
                                                        <div style="display:flex;">
                                                            <div class="card-date text-left" style="font-size:12px;flex:1">
                                                                <span ng-bind="d.NgayBatDau|date:'dd/MM/yyyy'"></span>
                                                                <span ng-bind="d.NgayKetThuc|date:' - dd/MM/yyyy'"></span>
                                                            </div>
                                                            <div>
                                                                <span v-if="d.giahan==0" style="color:#fff;border-radius:20px;padding:5px 10px;font-size:11px;width:100%;background-color:{{d.ttcolor}}">
                                                                    {{d.TrangthaiTen}}
                                                                </span>
                                                                <span v-if="d.giahan>0" style="color:#fff;border-radius:20px;padding:5px 10px;font-size:11px;width:100%;background-color:orange">
                                                                    Xin gia hạn
                                                                </span>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div style="display:flex" @click="goInfoCV(d)">
                                                        <div class="card-users" style="text-align:left;">
                                                            <ul>
                                                                <template v-for="u in limitData(d.Thanhviens,5)">
                                                                    <li class="IsType{{u.IsType}}" 
                                                                        v-tooltip.right="{ value: (u.full_name+'<br/>'+u.position_name+'<br/>'+u.department_name), escape: true }">
                                                                        <img v-if="u.anhThumb && u.anhThumb!=='/Content/noavatar.jpg'" class="ava" ng-src="{{$root.fileUrl+(u.anhThumb||'/Content/noavatar.jpg')}}" on-error="/Content/noavatar.jpg" />
                                                                        <div class="divav" style="display:{{!u.anhThumb || u.anhThumb==='/Content/noavatar.jpg'?'block':'none'}};background-color:{{$root.bgColor[$index%5]}};color:#fff!important;text-align:center;border-radius:50%;padding-top:3px;">
                                                                            <span>{{u.ten.trim().substring(0,1)}}</span>
                                                                        </div>
                                                                    </li>
                                                                </template>
                                                                <li v-if="d.Thanhviens.length > 5" class="IsType1">
                                                                    <div class="divav" style="background-color:#f8fafb;color:#98a9bc!important;text-align:center;border-radius:50%;padding-top: 2px;margin-left:-16px;border: none;">
                                                                        <span>+{{d.Thanhviens.length-5}}</span>
                                                                    </div>
                                                                </li>
                                                            </ul>
                                                        </div>
                                                        <div class="duan-process-bg" style="flex:1;position:unset;font-size:11px;padding-top:12px;margin:0 10px">
                                                            <div class="progress" style="height:16px;margin-bottom:0" v-if="d.Tiendo && d.Tiendo>0">
                                                                <div class="progress-bar" style="background-color:{{d.color}};max-width: {{d.Tiendo}}%" role="progressbar" aria-valuenow="{{d.Tiendo}}" aria-valuemin="0" aria-valuemax="100">
                                                                    <span ng-bind="(d.Tiendo||0)+'%'"></span>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </template>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div
                        v-if="isClose == false"
                        class="absolute format-center col-12 p-0 m-0 flex bottom-0"
                        style="border-radius: 0.25rem;border: 1px solid #b3b3b3;width:calc(100% - 1.5rem);"
                    >
                        <div class="border-0 flex p-0 m-0" style="flex:1;">
                            <QuillEditor
                                ref="comment_zone_main"
                                placeholder="Nhập nội dung bình luận..."
                                contentType="html"
                                :content="comment"
                                v-model:content="comment"
                                theme="bubble"
                                @selectionChange="Change($event)"
                                style="height: 5rem;width:100%;"
                                @keydown.enter.exact.prevent="addComment()"
                            />
                        </div>
                        <div class="flex p-0 m-0">
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
        </div>
        <div class="col-4 md:col-4 pb-0 px-3 pt-3 m-0" v-if="detail_request != null">
            <div class="row col-12 p-0 flex">
                <!-- <div
                    class="col-12 format-center font-bold py-3 mt-3 mb-2 flex"
                    :class="''"
                    style="background-color:#ccf2f6; color: #00626e;"
                >
                    <i class="pi pi-clock pr-2" v-if="TimeToDo != 'Chưa bắt đầu'" />
                    <span class="flex" v-html="TimeToDo"></span>
                </div> -->
                <div class="flex alert-request alert-danger ml-0"
                    style="align-items: center;" 
                    v-if="detail_request.is_overdue"
                >
                    <i class="pi pi-info-circle" style="font-size:16px;margin-top:2px;margin-right:5px"></i>
                    <div class="ml-2">
                        <span class="font-bold" v-if="detail_request.SoNgayHan != 0">Quá hạn ({{ detail_request.SoNgayHan + ' '}} giờ)</span>
                        <span class="font-bold" v-if="detail_request.SoNgayHan == 0">Đến hạn xử lý</span>
                    </div>
                    <span class="font-bold ml-2 span-deadline" v-if="detail_request.status != 2" 
                        @click="openModalDatelineRequest(detail_request)"
                    >
                        Gia hạn
                        <span class="font-bold ml-1" v-if="datelines.length > 0">
                            {{ '(' + datelines.length + ' lần)' }}
                        </span>
                    </span>
                </div>
                <div class="flex alert-request alert-warning ml-0"
                    style="align-items: center;"  
                    v-if="!detail_request.is_overdue && detail_request.times_processing_max > 0 && detail_request.IsProgress"
                >
                    <i class="pi pi-clock" style="font-size:16px"></i>
                    <div class="ml-2">
                        <span class="font-bold" v-if="detail_request.SoNgayHan != 0">Hạn còn {{ detail_request.SoNgayHan || ' ' }} giờ</span>
                        <span class="font-bold" v-if="detail_request.SoNgayHan == 0">Đến hạn xử lý</span>
                    </div>
                    <span class="font-bold ml-2 span-deadline" v-if="detail_request.status != 2" 
                        @click="openModalDatelineRequest(detail_request)"
                    >
                        Gia hạn
                        <span class="ml-1" v-if="datelines.length > 0">
                            {{ '(' + datelines.length + ' lần)' }}
                        </span>
                    </span>
                </div>
                <div class="flex alert-request alert-info ml-0"
                    style="align-items: center;"
                    v-if="!detail_request.is_overdue && !detail_request.IsProgress"
                >
                    <i class="pi pi-clock" style="font-size:16px"></i>
                    <span class="font-bold ml-2">
                        Số giờ xử lý: {{detail_request.times_processing || 0}}/{{detail_request.times_processing_max || 0}} giờ
                    </span>
                </div>
            </div>
            <div class="row col-12 p-0 flex">
                <span class="w-full rq"
                    :class="detail_request.objStatus.class || ''"
                    style="font-size:1rem;font-weight: bold;padding:0.75rem;border-radius:0.25rem;text-align: center;"
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
    ::v-deep(.classOver0) {
        .p-progressbar-value {
            background: #ff0000;
        }
    }
    ::v-deep(.classOver30) {
        .p-progressbar-value {
            background: #fe4d97;
        }
    }
    ::v-deep(.classOver50) {
        .p-progressbar-value {
            background: #2196f3;
        }
    }
    ::v-deep(.classOver75) {
        .p-progressbar-value {
            background: #6dd230;
        }
    }
    ::v-deep(.image-type-file) {
        img {
            max-height: 32px; 
            max-width: 100%; 
            object-fit: contain;
        }
    }
</style>