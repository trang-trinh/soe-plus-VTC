<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
//import moment from "moment";
import dialogAddFormRequest from "../category/component/dialog_add_form_request.vue";
import dialogShowFormTeam from "../category/component/dialog_show_form_team.vue";
//Khai báo

const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const toast = useToast();
const baseUrlCheck = baseURL;
const config = {
	headers: { Authorization: `Bearer ${store.getters.token}` },
};
const filters = ref({
	global: { value: null, matchMode: FilterMatchMode.CONTAINS },
	request_form_name: {
		operator: FilterOperator.AND,
		constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
	},
});
const selectedDatas = ref();
const options = ref({
	loading: true,
	SearchText: ''
});
const request_form = ref({
	request_form_name: "",
	request_form_code: "",
	request_group_id: null,
	description: "",
	is_type_team: null,
	status: true,
	is_order: 1,
});

const datalists = ref([]);
const loadData = (rf) => {
	options.value.loading = true;
	axios
		.post(
			baseUrlCheck + "/api/request/getData",
			{
				str: encr(
				JSON.stringify({
					proc: "request_ca_form_list",
					par: [
						{ par: "user_id", va: store.state.user.user_id },
						{ par: "search", va: options.value.SearchText },
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
			data.forEach((el, i) => {
				el.group_request = JSON.parse(el.group_request);
				if (el.group_request.length > 0){
					el.group_request.forEach((gr) => {
						gr.status = true;
					});
				}
			});

			datalists.value = data;
			options.value.loading = false;
		})
		.catch((error) => {
			toast.error("Tải dữ liệu không thành công!");
			options.value.loading = false;
			if (error && error.status === 401) {
				swal.fire({
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};

const displayBasic = ref(false);
const headerDialogForm = ref();
const openBasic = (headerName) => {
	displayBasic.value = true;
	headerDialogForm.value = headerName;
	forceRerenderForm();
};
const cpnAddFormRequest = ref(0);
const forceRerenderForm = () => {
	cpnAddFormRequest.value += 1;
};
const editForm = (data) => {
	displayBasic.value = true;
	headerDialogForm.value = "Cập nhật loại đề xuất";
	
}
//Checkbox
const onCheckBox = (value, check) => {
	if (check) {
		let data = {
			IntID: value.request_form_id,
			TextID: value.request_form_id + "",
			IntTrangthai: 1,
			BitTrangthai: value.status,
		};
		axios
			.put(
				baseUrlCheck +
					"/api/request_ca_form/update_status_request_ca_form",
				data,
				config
			)
			.then((response) => {
				if (response.data.err != "1") {
					swal.close();
					toast.success("Cập nhật trạng thái form thành công!");
					loadData(true);
					closeDialog();
				} else {
					swal.fire({
						title: "Error!",
						text: response.data.ms,
						icon: "error",
						confirmButtonText: "OK",
					});
				}
			})
			.catch((error) => {
				swal.close();
				swal.fire({
					title: "Error!",
					text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
					icon: "error",
					confirmButtonText: "OK",
				});
			});
	}
};
const closeDialog = () => {
	request_form.value = {
		request_form_name: "",
		request_form_code: "",
		request_group_id: null,
		description: "",
		is_type_team: null,
		status: true,
		is_order: 1,
	};

	displayBasic.value = false;
};

const refreshData = () => {
	options.value.SearchText = '';
	options.value.loading = true;
	selectedDatas.value = [];
	//isDynamicSQL.value = false;
	//filterSQL.value = [];
	loadData(true);
};
const expandedRows = ref([]);

//modal select team
const showSelectTeam = ref(false);
const selectedFormID = ref();
const headerDialogFormShowTeam = ref();
const openModalSelectTeam = (data) => {
	debugger
	selectedFormID.value = data.request_form_id;
	showSelectTeam.value = true;
	headerDialogFormShowTeam.value = 'Team sử dụng cho đề xuất'
}
const closeDialogFormShowTeam = () => {
	showSelectTeam.value = false;
}
//end

onMounted(() => {
	if (!checkURL(window.location.pathname, store.getters.listModule)) {
		//router.back();
	}
	loadData(true);
	return {
		datalists,
		options,
		selectedDatas,
		loadData,
	};
});
</script>
<template>	
	<div class="main-layout flex-grow-1 p-2 pb-0 pr-0" v-if="store.getters.islogin">
		<DataTable
			class="table-ca-request"
			:value="datalists"
			:paginator="false"
			:scrollable="true"
			scrollHeight="flex"
			:loading="options.loading"
			v-model:selection="selectedDatas"
			:lazy="true"
			dataKey="request_form_id"
			:rowHover="true"
			:showGridlines="true"
			responsiveLayout="scroll"
			rowGroupMode="subheader"
			groupRowsBy="request_group_id"
			v-model:expandedRows="expandedRows"
		>
			<template #header>
				<h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
				<i class="pi pi-book"></i> Danh sách loại đề xuất <span class="pl-1" v-if="datalists.length > 0">{{ '(' + datalists.length + ')' }}</span>
				</h3>
				<Toolbar class="w-full custoolbar">
					<template #start>
						<span class="p-input-icon-left">
						<i class="pi pi-search" />
						<InputText
							type="text"
							spellcheck="false"
							v-model="filters['global'].value"
							placeholder="Tìm kiếm"
							style="min-width:30rem;"
						/>
						</span>
					</template>
					<template #end>
						<Button
							@click="openBasic('Thêm loại đề xuất')"
							label="Thêm mới"
							icon="pi pi-plus"
							class="mr-2"
						/>
						<Button
							@click="refreshData"
							class="mr-2 p-button-outlined p-button-secondary"
							icon="pi pi-refresh"
							v-tooltip="'Tải lại'"
						/>
					</template>
				</Toolbar>
			</template>
			<template #groupheader="slotProps">
				<span class="ca-text pl-4">
					{{ slotProps.data.request_group_name }}
				</span>
				<span class="ca-text pl-1">{{ '(' + (slotProps.data.count_group || 0) + ')' }}</span>
			</template>
			<Column
				:expander="true"
				class="max-w-2rem"
				headerStyle="border-left:none;border-right:none;"
				bodyStyle="border-left:none;border-right:none;"
			/>
			<Column
				field="request_form_name"
				header="Tên loại đề xuất"
				headerStyle="height:50px;border-left:none;border-right:none;padding-left:0.5rem;padding-right:1.5rem;"
				bodyStyle="border-left:none;border-right:none;"
				class="align-items-center"
			>
				<template #body="data">
					<div class="name-hover"
						@click="showInfo(data.data)"
					>
						<span class="font-bold">{{ data.data.request_form_name }}</span>
					</div>
				</template>
			</Column>
			<Column
				field="report_name"
				header="Team sử dụng"
				headerStyle="text-align:center;max-width:10rem;height:50px;border-left:none;border-right:none;"
				bodyStyle="justify-content:center;max-width:10rem;border-left:none;border-right:none;"
				headerClass="align-items-center justify-content-center"
			>
				<template #body="data">
					<div class="cell-badge">
						<Badge :value="0 + '/' + 8" size="xlarge" severity="success"
							@click="openModalSelectTeam(data.data)"
							v-tooltip.top="'Chọn team đề xuất'"
							style="cursor:pointer;"
						></Badge>
					</div>
				</template>
			</Column>
			<Column
				field="self_point"
				header="Quy trình duyệt"
				headerStyle="max-width:40rem;height:50px;border-left:none;border-right:none;"
				bodyStyle="max-width:40rem;border-left:none;border-right:none;"
				class="align-items-center"
			>
				<template #body="data">
					<div class="name-hover">
						<!-- Avatar -->
					</div>
				</template>
			</Column>
			<Column
				field="reviewed_point"
				header="Form"
				headerStyle="text-align:center;max-width:8rem;height:50px;border-left:none;border-right:none;"
				bodyStyle="text-align:center;max-width:8rem;border-left:none;border-right:none;"
				class="align-items-center justify-content-center text-center"
			>
				<template #body="data">
					<i class="pi pi-cog" style="font-size:1.5rem;cursor:pointer;"
						@click="openModalSetting(data.data)"
						v-tooltip.top="'Thiết lập form'"
					></i>
				</template>
			</Column>
			<Column
				field="status"
				header="Trạng thái"
				headerStyle="text-align:center;max-width:8rem;height:50px;border-left:none;border-right:none;"
				bodyStyle="text-align:center;max-width:8rem;border-left:none;border-right:none;"
				class="align-items-center justify-content-center text-center"
			>
				<template #body="data">
					<Checkbox
						:disabled="
							!(store.state.user.is_super == true || store.state.user.user_id == data.data.created_by ||
								(store.state.user.role_id == 'admin' && store.state.user.organization_id == data.data.organization_id)
							)
						"
						:binary="true"
						v-model="data.data.status"
						@click="onCheckBox(data.data, true, true)"
					/>
				</template>
			</Column>
			<Column
				field="self_point"
				header="Quy trình"
				headerStyle="text-align:center;max-width:8rem;height:50px;border-left:none;border-right:none;"
				bodyStyle="text-align:center;max-width:8rem;border-left:none;border-right:none;"
				class="align-items-center justify-content-center text-center"
			>
				<template #body="data">
					<Button 
						icon="pi pi-users" 
						class="p-button-danger"
						@click="openAddGroupApproved(data.data)"
						v-tooltip.top="'Chọn nhóm duyệt'"
						style="width:2rem;height:2rem;"
					/>
				</template>
			</Column>
			<Column
				field=""
				header="Chức năng"
				headerStyle="text-align:center;height:50px;max-width:10rem;border-left:none;border-right:none;"
				bodyStyle="max-width:10rem;border-left:none;border-right:none;"
				class="align-items-center justify-content-center text-center"
			>
				<template #body="Tem">
					<div v-if="
						store.state.user.is_super == true || store.state.user.user_id == Tem.data.created_by ||
						(store.state.user.role_id == 'admin' && store.state.user.organization_id == Tem.data.organization_id)
					"
					>
						<Button
							@click="editForm(Tem.data)"
							class="
								p-button-rounded
								p-button-secondary
								p-button-outlined
								mx-1
							"
							type="button"
							icon="pi pi-pencil"
							v-tooltip.top="'Sửa'"
						></Button>
						<Button
							class="
								p-button-rounded
								p-button-danger
								p-button-outlined
								mx-1
							"
							type="button"
							icon="pi pi-trash"
							@click="delTem(Tem.data)"
							v-tooltip.top="'Xóa'"
						></Button>
					</div>
				</template>
			</Column>
			<template #expansion="slotProps">
				<div class="w-full">
					<DataTable
						class="tablesub-team"
						:value="slotProps.data.group_request"
						responsiveLayout="scroll"
						:scrollable="false"
					>
						<Column
							headerStyle="display:none;"
							bodyStyle="max-width:2rem;border-left:none;border-right:none;"
						>
						</Column>
						<Column
							headerStyle="display:none;"
							bodyStyle="align-items:center;border-left:none;border-right:none;"
						>
							<template #body="data">
								<div>{{ "--- " + (data.data.request_team_name || '') }}</div>
							</template>
						</Column>
						<Column
							headerStyle="display:none;"
							bodyStyle="justify-content:center;max-width:8rem;border-left:none;border-right:none;"
						>
							<template #body="data">
								<Checkbox
									:disabled="true"
									:binary="true"
									v-model="data.data.status"
								/>
							</template>
						</Column>
						<Column
							headerStyle="display:none;"
							bodyStyle="justify-content:center;max-width:8rem;border-left:none;border-right:none;"
						>
							<template #body="data">
								<Button 
									icon="pi pi-users" 
									class="p-button-danger"
									@click="openAddGroupApproved(data.data)"
									v-tooltip.top="'Chọn nhóm duyệt'"
									style="width:2rem;height:2rem;"
								/>
							</template>
						</Column>
						<Column 
							headerStyle="display:none;"
							bodyStyle="max-width:10rem;border-left:none;border-right:none;"
						>							
						</Column>
					</DataTable>
				</div>				
			</template>
			<template #empty>
				<div
				class="block w-full h-full format-center"
				v-if="datalists.length == 0"
				>
				<img
					src="../../../assets/background/nodata.png"
					height="144"
				/>
				<h3 class="m-1">Không có dữ liệu</h3>
				</div>
			</template>
		</DataTable>
	</div>

	<dialogAddFormRequest
		:key="cpnAddFormRequest"
		:headerDialog="headerDialogForm"
		:displayDialog="displayBasic"
		:dataForm="request_form"
		:reloadData="loadData"
		:closeDialog="closeDialog"
	></dialogAddFormRequest>
	<dialogShowFormTeam
		:id="selectedFormID"
		:headerDialog="headerDialogFormShowTeam"
		:displayDialog="showSelectTeam"
		:closeDialog="closeDialogFormShowTeam"
	></dialogShowFormTeam>
</template>
    
<style scoped>
	.ca-text {
		color: #0078d4;
	}
	
</style>
<style lang="scss" scoped>
	::v-deep(.table-ca-request.p-datatable-scrollable) {
		.p-datatable-tbody {
			height: calc(100vh - 210px);
			background-color: #fff;
		}
		.p-datatable-tbody .p-rowgroup-header td {
			padding: 1rem !important;
		}
		.p-datatable-tbody .p-datatable-row-expansion > td {
			padding: 0 !important;
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
	::v-deep(.cell-badge) {
		.p-badge-xl {
			border-radius: 50%;
			font-size: 0.9rem;
			min-width: 2.5rem;
			height: 2.5rem;
			line-height: 2.5rem;
		}
	}
	::v-deep(.tablesub-team.p-datatable-responsive-scroll) {
		.p-datatable-tbody {
			height: auto;
			background-color: #fff;
		}
	}
</style>
<style>
.p-dialog-mask.p-component-overlay{
		z-index: 10;
	}
</style>