<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { useRoute } from "vue-router";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../util/function.js";
import moment from "moment";
//Khai báo
import SidebarViewRequest from "../request/category/component/sidebar_view_request.vue";
import DetailedRequest from "../request/component_request/detail_request.vue";

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
const config = {
    headers: { Authorization: `Bearer ${store.getters.token}` },
};
const options = ref({
    loading: true,
    tab: null,
    type_tab: null,
});
const bgColor = ref([
    "#F8E69A",
    "#AFDFCF",
    "#F4B2A3",
    "#9A97EC",
    "#CAE2B0",
    "#8BCFFB",
    "#CCADD7",
]);
const listStatusRequests = ref([
    { id: 0, text: "Mới lập", class: "rqlap" },
    { id: 1, text: "Chờ duyệt", class: "rqchoduyet" },
    { id: 2, text: "Chấp thuận", class: "rqchapthuan" },
    { id: -2, text: "Từ chối", class: "rqtuchoi" },
    { id: -1, text: "Hủy", class: "rqhuy" },
    { id: 3, text: "Thu hồi", class: "rqthuhoi" },
    { id: -3, text: "Xóa", class: "rqxoa" }
]);
const counts = ref({
    count_cho_toi_duyet: 0,
    count_so_phieu_danh_gia: 0,
    count_dang_cho_duyet: 0,
    count_toi_da_duyet: 0,
    count_so_phieu_de_xuat: 0,
    count_da_tu_choi: 0,
    count_toi_theo_doi: 0,
    count_da_hoan_thanh: 0,
    count_da_qua_han: 0,
});
const chartDataPie1 = ref();
const optionsChartPie1 = {
    responsive: true,
    plugins: {
        title: {
            display: true,
            position: "bottom",
            text: "Gửi đến tôi",
        },
        legend: {
            position: "bottom",
        },
    },
};
const chartDataPie2 = ref();
const optionsChartPie2 = {
    responsive: true,
    plugins: {
        title: {
            display: true,
            position: "bottom",
            text: "Tôi gửi đii",
        },
        legend: {
            position: "bottom",
        },
    },
};
const list_cho_duyets = ref([]);
const list_hoan_thanhs = ref([]);
const list_qua_hans = ref([]);
const showSidebarRequest = ref(false);
// const PositionSideBar = ref('right');
// component request
const goToView = (tab, type) => {
    showSidebarRequest.value = true;
    options.value.tab = tab;
    options.value.type_tab = type;
}
emitter.on("SideBarListRequest", (obj) => {
    showSidebarRequest.value = false;
});
const goRouter = () => {}
emitter.on("SideBarRequest", (obj) => {
    showDetailRequest.value = false;
    selectedRequestID.value = null;
});
const PositionSideBar = ref('right');
const PositionSideBar1 = ref('right');
emitter.on("psbRequest", (obj) => {
    PositionSideBar.value = obj;
});
emitter.on("psbRequest1", (obj) => {
    PositionSideBar1.value = obj;
});
const cpnDetailRequest = ref(0);
const forceRerenderDetail = () => {
    cpnDetailRequest.value += 1;
};
const showDetailRequest = ref(false);
const selectedRequestID = ref();
const widthWindow = ref(window.screen.availWidth);
const idRequestLoaded = ref(route.params.id);
const hideall = () => {
    if (idRequestLoaded.value != null) {
        router.push({ name: "Request_Request", params: {} }).then(() => {
            //router.go(0);
        });
    } else {
        loadRequestDashBoardCount();
        loadRequestDashBoardList();
    }
};
const openViewRequest = (dataRequest) => {
    forceRerenderDetail();
    showDetailRequest.value = true;
    selectedRequestID.value = dataRequest.request_id;
};

