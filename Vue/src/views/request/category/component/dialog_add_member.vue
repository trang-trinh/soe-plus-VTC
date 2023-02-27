<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
//import moment from "moment";
import treeuser from "../../../../components/user/treeuser.vue";
const toast = useToast();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
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
	headerDialogMember: String,
	displayAddMember: Boolean,
	listUserTeamModal: Object,
	teamActive: Object,
	reloadData: Function,
	closeDialogEditUser: Function,
});
const delUser = (dataUser) => {
	let existIdx = listUserTeamModal.value.findIndex(x => x.user_id == dataUser.user_id);
	if (existIdx >= 0) {
		listUserTeamModal.value.splice(existIdx, 1);
	}
};
const listTypeUser = ref([
	{ name: "Bình thường", code: 0 },
	{ name: "Leader", code: 1 },
	{ name: "Cấp cao nhất (Manager)", code: 2 },
]);
const checkBoxUser = (dataUser) => {
	
};
const savingUserTeam = ref(false);
const saveDataUser = () => {
	savingUserTeam.value = true;
	let formData = new FormData();
	formData.append("model", JSON.stringify(props.teamActive));
	formData.append("listUser", JSON.stringify(listUserTeamModal.value));
	swal.fire({
		width: 110,
		didOpen: () => {
			swal.showLoading();
		},
	});
	axios
		.post(
			baseUrlCheck + "/api/request_ca_team/update_request_team_user",
			formData,
			config
		)
		.then((response) => {
			if (response.data.err != "1") {
				swal.close();
				toast.success("Cập nhật thành viên team thành công!");
				//loadData(true, true);
				props.reloadData(true, true);
				props.closeDialogEditUser();
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
			swal.fire({
				text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
				icon: "error",
				confirmButtonText: "OK",
			});
		});
};

// Choose user from treeuser
const selectedUser = ref([]);
const headerDialogUser = ref();
const displayDialogUser = ref(false);
const is_one = ref();
const is_type = ref();
const OpenDialogTreeUser = (one, type) => {
	selectedUser.value = [];
	if (type == 1) {
		listUserTeamModal.value.forEach((t) => {
			let select = { user_id: t.user_id };
			selectedUser.value.push(select);
		});
		headerDialogUser.value = "Chọn thành viên cho team";
	}
	displayDialogUser.value = true;
	is_one.value = one;
	is_type.value = type;
};
const closeDialogUser = () => {
  displayDialogUser.value = false;
};
const choiceTreeUser = () => {
	switch (is_type.value) {
		case 1:
			if (selectedUser.value.length > 0) {
				selectedUser.value.forEach((t) => {
					let existIdx = listUserTeamModal.value.findIndex(x => x.user_id == t.user_id);
					if (existIdx < 0) {
						t.status = true;
						t.is_type = null;
						t.department_name = t.organization_name;
						listUserTeamModal.value.push(t);
					}
				});
			}
			break;
		default:
			break;
	}
	displayDialogUser.value = false;
};

const listUserTeamModal = ref([]);
const setValueComponent = () => {
	listUserTeamModal.value = props.listUserTeamModal;
};
onMounted(() => {
	setValueComponent();
	return {};
});
</script>
<template>
	<Dialog
		:header="props.headerDialogMember"
		v-model:visible="props.displayAddMember"
		:style="{ width: '40vw' }"
		:closable="false"
		:modal="true"
	>
		<div class="grid formgrid m-0">
			<DataTable
				class="tablemodal-ca-request"
				filterDisplay="menu"
				filterMode="lenient"
				:scrollable="true"
				scrollHeight="flex"
				:showGridlines="true"
				columnResizeMode="fit"
				:lazy="true"
				:value="listUserTeamModal"
				:paginator="false"
				dataKey="request_team_id"
				responsiveLayout="scroll"
				:row-hover="true"
				style="width:100%;"
			>
				<template #header>
					<Toolbar class="w-full custoolbar">
						<template #start></template>

						<template #end>
							<Button
								@click="OpenDialogTreeUser(false, 1)"
								label="Chọn nhân sự"
								icon="pi pi-plus"
								class="mr-2"
							/>
						</template>
					</Toolbar>
				</template>
				<Column
					field="user_id"
					header="Nhân sự"
					headerStyle="text-align:left;height:50px"
					bodyStyle="text-align:left"
				>
					<template #body="{ data }">
						<div class="flex">
							<div class="flex w-3rem">
								<img class="ava" width="32" height="32" 
										v-bind:src="data.avatar
													? basedomainURL + data.avatar
													: basedomainURL + '/Portals/Image/noimg.jpg'
												"
										@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
										v-if="data.avatar"
										style="border:1px solid #ccc;"
									/>
									<Avatar v-else
										class="avt-replace"
										size="large"
										shape="circle"
										v-bind:label="(data.last_name ?? '').substring(0, 1)"
										style="cursor: pointer;width:32px; height:32px;border:1px solid #ccc;"
										:style="{ background: bgColor[data.STT % 7] + '!important'}"
									/>
							</div>
							<div class="flex" style="flex-direction:column;">
								<div>{{data.full_name}}</div>
								<div>{{data.position_name}}</div>
							</div>
						</div>
					</template>
				</Column>
				<Column
					field="is_type"
					header="Vai trò"
					class="align-items-center justify-content-center text-center"
					headerStyle="text-align:center;max-width:200px;height:50px"
					bodyStyle="text-align:center;max-width:200px"
				>
					<template #body="{ data }">
						<Dropdown
							class="col-12 p-0 m-0"
							v-model="data.is_type"
							:options="listTypeUser"
							optionLabel="name"
							optionValue="code"
							placeholder="--- Vai trò ---"
						/>
					</template>
				</Column>
				<Column
					field="status"
					header="Trạng thái"
					headerStyle="text-align:center;max-width:100px;height:50px"
					bodyStyle="text-align:center;max-width:100px"
					class="align-items-center justify-content-center text-center"
				>
					<template #body="data">
						<Checkbox
							:disabled="!(store.state.user.is_super == true || store.state.user.user_id == data.data.created_by ||
									(store.state.user.role_id == 'admin' && store.state.user.organization_id == data.data.organization_id))
							"
							:binary="true"
							v-model="data.data.status"
							@click="checkBoxUser(data.data, true)"
						/>
					</template>
				</Column>
				<Column
					header=""
					class="align-items-center justify-content-center text-center"
					headerStyle="text-align:center;max-width:80px;height:50px"
					bodyStyle="text-align:center;max-width:80px"
				>
					<template #body="Tem">
						<div>
							<Button
								class="
									p-button-rounded
									p-button-danger
									p-button-outlined
									mx-1
								"
								type="button"
								icon="pi pi-trash"
								@click="delUser(Tem.data)"
								v-tooltip.top="'Xóa'"
							></Button>
						</div>
					</template>
				</Column>
				<template #empty>
					<div class="
							align-items-center
							justify-content-center
							p-4
							text-center
							m-auto
						"
						v-if="listUserTeamModal.length == 0"
					>
						<img src="../../../../assets/background/nodata.png" height="144" />
						<h3 class="m-1">Không có dữ liệu</h3>
					</div>
				</template>
			</DataTable>
		</div>
		<template #footer>
			<Button
				label="Hủy"
				icon="pi pi-times"
				@click="props.closeDialogEditUser"
				class="p-button-outlined"
			/>

			<Button
				label="Lưu"
				icon="pi pi-check"
				@click="saveDataUser()"
				autofocus
			/>
		</template>
	</Dialog>
	
	<treeuser
		v-if="displayDialogUser == true"
		:headerDialog="headerDialogUser"
		:displayDialog="displayDialogUser"
		:one="is_one"
		:selected="selectedUser"
		:closeDialog="closeDialogUser"
		:choiceUser="choiceTreeUser"
	/>
</template>
<style scoped>
	img.ava {
		border-radius: 50%;
		vertical-align: middle;
		object-fit: cover;
		border: 1px solid #e7e7e7;
	}
</style>
<style lang="scss" scoped>
	::v-deep(.tablemodal-ca-request.p-datatable-scrollable) {
		.p-datatable-header {
			padding: 0.5rem;
		}
		.p-datatable-tbody {
			height: calc(100vh - 360px);
			background-color: #fff;
		}
		.p-datatable-emptymessage {
			height: 100%;
		}
		tr.p-datatable-emptymessage:not(.p-highlight):hover {
			background-color: #fff !important;
		}
		.p-paginator-bottom {
			position: absolute;
			width: 100%;
			bottom: 100px;
		}
	}
</style>