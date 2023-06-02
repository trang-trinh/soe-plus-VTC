<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
//import moment from "moment";
// import dialogAddFormRequest from "../category/component/dialog_add_form_request.vue";
// import dialogShowFormTeam from "../category/component/dialog_show_form_team.vue";
// import dialogSettingForm from "../category/component/dialog_setting_form.vue";
// import procedureDetail from "../category/component/procedure_detail.vue";
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
const request_number = ref([
    { value: "macongtydx", text: "Mã công ty" },
    { value: "madx", text: "Mã đề xuất" },
    { value: "namdx", text: "Năm đề xuất" },
    { value: "sodx", text: "Số đề xuất" },
]);
const options = ref({
    loading: true,
    SearchText: '',
    loading: false,
});
const loadData = (rf) => {
    options.value.loading = true;
    axios
        .post(
            baseUrlCheck + "/api/request/getData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "request_number_list",
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
            let data = JSON.parse(response.data.data);
            datalists.value = data[0];
            if (data[0].length == 0) {
                request_number.value.forEach((d, i) => {
                    let rq = { setting_number_id: "-1", organization_id: store.state.user.organization_id, id_key: d.value, is_order: i + 1, information_column: d.text, number_of_characters: null, is_use: true, seperate: ".", automatic: false };
                    datalists.value.push(rq);
                })
            }
            options.value.loading = false;
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
}
const saveData = () => {
    let formData = new FormData();
    formData.append("request_number", JSON.stringify(datalists.value));
    axios
        .post(
            baseURL +
            "/api/request_number/add_request_number",
            formData,
            config,
        )
        .then((response) => {
            if (response.data.err != "1") {
                swal.close();
                toast.success("Cập nhật thiết lập số hiệu đề xuất thành công!");
                loadData(true);
            }
        })
        .catch((res) => {
            swal.close();
            swal.fire({
                title: "Error!",
                text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
                icon: "error",
                confirmButtonText: "OK",
            });
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
    <div class="main-layout flex-grow-1 p-2 pb-0 pr-0" v-if="store.getters.islogin">
        <div style="height: 99%;background-color: #fff;width: 99.5%;">
            <div class="flex justify-content-center align-items-center" style="flex-direction: column;">
                <h2>BẢNG THIẾT LẬP SỐ HIỆU ĐỀ XUẤT</h2>
                <div class="title-box pB-15">
                    <h3>{{ store.state.user.organization_name }}</h3>
                </div>
                <div style="width: 80%;font-size: 14px;">
                    <DataTable class="table-request-number" :value="datalists" :paginator="false" :scrollable="true"
                        scrollHeight="flex" :loading="options.loading" v-model:selection="selectedDatas" :lazy="true"
                        dataKey="setting_number_id" :rowHover="true" :showGridlines="true" responsiveLayout="scroll">
                        <Column field="is_order" header="STT"
                            headerStyle="text-align:center;width:3rem;height:50px;border-left:none;border-right:none;"
                            bodyStyle="justify-content:center;width:3rem;border-left:none;border-right:none;"
                            headerClass="align-items-center justify-content-center">
                        </Column>
                        <Column field="id_key" header="IDKey"
                            headerStyle="text-align:center;width:15rem;height:50px;border-left:none;border-right:none;"
                            bodyStyle="justify-content:center;width:15rem;border-left:none;border-right:none;"
                            headerClass="align-items-center justify-content-center">
                            <template #body="data">
                                <InputText v-model="data.data.id_key" :disabled="true" class="col-12 ip36 p-2" />
                            </template>
                        </Column>
                        <Column field="information_column" header="Cột thông tin"
                            headerStyle="text-align:center;width:15rem;height:50px;border-left:none;border-right:none;"
                            bodyStyle="justify-content:center;width:15rem;border-left:none;border-right:none;"
                            headerClass="align-items-center justify-content-center">
                            <template #body="data">
                                <InputText v-model="data.data.information_column" class="col-12 ip36 p-2" />
                            </template>
                        </Column>
                        <Column field="is_use" header="Sử dụng"
                            headerStyle="text-align:center;width:15rem;height:50px;border-left:none;border-right:none;"
                            bodyStyle="justify-content:center;width:15rem;border-left:none;border-right:none;"
                            headerClass="align-items-center justify-content-center">
                            <template #body="data">
                                <InputSwitch class="col-12" style="position: absolute;" v-model="data.data.is_use" />
                            </template>
                        </Column>
                        <Column field="seperate" header="Ký tự ngăn cách"
                            headerStyle="text-align:center;width:15rem;height:50px;border-left:none;border-right:none;"
                            bodyStyle="justify-content:center;width:15rem;border-left:none;border-right:none;"
                            headerClass="align-items-center justify-content-center">
                            <template #body="data">
                                <InputText v-model="data.data.seperate" class="col-12 ip36 p-2 p-input-align" />
                            </template>
                        </Column>
                        <Column field="number_of_characters" header="Số ký tự"
                            headerStyle="text-align:center;width:15rem;height:50px;border-left:none;border-right:none;"
                            bodyStyle="justify-content:center;width:15rem;border-left:none;border-right:none;"
                            headerClass="align-items-center justify-content-center">
                            <template #body="data">
                                <InputNumber v-model="data.data.number_of_characters"
                                    class="col-12 p-0 h-full p-input-align" />
                            </template>
                        </Column>
                    </DataTable>
                    <Button @click="saveData()" label="Lưu" icon="pi pi-check" class="mr-2" style="margin-top: 10px;" />
                    <div style="margin-top: 10px;">
                        <i style="color: #ecec15" class="pi pi-info-circle"></i> Bảng thiết lập bộ mã ký hiệu đề xuất,
                        mục đích hệ thống tự động sinh ra bộ mã theo tiêu chí đơn vị sử dụng thiết lập.<br />
                        Ví dụ: Đơn vị sử dụng các cột thông tin trên, theo thứ tự mặc định (từ nhỏ đến lớn)
                    </div>
                    <div class="pL-30" v-for="(k, index) in datalists" style="margin-top: 10px;">
                        <span>{{ index + 1 }}. </span><span>{{ k.information_column }}: {{ k.id_key === 'sodx' ? '1 (tự động tăng) ' :
                        k.id_key === 'madx' ? 'F01' : k.id_key === 'namdx' ? '2020' : k.id_key === 'macongtydx' ?
                            'TCHC-ECP' : '' }}</span>
                    </div>
                    <i style="margin-top: 10px;" class="pi pi-arrow-right"></i> Hệ thống tự động sinh ra bộ mã ký hiệu như
                    sau: <b v-if="datalists.length > 0" v-for="k in datalists"><span>{{ k.id_key === 'sodx' ? '1' : k.id_key
                        ===
                        'madx' ? 'F01' : k.id_key === 'namdx' ? '2020' : k.id_key === 'macongtydx' ? 'TCHC-ECP' :
                            '' }}{{ k.seperate }}</span></b>
                </div>
            </div>
        </div>
    </div>
</template>
<style lang="scss" scoped>
::v-deep(.table-request-number) {

    .p-input-align,
    .p-inputnumber-input {
        text-align: center;
    }
}

.pL-30 {
    padding-left: 30px;
}</style>