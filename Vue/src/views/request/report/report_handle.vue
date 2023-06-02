<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { useRoute } from "vue-router";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
import moment from "moment";

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
const group = ref();
const options = ref({
    loading: true,
    search: '',
    pageNo: 0,
    pageSize: 20,
    total: 0,
    is_team: false,
    active_group: 3,
    group_by: false,
});
const menuGroupByButs = ref();
const itemGroupBys = ref([
    {
        label: "Không group",
        value: 0,
        active: false,
        command: () => {
            ChangeGroupBy(0);
        },
    },
    {
        label: "Loại đề xuất",
        value: 1,
        active: false,
        command: () => {
            ChangeGroupBy(1);
        },
    },
    {
        label: "Team",
        value: 2,
        active: false,
        command: () => {
            ChangeGroupBy(2);
        },
    },
    {
        label: "Loại đề xuất & Team",
        value: 3,
        active: true,
        command: () => {
            ChangeGroupBy(3);
        },
    },
]);

const datalists = ref([]);
const status = ref([
    { value: "-3", label: "Đã xóa", count: 0 },
    { value: "-2", label: "Trả lại", count: 0 },
    { value: "-1", label: "Hủy", count: 0 },
    { value: "0", label: "Mới tạo", count: 0 },
    { value: "1", label: "Đang trình", count: 0 },
    { value: "2", label: "Hoàn thành", count: 0 },
    { value: "3", label: "Thu hồi", count: 0 },
]);
const loadMainData = () => {
    swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
    axios
        .post(
            baseURL + "/api/DictionaryProc/getData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "report_request_handle_list",
                        par: [
                            { par: "user_id", va: store.getters.user.user_id }, 
                            { par: "optionView", va: options.value.active_group },
                            { par: "search", va: options.value.search },
                        ],
                    }),
                    SecretKey,
                    cryoptojs,
                ).toString(),
            },
            config,
        )
        .then((response) => {
            let data = JSON.parse(response.data.data)[0];
            data.forEach((x, i) => {
                if (x.request_team_id == null) {
                    x.request_team_id = -1;
                    x.request_team_name = "Cá nhân";
                }
                if (x.request_form_id == null) {
                    x.request_form_id = -1;
                    x.request_form_name = "Đề xuất khác";
                }
                x.STT = options.value.pageNo * options.value.pageSize + (i + 1);
                x.status_display = status.value.filter(
                    (z) => z.value == x.status,
                )[0].label;
                x.sign_users = ["Nguyễn Thị Hương", "Trần Cao Duy", "Dữ liệu Fake"];
                x.TienDo = 50;
            });
            let dataTeam = [];
            if (options.value.active_group == 3) {
                data.forEach((x, i) => {
                    if (dataTeam.length == 0) {
                        let k = {
                            request_form_id: x.request_form_id,
                            request_form_name: x.request_form_name,
                            request_team_data: [],
                        };
                        dataTeam.push(k);
                    } else {
                        let t = dataTeam.filter(
                            (z) => z.request_form_id == x.request_form_id,
                        );
                        if (t.length == 0) {
                            let k = {
                                request_form_id: x.request_form_id,
                                request_form_name: x.request_form_name,
                                request_team_data: [],
                            };
                            dataTeam.push(k);
                        }
                    }
                });
                dataTeam.forEach((element) => {
                    data.forEach((k) => {
                        if (element.request_team_data.length == 0) {
                            let container = {
                                request_team_id: k.request_team_id,
                                request_team_name: k.request_team_name,
                                team_data: [],
                            };
                            element.request_team_data.push(container);
                        } else {
                            let filter = element.request_team_data.filter(
                                (u) => u.request_team_id == k.request_team_id,
                            );
                            if (filter.length == 0) {
                                let container = {
                                    request_team_id: k.request_team_id,
                                    request_team_name: k.request_team_name,
                                    team_data: [],
                                };
                                element.request_team_data.push(container);
                            }
                        }
                    });
                });

                dataTeam.forEach((z, i) => {
                    z.request_team_data.forEach((x, k) => {
                        let datafilter = data.filter(
                            (d) =>
                                x.request_team_id == d.request_team_id &&
                                z.request_form_id == d.request_form_id,
                        );
                        datafilter.forEach((x, i) => {
                            x.STT = options.value.pageNo * options.value.pageSize + (i + 1);
                        });
                        x.team_data = datafilter;
                    });
                });
                datalists.value = dataTeam;
            } else datalists.value = data;
            swal.close();
        })
        .catch((error) => {
            if (error && error.status === 401) {
                swal.fire({
                    title: "Thông báo",
                    text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                    icon: "error",
                    confirmButtonText: "OK",
                });
            }
        });
};
const listStatusRequests = ref([
    { id: 0, text: "Mới lập", value: 0, class: "rqlap", bg: "rgb(116, 185, 255)", count: 0 },
    { id: 1, text: "Chờ duyệt", value: 0, class: "rqchoduyet", bg: "rgb(51, 201, 220)", count: 0 },
    { id: 2, text: "Hoàn thành", value: 0, classclass: "rqchapthuan", bg: "rgb(109, 210, 48)", count: 0 },
    { id: -2, text: "Từ chối", value: 0, class: "rqtuchoi", bg: "rgb(241, 122, 199)", count: 0 },
    { id: -1, text: "Quá hạn", value: 0, class: "rqhuy", bg: "rgb(255, 139, 78)", count: 0 },
    { id: 3, text: "Xử lý đánh giá", value: 0, class: "rqthuhoi", bg: "rgb(245, 176, 65)", count: 0 },
]);
// const LoadCount = () => {
//     swal.fire({
//         width: 110,
//         didOpen: () => {
//             swal.showLoading();
//         },
//     });

