<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { useRoute } from "vue-router";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";

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
    search: "",
    pageNo: 1,
    pageSize: 20,
    total: 0,
    is_team: false,
});

const datalists = ref();
const count_all_request = ref(0);
const count_all_request_moitao = ref(0);
const count_all_request_choduyet = ref(0);
const count_all_request_hoanthanh = ref(0);
const count_all_request_quahan_choduyet = ref(0);
const count_all_request_quahan_daduyet = ref(0);
const count_all_request_quahan = ref(0);

const loadData = (rf) => {
    options.value.loading = true;
    axios
        .post(
            baseURL + "/api/request/getData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "report_request_statistical_list",
                        par: [
                            { par: "user_id", va: store.getters.user.user_id },
                            { par: "search", va: options.value.search },
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
                datalists.value = data[0];
                count_all_request.value = data[1][0].count_all_request;
                count_all_request_choduyet.value = data[3][0].count_all_request_choduyet;
                count_all_request_moitao.value = data[2][0].count_all_request_moitao;
                count_all_request_hoanthanh.value = data[4][0].count_all_request_hoanthanh;
                count_all_request_quahan_choduyet.value = data[5][0].count_all_request_quahan_choduyet;
                count_all_request_quahan_daduyet.value = data[6][0].count_all_request_quahan_daduyet;
                datalists.value.forEach((d) => {
                    d.is_show = d.lv == 0 ? true : false;
                })
            }
            swal.close();
            if (options.value.loading) options.value.loading = false;
        })
        .catch((error) => {
            toast.error("Tải dữ liệu không thành công!");
            options.value.loading = false;
            if (error && error.status === 401) {
                swal.fire({
                    text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                    confirmButtonText: "OK",
                });
                store.commit("gologout");
            }
        });
};
const refresh = () => {
    options.value = ({
        loading: true,
        search: "",
        pageNo: 1,
        pageSize: 20,
        total: 0,
        is_team: false,
    });
    loadData(true);
}
onMounted(() => {
    loadData(true);
    return {
        datalists,
        loadData,
    };
});

const ChangeIsShowTeam = () => {
    datalists.value.forEach((d) => {
        if (d.lv == 1) {
            d.is_show = options.value.is_team ? true : false;
        }
    })
}
</script>
<template>
    <div class="surface-100 p-2">
        <Toolbar class="outline-none surface-0 border-none">
            <template #start>
                <div class="flex" style="width: 100%;">
                    <span class="p-input-icon-left"
                        style="position: relative;width: 50%;display: flex;align-items: center;">
                        <InputSwitch @change="ChangeIsShowTeam()" class="col-6" style="position: absolute;"
                            v-model="options.is_team" />
                        <label class="col-3 text-left p-0" style="position: absolute; left: 50px">Hiện team</label>
                    </span>
                    <span class="p-input-icon-left" style="width: 50%;">
                        <i class="pi pi-search" />
                        <InputText type="text" spellcheck="false" v-model="options.search" style="width: 75%;"
                            placeholder="Tìm kiếm" v-on:keyup.enter="loadData(true)" />
                    </span>
                </div>
            </template>
            <template #end>
                <Button @click="refresh()" class="p-button-outlined p-button-secondary mr-2" icon="pi pi-file"
                    label="Export" />
                <Button @click="refresh()" class="p-button-outlined p-button-secondary mr-2" icon="pi pi-refresh"
                    label="Tải lại" />
            </template>
        </Toolbar>
        <div class="flex"
            style="width: 100%;height: 50px;justify-content: center;align-items: center;font-weight: bold;font-size: 14px;">
            BÁO CÁO THỐNG KÊ TỔNG HỢP ĐỀ XUẤT
        </div>
        <div
            style="overflow: scroll;max-height: calc(100vh - 124px);min-height: calc(100vh - 124px);background-color: #fff;">
            <table cellspacing=0 id="table-bc" class="table table-condensed table-hover tbpad" style="width: 100%;">
                <thead style="position: sticky; z-index: 6; top:0">
                    <tr>
                        <th class="text-center" rowspan="3" width="200">Form</th>
                        <th class="text-center" rowspan="2" width="100">Tổng số yêu cầu</th>
                        <th class="text-center" rowspan="2" width="100">Mới lập</th>
                        <th class="text-center" rowspan="2" width="100">Chờ duyệt</th>
                        <th class="text-center" rowspan="2" width="100">Hoàn thành</th>
                        <th class="text-center" colspan="3">Quá hạn</th>
                    </tr>
                    <tr>
                        <th class="text-center" width="100">Quá hạn chờ duyệt</th>
                        <th class="text-center" width="100">Quá hạn đã duyệt</th>
                        <th class="text-center" width="100">Tổng cộng</th>
                    </tr>
                    <tr>
                        <th class="text-center" width="100">{{ count_all_request }}</th>
                        <th class="text-center" width="100">{{ count_all_request_moitao }}</th>
                        <th class="text-center" width="100">{{ count_all_request_choduyet }}</th>
                        <th class="text-center" width="100">{{ count_all_request_hoanthanh }}</th>
                        <th class="text-center" width="100">{{ count_all_request_quahan_choduyet }}</th>
                        <th class="text-center" width="100">{{ count_all_request_quahan_daduyet }}</th>
                        <th class="text-center" width="100">{{ count_all_request_quahan_choduyet +
                            count_all_request_quahan_daduyet }}</th>
                    </tr>
                </thead>
                <tbody v-for="(d, index) in datalists" :key="index">
                    <tr class="item-hover" v-if="d.is_show == true">
                        <td align="left" :class="d.lv == 0 ? 'td-form-0' : 'td-form-1'">{{ d.table_name }}</td>
                        <td align="center">{{ d.count_all_request }}</td>
                        <td align="center">{{ d.count_all_request_moitao }}</td>
                        <td align="center">{{ d.count_all_request_choduyet }}</td>
                        <td align="center">{{ d.count_all_request_hoanthanh }}</td>
                        <td align="center">{{ d.count_all_request_quahan_choduyet }}</td>
                        <td align="center">{{ d.count_all_request_quahan_daduyet }}</td>
                        <td align="center">{{ d.count_all_request_quahan_choduyet + d.count_all_request_quahan_daduyet }}
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</template>
<style lang="scss" scoped>
::v-deep(.p-toolbar) {
    .p-toolbar-group-left {
        width: 70%;
    }
}
</style>
<style scoped>
.item-hover:hover {
    background-color: #f0f8ff !important;
}

