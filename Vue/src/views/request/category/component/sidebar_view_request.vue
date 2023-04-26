<script setup>
import { ref, inject, onMounted, watch, onBeforeUnmount } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import moment from "moment";
import { encr } from "../../../../util/function.js";
import { useRoute } from "vue-router";
const cryoptojs = inject("cryptojs");
const router = inject("router");
const route = useRoute();
const basedomainURL = fileURL;
const componentKey = ref(0);
const forceRerender = () => {
    componentKey.value += 1;
};
const listViewRequest = ref([]);
const opition = ref({
    search: '',
    PageNo: 0,
    PageSize: 20,
    sort: "created_date",
    ob: "DESC",
});
const loadData = (rf) => {

}
onMounted(() => {
    loadData(true);
    return {
        loadData,
    };
});
</script>
<template>
    <div class="sidebar-request" style="height: 98vh;">
        <div class="d-grid formgrid m-1">
            <div class="flex justify-content-center align-items-center">
                <Toolbar class="w-full custoolbar">
                    <template #start>
                        <span class="p-input-icon-left">
                            <i class="pi pi-search" />
                            <InputText style="min-width: 300px" type="text" spellcheck="false" v-model="opition.search"
                                placeholder="Tìm kiếm" @keyup.enter="loadData(true)" />
                        </span>
                    </template>
                    <template #end>
                        <Button @click="addTask('Tạo công việc')" label="Tải lại" icon="pi pi-plus" class="mr-2" />
                    </template>
                </Toolbar>
            </div>
        </div>
        <div class="d-grid formgrid m-1">
            <ul class="list-type-request">
                <li class="zoom" style="background-color: #33c9dc;text-align: center;">Chờ tôi duyệt (0)</li>
                <li class="zoom" style="background-color: #2196f3;text-align: center;">Đã phê duyệt (0)</li>
                <li class="zoom" style="background-color: #74b9ff;text-align: center;">Tất cả (0)</li>
                <li class="zoom" style="background-color: #33c9dc;text-align: center;">Chờ duyệt (0)</li>
                <li class="zoom" style="background-color: #f17ac7;text-align: center;">Đã từ chối (0)</li>
                <li class="zoom" style="background-color: blueviolet;text-align: center;">Tôi theo dõi (0)</li>
                <li class="zoom" style="background-color: #6dd230;text-align: center;">Hoàn thành (0)</li>
                <li class="zoom" style="background-color: #ff8b4e;text-align: center;">Đã quá hạn (0)</li>
                <li class="zoom" style="background-color: #f5b041;text-align: center;">Xử lý đánh giá (0)</li>
            </ul>
        </div>
        <div class="d-grid formgrid m-1">
            <DataTable class="table-view-request" v-model:first="first" :rowHover="true" :value="listViewRequest"
                :paginator="true" :rows="opition.PageSize"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
                :rowsPerPageOptions="[1, 20, 30, 50, 100, 200]" :scrollable="true" scrollHeight="flex"
                :totalRecords="opition.totalRecords" :row-hover="true" dataKey="project_id"
                v-model:selection="selectedProjectMains" @page="onPage($event)" @sort="onSort($event)"
                @filter="onFilter($event)" selectionMode="single">
                <Column field="STT" header="Mã số" class="align-items-center justify-content-center text-center font-bold"
                    headerStyle="text-align:center;max-width:10rem" bodyStyle="text-align:center;max-width:10rem">
                </Column>
                <Column field="project_name" header="Tên đề xuất" headerStyle="max-width:auto;">
                    <template #body="md">
                        <div style="display: flex; align-items: center">
                            <span style="margin-left: 5px">{{
                                md.data.project_name
                            }}</span>
                        </div>
                    </template>
                </Column>
                <Column field="status" header="Trạng thái" class="align-items-center justify-content-center text-center"
                    headerStyle="text-align:center;max-width:120px" bodyStyle="text-align:center;max-width:120px">
                    <template #body="md">
                        <Chip :style="{
                                    background: md.data.status_bg_color,
                                    color: md.data.status_text_color,
                                }" v-bind:label="md.data.status_name" />
                    </template>
                </Column>
                <Column field="STT" header="Người tạo"
                    class="align-items-center justify-content-center text-center font-bold"
                    headerStyle="text-align:center;max-width:10rem" bodyStyle="text-align:center;max-width:10rem">
                </Column>
                <Column field="STT" header="Người duyệt"
                    class="align-items-center justify-content-center text-center font-bold"
                    headerStyle="text-align:center;max-width:10rem" bodyStyle="text-align:center;max-width:10rem">
                </Column>
                <Column field="STT" header="Ngày tạo"
                    class="align-items-center justify-content-center text-center font-bold"
                    headerStyle="text-align:center;max-width:10rem" bodyStyle="text-align:center;max-width:10rem">
                </Column>
                <template #empty>
                    <div class="align-items-center justify-content-center p-4 text-center m-auto" style="
                                        min-height: calc(100vh - 215px);
                                        max-height: calc(100vh - 215px);
                                        display: flex;
                                        flex-direction: column;
                                    " v-if="listViewRequest != null">
                        <img src="../../../../assets/background/nodata.png" height="144" />
                        <h3 class="m-1">Không có dữ liệu</h3>
                    </div>
                </template>
            </DataTable>
        </div>
    </div>
</template>
<style>
.p-sidebar-content{
    overflow: hidden !important;
}
</style>
<style lang="scss" scoped>
.list-type-request {
    display: flex;
}

.list-type-request li {
    list-style: none;
    margin: 10px;
    padding: 10px 10px;
    color: #fff;
}

.zoom {
    cursor: pointer;
    border-radius: 0.25rem;
    box-shadow: 0 2px 4px rgb(0 0 0 / 23%);
    transition: transform 0.3s !important;
}

.zoom:hover {
    transform: scale(0.9) !important;
    /* box-shadow: 10px 10px 15px rgb(0 0 0 / 23%) !important; */
    cursor: pointer !important;
}

.sidebar-request .p-sidebar-content {
    overflow: hidden !important;
}
</style>