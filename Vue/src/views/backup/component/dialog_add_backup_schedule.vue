<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";

const toast = useToast();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const basedomainURL = baseURL;
const config = {
	headers: { Authorization: `Bearer ${store.getters.token}` },
};
const props = defineProps({
	key: Number,
	headerDialog: String,
	displayDialog: Boolean,
	dataForm: Object,
    reloadData: Function,
	closeDialog: Function,
});
const rules = {
	title: {
		required,
		maxLength: maxLength(500),
		$errors: [
			{
				$property: "title",
				$validator: "required",
				$message: "Nội dung không được để trống!",
			},
		],
	},
    type_backup: {
		required,
		$errors: [
			{
				$property: "type_backup",
				$validator: "required",
				$message: "Loại backup không được để trống!",
			},
		],
	},    
    type_path_backup: {
		required,
		$errors: [
			{
				$property: "type_path_backup",
				$validator: "required",
				$message: "Nơi lưu dữ liệu không được để trống!",
			},
		],
	},    
    folder_backup_path: {
		required,
		$errors: [
			{
				$property: "folder_backup_path",
				$validator: "required",
				$message: "Folder backup không được để trống!",
			},
		],
	},
};
const backup_form = ref(props.dataForm);
const v$ = useVuelidate(rules, backup_form);
const submitted = ref(false);
const typegroups = ref([
    { name: "Database", code: 0 },
    { name: "File", code: 1 },
    { name: "Database & File", code: 2 },
]);
const typeSaveDatas = ref([
    { name: "Mặc định", code: 0 },
    { name: "Tùy chỉnh", code: 1 },
]);
const typeSaveDatasOther = ref([
    { name: "Local", code: 0 },
    { name: "FTP", code: 1 },
]);
const typeBackupDatas = ref([
    { name: "Hàng ngày", code: 0 },
    { name: "Hàng tuần", code: 1 },
    { name: "Hàng tháng", code: 2 },
]);
const dateBackupWeek = ref([
    { name: "Chủ nhật", code: 1 },
    { name: "Thứ 2", code: 2 },
    { name: "Thứ 3", code: 3 },
    { name: "Thứ 4", code: 4 },
    { name: "Thứ 5", code: 5 },
    { name: "Thứ 6", code: 6 },
    { name: "Thứ 7", code: 7 },
]);
const dateBackupMonth = ref([]);
const renderDayMonth = () => {
    dateBackupMonth.value = [];
    for (let i = 1; i <= 31; i++) {
        if (i < 10) {
            dateBackupMonth.value.push({ name: '0' + i, code: i});
        }
        else {
            dateBackupMonth.value.push({ name: i.toString(), code: i});
        }
    }
};
const isSaving = ref(false);
const saveData = (isFormValid, type) => {
    submitted.value = true;    
    if (!isFormValid) {
        return;
    }
    if (backup_form.value.type_time_backup != null && backup_form.value.time_backup == null) {
        swal.fire({
            title: "Thông báo",
            text: "Vui lòng chọn thời gian backup dữ liệu!",
            icon: "warning",
            confirmButtonText: "OK",
        });
        return;
    }
    if ((backup_form.value.type_time_backup == 1 && backup_form.value.time_backup_week == null) || 
        (backup_form.value.type_time_backup == 2 && backup_form.value.time_backup_month == null)) {
        swal.fire({
            title: "Thông báo",
            text: "Vui lòng chọn ngày backup dữ liệu!",
            icon: "warning",
            confirmButtonText: "OK",
        });
        return;
    }
    if (backup_form.value.type_path_backup == 1 && backup_form.value.type_path_backup_other == null) {
        swal.fire({
            title: "Thông báo",
            text: "Vui lòng chọn vị trí backup dữ liệu!",
            icon: "warning",
            confirmButtonText: "OK",
        });
        return;
    }
    if (isSaving.value) {
        return;
    }
    let formData = new FormData();
	formData.append("model", JSON.stringify(backup_form.value));
    isSaving.value = true;
    swal.fire({
        width: 110,
        didOpen: () => {
            swal.showLoading();
        },
    });
    
    axios({
        method: backup_form.value.backup_id == null ? "post" : "put",
        url: baseURL + `/api/Backup/${backup_form.value.backup_id == null ? "Add_Form_Backup" : "Update_Form_Backup"}`,
        data: formData,
        headers: {
            Authorization: `Bearer ${store.getters.token}`,
        },
    })
    .then((response) => {
        swal.close();
        isSaving.value = false;
        if (response.data.err != "1") {
            toast.success("Thêm lịch backup thành công.");
            props.reloadData();
            if (type == true) {
                runBackup();
            }
            else {
                props.closeDialog();
            }
        }
        else {
            swal.fire({
                title: "Thông báo!",
                text: "Xảy ra lỗi khi lưu lịch backup",
                icon: "error",
                confirmButtonText: "OK",
            });
        }
    })
    .catch((error) => {
        swal.close();
        isSaving.value = false;
        if (error && error.status === 401) {
            swal.fire({
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
            });
        }
    });
};