//     axios
//         .post(
//             baseURL + "/api/DictionaryProc/getData",
//             {
//                 str: encr(
//                     JSON.stringify({
//                         proc: "request_team_count",
//                         par: [{ par: "user_id", va: store.getters.user.user_id }],
//                     }),
//                     SecretKey,
//                     cryoptojs,
//                 ).toString(),
//             },
//             config,
//         )
//         .then((response) => {
//             let data = JSON.parse(response.data.data)[0];

//             listStatusRequests.value[0].value = data[0].allreq;
//             listStatusRequests.value[1].value = data[0].waiting;
//             listStatusRequests.value[2].value = data[0].completed;
//             listStatusRequests.value[3].value = data[0].refused;
//             listStatusRequests.value[4].value = data[0].finished;
//             listStatusRequests.value[5].value = data[0].expired;
//             swal.close();
//         })
//         .catch((error) => {
//             if (error && error.status === 401) {
//                 swal.fire({
//                     title: "Thông báo",
//                     text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
//                     icon: "error",
//                     confirmButtonText: "OK",
//                 });
//             }
//         });
// };
const toggleGroupBy = (event) => {
    menuGroupByButs.value.toggle(event);
};
const ChangeGroupBy = (value) => {
    options.value.active_group = value;
    if (value == 1) {
        group.value = 'request_team_name';
    } else if (value == 2) {
        group.value = 'request_form_name';
    } else {
        group.value = null;
    }
    itemGroupBys.value.forEach((i) => {
        if (i.value == value) {
            i.active = true;
        } else {
            i.active = false;
        }
    });
    menuGroupByButs.value.toggle();
    loadMainData();
}
const refresh = () => {
    options.value = ({
        loading: true,
        search: '',
        pageNo: 0,
        pageSize: 20,
        total: 0,
        is_team: false,
        active_group: 3,
        group_by: false,
    });
    loadMainData();
}
onMounted(() => {
    // LoadCount();
    loadMainData();
    return {
        datalists,
        loadMainData,
    };
});
</script>
<template>
    <div class="surface-100 p-2">
        <Toolbar class="outline-none surface-0 border-none">
            <template #start>
                <div class="flex" style="width: 100%;">
                    <span class="p-input-icon-left"
                        style="position: relative;width: 50%;display: flex;align-items: center;">

                    </span>
                    <span class="p-input-icon-left" style="width: 50%;">
                        <i class="pi pi-search" />
                        <InputText type="text" spellcheck="false" v-model="options.search" style="width: 75%;"
                            placeholder="Tìm kiếm" v-on:keyup.enter="loadMainData()" />
                    </span>
                </div>
            </template>
            <template #end>
                <ul id="toolbar_right" style="padding: 0px; margin: 0px; display: flex">
                    <li @click="toggleGroupBy($event)" :class="{ active: options.group_by }" aria-haspopup="true"
                        aria-controls="overlay_Export">
                        <a> Nhóm dữ liệu
                            <i class="pi pi-angle-down"></i></a>
                    </li>
                </ul>
                <Menu id="request_group_by" :model="itemGroupBys" ref="menuGroupByButs" :popup="true">
                    <template #item="{ item }">
                        <a @click="ChangeGroupBy(item.value)" :class="{ active: item.active }" style="display: block;">{{
                            item.label }}</a>
                    </template>
                </Menu>
                <Button @click="refresh()" class="p-button-outlined p-button-secondary mr-2" icon="pi pi-file"
                    label="Export" />
                <Button @click="refresh()" class="p-button-outlined p-button-secondary mr-2" icon="pi pi-refresh"
                    label="Tải lại" />
            </template>
        </Toolbar>
        <div class="flex"
            style="width: 100%;height: 50px;justify-content: center;align-items: center;font-weight: bold;font-size: 14px;background-color: #fff;">
            BÁO CÁO XỬ LÝ ĐỀ XUẤT
        </div>
        <div class="d-lang-table">
            <DataTable v-if="options.active_group != 3" :value="datalists" :paginator="true" :rows="options.pageSize"
                :scrollable="true" scrollHeight="flex" :rowHover="true" :showGridlines="true"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
                :rowsPerPageOptions="[20, 100, 200, 300, 500]" responsiveLayout="scroll" style="
              overflow-y: auto;
              overflow-x: hidden;
              min-height: calc(100vh - 13rem);
              max-height: calc(100vh - 13rem);
            " @row-click="openViewRequest" dataKey="request_id" :rowGroupMode="group != null ? 'subheader' : ''"
                :groupRowsBy="group != null ? group : ''" @page="onPage">
                <template #empty>
                    <div class="w-full align-items-center justify-content-center p-4 text-center">
                        <img src="/src/assets/background/nodata.png" height="144" />
                        <h3 class="m-1">Không có dữ liệu</h3>
                    </div>
                </template>
                <template #groupheader="slotProps">
                    <div class="flex align-items-center gap-2">
                        <span v-if="options.active_group == 2">{{
                            slotProps.data.request_form_name
                        }}</span>
                        <span v-if="options.active_group == 1">{{
                            slotProps.data.request_team_name
                        }}</span>
                    </div>
                </template>
                <Column header="STT" field="STT" class="align-items-center justify-content-center max-w-3rem"></Column>
                <Column header="Mã đề xuất" field="request_code"
                    headerClass="align-items-center justify-content-center text-center max-w-8rem "
                    bodyClass="align-items-center justify-content-center  text-center max-w-8rem word-break-break-all">
                </Column>
                <Column header="Tên đề xuất" field="request_name" headerClass="align-items-center justify-content-center ">
                </Column>
                <Column header="Người lập" field="created_by_full_name"
                    headerClass="align-items-center justify-content-center max-w-10rem "
                    bodyClass="align-items-center justify-content-center text-center max-w-10rem word-break-break-word">
                </Column>
                <Column header="Ngày lập" field="created_date" class="align-items-center justify-content-center max-w-8rem">
                    <template #body="data">
                        {{
                            moment(new Date(data.data.created_date)).format("DD/MM/YYYY")
                        }}
                    </template>
                </Column>
                <Column header="Trạng thái" field="status_display"
                    class="align-items-center justify-content-center text-center max-w-7rem"></Column>
                <Column header="Tiến độ" field="TienDo"
                    class="align-items-center justify-content-center text-center max-w-7rem">
                    <template #body="data">
                        <ProgressBar v-if="data.data.TienDo > 0" :value="data.data.TienDo || 0" :show-value="true"
                            class="w-full">
                        </ProgressBar>
                        <div v-else>0%</div>
                    </template>
                </Column>
                <Column header="Người duyệt" field="sign_users"
                    class="align-items-center justify-content-center text-center max-w-10rem">
                    <template #body="data">
                        <div class="block">
                            <span v-for="(user, index) in data.data.sign_users" :key="index">
                                - {{ user }}<br />
                            </span>
                        </div>
                    </template>
                </Column>
                <Column header="Ngày hoàn thành" field="completed_date"
                    class="align-items-center justify-content-center text-center max-w-10rem">
                    <template #body="data">
                        <div v-if="data.data.completed_date">
                            {{
                                moment(new Date(data.data.completed_date)).format(
                                    "DD/MM/YYYY",
                                )
                            }}
                        </div>
                    </template>
                </Column>
                <Column header="Số giờ" field="date"
                    class="align-items-center justify-content-center text-center max-w-5rem">
                    <template #body="data">
                        <div v-if="data.data.times_processing_max">
                            {{ data.data.SoNgayHan }} /
                            {{ data.data.times_processing_max }}
                        </div>
                    </template>
                </Column>
            </DataTable>
            <div v-else
                class="p-datatable p-component p-datatable-hoverable-rows p-datatable-scrollable p-datatable-scrollable-vertical p-datatable-flex-scrollable p-datatable-responsive-scroll p-datatable-gridlines"
                data-scrollselectors=".p-datatable-wrapper" style="
              overflow: hidden auto;
              min-height: calc(100vh - 13rem);
              max-height: calc(100vh - 13rem);
            ">
                <div class="p-datatable-wrapper">
                    <table role="table" class="p-datatable-table" v-if="datalists.length > 0">
                        <thead class="p-datatable-thead" role="rowgroup" style="z-index: 100;">
                            <tr role="row">
                                <th class="align-items-center justify-content-center max-w-3rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">STT</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center text-center max-w-8rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Mã đề xuất</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Tên đề xuất</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center max-w-10rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Người lập</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center max-w-8rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Ngày lập</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center text-center max-w-7rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Trạng thái</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center text-center max-w-7rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Tiến độ</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center text-center max-w-10rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Người duyệt</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center text-center max-w-10rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Ngày hoàn thành</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center text-center max-w-5rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Số giờ</span>
                                    </div>
                                </th>
                            </tr>
                        </thead>

                        <tbody class="p-datatable-tbody" role="rowgroup" v-for="(item, index) in datalists" :key="index"
                            v-if="options.active_group == 3">
                            <tr class="p-rowgroup-header" role="row" style="top: 42px">
                                <td colspan="9" class="p-2" style="background-color: #f6ddcc; font-size: 13px">
                                    <div class="flex align-items-center gap-2">
                                        <span>{{ item.request_form_name }}</span>
                                        <!-- <span>{{ item }}</span> -->
                                    </div>
                                </td>
                            </tr>
                            <div>
                                <table role="table" class="p-datatable-table">
                                    <tbody class="p-datatable-tbody" role="rowgroup"
                                        v-for="(team, teamIndex) in item.request_team_data" :key="teamIndex">
                                        <tr class="" role="row" style="top: 42px">
                                            <td colspan="9" style="background-color: #d5f5e3">
                                                <div class="flex align-items-center gap-2">
                                                    <span>{{ team.request_team_name }}</span>
                                                </div>
                                            </td>
                                        </tr>
                                        <tr class="" role="row" v-for="(request, reqIndex) in team.team_data"
                                            :key="reqIndex">
                                            <td class="align-items-center justify-content-center max-w-3rem" role="cell">
                                                {{ request.STT }}
                                            </td>
                                            <td class="align-items-center justify-content-center text-center max-w-8rem word-break-break-all"
                                                role="cell">
                                                {{ request.request_code }}
                                            </td>
                                            <td class="" role="cell">
                                                {{ request.request_name }}
                                            </td>
                                            <td class="align-items-center justify-content-center text-center max-w-10rem word-break-break-word"
                                                role="cell">
                                                {{ request.created_by_full_name }}
                                            </td>
                                            <td class="align-items-center justify-content-center max-w-8rem" role="cell">
                                                {{
                                                    moment(request.created_date).format("DD/MM/YYYY")
                                                }}
                                            </td>
                                            <td class="align-items-center justify-content-center text-center max-w-7rem"
                                                role="cell">
                                                {{ request.status_display }}
                                            </td>
                                            <td class="align-items-center justify-content-center text-center max-w-7rem"
                                                role="cell">
                                                <ProgressBar v-if="request.TienDo > 0" :value="request.TienDo || 0"
                                                    :show-value="true" class="w-full">
                                                </ProgressBar>
                                                <div v-else>0%</div>
                                            </td>
                                            <td class="align-items-center justify-content-center text-center max-w-10rem"
                                                role="cell">
                                                <div class="block">
                                                    <span v-for="(user, index) in request.sign_users" :key="index">
                                                        - {{ user }}<br />
                                                    </span>
                                                </div>
                                            </td>
                                            <td class="align-items-center justify-content-center text-center max-w-10rem"
                                                role="cell">
                                                <div v-if="request.completed_date">
                                                    {{
                                                        moment(new Date(request.completed_date)).format(
                                                            "DD/MM/YYYY",
                                                        )
                                                    }}
                                                </div>
                                            </td>
                                            <td class="align-items-center justify-content-center text-center max-w-5rem"
                                                role="cell">
                                                <div v-if="request.times_processing_max">
                                                    {{ request.SoNgayHan }} /
                                                    {{ request.times_processing_max }}
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </tbody>

                    </table>
                    <table v-else role="table" class="p-datatable-table">
                        <thead class="p-datatable-thead" role="rowgroup">
                            <tr role="row">
                                <th class="align-items-center justify-content-center max-w-3rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">STT</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center text-center max-w-8rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Mã đề xuất</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Tên đề xuất</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center max-w-10rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Người lập</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center max-w-8rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Ngày lập</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center text-center max-w-7rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Trạng thái</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center text-center max-w-7rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Tiến độ</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center text-center max-w-10rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Người duyệt</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center text-center max-w-10rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Ngày hoàn thành</span>
                                    </div>
                                </th>
                                <th class="align-items-center justify-content-center text-center max-w-5rem" role="cell">
                                    <div class="p-column-header-content">
                                        <span class="p-column-title">Số giờ</span>
                                    </div>
                                </th>
                            </tr>
                        </thead>

                        <tbody class="p-datatable-tbody" role="rowgroup">
                            <tr class="p-datatable-emptymessage" role="row">
                                <td colspan="10">
                                    <div data-v-e7fddb26=""
                                        class="w-full align-items-center justify-content-center p-4 text-center">
                                        <img data-v-e7fddb26="" src="/src/assets/background/nodata.png" height="144" />
                                        <h3 data-v-e7fddb26="" class="m-1">
                                            Không có dữ liệu
                                        </h3>
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <Paginator :rows="options.pageSize" :totalRecords="datalists.length" template="FirstPageLink PrevPageLink PageLinks NextPageLink
            LastPageLink RowsPerPageDropdown" :rowsPerPageOptions="[20, 100, 200, 300, 500]" @page="onPage" />
            </div>
        </div>
    </div>
