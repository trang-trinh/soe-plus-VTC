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
    search: '',
    pageNo: 1,
    pageSize: 20,
    total: 0,
    is_team: false,
});

const datalists = ref();

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
                let data = JSON.parse(response.data.data)[0];
                datalists.value = data;
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
};

onMounted(() => {
    loadData(true);
    return {
        datalists,
        loadData,
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
        <div class="flex" style="width: 100%;height: 50px;justify-content: center;align-items: center;font-weight: bold;font-size: 14px;">
            BÁO CÁO ĐỀ XUẤT CÁ NHÂN
        </div>
        <div class="field col-12 md:col-12 algn-items-center flex p-0">
            <div class="col-3 text-left flex p-0" style="align-items:center;">
                
            </div>
        </div>
        <!-- <div class="flex" style="width: 100%;height: 50px;justify-content: center;align-items: center;font-weight: bold;font-size: 14px;background-color: #fff;">
            BÁO CÁO ĐỀ XUẤT CÁ NHÂN
        </div> -->
        <div class="d-lang-table">
            <DataTable class="table-request-data" :value="datalists" :scrollable="true" selectionMode="single"
                dataKey="request_id" scrollHeight="calc(100vh - 170px)" :rowHover="true" v-model:selection="selectedNodes">
                <Column field="request_form_name" header="Form" headerStyle="text-align:left;height:45px"
                    bodyStyle="text-align:left;" class="align-items-center">
                </Column>
                <Column field="created_date" header="Ngày tạo" headerStyle="text-align:center;max-width:140px;height:45px"
                    bodyStyle="text-align:center;max-width:140px;"
                    class="align-items-center justify-content-center text-center">
                </Column>
                <Column field="created_by" header="Người tạo" headerStyle="text-align:center;max-width:120px;height:45px"
                    bodyStyle="text-align:center;max-width:120px;"
                    class="align-items-center justify-content-center text-center">
                </Column>
                <Column field="listSignUser" header="Người duyệt"
                    headerStyle="text-align:center;max-width:250px;height:45px"
                    bodyStyle="text-align:center;max-width:250px;"
                    class="align-items-center justify-content-center text-center">
                </Column>
                <Column field="status" header="Trạng thái" headerStyle="text-align:center;max-width:180px;height:45px"
                    bodyStyle="text-align:center;max-width:180px;"
                    class="align-items-center justify-content-center text-center">
                </Column>
                <Column header="" headerStyle="text-align:center;max-width:50px"
                    bodyStyle="text-align:center;max-width:50px"
                    class="align-items-center justify-content-center text-center" v-if="options.is_func">
                </Column>
                <template #empty>
                    <div class="align-items-center justify-content-center p-4 text-center m-auto" :style="{
                        display: 'flex',
                        width: '100%',
                        height: 'calc(100vh - 230px)',
                        backgroundColor: '#fff',
                    }">
                        <div v-if="options.total == 0">
                            <img src="../../../assets/background/nodata.png" height="144" />
                            <h3 class="m-1">Không có dữ liệu</h3>
                        </div>
                    </div>
                </template>
            </DataTable>
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