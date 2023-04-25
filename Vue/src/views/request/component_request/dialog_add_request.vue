<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr } from "../../../util/function";
import treeuser from "../../../components/user/treeuser.vue";

const toast = useToast();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const cryoptojs = inject("cryptojs");
const basedomainURL = baseURL;
const baseUrlCheck = "http://localhost:8080/";
const config = {
	headers: { Authorization: `Bearer ${store.getters.token}` },
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
const props = defineProps({
	key: Number,
	headerDialog: String,
	displayDialog: Boolean,
	dataForm: Object,
    dictionarys: Array,
    listFiles: Array,
    listTypeRequest: Array,
    listUserApproved: Array,
    listUserFollow: Array,
    listUserManage: Array,
    reloadData: Function,
	closeDialog: Function,
});

const rules = {
	request_name: {
		required,
		maxLength: maxLength(500),
		$errors: [
			{
				$property: "request_name",
				$validator: "required",
				$message: "Tên đề xuất không được để trống!",
			},
		],
	},
    content: {
		required,
		maxLength: maxLength(500),
		$errors: [
			{
				$property: "content",
				$validator: "required",
				$message: "Nội dung không được để trống!",
			},
		],
	},
};
const listUserApproved = ref(props.listUserApproved);
const listUserFollow = ref(props.listUserFollow);
const listUserManage = ref(props.listUserManage);
const request_data = ref(props.dataForm);
const v$ = useVuelidate(rules, request_data);

const submitted = ref(false);
const listPriorityLevel = ref([
    { name: 'Bình thường', code: 0 },
    { name: 'Gấp', code: 1 },
    { name: 'Rất gấp', code: 2 },
]);
const list_type_process = ref([
    { name: 'Một trong nhiều', code: 0 },
    { name: 'Duyệt tuần tự'  , code: 1 },
    { name: 'Duyệt đồng thời', code: 2 }
]);

// File 
const files = ref([]);
const selectFile = (event) => {
    files.value = [];
    event.files.forEach((element) => {
        files.value.push(element);
    });
};
const removeFile = (event) => {
    files.value = [];
    event.files.forEach((element) => {
        files.value.push(element);
    });
};
const goFile = (file) => {
    window.open(basedomainURL + file.file_path, "_blank");
};
const deleteFile = (item, idx) => {
    swal
        .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá tệp đính kèm này không!",
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
            if (item["file_id"] != null) {
                var ids = [item["file_id"]];
                axios
                    .delete(baseURL + "/api/request/delete_file", {
                        headers: { Authorization: `Bearer ${store.getters.token}` },
                        data: ids,
                    })
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
                        if (ids.length > 0) {
                            ids.forEach((element, i) => {
                                var idx = request_data.files.findIndex((x) => x.file_id == SVGSetElement);
                                if (idx != -1) {
                                    request_data.files.splice(idx, 1);
                                }
                            });
                        }
                        swal.close();
                        toast.success("Xoá tệp đính kèm thành công!");
                    })
                    .catch((error) => {
                        swal.close();
                        addLog({
                            title: "Lỗi Console delItem",
                            controller: "boardroom.vue",
                            logcontent: error.message,
                            loai: 2,
                        });
                        if (error.status === 401) {
                            swal.fire({
                                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                                confirmButtonText: "OK",
                            });
                            return;
                        }
                    });
            } else {
                request_data.files.splice(idx, 1);
                swal.close();
            }
        }
    });
};
//

// Modal Tree User
const selectedUser = ref([]);
const is_one = ref(false);
const is_type = ref();
const headerDialogUser = ref();
const displayDialogUser = ref(false);
const closeDialogUser = () => {
  	displayDialogUser.value = false;
};
const showModalUser = (one, type) => {
	selectedUser.value = [];
	headerDialogUser.value = "Chọn người dùng";
    if (type != null) {
        switch (type) {
        case 0:
            selectedUser.value = [...listUserApproved.value];
            headerDialogUser.value = "Chọn người duyệt";
            break;
        case 1:
            selectedUser.value = [...listUserManage.value];
            headerDialogUser.value = "Chọn người quản lý";
            break;
        case 2:
            selectedUser.value = [...listUserFollow.value];
            headerDialogUser.value = "Chọn người theo dõi";
            break;
        default:
            break;
        }
    }
	is_one.value = one;
	is_type.value = type;
	displayDialogUser.value = true;
};
const choiceUser = () => {
	switch (is_type.value) {
		case 0:
			var notexist = selectedUser.value.filter((a) => listUserApproved.value.findIndex((b) => b["user_id"] === a["user_id"]) === -1);
			if (notexist.length > 0) {
        		notexist.forEach((e) => {
          			listUserApproved.value.push(e);
				});
				//props.listMember = props.listMember.concat(notexist);
			}
			break;
        case 1:
			var notexist = selectedUser.value.filter((a) => listUserManage.value.findIndex((b) => b["user_id"] === a["user_id"]) === -1);
			if (notexist.length > 0) {
        		notexist.forEach((e) => {
          			listUserManage.value.push(e);
				});
			}
			break;
        case 2:
			var notexist = selectedUser.value.filter((a) => listUserFollow.value.findIndex((b) => b["user_id"] === a["user_id"]) === -1);
			if (notexist.length > 0) {
        		notexist.forEach((e) => {
          			listUserFollow.value.push(e);
				});
			}
			break;
		default:
			break;
	}
	closeDialogUser();
};

