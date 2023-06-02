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

const formatByte = ((bytes, precision) => {
	if (isNaN(parseFloat(bytes)) || !isFinite(bytes)) return '-';
	if (typeof precision === 'undefined') precision = 1;
	let units = ['B', 'KB', 'MB', 'GB', 'TB', 'PB'];
	if (typeof bytes === 'string' || bytes instanceof String){
		bytes = parseFloat(bytes);
	}
	let	number = Math.floor(Math.log(bytes) / Math.log(1024));
	return (bytes / Math.pow(1024, Math.floor(number))).toFixed(precision) + ' ' + units[number];
});

const first = ref(0);
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
                        proc: "backup_history_list",
                        par: [
                        { par: "pageno", va: option.value.PageNo },
                        { par: "pagesize", va: option.value.PageSize },
                        { par: "search", va: option.value.search },
                        { par: "user_id", va: option.value.user_id },
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
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
const filterHistory = (event) => {
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
  { name: "Đang thực hiện", code: 0 },
  { name: "Thành công", code: 1 },
  { name: "Thất bại", code: 2 },
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
const downloadHistory = (file) => {
	if (file.file_path != null) {
        var url = baseURL + file.file_path;
        var name = file.file_path.substring(file.file_path.lastIndexOf("/") + 1) || "file_download";
        const a = document.createElement("a");
        a.href = basedomainURL + '/Viewer/DownloadFile?url='+ encodeURIComponent(file.file_path) + '&title=' + encodeURIComponent(name);
        a.download = name;
        //a.target = "_blank";
        a.click();
        a.remove();
    }
}

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
                    Lịch sử backup ({{ option.totalRecords }})
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
                                            @click="filterHistory"
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
                            @click="refresh"
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
                bodyStyle="text-align:center;max-width:75px;"
                class="align-items-center justify-content-center text-center"
                v-if="store.state.user.is_admin == true && datalists.length > 0"
            >
            </Column> -->
            <Column
                field="is_order"
                header="STT"
                headerStyle="text-align:center;max-width:60px;height:50px"
                bodyStyle="text-align:center;max-width:60px;"
                class="align-items-center justify-content-center text-center"
            >
            </Column>            
            <Column
                field="backup_date"
                header="Thời gian backup"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px;"
            >
                <template #body="data">
                    <span>{{ data.data.backup_date != null ? moment(data.data.backup_date).format("DD/MM/YYYY HH:mm") : ""}}</span>
                </template>
            </Column>
            <Column
                field="title_backup"
                header="Nội dung"
                headerStyle="height:50px"
            >
            </Column>
            <Column
                field="created_by"
                header="Người thực hiện"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:200px;height:50px"
                bodyStyle="text-align:center;max-width:200px;"
            >
                <template #body="data">
                    <span>{{ data.data.full_name }}</span>
                </template>
            </Column>
            <Column
                field="file_path"
                header="File backup"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:300px;height:50px"
                bodyStyle="justify-content:left !important;max-width:300px;"
            >
                <template #body="data">
                    <span class="flex" style="align-items: center;">
                        <img
                          class="cursor-pointer"
                          style="height: 32px; object-fit: contain"
                          v-bind:src="
                            basedomainURL + '/Portals/file/' +
                            (data.data.file_backup_path != null ? data.data.file_backup_path.substring(data.data.file_backup_path.lastIndexOf('.') + 1) : '')
                            + '.png'
                          "
                          @error="$event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'"
                          @click=""
                        />
                        <span class="flex-1 ml-2" style="word-break: break-word;">{{ data.data.file_backup_path }}</span>
                    </span>
                </template>
            </Column>
            <Column
                field="file_size"
                header="Dung lượng"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:150px;height:50px"
                bodyStyle="text-align:center;max-width:150px;"
            >
                <template #body="data">
                    <span>{{data.data.file_size ? formatByte(data.data.file_size) : '0 KB'}}</span>
                </template>
            </Column>
            <Column
                field="status"
                header="Trạng thái"
                headerStyle="text-align:center;max-width:180px;height:50px"
                bodyStyle="text-align:center;max-width:180px;"
                class="align-items-center justify-content-center text-center"
            >
                <template #body="data">
                    <Chip
                        :style="{
                            background: (data.data.status == 0 ? '#2196f3' : data.data.status == 1 ? '#6dd230' : data.data.status == 2 ? '#ff0000' : '#dee2e6'),
                            color: data.data.status == null ? '#495057' : '#ffffff',
                        }"
                        v-bind:label="data.data.status == 0 ? 'Đang thực hiện' : data.data.status == 1 ? 'Thành công' : data.data.status == 2 ? 'Thất bại' : ''"
                    />
                </template>
            </Column>
            <Column
                header="Chức năng"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:100px;height:50px"
                bodyStyle="text-align:center;max-width:100px;"
            >
                <template #body="slotprops">
                    <div>
                        <Button
                            @click="downloadHistory(slotprops.data)"
                            class="p-button-rounded p-button-secondary p-button-outlined mx-1"
                            type="button"
                            icon="pi pi-download"
                            v-tooltip.top="'Download'"
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
