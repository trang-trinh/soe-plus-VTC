<script setup>
import { ref, inject, onMounted, watch, onBeforeUnmount } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import DetailedWork from "../../components/task_origin/DetailedWork.vue";
import moment from "moment";
import { concat } from "lodash";
import { encr } from "../../util/function.js";
import ModalListUserDepartmentVue from "../department_configuration/ModalListUserDepartment.vue";
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const basedomainURL = fileURL;

const toast = useToast();
const swal = inject("$swal");
const store = inject("store");
const axios = inject("axios"); // inject axios

const expandedKeys = ref({});
const config = {
    headers: { Authorization: `Bearer ${store.getters.token}` },
};
const addLog = (log) => {
    axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const Department = {
    id: null,
    department_id: null,
    user_id: null
}

const expandNode = (node) => {
  if (node.children && node.children.length) {
    expandedKeys.value[node.key] = true;
    for (let child of node.children) {
      expandNode(child);
    }
  }
};

const listDepartments = ref();
const listTreeDepartments = ref();
const opition = ref({
    IsNext: true,
    sort: "created_date",
    ob: "DESC",
    PageNo: 1,
    PageSize: 20,
    search: '',
    SearchTextUser: '',
    Filteruser_id: null,
    organization_type: null,
    user_id: store.getters.user_id,
});

const renderTree = (data, id, name, title) => {
    let arrChils = [];
    let arrtreeChils = [];
    data
        .filter((x) => x.parent_id == null)
        .forEach((m, i) => {
            m.IsOrder = i + 1;
            m.label_order = m.IsOrder.toString();
            if (opition.value.PageNo > 0) {
                m.STT = (opition.value.PageNo - 1) * opition.value.PageSize + i + 1;
            } else {
                m.STT = i + 1;
            }
            let om = { key: m[id], data: m };
            const rechildren = (mm, pid) => {
                let dts = data.filter((x) => x.parent_id == pid);
                if (dts.length > 0) {
                    if (!mm.children) mm.children = [];
                    dts.forEach((em, index) => {
                        em.label_order = mm.data.label_order + "." + em.is_order;
                        em.STT = mm.data.STT + "." + (index + 1);
                        let om1 = { key: em[id], data: em };
                        rechildren(om1, em[id]);
                        mm.children.push(om1);
                    });
                }
            };
            rechildren(om, m[id]);
            arrChils.push(om);
            //
            om = { key: m[id], data: m[id], label: m[name] };
            const retreechildren = (mm, pid) => {
                let dts = data.filter((x) => x.parent_id == pid);
                if (dts.length > 0) {
                    if (!mm.children) mm.children = [];
                    dts.forEach((em) => {
                        let om1 = { key: em[id], data: em[id], label: em[name] };
                        retreechildren(om1, em[id]);
                        mm.children.push(om1);
                    });
                }
            };
            retreechildren(om, m[id]);
            arrtreeChils.push(om);
        });
    arrtreeChils.unshift({
        key: -1,
        data: -1,
        label: "-----Chọn " + title + "----",
    });
    return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};

const loadData = (rf) => {
    if (rf) {
        opition.value.loading = true;
        swal.fire({
            width: 110,
            didOpen: () => {
                swal.showLoading();
            },
        });
    }
    axios
        .post(
            baseURL + "/api/TaskProc/getTaskData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "sys_organization_list",
                        par: [
                            { par: "pageno", va: opition.value.PageNo },
                            { par: "pagesize", va: opition.value.PageSize },
                            { par: "search", va: opition.value.search },
                            { par: "organization_type", va: opition.value.organization_type },
                            { par: "user_id", va: store.getters.user.user_id },
                        ],
                    }),
                    SecretKey,
                    cryoptojs,
                ).toString(),
            },
            config
        )
        .then((response) => {
            let data = JSON.parse(response.data.data);
            if (data.length > 0) {
                data[0].forEach((d) => {
                    d.Thanhviens = JSON.parse(d.Thanhviens);
                })
                let obj = renderTree(
                    data[0],
                    "organization_id",
                    "organization_name",
                    "đơn vị"
                );
                
                listDepartments.value = obj.arrChils;
                listDepartments.value.forEach((element) => {
                    expandNode(element);
                });
                listTreeDepartments.value = obj.arrtreeChils;
                opition.value.totalRecords = data[1][0].totalrecords;
            } else {
                listDepartments.value = [];
            }
            if (rf) {
                opition.value.loading = false;
                swal.close();
            }
        })
        .catch((error) => {
            toast.error("Tải dữ liệu không thành công!");
            opition.value.loading = false;
            addLog({
                title: "Lỗi Console loadData",
                controller: "LogsView.vue",
                log_content: error.message,
                loai: 2,
            });
            if (error && error.status === 401) {
                swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
                store.commit("gologout");
            }
        });
};


const DelDepartmentUser = (data) => {
    swal
        .fire({
            title: "Thông báo",
            text: "Bạn có muốn xoá người chủ trì của phòng ban này không!",
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
                var listId = [];
                listId.push(data.department_configuration_id);
                axios
                    .delete(baseURL + "/api/task_origin/Delete_DepartmentConfiguration", {
                        headers: { Authorization: `Bearer ${store.getters.token}` },
                        data: listId,
                    })
                    .then((response) => {
                        swal.close();
                        if (response.data.err != "1") {
                            swal.close();
                            toast.success("Xoá người chủ trì phòng ban thành công!");
                            loadData(true);
                        } else {
                            swal.fire({
                                title: "Thông báo!",
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
                                title: "Thông báo!",
                                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                                icon: "error",
                                confirmButtonText: "OK",
                            });
                        }
                    });
            }
        });
}
const IDepartment = ref();
const openModalDialog = ref(false);
const dtitle = ref();
const User_ID_Department = ref();
const updateDepartmentUser = (data) => {
    if(data.Thanhviens){
        User_ID_Department.value = data.Thanhviens[0].user_id;
    }else{
        User_ID_Department.value = null;
    }
    dtitle.value = "Cập nhật người chủ trì phòng ban (" + data.organization_name + ")";
    openModalDialog.value = true;
    IDepartment.value = data.organization_id;
}

