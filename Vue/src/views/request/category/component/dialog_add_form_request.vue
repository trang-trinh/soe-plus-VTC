<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr, checkURL } from "../../../../util/function.js";
//import moment from "moment";
import treeuser from "../../../../components/user/treeuser.vue";
//const cryoptojs = inject("cryptojs");
const toast = useToast();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
//const emitter = inject("emitter");
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const config = {
	headers: { Authorization: `Bearer ${store.getters.token}` },
};
const bgColor = ref([
	"#F8E69A", "#AFDFCF", "#F4B2A3", "#9A97EC", "#CAE2B0", "#8BCFFB", "#CCADD7"
]);
const props = defineProps({
	key: Number,
	headerDialog: String,
	displayDialog: Boolean,
	dataForm: Object,
	reloadData: Function,
	closeDialog: Function,
});
const listDropdownTypeProcess = ref([
	{ value: 0, text: "Một trong nhiều" },
	{ value: 1, text: "Duyệt lần lượt" },
	{ value: 2, text: "Duyệt ngẫu nhiên" },
])
const expandedRows = ref([]);
const selectedDatas = ref();
const countTeamUse = ref(0);
const listGroupTeams = ref([]);
const listGroupTeamUsers = ref([]);
const rules = {
	request_form_name: {
		required,
		maxLength: maxLength(500),
		$errors: [
			{
				$property: "request_form_name",
				$validator: "required",
				$message: "Tên loại đề xuất không được để trống!",
			},
		],
	},
	request_form_code: {
		required,
		maxLength: maxLength(50),
		$errors: [
			{
				$property: "request_form_code",
				$validator: "required",
				$message: "Mã loại đề xuất không được để trống!",
			},
		],
	},
};
const request_form = ref(props.dataForm);
const v$ = useVuelidate(rules, request_form);
const listTypeApproved = ref([
	{ type: "Duyệt 1 trong nhiều", value: 0 },
	{ type: "Duyệt lần lượt", value: 1 },
	{ type: "Duyệt ngẫu nhiên", value: 2 },
]);
const addGroupTeam = () => {
	let groupteam = { request_form_sign_id: null, request_team_id: null, request_form_id: null, group_name: null, type_process: 0, is_order: listGroupTeams.value.length + 1, GroupTeamUsers: [] };
	listGroupTeams.value.push(groupteam);
}
const displayDialogUser = ref(false);
const selectedUser = ref([]);
const headerDialogUser = ref();
const is_one = ref(false);
const is_type = ref();
const indexSelect = ref();
const OpenDialogTreeUser = (one, index) => {
	indexSelect.value = index;
	selectedUser.value = [];
	headerDialogUser.value = "Chọn người duyệt";
	displayDialogUser.value = true;
};
const closeDialog = () => {
	displayDialogUser.value = false;
};
const choiceTreeUser = () => {
	if (selectedUser.value.length > 0) {
		selectedUser.value.forEach(function (u, j) {
			let groupuser = { request_form_sign_user_id: null, request_form_sign_id: null, user_id: u.user_id, full_name: u.full_name, avatar: u.avatar, last_name: u.last_name, created_by: null, created_date: null, created_ip: null, created_token_id: null, STT: null };
			listGroupTeams.value.forEach((d, i) => {
				if (i == indexSelect.value) {
					groupuser.STT = j + 1;
					d.GroupTeamUsers.push(groupuser);
				}
			})
		})
	}
	displayDialogUser.value = false;
};
const Remove_FormSign = (arr, rowto, idx) => {
	arr.splice(idx, 1);
}
onMounted(() => {
	return {};
});
</script>
<template>
	<Dialog :header="props.headerDialog" v-model:visible="props.displayDialog" :style="{ width: '50vw' }" :closable="false"
		:modal="true">
		<form @submit.prevent="">
			<div class="grid formgrid m-0">
				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					<div class="col-3 text-left flex p-0" style="align-items:center;">
						Mã loại đề xuất <span class="redsao pl-1"> (*)</span>
					</div>
					<Textarea v-model="request_form.request_form_code" spellcheck="false" class="col-9 ip36 p-2" autoResize
						autofocus rows="1" :class="{ 'p-invalid': v$.request_form_code.$invalid && submitted, }" />
				</div>
				<div class="field col-12 md:col-12 flex p-0">
					<div class="col-3 text-left"></div>
					<small v-if="
						(v$.request_form_code.$invalid && submitted) ||
						v$.request_form_code.$pending.$response
					" class="col-9 p-error">
						<span class="col-12 p-0">
							{{
								v$.request_form_code.required.$message
									.replace(
										"Value",
										"Mã loại đề xuất"
									)
									.replace(
										"is required",
										"không được để trống"
									)
							}}
						</span>
					</small>
					<small class="col-12 p-error mt-2" v-if="
						(v$.request_form_code.maxLength.$invalid &&
							submitted) ||
						v$.request_form_code.maxLength.$pending.$response
					">
						<div class="col-12 md:col-12 flex">
							<label class="col-2 text-left"></label>
							<span class="col-10 p-0">
								{{
									v$.request_form_code.maxLength.$message.replace(
										"The maximum length allowed is",
										"Mã loại đề xuất không được vượt quá"
									)
								}}
								ký tự
							</span>
						</div>
					</small>
				</div>

				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					<div class="col-3 text-left flex p-0" style="align-items:center;">
						Tên loại đề xuất <span class="redsao pl-1"> (*)</span>
					</div>
					<Textarea v-model="request_form.request_form_name" spellcheck="false" class="col-9 ip36 p-2" autoResize
						autofocus rows="1" :class="{ 'p-invalid': v$.request_form_name.$invalid && submitted, }" />
				</div>
				<div class="field col-12 md:col-12 flex p-0">
					<div class="col-3 text-left"></div>
					<small v-if="
						(v$.request_form_name.$invalid && submitted) ||
						v$.request_form_name.$pending.$response
					" class="col-9 p-error">
						<span class="col-12 p-0">
							{{
								v$.request_form_name.required.$message
									.replace(
										"Value",
										"Tên loại đề xuất"
									)
									.replace(
										"is required",
										"không được để trống"
									)
							}}
						</span>
					</small>
					<small class="col-12 p-error mt-2" v-if="
						(v$.request_form_name.maxLength.$invalid &&
							submitted) ||
						v$.request_form_name.maxLength.$pending.$response
					">
						<div class="col-12 md:col-12 flex">
							<label class="col-2 text-left"></label>
							<span class="col-10 p-0">
								{{
									v$.request_form_name.maxLength.$message.replace(
										"The maximum length allowed is",
										"Tên loại đề xuất không được vượt quá"
									)
								}}
								ký tự
							</span>
						</div>
					</small>
				</div>

				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					<div class="col-3 text-left flex p-0" style="align-items:center;">
						Mô tả
					</div>
					<Textarea v-model="request_form.description" spellcheck="false" class="col-9 ip36 p-2" autoResize
						rows="2" />
				</div>
				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					<div class="col-3 text-left flex p-0" style="align-items:center;">
						Nhóm đề xuất
					</div>
					<Dropdown :options="props.groups" :filter="true" :showClear="true" :editable="false"
						v-model="request_form.request_group_id" optionLabel="request_group_name"
						optionValue="request_group_id" placeholder="Chọn nhóm đề xuất" class="col-9 ip36">
					</Dropdown>
				</div>

				<div class="col-12 field md:col-12 flex p-0">
					<div class="field col-6 md:col-6 p-0 align-items-center flex">
						<div class="col-6 text-left p-0" style="align-items:center;">
							Số giờ xử lý tối đa <span class="redsao pl-1"> (*)</span>
						</div>
						<InputNumber v-model="request_form.time_max_process" class="col-6 ip36 p-0" />
					</div>
					<div class="field col-6 md:col-6 p-0 align-items-center flex">
						<div class="col-6 text-center p-0">Thứ tự</div>
						<InputNumber v-model="request_form.is_order" :disabled="true" class="col-6 ip36 p-0" />
					</div>
				</div>

				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					<div class="col-3 text-left flex p-0" style="align-items:center;">
						Kiểu duyệt
					</div>
					<Dropdown :options="listTypeApproved" :filter="true" :showClear="true" :editable="false"
						v-model="request_form.is_process" optionLabel="type" optionValue="value"
						placeholder="Chọn kiểu duyệt" class="col-9 ip36">
					</Dropdown>
				</div>
				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					<div class="col-12 text-left flex p-0" style="align-items:center;color: #0d89ec;">
						<span class="circle-count">{{ listGroupTeams.length }}</span>
						<i class="pi pi-plus-circle"></i>
						<span @click="addGroupTeam()" class="hover-cursor" style="margin-left:5px;">Thêm nhóm duyệt</span>
					</div>
				</div>
				<div class="field col-12 md:col-12 algn-items-center flex p-0" v-if="listGroupTeams.length > 0">
					<div class="col-12 p-0" style="align-items:center;">
						<DataTable class="table-ca-request" :value="listGroupTeams" :paginator="false" :scrollable="true"
							@rowExpand="onRowExpand" @rowCollapse="onRowCollapse" scrollHeight="flex" :lazy="true"
							dataKey="request_form_sign_id" :rowHover="true" v-model:selection="selectedDatas"
							responsiveLayout="scroll" rowGroupMode="subheader" groupRowsBy="request_form_sign_id"
							v-model:expandedRows="expandedRows">
							<Column expander style="max-width: 2rem;" />
							<Column field="group_name" header="Tên nhóm duyệt"
								headerStyle="text-align:center;max-width:auto;height:50px;border-left:none;border-right:none;"
								bodyStyle="text-align:center;max-width:auto;border-left:none;border-right:none;"
								class="align-items-center justify-content-center text-center">
								<template #body="data">
									<InputText v-model="data.data.group_name" class="col-12 ip36 px-2" />
								</template>
							</Column>
							<Column field="" header="Chức năng"
								headerStyle="text-align:center;max-width:10rem;height:50px;border-left:none;border-right:none;"
								bodyStyle="text-align:center;max-width:10rem;border-left:none;border-right:none;"
								class="align-items-center justify-content-center text-center">
								<template #body="data">
									<span style="width: 100%;">
										<i style="width:25%;font-size: 17px;" @click="OpenDialogTreeUser(true, data.index)"
											class="pi pi-plus-circle hover-cursor" v-tooltip.top="'Thêm người duyệt'"></i>
										<i style="width:25%;font-size: 17px;" class="pi pi-angle-down hover-cursor"
											v-tooltip.top="'Xuống'" disabled></i>
										<i style="width:25%;font-size: 17px;" class="pi pi-angle-up hover-cursor"
											v-tooltip.top="'Lên'"></i>
										<i style="width:25%;font-size: 17px;" class="pi pi-trash hover-cursor"
											@click="Remove_FormSign(listGroupTeams, data.data, data.index)"
											v-tooltip.top="'Xóa'"></i>
									</span>
								</template>
							</Column>
							<Column field="" header="Loại duyệt"
								headerStyle="text-align:center;max-width:20rem;height:50px;border-left:none;border-right:none;"
								bodyStyle="text-align:center;max-width:20rem;border-left:none;border-right:none;"
								class="align-items-center justify-content-center text-center">
								<template #body="data">
									<Dropdown :filter="true" style="margin-top: 5px" v-model="data.data.type_process"
										:options="listDropdownTypeProcess" optionLabel="text" optionValue="value"
										placeholder="Loại duyệt" spellcheck="false" class="col-9 ip36 p-0">
										<template #option="slotProps">
											<div class="country-item flex">
												<div class="pt-1">{{ slotProps.option.text }}</div>
											</div>
										</template>
									</Dropdown>
								</template>
							</Column>
							<template #expansion="slotProps">
								<div class="w-full" v-if="slotProps.data.GroupTeamUsers.length > 0">
									<DataTable class="tablesub-team" :value="slotProps.data.GroupTeamUsers"
										responsiveLayout="scroll" :scrollable="false">
										<Column field="STT" header="STT"
											headerStyle="text-align:center;max-width:4rem;min-height:3.125rem"
											bodyStyle="text-align:center;max-width:4rem;"
											class="align-items-center justify-content-center text-center">
										</Column>
										<Column field="" header="Thành viên" headerStyle="height:50px;">
											<template #body="value">
												<Avatar v-tooltip.bottom="{
													value:
														value.data.full_name +
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
															background: bgColor[value.index] + '!important',
														}" class="cursor-pointer" size="xlarge" shape="circle" />
												<span style="margin-left: 10px;">{{ value.data.full_name }}</span>
											</template>
										</Column>
										<Column field="" header=""
											headerStyle="text-align:center;max-width:10rem;height:50px;border-left:none;border-right:none;"
											bodyStyle="text-align:center;max-width:10rem;border-left:none;border-right:none;"
											class="align-items-center justify-content-center text-center">
											<template #body="data">
												<span style="width: 100%;">
													<i style="width:25%;font-size: 17px;"
														class="pi pi-angle-down hover-cursor" v-tooltip.top="'Xuống'"
														disabled></i>
													<i style="width:25%;font-size: 17px;"
														class="pi pi-angle-up hover-cursor" v-tooltip.top="'Lên'"></i>
													<i style="width:25%;font-size: 17px;" class="pi pi-trash hover-cursor"
														@click="Remove_FormSign(listGroupTeams, data.data, data.index)"
														v-tooltip.top="'Xóa'"></i>
												</span>
											</template>
										</Column>
										<Column header="Vai trò"
											headerStyle="text-align:center;max-width:20rem;height:50px;border-left:none;border-right:none;"
											bodyStyle="text-align:center;max-width:20rem;border-left:none;border-right:none;"
											class="align-items-center justify-content-center text-center">
											<template #body="data">
									<Dropdown :filter="true" style="margin-top: 5px" v-model="data.data.IsType"
										:options="listDropdownTypeProcess" optionLabel="text" optionValue="value"
										placeholder="Loại duyệt" spellcheck="false" class="col-9 ip36 p-0">
										<template #option="slotProps">
											<div class="country-item flex">
												<div class="pt-1">{{ slotProps.option.text }}</div>
											</div>
										</template>
									</Dropdown>
								</template>
										</Column>
									</DataTable>
								</div>
							</template>
						</DataTable>
					</div>
				</div>
				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					<div class="col-12 text-left flex p-0" style="align-items:center;color: #0d89ec;">
						<span class="circle-count">{{ countTeamUse }}</span>
						<i class="pi pi-users"></i>
						<span @click="addTeamUse()" class="hover-cursor" style="margin-left:5px;">Thiết lập team sử
							dụng</span>
					</div>
				</div>
			</div>
		</form>
		<template #footer>
			<Button label="Hủy" icon="pi pi-times" @click="props.closeDialog" class="p-button-outlined" />

			<Button label="Lưu" icon="pi pi-check" @click="saveData()" autofocus />
		</template>
	</Dialog>
	<treeuser v-if="displayDialogUser === true" :headerDialog="headerDialogUser" :displayDialog="displayDialogUser"
		:one="is_one" :selected="selectedUser" :closeDialog="closeDialog" :choiceUser="choiceTreeUser" />
</template>
<style scoped>
.circle-count {
	width: 25px;
	height: 25px;
	background-color: #6dd230;
	margin-right: 10px;
	border-radius: 50%;
	color: #fff;
	font-weight: bold;
	display: flex;
	justify-content: center;
	align-items: center;
}

.hover-cursor:hover {
	cursor: pointer;
}
</style>