<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../util/function.js";
import moment from "moment";
import dialogAddFormBackup from "../backup/component/dialog_add_backup_schedule.vue";

const toast = useToast();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const cryoptojs = inject("cryptojs");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const option = ref({
    loading: false,
    search: "",
    PageNo: 0,
    PageSize: 20,
    organization_type: store.getters.user.organization_id,
    user_id: store.getters.user.user_id,
    totalRecords: 0,
    status: -1,
});
const first = ref(0);

const backup_schedule = ref({
    title: "",
    folder_backup_path: "",
    is_active: false,
});
const datalists = ref([]);
const loadData = (rf) => {
    if (rf) {
        option.value.loading = true;
    }
    swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
    axios
        .post(
            baseURL + "/api/Backup/getData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "backup_schedule_list",
                        par: [
                        { par: "pageno", va: option.value.PageNo },
                        { par: "pagesize", va: option.value.PageSize },
                        { par: "search", va: option.value.search },
                        { par: "user_id", va: store.getters.user.user_id },
                        { par: "status", va: option.value.status },
                        ],
                    }), SecretKey, cryoptojs,
                ).toString(),
            },
            config,
        )
        .then((response) => {
            swal.close();
            let data = JSON.parse(response.data.data);
            if (data.length > 0) {
                data[0].forEach((el, idx) => {
                    el.is_order = idx + 1 + option.value.PageNo * option.value.PageSize;
                });
                datalists.value = data[0];
                option.value.totalRecords = data[1][0].totalRecords;
            } else {
                datalists.value = [];
                option.value.totalRecords = 0;
            }
            if (rf) {
                option.value.loading = false;
            }
        })
        .catch((error) => {
            swal.close();
            if (error && error.status === 401) {
                swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
                });
            }
        });
};

const searchSchedule = () => {
    option.value.PageNo = 0;
    loadData(true);
};
const onPage = (event) => {
    if (event.rows != option.value.PageSize) {
        option.value.PageSize = event.rows;
    }    
    if (event.page == 0) {
        //Trang đầu
        option.value.id = null;
        option.value.IsNext = true;
    } else if (event.page > option.value.PageNo + 1) {
        //Trang cuối
        option.value.id = -1;
        option.value.IsNext = false;
    } else if (event.page > option.value.PageNo) {
        //Trang sau
        option.value.id = datalists.value[datalists.value.length - 1].place_id;
        option.value.IsNext = true;
    } else if (event.page < option.value.PageNo) {
        //Trang trước
        option.value.id = datalists.value[0].place_id;
        option.value.IsNext = false;
    }
    option.value.PageNo = event.page;
    loadData(true);
};
const filterSchedules = (event) => {
    styleObj.value = style.value;
    loadData(true);
};
const op = ref();
const toggle = (event) => {
    op.value.toggle(event);
};
const selectedBackup = ref([]);
const styleObj = ref();
const style = ref({
  "background-color": "#2196F3 !important",
  "color": "#fff !important",
  " border": "1px solid #5ca7e3 !important",
});
const trangThai = ref([
  { name: "Tất cả", code: -1 },
  { name: "Kích hoạt", code: 1 },
  { name: "Không kích hoạt", code: 0 },
]);
const reFilter = () => {
    option.value.status = -1;
    styleObj.value = "";
};
const refresh = () => {
    option.value = {
        search: "",
        PageNo: 0,
        PageSize: 20,
        totalRecords: 0,
        status: -1,
    };
    first.value = 0;
    selectedBackup.value = [];
    styleObj.value = "";
    loadData(true);
};

