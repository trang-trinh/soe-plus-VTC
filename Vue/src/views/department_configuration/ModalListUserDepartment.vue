<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
// import FileInfoVue from "./FileInfo.vue";
import { encr } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const emitter = inject("emitter");

const config = {
    headers: { Authorization: `Bearer ${store.getters.token}` },
};
const today = ref({});
today.value = new Date();
const basedomainURL = fileURL;
//Lấy size màn hình
const props = defineProps({
    id: Intl,
    userdepartment_id: String,
    header: String,
    visible: Boolean,
});
const listUsers = ref();
const options = ref({
    SearchTextUser: '',
    loading: true,
    firstPage: 0,
    PageNo: 0,
    PageSize: 2,
});

// const selectedFields = ref();
const openModalDialog = ref();
watch(openModalDialog, (vl) => {
    if (openModalDialog.value == false) {
        emitter.emit("closeModalDialog", false);
    }
});
// const checkSelect = ref(false);
// watch(selectedFields, (vl) => {
//     console.log(selectedFields.value);
//     if (
//         selectedFields.value == null ||
//         selectedFields.value == [] ||
//         selectedFields.value.length == 0 ||
//         selectedFields.value == {}
//     ) {
//         checkSelect.value = false;
//     } else checkSelect.value = true;
// });
// const onPage = (event) => {
//     if (event.rows != options.value.PageSize) {
//         options.value.PageSize = event.rows;
//     }
//     options.value.PageNo = event.page;
//     LoadDocToLink();
// };
const Department = ref(false);
const listUser = () => {
    axios
        .post(
            baseURL + "/api/TaskProc/getTaskData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "sys_users_list_task_origin",
                        par: [
                            { par: "search", va: options.value.SearchTextUser },
                            { par: "user_id", va: store.getters.user.user_id },
                            { par: "role_id", va: null },
                            { par: "organization_id", va: store.getters.user.organization_id },
                            { par: "department_id", va: props.id },
                            { par: "position_id", va: null },
                            { par: "isadmin", va: null },
                            { par: "status", va: null },
                            { par: "start_date", va: null },
                            { par: "end_date", va: null },
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
            listUsers.value = data.filter(x=>x.status == 1).map((x) => ({
                name: x.full_name,
                name_en: x.full_name_en,
                code: x.user_id,
                avatar: x.avatar,
                is_check: (x.user_id == props.userdepartment_id) ? true : false,
                last_name: x.last_name,
                is_order: x.is_order,
                tenChucVu: x.position_name,
                tenToChuc: x.organization_name,
            }));
        })
        .catch((error) => {
            console.log(error);
            toast.error("Tải dữ liệu không thành công!");
            opition.value.loading = false;

            if (error && error.status === 401) {
                swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
                store.commit("gologout");
            }
        });
};

const bgColor = ref([
    "#F8E69A",
    "#AFDFCF",
    "#F4B2A3",
    "#9A97EC",
    "#CAE2B0",
    "#8BCFFB",
    "#CCADD7",
]);


const closeDialogDepartment = () => {
    openModalDialog.value = false;
}

const saveDepartmentUser = () => {
    let formData = new FormData();
    if (listUsers.value) {
        if (listUsers.value.filter(x => x.is_check == true).length > 0) {
            listUsers.value.filter(x => x.is_check == true).forEach((t) => {
                Department.value.user_id = t.code;
            })
        }
    }
    formData.append("department", JSON.stringify(Department.value));
    axios
        .post(baseURL + "/api/task_origin/Update_DepartmentConfiguration", formData, config)
        .then((response) => {
            if (response.data.err != "1") {
                swal.close();
                toast.success("Cập nhật phòng ban thành công!");
                listUser();
                closeDialogDepartment();
            } else {
                swal.fire({
                    title: "Thông báo!",
                    html: response.data.ms,
                    icon: "error",
                    confirmButtonText: "OK",
                });
            }
        })
        .catch((response) => {
            swal.close();
            swal.fire({
                title: "Error!",
                text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
                icon: "error",
                confirmButtonText: "OK",
            });
        });
}


