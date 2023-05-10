<script setup>
import { onMounted, inject, ref } from "vue";
import { encr } from "../../util/function";
import { useToast } from "vue-toastification";
import { useRoute } from "vue-router";
import moment from "moment";
import dialogAddRequest from "../request/component_request/dialog_add_request.vue";
import DetailedRequest from "../request/component_request/detail_request.vue";
import toolbarSearchRequest from "../request/component_request/toolbar_search_request.vue";
import dialogSend from "../request/component_request/dialog_send.vue";

const router = inject("router");
const route = useRoute();
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const emitter = inject("emitter");
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();
const cryoptojs = inject("cryptojs");

const tabs = ref([
    { status: 100, title: "Tất cả", icon: "", total: 0 },
    { status: 1, title: "Mới lập", icon: "", total: 0 },
    { status: 2, title: "Chờ duyệt", icon: "", total: 0 },
    { status: 3, title: "Đã duyệt", icon: "", total: 0 },
    { status: 4, title: "Từ chối", icon: "", total: 0 },
    { status: 5, title: "Hoàn thành", icon: "", total: 0 },
    { status: 6, title: "Quá hạn", icon: "", total: 0 },
]);
const tabOthers = ref([
    {   label: "Phê duyệt đề xuất", icon: "pi pi-check-circle", 
        items: [
            { status: 8, label: "Phê duyệt đúng hạn", icon: "", total: 0, command: (event) => { activeTab(event); }, },
            { status: 9, label: "Hoàn thành đúng hạn", icon: "", total: 0, hasBreak: true },
            { status: 10, label: "Chờ duyệt quá hạn", icon: "", total: 0 },
            { status: 11, label: "Hoàn thành quá hạn", icon: "", total: 0, hasBreak: true },
            { status: 12, label: "Đề xuất xử lý gấp", icon: "", total: 0 },
            { status: 13, label: "Đề xuất đã tạo công việc", icon: "", total: 0 },
            { status: 14, label: "Đề xuất đã gia hạn duyệt", icon: "", total: 0, hasBreak: true },
            { status: 15, label: "Quản lý", icon: "", total: 0 },
            { status: 16, label: "Theo dõi", icon: "", total: 0 },
        ] 
    },            
    {   label: "Thực hiện đề xuất", icon: "pi pi-list", 
        items: [
            { status: 17, label: "Đang thực hiện", icon: "", total: 0 },
            { status: 18, label: "Chờ đánh giá", icon: "", total: 0 },
            { status: 19, label: "Đã đánh giá", icon: "", total: 0 },
            { status: 20, label: "Tạm dừng", icon: "", total: 0 },
        ] 
    },
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
const options = ref({
    loading: true,
    search: "",
    pageNo: 1,
    pageSize: 20,
    total: 0,
    tab: 100,
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

const isFilter = ref(false);
const isFirst = ref(true);
const datas = ref([]);
const listRequest = (rf) => {
    if (isFilter.value) {
        initDataFilter();
        return;
    }
    swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
    datas.value = [];
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
                        // fake data
                        // item.Tiendo = 20;
                        // item.status_processing = 3;
                        // item.evaluated_score = 4;
                        // item.evaluated_date = new Date();
                        // end fake data
                        if (item.listSignUser != null) {
                            item.listSignUser = JSON.parse(item.listSignUser);
                            if (item.listSignUser.length > 0) {
                                item.listSignUser.forEach((su) => {
                                    su.status = su.status == '1'; // Trạng thái nhận
                                    su.is_type = parseInt(su.is_type);
                                    su.is_order = parseInt(su.is_order);
                                });
                            }
                        }
                        else {
                            item.listSignUser = [];
                        }
                    });
                    datas.value = data[0];
                    options.value.is_func = datas.value.filter(x => x.is_func && (x.status == 1 || x.status == 0 || x.status == -1 || x.status == 3)).length > 0;
                    if (data[1] != null && data[1].length > 0) {
                        options.value.total = data[1][0].total;
                    }
                } else {
                    options.value.total = 0;
                }
            }
        }
        if (isFirst.value) isFirst.value = false;
        swal.close();
        if (options.value.loading) options.value.loading = false;
    })
    .catch((error) => {
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
};
const initDataFilter = () => {

};

const filterTab = ref();
const activeTab = (tab, event) => {
    if (tab.status != null) {
        options.value.tab = tab.status;
        if (tab.status > 7) {
            filterTab.value.toggle(event);
        }
        listRequest(true);
    }
};
const search = () => {
    options.value.pageNo = 1;
    countRequest();
    listRequest(true);
};

const filter = (event) => {
    //opfilter.value.toggle(event);
    isFilter.value = true;
    countRequest();
    listRequest(true);
};
/*
const opfilter = ref();
const toggleFilter = (event) => {
    opfilter.value.toggle(event);
};
const deadlineRequest = ref([
    //{ name: "Tất cả đề xuất", code: -1 },
    { name: "Đề xuất sắp đến hạn duyệt", code: 0 },
    { name: "Đề xuất duyệt đúng hạn", code: 1 },
    { name: "Đề xuất quá hạn duyệt", code: 2 },
]);

const userCreated = ref([
    { name: "Tất cả", code: -1 },
    { name: "Tôi lập", code: 0 },
]);
const list_roles = ref([
    { name: 'Người tạo'     , code: 1 },
    { name: 'Người theo dõi', code: 2 },
    { name: 'Người quản lý' , code: 3 },
    { name: 'Người duyệt'   , code: 4 },
]);
const list_status = ref([
    { name: 'Mới tạo'   , code: 0 },
    { name: 'Đang trình', code: 1 },
    { name: 'Chấp thuận', code: 2 },
    { name: 'Thu hồi'   , code: 3 },
    { name: 'Hủy'       , code: 4 },
    { name: 'Trả lại'   , code: 5 },
]);
*/
const changeDate = () => {};
const removeFilter = (idx, array, isTree) => {
    if (isTree) {
        array[idx["key"]]["checked"] = false;
    } else {
        array.splice(idx, 1);
    }
};

const resetFilter = () => {
    options.value.organizations = [];
    options.value.teams = [];
    options.value.roles = [];
    options.value.type_form_requests = [];
    options.value.status_overdue = [];
    options.value.status_requests= [];
    options.value.created_by = null;
};

const toggleTabs = (event) => {
    filterTab.value.toggle(event);
};

const dictionarys = ref();
const listTypeRequest = ref([]);
const listTypeRequestFilter = ref([]);
const initDictionary = () => {
  axios
    .post(
        baseURL + "/api/request/getData",
        {
            str: encr(
                JSON.stringify({
                    proc: "request_dictionary_by_user",
                    par: [{ par: "user_id", va: store.getters.user.user_id }],
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
                let dataDictionary = JSON.parse(data);
                dictionarys.value = dataDictionary;
                let listGroupRequest = [];
                if (dataDictionary.length > 0) {
                    dataDictionary[0].forEach((el) => {
                        let idxGr = listGroupRequest.findIndex(x => x.label == el.request_group_name);
                        if (idxGr < 0) {
                            listGroupRequest.push({ 
                                label: el.request_group_name, 
                                items: [ { label: el.request_form_name, request_form_id: el.request_form_id } ] 
                            });
                        }
                        else {
                            listGroupRequest[idxGr].items.push({ 
                                label: el.request_form_name, 
                                request_form_id: el.request_form_id 
                            });
                        }
                    });
                    //dictionarys.value[0] = listGroupRequest;
                    listTypeRequest.value = listGroupRequest;
                    listTypeRequestFilter.value = [...listGroupRequest];
                    listTypeRequestFilter.value.unshift({ 
                        label: "Trực tiếp", 
                        items: [{ label: "Đề xuất trực tiếp", request_form_id: null }] 
                    });
                }
            }
        }
    });
};

const countRequestTabs = ref([]);
const countRequest = () => {
    let list_status_overdue = "";
    if (options.value.status_overdue.length > 0) {
        options.value.status_overdue.forEach((x) => {
            list_status_overdue += ("," + x.code);
        });
        list_status_overdue = list_status_overdue.substring(1);
    }
    let list_status_requests = "";
    if (options.value.status_requests.length > 0) {
        options.value.status_requests.forEach((x) => {
            list_status_requests += ("," + x.code);
        });
        list_status_requests = list_status_requests.substring(1);
    }
    axios
        .post(
            basedomainURL + "/api/request/getData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "request_count_by_user",
                        par: [
                            { par: "user_id", va: store.getters.user.user_id },
                            // { par: "search", va: options.value.search },
                            { par: "request_form_id", va: options.value.request_form_id },
                            { par: "status_overdue", va: list_status_overdue },
                            { par: "status_request", va: list_status_requests },
                            { par: "is_in", va: options.value.is_in || false },
                            { par: "is_out", va: options.value.is_out || false },
                            { par: "is_me", va: options.value.is_me || false },
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
            let data = response.data.data;
            if (data != null) {
                let dataConvert = JSON.parse(data);
                countRequestTabs.value = dataConvert[0];
                tabs.value.forEach((el) => {
                    let statusExist = countRequestTabs.value.find(x => x.status_value == el.status);
                    if (statusExist != null) {
                        el.total = statusExist.count;
                    }
                });
                tabOthers.value.forEach((el) => {
                    let statusExist = countRequestTabs.value.find(x => x.status_value == el.status);
                    if (statusExist != null) {
                        el.total = statusExist.count;
                    }
                });
            }
        }
    })
    .catch((error) => {
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
};
const request_data = ref({
    request_name: "",
    content: "",
    created_date: new Date(),
    priority_level: 0,
    type_process: 1,
    is_evaluate: true,
    IsEdit: true,
});
const headerAddDialog = ref();
const displayAddRequest = ref(false);
const closeDialog = () => {
    request_data.value = {
        request_name: "",
        content: "",
        created_date: new Date(),
        priority_level: 0,
        type_process: 1,
        is_evaluate: true,
        request_team_id: dictionarys.value[2].length > 0 ? dictionarys.value[2][0].request_team_id : null,
        IsEdit: true,
    };
    displayAddRequest.value = false;
    forceRerenderForm();
};
const cpnAddRequest = ref(0);
const forceRerenderForm = () => {
	cpnAddRequest.value += 1;
};

const listFilesRequest = ref([]);
const listUserApproved = ref([]);
const listUserFollow = ref([]);
const listUserManage = ref([]);
const detailFormDynamic = ref([]);
const openAddDialog = (title) => {
    headerAddDialog.value = title;
    request_data.value = {
        request_name: "",
        content: "",
        created_date: new Date(),
        priority_level: 0,
        type_process: 1,
        is_evaluate: true,
        request_team_id: dictionarys.value[2].length > 0 ? dictionarys.value[2][0].request_team_id : null,
        IsEdit: true,
    };
    listFilesRequest.value = [];
    listUserApproved.value = [];
    listUserFollow.value = [];
    listUserManage.value = [];
    detailFormDynamic.value = [];
    displayAddRequest.value = true;
    forceRerenderForm();
};

// component request
emitter.on("SideBarRequest", (obj) => {
    showDetailRequest.value = false;
    selectedRequestID.value = null;
});
const PositionSideBar = ref('right');
emitter.on("psbRequest", (obj) => {
    PositionSideBar.value = obj;
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
        listRequest(true);
    }
};
const openViewRequest = (dataRequest) => {
    forceRerenderDetail();
    showDetailRequest.value = true;
    selectedRequestID.value = dataRequest.request_id;
};
// ---

const editRequest = (dataRequest) => {
    listFilesRequest.value = [];
    listUserApproved.value = [];
    listUserFollow.value = [];
    listUserManage.value = [];
    detailFormDynamic.value = [];
    axios
        .post(
        baseURL + "/api/request/getData",
        {
            str: encr(
                JSON.stringify({
                    proc: "request_get",
                    par: [
                        { par: "request_id", va: dataRequest.request_id },
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
            let data = response.data.data;
            if (data != null) {
                let dataConvert = JSON.parse(data);
                request_data.value = dataConvert[0][0];
                request_data.value.request_form_id = request_data.value.request_form_id != null ? { label: request_data.value.request_form_name, request_form_id: request_data.value.request_form_id} : null;
                listFilesRequest.value = dataConvert[1];
                listUserApproved.value = dataConvert[2];
                listUserManage.value = dataConvert[3];
                listUserFollow.value = dataConvert[4];
                detailFormDynamic.value = dataConvert[5];
                if (request_data.value.created_date != null) {
                    request_data.value.created_date = new Date(request_data.value.created_date);
                }
                request_data.value.Radio = true;
                headerAddDialog.value = "Cập nhật đề xuất";
                displayAddRequest.value = true;
                forceRerenderForm();
            }
        }
    })
    .catch((error) => {
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
};

const deleteRequest = (item) => {
    swal
        .fire({
            title: "Thông báo",
            text: "Bạn có muốn xoá đề xuất này không!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Có",
            cancelButtonText: "Không",
    })
    .then((result) => {
        if (result.isConfirmed) {
            swal.fire({
                width: 110,
                didOpen: () => {
                    swal.showLoading();
                },
            });

            axios
                .delete(baseUrlCheck + "/api/request/Delete_Request", {
                    headers: { Authorization: `Bearer ${store.getters.token}` },
                    data: item != null ? [item.request_id] : "-1",
                })
                .then((response) => {
                    swal.close();
                    if (response.data.err != "1") {
                        swal.close();
                        toast.success("Xoá đề xuất thành công!");
                        options.value.PageNo = 1;
                        listRequest(true);
                        countRequest();
                    } else {
                        swal.fire({
                            title: "Thông báo",
                            text: "Xảy ra lỗi khi xóa đề xuất",
                            icon: "error",
                            confirmButtonText: "OK",
                        });
                    }
                })
                .catch((error) => {
                    swal.close();
                    if (error.status === 401) {
                        swal.fire({                            
                            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",                            
                            confirmButtonText: "OK",
                        });
                    }
                });
        }
    });
};

const refresh = () => {
    options.value = {
        loading: true,
        search: "",
        pageNo: 1,
        pageSize: 20,
        total: 0,
        tab: 100,
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
    };
    isFirst.value = true;
    selectedNodes.value = [];
    isFilter.value = false;
    listRequest(true);
    countRequest();
};
const request_select = ref();
const menuButMores = ref();
const headerSend = ref("");
const displaySend = ref(false);
const dataSelected = ref([]);
const cpnSendRequest = ref(0);
const forceRerenderSend = () => {
	cpnSendRequest.value += 1;
};
const closeDialogSend = () => {
    //listRequest(true);
    dataSelected.value = [];
    displaySend.value = false;
};
const modelsend = ref({
    type_send: 0,
    type_module: 0,
    module_key:"M12"
});
const itemButMores = ref([
    {
        label: "Chuyển đến quy trình",
        icon: "pi pi-chart-line",
        command: (event) => {
            headerSend.value = "Chuyển đến quy trình";
            modelsend.value.type_send = 0;
            displaySend.value = true;
            forceRerenderSend();
        },
    },
    {
        label: "Chuyển đến nhóm",
        icon: "pi pi-users",
        command: (event) => {
            headerSend.value = "Chuyển đến nhóm";
            modelsend.value.type_send = 1;
            displaySend.value = true;
            forceRerenderSend();
        },
    },
    {
        label: "Chuyển đích danh",
        icon: "pi pi-user-edit",
        command: (event) => {
            headerSend.value = "Chuyển đích danh";
            modelsend.value.type_send = 2;
            displaySend.value = true;
            forceRerenderSend();
        },
    },
    {
        label: "Chỉnh sửa",
        icon: "pi pi-pencil",
        statusDisplay: 0,
        command: (event) => {
            editRequest(request_select.value);
        },
    },
    {
        label: "Xoá",
        icon: "pi pi-trash",
        class: "red-text",
        statusDisplay: 0,
        command: (event) => {
            deleteRequest(request_select.value);
        },
    },
]);
const itemsFuncByStatus = () => {
    if (request_select.value != null && request_select.value.status_processing != 0) {
        return itemButMores.value.filter(x => x.statusDisplay != 0);
    }
    return itemButMores.value;
};
const selectedNodes = ref({});
const toggleMores = (event, item) => {
    request_select.value = item;
    dataSelected.value = [];
    dataSelected.value.push(item);
    menuButMores.value.toggle(event);
    selectedNodes.value = item;
    options.value["filterRequest_id"] = selectedNodes.value["request_id"];
};

onMounted(() => {
    listRequest(true);
    countRequest();
    initDictionary();
    return {
        filterTab,
        displayAddRequest,
    }
});
</script>
<template>
	<div class="surface-100 p-2">
        <Toolbar class="outline-none surface-0 border-none">
            <template #start>
                <toolbarSearchRequest
                    :options="options"
                    :dictionarys="dictionarys"
                    :search="search"
                    :resetFilter="resetFilter"
                    :filter="filter"
                ></toolbarSearchRequest>
            </template>
            <template #end>
                <Button
                    @click="openAddDialog('Thêm mới đề xuất')"
                    label="Thêm mới"
                    icon="pi pi-plus"
                    class="mr-2"
                />
                <!-- <Button
                    icon="pi pi-trash"
                    label="Xóa"
                    class="p-button-danger mr-2"
                    @click="deleteRequest()"
                /> -->
                <Button
                    @click="refresh()"
                    class="p-button-outlined p-button-secondary mr-2"
                    icon="pi pi-refresh"
                    label="Tải lại"
                />              
            </template>
        </Toolbar>
        <div class="tabview">
            <div class="tableview-nav-content">
                <ul class="tableview-nav">
                    <li class="tableview-header"
                        :class="{ highlight: options.tab === tab.status }"
                        v-for="(tab, key) in tabs"
                        :key="key"
                        @click="activeTab(tab, $event)"
                    >
                        <a>
                            <i :class="tab.icon"></i>
                            <span>{{ tab.title }} ({{ tab.total }})</span>
                        </a>
                    </li>
                    <li class="tableview-header"
                        :class="{ highlight: options.tab != 100 && options.tab > 6 }"
                        style="padding:0;"
                    >
                        <Button class="p-button-text p-0"
                            @click="toggleTabs($event)" 
                            aria-haspopup="true" 
                            aria-controls="overlay_menutab"
                            style="color:#495057;font-weight:bold;padding:1.25rem !important;"
                        >
                            <span class="pr-1">Khác</span>
                            <i class="pi pi-angle-down"></i>
                        </Button>
                        <Menu ref="filterTab" 
                            id="overlay_menutab" 
                            :model="tabOthers" 
                            :popup="true"
                            style="height: 325px; overflow-y: auto;"
                        >
                            <template #item="slotProps">
                                <div :class="{ 'sub-menu-tab': slotProps.item.status != null, 
                                        'sub-menu-grouptab': slotProps.item.status == null, 
                                        'active-menu-tab': slotProps.item.status == options.tab 
                                    }"
                                    @click="activeTab(slotProps.item, $event)">
                                    <span class="">
                                        <i :class="slotProps.item.icon" v-if="slotProps.item.icon != ''"></i>
                                        {{ slotProps.item.label }}
                                    </span> 
                                    <span class="pl-1" v-if="slotProps.item.status != null">({{ slotProps.item.total }})</span>
                                </div>
                                <div class="divider" v-if="slotProps.item.hasBreak"></div>
                            </template>
                        </Menu>
                    </li>
                </ul>
            </div>
        </div>
        <div class="d-lang-table">
            <DataTable
                class="table-request-data"
                :value="datas"
                :scrollable="true"
                selectionMode="single"
                dataKey="request_id"
                scrollHeight="calc(100vh - 170px)"
                :rowHover="true"                
                v-model:selection="selectedNodes"
                @rowSelect="openViewRequest($event.data)"
            >
                <Column
                    field="request_code"
                    header="Mã số"
                    headerStyle="text-align:center;max-width:150px;height:45px"
                    bodyStyle="text-align:center;max-width:150px;"  
                    class="align-items-center justify-content-center text-center"
                >
                    <template #body="slotProps">
                        <div class="flex" 
                            :class="slotProps.data.status != 2 && slotProps.data.is_overdue && slotProps.data.Deadline && slotProps.data.SoNgayHan <= 24 ? 'overdue-request' : ''"
                            style="flex-direction: column;">
                            <span style="word-break: break-all;">{{ slotProps.data.request_code }}</span>
                            <div class="mt-2" v-if="slotProps.data.status_processing == 3">
                                <Rating class="star-rating-custom"
                                    v-model="slotProps.data.evaluated_score"
                                    v-tooltip.top="{ value: 'Ngày đánh giá: <br/>' + (slotProps.data.evaluated_date ? moment(new Date(slotProps.data.evaluated_date)).format('HH:mm DD/MM/yyyy') : ''), escape: true }"
                                    :stars="5"
                                    :cancel="false" 
                                    :readonly="true"
                                />
                            </div>
                        </div>
                    </template>
                </Column>
                <Column
                    field="request_name"
                    header="Tên đề xuất"
                    headerStyle="text-align:left;height:45px"
                    bodyStyle="text-align:left;"
                    class="align-items-center"
                >
                    <template #body="slotProps">
                        <div class="flex" style="flex-direction: column;">
                            <div class="flex" style="align-items: baseline;">
                                <span class="uutien mr-2" 
                                    :class="'uutien' + (slotProps.data.is_security||0)" 
                                    v-if="slotProps.data.is_security > 0"
                                >
                                    {{ slotProps.data.priority_level == 1 ? 'Gấp' : 'Rất gấp' }}
                                </span>
                                <span class="uutien mr-2" v-tooltip.top="'Bảo mật'"
                                    v-if="slotProps.data.is_security">
                                    <i class="pi pi-flag" style="color:red;"></i>
                                </span>
                                <span class="card-nhom flex text-left" 
                                    style="padding:0.25rem 0.5rem;background-color: #ff8b4e;color: #fff;margin-right: 0.5rem !important;" 
                                    v-if="slotProps.data.is_change_process"
                                >
                                    Quy trình động
                                </span>
                                <span class="flex font-bold"
                                    style="flex:1;"
                                    :style="slotProps.data.is_overdue ? 'color:red !important;' : ''"
                                >
                                    {{ slotProps.data.request_name }}
                                </span>                                
                            </div>
                            <div class="flex mt-2" style="align-items: center;">
                                <span style="padding:0.25rem 0.5rem;background-color: antiquewhite;white-space: normal;" 
                                    class="card-nhom flex text-left">
                                    {{ slotProps.data.last_name + " - " + (slotProps.data.request_form_name || 'Đề xuất trực tiếp') }}
                                </span>
                                <span class="flex ml-2 span-note-request" v-if="slotProps.data.is_overdue" 
                                    v-tooltip.top="{ value: 'Hạn xử lý: ' + 
                                        (slotProps.data.Deadline ? moment(new Date(slotProps.data.Deadline)).format('HH:mm DD/MM/yyyy') : ''), escape: true }" 
                                    style="color:red;">
                                    <i style="font-size:12px" class="pi pi-clock"></i> 
                                    <span class="pl-1">
                                        ({{ slotProps.data.SoNgayHan || 0 }}h)
                                    </span>
                                </span>
                                <span class="flex ml-2 span-note-request" v-if="!slotProps.data.is_overdue && slotProps.data.Deadline" 
                                    v-tooltip.top="{ value: 'Hạn xử lý: ' + 
                                        (slotProps.data.Deadline ? moment(new Date(slotProps.data.Deadline)).format('HH:mm DD/MM/yyyy') : ''), escape: true }" 
                                    :style="slotProps.data.SoNgayHan <= 24 ? 'color:orange' : 'color:#333'">
                                    <i style="font-size:12px" class="pi pi-clock"></i> 
                                    <span class="pl-1">
                                        ({{ slotProps.data.SoNgayHan || 0 }}h)
                                    </span>
                                </span>
                                <span class="flex ml-2 span-note-request" v-if="(slotProps.data.SoFile > 0)">
                                    <i style="font-size:12px" class="pi pi-paperclip"></i> 
                                    <span class="pl-1" v-tooltip.top="'File đính kèm'">
                                        ({{slotProps.data.SoFile || 0}})
                                    </span>
                                </span>
                                <span class="flex ml-2 span-note-request" v-if="(slotProps.data.SoChat > 0)">
                                    <i v-tooltip.top="'Số thảo luận'" style="font-size:12px" class="pi pi-comment"></i>
                                    <span class="pl-1" v-tooltip.top="'Số thảo luận'">
                                        ({{ slotProps.data.SoChat || 0 }})</span>
                                    <Badge class="flex ml-1"
                                        style="align-items: center;justify-content: center;width:1.25rem;height:1.25rem;"
                                        v-tooltip.top="'Số thảo luận chưa đọc'"
                                        :value="slotProps.data.SoSendhub || 0"
                                        v-if="(slotProps.data.SoSendhub > 0)"
                                    ></Badge>
                                </span>
                                <span class="flex ml-2 span-note-request" 
                                    style="color:#2196f3 !important;" 
                                    v-if="(slotProps.data.Stask > 0)">
                                    <font-awesome-icon icon="fa-solid fa-list-check" 
                                        style="font-size: 12px; display: block; color: #2196f3"
                                    />
                                    <span class="pl-1">
                                        ({{ (slotProps.data.StaskFinish || 0) + "/" + (slotProps.data.Stask || 0) }})
                                    </span>
                                    <span v-tooltip.top="'Tiến độ công việc: ' + slotProps.data.StaskTiendo + '%'" 
                                        class="radialProgressBar pl-1"
                                        :class="'progress-' + slotProps.data.bgtiendo" 
                                    >
                                        <span class="overlay" style="color:#000">
                                            {{slotProps.data.StaskTiendo || 0}}%
                                        </span>
                                    </span>
                                </span>
                            </div>
                        </div>
                    </template>
                </Column>
                <Column
                    field="created_date"
                    header="Ngày tạo"
                    headerStyle="text-align:center;max-width:140px;height:45px"
                    bodyStyle="text-align:center;max-width:140px;"
                    class="align-items-center justify-content-center text-center"
                >
                    <template #body="slotProps">
                        <div class="flex" style="flex-direction: column;">
                            <span>{{ moment(slotProps.data.created_date).format("DD/MM/YYYY") }}</span>
                            <div class="mt-2" style="vertical-align:middle;width:140px" 
                                v-if="slotProps.data.Tiendo && slotProps.data.Tiendo > 0"
                            >
                                <ProgressBar class="progress-bar-custom" 
                                    :class="renderColorProgress(slotProps.data.Tiendo)"
                                    :value="(slotProps.data.Tiendo || 0)"
                                ></ProgressBar>
                            </div>
                        </div>
                    </template>
                </Column>
                <Column
                    field="created_by"
                    header="Người tạo"
                    headerStyle="text-align:center;max-width:120px;height:45px"
                    bodyStyle="text-align:center;max-width:120px;"
                    class="align-items-center justify-content-center text-center"
                >
                    <template #body="slotProps">
                        <div class="relative">
                            <Avatar
                                v-bind:label="
                                    slotProps.data.avatar
                                    ? ''
                                    : (slotProps.data.last_name ?? '').substring(0, 1).toUpperCase()
                                "
                                v-bind:image="
                                    slotProps.data.avatar
                                    ? basedomainURL + slotProps.data.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                v-tooltip.top="{ value: (slotProps.data.full_name + '<br/>' + slotProps.data.position_name + '<br/>' + slotProps.data.department_name), escape: true }"
                                style="color: #ffffff; width: 2rem; height: 2rem; font-size: 1rem !important;"
                                :style="{ background: bgColor[slotProps.index % 7], }"
                                size="xlarge"
                                shape="circle"
                                class="border-radius"
                            />
                        </div>
                    </template>
                </Column>
                <Column
                    field="listSignUser"
                    header="Người duyệt"
                    headerStyle="text-align:center;max-width:250px;height:45px"
                    bodyStyle="text-align:center;max-width:250px;"
                    class="align-items-center justify-content-center text-center"
                >
                    <template #body="slotProps">
                        <div class="relative">
                            <AvatarGroup>                            
                                <Avatar v-for="(us, idxUser) in slotProps.data.listSignUser" :key="idxUser"
                                    v-bind:label="us.avatar ? '' : (us.last_name ?? '').substring(0, 1)"
                                    v-bind:image="
                                        us.avatar
                                        ? basedomainURL + us.avatar
                                        : basedomainURL + '/Portals/Image/noimg.jpg'
                                    "
                                    v-tooltip.top="{ value: (us.full_name + '<br/>' + us.position_name + '<br/>' + us.department_name), escape: true }"
                                    style="background-color: #2196f3; color: #ffffff; width: 2rem; height: 2rem; font-size: 1rem !important;"
                                    :style="{ background: bgColor[idxUser % 7], }"
                                    class="text-avatar"
                                    size="xlarge" 
                                    shape="circle" 
                                >
                                    <template #body="">
                                        <span v-if="us.status" class="is-sign">
                                            <font-awesome-icon icon="fa-solid fa-circle-check" 
                                                style="font-size: 16px; display: block; color: #f4b400"
                                            />
                                        </span>
                                    </template>
                                </Avatar>
                            </AvatarGroup>
                        </div>
                    </template>
                </Column>
                <Column
                    field="status"
                    header="Trạng thái"
                    headerStyle="text-align:center;max-width:180px;height:45px"
                    bodyStyle="text-align:center;max-width:180px;"
                    class="align-items-center justify-content-center text-center"
                >
                    <template #body="slotProps">
                        <Chip class="status_request"
                            :class="slotProps.data.objStatus.class || ''"
                            v-bind:label="slotProps.data.objStatus.text || ''"
                            style="border-radius: 5px !important;"
                        />
                    </template>
                </Column>
                <Column
                    header=""
                    headerStyle="text-align:center;max-width:50px"
                    bodyStyle="text-align:center;max-width:50px"
                    class="align-items-center justify-content-center text-center"
                    v-if="options.is_func"
                >
                    <template #body="slotProps">
                        <Button
                            icon="pi pi-ellipsis-h"
                            class="p-button-rounded p-button-text"
                            @click="toggleMores($event, slotProps.data)"
                            aria-haspopup="true"
                            aria-controls="overlay_More"
                            v-tooltip.top="'Tác vụ'"
                        />
                    </template>
                </Column>
                <template #empty>
                    <div
                        class="align-items-center justify-content-center p-4 text-center m-auto"
                        :style="{
                            display: 'flex',
                            width: '100%',
                            height: 'calc(100vh - 230px)',
                            backgroundColor: '#fff',
                        }"
                    >
                        <div v-if="options.total == 0">
                            <img src="../../assets/background/nodata.png" height="144" />
                            <h3 class="m-1">Không có dữ liệu</h3>
                        </div>
                    </div>
                </template>
            </DataTable>
        </div>
    </div>
    <Menu
        class="menu-request"
        id="overlay_More"
        ref="menuButMores"
        :model="itemsFuncByStatus()"
        :popup="true"
    />
    <Sidebar
        class="sidebar-request"
        v-model:visible="showDetailRequest"
        :position="'right'"
        :style="{
            width: PositionSideBar == 'right' ? (widthWindow > 1800 ? '65vw' : '75vw') : '100%',
            'min-height': '100vh !important',
        }"
        :baseZindex="1000"        
		:autoZIndex="true"
        :showCloseIcon="false"
        @hide="hideall()"
        >
        <DetailedRequest
            :isShow="showDetailRequest"
            :id="selectedRequestID"
            :key="cpnDetailRequest"
            :listStatusRequests="listStatusRequests"
        >
        </DetailedRequest>
    </Sidebar>
    <dialogAddRequest
        v-if="displayAddRequest == true"
        :key="cpnAddRequest"
		:headerDialog="headerAddDialog"
		:displayDialog="displayAddRequest"
		:dataForm="request_data"
        :dictionarys="dictionarys"
        :listFiles="listFilesRequest"
        :listTypeRequest="listTypeRequest"
        :listUserApproved="listUserApproved"
        :listUserFollow="listUserFollow"
        :listUserManage="listUserManage"
        :detailFormDynamic="detailFormDynamic"
		:reloadData="listRequest"
		:closeDialog="closeDialog"
    ></dialogAddRequest>
    <dialogSend
        v-if="displaySend == true"
        :key="cpnSendRequest"
        :headerDialog="headerSend"
        :displayDialog="displaySend"
        :dataSelected="dataSelected"
        :modelsend="modelsend"
        :closeDialog="closeDialogSend"
    />
</template>
<style scoped>
@import url(style_request.css);
</style>
<style lang="scss" scoped>
    ::v-deep(.custom-multiselect) {
        .p-multiselect-label {
            padding: 0 0.5rem;
        }
        .p-multiselect-label.p-placeholder {
            min-height: 34px;
            align-items: center;
            display: flex;
        }
    }
    ::v-deep(.table-request-data) {
		.p-datatable-emptymessage td {
			padding: 0 !important;
		}
        td:has(div.overdue-request) {
            border-left: 4px solid red;
        }
        .p-datatable-thead {
            position: sticky;
            top: 0;
            z-index: auto;
        }
	}
    ::v-deep(.progress-bar-custom) {
        .p-progressbar-label {
            font-size: 0.75rem;
        }
    }
    ::v-deep(.star-rating-custom.p-rating) {
        .p-rating-icon.pi-star-fill {
            color: orange;
        }
    }
    ::v-deep(.p-menuitem) {
        .red-text {
            color: red;
        }
    }
</style>