.bg-group {
    background-color: rgb(222, 230, 240) !important;
}

.bg-stt {
    background-color: #e6e6e6;
}

.active-item {
    background-color: #ffd95f;
}

.active-border {
    border: 2px solid #008eff !important
}

.table {
    margin-bottom: 0px !important;
}


.left-sticky1 {
    position: sticky;
    z-index: 5;
    vertical-align: middle !important;
}

.left-1 {
    left: -1px;
}

.left-2 {
    left: 49px;
}

.left-3 {
    left: 149px;
}

.left-4 {
    left: 299px;
}

.btn.btn-secondary:hover {
    background-color: #e6f0f8 !important;
    color: #2f90d1 !important;
}

table {
    border: 0.3px solid rgba(0, 0, 0, .3) !important;
}

tr td {
    word-break: break-word;
    vertical-align: middle !important;
    cursor: pointer;
}

table th {
    background-color: #8BCFFB !important;
}

table .td-form-0 {
    background-color: #f6ddcc !important;
}

table .td-form-1 {
    background-color: #d5f5e3 !important;
}

table th,
table td {
    border: 0.3px solid rgba(0, 0, 0, .3) !important;
}
</style>
<style scoped>
/* #table-bc{
max-height: calc(100vh - 110px);
overflow-y: auto;
overflow-x: scroll;

} */
th,
td {
    background: #fff;
    padding: 0.6rem;
}

.row-parent {
    font-weight: bold;
    margin-left: 0.5em;
}

.row-child {
    cursor: pointer;
    margin-left: 1.5em;
}

.row-child:hover {
    color: #0078d4;
}

.toolbar-filter {
    border: unset;
    outline: unset;
    background-color: #fff;
    padding-bottom: 0px;
}

td.bg-group>b {
    position: sticky;
    left: 10px;
}
</style>
<style lang="scss" scoped>::v-deep(.p-rowgroup-header) {
    td {
        flex: 1 1 0 !important;
    }
}

::v-deep(.p-datatable-emptymessage) {
    td {
        flex: 1 1 0 !important;
    }
}</style>