const displayAddForm = ref(false);
const headerAddDialog = ref();
const openAddForm = () => {
    headerAddDialog.value = "Thêm mới thiết lập backup";
    displayAddForm.value = true;
    backup_schedule.value = {
        title: "",
        folder_backup_path: "",
        is_active: false,
    };
    forceRerenderForm();
};
const editSchedule = (dataBackup) => {
    axios
        .post(
            baseURL + "/api/Backup/getData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "backup_schedule_get",
                        par: [
                            { par: "backup_id", va: dataBackup.backup_id },
                        ],
                    }), SecretKey, cryoptojs,
                ).toString(),
            },
            config,
        )
        .then((response) => {
            let data = JSON.parse(response.data.data);
            if (data.length > 0 && data[0].length > 0) {
                if (data[0][0].time_backup) {
                    data[0][0].time_backup = new Date(data[0][0].time_backup);
                }
                backup_schedule.value = data[0][0];
            } else {
                backup_schedule.value = {
                    title: "",
                    folder_backup_path: "",
                    is_active: true,
                };
            }
            headerAddDialog.value = "Cập nhật thiết lập backup";
            displayAddForm.value = true;
            forceRerenderForm();
        })
        .catch((error) => {
            if (error && error.status === 401) {
                swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
                });
            }
        });
};
const onCheckBox = (value) => {
    let data = {
        TextID: value.backup_id + "",
        BitTrangthai: value.is_active,
    };
    if (
        store.state.user.is_super == true ||
        store.state.user.user_id == value.created_by ||
        (store.state.user.role_id == "admin" &&
        store.state.user.organization_id == value.organization_id)
    ) {
        axios
        .put(baseURL + "/api/Backup/Update_StatusFormBackup", data, config)
        .then((response) => {
            if (response.data.err != "1") {
                toast.success("Cập nhật trạng thái thành công!");
                loadData(true);
            } else {
                swal.fire({
                    title: "Thông báo",
                    text: response.data.ms,
                    icon: "error",
                    confirmButtonText: "OK",
                });
            }
        })
        .catch((error) => {
            swal.close();
            swal.fire({
                title: "Thông báo",
                text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
                icon: "error",
                confirmButtonText: "OK",
            });
        });
    } else {
        swal.fire({
            title: "Thông báo!",
            text: "Bạn không có quyền chỉnh sửa!",
            icon: "error",
            confirmButtonText: "OK",
        });
        loadData(true);
    }
};

//Xóa bản ghi
const delSchedule = (backupForm) => {
    swal.fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá lịch backup này không!",
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
            if (backupForm != null && backupForm.backup_id != null && backupForm.allowDel == 1) {
                axios
                .delete(baseURL + "/api/Backup/Delete_FormBackup", {
                    headers: { Authorization: `Bearer ${store.getters.token}` },
                    data: backupForm != null ? [backupForm.backup_id] : "",
                })
                .then((response) => {
                    swal.close();
                    if (response.data.err == "0") {
                        swal.close();
                        toast.success("Xoá lịch backup thành công!");
                        loadData(true);
                    } else {
                        swal.fire({
                            text: response.data.ms,
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
                            icon: "error",
                            confirmButtonText: "OK",
                        });
                    }
                });
            }
            else {
                swal.close();
                toast.warning("Lịch backup không thể xóa!");
            }
        }
    });
};

const isRunningBackup = ref(false);
const runScheduleBackup = (scheduleBackup) => {
    let formData = new FormData();
	formData.append("model", JSON.stringify(scheduleBackup));
    isRunningBackup.value = true;
    axios
    .post(baseURL + "api/Backup/Run_Backup",
        formData,
        config,
    )
    .then((response) => {
        toast.info("Thực hiện backup");
        isRunningBackup.value = false;
    })
    .catch((error) => {
        isRunningBackup.value = false;
        if (error && error.status === 401) {
            swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
            });
        }
    });
};

// Component
const closeDialog = () => {
	backup_schedule.value = {
		title: "",
        folder_backup_path: "",
        is_active: false,
	};
	displayAddForm.value = false;
};
const cpnAddForm = ref(0);
const forceRerenderForm = () => {
	cpnAddForm.value += 1;
};
//