const removeMember = (user, arr, type) => {
    if ((type == 'approver' && user["request_sign_user_id"] != null) || (type != 'approver' && user["request_follow_id"] != null)) {
        swal.fire({
            title: "Thông báo",
            text: "Bạn có muốn xoá thành viên này không!",
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
                var ids = type == 'approver' ? [user["request_sign_user_id"]] : [user["request_follow_id"]];
                axios
                    .delete(baseURL + "/api/request/delete_member", {
                    headers: { Authorization: `Bearer ${store.getters.token}` },
                    data: ids,
                })
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
                    ids.forEach((value, i) => {
                        var idx = type == 'approver' ? 
                                    arr.findIndex((x) => x["request_sign_user_id"] == value) :
                                    arr.findIndex((x) => x["request_follow_id"] == value);
                        if (idx != -1) {
                            arr.splice(idx, 1);
                        }
                    });

                    swal.close();
                    toast.success("Xoá thành viên thành công!");
                })
                .catch((error) => {
                    swal.close();
                    addLog({
                        title: "Lỗi Console delItem",
                        controller: "dialog_add_request.vue",
                        logcontent: error.message,
                        loai: 2,
                    });
                    if (error.status === 401) {
                        swal.fire({
                            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                            confirmButtonText: "OK",
                        });
                        return;
                    }
                });
            }
        });
    } else {
        var idx = arr.findIndex((x) => x["user_id"] === user["user_id"]);
        if (idx != -1) {
            arr.splice(idx, 1);
        }
    }
};
const changeAttendees = () => {};
//
const saveData = (frm) => {
    if (!frm) {
        return false;
    }    
    submitted.value = true;
    if (request_data.value.request_form_id == null) {
        request_data.value.is_change_process = true;
    }
    var formData = new FormData();
    formData.append("modelRequest", JSON.stringify(request_data.value));
    formData.append("listApprover", JSON.stringify(listUserApproved.value));
    formData.append("listManager", JSON.stringify(listUserManage.value));
    formData.append("listFollower", JSON.stringify(listUserFollow.value));
    for (var i = 0; i < files.value.length; i++) {
        let file = files.value[i];
        formData.append("files", file);
    }
    axios({
        method: request_data.value.request_id == null ? 'post' : 'put',
        url: baseUrlCheck 
            + `/api/request/${request_data.value.request_id == null ? "Add_Request" : "Update_Request"}`,
		data: formData,
		headers: {
			Authorization: `Bearer ${store.getters.token}`,
		},
    })
    .then((response) => {
        props.closeDialog();
        props.reloadData();
    })
    .catch((error) => {
        if (error.status === 401) {
            swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
            });
            return;
        }
    });
};
const filterTeamRequest = ref(props.dictionarys[2]);
const listSignUser = ref([]);
const formDS = ref([]);
const Ftables = ref([]);
const loadFormD = (form_id) => {
    axios({
        method: "post",
        url: basedomainURL + "api/request/getData",
        data: {
            str: encr(
                JSON.stringify({
                    proc: "request_get_form_detail",
                    par: [ 
                        { par: "request_form_id", va: form_id },
                    ],
                }),
                SecretKey,
                cryoptojs
            ).toString(),
        },
        headers: {
			Authorization: `Bearer ${store.getters.token}`,
		},
    })
    .then((response) => {
        if (response.data.err !== '1') {
            var data = JSON.parse(res.data.data);
            formDS.value = data[0];
            let ftables = [];
            let formDynamic = data[0].filter(x => x.is_type == 3);
            if (formDynamic.length > 0) {
                formDynamic.forEach(function (r) {
                    var fr = [];
                    let parentProps = data[0].filter(x => x.is_parent_id == r.request_formd_id);
                    if (parentProps.length > 0) {
                        parentProps.forEach(function (rr) {
                            var o = { ...rr };
                            o.STTRow = 0;
                            fr.push(o);
                        });
                    }
                    ftables.push([]);
                    ftables[ftables.length - 1].push(fr);
                });
            }
            Ftables.value = ftables;
        }
    })
    .catch((error) => {
        if (error.status === 401) {
            swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
            });
            return;
        }
    });
};
const listManager = ref([]);
const listFollower = ref([]);
const request_LoadSignUser = (Form_ID, Team_ID) => {
    listManager.value = []; // quanlys
    listFollower.value = []; // theodois
    axios({
        method: "post",
        url: basedomainURL + "api/request/getData",
        data: {
            str: encr(
                JSON.stringify({
                    proc: "request_get_signuser",
                    par: [ 
                        { par: "request_form_id", va: Form_ID },
                        { par: "request_team_id", va: Team_ID },
                    ],
                }),
                SecretKey,
                cryoptojs
            ).toString(),
        },
        headers: {
			Authorization: `Bearer ${store.getters.token}`,
		},
    })
    .then((response) => {
        if (response.data.err != '1') {
            var data = JSON.parse(res.data.data);
            listSignUser.value = data[0];
            data[0].forEach((r) => {
                r.LoaiGroupName = r.group_name;
                if (r.type_process == 0) {
                    r.LoaiGroupName += ' (Duyệt 1 trong nhiều)';
                } else if (r.type_process == 1) {
                    r.LoaiGroupName += ' (Duyệt tuần tự)';
                } else if (r.type_process == 2) {
                    r.LoaiGroupName += ' (Duyệt đồng thời)';
                }
                if (r.sign_user) {
                    r.sign_user = JSON.parse(r.sign_user);
                    r.sign_user.filter(x => parseInt(x.is_type) == 3).forEach((fl) => {
                        if (listFollower.value == null) {
                            listFollower.value = [];
                        }
                        listFollower.value.push(fl);
                    });
                }
                if (r.sign_user)
                    r.sign_user_copy = [...r.sign_user.filter(x => parseInt(x.is_type) !== 3)];
            });
            if (data[1] != null && data[1].length > 0) {
                listManager.value = data[1];
            }
        }
    })
    .catch((error) => {
        if (error.status === 401) {
            swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
            });
            return;
        }
    });
};
const filterForm = () => {
    let objRequestForm = request_data.value.request_form_id;
    var fo = props.dictionarys[0].find(x => x.request_form_id == objRequestForm.request_form_id);
    listSignUser.value = []; // SignUser
    formDS.value = [];
    if (fo != null) {
        if (fo.is_use_all) {
            filterTeamRequest.value = props.dictionarys[2];
        } else {
            filterTeamRequest.value = props.dictionarys[2].filter(x => props.dictionarys[3].filter(a => a.request_form_id == objRequestForm.request_form_id).length > 0);
        }
        request_data.value.is_notify_manager = fo.is_notify_manage;
        request_data.value.is_private = fo != null ? fo.is_private : false;
        var ft = props.dictionarys[3].find(x => x.request_form_id == objRequestForm.request_form_id && x.request_team_id == request_data.value.request_team_id);
        request_data.value.times_processing_max = ft != null ? ft.IsSLA : 8;
        request_data.value.is_change_process = ft != null ? ft.IsChangeQT : false;
        request_data.value.is_skip = ft != null ? (ft.IsSkip || false) : false;
        request_data.value.type_process = ft != null ? ft.is_process : 1;
        //
        loadFormD(objRequestForm.request_form_id);
        if (request_data.value.request_team_id) {
            request_LoadSignUser(objRequestForm.request_form_id, request_data.value.request_team_id);
        }
    } else {// Đề xuất trực tiếp
        request_data.value.times_processing_max = 8; // Số giờ xử lý tối đa
        request_data.value.is_change_process = true;
        request_data.value.is_skip = false;
        request_data.value.type_process = 1;
    }
};
const formDS_filter = (parentFilter) => {
    return formDS.value.filter(x => x.is_parent_id == parentFilter);
};