const bgColor = ref([
    "#F8E69A",
    "#AFDFCF",
    "#F4B2A3",
    "#9A97EC",
    "#CAE2B0",
    "#8BCFFB",
    "#CCADD7",
]);

emitter.on("closeModalDialog", (obj) => {
  if (obj == false) {
    openModalDialog.value = false;
    loadData(true);
  }
});

onMounted(() => {
    loadData(true);
    return {};
});
</script>
<template>
    <div v-if="store.getters.islogin" class="main-layout true flex-grow-1 p-2">
        <TreeTable :value="listDepartments" :expandedKeys="expandedKeys" v-model:selectionKeys="selectedKey" v-model:first="first"
            :loading="opition.loading" @page="onPage($event)" @sort="onSort($event)" :paginator="true"
            :rows="opition.PageSize" :totalRecords="opition.totalRecords"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
            :rowsPerPageOptions="[20, 30, 50, 100, 200]" :filters="filters" :showGridlines="true" filterMode="strict"
            class="p-treetable-sm" :rowHover="true" responsiveLayout="scroll" :lazy="true" :scrollable="true"
            scrollHeight="flex">
            <template #header>
                <h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
                    <i class="pi pi-microsoft"></i> Danh sách phòng ban
                    ({{
                            opition.totalRecords
                    }})
                </h3>
                <Toolbar class="w-full custoolbar">
                    <template #start>
                        <span class="p-input-icon-left">
                            <i class="pi pi-search" />
                            <InputText type="text" spellcheck="false" v-model="opition.search"
                                placeholder="Tìm kiếm theo tên phòng ban" v-on:keyup.enter="loadData(true)" />
                        </span>
                    </template>
                </Toolbar>
            </template>
            <Column field="STT" header="STT" class="align-items-center justify-content-center text-center font-bold"
                headerStyle="text-align:center;max-width:100px" bodyStyle="text-align:center;max-width:100px">
            </Column>
            <Column field="organization_name" header="Tên phòng ban" :expander="true"
                headerStyle="height:50px;max-width:auto;" bodyStyle="max-height:60px">
                <template #body="md">
                    <div style="display: flex; align-items: center;">
                        <span style="margin-left:5px;">{{ md.node.data.organization_name }}</span>
                    </div>
                </template>
            </Column>
            <Column header="Người chủ trì" headerStyle="height:50px;max-width:100px;"
                class="align-items-center justify-content-center text-center font-bold"
                bodyStyle="text-align:center;max-height:60px; max-width: 100px;">
                <template #body="data">
                    <AvatarGroup>
                        <div v-for="(value, index) in data.node.data.Thanhviens" :key="index">
                            <div>
                                <Avatar @error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'" v-tooltip.bottom="{
                                    value:
                                        value.type_name +
                                        ': ' +
                                        value.fullName +
                                        '<br/>' +
                                        (value.tenChucVu || '') +
                                        '<br/>' +
                                        (value.tenToChuc || ''),
                                    escape: true,
                                }" v-bind:label="
    value.avatar ? '' : (value.ten ?? '').substring(0, 1)
" v-bind:image="basedomainURL + value.avatar" style="
                    background-color: #2196f3;
                    color: #ffffff;
                    width: 32px;
                    height: 32px;
                    font-size: 15px !important;
                    margin-left: -10px;
                  " :style="{
                      background: bgColor[index % 7] + '!important',
                  }" class="cursor-pointer" size="xlarge" shape="circle" />
                            </div>
                        </div>
                    </AvatarGroup>
                </template>
            </Column>
            <Column header="Chức năng" headerClass="text-center"
                class="align-items-center justify-content-center text-center"
                headerStyle="text-align:center;max-width:150px" bodyStyle="text-align:center;max-width:150px">
                <template #header> </template>
                <template #body="md">
                    <div v-if="(md.node.data.organization_type == 1)">
                        <Button type="button" icon="pi pi-user-plus" v-tooltip.top="'Cập nhật người chủ trì'"
                            class="p-button-rounded p-button-secondary p-button-outlined" style="margin-right: 0.5rem"
                            @click="updateDepartmentUser(md.node.data)"></Button>
                        <Button v-if="(md.node.data.Thanhviens)" type="button" icon="pi pi-trash"
                            v-tooltip.top="'Xóa người chủ trì'"
                            class="p-button-rounded p-button-secondary p-button-outlined"
                            @click="DelDepartmentUser(md.node.data)"></Button>
                    </div>
                </template>
            </Column>
            <template #empty>
                <div class="align-items-center justify-content-center p-4 text-center m-auto"
                    style="min-height: calc(100vh - 220px);max-height: calc(100vh - 220px);display: flex;flex-direction: column;"
                    v-if="!isFirst">
                    <img src="../../assets/background/nodata.png" height="144" />
                    <h3 class="m-1">Không có dữ liệu</h3>
                </div>
            </template>
        </TreeTable>

        <ModalListUserDepartmentVue v-if="openModalDialog == true" :visible="openModalDialog" :id="IDepartment" :userdepartment_id="User_ID_Department"
            :header="dtitle"></ModalListUserDepartmentVue>
    </div>
</template>
<style lang='scss' scope>
.p-datatable .p-column-header-content {
    justify-content: center;
}

.nguoi-dung .p-column-header-content {
    justify-content: left !important;
}
</style>