const loadRequestDashBoardCount = () => {
    options.value.loading = true;
    axios
        .post(
            baseURL + "/api/request/getData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "request_dashboard_count",
                        par: [
                            { par: "user_id", va: store.getters.user.user_id },
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
                counts.value.count_cho_toi_duyet = data[0][0].count;
                counts.value.count_so_phieu_danh_gia = data[0][7].count;
                counts.value.count_dang_cho_duyet = data[1][0].count;
                counts.value.count_toi_da_duyet = data[0][1].count;
                counts.value.count_so_phieu_de_xuat = data[0][6].count;
                counts.value.count_da_tu_choi = data[0][2].count;
                counts.value.count_toi_theo_doi = data[0][5].count;
                counts.value.count_da_hoan_thanh = data[0][3].count;
                counts.value.count_da_qua_han = data[0][4].count;
                let data1 = 0;
                let data2 = 0;
                let data3 = 0;
                let data4 = 0;
                data[0].forEach((d) => {
                    if (d.status_value == 3) {
                        data1 += d.count;
                    } else if (d.status_value == 2) {
                        data2 += d.count;
                    }
                })
                data[1].forEach((d) => {
                    if (d.status_value == 2) {
                        data3 += d.count;
                    } else if (d.status_value == 3) {
                        data4 += d.count;
                    }
                })
                let chart1 = {
                    labels: [],
                    datasets: [
                        {
                            data: [],
                            backgroundColor: [],
                            hoverBackgroundColor: [],
                        },
                    ],
                };
                let chart2 = {
                    labels: [],
                    datasets: [
                        {
                            data: [],
                            backgroundColor: [],
                            hoverBackgroundColor: [],
                        },
                    ],
                };
                chart1.labels.push('Đã phê duyệt');
                chart1.datasets[0].data.push(data1);
                chart1.datasets[0].backgroundColor.push('#6dd230');
                chart1.datasets[0].hoverBackgroundColor.push('#6dd230');
                chart1.labels.push('Đang chờ duyệt');
                chart1.datasets[0].data.push(data2);
                chart1.datasets[0].backgroundColor.push('#74b9ff');
                chart1.datasets[0].hoverBackgroundColor.push('#74b9ff');

                chartDataPie1.value = chart1;

                chart2.labels.push('Đã phê duyệt');
                chart2.datasets[0].data.push(data3);
                chart2.datasets[0].backgroundColor.push('#6dd230');
                chart2.datasets[0].hoverBackgroundColor.push('#6dd230');
                chart2.labels.push('Đang chờ duyệt');
                chart2.datasets[0].data.push(data4);
                chart2.datasets[0].backgroundColor.push('#74b9ff');
                chart2.datasets[0].hoverBackgroundColor.push('#74b9ff');

                chartDataPie2.value = chart2;
            }
            swal.close();
            if (options.value.loading) options.value.loading = false;
        })
        .catch((error) => {
            toast.error("Tải dữ liệu không thành công!");
            options.value.loading = false;
            if (error && error.status === 401) {
                swal.fire({
                    text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                    confirmButtonText: "OK",
                });
                store.commit("gologout");
            }
        });
}
const loadRequestDashBoardList = () => {
    options.value.loading = true;
    axios
        .post(
            baseURL + "/api/request/getData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "request_dashboard_list",
                        par: [
                            { par: "user_id", va: store.getters.user.user_id },
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
                list_cho_duyets.value = data[0].filter(x => x.type_request == 1);
                list_hoan_thanhs.value = data[0].filter(x => x.type_request == 2);
                list_qua_hans.value = data[0].filter(x => x.type_request == 3);
                //fake data
                // list_cho_duyets.value = [
                //     {
                //         request_code: '123', request_id: '0A28D5C3352D480CB6FBD0708FF47E94', request_name: 'Đơn xin nghỉ', content: 'Nội dung', created_date: new Date(), Members: [
                //             { user_id: '123', full_name: 'Trịnh Văn Tráng', last_name: 'Tráng', avatar: null },
                //             { user_id: '1234', full_name: 'Trịnh Văn A', last_name: 'A', avatar: null },
                //             { user_id: '12345', full_name: 'Trịnh Văn B', last_name: 'B', avatar: null },
                //             { user_id: '123456', full_name: 'Trịnh Văn C', last_name: 'C', avatar: null },
                //             { user_id: '1234567', full_name: 'Trịnh Văn D', last_name: 'D', avatar: null },
                //             { user_id: '12345678', full_name: 'Trịnh Văn E', last_name: 'E', avatar: null },
                //             { user_id: '123456789', full_name: 'Trịnh Văn F', last_name: 'F', avatar: null }
                //         ]
                //     }
                // ];
                list_cho_duyets.value.forEach((element) => {
                    element.MemberShows = [];
                    if (element.Members != null) {
                        element.Members = JSON.parse(element.Members);
                        if (element.Members.length > 5) {
                            element.MemberShows = element.Members.slice(0, 5);
                        } else {
                            element.MemberShows = [...element.Members];
                        }
                    }
                    else {
                        element.Members = [];
                    }
                })
                // list_hoan_thanhs.value = [
                //     {
                //         request_code: '123', request_id: '0A28D5C3352D480CB6FBD0708FF47E94', request_name: 'Đơn xin nghỉ', content: 'Nội dung', created_date: new Date(), Members: [
                //             { user_id: '123', full_name: 'Trịnh Văn Tráng', last_name: 'Tráng', avatar: null },
                //             { user_id: '1234', full_name: 'Trịnh Văn A', last_name: 'A', avatar: null },
                //             { user_id: '12345', full_name: 'Trịnh Văn B', last_name: 'B', avatar: null },
                //             { user_id: '123456', full_name: 'Trịnh Văn C', last_name: 'C', avatar: null },
                //             { user_id: '1234567', full_name: 'Trịnh Văn D', last_name: 'D', avatar: null },
                //             { user_id: '12345678', full_name: 'Trịnh Văn E', last_name: 'E', avatar: null },
                //             { user_id: '123456789', full_name: 'Trịnh Văn F', last_name: 'F', avatar: null }
                //         ]
                //     }
                // ];
                list_hoan_thanhs.value.forEach((element) => {
                    element.MemberShows = [];
                    if (element.Members != null) {
                        element.Members = JSON.parse(element.Members);
                        if (element.Members.length > 5) {
                            element.MemberShows = element.Members.slice(0, 5);
                        } else {
                            element.MemberShows = [...element.Members];
                        }
                    }
                    else {
                        element.Members = [];
                    }
                })
                // list_qua_hans.value = [
                //     {
                //         request_code: '123', request_id: '0A28D5C3352D480CB6FBD0708FF47E94', request_name: 'Đơn xin nghỉ', content: 'Nội dung', created_date: new Date(), Members: [
                //             { user_id: '123', full_name: 'Trịnh Văn Tráng', last_name: 'Tráng', avatar: null },
                //             { user_id: '1234', full_name: 'Trịnh Văn A', last_name: 'A', avatar: null },
                //             { user_id: '12345', full_name: 'Trịnh Văn B', last_name: 'B', avatar: null },
                //             { user_id: '123456', full_name: 'Trịnh Văn C', last_name: 'C', avatar: null },
                //             { user_id: '1234567', full_name: 'Trịnh Văn D', last_name: 'D', avatar: null },
                //             { user_id: '12345678', full_name: 'Trịnh Văn E', last_name: 'E', avatar: null },
                //             { user_id: '123456789', full_name: 'Trịnh Văn F', last_name: 'F', avatar: null }
                //         ]
                //     }
                // ];
                list_qua_hans.value.forEach((element) => {
                    element.title_quahan = "Quá hạn 2 giờ";
                    element.MemberShows = [];
                    if (element.Members != null) {
                        element.Members = JSON.parse(element.Members);
                        if (element.Members.length > 5) {
                            element.MemberShows = element.Members.slice(0, 5);
                        } else {
                            element.MemberShows = [...element.Members];
                        }
                    }
                    else {
                        element.Members = [];
                    }
                })
                //end
            }
            swal.close();
            if (options.value.loading) options.value.loading = false;
        })
        .catch((error) => {
            toast.error("Tải dữ liệu không thành công!");
            options.value.loading = false;
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
    loadRequestDashBoardCount();
    loadRequestDashBoardList();
    return {};
});
// ---
</script>
<template>
    <div style="min-height: calc(100vh - 50px);max-height: calc(100vh - 50px);overflow-x: hidden;overflow-y: scroll;">
        <div class="surface-100 dashboard">
            <div class="d-grid formgrid m-1" style="height: 100%;">
                <div class="col-6 md:col-6 p-0" style="height: 100%;">
                    <div class="d-grid formgrid" style="height: 100%;">
                        <div class="col-12 md:col-12 p-0">
                            <div class="card m-1" style="height: 98%;">
                                <div class="card-body p-0" style="height: max-content">
                                    <div class="d-grid formgrid" style="height: 100%;">
                                        <div class="col-4 md:col-4" @click="goToView(2,1)">
                                            <div class="card zoom" style="background-color: #33c9dc; color: #fff;"
                                                @click="goRouter('calendarenact')">
                                                <div class="card-body">
                                                    <div class="format-grid-center">
                                                        <h1 class="my-2" style="word-break: break-all">
                                                            {{ counts.count_cho_toi_duyet }}
                                                        </h1>
                                                        <h4 class="m-0">Chờ tôi duyệt</h4>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-4 md:col-4" @click="goToView(101)">
                                            <div class="card zoom" style="background-color: #f5b041; color: #fff"
                                                @click="goRouter('docreceive')">
                                                <div class="card-body">
                                                    <div class="format-grid-center">
                                                        <h1 class="my-2" style="word-break: break-all">
                                                            {{ counts.count_so_phieu_danh_gia }}
                                                        </h1>
                                                        <h4 class="m-0">Tổng số phiếu đánh giá</h4>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-4 md:col-4" @click="goToView(2,2)">
                                            <div class="card zoom" style="background-color: #33c9dc; color: #fff"
                                                @click="goRouter('taskmain')">
                                                <div class="card-body">
                                                    <div class="format-grid-center">
                                                        <h1 class="my-2" style="word-break: break-all">
                                                            {{ counts.count_dang_cho_duyet }}
                                                        </h1>
                                                        <h4 class="m-0">Đang chờ duyệt</h4>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-4 md:col-4" @click="goToView(3)">
                                            <div class="card zoom" style="background-color: #2196f3; color: #fff"
                                                @click="goRouter('bookingmeal')">
                                                <div class="card-body">
                                                    <div class="format-grid-center">
                                                        <h1 class="my-2" style="word-break: break-all">
                                                            {{ counts.count_toi_da_duyet }}
                                                        </h1>
                                                        <h4 class="m-0">Tôi đã duyệt</h4>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-4 md:col-4" @click="goToView(100)">
                                            <div class="card zoom" style="background-color: #74b9ff; color: #fff"
                                                @click="goRouter('chat_message')">
                                                <div class="card-body">
                                                    <div class="format-grid-center">
                                                        <h1 class="my-2" style="word-break: break-all">
                                                            {{ counts.count_so_phieu_de_xuat }}
                                                        </h1>
                                                        <h4 class="m-0">Tổng số phiếu đề xuất</h4>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-4 md:col-4" @click="goToView(4)">
                                            <div class="card zoom" style="background-color: #f17ac7; color: #fff"
                                                @click="goRouter('lawmain')">
                                                <div class="card-body">
                                                    <div class="format-grid-center">
                                                        <h1 class="my-2" style="word-break: break-all">
                                                            {{ counts.count_da_tu_choi }}
                                                        </h1>
                                                        <h4 class="m-0">Đã từ chối</h4>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-4 md:col-4" @click="goToView(16)">
                                            <div class="card zoom" style="background-color: blueviolet; color: #fff"
                                                @click="goRouter('lawmain')">
                                                <div class="card-body">
                                                    <div class="format-grid-center">
                                                        <h1 class="my-2" style="word-break: break-all">
                                                            {{ counts.count_toi_theo_doi }}
                                                        </h1>
                                                        <h4 class="m-0">Tôi theo dõi</h4>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-4 md:col-4" @click="goToView(5)">
                                            <div class="card zoom" style="background-color: #6dd230; color: #fff"
                                                @click="goRouter('lawmain')">
                                                <div class="card-body">
                                                    <div class="format-grid-center">
                                                        <h1 class="my-2" style="word-break: break-all">
                                                            {{ counts.count_da_hoan_thanh }}
                                                        </h1>
                                                        <h4 class="m-0">Đã hoàn thành</h4>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-4 md:col-4" @click="goToView(6)">
                                            <div class="card zoom" style="background-color: #ff8b4e; color: #fff"
                                                @click="goRouter('lawmain')">
                                                <div class="card-body">
                                                    <div class="format-grid-center">
                                                        <h1 class="my-2" style="word-break: break-all">
                                                            {{ counts.count_da_qua_han }}
                                                        </h1>
                                                        <h4 class="m-0">Đã quá hạn</h4>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-3 md:col-3 p-0" style="height: 100%;">
                    <div class="d-grid formgrid" style="height: 100%;">
                        <div class="col-12 md:col-12 p-0" style="height: 100%;">
                            <div class="formgrid m-1 p-0" style="height: 100%;">
                                <div style="height: 98%;background-color: #fff;">
                                    <div class="formgrid p-0" style="height: 100%;background-color: #fff;">
                                        <div class="col-12 md:col-12 title-dashbord">
                                            Gửi đến tôi
                                        </div>
                                        <div class="col-12">
                                            <Chart type="doughnut" :height="300" :width="300" :data="chartDataPie1"
                                                :options="optionsChartPie1" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-3 md:col-3 p-0" style="height: 100%;">
                    <div class="d-grid formgrid" style="height: 100%;">
                        <div class="col-12 md:col-12 p-0" style="height: 100%;">
                            <div class="formgrid m-1 p-0" style="height: 100%;">
                                <div style="height: 98%;background-color: #fff;">
                                    <div class="formgrid p-0" style="height: 100%;background-color: #fff;">
                                        <div class="col-12 md:col-12 title-dashbord">
                                            Tôi gửi đi
                                        </div>
                                        <div class="col-12">
                                            <Chart type="doughnut" :height="300" :width="300" :data="chartDataPie2"
                                                :options="optionsChartPie2" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="surface-100 dashboard">
            <div class="d-grid formgrid m-1" style="height: 100%;">
                <div class="col-4 md:col-4 p-0" style="height: 100%;">
                    <div class="d-grid formgrid" style="height: 100%;">
                        <div class="col-12 md:col-12 p-0" style="height: 100%;">
                            <div class="formgrid m-1 p-0" style="height: 100%;">
                                <div style="height: 100%;background-color: #fff;">
                                    <div class="formgrid p-0" style="height: 100%;background-color: #fff;">
                                        <div class="col-12 md:col-12 title-dashbord">
                                            <i class="pi pi-list"></i>
                                            Đề xuất chờ duyệt mới nhất
                                        </div>
                                        <div class="content-request p-10">
                                            <div style="width: 100%; height: 100%; overflow: hidden auto"
                                                class="scroll-outer" v-if="list_cho_duyets.length > 0">
                                                <div class="scroll-inner">
                                                    <ul>
                                                        <li v-for="l in list_cho_duyets">
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;font-size:13px;">
                                                                        {{ l.request_code }}
                                                                    </span>
                                                                    <label style="margin-left: 5px;font-weight: bold;line-height: 1.5;">
                                                                        {{ l.request_name }}
                                                                    </label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: {{ moment(new
                                                                            Date(l.created_date)).format("HH:mm DD/MM/YYYY")
                                                                        }}</span>
                                                                    </div>
                                                                    <!-- <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span> -->
                                                                </div>
                                                            </div>
                                                            <div v-if="l.MemberShows.length > 0" style="margin-left: 15px;">
                                                                <AvatarGroup>
                                                                    <div v-for="(value, index) in l.MemberShows"
                                                                        :key="index">
                                                                        <div>
                                                                            <Avatar v-tooltip.bottom="{
                                                                                    value: value.full_name +
                                                                                        (value.department_name ? ('<br/>' + (value.department_name || '')) : '') +
                                                                                        (value.position_name ? ('<br/>' + (value.position_name || '')) : ''),
                                                                                    escape: true,
                                                                                }" 
                                                                                v-bind:label="value.avatar ? '' : (value.last_name ?? '').substring(0, 1)" 
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
                                                                                class="cursor-pointer" size="xlarge" shape="circle" 
                                                                            />
                                                                        </div>
                                                                    </div>
                                                                    <Avatar v-if="(l.Members.length - l.MemberShows.length) > 0" 
                                                                        :label="'+' + (l.Members.length - l.MemberShows.length) + ''" 
                                                                        class="cursor-pointer" shape="circle" 
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
                                                        </li>
                                                    </ul>
                                                </div>
                                            </div>
                                            <div v-if="list_cho_duyets.length == 0"
                                                style="height: 100%;display: flex;align-items: center;justify-content: center;flex-direction: column;">
                                                <img src="../../assets/background/nodata.png" height="144" />
                                                <h4 class="m-1">Chưa có phiếu đề xuất nào chờ duyệt</h4>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-4 md:col-4 p-0" style="height: 100%;">
                    <div class="d-grid formgrid" style="height: 100%;">
                        <div class="col-12 md:col-12 p-0" style="height: 100%;">
                            <div class="formgrid m-1 p-0" style="height: 100%;">
                                <div style="height: 100%;background-color: #fff;">
                                    <div class="formgrid p-0" style="height: 100%;background-color: #fff;">
                                        <div class="col-12 md:col-12 title-dashbord">
                                            <i class="pi pi-list"></i>
                                            Đề xuất đã hoàn thành mới nhất
                                        </div>
                                        <div class="content-request p-10">
                                            <div style="width: 100%; height: 100%; overflow: hidden auto"
                                                class="scroll-outer" v-if="list_hoan_thanhs.length > 0">
                                                <div class="scroll-inner">
                                                    <ul>
                                                        <li v-for="l in list_hoan_thanhs">
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;font-size:13px;">
                                                                        {{ l.request_code }}
                                                                    </span>
                                                                    <label style="margin-left: 5px;font-weight: bold;line-height: 1.5;">
                                                                        {{ l.request_name }}
                                                                    </label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: {{ moment(new
                                                                            Date(l.created_date)).format("HH:mm DD/MM/YYYY")
                                                                        }}</span>
                                                                    </div>
                                                                    <!-- <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span> -->
                                                                </div>
                                                            </div>
                                                            <div v-if="l.MemberShows.length > 0"
                                                                style="margin-left: 15px;display: flex;align-items: center;">
                                                                <AvatarGroup>
                                                                    <div v-for="(value, index) in l.MemberShows"
                                                                        :key="index">
                                                                        <div>
                                                                            <Avatar v-tooltip.bottom="{
                                                                                    value: value.full_name +
                                                                                        (value.department_name ? ('<br/>' + (value.department_name || '')) : '') +
                                                                                        (value.position_name ? ('<br/>' + (value.position_name || '')) : ''),
                                                                                    escape: true,
                                                                                }" 
                                                                                v-bind:label="value.avatar ? '' : (value.last_name ?? '').substring(0, 1)" 
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
                                                                                class="cursor-pointer" size="xlarge" shape="circle" 
                                                                            />
                                                                        </div>
                                                                    </div>
                                                                    <Avatar v-if="(l.Members.length - l.MemberShows.length) > 0" 
                                                                        :label="'+' + (l.Members.length - l.MemberShows.length) + ''" 
                                                                        class="cursor-pointer" shape="circle" 
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
                                                                <div style="width: 100px; margin-left: 10px;"
                                                                    class="progressbar-hoanthanh">
                                                                    <ProgressBar :value="100" />
                                                                </div>
                                                            </div>
                                                        </li>
                                                    </ul>
                                                </div>
                                            </div>
                                            <div v-if="list_hoan_thanhs.length == 0"
                                                style="height: 100%;display: flex;align-items: center;justify-content: center;flex-direction: column;">
                                                <img src="../../assets/background/nodata.png" height="144" />
                                                <h4 class="m-1">Chưa có phiếu đề xuất nào hoàn thành</h4>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-4 md:col-4 p-0" style="height: 100%;">
                    <div class="d-grid formgrid" style="height: 100%;">
                        <div class="col-12 md:col-12 p-0" style="height: 100%;">
                            <div class="formgrid m-1 p-0" style="height: 100%;">
                                <div style="height: 100%;background-color: #fff;">
                                    <div class="formgrid p-0" style="height: 100%;background-color: #fff;">
                                        <div class="col-12 md:col-12 title-dashbord">
                                            <i class="pi pi-list"></i>
                                            Đề xuất quá hạn mới nhất
                                        </div>
                                        <div class="content-request p-10">
                                            <div style="width: 100%; height: 100%; overflow: hidden auto"
                                                class="scroll-outer" v-if="list_qua_hans.length > 0">
                                                <div class="scroll-inner">
                                                    <ul>
                                                        <li v-for="l in list_qua_hans" @click="openViewRequest(l)">
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;font-size:13px;">
                                                                        {{ l.request_code }}
                                                                    </span>
                                                                    <label style="margin-left: 5px;font-weight: bold;line-height: 1.5;">
                                                                        {{ l.request_name }}
                                                                    </label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: {{ moment(new Date(l.created_date)).format("HH:mm DD/MM/YYYY") }}</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">
                                                                        {{ l.title_quahan }}
                                                                    </span>
                                                                </div>
                                                            </div>
                                                            <div v-if="l.MemberShows.length > 0"
                                                                style="margin-left: 15px;display: flex;align-items: center;">
                                                                <AvatarGroup>
                                                                    <div v-for="(value, index) in l.MemberShows"
                                                                        :key="index">
                                                                        <div>
                                                                            <Avatar v-tooltip.bottom="{
                                                                                    value: value.full_name +
                                                                                        (value.department_name ? ('<br/>' + (value.department_name || '')) : '') +
                                                                                        (value.position_name ? ('<br/>' + (value.position_name || '')) : ''),
                                                                                    escape: true,
                                                                                }" 
                                                                                v-bind:label="value.avatar ? '' : (value.last_name ?? '').substring(0, 1)" 
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
                                                                                class="cursor-pointer" size="xlarge" shape="circle" 
                                                                            />
                                                                        </div>
                                                                    </div>
                                                                    <Avatar v-if="(l.Members.length - l.MemberShows.length) > 0" 
                                                                        :label="'+' + (l.Members.length - l.MemberShows.length) + ''" 
                                                                        class="cursor-pointer" shape="circle" 
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
                                                        </li>
                                                    </ul>
                                                </div>
                                            </div>
                                            <div v-if="list_qua_hans.length == 0"
                                                style="height: 100%;display: flex;align-items: center;justify-content: center;flex-direction: column;">
                                                <img src="../../assets/background/nodata.png" height="144" />
                                                <h4 class="m-1">Chưa có phiếu đề xuất nào quá hạn</h4>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <Sidebar class="sidebar-request" v-model:visible="showSidebarRequest" :position="PositionSideBar1" 
        :style="{
            width:
                PositionSideBar1 == 'right'
                    ? width1 > 1800
                        ? ' 60vw'
                        : '80vw'
                    : '100vw',
            'height': '101vh !important',
        }" 
        :showCloseIcon="false"
    >
        <SidebarViewRequest :isShow="showSidebarRequest" :turn="0" :counts="counts" :tab="options.tab" :type_tab="options.type_tab">
        </SidebarViewRequest>
    </Sidebar>
    <Sidebar class="sidebar-request" v-model:visible="showDetailRequest" :position="'right'" :style="{
        width: PositionSideBar == 'right' ? (widthWindow > 1800 ? '65vw' : '75vw') : '100%',
        'min-height': '100vh !important',
    }" :showCloseIcon="false" @hide="hideall()">
        <DetailedRequest :isShow="showDetailRequest" :id="selectedRequestID" :key="cpnDetailRequest"
            :listStatusRequests="listStatusRequests">
        </DetailedRequest>
    </Sidebar>