const ChangeUserDepartment = (model) => {
    listUsers.value.forEach((u) => {
        if (u.code != model.code) {
            u.is_check = false;
        } else {
            u.is_check = true;
        }
    })
}

onMounted(() => {
    listUser();
    openModalDialog.value = props.visible;
    Department.value = {
        id: -1,
        department_id: props.id,
        user_id: null
    }
    return {};
});
</script>
<template>
    <div></div>
    <Dialog :header="props.header" v-model:visible="openModalDialog" :style="{ width: '40vw' }" :closable="true"
        :maximizable="true">
        <div class="grid formgrid m-2">
            <div class="field col-12 md:col-6"></div>
            <div class="field col-12 md:col-6">
                <span class="p-input-icon-left md:col-12">
                    <i style="margin-left: 5px;;" class="pi pi-search" />
                    <InputText type="text" spellcheck="false" v-model="options.SearchTextUser" class="md:col-12"
                        placeholder="Tìm kiếm ..." @keyup.enter="listUser()"></InputText>
                </span>

            </div>
            <div class="field col-12 md:col-12">
                <DataTable v-model:first="first" :rowHover="true" :value="listUsers" :row-hover="true" dataKey="code"
                    v-model:selection="selectedTasks" @page="onPage($event)" @sort="onSort($event)"  @filter="onFilter($event)" :lazy="true"
                    selectionMode="single" @rowSelect="onRowSelect($event.data)"
                    @rowUnselect="onRowUnselect($event.data)">
                    <Column headerStyle="text-align:center;width:1rem;min-height:3.125rem"
                        bodyStyle="text-align:center;width:1rem;"
                        class="align-items-center justify-content-center text-center">
                        <template #body="data">
                            <Checkbox inputId="binary" @change="ChangeUserDepartment(data.data)"
                                v-model="data.data.is_check" :binary="true" />
                        </template>
                    </Column>
                    <Column header="Ảnh đại diện"
                        headerStyle="text-align:center;width:10rem;min-height:3.125rem; justify-content: center;"
                        bodyStyle="text-align:center;width:10rem;"
                        class="align-items-center justify-content-center text-center">
                        <template #body="value">
                            <Avatar v-tooltip.bottom="{
                                value:
                                    value.data.name +
                                    '<br/>' +
                                    (value.data.tenChucVu || '') +
                                    '<br/>' +
                                    (value.data.tenToChuc || ''),
                                escape: true,
                            }" v-bind:label="
    value.data.avatar
        ? ''
        : (value.data.last_name ?? '').substring(0, 1)
" v-bind:image="basedomainURL + value.data.avatar" style="
              background-color: #2196f3;
              color: #ffffff;
              width: 2.5rem;
              height: 2.5rem;
              font-size: 15px !important;
            " :style="{
                background: bgColor[value.data.is_order % 7] + '!important',
            }" class="cursor-pointer" size="xlarge" shape="circle" />
                        </template>
                    </Column>
                    <Column field="name" header="Tên người dùng"
                        headerStyle="text-align:center;max-width:auto;min-height:3.125rem"
                        bodyStyle="text-align:left;max-width:auto;"
                        class="align-items-left justify-content-center text-left nguoi-dung">
                    </Column>
                    <template #empty>
                        <div class="align-items-center justify-content-center p-4 text-center m-auto" style="
          " v-if="listUsers != null">
                            <h3 class="m-1">Không có dữ liệu</h3>
                        </div>
                    </template>
                </DataTable>
            </div>
        </div>
        <template #footer>
            <Button label="Hủy" icon="pi pi-times" @click="closeDialogDepartment()" class="p-button-text" />
            <Button label="Lưu" icon="pi pi-check" @click="saveDepartmentUser()" />
        </template>
    </Dialog>
</template>
<style lang='scss' scoped>
.p-button-hover:hover {
    color: #0025f8 !important;
    background: #ffffff !important;
}
</style>