</template>
<style>
#request_group_by {
    min-width: fit-content !important;
}

#request_group_by .p-menuitem {
    padding: 5px 10px;
}

#request_group_by .p-menuitem:hover {
    cursor: pointer;
    background-color: #e9ecef;
}

#request_group_by .p-menuitem .active {
    color: #2196f3 !important;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-toolbar) {
    .p-toolbar-group-left {
        width: 70%;
    }
}

.zoom {
    cursor: pointer;
    border-radius: 0.25rem;
    box-shadow: 0 2px 4px rgb(0 0 0 / 23%);
    transition: transform 0.3s !important;
    display: flex;
    align-items: center;
    justify-content: center;
    height: 40px;
}

.zoom:hover {
    transform: scale(0.9) !important;
    /* box-shadow: 10px 10px 15px rgb(0 0 0 / 23%) !important; */
    cursor: pointer !important;
}

#toolbar_right li {
    list-style: none;
    display: flex;
    justify-content: center;
    align-items: center;
    height: 30px;
    border: 1px solid #979797;
    border-radius: 4px;
    margin: 0px 5px 0px 0px;
}

#toolbar_right li a {
    padding: 0px 10px;
}

#toolbar_right li:hover {
    cursor: pointer;
    background-color: #2196f3 !important;
    border: 1px solid #5ca7e3 !important;
    color: #fff;
}
</style>
