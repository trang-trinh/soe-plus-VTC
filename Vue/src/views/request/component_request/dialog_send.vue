<script setup>
import { ref, inject, onMounted } from "vue";
import { encr } from "../../../util/function";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import moment from "moment";

const cryoptojs = inject("cryptojs");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");

const basedomainURL = baseURL;
const config = {
    headers: {
        Authorization: `Bearer ${store.getters.token}`,
    },
};
const toast = useToast();
//Declare
const bgColor = ref([
    "#F8E69A",
    "#AFDFCF",
    "#F4B2A3",
    "#9A97EC",
    "#CAE2B0",
    "#8BCFFB",
    "#CCADD7",
]);
const typeprocedures = ref([
    { value: 0, title: "Duyệt tuần tự" },
    { value: 1, title: "Duyệt một trong nhiều" },
    //{ value: 2, title: "Duyệt ngẫu nhiên" },
]);

//Get arguments
const props = defineProps({
    headerDialog: String,
    displayDialog: Boolean,
    dataSelected: Array,
    modelsend: Object,
    closeDialog: Function,
});

//Function
const listTCard = ref([
    { name: "Duyệt một nhiều", code: 1 },
    { name: "Duyệt tuần tự", code: 2 },
    { name: "Duyệt ngẫu nhiên", code: 3 },
]);

const displayDialog = ref(false);

const filesList = ref([]);