onMounted(() => {
    loadData(true);
    return {
        datalists,
        loadData,
        selectedBackup,
    }
});
</script>
<template>
    <div class="main-layout flex-grow-1 p-2">
        <DataTable
            class="table-backup"
            v-model:first="first"
            :value="datalists"
            :paginator="true"
            :rows="option.PageSize"
            :scrollable="true"
            scrollHeight="flex"
            :loading="option.loading"
            v-model:selection="selectedBackup"
            :lazy="true"
            @page="onPage($event)"
            :totalRecords="option.totalRecords"
            dataKey="backup_id"
            :rowHover="true"
            :showGridlines="true"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
            :rowsPerPageOptions="[20, 30, 50, 100, 200]"
            responsiveLayout="scroll"
        >
            <template #header>
                <h3 class="module-title mt-0 ml-1 mb-2">
                <i class="pi pi-pi-bookmark"></i>
                    Danh sách lịch backup ({{ option.totalRecords }})
                </h3>

                <Toolbar class="w-full custoolbar">
                    <template #start>
                        <span class="p-input-icon-left">
                            <i class="pi pi-search" />
                            <InputText
                                v-model="option.search"
                                @keyup.enter="searchSchedule"
                                type="text"
                                spellcheck="false"
                                placeholder="Tìm kiếm"
                            />
                        </span>
                        <Button
                            type="button"
                            class="ml-2 p-button-outlined p-button-secondary"
                            icon="pi pi-filter"
                            @click="toggle"
                            aria:haspopup="true"
                            aria-controls="overlay_panel"
                            v-tooltip="'Bộ lọc'"
                            :style="[styleObj]"
                        />
                        <OverlayPanel
                            ref="op"
                            appendTo="body"
                            class="overlay-panel-setup p-0 m-0"
                            :showCloseIcon="false"
                            id="overlay_panel"
                            style="width:300px"
                        >
                            <div class="grid formgrid m-0">
                                <div class="flex field col-12 p-0">
                                    <div class="col-4 text-left pt-2 p-0"
                                        style="text-align: center,justify-content:center"
                                    >
                                        Trạng thái
                                    </div>
                                    <div class="col-8">
                                        <Dropdown
                                            class="col-12 p-0 m-0"
                                            v-model="option.status"
                                            :options="trangThai"
                                            optionLabel="name"
                                            optionValue="code"
                                            placeholder="Trạng thái"
                                        />
                                    </div>
                                </div>
                                <div class="flex col-12 p-0">
                                    <Toolbar
                                        class="border-none surface-0 outline-none pb-0 w-full"
                                    >
                                        <template #start>
                                        <Button
                                            @click="reFilter"
                                            class="p-button-outlined"
                                            label="Xóa"
                                        ></Button>
                                        </template>
                                        <template #end>
                                        <Button
                                            @click="filterSchedules"
                                            label="Lọc"
                                        ></Button>
                                        </template>
                                    </Toolbar>
                                </div>
                            </div>
                        </OverlayPanel>
                    </template>

                    <template #end>
                        <Button
                            @click="openAddForm()"
                            label="Thêm mới"
                            icon="pi pi-plus"
                            class="mr-2"
                        />
                        <Button
                            @click="refresh()"
                            class="mr-2 p-button-outlined p-button-secondary"
                            icon="pi pi-refresh"
                            v-tooltip="'Tải lại'"
                        />
                    </template>
                </Toolbar>
            </template>
            <!-- <Column
                selectionMode="multiple"
                headerStyle="text-align:center;max-width:75px;height:50px"
                bodyStyle="text-align:center;max-width:75px;max-height:60px"
                class="align-items-center justify-content-center text-center"
                v-if="store.state.user.is_admin == true && datalists.length > 0"
            >
            </Column> -->
            <Column
                field="is_order"
                header="STT"
                headerStyle="text-align:center;max-width:75px;height:50px"
                bodyStyle="text-align:center;max-width:75px;max-height:60px"
                class="align-items-center justify-content-center text-center"
            >
            </Column>

            <Column
                field="title"
                header="Nội dung"
                headerStyle="height:50px"
                bodyStyle="max-height:60px"
            >
            </Column>
            <Column
                field="type_backup"
                header="Loại backup"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:200px;height:50px"
                bodyStyle="text-align:center;max-width:200px;max-height:60px"
            >
                <template #body="data">
                    <span>{{ data.data.type_backup == 0 ? 'Database' : 
                                data.data.type_backup == 1 ? 'File' :
                                    'Database & File' }}</span>
                </template>
            </Column>
            <Column
                field="time_backup"
                header="Thời gian backup"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:200px;height:50px"
                bodyStyle="text-align:center;max-width:200px;max-height:60px"
            >
                <template #body="data">
                    <span>{{ data.data.time_backup != null ? moment(data.data.time_backup).format("HH:mm") : ""}}</span>
                    <span class="pl-1" v-if="data.data.type_time_backup == 0">hàng ngày</span>
                    <span class="pl-1" v-if="data.data.type_time_backup == 1">
                        {{(data.data.time_backup_week == 1 ? 'chủ nhật' : ('thứ ' + data.data.time_backup_week)) + ' hàng tuần'}}
                    </span>
                    <span class="pl-1" v-if="data.data.type_time_backup == 2">
                        {{'ngày ' + (data.data.time_backup_month == null ? '01' : (data.data.time_backup_month < 10 ? ('0' + data.data.time_backup_month) : data.data.time_backup_month)) + ' hàng tháng'}}
                    </span>
                    <span v-else>{{ data.data.time_backup == null ? "" : "" }}</span>
                </template>
            </Column>            
            <Column
                field="created_date"
                header="Ngày lập"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px;max-height:60px"
            >
                <template #body="data">
                    <span>{{ moment(data.data.created_date).format("DD/MM/YYYY HH:mm") }}</span>
                </template>
            </Column>
            <Column
                field="is_active"
                header="Trạng thái"
                headerStyle="text-align:center;max-width:100px;height:50px"
                bodyStyle="text-align:center;max-width:100px;max-height:60px"
                class="align-items-center justify-content-center text-center"
            >
                <template #body="data">
                    <Checkbox
                        :binary="true"
                        v-model="data.data.is_active"
                        @click="onCheckBox(data.data)"
                        :disabled="!((store.state.user.is_super == true && (store.state.user.organization_id == null || store.state.user.organization_id == data.data.organization_id)) ||
                            store.state.user.user_id == data.data.created_by)"
                    />
                </template>
            </Column>
            <Column
                header="Chức năng"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px;max-height:60px"
            >
                <template #body="slotprops">
                    <div v-if="
                            (store.state.user.is_super == true && (store.state.user.organization_id == null || store.state.user.organization_id == slotprops.data.organization_id)) ||
                            store.state.user.user_id == slotprops.data.created_by
                        "
                    >
                        <Button
                            @click="runScheduleBackup(slotprops.data)"
                            class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                            type="button"
                            v-tooltip="'Backup'"
                            style="border-radius:50%; height:2.357rem; width:2.357rem; padding:0.5rem 0; justify-content:center;"
                            v-if="slotprops.data.is_active == true"
                        >
                            <i class="pi pi-spin pi-spinner" v-if="isRunningBackup"></i>
                            <i class="pi pi-sync" v-else></i>
                        </Button>
                        <Button
                            @click="editSchedule(slotprops.data)"
                            class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                            type="button"
                            icon="pi pi-pencil"
                            v-tooltip="'Sửa'"
                        ></Button>
                        <Button
                            @click="delSchedule(slotprops.data, true)"
                            class="p-button-rounded p-button-danger p-button-outlined mx-1"
                            type="button"
                            icon="pi pi-trash"
                            v-tooltip="'Xóa'"
                        ></Button>
                    </div>
                </template>
            </Column>
            <template #empty>
                <div class="align-items-center justify-content-center p-4 text-center m-auto"
                    v-if="datalists.length == 0"
                >
                    <img
                        src="../../assets/background/nodata.png"
                        height="144"
                    />
                    <h3 class="m-1">Không có dữ liệu</h3>
                </div>
            </template>
        </DataTable>
    </div>
    
    <dialogAddFormBackup
		:key="cpnAddForm"
		:headerDialog="headerAddDialog"
		:displayDialog="displayAddForm"
		:dataForm="backup_schedule"
		:reloadData="loadData"
		:closeDialog="closeDialog"
	></dialogAddFormBackup>
</template>
<style>

</style>
<style lang="scss" scoped>
	::v-deep(.table-backup.p-datatable-scrollable) {
		.p-datatable-tbody {
			height: calc(100vh - 270px);
			background-color: #fff;
		}
		.p-datatable-emptymessage td {
			border-bottom: none;
		}
		tr.p-datatable-emptymessage:not(.p-highlight):hover {
			background-color: #fff !important;
		}
	}

</style>