</template>
<style>
.progressbar-hoanthanh .p-progressbar .p-progressbar-value {
    background-color: #6dd230 !important;
}

.progressbar-hoanthanh .p-progressbar {
    height: 1.2rem !important;
    font-size: 11px;
}
</style>
<style scoped>
.dashboard {
    /* overflow: auto;
    max-height: calc(100vh - 360px);
    min-height: calc(100vh - 360px); */
    height: 50%;
}

.d-grid {
    display: flex;
    flex-wrap: wrap;
}

.d-lang-table {
    height: calc(100vh - 190px);
    overflow-y: auto;
}

.format-flex-center {
    display: flex;
    align-items: center;
    justify-content: center;
    text-align: center;
}

.format-grid-center {
    display: grid;
    align-items: center;
    justify-content: center;
    text-align: center;
}

.form-group {
    display: grid;
    margin-bottom: 1rem;
}

.form-group>label {
    margin-bottom: 0.5rem;
}

.ip36 {
    width: 100%;
}

.p-ulchip {
    margin: 0;
    margin-top: 0.5rem;
    padding: 0;
    list-style: none;
}

.p-lichip {
    float: left;
}

.p-multiselect-label {
    height: 100%;
    display: flex;
    align-items: center;
}

.dashboard .card {
    border: none;
    border-radius: 0;
    position: relative;
    display: -webkit-box;
    display: -ms-flexbox;
    display: flex;
    -webkit-box-orient: vertical;
    -webkit-box-direction: normal;
    -ms-flex-direction: column;
    flex-direction: column;
    min-width: 0;
    word-wrap: break-word;
    background-color: #fff;
    background-clip: border-box;
    height: 100%;
}