const onUploadFile = (event) => {
    filesList.value = [];
    var ms = false;
    event.files.forEach((fi) => {
        let formData = new FormData();
        formData.append("fileupload", fi);
        axios({
            method: "post",
            url: baseURL + `/api/chat/ScanFileUpload`,
            data: formData,
            headers: {
                Authorization: `Bearer ${store.getters.token}`,
            },
        })
        .then((response) => {
            if (response.data.err != "1") {
                if (fi.size > 100 * 1024 * 1024) {
                    ms = true;
                } else {
                    filesList.value.push(fi);
                }
            } else {
                filesList.value = filesList.value.filter((x) => x.name != fi.name);
                swal.fire({
                    title: "Cảnh báo",
                    text: "File bị xóa do tồn tại mối đe dọa với hệ thống!",
                    icon: "warning",
                    confirmButtonText: "OK",
                });
            }
            if (ms) {
                swal.fire({
                    icon: "warning",
                    type: "warning",
                    title: "Thông báo",
                    text: "Bạn chỉ được upload file có dung lượng tối đa 100MB!",
                });
            }
        })
        .catch(() => {
            swal.fire({
                title: "Thông báo",
                text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
                icon: "error",
                confirmButtonText: "OK",
            });
        });
    });
};
const removeFile = (event) => {
    filesList.value = filesList.value.filter((a) => a != event.file);
};
const process = ref({
    content: null,
    key_id: null,
    aprroved_type: 3,
});
const listProcess = ref([]);
const listAproved = ref([]);
const listUsers = ref([]);
const initTudien = () => {
    // Chuyển đến quy trình
    if (props.modelsend.type_module == 0 && props.modelsend.type_send == 0) {
        listProcess.value = [];
        axios
            .post(
                baseURL + "/api/request/getData",
                {
                    str: encr(
                        JSON.stringify({
                            proc: "request_list_process_by_master",
                            par: [
                                { par: "user_id", va: store.getters.user.user_id },
                                { par: "module_key", va: props.modelsend.module_key },
                                { par: "request_id", va: props.dataSelected[0].request_id },
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
                    if (data.length > 0) {
                        data[0].forEach((element, i) => {
                            listProcess.value.push({
                                name: element.config_process_name,
                                code: element.config_process_id,
                                request_form_id: element.request_form_id,
                            });
                        });
                        if (listProcess.value.length > 0 && listProcess.value[0].code == 0) {
                            process.value.key_id = listProcess.value[0].code;
                            changeProcedure(process.value.key_id);
                        }
                    } else {
                        listProcess.value = [];
                    }
                } else {
                    listProcess.value = [];
                }
                swal.close();
            })
            .catch((error) => {
                swal.close();
                swal.fire({
                    title: "Thông báo!",
                    text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
                    icon: "error",
                    confirmButtonText: "OK",
                });
                return;
            });
    } 
    // Chuyển đến nhóm (coding ...)
    else if (props.modelsend.type_module == 0 && props.modelsend.type_send == 1) 
    {
        listAproved.value = [];
        axios
            .post(
                baseURL + "/api/request/getData",
                {
                    str: encr(
                        JSON.stringify({
                            proc: "request_group_approved_all",
                            par: [
                                { par: "user_id", va: store.getters.user.user_id },
                                { par: "module_key", va: props.modelsend.module_key },
                                { par: "request_id", va: props.dataSelected[0].request_id },
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
                    var data = response.data.data;
                    if (data != null) {
                        let tbs = JSON.parse(data);
                        if (tbs[0] != null && tbs[0].length > 0) {
                            if (tbs[0].length > 0) {
                                tbs[0].forEach((element, i) => {
                                    element["STT"] = i + 1;
                                    if (element["signusers"] != null) {
                                        element["signusers"] = JSON.parse(element["signusers"]);
                                    }
                                    if (element["signusers"] != null) {
                                        element["signusers"].forEach((su) => {
                                            if (su.is_order == "") {
                                                su.is_order = null;
                                            }
                                            else {
                                                su.is_order = Number(su.is_order);
                                            }
                                            if (su.approved_users_id == "") {
                                                su.approved_users_id = null;
                                            }
                                            else {
                                                su.approved_users_id = Number(su.approved_users_id);
                                            }
                                            if (su.department_id == "") {
                                                su.department_id = null;
                                            }
                                            else {
                                                su.department_id = Number(su.department_id);
                                            }
                                            if (su.avatar == "") {
                                                su.avatar = null;
                                            }
                                        });

                                        listAproved.value.push({
                                            name: element.approved_group_name,
                                            code: element.approved_groups_id,
                                            approved_type_name: element.approved_type_name,
                                            data: element,
                                            signusers: element.signusers,
                                        });
                                    }
                                });
                            }
                        }
                    }
                }
                swal.close();
            })
            .catch((error) => {
                swal.close();
                swal.fire({
                    title: "Thông báo!",
                    text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
                    icon: "error",
                    confirmButtonText: "OK",
                });
                console.log(error);
                return;
            });
    } 
    // Chuyển đích danh
    else if (props.modelsend.type_module == 0 && props.modelsend.type_send == 2) 
    {
        listUsers.value = [];
        axios
            .post(
                baseURL + "/api/request/getData",
                {
                    str: encr(
                        JSON.stringify({
                            proc: "request_users_list",
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
                let data = JSON.parse(response.data.data);
                if (data.length > 0) {
                    data[0].forEach((element, i) => {
                        listUsers.value.push({
                            full_name: element.full_name,
                            last_name: element.last_name,
                            user_id: element.user_id,
                            avatar: element.avatar,
                            department_name: element.department_name,
                            role_name: element.role_name,
                            position_name: element.position_name,
                        });
                    });
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
    }
};
const checkTimline = ref(false);
const signforms = ref();
const changeProcedure = (procedureform_id) => {
    if (procedureform_id != null) {
        let requestFormID = listProcess.value.filter(x => x.code == procedureform_id)[0].request_form_id;
        axios
            .post(
                baseURL + "/api/request/getData",
                {
                    str: encr(
                        JSON.stringify({
                            proc: "request_group_approved_list",
                            par: [
                                { par: "config_process_id", va: procedureform_id },
                                { par: "request_form_id", va: procedureform_id != 0 ? null : requestFormID },
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
                    var data = response.data.data;

                    if (data != null) {
                        let tbs = JSON.parse(data);
                        if (tbs[0] != null && tbs[0].length > 0) {
                            signforms.value = tbs[0];
                            if (signforms.value.length > 0) {
                                signforms.value.forEach((element, i) => {
                                    element["STT"] = i + 1;

                                    if (element["signusers"] != null) {
                                        element["signusers"] = JSON.parse(element["signusers"]);
                                    }
                                });
                            }
                            checkTimline.value = true;
                        } else {
                            signforms.value = [];
                        }
                    }
                }
                //swal.close();
            })
            .catch((error) => {
                //swal.close();
                swal.fire({
                    title: "Thông báo!",
                    text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
                    icon: "error",
                    confirmButtonText: "OK",
                });
                return;
            });
    }
    else {
        checkTimline.value = false;
        signforms.value = [];
    }
};
const onTaskUserFilter = (item) => {

};
const removeMember = (user, arr, type) => {
    if (type == null) {
        var idx = arr.findIndex((x) => x["user_id"] === user["user_id"]);
        if (idx != -1) {
            arr.splice(idx, 1);
        }
    }
};
const submitted = ref(false);
const send = () => {
    if (submitted.value == true) {
        return;
    }
    submitted.value = true;
    if (!process.value.key_id) {
        swal.fire({
            title: "Thông báo",
            text: "Vui lòng điền đầy đủ thông tin trường bôi đỏ!",
            icon: "error",
            confirmButtonText: "OK",
        });
        return;
    }
    var obj = { ...props.modelsend };
    let formData = new FormData();
    if (props.modelsend.type_send == 2) { // chuyen dich danh
        formData.append("aprroved_type", process.value.aprroved_type);
        let strv = "", strc = "";
        process.value.key_id.forEach((element) => {
            strv += strc + element.code;
            strc = ",";
        });
        obj.key_id = strv;
    } else if (props.modelsend.type_send == 1) { // chuyen den nhom
        obj.key_id = process.value.key_id.code;
    } else { // chuyen theo quy trinh
        obj.key_id = process.value.key_id;
    }
    if (process.value.content)
        obj.content = process.value.content;
    else
        obj.content = "";

    formData.append("type_send", obj["type_send"]);
    formData.append("key_id", obj["key_id"]);
    //formData.append("type_module", obj["type_module"]);
    formData.append("content", obj["content"]);
    for (var i = 0; i < filesList.value.length; i++) {
        let file = filesList.value[i];
        formData.append("files", file);
    }
    formData.append("request_obj", JSON.stringify(props.dataSelected));

    swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
    axios
        .post(
            baseURL + "/api/request/Send_Request",
            formData,
            config
        )
        .then((response) => {
            if (response.data.err === "1") {
                swal.fire({
                    title: "Thông báo!",
                    text: response.data.ms,
                    icon: "error",
                    confirmButtonText: "OK",
                });
                return;
            }
            
            if (submitted.value) submitted.value = false;
            swal.close();
            toast.success("Gửi thành công!");
            props.closeDialog();
        })
        .catch((error) => {
            if (submitted.value) submitted.value = false;
            swal.close();
            swal.fire({
                title: "Thông báo!",
                text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
                icon: "error",
                confirmButtonText: "OK",
            });
        });
};

onMounted(() => {
    displayDialog.value = props.displayDialog;
    initTudien();
});
</script>
<template>
    <Dialog 
        :header="props.headerDialog" 
        v-model:visible="displayDialog" 
        :style="{ width: '40vw' }" 
        :maximizable="false"
        :closable="true" 
        @hide="props.closeDialog" 
        :modal="true"
    >
        <form>
            <div class="grid formgrid m-2">
                <div class="col-12 md-col-12">
                    <div class="form-group">
                        <label>
                            {{
                                props.modelsend.type_send === 0
                                ? "Quy trình"
                                : props.modelsend.type_send === 1
                                    ? "Nhóm duyệt"
                                    : props.modelsend.type_send === 2
                                        ? "Người duyệt"
                                        : ""
                            }}
                            <span class="redsao">(*)</span>
                        </label>
                        <div>
                            <div v-if="props.modelsend.type_send === 0">
                                <Dropdown 
                                    :options="listProcess" 
                                    :filter="true" 
                                    :showClear="true" 
                                    :class="{'p-invalid': process.key_id == null && submitted }" 
                                    v-model="process.key_id" 
                                    optionLabel="name" 
                                    optionValue="code" 
                                    placeholder="Chọn quy trình"
                                    class="ip36 mb-2" 
                                    @change="changeProcedure(process.key_id)"
                                >
                                </Dropdown>
                                <div v-if="checkTimline" class="p-inputtext ip36 p-3"
                                    style="min-height: 36px; height: auto">
                                    <Timeline :value="signforms" 
                                        align="alternate" 
                                        class="customized-timeline">
                                        <template #content="slotProps">
                                            <Card class="my-2">
                                                <template #title>
                                                    {{ slotProps.item.group_by_form ? slotProps.item.group_name : slotProps.item.approved_group_name }}
                                                </template>
                                                <template #subtitle>
                                                    {{
                                                        ((!slotProps.item.group_by_form && slotProps.item.approved_type == 1) || (slotProps.item.group_by_form && slotProps.item.type_process == 0))
                                                        ? "Duyệt một nhiều"
                                                        : ((!slotProps.item.group_by_form && slotProps.item.approved_type == 2) || (slotProps.item.group_by_form && slotProps.item.type_process == 1))
                                                        ? "Duyệt tuần tự"
                                                        : "Duyệt ngẫu nhiên"
                                                    }}
                                                </template>
                                                <template #content>
                                                    <AvatarGroup class="format-flex-center">
                                                        <Avatar v-for="(item, index) in slotProps.item.signusers.slice(0, 3)" 
                                                            :key="item.user_id"
                                                            v-bind:label="item.avatar ? '' : item.last_name.substring(0, 1)" 
                                                            v-bind:image="item.avatar
                                                                ? basedomainURL + item.avatar
                                                                : basedomainURL + '/Portals/Image/noimg.jpg'
                                                            " 
                                                            v-tooltip.top="{ 
                                                                value: item.full_name + (item.position_name ? ('<br/>' + item.position_name) : '') + (item.department_name ? ('<br/>' + item.department_name) : ''),
                                                                escape: true,
                                                            }"
                                                            style="border: 2px solid white; color: white"
                                                            @click="onTaskUserFilter(item)" 
                                                            @error="basedomainURL + '/Portals/Image/noimg.jpg'" 
                                                            size="large" 
                                                            shape="circle" 
                                                            class="cursor-pointer"
                                                            :style="{ backgroundColor: bgColor[index % 7] }" 
                                                        />
                                                        <Avatar v-if="slotProps.item.signusers != null && slotProps.item.signusers.length > 3"
                                                            v-bind:label="'+' + (slotProps.item.signusers.length - 3).toString()" 
                                                            shape="circle" 
                                                            size="large" 
                                                            style="background-color: #2196f3; color: #ffffff"
                                                            class="cursor-pointer" 
                                                        />
                                                    </AvatarGroup>
                                                </template>
                                            </Card>
                                        </template>
                                    </Timeline>
                                </div>
                            </div>
                            <div v-if="props.modelsend.type_send === 1" class="col-12 md:col-12 p-0 flex">
                                <Dropdown v-model="process.key_id" 
                                    :options="listAproved" 
                                    optionLabel="name" 
                                    :filter="true"
                                    panelClass="d-design-dropdown" 
                                    placeholder="Chọn nhóm duyệt"
                                    class="p-0 p-design-dropdown w-full mb-2" 
                                    ref="isRefAprroved"
                                >
                                    <template #value="slotProps">
                                        <div class="p-dropdown-car-value format-center w-full h-full"
                                            v-if="slotProps.value">
                                            <div>
                                                <div class="p-2 px-0">
                                                    <span class="font-bold">{{ slotProps.value.name }}</span>
                                                    <span class="ml-2" v-if="slotProps.value.approved_type_name != null">
                                                        ({{ slotProps.value.approved_type_name }})
                                                    </span>
                                                </div>

                                                <div class="flex px-2 format-center py-0">
                                                    <div v-if="slotProps.value.signusers">
                                                        <AvatarGroup v-if="slotProps.value.signusers && slotProps.value.signusers.length > 0">
                                                            <Avatar v-for="(elen, index1) in slotProps.value.signusers.slice(0, 3)"
                                                                :key="index1"  
                                                                v-bind:label="elen.avatar ? '' : elen.last_name.substring(0, 1)" 
                                                                v-bind:image="elen.avatar
                                                                    ? basedomainURL + elen.avatar
                                                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                                                " 
                                                                v-tooltip.bottom="{
                                                                    value: elen.full_name + (elen.position_name ? ('<br/>' + elen.position_name) : '') + (elen.department_name ? ('<br/>' + elen.department_name) : ''),
                                                                    escape: true,
                                                                }" 
                                                                style="
                                                                    border: 2px solid #ffffff;
                                                                    color: #ffffff;
                                                                " 
                                                                @error="basedomainURL + '/Portals/Image/noimg.jpg'" 
                                                                size="large" 
                                                                shape="circle" 
                                                                class="cursor-pointer" 
                                                                :style="{ backgroundColor: bgColor[index1 % 7] }" 
                                                            />
                                                            <Avatar v-if="slotProps.value.signusers && slotProps.value.signusers.length > 3"
                                                                v-bind:label="'+' + (slotProps.value.signusers.length - 3).toString()" 
                                                                shape="circle" 
                                                                size="large" 
                                                                style="
                                                                    background-color: #2196f3;
                                                                    color: #ffffff;
                                                                " 
                                                                class="cursor-pointer" 
                                                            />
                                                        </AvatarGroup>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div v-else class="format-center h-full">
                                            {{ slotProps.placeholder }}
                                        </div>
                                    </template>
                                    <template #option="slotProps">
                                        <div class="p-dropdown-car-option format-center">
                                            <div>
                                                <div class="p-2 px-0">
                                                    <span class="font-bold">{{ slotProps.option.name }}</span>
                                                    <span class="ml-2" v-if="slotProps.option.approved_type_name != null">
                                                        ({{ slotProps.option.approved_type_name }})
                                                    </span>
                                                </div>

                                                <div class="flex px-2 py-0 format-center">
                                                    <div v-if="slotProps.option.signusers">
                                                        <AvatarGroup v-if="slotProps.option.signusers && slotProps.option.signusers.length > 0">
                                                            <Avatar v-for="(elen, index1) in slotProps.option.signusers.slice(0, 3)"
                                                                :key="index1"
                                                                v-bind:label="elen.avatar ? '' : elen.last_name.substring(0, 1)" 
                                                                v-bind:image="elen.avatar
                                                                    ? basedomainURL + elen.avatar
                                                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                                                " 
                                                                v-tooltip.bottom="{
                                                                    value: elen.full_name + (elen.position_name ? ('<br/>' + elen.position_name) : '') + (elen.department_name ? ('<br/>' + elen.department_name) : ''),
                                                                    escape: true,
                                                                }"  
                                                                style="
                                                                    border: 2px solid #ffffff;
                                                                    color: #ffffff;
                                                                " 
                                                                @error="basedomainURL + '/Portals/Image/noimg.jpg'" 
                                                                size="large" 
                                                                shape="circle" 
                                                                class="cursor-pointer" 
                                                                :style="{ backgroundColor: bgColor[index1 % 7] }" 
                                                            />
                                                            <Avatar v-if="slotProps.option.signusers && slotProps.option.signusers.length > 3"
                                                                v-bind:label="'+' + (slotProps.option.signusers.length - 3).toString()" 
                                                                shape="circle" 
                                                                size="large" 
                                                                style="
                                                                    background-color: #2196f3;
                                                                    color: #ffffff;
                                                                " 
                                                                class="cursor-pointer" 
                                                            />
                                                        </AvatarGroup>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </template>
                                </Dropdown>
                            </div>
                            <div v-if="props.modelsend.type_send === 2">
                                <div class="col-12 p-0">
                                    <MultiSelect 
                                        :options="listUsers" 
                                        :filter="true" 
                                        :showClear="true" 
                                        :class="{ 'p-invalid': process.key_id == null && submitted }" 
                                        v-model="process.key_id" 
                                        optionLabel="full_name" 
                                        optioncode="user_id" 
                                        display="chip"
                                        placeholder="Chọn người duyệt" 
                                        class="ip36 mb-2"
                                        style="height: auto; min-height: 36px"
                                    >
                                        <template #value="slotProps">
                                            <ul class="p-ulchip"
                                                v-if="slotProps.value && slotProps.value.length > 0"
                                            >
                                                <li class="p-lichip"
                                                    v-for="(value, index) in slotProps.value"
                                                    :key="index"
                                                >
                                                    <Chip class="mr-2 mb-2 pl-0"
                                                        :image="value.avatar"
                                                        :label="value.full_name"                                        
                                                    >
                                                        <div class="flex">
                                                            <div class="format-flex-center">
                                                                <Avatar
                                                                    v-bind:label="value.avatar ? '' : (value.last_name ?? 'A').substring(0, 1)"
                                                                    v-bind:image="
                                                                        value.avatar
                                                                        ? basedomainURL + value.avatar
                                                                        : basedomainURL + '/Portals/Image/noimg.jpg'
                                                                    "
                                                                    style="background-color: #2196f3; color: #ffffff; width: 2rem; height: 2rem;"
                                                                    :style="{ background: bgColor[index % 7], }"
                                                                    class="mr-2 text-avatar"
                                                                    size="xlarge"
                                                                    shape="circle"
                                                                />
                                                            </div>
                                                            <div class="format-flex-center">
                                                                <span>{{ value.full_name }}</span>
                                                            </div>
                                                            <span tabindex="0"
                                                                class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                                                @click="removeMember(value, process.key_id)"
                                                            ></span>
                                                        </div>
                                                    </Chip>
                                                </li>
                                            </ul>
                                            <span v-else> {{ slotProps.placeholder }} </span>
                                        </template>
                                        <template #option="slotProps">
                                            <div v-if="slotProps.option" class="flex">
                                                <div class="format-center ml-1">
                                                    <Avatar
                                                        v-bind:label="slotProps.option.avatar
                                                            ? ''
                                                            : slotProps.option.last_name.substring(0, 1)
                                                        " 
                                                        :image="basedomainURL + slotProps.option.avatar" 
                                                        style="color: #ffffff; width: 2rem; height: 2rem;margin-top: 0;"
                                                        :style="slotProps.option.avatar
                                                            ? 'background-color: #2196f3'
                                                            : 'background:' + bgColor[slotProps.option.full_name.length % 7]
                                                        " 
                                                        shape="circle" 
                                                        @error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'" 
                                                    />
                                                </div>
                                                <div class="ml-3 flex" style="flex-direction: column; justify-content: center;">
                                                    <div class="mb-1">{{ slotProps.option.full_name }}</div>
                                                    <div class="description">
                                                        <div v-if="slotProps.option.position_name">
                                                            {{ slotProps.option.position_name || "" }}
                                                        </div>
                                                        <div v-if="slotProps.option.department_name">
                                                            {{ slotProps.option.department_name || "" }}
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </template>
                                    </MultiSelect>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-12 md:col-12" v-if="props.modelsend.type_send == 2">
                    <div class="form-group">
                        <label>Loại duyệt</label>
                        <Dropdown 
                            v-model="process.aprroved_type" 
                            panelClass="d-design-dropdown" 
                            :options="listTCard"
                            :filter="true" 
                            optionLabel="name" 
                            optionValue="code" 
                            class="w-full"
                        >
                        </Dropdown>
                    </div>
                </div>
                <div class="col-12 p-0 field" v-if="process.aprroved_type == 2">
                    <div class="col-12  field">
                        <OrderList v-model="process.key_id" 
                            listStyle="height:auto" 
                            class="w-full order-approver-request" 
                            dataKey="id"
                        >
                            <template #header>Thứ tự duyệt </template>
                            <template #item="slotProps">
                                <Toolbar class="surface-0 m-0 p-0 border-0 w-full">
                                    <template #start>
                                        <div class="flex align-items-center">
                                            <div class="format-flex-center">
                                                <b class="px-3" 
                                                    style="padding-top: 0.75rem; padding-bottom: 0.75rem;"
                                                >
                                                    {{ slotProps.index + 1 }}
                                                </b>
                                            </div>
                                            <div class="flex">
                                                <Avatar 
                                                    v-bind:label="slotProps.item.avatar
                                                        ? ''
                                                        : (slotProps.item.last_name 
                                                            ? slotProps.item.last_name.substring(0, 1) 
                                                            : 'A'
                                                        )
                                                    " 
                                                    :image="basedomainURL + slotProps.item.avatar" 
                                                    style="width: 2rem; height: 2rem; font-size: 1rem !important;"
                                                    :style="slotProps.item.avatar
                                                        ? 'background-color: #2196f3'
                                                        : 'background:' + bgColor[slotProps.item.full_name.length % 7]
                                                    " 
                                                    shape="circle" 
                                                    @error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'" 
                                                />
                                                <div class="ml-3 flex" style="flex-direction: column; justify-content: center;">
                                                    <div class="mb-1">
                                                        {{ slotProps.item.full_name }}
                                                    </div>
                                                    <div class="description">
                                                        <div v-if="slotProps.item.position_name">
                                                            {{ slotProps.item.position_name || "" }}
                                                        </div>
                                                        <div v-if="slotProps.item.department_name">
                                                            {{ slotProps.item.department_name || "" }}
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </template>

                                </Toolbar>
                            </template>
                        </OrderList>
                    </div>
                </div>
                <div class="col-12 md:col-12">
                    <div class="form-group">
                        <label>Nội dung</label>
                        <Textarea v-model="process.content" :autoResize="true" rows="4" cols="30" />
                    </div>
                </div>
                <div class="col-12 md-col-12">
                    <div class="form-group">
                        <label>Tệp đính kèm</label>
                        <FileUpload 
                            chooseLabel="Chọn tệp" 
                            :showUploadButton="false" 
                            :showCancelButton="true"
                            :multiple="false" 
                            :maxFileSize="104857600" 
                            @select="onUploadFile" 
                            @remove="removeFile"
                            :invalidFileSizeMessage="'{0}: Dung lượng File không được lớn hơn {1}'"
                        >
                            <template #empty>
                                <p class="p-0 m-0 text-500">Kéo thả hoặc chọn File.</p>
                            </template>
                        </FileUpload>
                    </div>
                </div>
            </div>
        </form>
        <template #footer>
            <Button label="Hủy" icon="pi pi-times" @click="closeDialog()" class="p-button-outlined" />
            <Button label="Gửi" icon="pi pi-send" @click="send()" />
        </template>
    </Dialog>
</template>
<style scoped>
    @import url(../style_request.css);
</style>
<style lang="scss" scoped>
    ::v-deep(.d-lang-table) {
        .p-datatable-thead .justify-content-center .p-column-header-content {
            justify-content: center !important;
        }

        .p-datatable-table {
            position: absolute;
        }

        .p-datatable-thead {
            position: sticky;
            top: 0;
            z-index: 1;
        }
    }

    ::v-deep(.form-group) {

        .p-multiselect .p-multiselect-label,
        .p-dropdown .p-dropdown-label {
            height: 100%;
            display: flex;
            align-items: center;
        }

        .p-chip img {
            margin: 0;
        }

        .p-avatar-text {
            font-size: 1rem;
        }
    }

    ::v-deep(.avatar-item) {
        .p-avatar.p-avatar-lg {
            width: 3rem;
            height: 3rem;
        }
    }

    ::v-deep(.is-close) {
        .p-panel-header {
            color: red;
        }
    }

    ::v-deep(.order-approver-request) {
        .p-orderlist-list {
            max-height: 18rem;
        }
        .p-orderlist-list .p-orderlist-item {
            padding: 0.5rem;
        }
        .p-orderlist-list .p-orderlist-item:hover {
            background: #e9ecef;
            color: #495057;
        }
    }
</style>
