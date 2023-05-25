<script setup>
import { ref, inject, onMounted, watch, onBeforeUnmount } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { integer, required } from "@vuelidate/validators";
import moment from "moment";
import { encr } from "../../../../util/function.js";
import { useRoute } from "vue-router";
import DetailedRequest from "../../../request/component_request/detail_request.vue";
const router = inject("router");
const route = useRoute();
const emitter = inject("emitter");
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const toast = useToast();
const baseUrlCheck = baseURL;
const widthWindow = ref(window.screen.availWidth);
const PositionSideBar = ref('right');
const PositionSideBar1 = ref('right');
const config = {
    headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = fileURL;
const componentKey = ref(0);
const forceRerender = () => {
    componentKey.value += 1;
};
const props = defineProps({
    isShow: Boolean,
    counts: Object,
    tab: Number,
    type_tab: Number,
});
const options = ref({
    loading: true,
    search: "",
    pageNo: 1,
    pageSize: 20,
    total: 0,
    tab: props.tab,
    type_tab: props.type_tab,
    type_form_requests: [],
    status_overdue: [],
    status_requests: [],
    created_by: null,
    organizations: [],
    teams: [],
    roles: [],
    start_created: null,
    end_created: null,
    start_completed: null,
    end_completed: null,
    is_func: false,
    request_form_id: null,
});
const listViewRequest = ref([]);
const opition = ref({
    search: '',
    PageNo: 0,
    PageSize: 20,
    sort: "created_date",
    ob: "DESC",
});
const isFirst = ref(true);
const listStatusRequests = ref([
    { id: 0, text: "Mới lập", class: "rqlap" },
    { id: 1, text: "Chờ duyệt", class: "rqchoduyet" },
    { id: 2, text: "Chấp thuận", class: "rqchapthuan" },
    { id: -2, text: "Từ chối", class: "rqtuchoi" },
    { id: -1, text: "Hủy", class: "rqhuy" },
    { id: 3, text: "Thu hồi", class: "rqthuhoi" },
    { id: -3, text: "Xóa", class: "rqxoa" }
]);
const bgColor = ref([
    "#F8E69A",
    "#AFDFCF",
    "#F4B2A3",
    "#9A97EC",
    "#CAE2B0",
    "#8BCFFB",
    "#CCADD7",
]);
const loadData = (rf) => {
    let is_in = options.value.tab == 2 ? options.value.type_tab == 1 ? true : false : true;
    let is_out = options.value.tab == 2 ? options.value.type_tab == 2 ? true : false : false;
    axios
        .post(
            baseURL + "/api/request/getData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "request_list_by_user",
                        par: [
                            { par: "user_id", va: store.getters.user.user_id },
                            { par: "search", va: options.value.search },
                            { par: "pageNo", va: options.value.pageNo },
                            { par: "pageSize", va: options.value.pageSize },
                            { par: "tab", va: options.value.tab },
                            { par: "is_in", va: is_in  },
                            { par: "is_out", va: is_out },
                        ],
                    }),
                    SecretKey,
                    cryoptojs
                ).toString(),
            },
            config
        )
        .then((response) => {
            if (response != null && response.data != null) {
                let data = JSON.parse(response.data.data);
                if (data != null) {
                    if (data[0] != null && data[0].length > 0) {
                        data[0].forEach((item, i) => {
                            item["STT"] = i + 1;
                            item.bgtiendo = parseInt(item.StaskTiendo / 10) * 10;
                            item.objStatus = listStatusRequests.value.find(x => x.id == item.status);
                            if (item.listSignUser != null) {
                                item.listSignUser = JSON.parse(item.listSignUser);
                                if (item.listSignUser.length > 0) {
                                    item.listSignUser.forEach((su) => {
                                        //su.status = su.status == '1'; // Trạng thái nhận
                                        su.is_type = parseInt(su.is_type);
                                        su.is_order = parseInt(su.is_order);
                                    });
                                }
                            }
                            else {
                                item.listSignUser = [];
                            }
                            item.IsLast = (item.daky || 0) + 1 == (item.soky || 0);
                            item.is_overdue = item.status == 2 ? (item.times_processing > item.times_processing_max ? true : false) : 
                                                ((item.SoNgayHan || 0) < 0 ? true : false);
                        });
                        listViewRequest.value = data[0];
                        options.value.is_func = listViewRequest.value.filter(x => x.is_func && (x.status == 1 || x.status == 0 || x.status == -1 || x.status == 3)).length > 0;
                        if (data[1] != null && data[1].length > 0) {
                            options.value.total = data[1][0].total;
                        }
                    } else {
                        listViewRequest.value = data[0];
                        options.value.total = 0;
                    }
                }
            }
            if (isFirst.value) isFirst.value = false;
            swal.close();
            if (options.value.loading) options.value.loading = false;
        })
        .catch((error) => {
            debugger
            swal.close();
            if (options.value.loading) options.value.loading = false;
            if (error && error.status === 401) {
                swal.fire({
                    text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                    confirmButtonText: "OK",
                });
                store.commit("gologout");
                return;
            } else {
                swal.fire({
                    title: "Thông báo!",
                    text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
                    icon: "error",
                    confirmButtonText: "OK",
                });
                return;
            }
        });
}
const showDetailRequest = ref(false);
const selectedRequestID = ref();
const openViewRequest = (dataRequest) => {
    forceRerenderDetail();
    showDetailRequest.value = true;
    selectedRequestID.value = dataRequest.request_id;
};
const cpnDetailRequest = ref(0);
const forceRerenderDetail = () => {
    cpnDetailRequest.value += 1;
};
emitter.on("SideBarRequest", (obj) => {
    showDetailRequest.value = false;
    selectedRequestID.value = null;
});
const hideall = () => {
    // if (idRequestLoaded.value != null) {
    //     router.push({ name: "Request_Request", params: {} }).then(() => {
    //         //router.go(0);
    //     });
    // } else {
    //     listRequest(true);
    // }
};
const activeTab = (tab, type) => {
    if (tab != null) {
        options.value.tab = tab;
        options.value.type_tab = type;
        loadData(true);
    }
};
const refresh = () => {
    options.value.search = '';
    loadData(true);
}
onMounted(() => {
    loadData(true);
    return {
        loadData,
    };
});
emitter.on("psbRequest", (obj) => {
    PositionSideBar.value = obj;
});
const closeSildeBar = () => {
    emitter.emit("SideBarListRequest", false);
};
const MaxMin = (m) => {
    PositionSideBar1.value = m;
    emitter.emit("psbRequest1", m);
};
</script>
<template>
    <div class="sidebar-request" style="height: 98vh;">
        <div class="d-grid formgrid m-1">
            <div class="flex justify-content-center align-items-center">
                <!-- <Toolbar class="w-full custoolbar">
                    <template #start>
                        <span class="p-input-icon-left">
                            <i class="pi pi-search" />
                            <InputText style="min-width: 300px" type="text" spellcheck="false" v-model="options.search"
                                placeholder="Tìm kiếm" @keyup.enter="loadData(true)" />
                        </span>
                    </template>
                    <template #end>
                        <Button @click="refresh()" label="Tải lại" icon="pi pi-refresh" class="mr-2" />
                    </template>
                </Toolbar> -->
                <div class="row col-12 flex justify-content-center px-0 mx-0 format-center">
                    <div class="col-1 p-0 m-0 flex">
                        <Button icon="pi pi-times" class="p-button-rounded p-button-text" v-tooltip="{ value: 'Đóng' }"
                            @click="closeSildeBar()" />
                        <Button icon="pi pi-window-maximize" class="p-button-rounded p-button-text"
                            v-tooltip="{ value: 'Phóng to' }" @click="MaxMin('full')" v-if="PositionSideBar1 == 'right'" />
                        <Button icon="pi pi-window-minimize" class="p-button-rounded p-button-text"
                            v-tooltip="{ value: 'Thu nhỏ' }" @click="MaxMin('right')" v-if="PositionSideBar1 == 'full'" />
                    </div>
                    <div class="col-11 p-0 pl-3 m-0 flex" style="justify-content: space-between;">
                        <div class="flex" style="align-items: center;">
                            <span class="p-input-icon-left">
                            <i class="pi pi-search" />
                            <InputText style="min-width: 300px" type="text" spellcheck="false" v-model="options.search"
                                placeholder="Tìm kiếm" @keyup.enter="loadData(true)" />
                        </span>
                        </div>
                        <div class="flex" style="align-items: center;">
                            <Button @click="refresh()" label="Tải lại" icon="pi pi-refresh" class="mr-2" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="d-grid formgrid m-1">
            <ul class="list-type-request">
                <li @click="activeTab(100)" class="zoom" :class="options.tab == 100 ? 'active' : ''"
                    style="background-color: #74b9ff;text-align: center;">Tất cả ({{
                        props.counts.count_so_phieu_de_xuat }})</li>
                <li @click="activeTab(2,1)" class="zoom"
                    :class="(options.tab == 2 && options.type_tab == 1) ? 'active' : ''"
                    style="background-color: #33c9dc;text-align: center;">Chờ tôi duyệt ({{
                        props.counts.count_cho_toi_duyet }})</li>
                <li @click="activeTab(3)" class="zoom" :class="options.tab == 3 ? 'active' : ''"
                    style="background-color: #2196f3;text-align: center;">Đã phê duyệt ({{
                        props.counts.count_toi_da_duyet }})</li>
                <li @click="activeTab(2,2)" class="zoom"
                    :class="(options.tab == 2 && options.type_tab == 2) ? 'active' : ''"
                    style="background-color: #33c9dc;text-align: center;">Chờ duyệt ({{
                        props.counts.count_dang_cho_duyet }})</li>
                <li @click="activeTab(4)" class="zoom" :class="options.tab == 4 ? 'active' : ''"
                    style="background-color: #f17ac7;text-align: center;">Đã từ chối ({{
                        props.counts.count_da_tu_choi }})</li>
                <li @click="activeTab(16)" class="zoom" :class="options.tab == 16 ? 'active' : ''"
                    style="background-color: blueviolet;text-align: center;">Tôi theo dõi ({{
                        props.counts.count_toi_theo_doi }})</li>
                <li @click="activeTab(5)" class="zoom" :class="options.tab == 5 ? 'active' : ''"
                    style="background-color: #6dd230;text-align: center;">Hoàn thành ({{
                        props.counts.count_da_hoan_thanh }})</li>
                <li @click="activeTab(6)" class="zoom" :class="options.tab == 6 ? 'active' : ''"
                    style="background-color: #ff8b4e;text-align: center;">Đã quá hạn ({{
                        props.counts.count_da_qua_han }})</li>
                <li @click="activeTab(101)" class="zoom" :class="options.tab == 101 ? 'active' : ''"
                    style="background-color: #f5b041;text-align: center;">Xử lý đánh giá ({{
                        props.counts.count_so_phieu_danh_gia }})</li>
            </ul>
        </div>
        <div class="d-grid formgrid m-1">
            <DataTable class="table-request-data" :value="listViewRequest" :scrollable="true" selectionMode="single"
                dataKey="request_id" scrollHeight="calc(100vh - 170px)" :rowHover="true" v-model:selection="selectedNodes">
                <Column field="request_code" header="Mã số" headerStyle="text-align:center;max-width:150px;height:45px"
                    bodyStyle="text-align:center;max-width:150px;padding:0 0.5rem !important;"
                    class="align-items-center justify-content-center text-center">
                    <template #body="slotProps">
                        <div class="flex"
                            :class="slotProps.data.status != 2 && slotProps.data.is_overdue && slotProps.data.deadline && slotProps.data.SoNgayHan <= 24 ? 'overdue-request' : ''"
                            style="flex-direction: column;height:100%;justify-content: center;"
                            @click="openViewRequest(slotProps.data)">
                            <span style="word-break: break-all;">
                                {{ slotProps.data.request_code }}
                            </span>
                            <div class="mt-2" v-if="slotProps.data.status_processing == 3">
                                <Rating class="star-rating-custom" v-model="slotProps.data.evaluated_score"
                                    v-tooltip.top="{ value: 'Ngày đánh giá: <br/>' + (slotProps.data.evaluated_date ? moment(new Date(slotProps.data.evaluated_date)).format('HH:mm DD/MM/yyyy') : ''), escape: true }"
                                    :stars="5" :cancel="false" :readonly="true" />
                            </div>
                        </div>
                    </template>
                </Column>
                <Column field="request_name" header="Tên đề xuất" headerStyle="text-align:left;height:45px"
                    bodyStyle="text-align:left;" class="align-items-center">
                    <template #body="slotProps">
                        <div class="flex" style="flex-direction: column;height: 100%; width: 100%; justify-content: center;"
                            @click="openViewRequest(slotProps.data)">
                            <div class="flex" style="align-items: baseline;">
                                <span class="uutien mr-2" :class="'uutien' + (slotProps.data.is_security || 0)"
                                    v-if="slotProps.data.is_security > 0">
                                    {{ slotProps.data.priority_level == 1 ? 'Gấp' : 'Rất gấp' }}
                                </span>
                                <span class="uutien mr-2" v-tooltip.top="'Bảo mật'" v-if="slotProps.data.is_security">
                                    <i class="pi pi-flag" style="color:red;"></i>
                                </span>
                                <span class="card-nhom flex text-left"
                                    style="padding:0.25rem 0.5rem;background-color: #ff8b4e;color: #fff;margin-right: 0.5rem !important;"
                                    v-if="slotProps.data.is_change_process">
                                    Quy trình động
                                </span>
                                <span class="flex font-bold" style="flex:1;"
                                    :style="slotProps.data.is_overdue ? 'color:red !important;' : ''">
                                    {{ slotProps.data.request_name }}
                                </span>
                            </div>
                            <div class="flex mt-2" style="align-items: center;">
                                <span style="padding:0.25rem 0.5rem;background-color: antiquewhite;white-space: normal;"
                                    class="card-nhom flex text-left">
                                    {{ slotProps.data.last_name + " - " + (slotProps.data.request_form_name || 'Đề xuất trực tiếp') }}
                                </span>
                                <span class="flex ml-2 span-note-request" v-if="slotProps.data.is_overdue" v-tooltip.top="{
                                    value: 'Hạn xử lý: ' +
                                        (slotProps.data.deadline ? moment(new Date(slotProps.data.deadline)).format('HH:mm DD/MM/yyyy') : ''), escape: true
                                }" style="color:red;">
                                    <i style="font-size:12px" class="pi pi-clock"></i>
                                    <span class="pl-1">
                                        ({{ slotProps.data.SoNgayHan || 0 }}h)
                                    </span>
                                </span>
                                <span class="flex ml-2 span-note-request"
                                    v-if="!slotProps.data.is_overdue && slotProps.data.deadline" v-tooltip.top="{
                                        value: 'Hạn xử lý: ' +
                                            (slotProps.data.deadline ? moment(new Date(slotProps.data.deadline)).format('HH:mm DD/MM/yyyy') : ''), escape: true
                                    }" :style="slotProps.data.SoNgayHan <= 24 ? 'color:orange' : 'color:#333'">
                                    <i style="font-size:12px" class="pi pi-clock"></i>
                                    <span class="pl-1">
                                        ({{ slotProps.data.SoNgayHan || 0 }}h)
                                    </span>
                                </span>
                                <span class="flex ml-2 span-note-request" v-if="(slotProps.data.SoFile > 0)">
                                    <i style="font-size:12px" class="pi pi-paperclip"></i>
                                    <span class="pl-1" v-tooltip.top="'File đính kèm'">
                                        ({{ slotProps.data.SoFile || 0 }})
                                    </span>
                                </span>
                                <span class="flex ml-2 span-note-request" v-if="(slotProps.data.SoChat > 0)">
                                    <i v-tooltip.top="'Số thảo luận'" style="font-size:12px" class="pi pi-comment"></i>
                                    <span class="pl-1" v-tooltip.top="'Số thảo luận'">
                                        ({{ slotProps.data.SoChat || 0 }})</span>
                                    <Badge class="flex ml-1"
                                        style="align-items: center;justify-content: center;width:1.25rem;height:1.25rem;"
                                        v-tooltip.top="'Số thảo luận chưa đọc'" :value="slotProps.data.SoSendhub || 0"
                                        v-if="(slotProps.data.SoSendhub > 0)"></Badge>
                                </span>
                                <span class="flex ml-2 span-note-request" style="color:#2196f3 !important;"
                                    v-if="(slotProps.data.Stask > 0)">
                                    <font-awesome-icon icon="fa-solid fa-list-check"
                                        style="font-size: 12px; display: block; color: #2196f3" />
                                    <span class="pl-1">
                                        ({{ (slotProps.data.StaskFinish || 0) + "/" + (slotProps.data.Stask || 0) }})
                                    </span>
                                    <span v-tooltip.top="'Tiến độ công việc: ' + slotProps.data.StaskTiendo + '%'"
                                        class="radialProgressBar pl-1" :class="'progress-' + slotProps.data.bgtiendo">
                                        <span class="overlay" style="color:#000">
                                            {{ slotProps.data.StaskTiendo || 0 }}%
                                        </span>
                                    </span>
                                </span>
                            </div>
                        </div>
                    </template>
                </Column>
                <Column field="created_date" header="Ngày tạo" headerStyle="text-align:center;max-width:140px;height:45px"
                    bodyStyle="text-align:center;max-width:140px;"
                    class="align-items-center justify-content-center text-center">
                    <template #body="slotProps">
                        <div class="flex" style="flex-direction: column;">
                            <span>{{ moment(slotProps.data.created_date).format("DD/MM/YYYY") }}</span>
                            <div class="mt-2" style="vertical-align:middle;width:140px"
                                v-if="slotProps.data.Tiendo && slotProps.data.Tiendo > 0">
                                <ProgressBar class="progress-bar-custom" :class="renderColorProgress(slotProps.data.Tiendo)"
                                    :value="(slotProps.data.Tiendo || 0)"></ProgressBar>
                            </div>
                        </div>
                    </template>
                </Column>
                <Column field="created_by" header="Người tạo" headerStyle="text-align:center;max-width:120px;height:45px"
                    bodyStyle="text-align:center;max-width:120px;"
                    class="align-items-center justify-content-center text-center">
                    <template #body="slotProps">
                        <div class="relative">
                            <Avatar v-bind:label="slotProps.data.avatar
                                ? ''
                                : (slotProps.data.last_name ?? '').substring(0, 1).toUpperCase()
                                " v-bind:image="slotProps.data.avatar
        ? basedomainURL + slotProps.data.avatar
        : basedomainURL + '/Portals/Image/noimg.jpg'
        " v-tooltip.top="{ value: (slotProps.data.full_name + '<br/>' + slotProps.data.position_name + '<br/>' + slotProps.data.department_name), escape: true }"
                                style="color: #ffffff; width: 2rem; height: 2rem; font-size: 1rem !important;"
                                :style="{ background: bgColor[slotProps.index % 7], }" size="xlarge" shape="circle"
                                class="border-radius" />
                        </div>
                    </template>
                </Column>
                <Column field="listSignUser" header="Người duyệt"
                    headerStyle="text-align:center;max-width:250px;height:45px"
                    bodyStyle="text-align:center;max-width:250px;"
                    class="align-items-center justify-content-center text-center">
                    <template #body="slotProps">
                        <div class="relative">
                            <AvatarGroup>
                                <Avatar v-for="(us, idxUser) in slotProps.data.listSignUser" :key="idxUser"
                                    v-bind:label="us.avatar ? '' : (us.last_name ?? '').substring(0, 1)" v-bind:image="us.avatar
                                        ? basedomainURL + us.avatar
                                        : basedomainURL + '/Portals/Image/noimg.jpg'
                                        "
                                    v-tooltip.top="{ value: (us.full_name + '<br/>' + us.position_name + '<br/>' + us.department_name), escape: true }"
                                    style="background-color: #2196f3; color: #ffffff; width: 2rem; height: 2rem; font-size: 1rem !important;"
                                    :style="{ background: bgColor[idxUser % 7], }" class="text-avatar" size="xlarge"
                                    shape="circle">
                                    <template #body="">
                                        <span v-if="us.status" class="is-sign">
                                            <font-awesome-icon icon="fa-solid fa-circle-check"
                                                style="font-size: 16px; display: block; color: #f4b400" />
                                        </span>
                                    </template>
                                </Avatar>
                            </AvatarGroup>
                        </div>
                    </template>
                </Column>
                <Column field="status" header="Trạng thái" headerStyle="text-align:center;max-width:180px;height:45px"
                    bodyStyle="text-align:center;max-width:180px;"
                    class="align-items-center justify-content-center text-center">
                    <template #body="slotProps">
                        <Chip class="status_request" :class="slotProps.data.objStatus.class || ''"
                            v-bind:label="slotProps.data.objStatus.text || ''" style="border-radius: 5px !important;" />
                    </template>
                </Column>
                <template #empty>
                    <div class="align-items-center justify-content-center p-4 text-center m-auto" :style="{
                        display: 'flex',
                        width: '100%',
                        height: 'calc(100vh - 230px)',
                        backgroundColor: '#fff',
                    }">
                        <div v-if="options.total == 0">
                            <img src="../../../../assets/background/nodata.png" height="144" />
                            <h3 class="m-1">Không có dữ liệu</h3>
                        </div>
                    </div>
                </template>
            </DataTable>
        </div>
    </div>
    <Sidebar class="sidebar-request" v-model:visible="showDetailRequest" :position="'right'" :style="{
        width: PositionSideBar == 'right' ? (widthWindow > 1800 ? '65vw' : '75vw') : '100%',
        'min-height': '100vh !important',
    }" :baseZindex="1000" :autoZIndex="true" :showCloseIcon="false" @hide="hideall()">
        <DetailedRequest :isShow="showDetailRequest" :id="selectedRequestID" :key="cpnDetailRequest"
            :listStatusRequests="listStatusRequests">
        </DetailedRequest>
    </Sidebar>
</template>
<style>
.p-sidebar-content {
    overflow: hidden !important;
}
</style>
<style lang="scss" scoped>
.list-type-request {
    display: flex;
    justify-content: center;
}

.list-type-request li {
    list-style: none;
    margin: 10px;
    padding: 10px 10px;
    color: #fff;
}

.zoom {
    cursor: pointer;
    border-radius: 0.25rem;
    box-shadow: 0 2px 4px rgb(0 0 0 / 23%);
    transition: transform 0.3s !important;
    display: flex;
    align-items: center;
}

.active {
    border: 5px solid #f4d03f !important;
    transform: scale(0.9) !important;
    /* box-shadow: 10px 10px 15px rgb(0 0 0 / 23%) !important; */
    cursor: pointer !important;
}

.zoom:hover {
    transform: scale(0.9) !important;
    /* box-shadow: 10px 10px 15px rgb(0 0 0 / 23%) !important; */
    cursor: pointer !important;
}

.sidebar-request .p-sidebar-content {
    overflow: hidden !important;
}
</style>