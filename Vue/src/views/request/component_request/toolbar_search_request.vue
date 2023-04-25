<script setup>
import { ref, inject, onMounted } from "vue";
import { encr } from "../../../util/function";

const axios = inject("axios");
const store = inject("store");
const cryoptojs = inject("cryptojs");
const basedomainURL = baseURL;const config = {
	headers: { Authorization: `Bearer ${store.getters.token}` },
};

const props = defineProps({
    options: Object,
    dictionarys: Array,
    search: Function,
    resetFilter: Function,
    filter: Function,
});
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
const deadlineRequest = ref([
    //{ name: "Tất cả đề xuất", code: -1 },
    { name: "Đề xuất sắp đến hạn duyệt", code: 0 },
    { name: "Đề xuất duyệt đúng hạn", code: 1 },
    { name: "Đề xuất quá hạn duyệt", code: 2 },
]);
const opfilter = ref();
const toggleFilter = (event) => {
    opfilter.value.toggle(event);
};
const removeFilter = (idx, array, isTree) => {
    if (isTree) {
        array[idx["key"]]["checked"] = false;
    } else {
        array.splice(idx, 1);
    }
};
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
const filterData = (event) => {
    opfilter.value.toggle(event);
    props.filter();
};

onMounted(() => {
    initDictionary();
});
</script>
<template>
    <span class="p-input-icon-left">
        <i class="pi pi-search" />
        <InputText
            @keypress.enter="props.search()"
            v-model="props.options.search"
            type="text"
            spellcheck="false"
            placeholder="Tìm kiếm"
            class="input-search"
        />
    </span>
    <Button
        @click="toggleFilter($event)"
        type="button"
        class="ml-2 p-button-outlined p-button-secondary"
        aria:haspopup="true"
        aria-controls="overlay_panel"
        style="padding: 0.5rem 0.75rem;"
    >
        <div>
            <span class=""><i class="pi pi-filter"></i></span>
        </div>
    </Button>
    <OverlayPanel
        :showCloseIcon="false"
        ref="opfilter"
        appendTo="body"
        class="p-0 m-0"
        id="overlay_panel"
        style="width: 700px"
    >
        <div class="grid formgrid m-0">
            <div
                class="col-12 md:col-12 p-0"
                :style="{
                    minHeight: 'unset',
                    maxheight: 'calc(100vh - 300px)',
                    overflow: 'auto',
                }"
            >
                <div class="row">
                    <div class="col-6 md:col-6">
                        <div class="row">
                            <div class="col-12 md:col-12 p-0">
                                <div class="form-group">
                                    <label>Đơn vị</label>
                                    <MultiSelect
                                        :options="props.dictionarys[1]"
                                        :filter="true"
                                        :showClear="true"
                                        :editable="false"
                                        v-model="props.options.organizations"
                                        optionLabel="organization_name"
                                        placeholder="Chọn đơn vị"
                                        class="w-full limit-width custom-multiselect"
                                        style="min-height: 36px"
                                        panelClass="d-design-dropdown"
                                    >
                                        <template #value="slotProps">
                                            <ul class="p-ulchip"
                                                v-if="slotProps.value && slotProps.value.length > 0"
                                            >
                                            <li class="p-lichip"
                                                v-for="(value, index) in slotProps.value"
                                                :key="index"
                                            >
                                                <Chip class="mr-2 mb-2 px-3 py-2">
                                                    <div class="flex" style="align-items: center;">
                                                        <div>
                                                            <span>{{ value.organization_name }}</span>
                                                        </div>
                                                        <span
                                                            tabindex="0"
                                                            class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                                            @click="removeFilter(index, props.options.organizations);
                                                                $event.stopPropagation();
                                                            "
                                                            v-tooltip.top="'Xóa'"
                                                        ></span>
                                                    </div>
                                                </Chip>
                                            </li>
                                            </ul>
                                            <span v-else>
                                                {{ slotProps.placeholder }}
                                            </span>
                                        </template>
                                    </MultiSelect>
                                </div>
                            </div>
                            <div class="col-12 md:col-12 p-0">
                                <div class="form-group">
                                    <label>Team</label>
                                    <MultiSelect
                                        :options="props.dictionarys[2]"
                                        :filter="true"
                                        :showClear="true"
                                        :editable="false"
                                        v-model="props.options.teams"
                                        optionLabel="request_team_name"
                                        placeholder="Chọn team"
                                        class="w-full limit-width custom-multiselect"
                                        style="min-height: 36px"
                                        panelClass="d-design-dropdown"
                                    >
                                        <template #value="slotProps">
                                            <ul class="p-ulchip"
                                                v-if="slotProps.value && slotProps.value.length > 0"
                                            >
                                            <li class="p-lichip"
                                                v-for="(value, index) in slotProps.value"
                                                :key="index"
                                            >
                                                <Chip class="mr-2 mb-2 px-3 py-2">
                                                    <div class="flex" style="align-items: center;">
                                                        <div>
                                                            <span>{{ value.request_team_name }}</span>
                                                        </div>
                                                        <span
                                                            tabindex="0"
                                                            class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                                            @click="removeFilter(index, props.options.teams);
                                                                $event.stopPropagation();
                                                            "
                                                            v-tooltip.top="'Xóa'"
                                                        ></span>
                                                    </div>
                                                </Chip>
                                            </li>
                                            </ul>
                                            <span v-else>
                                                {{ slotProps.placeholder }}
                                            </span>
                                        </template>
                                    </MultiSelect>
                                </div>
                            </div>
                            <div class="col-12 md:col-12 p-0">
                                <div class="form-group">
                                    <label>Vai trò</label>
                                    <MultiSelect
                                        :options="list_roles"
                                        :filter="true"
                                        :showClear="true"
                                        :editable="false"
                                        v-model="props.options.roles"
                                        optionLabel="name"
                                        placeholder="Chọn vai trò"
                                        class="w-full limit-width custom-multiselect"
                                        style="min-height: 36px"
                                        panelClass="d-design-dropdown"
                                    >
                                        <template #value="slotProps">
                                            <ul class="p-ulchip"
                                                v-if="slotProps.value && slotProps.value.length > 0"
                                            >
                                            <li class="p-lichip"
                                                v-for="(value, index) in slotProps.value"
                                                :key="index"
                                            >
                                                <Chip class="mr-2 mb-2 px-3 py-2">
                                                    <div class="flex" style="align-items: center;">
                                                        <div>
                                                            <span>{{ value.name }}</span>
                                                        </div>
                                                        <span
                                                            tabindex="0"
                                                            class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                                            @click="removeFilter(index, props.options.roles);
                                                                $event.stopPropagation();
                                                            "
                                                            v-tooltip.top="'Xóa'"
                                                        ></span>
                                                    </div>
                                                </Chip>
                                            </li>
                                            </ul>
                                            <span v-else>
                                                {{ slotProps.placeholder }}
                                            </span>
                                        </template>
                                    </MultiSelect>
                                </div>
                            </div>
                            <div class="col-12 md:col-12 p-0">
                                <div class="form-group">
                                    <label>Trạng thái đề xuất</label>
                                    <MultiSelect
                                        :options="list_status"
                                        :filter="true"
                                        :showClear="true"
                                        :editable="false"
                                        v-model="props.options.status_requests"
                                        optionLabel="name"
                                        placeholder="Chọn trạng thái đề xuất"
                                        class="w-full limit-width custom-multiselect"
                                        style="min-height: 36px"
                                        panelClass="d-design-dropdown"
                                    >
                                        <template #value="slotProps">
                                            <ul class="p-ulchip"
                                                v-if="slotProps.value && slotProps.value.length > 0"
                                            >
                                            <li class="p-lichip"
                                                v-for="(value, index) in slotProps.value"
                                                :key="index"
                                            >
                                                <Chip class="mr-2 mb-2 px-3 py-2">
                                                    <div class="flex" style="align-items: center;">
                                                        <div>
                                                            <span>{{ value.name }}</span>
                                                        </div>
                                                        <span
                                                            tabindex="0"
                                                            class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                                            @click="removeFilter(index, props.options.status_requests);
                                                                $event.stopPropagation();
                                                            "
                                                            v-tooltip.top="'Xóa'"
                                                        ></span>
                                                    </div>
                                                </Chip>
                                            </li>
                                            </ul>
                                            <span v-else>
                                                {{ slotProps.placeholder }}
                                            </span>
                                        </template>
                                    </MultiSelect>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-6 md:col-6">
                        <div class="row">
                            <div class="col-12 md:col-12 p-0">
                                <div class="form-group">
                                    <label>Loại đề xuất</label>
                                    <MultiSelect 
                                        v-model="props.options.type_form_requests" 
                                        :options="listTypeRequestFilter" 
                                        optionLabel="label" 
                                        optionGroupLabel="label" 
                                        optionGroupChildren="items" 
                                        display="chip" 
                                        :filter="true"
                                        :showClear="true"
                                        placeholder="Chọn loại đề xuất" 
                                        class="w-full limit-width custom-multiselect">
                                        <template #optiongroup="slotProps">
                                            <div class="flex align-items-center">
                                                <div>{{ slotProps.option.label }}</div>
                                            </div>
                                        </template>
                                        <template #value="slotProps">
                                            <ul class="p-ulchip"
                                                v-if="slotProps.value && slotProps.value.length > 0"
                                            >
                                                <li class="p-lichip"
                                                    v-for="(value, index) in slotProps.value"
                                                    :key="index"
                                                >
                                                    <Chip class="mr-2 mb-2 px-3 py-2">
                                                        <div class="flex" style="align-items: center;">
                                                            <div>
                                                                <span>{{ value.label }}</span>
                                                            </div>
                                                            <span
                                                                tabindex="0"
                                                                class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                                                @click="
                                                                    removeFilter(index, props.options.type_form_requests);
                                                                    $event.stopPropagation();
                                                                "
                                                                v-tooltip.top="'Xóa'"
                                                            ></span>
                                                        </div>
                                                    </Chip>
                                                </li>
                                            </ul>
                                            <span v-else>
                                                {{ slotProps.placeholder }}
                                            </span>
                                        </template>
                                    </MultiSelect>
                                </div>
                            </div>
                            <div class="col-12 md:col-12 p-0">
                                <div class="form-group">
                                    <label>Tình trạng đề xuất</label>
                                    <MultiSelect
                                        :options="deadlineRequest"
                                        :filter="true"
                                        :showClear="true"
                                        v-model="props.options.status_overdue"
                                        optionLabel="name"
                                        placeholder="Chọn tình trạng đề xuất"
                                        class="w-full limit-width custom-multiselect"
                                        style="min-height: 36px"
                                        panelClass="d-design-dropdown"
                                    >
                                        <template #value="slotProps">
                                            <ul class="p-ulchip"
                                                v-if="slotProps.value && slotProps.value.length > 0"
                                            >
                                                <li class="p-lichip"
                                                    v-for="(value, index) in slotProps.value"
                                                    :key="index"
                                                >
                                                    <Chip class="mr-2 mb-2 px-3 py-2">
                                                        <div class="flex" style="align-items: center;">
                                                            <div>
                                                                <span>{{ value.name }}</span>
                                                            </div>
                                                            <span
                                                                tabindex="0"
                                                                class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                                                @click="
                                                                    removeFilter(index, props.options.status_overdue);
                                                                    $event.stopPropagation();
                                                                "
                                                                v-tooltip.top="'Xóa'"
                                                            ></span>
                                                        </div>
                                                    </Chip>
                                                </li>
                                            </ul>
                                            <span v-else>
                                                {{ slotProps.placeholder }}
                                            </span>
                                        </template>
                                    </MultiSelect>
                                </div>
                            </div>
                            <div class="col-12 md:col-12 p-0">
                                <div class="form-group m-0">
                                    <label>Ngày lập</label>
                                </div>
                            </div>
                            <div class="col-6 md:col-6 p-0 pr-1">
                                <div class="form-group">
                                    <Calendar
                                        :showIcon="true"
                                        class="ip36"
                                        autocomplete="on"
                                        inputId="time24"
                                        v-model="props.options.start_created"
                                        placeholder="Từ ngày"
                                    />
                                </div>
                            </div>
                            <div class="col-6 md:col-6 p-0 pl-1">
                                <div class="form-group">
                                    <Calendar
                                        :showIcon="true"
                                        class="ip36"
                                        autocomplete="on"
                                        inputId="time24"
                                        v-model="props.options.end_created"
                                        placeholder="Đến ngày"
                                    />
                                </div>
                            </div>
                            <div class="col-12 md:col-12 p-0">
                                <div class="form-group m-0">
                                    <label>Ngày hoàn thành</label>
                                </div>
                            </div>
                            <div class="col-6 md:col-6 p-0 pr-1">
                                <div class="form-group">
                                    <Calendar
                                        :showIcon="true"
                                        class="ip36"
                                        autocomplete="on"
                                        inputId="time24"
                                        v-model="props.options.start_completed"
                                        placeholder="Từ ngày"
                                    />
                                </div>
                            </div>
                            <div class="col-6 md:col-6 p-0 pl-1">
                                <div class="form-group">
                                    <Calendar
                                        :showIcon="true"
                                        class="ip36"
                                        autocomplete="on"
                                        inputId="time24"
                                        v-model="props.options.end_completed"
                                        placeholder="Đến ngày"
                                    />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-12 md:col-12 p-0">
                <Toolbar
                    class="border-none surface-0 outline-none px-0 pb-0 w-full"
                >
                    <template #start>
                        <Button
                            @click="props.resetFilter()"
                            class="p-button-outlined"
                            label="Bỏ chọn"
                        />
                    </template>
                    <template #end>
                        <Button @click="filterData($event)" label="Lọc" />
                    </template>
                </Toolbar>
            </div>
        </div>
    </OverlayPanel>
</template>
<style scoped>
    @import url(../style_request.css);
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
</style>