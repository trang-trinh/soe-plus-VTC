<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../util/function.js";
//import moment from "moment";
//Khai báo
import SidebarViewRequest from "../request/category/component/sidebar_view_request.vue";

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
const chartDataPie1 = ref(
    {
        labels: ['Đã hoàn thành', 'Chưa hoàn thành'],
        datasets: [
            {
                data: [1, 2],
                backgroundColor: ['#aaa', '#ccc'],
                hoverBackgroundColor: [],
            },
        ],
    }
);
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
const chartDataPie2 = ref(
    {
        labels: ['Đã hoàn thành', 'Chưa hoàn thành'],
        datasets: [
            {
                data: [1, 2],
                backgroundColor: ['#aaa', '#ccc'],
                hoverBackgroundColor: [],
            },
        ],
    }
);
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
const list_cho_duyets = ref();
const isShowSidebar = ref(false);
const showDetailRequest = ref(false);
const PositionSideBar = ref('right');
const goToView = () => {
    isShowSidebar.value = true;
}
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
                                        <div class="col-4 md:col-4" @click="goToView()">
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
                                        <div class="col-4 md:col-4" @click="goToView()">
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
                                        <div class="col-4 md:col-4" @click="goToView()">
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
                                        <div class="col-4 md:col-4" @click="goToView()">
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
                                        <div class="col-4 md:col-4" @click="goToView()">
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
                                        <div class="col-4 md:col-4" @click="goToView()">
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
                                        <div class="col-4 md:col-4" @click="goToView()">
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
                                        <div class="col-4 md:col-4" @click="goToView()">
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
                                        <div class="col-4 md:col-4" @click="goToView()">
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
                                            Đề xuất đã hoàn thành mới nhất
                                        </div>
                                        <div class="content-request p-10">
                                            <div style="width: 100%; height: 100%; overflow: hidden auto"
                                                class="scroll-outer">
                                                <div class="scroll-inner">
                                                    <ul>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                    </ul>
                                                </div>
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
                                                class="scroll-outer">
                                                <div class="scroll-inner">
                                                    <ul>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                    </ul>
                                                </div>
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
                                                class="scroll-outer">
                                                <div class="scroll-inner">
                                                    <ul>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                        <li>
                                                            <div>
                                                                <div class="col-12 md:col-12">
                                                                    <span
                                                                        style="padding: 2px 10px;border: 1px solid #0078d4;background-color: #0078d4;color: #fff;border-radius: 5px;">NP04.23.0539</span>
                                                                    <label style="margin-left: 5px;font-weight: bold;">Xin
                                                                        nghỉ
                                                                        phép</label>
                                                                </div>
                                                                <div class="col-12 md:col-12"
                                                                    style="display: flex;align-items: center;padding-top: 0px !important;">
                                                                    <div
                                                                        style="display: flex; flex: 1;font-size: 12px;color: #98a9bc;">
                                                                        <span>Ngày lập: 10:17 02/02/2023</span>
                                                                    </div>
                                                                    <span
                                                                        style=" background-color: #ff8b4e;font-size: 10px;padding: 5px 5px;color: #fff;border-radius: 5px;height: fit-content;">Quá
                                                                        hạn 0 giờ</span>
                                                                </div>
                                                            </div>
                                                            <div>

                                                            </div>
                                                        </li>
                                                    </ul>
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
    </div>

    <Sidebar class="sidebar-request" v-model:visible="isShowSidebar" :position="PositionSideBar" :style="{
            width:
                PositionSideBar == 'right'
                    ? width1 > 1800
                        ? ' 60vw'
                        : '80vw'
                    : '100vw',
            'height': '101vh !important',
        }" :showCloseIcon="false">
        <SidebarViewRequest :isShow="showDetailRequest" :turn="0">
        </SidebarViewRequest>
    </Sidebar>
</template>
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