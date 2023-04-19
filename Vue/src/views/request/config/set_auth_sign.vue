<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
//import moment from "moment";
import dialogAddFormRequest from "../category/component/dialog_add_form_request.vue";
import dialogShowFormTeam from "../category/component/dialog_show_form_team.vue";
import dialogSettingForm from "../category/component/dialog_setting_form.vue";
import procedureDetail from "../category/component/procedure_detail.vue";
//Khai báo

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

const datalists = ref([]);
const options = ref({
    loading: true,
    SearchText: '',
    loading: false,
});
const request_setting = ref({
    setting_id: "-1", is_num_device: null, is_verify_sign: null, is_skip_web: null, is_one_device: null, is_creator_cancel_request: null, is_creator_del_request: null,
    is_creator_tag_follow: null, is_creator_handle: null, is_approved_cancel_request: null, is_approved_del_request: null, is_approved_tag_follow: null, is_approved_handle: null, organization_id: null
});
const listCompanys = ref([]);
const headerSelectCompany = ref();
const displaySelectCompany = ref(false);
const selectedCompanys = ref();
const openSelectCompany = () => {
    axios
        .post(
            baseUrlCheck + "/api/request/getData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "request_setting_get_list_company_select",
                        par: [
                            { par: "user_id", va: store.state.user.user_id },
                        ],
                    }),
                    SecretKey,
                    cryoptojs
                ).toString(),
            },
            config
        )
        .then((response) => {
            let data = JSON.parse(response.data.data);
            if(datalists.value.length > 0){
                data[0].forEach((e) => {
                    if(datalists.value.filter(x=>x.organization_id == e.organization_id).length == 0){
                        listCompanys.value.push(e);
                    }
                })
            }
            headerSelectCompany.value = "Chọn công ty";
            displaySelectCompany.value = true;
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
const closeDialogSelectCompany = () => {
    displaySelectCompany.value = false;
}
const ChoseCompany = () => {
    if (selectedCompanys.value.length > 0) {
        selectedCompanys.value.forEach((e) => {
            let arr = {
                setting_id: "-1", is_num_device: null, is_verify_sign: false, is_skip_web: false, is_one_device: false, is_creator_cancel_request: false, is_creator_del_request: false,
                is_creator_tag_follow: false, is_creator_handle: false, is_approved_cancel_request: false, is_approved_del_request: false, is_approved_tag_follow: false, is_approved_handle: false, organization_id: e.organization_id, organization_name: e.organization_name
            };
            datalists.value.push(arr);
        })
    }
    closeDialogSelectCompany();
}
const saveData = () => {
    let formData = new FormData();
    formData.append("request_setting", JSON.stringify(datalists.value));
    axios
        .post(
            baseURL +
            "/api/request_setting/add_request_setting",
            formData,
            config,
        )
        .then((response) => {
            if (response.data.err != "1") {
                swal.close();
                toast.success("Cập nhật thiết lập xác thực ký thành công!");
                loadData(true);
            }
        })
        .catch(() => {
            swal.close();
            swal.fire({
                title: "Error!",
                text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
                icon: "error",
                confirmButtonText: "OK",
            });
        });
}
const loadData = (rf) => {
    options.value.loading = true;
    axios
        .post(
            baseUrlCheck + "/api/request/getData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "request_setting_list",
                        par: [
                            { par: "user_id", va: store.state.user.user_id },
                            // { par: "search", va: options.value.SearchText },
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
            datalists.value = data;
            options.value.loading = false;
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
    if (!checkURL(window.location.pathname, store.getters.listModule)) {
        //router.back();
    }
    loadData(true);
    return {
        datalists,
        options,
        loadData,
    };
});
</script>
<template>
    <div class="main-layout flex-grow-1 p-2 pb-0 pr-0" v-if="store.getters.islogin">\
        <DataTable class="table-request-setting" :value="datalists" :paginator="false" :scrollable="true" scrollHeight="flex"
            :loading="options.loading" v-model:selection="selectedDatas" :lazy="true" dataKey="setting_id" :rowHover="true"
            :showGridlines="true" responsiveLayout="scroll">
            <template #header>
                <h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
                    <i class="pi pi-book"></i> Thiết lập xác thực ký <span class="pl-1" v-if="datalists.length > 0">{{ '('
                        + datalists.length + ')' }}</span>
                </h3>
                <Toolbar class="w-full custoolbar">
                    <template #start>
                        <span class="p-input-icon-left">
                            <i class="pi pi-search" />
                            <InputText type="text" spellcheck="false" placeholder="Tìm kiếm" style="min-width:30rem;" />
                        </span>
                    </template>
                    <template #end>
                        <Button @click="openSelectCompany('Chọn công ty')" label="Chọn công ty" icon="pi pi-plus"
                            class="mr-2" />
                        <Button v-if="datalists.length > 0" @click="saveData()" label="Lưu" icon="pi pi-check"
                            class="mr-2" />
                        <Button @click="refreshData" class="mr-2 p-button-outlined p-button-secondary" icon="pi pi-refresh"
                            v-tooltip="'Tải lại'" />
                    </template>
                </Toolbar>
            </template>
            <template #groupheader="slotProps">
                <span class="ca-text pl-4">
                    {{ slotProps.data.request_group_name }}
                </span>
                <span class="ca-text pl-1">{{ '(' + (slotProps.data.count_group || 0) + ')' }}</span>
            </template>
            <Column field="organization_name" header="Công ty"
                headerStyle="text-align:center;height:50px;border-left:none;border-right:none;justify-content:center"
                bodyStyle="border-left:none;border-right:none;" class="align-items-center text-center">
            </Column>
            <Column field="report_name" header="Số thiết bị đăng ký"
                headerStyle="text-align:center;max-width:15rem;height:50px;border-left:none;border-right:none;"
                bodyStyle="justify-content:center;max-width:15rem;border-left:none;border-right:none;"
                headerClass="align-items-center justify-content-center">
                <template #body="data">
                    <InputNumber v-model="data.data.is_num_device" style="text-align: center;" class="col-12 ip36 p-0" />
                </template>
            </Column>
            <Column field="" header="Xác thực trước khi ký"
                headerStyle="text-align:center;max-width:15rem;height:50px;border-left:none;border-right:none;"
                bodyStyle="justify-content:center;max-width:15rem;border-left:none;border-right:none;position: relative;"
                headerClass="align-items-center justify-content-center">
                <template #body="data">
                    <InputSwitch class="col-12" style="position: absolute;" v-model="data.data.is_verify_sign" />
                </template>
            </Column>
            <Column field="" header="Xác thực sau khi ký trên web"
                headerStyle="text-align:center;max-width:15rem;height:50px;border-left:none;border-right:none;"
                bodyStyle="text-align:center;max-width:15rem;border-left:none;border-right:none;position: relative;"
                class="align-items-center justify-content-center text-center">
                <template #body="data">
                    <InputSwitch class="col-12" style="position: absolute;" v-model="data.data.is_skip_web" />
                </template>
            </Column>
            <Column field="status" header="1 thiết bị chỉ ký 1 user"
                headerStyle="text-align:center;max-width:15rem;height:50px;border-left:none;border-right:none;"
                bodyStyle="text-align:center;max-width:15rem;border-left:none;border-right:none;position: relative;"
                class="align-items-center justify-content-center text-center">
                <template #body="data">
                    <InputSwitch class="col-12" style="position: absolute;" v-model="data.data.is_one_device" />
                </template>
            </Column>
            <template #empty>
                <div class="block w-full h-full format-center" v-if="datalists.length == 0">
                    <img src="../../../assets/background/nodata.png" height="144" />
                    <h3 class="m-1">Không có dữ liệu</h3>
                </div>
            </template>
        </DataTable>
    </div>

    <Dialog :header="headerSelectCompany" v-model:visible="displaySelectCompany" :style="{ width: '40vw' }" :closable="true"
        position="top" :modal="true">
        <form @submit.prevent="">
            <div class="grid formgrid m-0">
                <div class="field col-12 md:col-12 algn-items-center flex p-0">
                    <div class="col-6 text-left flex p-0" style="align-items:center;">
                        <span class="p-input-icon-left">
                            <i class="pi pi-search" />
                            <InputText type="text" spellcheck="false" placeholder="Tìm kiếm" style="min-width:20rem;" />
                        </span>
                    </div>
                </div>
                <div class="field col-12 md:col-12 algn-items-center p-0">
                    <DataTable class="table-ca-request" :value="listCompanys" :paginator="false" :scrollable="true"
                        scrollHeight="flex" :lazy="true" dataKey="organization_id" :rowHover="true"
                        v-model:selection="selectedCompanys">
                        <Column selectionMode="multiple" headerStyle="text-align:center;max-width:4rem;"
                            bodyStyle="text-align:center;max-width:4rem;"
                            class="align-items-center justify-content-center text-center">
                        </Column>
                        <Column field="organization_name" header="Tên công ty" headerStyle="text-align:left;height:50px"
                            bodyStyle="text-align:left">
                        </Column>
                        <template #empty>
                            <div class="align-items-center justify-content-center p-4 text-center m-auto" style="
                                                          display: flex;
                                                          flex-direction: column;
                                                        " v-if="listCompanys.length == 0">
                                <img src="../../../assets/background/nodata.png" height="144" />
                                <h3 class="m-1">Không có dữ liệu</h3>
                            </div>
                        </template>
                    </DataTable>
                </div>
            </div>
        </form>
        <template #footer>
            <Button label="Hủy" icon="pi pi-times" @click="closeDialogSelectCompany()" class="p-button-outlined" />
            <Button label="Chọn" icon="pi pi-check" @click="ChoseCompany()" autofocus />
        </template>
    </Dialog>
</template>
<style lang="scss" scoped>
::v-deep(.table-request-setting) {
  .p-inputnumber-input {
    text-align: center;
  }
}
</style>