const changePlaceBackup = () => {
    if (backup_form.value.type_path_backup == 0) {
        backup_form.value.folder_backup_path = "Default";
    }
};

// Thực hiện backup
const isRunningBackup = ref(false);
const callBackup = () => {
    if (backup_form.value.backup_id == null) {
        var isFormValid = (backup_form.value.title || '').trim() == '' || backup_form.value.type_backup == null || backup_form.value.type_path_backup == null || backup_form.value.folder_backup_path == null;
        saveData(!isFormValid, true);
    }
    else {
        runBackup();
    }
};
const runBackup = () => {
    isRunningBackup.value = true;
    // call nodejs backup file
    // ...
    // return (isRunningBackup.value = false) after backup success
};

onMounted(() => {
    renderDayMonth();
    return {};
});
</script>
<template>
    <Dialog :header="props.headerDialog" 
        v-model:visible="props.displayDialog" 
        :style="{ width: '50vw' }" 
        :closable="false"
		:modal="true"
    >		
        <form @submit.prevent="">
			<div class="grid formgrid m-0">
				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					<div class="col-2 text-left flex p-0" style="align-items:center;">
						Nội dung <span class="redsao pl-1"> (*)</span>                        
					</div>
					<Textarea v-model="backup_form.title" 
                        spellcheck="false" 
                        class="col-10 p-2" 
                        autoResize
						autofocus rows="2" 
                        :class="{ 'p-invalid': v$.title.$invalid && submitted, }" />
				</div>
				<div class="field col-12 md:col-12 flex p-0"
                    v-if="(v$.title.required.$invalid && submitted) || v$.title.$pending.$response || (v$.title.maxLength.$invalid && submitted) || v$.title.maxLength.$pending.$response">
					<div class="col-2 text-left"></div>
                    <div class="col-10 pl-0 flex" style="flex-direction: column;">
                        <small v-if="(v$.title.required.$invalid && submitted) || v$.title.$pending.$response" 
                            class="col-12 pl-0 p-error">
                            <span class="col-12 p-0">
                                {{
                                    v$.title.required.$message
                                        .replace("Value", "Nội dung")
                                        .replace("is required", "không được để trống")
                                }}
                            </span>
                        </small>
                        <small class="col-12 pl-0 p-error mt-2" 
                            v-if="(v$.title.maxLength.$invalid && submitted) || v$.title.maxLength.$pending.$response">
                            <span class="col-12 p-0">
                                {{
                                    v$.title.maxLength.$message.replace(
                                        "The maximum length allowed is",
                                        "Nội dung không được vượt quá"
                                    )
                                }}
                                ký tự
                            </span>
                        </small>
                    </div>
				</div>
				<div class="field col-12 md:col-12 algn-items-center flex p-0">
                    <div class="col-6 md:col-6 p-0 align-items-center flex">
                        <div class="col-4 text-left flex p-0" style="align-items:center;">
                            Loại backup dữ liệu <span class="redsao pl-1"> (*)</span>
                        </div>
                        <Dropdown :options="typegroups" 
                            :filter="true" 
                            v-model="backup_form.type_backup" 
                            optionLabel="name"
                            optionValue="code" 
                            placeholder="Chọn loại back up dữ liệu" 
                            class="col-8"
                            :class="{ 'p-invalid': v$.title.$invalid && submitted, }">
                        </Dropdown>
                    </div>
                    <div class="col-6 md:col-6 p-0 pl-3 align-items-center flex">
                        <div class="col-4 text-left flex p-0" style="align-items:center;">Thời gian backup</div>
                        <Calendar class="col-8 p-0" id="calendar-timeonly" placeholder="HH:mm" v-model="backup_form.time_backup" timeOnly />
                    </div>
				</div>
                <div class="field col-12 md:col-12 flex p-0"
                    v-if="(v$.type_backup.required.$invalid && submitted) || v$.type_backup.$pending.$response">
					<div class="col-2 text-left"></div>
                    <div class="col-4 pl-0 flex" style="flex-direction: column;">
                        <small v-if="(v$.type_backup.required.$invalid && submitted) || v$.type_backup.$pending.$response" 
                            class="col-12 pl-0 p-error">
                            <span class="col-12 p-0">
                                {{
                                    v$.type_backup.required.$message
                                        .replace("Value", "Loại backup")
                                        .replace("is required", "không được để trống")
                                }}
                            </span>
                        </small>
                    </div>
				</div>
                <div class="field col-12 md:col-12 algn-items-center flex p-0">
                    <div class="col-6 md:col-6 p-0 align-items-center flex">
                        <div class="col-4 text-left p-0" style="align-items:center;">
                            Kiểu backup định kỳ
                        </div>
                        <Dropdown :options="typeBackupDatas" 
                            :filter="true" 
                            v-model="backup_form.type_time_backup" 
                            optionLabel="name"
                            optionValue="code" 
                            placeholder="Chọn kiểu backup" 
                            class="col-8">
                        </Dropdown>
                    </div>
                    <div class="col-6 md:col-6 p-0 pl-3 align-items-center flex" v-if="backup_form.type_time_backup == 1">
                        <div class="col-4 text-left p-0" style="align-items:center;">
                            Ngày backup
                        </div>
                        <Dropdown :options="dateBackupWeek" 
                            :filter="true" 
                            v-model="backup_form.time_backup_week" 
                            optionLabel="name"
                            optionValue="code" 
                            placeholder="Chọn ngày backup" 
                            class="col-8">
                        </Dropdown>
                    </div>
                    <div class="col-6 md:col-6 p-0 pl-3 align-items-center flex" v-if="backup_form.type_time_backup == 2">
                        <div class="col-4 text-left p-0" style="align-items:center;">
                            Ngày backup
                        </div>
                        <Dropdown :options="dateBackupMonth" 
                            :filter="true" 
                            v-model="backup_form.time_backup_month" 
                            optionLabel="name"
                            optionValue="code" 
                            placeholder="Chọn ngày backup" 
                            class="col-8">
                        </Dropdown>
                    </div>
                </div>
                <div class="field col-12 md:col-12 algn-items-center flex p-0">
                    <div class="col-6 md:col-6 p-0 align-items-center flex">
                        <div class="col-4 text-left p-0" style="align-items:center;">
                            Nơi lưu dữ liệu <span class="redsao pl-1"> (*)</span>
                        </div>
                        <Dropdown :options="typeSaveDatas" 
                            :filter="true" 
                            v-model="backup_form.type_path_backup" 
                            optionLabel="name"
                            optionValue="code" 
                            placeholder="Chọn nơi lưu dữ liệu" 
                            @change="changePlaceBackup()"
                            class="col-8"
                            :class="{ 'p-invalid': v$.type_path_backup.$invalid && submitted, }">
                        </Dropdown>
                    </div>
                    <div class="col-6 md:col-6 p-0 pl-3 align-items-center flex" 
                        v-if="backup_form.type_path_backup == 1">
                        <div class="col-4 text-left p-0" style="align-items:center;">
                            Vị trí backup
                        </div>
                        <Dropdown :options="typeSaveDatasOther" 
                            :filter="true" 
                            v-model="backup_form.type_path_backup_other" 
                            optionLabel="name"
                            optionValue="code" 
                            placeholder="Chọn vị trí" 
                            class="col-8">
                        </Dropdown>
                    </div>
				</div>
                <div class="field col-12 md:col-12 flex p-0"
                    v-if="(v$.type_path_backup.required.$invalid && submitted) || v$.type_path_backup.$pending.$response">
					<div class="col-2 text-left"></div>
                    <div class="col-4 pl-0 flex" style="flex-direction: column;">
                        <small v-if="(v$.type_path_backup.required.$invalid && submitted) || v$.type_path_backup.$pending.$response" 
                            class="col-12 pl-0 p-error">
                            <span class="col-12 p-0">
                                {{
                                    v$.type_path_backup.required.$message
                                        .replace("Value", "Nơi lưu dữ liệu")
                                        .replace("is required", "không được để trống")
                                }}
                            </span>
                        </small>
                    </div>
				</div>
				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					<div class="col-2 text-left flex p-0" style="align-items:center;">
						Folder backup <span class="redsao pl-1" v-if="backup_form.type_path_backup == 1"> (*)</span>
					</div>
                    <Textarea v-model="backup_form.folder_backup_path" 
                        spellcheck="false" 
                        class="col-10 p-2" 
                        autoResize
						rows="2" 
                        placeholder="C://Program Files/FolderBackup"
                        :class="{ 'p-invalid': v$.folder_backup_path.$invalid && submitted, }" />
				</div>
                <div class="field col-12 md:col-12 flex p-0"
                    v-if="backup_form.type_path_backup == 1 && (v$.folder_backup_path.required.$invalid && submitted) || v$.folder_backup_path.$pending.$response">
					<div class="col-2 text-left"></div>
                    <div class="col-4 pl-0 flex" style="flex-direction: column;">
                        <small v-if="(v$.folder_backup_path.required.$invalid && submitted) || v$.folder_backup_path.$pending.$response" 
                            class="col-12 pl-0 p-error">
                            <span class="col-12 p-0">
                                {{
                                    v$.folder_backup_path.required.$message
                                        .replace("Value", "Folder backup")
                                        .replace("is required", "không được để trống")
                                }}
                            </span>
                        </small>
                    </div>
				</div>
				
				<div class="col-12 md:col-12 flex p-0">
					<div class="col-6 md:col-6 p-0 align-items-center flex">
						<div class="col-4 text-left p-0" style="align-items:center;">
							Trạng thái
						</div>
						<InputSwitch v-model="backup_form.is_active" class="col-8 ip36 p-0" />
					</div>
				</div>

			</div>
		</form>
		<template #footer>
			<Button class="p-button-secondary" label="Hủy" icon="pi pi-times" @click="props.closeDialog" />
			<Button @click="callBackup()">
                <i class="pi pi-spin pi-spinner" style="font-size:1rem" v-if="isRunningBackup"></i>
                <i class="pi pi-sync" style="font-size:1rem" v-else></i>
                <span class="pl-2">Run backup</span>
            </Button>
			<Button label="Lưu" icon="pi pi-check" @click="saveData(!v$.$invalid)" :disabled="isRunningBackup" />
		</template>
	</Dialog>
</template>