.dashboard .card-header {
    -webkit-box-flex: 1;
    -ms-flex: 1 1 auto;
    flex: 1 1 auto;
    padding: 1rem;
    overflow: hidden;
    border-bottom: solid 1px rgba(0, 0, 0, 0.1);
    font-size: 15px;
    font-weight: bold;
    color: #005a9e;
}

.dashboard .card-body {
    -webkit-box-flex: 1;
    -ms-flex: 1 1 auto;
    flex: 1 1 auto;
    padding: 1rem;
    overflow: hidden;
}

.dashboard .card h4 {
    text-transform: uppercase;
    ;
}

.zoom {
    cursor: pointer;
    border-radius: 0.25rem;
    box-shadow: 0 2px 4px rgb(0 0 0 / 23%);
    transition: transform 0.3s !important;
}

.zoom:hover {
    transform: scale(0.9) !important;
    box-shadow: 10px 10px 15px rgb(0 0 0 / 23%) !important;
    cursor: pointer !important;
}

.title-dashbord {
    background-color: #e6e6e6;
    height: 8%;
    font-weight: bold;
}

.title-dashbord {
    background-color: #e6e6e6;
    height: 8%;
    font-weight: bold;
}

.content-request {
    height: 92%;
    /* overflow-x: scroll; */
}

.content-request ul {
    padding: 0px;
    margin: 0px;
    height: 100%;
}

.scroll-outer {
    visibility: hidden;
}

.scroll-inner,
.scroll-outer:hover,
.scroll-outer:focus {
    visibility: visible;
}

.content-request ul li {
    list-style: none;
    padding: 5px;
}

.content-request ul li:hover {
    cursor: pointer;
    background-color: beige;
}
</style>