const renderWidth = (kieu) => {
    switch (kieu) {
        case "checkbox":
        case "radio":
            return 60;
        case "date":
        case "time":
        case "datetime":
            return 160;
        default:
            return '';
    }
};
onMounted(() => {
    return {
        
    };
});
</script>
<template>
    <Dialog :header="props.headerDialog" 
        v-model:visible="props.displayDialog" 
        :style="{ width: '50vw' }" 
        :closable="false"
		:modal="true"
        :position="'top'"
    >
        <form @submit.prevent="">
            <div class="grid formgrid m-0">
                <div class="col-12 md:col-12 flex p-0">
                    <div class="col-4 md:col-8 p-0 pr-3">
                        <div class="form-group">
                            <label>Loại đề xuất (Không chọn nếu là đề xuất trực tiếp)</label>
                            <Dropdown class=""
                                v-model="request_data.request_form_id" 
                                :options="props.listTypeRequest" 
                                optionLabel="label" 
                                optionGroupLabel="label" 
                                optionGroupChildren="items" 
                                :filter="true"
                                :showClear="true"
                                placeholder="-- Chọn loại đề xuất --"
                                @change="filterForm()"
                            >
                                <template #optiongroup="slotProps">
                                    <div class="flex align-items-center">
                                        <div>{{ slotProps.option.label }}</div>
                                    </div>
                                </template>
                            </Dropdown>
                        </div>
                    </div>
                    <div class="col-8 md:col-4 p-0">
                        <div class="form-group">
                            <label>Ngày lập</label>
                            <Calendar
                                :showIcon="true"
                                class="ip36"
                                autocomplete="on"
                                inputId="time24"
                                v-model="request_data.created_date"
                                placeholder="DD/MM/yyyy"
                                disabled
                            />
                        </div>
                    </div>
                </div>
                <div class="col-12 md:col-12 flex p-0">                    
                    <div class="form-group">
                        <label class="text-left flex p-0" style="align-items:center;">
                            Tên đề xuất <span class="redsao pl-1"> (*)</span>                        
                        </label>
                        <Textarea v-model="request_data.request_name" 
                            spellcheck="false" 
                            class="p-2" 
                            autoResize
                            rows="1" 
                            :class="{ 'p-invalid': v$.request_name.$invalid && submitted, }" 
                        />
                    </div>
				</div>
                <div class="field col-12 md:col-12 flex p-0"
                    v-if="(v$.request_name.required.$invalid && submitted) || v$.request_name.$pending.$response || (v$.request_name.maxLength.$invalid && submitted) || v$.request_name.maxLength.$pending.$response">
                    <div class="pl-0 flex" style="flex-direction: column;">
                        <small v-if="(v$.request_name.required.$invalid && submitted) || v$.request_name.$pending.$response" 
                            class="col-12 pl-0 p-error">
                            <span class="col-12 p-0">
                                {{
                                    v$.request_name.required.$message
                                        .replace("Value", "Tên đề xuất")
                                        .replace("is required", "không được để trống")
                                }}
                            </span>
                        </small>
                        <small class="col-12 pl-0 p-error mt-2" 
                            v-if="(v$.request_name.maxLength.$invalid && submitted) || v$.request_name.maxLength.$pending.$response">
                            <span class="col-12 p-0">
                                {{
                                    v$.request_name.maxLength.$message.replace(
                                        "The maximum length allowed is",
                                        "Tên đề xuất không được vượt quá"
                                    )
                                }}
                                ký tự
                            </span>
                        </small>
                    </div>
				</div>
                <div class="col-12 md:col-12 flex p-0">
                    <div class="col-4 md:col-8 p-0 pr-3">
                        <div class="form-group">
                            <label>Mức độ ưu tiên</label>
                            <Dropdown class=""
                                v-model="request_data.priority_level" 
                                :options="listPriorityLevel" 
                                optionLabel="name" 
                                optionValue="code" 
                                :filter="true"
                                placeholder="-- Chọn mức độ ưu tiên --"
                            >
                            </Dropdown>
                        </div>
                    </div>
                    <div class="col-8 md:col-4 p-0">
                        <div class="form-group">
                            <label>Số giờ xử lý</label>
                            <InputNumber class="ip36"
                                spellcheck="false"
                                :min="0"
                                v-model="request_data.times_processing_max"
                            />
                        </div>
                    </div>
                </div>
                <div class="col-12 md:col-12 flex p-0">
                    <div class="form-group">
                        <label class="text-left flex p-0" style="align-items:center;">
                            Nội dung <span class="redsao pl-1"> (*)</span>                        
                        </label>
                        <Textarea v-model="request_data.content" 
                            spellcheck="false" 
                            class="p-2" 
                            autoResize
                            rows="2" 
                            :class="{ 'p-invalid': v$.content.$invalid && submitted, }" 
                        />
                    </div>
				</div>
                <div class="field col-12 md:col-12 flex p-0"
                    v-if="(v$.content.required.$invalid && submitted) || v$.content.$pending.$response || (v$.content.maxLength.$invalid && submitted) || v$.content.maxLength.$pending.$response">
                    <div class="pl-0 flex" style="flex-direction: column;">
                        <small v-if="(v$.content.required.$invalid && submitted) || v$.content.$pending.$response" 
                            class="col-12 pl-0 p-error">
                            <span class="col-12 p-0">
                                {{
                                    v$.content.required.$message
                                        .replace("Value", "Nội dung đề xuất")
                                        .replace("is required", "không được để trống")
                                }}
                            </span>
                        </small>
                        <small class="col-12 pl-0 p-error mt-2" 
                            v-if="(v$.content.maxLength.$invalid && submitted) || v$.content.maxLength.$pending.$response">
                            <span class="col-12 p-0">
                                {{
                                    v$.content.maxLength.$message.replace(
                                        "The maximum length allowed is",
                                        "Nội dung đề xuất không được vượt quá"
                                    )
                                }}
                                ký tự
                            </span>
                        </small>
                    </div>
				</div>
                <!-- <div class="col-12 md:col-12 flex p-0" v-if="formDS && formDS_filter().length > 0"> -->
                <div class="col-12 md:col-12 flex p-0" v-if="false">
                    <div class="formd-0">
                        <div class="formd" 
                            :class="d.is_class"
                            v-for="(d, idxForm) in formDS_filter(null)"
                            :key="idxForm" 
                        >
                            <div v-if="d.is_type != 3">
                                <div class="form-group formlabel" style="margin-top:15px" v-if="d.is_label">{{ d.ten_truong }}</div>
                                <div class="form-group" v-else>
                                    <div v-if="d.kieu_truong != 'checkbox' && d.kieu_truong != 'radio' && d.is_type != 2">
                                        <label class="fw-500">{{ d.ten_truong }}</label>
                                        <span v-if="d.is_required" class="redsao pl-1">(*)</span> 
                                    </div>
                                    <div v-switch="d.kieu_truong">
                                        <div v-case="email">
                                            <InputText :max="d.is_length" 
                                                type="email" 
                                                spellcheck="false" 
                                                v-model="d.value_field" 
                                                :required="d.is_required" 
                                                class="form-control" 
                                            />
                                        </div>
                                        <div v-case="varchar|nvarchar">
                                            <InputText :max="d.is_length" 
                                                type="text" 
                                                v-if="d.is_length <= 500" 
                                                spellcheck="false" 
                                                v-model="d.value_field" 
                                                :required="d.is_required" 
                                                class="form-control" 
                                            />
                                        </div>
                                        <div v-case="int|float">
                                            <InputNumber smart-float 
                                                max="10" 
                                                type="text" 
                                                spellcheck="false" 
                                                v-model="d.value_field" 
                                                :required="d.is_required" 
                                                class="form-control" 
                                            />
                                        </div>
                                        <div v-case="textarea">
                                            <Textarea :max="d.is_length" 
                                                spellcheck="false" 
                                                v-model="d.value_field" 
                                                :required="d.is_required"
                                                class="form-control" 
                                                rows="2"
                                            />
                                        </div>
                                        <div v-case="checkbox">
                                            <div class="form-group">
                                                <InputSwitch v-model="d.value_field" />
                                                <label>{{ d.ten_truong }}</label>
                                            </div>
                                        </div>
                                        <div v-case="radio">
                                            <div class="form-group">
                                                <RadioButton :value="d.request_formd_id" v-model="request_data.Radio"/>
                                                <label>{{ d.ten_truong }}</label>
                                            </div>
                                        </div>
                                        <div v-case="date|datetime" class="dropdown datetimeinput">
                                            <Calendar
                                                :showIcon="true"
                                                class="form-control ip36"
                                                autocomplete="on"
                                                inputId="time24"
                                                v-model="d.value_field"
                                                placeholder="dd/MM/yyyy"
                                                :required="d.is_required" 
                                            />
                                        </div>
                                        <div v-case="time">
                                            <!-- <Input type="text" class="form-control" v-model="d.value_field" placeholder="HH:MM:SS" onkeypress="formatTime(this)" max="8" :required="d.is_required" /> -->
                                            <Calendar
                                                :showIcon="true"
                                                class="form-control ip36"
                                                autocomplete="on"
                                                inputId="time24"
                                                v-model="d.value_field"
                                                placeholder="HH:mm"
                                                timeOnly
                                                :required="d.is_required" 
                                            />
                                        </div>
                                    </div>
                                </div>
                                <div class="formd" 
                                    :class="dc.is_class"
                                    v-for="(dc, idxChildForm) in formDS_filter(d.request_formd_id)"
                                    :key="idxChildForm" 
                                ></div>
                            </div>
                            <div v-if="d.is_type == 3">
                                <div class="form-group" style="margin-top:15px" v-if="d.is_label">
                                    <div class="form-group formlabel" style="margin-bottom:0;display:flex">
                                        <label>{{ d.ten_truong }}</label>
                                        <a v-if="request_data.IsEdit" @click="addRow($index)">
                                            <i class="pi pi-plus-circle"></i>
                                            <span class="pl-2">Thêm dòng</span>
                                        </a>                                        
                                        <div style="flex:1"></div>
                                        <a v-if="request_data.IsEdit" @click="openRelate(null,'srequest',0)">
                                            <i class="pi pi-sliders-h"></i>
                                            <span class="pl-2">Tổng hợp đề xuất</span>
                                        </a>
                                    </div>
                                    <table class="table table-bordered">
                                        <thead style="background-color:#eee">
                                            <tr>
                                                <th :width="renderWidth(th.kieu_truong)" 
                                                    v-for="(dc, idxChildForm) in formDS_filter(d.request_formd_id)"
                                                    :key="idxChildForm"
                                                >
                                                    {{dc.TenTruong}}
                                                </th>
                                                <th width="36"></th>
                                            </tr>
                                        </thead>
                                        <tbody ng-init="$pindex=$index">
                                            <tr v-for="(r, indexF) in Ftables[$pindex]" :key="indexF">
                                                <td v-for="td in r">
                                                    <div ng-switch="td.kieu_truong">
                                                        <div v-case="email">
                                                            <InputText :max="td.is_length" 
                                                                type="email" 
                                                                spellcheck="false" 
                                                                v-model="td.value_field" 
                                                                :required="td.is_required" 
                                                                class="form-control" 
                                                            />
                                                        </div>
                                                        <div v-case="varchar|nvarchar">
                                                            <InputText :max="td.is_length" 
                                                                type="text" 
                                                                v-if="td.is_length <= 500" 
                                                                spellcheck="false" 
                                                                v-model="td.value_field" 
                                                                :required="td.is_required" 
                                                                class="form-control" 
                                                            />
                                                        </div>
                                                        <div v-case="int|float">
                                                            <InputNumber smart-float 
                                                                max="10" 
                                                                type="text" 
                                                                spellcheck="false" 
                                                                v-model="td.value_field" 
                                                                :required="td.is_required" 
                                                                class="form-control" 
                                                            />
                                                        </div>
                                                        <div v-case="textarea">
                                                            <Textarea :max="td.is_length" 
                                                                spellcheck="false" 
                                                                v-model="td.value_field" 
                                                                :required="td.is_required"
                                                                class="form-control" 
                                                                rows="2"
                                                            />
                                                        </div>
                                                        <div v-case="checkbox">
                                                            <div class="form-group">
                                                                <InputSwitch v-model="td.value_field" />
                                                                <label>{{ td.ten_truong }}</label>
                                                            </div>
                                                        </div>
                                                        <div v-case="radio">
                                                            <div class="form-group">
                                                                <RadioButton :value="td.request_formd_id" v-model="request_data.Radio"/>
                                                                <label>{{ td.ten_truong }}</label>
                                                            </div>
                                                        </div>
                                                        <div v-case="date|datetime" class="dropdown datetimeinput">
                                                            <Calendar
                                                                :showIcon="true"
                                                                class="form-control ip36"
                                                                autocomplete="on"
                                                                inputId="time24"
                                                                v-model="td.value_field"
                                                                placeholder="dd/MM/yyyy"
                                                                :required="td.is_required" 
                                                            />
                                                        </div>
                                                        <sdiv v-case="time">
                                                            <!-- <input type="text" class="form-control" v-model="td.value_field" placeholder="HH:MM:SS" onkeypress="formatTime(this)" MaxLength="8" :required="td.is_required" /> -->
                                                            <Calendar
                                                                :showIcon="true"
                                                                class="form-control ip36"
                                                                autocomplete="on"
                                                                inputId="time24"
                                                                v-model="td.value_field"
                                                                placeholder="HH:mm"
                                                                timeOnly
                                                                :required="td.is_required" 
                                                            />
                                                        </sdiv>
                                                    </div>
                                                </td>
                                                <td style="text-align:center">
                                                    <a v-if="Ftables[$pindex].length>1" 
                                                        @click="removeRow($pindex, indexF)">
                                                        <i class="pi pi-trash"></i>
                                                    </a>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-12 md:col-12 flex p-0">
                    <div class="form-group">
                        <label>Lập đề xuất của Team</label>
                        <Dropdown class=""
                            v-model="request_data.request_team_id" 
                            :options="filterTeamRequest" 
                            optionLabel="request_team_name" 
                            optionValue="request_team_id" 
                            :filter="true"
                            placeholder="-- Chọn team --"
                        >
                        </Dropdown>
                    </div>
                </div>
                <div class="col-12 md:col-12 p-0" v-if="request_data.status != 1 && request_data.status != 2">
                    <div class="col-12 md:col-12 flex p-0 mb-3">
                        <label style="font-weight:bold;">Người duyệt phiếu</label>
                    </div>
                    <div class="col-12 md:col-12 flex p-0" 
                        v-if="request_data.request_form_id && listSignUser.length > 0">
                        <AvatarGroup>                            
                            <Avatar v-for="(us, idxUser) in listSignUser" :key="idxUser"
                                v-bind:label="us.avatar ? '' : (us.last_name ?? '').substring(0, 1)"
                                v-bind:image="
                                    us.avatar
                                    ? basedomainURL + us.avatar
                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                "
                                v-tooltip="{ value: us.LoaiGroupName, escape: true }"
                                style="background-color: #2196f3; color: #ffffff;"
                                :style="{ background: bgColor[idxUser % 7], }"
                                class="text-avatar"
                                size="large" 
                                shape="circle" 
                            />
                            <!-- <template v-for="(us, idxUser) in listSignUser" :key="idxUser">
                                <Avatar v-if="idxUser < 3"
                                    v-bind:label="us.avatar ? '' : (us.last_name ?? '').substring(0, 1)"
                                    v-bind:image="
                                        us.avatar
                                        ? basedomainURL + us.avatar
                                        : basedomainURL + '/Portals/Image/noimg.jpg'
                                    "
                                    style="background-color: #2196f3; color: #ffffff;"
                                    :style="{ background: bgColor[idxUser % 7], }"
                                    class="text-avatar"
                                    size="large" 
                                    shape="circle" 
                                />
                            </template>                            
                            <Avatar :label="'+' + (listSignUser.length - 3)" 
                                style="background-color: #2196f3; color: #ffffff;" 
                                class="text-avatar"
                                size="large" 
                                shape="circle"
                                v-if="listSignUser.length > 3"
                            /> -->
                        </AvatarGroup>
                    </div>
                    <div class="col-12 md:col-12 flex p-0" 
                        v-if="request_data.is_change_process && request_data.request_form_id">

                    </div>
                    <div class="col-12 md:col-12 p-0" v-if="!request_data.request_form_id">
                        <div class="col-12 md:col-12 flex p-0">
                            <div class="form-group">
                                <label>Loại quy trình duyệt</label>
                                <Dropdown class=""
                                    v-model="request_data.type_process" 
                                    :options="list_type_process" 
                                    optionLabel="name" 
                                    optionValue="code" 
                                    :filter="true"
                                    placeholder="-- Chọn loại quy trình --"
                                >
                                </Dropdown>
                            </div>
                        </div>
                        <div class="col-12 md:col-12 flex p-0">
                            <div class="form-group">
                                <label>Người duyệt
                                    <i class="pi pi-user-plus ml-2"
                                        v-tooltip.top="'Chọn người duyệt'"
                                        @click="showModalUser(false, 0)"
                                        style="cursor: pointer; color: #2196f3"
                                    ></i>
                                </label>
                                <MultiSelect :options="props.dictionarys[4]"
                                    :filter="true"
                                    :showClear="true"
                                    optionLabel="full_name"
                                    placeholder="Chọn người duyệt"
                                    v-model="listUserApproved"
                                    class="ip36 hide-icon-down"
                                    style="height: auto; min-height: 36px"
                                    disabled
                                    v-if="listUserApproved.length == 0"
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
                                                                v-bind:label="value.avatar ? '' : (value.last_name ?? '').substring(0, 1)"
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
                                                        <!-- <span tabindex="0"
                                                            class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                                            @click="removeMember(value, listUserApproved)"
                                                        ></span> -->
                                                    </div>
                                                </Chip>
                                            </li>
                                        </ul>
                                        <span v-else> {{ slotProps.placeholder }} </span>
                                    </template>
                                    <!-- <template #option="slotProps">
                                        <div v-if="slotProps.option" class="flex">
                                            <div class="format-center">
                                                <Avatar
                                                    v-bind:label="slotProps.option.avatar ? '' : slotProps.option.last_name.substring(0, 1)"
                                                    v-bind:image="
                                                        slotProps.option.avatar
                                                        ? basedomainURL + slotProps.option.avatar
                                                        : basedomainURL + '/Portals/Image/noimg.jpg'
                                                    "
                                                    style="background-color: #2196f3; color: #ffffff; width: 3rem; height: 3rem;"
                                                    :style="{ background: bgColor[slotProps.index % 7], }"
                                                    class="text-avatar"
                                                    size="xlarge"
                                                    shape="circle"
                                                />
                                            </div>
                                            <div class="ml-3" 
                                                style="display: flex; flex-direction: column; justify-content: center;">
                                                <div class="mb-1">{{ slotProps.option.full_name }}</div>
                                                <div class="description">
                                                    <div>{{ slotProps.option.position_name }}</div>
                                                    <div>{{ slotProps.option.department_name }}</div>
                                                </div>
                                            </div>
                                        </div>
                                        <span v-else> Chưa có dữ liệu </span>
                                    </template> -->
                                </MultiSelect>
                                <OrderList class="order-list-screen w-full"
                                    v-model="listUserApproved"
                                    :listStyle="'height:185px;'"
                                    dataKey="user_id"
                                    v-else	
                                >
                                    <template #item="slotProps">
                                        <div class="product-item flex" 
                                            style="align-items: center;justify-content:space-between;">
                                            <div class="flex">
                                                <div class="format-center">
                                                    <Avatar
                                                        v-bind:label="slotProps.item.avatar ? '' : slotProps.item.last_name.substring(0, 1)"
                                                        v-bind:image="
                                                            slotProps.item.avatar
                                                            ? basedomainURL + slotProps.item.avatar
                                                            : basedomainURL + '/Portals/Image/noimg.jpg'
                                                        "
                                                        style="background-color: #2196f3; color: #ffffff; width: 3rem; height: 3rem;"
                                                        :style="{ background: bgColor[slotProps.index % 7], }"
                                                        class="text-avatar"
                                                        size="xlarge"
                                                        shape="circle"
                                                    />
                                                </div>
                                                <div class="ml-3" 
                                                    style="display: flex; flex-direction: column; justify-content: center;">
                                                    <div class="mb-1">{{ slotProps.item.full_name }}</div>
                                                    <div class="description">
                                                        <div>{{ (slotProps.item.role_name || '') + (slotProps.item.position_name ? (' | ' + slotProps.item.position_name) : '') }}</div>
                                                        <div>{{ slotProps.item.department_name }}</div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div>
                                                <Button
                                                    @click="removeMember(slotProps.item, listUserApproved, 'approver')"
                                                    class="p-button-rounded p-button-danger p-button-outlined p-0"
                                                    type="button"
                                                    style="height:2rem; width:2rem;"
                                                    icon="pi pi-times"
                                                    v-tooltip="'Xóa'"
                                                />
                                            </div>
                                        </div>
                                    </template>
                                </OrderList>
                            </div>
                        </div>
                    </div>
                    <div class="col-12 md:col-12 flex p-0">
                        <div class="form-group">
                            <label>Người quản lý
                                <i class="pi pi-user-plus ml-2"
                                    v-tooltip.top="'Chọn người quản lý'"
                                    @click="showModalUser(false, 1)"
                                    style="cursor: pointer; color: #2196f3"
                                ></i>
                            </label>
                            <MultiSelect :options="props.dictionarys[4]"
                                :filter="true"
                                :showClear="true"
                                optionLabel="full_name"
                                placeholder="Chọn người quản lý"
                                v-model="listUserManage"
                                class="ip36 hide-icon-down"
                                style="height: auto; min-height: 36px"
                                disabled
                                v-if="listUserManage.length == 0"
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
                                                            v-bind:label="value.avatar ? '' : (value.last_name ?? '').substring(0, 1)"
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
                                                    <!-- <span tabindex="0"
                                                        class="p-chip-remove-icon pi pi-times-circle format-flex-center"
                                                        @click="removeMember(value, listUserManage)"
                                                    ></span> -->
                                                </div>
                                            </Chip>
                                        </li>
                                    </ul>
                                    <span v-else> {{ slotProps.placeholder }} </span>
                                </template>
                                <!-- <template #option="slotProps">
                                    <div v-if="slotProps.option" class="flex">
                                        <div class="format-center">
                                            <Avatar
                                                v-bind:label="slotProps.option.avatar ? '' : slotProps.option.last_name.substring(0, 1)"
                                                v-bind:image="
                                                    slotProps.option.avatar
                                                    ? basedomainURL + slotProps.option.avatar
                                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                                "
                                                style="background-color: #2196f3; color: #ffffff; width: 3rem; height: 3rem;"
                                                :style="{ background: bgColor[slotProps.index % 7], }"
                                                class="text-avatar"
                                                size="xlarge"
                                                shape="circle"
                                            />
                                        </div>
                                        <div class="ml-3" 
                                            style="display: flex; flex-direction: column; justify-content: center;">
                                            <div class="mb-1">{{ slotProps.option.full_name }}</div>
                                            <div class="description">
                                                <div>{{ slotProps.option.position_name }}</div>
                                                <div>{{ slotProps.option.department_name }}</div>
                                            </div>
                                        </div>
                                    </div>
                                    <span v-else> Chưa có dữ liệu </span>
                                </template> -->
                            </MultiSelect>
                            <OrderList class="order-list-screen w-full"
                                v-model="listUserManage"
                                :listStyle="'height:185px;'"
                                dataKey="user_id"
                                v-else	
                            >
                                <template #item="slotProps">
                                    <div class="product-item flex" 
                                        style="align-items: center;justify-content:space-between;">
                                        <div class="flex">
                                            <div class="format-center">
                                                <Avatar
                                                    v-bind:label="slotProps.item.avatar ? '' : slotProps.item.last_name.substring(0, 1)"
                                                    v-bind:image="
                                                        slotProps.item.avatar
                                                        ? basedomainURL + slotProps.item.avatar
                                                        : basedomainURL + '/Portals/Image/noimg.jpg'
                                                    "
                                                    style="background-color: #2196f3; color: #ffffff; width: 3rem; height: 3rem;"
                                                    :style="{ background: bgColor[slotProps.index % 7], }"
                                                    class="text-avatar"
                                                    size="xlarge"
                                                    shape="circle"
                                                />
                                            </div>
                                            <div class="ml-3" 
                                                style="display: flex; flex-direction: column; justify-content: center;">
                                                <div class="mb-1">{{ slotProps.item.full_name }}</div>
                                                <div class="description">
                                                    <div>{{ (slotProps.item.role_name || '') + (slotProps.item.position_name ? (' | ' + slotProps.item.position_name) : '') }}</div>
                                                    <div>{{ slotProps.item.department_name }}</div>
                                                </div>
                                            </div>
                                        </div>
                                        <div>
                                            <Button
                                                @click="removeMember(slotProps.item, listUserManage, 'manager')"
                                                class="p-button-rounded p-button-danger p-button-outlined p-0"
                                                type="button"
                                                style="height:2rem; width:2rem;"
                                                icon="pi pi-times"
                                                v-tooltip="'Xóa'"
                                            />
                                        </div>
                                    </div>
                                </template>
                            </OrderList>
                        </div>
                    </div>
                    <div class="col-12 md:col-12 flex p-0">
                        <div class="form-group">
                            <label>Người theo dõi
                                <i class="pi pi-user-plus ml-2"
                                    v-tooltip.top="'Chọn người theo dõi'"
                                    @click="showModalUser(false, 2)"
                                    style="cursor: pointer; color: #2196f3"
                                ></i>
                            </label>
                            <MultiSelect :options="props.dictionarys[4]"
                                :filter="true"
                                :showClear="true"
                                optionLabel="full_name"
                                placeholder="Chọn người theo dõi"
                                v-model="listUserFollow"
                                class="ip36"
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
                                                            v-bind:label="value.avatar ? '' : (value.last_name ?? '').substring(0, 1)"
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
                                                        @click="removeMember(value, listUserFollow)"
                                                    ></span>
                                                </div>
                                            </Chip>
                                        </li>
                                    </ul>
                                    <span v-else> {{ slotProps.placeholder }} </span>
                                </template>
                                <template #option="slotProps">
                                    <div v-if="slotProps.option" class="flex">
                                        <div class="format-center">
                                            <Avatar
                                                v-bind:label="slotProps.option.avatar ? '' : slotProps.option.last_name.substring(0, 1)"
                                                v-bind:image="
                                                    slotProps.option.avatar
                                                    ? basedomainURL + slotProps.option.avatar
                                                    : basedomainURL + '/Portals/Image/noimg.jpg'
                                                "
                                                style="background-color: #2196f3; color: #ffffff; width: 3rem; height: 3rem;"
                                                :style="{ background: bgColor[slotProps.index % 7], }"
                                                class="text-avatar"
                                                size="xlarge"
                                                shape="circle"
                                            />
                                        </div>
                                        <div class="ml-3" 
                                            style="display: flex; flex-direction: column; justify-content: center;">
                                            <div class="mb-1">{{ slotProps.option.full_name }}</div>
                                            <div class="description">
                                                <div>{{ slotProps.option.position_name }}</div>
                                                <div>{{ slotProps.option.department_name }}</div>
                                            </div>
                                        </div>
                                    </div>
                                    <span v-else> Chưa có dữ liệu </span>
                                </template>
                            </MultiSelect>
                        </div>
                    </div>
                </div>
                <div class="col-4 md:col-4 flex p-0">
                    <div class="form-group mb-2">
                        <div class="field-checkbox flex"
                            style="height: 100%"
                        >
                            <InputSwitch v-model="request_data.is_evaluate" />
                            <label>Đánh giá kết quả</label>
                        </div>
                    </div>
                </div>
                <div class="col-4 md:col-4 flex p-0">
                    <div class="form-group mb-2">
                        <div class="field-checkbox flex"
                            style="height: 100%"
                        >
                            <InputSwitch v-model="request_data.is_mail" />
                            <label>Gửi email</label>
                        </div>
                    </div>
                </div>
                <div class="col-4 md:col-4 flex p-0">
                    <div class="form-group mb-2">
                        <div class="field-checkbox flex"
                            style="height: 100%"
                        >
                            <InputSwitch v-model="request_data.is_sign_ca" />
                            <label>Yêu cầu chữ ký số (CA)</label>
                        </div>
                    </div>
                </div>
                <div class="col-4 md:col-4 flex p-0" v-if="request_data.request_form_id">
                    <div class="form-group">
                        <div class="field-checkbox flex"
                            style="height: 100%"
                        >
                            <InputSwitch v-model="request_data.is_general_request" />
                            <label>Tổng hợp đề xuất</label>
                        </div>
                    </div>
                </div>
                <div class="col-4 md:col-4 flex p-0">
                    <div class="form-group">
                        <div class="field-checkbox flex"
                            style="height: 100%"
                        >
                            <InputSwitch v-model="request_data.is_security" />
                            <label>Kích hoạt bảo mật</label>
                        </div>
                    </div>
                </div>
                <div class="col-12 md-col-12">
                    <div class="form-group">
                        <label>Tệp đính kèm</label>
                        <FileUpload
                            :multiple="true"
                            :show-upload-button="false"
                            :show-cancel-button="true"
                            @remove="removeFile"
                            @select="selectFile"
                            accept=""
                            choose-label="Chọn tệp"
                            cancel-label="Hủy"
                        >
                            <template #empty>
                                <p>Kéo thả tệp đính kèm vào đây.</p>
                            </template>
                        </FileUpload>
                        <div v-if="request_data.files != null && request_data.files.length > 0">
                            <DataView
                                :lazy="true"
                                :value="request_data.files"
                                :rowHover="true"
                                :scrollable="true"
                                class="w-full h-full ptable p-datatable-sm flex flex-column"
                                layout="list"
                                responsiveLayout="scroll"
                            >
                                <template #list="slotProps">
                                    <div class="w-full">
                                        <Toolbar class="w-full">
                                            <template #start>
                                                <div class="flex align-items-center"
                                                    @click="goFile(slotProps.data)"
                                                >
                                                    <img
                                                        class="mr-2"
                                                        :src="basedomainURL + '/Portals/Image/file/' + slotProps.data.file_type + '.png'"
                                                        style="object-fit: contain"
                                                        width="40"
                                                        height="40"
                                                    />
                                                    <span style="line-height: 1.5">
                                                        {{ slotProps.data.file_name }}</span
                                                    >
                                                </div>
                                            </template>
                                            <template #end>
                                                <Button
                                                    icon="pi pi-times"
                                                    class="p-button-rounded p-button-danger"
                                                    @click="deleteFile(slotProps.data, slotProps.data.index)"
                                                />
                                            </template>
                                        </Toolbar>
                                    </div>
                                </template>
                            </DataView>
                        </div>
                    </div>
                </div>
            </div>
        </form>
		<template #footer>
			<Button class="p-button-secondary" label="Hủy" icon="pi pi-times" @click="props.closeDialog" />
			<Button label="Lưu" icon="pi pi-check" @click="saveData(!v$.$invalid)" />
		</template>
    </Dialog>
    <!-- Component tree user -->
    <treeuser
		v-if="displayDialogUser === true"
		:headerDialog="headerDialogUser"
		:displayDialog="displayDialogUser"
		:closeDialog="closeDialogUser"
		:one="is_one"
		:selected="selectedUser"
		:choiceUser="choiceUser"
	/>
</template>
<style scoped>
@import url(../style_request.css);
</style>
<style lang="scss" scoped>
    ::v-deep(.text-avatar) {
        .p-avatar-text {
            color: #000000;
            font-size: 1rem;
        }
    }
    ::v-deep(.form-group) {
        .text-avatar .p-avatar-text {
            color: #000000;
            font-weight: bold;
            font-size: 0.75rem;
        }
        .p-chip img {
            margin: 0;
        }
    }
    ::v-deep(.hide-icon-down) {
        .p-multiselect-trigger span.pi-chevron-down {
            display: none;
        }
    }
</style>