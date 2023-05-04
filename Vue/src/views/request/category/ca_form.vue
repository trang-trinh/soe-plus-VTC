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
import dialogSettingForm from "../category/component/dialog_setting_form.vue";
import procedureDetail from "../category/component/procedure_detail.vue";
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
const listDropdownTypeProcess = ref([
	{ value: 0, text: "Một trong nhiều" },
	{ value: 1, text: "Duyệt lần lượt" },
	{ value: 2, text: "Duyệt ngẫu nhiên" },
])
const bgColor = ref([
	"#F8E69A",
	"#AFDFCF",
	"#F4B2A3",
	"#9A97EC",
	"#CAE2B0",
	"#8BCFFB",
	"#CCADD7",
]);
const datalists = ref([]);
const listRequestCaGroup = ref();
const request_form = ref({
	request_form_name: "",
	request_form_code: "",
	request_group_id: null,
	description: "",
	is_type_team: null,
	status: true,
	is_order: null,
	time_max_process: 1,
	is_process: 0,
	is_review: false,
	is_use_all: true,
});
const listTeamUses = ref([]);
const listGroupTeams = ref();
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
			listRequestCaGroup.value = JSON.parse(response.data.data)[1];
			data.forEach((el, i) => {
				el.request_group_name = el.request_group_name ? el.request_group_name : "Loại khác";
				el.group_request = el.group_request ? JSON.parse(el.group_request) : [];
				el.list_group_users = el.list_group_users ? JSON.parse(el.list_group_users) : [];
				if (el.group_request.length > 0) {
					el.group_request.forEach((gr) => {
						gr.status = true;
					});
				}
				let listgroupby = groupBy(el.list_group_users, "request_form_sign_id");
				el.arrNew = [];
				for (let k in listgroupby) {
					let requestGroup1 = [];
					let requestGroup2 = [];
					listgroupby[k].forEach(function (r, i) {
						if (i <= 2) {
							requestGroup1.push(r);
						} else {
							requestGroup2.push(r);
						}
					});
					el.arrNew.push({
						request_form_sign_id: k,
						requestGroup1: requestGroup1,
						requestGroup2: requestGroup2,
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
const searchData = (event) => {
	if (event.code == "Enter") {
		loadData(true);
	}
};

const displayBasic = ref(false);
const isAdd = ref(false);
const headerDialogForm = ref();
const openBasic = (headerName) => {
	request_form.value = {
		request_form_name: "",
		request_form_code: "",
		request_group_id: null,
		description: "",
		is_type_team: null,
		status: true,
		is_order: datalists.value.length + 1,
		time_max_process: 1,
		is_process: 0,
		is_review: false,
		is_use_all: true,
	};
	listGroupTeams.value = [];
	listTeamUses.value = [];
	displayBasic.value = true;
	isAdd.value = true;
	headerDialogForm.value = headerName;
	forceRerenderForm();
};
const cpnAddFormRequest = ref(0);
const forceRerenderForm = () => {
	cpnAddFormRequest.value += 1;
};
const cpnFormTeam = ref(0);
const forceRerenderFormTeam = () => {
	cpnFormTeam.value += 1;
};
const cpnSettingForm = ref(0);
const forceRerenderFormSetting = () => {
	cpnSettingForm.value += 1;
};
const editForm = (data) => {
	axios
		.post(
			baseUrlCheck + "/api/request/getData",
			{
				str: encr(
					JSON.stringify({
						proc: "request_ca_form_get",
						par: [
							{ par: "request_form_id", va: data.request_form_id },
						],
					}),
					SecretKey,
					cryoptojs
				).toString(),
			},
			config
		)
		.then((response) => {
			isAdd.value = false;
			let data = JSON.parse(response.data.data);
			request_form.value = data[0][0];
			data[1].forEach((e, k) => {
				e.GroupTeamUsers = e.GroupTeamUsers
					? JSON.parse(e.GroupTeamUsers)
					: [];
				e.GroupTeamUsers.forEach((m, i) => {
					if (e.GroupTeamUsers.length == 1) {
						m.isDisableDown = true;
						m.isDisableUp = true;
					} else if (i == 0 && e.GroupTeamUsers.length > 1) {
						m.isDisableDown = false;
						m.isDisableUp = true;
					} else if ((i + 1 == e.GroupTeamUsers.length) && e.GroupTeamUsers.length > 1) {
						m.isDisableDown = true;
						m.isDisableUp = false;
					} else if (i > 0 && i < e.GroupTeamUsers.length - 1) {
						m.isDisableDown = false;
						m.isDisableUp = false;
					}
				})
				if (data[1].length == 1) {
					e.isDisableDown = true;
					e.isDisableUp = true;
				} else if (k == 0 && data[1].length > 1) {
					e.isDisableDown = false;
					e.isDisableUp = true;
				} else if ((k + 1 == data[1].length) && data[1].length > 1) {
					e.isDisableDown = true;
					e.isDisableUp = false;
				} else if (k > 0 && k < data[1].length - 1) {
					e.isDisableDown = false;
					e.isDisableUp = false;
				}
			})
			data[2].forEach((e, i) => {
				e.STT = i + 1;
			})
			listGroupTeams.value = data[1];
			listTeamUses.value = data[2];
			displayBasic.value = true;
			headerDialogForm.value = "Cập nhật loại đề xuất";
			forceRerenderForm();
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
}
const delTem = (model) => {
	swal
		.fire({
			title: "Thông báo",
			text: "Bạn có muốn xoá loại đề xuất này không!",
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
				axios
					.delete(
						baseURL + "/api/request_ca_form/delete_request_ca_form",
						{
							headers: { Authorization: `Bearer ${store.getters.token}` },
							data: [model.request_form_id],
						},
					)
					.then((response) => {
						swal.close();
						if (response.data.err != "1") {
							swal.close();
							toast.success("Xoá loại đề xuất thành công!");
							// checkDelList.value = false;
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
//Checkbox
const onCheckBox = (value, check) => {
	if (check) {
		let formData = new FormData();
		formData.append("request_form", JSON.stringify(value));
		axios
			.post(
				baseUrlCheck +
				"/api/request_ca_form/update_status_request_ca_form",
				formData,
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
		is_order: null,
		time_max_process: 1,
		is_process: 0,
		is_review: false,
		is_use_all: true,
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
const showSelectTeam = ref(false);
const selectedFormID = ref();
const headerDialogFormShowTeam = ref();
const openTeamUse = (model) => {
	axios
		.post(
			baseUrlCheck + "/api/request/getData",
			{
				str: encr(
					JSON.stringify({
						proc: "request_ca_form_get",
						par: [
							{ par: "request_form_id", va: model.request_form_id },
						],
					}),
					SecretKey,
					cryoptojs
				).toString(),
			},
			config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data);
			request_form.value = data[0][0];
			data[2].forEach((e, i) => {
				e.STT = i + 1;
			})
			listTeamUses.value = data[2];
			selectedFormID.value = model.request_form_id;
			showSelectTeam.value = true;
			headerDialogFormShowTeam.value = 'Team sử dụng cho đề xuất';
			forceRerenderFormTeam();
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
}
const closeDialogFormShowTeam = () => {
	showSelectTeam.value = false;
}
// Thiết lập form
const listDataType = ref([
	{
		label: 'Chung',
		code: 0,
		items: [
			{ label: 'Trường bình thường', value: 0 },
			{ label: 'Trường tổng hợp (tính tổng)', value: 1 },
			{ label: 'Trường ẩn tên', value: 2 },
			{ label: 'Table', value: 3 },
			{ label: 'Column', value: 4 },
			{ label: 'Row', value: 5 },
		]
	},
	{
		label: 'HR-Nghỉ phép',
		code: 1,
		items: [
			{ label: 'Ngày', value: 6 },
			{ label: 'Giờ', value: 7 },
			{ label: 'Nghỉ phép?', value: 8 },
		]
	},
])
const headerSettingForm = ref();
const displaySettingForm = ref(false);
const listSettingForms = ref();
const openModalSetting = (model) => {
	axios
		.post(
			baseUrlCheck + "/api/request/getData",
			{
				str: encr(
					JSON.stringify({
						proc: "request_ca_formd_list",
						par: [
							{ par: "request_form_id", va: model.request_form_id },
						],
					}),
					SecretKey,
					cryoptojs
				).toString(),
			},
			config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data);
			selectedFormID.value = model.request_form_id;
			request_form.value = model;
			if (data[0].length > 0) {
				data[0].forEach((e, i) => {
					if (e.is_type >= 0 && e.is_type <= 5) {
						let arr = listDataType.value.filter(x => x.code == 0)[0];
						e.selectedType = { label: arr.items.filter(x => x.value == e.is_type)[0].label, value: arr.items.filter(x => x.value == e.is_type)[0].value };
					} else {
						let arr = listDataType.value.filter(x => x.code == 1)[0];
						e.selectedType = { label: arr.items.filter(x => x.value == e.is_type)[0].label, value: arr.items.filter(x => x.value == e.is_type)[0].value };
					}
					if (data[0].length == 1) {
						e.isDisableDown = true;
						e.isDisableUp = true;
					} else if (i == 0 && data[0].length > 1) {
						e.isDisableDown = false;
						e.isDisableUp = true;
					} else if ((i + 1 == data[0].length) && data[0].length > 1) {
						e.isDisableDown = true;
						e.isDisableUp = false;
					} else if (i > 0 && i < data[0].length - 1) {
						e.isDisableDown = false;
						e.isDisableUp = false;
					}
					e.is_edit = false;
				})
			}
			listSettingForms.value = data[0];
			displaySettingForm.value = true;
			headerSettingForm.value = "Thiết lập " + model.request_form_name;
			forceRerenderFormSetting();
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

}
const closeDialogFormSetting = () => {
	displaySettingForm.value = false;
}
// emitter.emit('listTeamUses', listTeamUses.value);
const showProcedure = ref(false);
const headerShowProcedure = ref();
const PositionSideBar = ref("right");
const listGroups = ref([]);
const listGroupUsers = ref([]);
const componentKey = ref(0);
const forceRerender = () => {
	componentKey.value += 1;
};
const groupBy = (list, props) => {
	return list.reduce((a, b) => {
		(a[b[props]] = a[b[props]] || []).push(b);
		return a;
	}, {});
};
const openSlideBarprocedure = (model) => {
	headerShowProcedure.value = model.request_form_name;
	showProcedure.value = true;
	selectedFormID.value = model.request_form_id;
	forceRerender();
	// axios
	// 	.post(
	// 		baseUrlCheck + "/api/request/getData",
	// 		{
	// 			str: encr(
	// 				JSON.stringify({
	// 					proc: "request_ca_form_sign_get_list",
	// 					par: [
	// 						{ par: "request_form_id", va: model.request_form_id },
	// 						{ par: "search", va: '' },
	// 					],
	// 				}),
	// 				SecretKey,
	// 				cryoptojs
	// 			).toString(),
	// 		},
	// 		config
	// 	)
	// 	.then((response) => {
	// 		let data = JSON.parse(response.data.data);
	// 		data[0].forEach((element) => {
	// 			element.type_process_name = listDropdownTypeProcess.value.filter(
	// 				(x) => x.value == element.type_process,
	// 			)[0].text;
	// 		})
	// 		listGroups.value = data[0];
	// 		var listgroupby = groupBy(data[1], "request_form_sign_id");
	// 		var arrNew = [];
	//         for (let k in listgroupby) {
	//           var requestGroup = [];
	//           listgroupby[k].forEach(function (r) {
	//             requestGroup.push(r);
	//           });
	//           arrNew.push({
	//             request_form_sign_id: k,
	//             group_name: data[1].filter(x=>x.request_form_sign_id == k)[0].group_name,
	//             requestGroup: requestGroup,
	//           });
	//         }
	// 		listGroupUsers.value = arrNew;
	// 		headerShowProcedure.value = model.request_form_name;
	// 		showProcedure.value = true;
	// 		selectedFormID.value = model.request_form_id;
	// 		forceRerender();
	// 	})x
	// 	.catch((error) => {
	// 		toast.error("Tải dữ liệu không thành công!");
	// 		options.value.loading = false;
	// 		if (error && error.status === 401) {
	// 			swal.fire({
	// 				text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
	// 				confirmButtonText: "OK",
	// 			});
	// 			store.commit("gologout");
	// 		}
	// 	});
}
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
		<DataTable class="table-ca-request" :value="datalists" :paginator="false" :scrollable="true" scrollHeight="flex"
			:loading="options.loading" v-model:selection="selectedDatas" :lazy="true" dataKey="request_form_id"
			:rowHover="true" :showGridlines="true" responsiveLayout="scroll" rowGroupMode="subheader"
			groupRowsBy="request_group_id" v-model:expandedRows="expandedRows">
			<template #header>
				<h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
					<i class="pi pi-book"></i> Danh sách loại đề xuất <span class="pl-1" v-if="datalists.length > 0">{{ '('
						+ datalists.length + ')' }}</span>
				</h3>
				<Toolbar class="w-full custoolbar">
					<template #start>
						<span class="p-input-icon-left">
							<i class="pi pi-search" />
							<InputText type="text" spellcheck="false" v-model="options.SearchText" placeholder="Tìm kiếm"
								@keyup="searchData" style="min-width:30rem;" />
						</span>
					</template>
					<template #end>
						<Button @click="openBasic('Thêm loại đề xuất')" label="Thêm mới" icon="pi pi-plus" class="mr-2" />
						<Button @click="refreshData" class="mr-2 p-button-outlined p-button-secondary" icon="pi pi-refresh"
							v-tooltip="'Tải lại'" />
					</template>
				</Toolbar>
			</template>
			<template #groupheader="slotProps">
				<span class="ca-text pl-4">
					{{ slotProps.data.request_group_name }}
				</span>
				<span class="ca-text pl-1">{{ '(' + (slotProps.data.count_group || 0) + ')' }}</span>
			</template>
			<Column :expander="true" class="max-w-2rem" headerStyle="border-left:none;border-right:none;"
				bodyStyle="border-left:none;border-right:none;" />
			<Column field="request_form_name" header="Tên loại đề xuất"
				headerStyle="height:50px;border-left:none;border-right:none;padding-left:0.5rem;padding-right:1.5rem;"
				bodyStyle="border-left:none;border-right:none;" class="align-items-center">
				<template #body="data">
					<div class="name-hover" @click="showInfo(data.data)">
						<span class="font-bold">{{ data.data.request_form_name }}</span>
					</div>
				</template>
			</Column>
			<Column field="report_name" header="Team sử dụng"
				headerStyle="text-align:center;max-width:10rem;height:50px;border-left:none;border-right:none;"
				bodyStyle="justify-content:center;max-width:10rem;border-left:none;border-right:none;"
				headerClass="align-items-center justify-content-center">
				<template #body="data">
					<div class="cell-badge">
						<Badge :value="0 + '/' + 8" size="xlarge" severity="success" @click="openTeamUse(data.data)"
							v-tooltip.top="'Chọn team đề xuất'" style="cursor:pointer;"></Badge>
					</div>
				</template>
			</Column>
			<Column field="self_point" header="Quy trình duyệt"
				headerStyle="max-width:40rem;height:50px;border-left:none;border-right:none;"
				bodyStyle="max-width:40rem;border-left:none;border-right:none;" class="align-items-center">
				<template #body="data">
					<div class="name-hover" style="width: 100%;display: flex;">
						<div class="text-left" v-for="l in data.data.arrNew" style="width: 30%;">
							<AvatarGroup>
								<div v-for="(value, index) in l.requestGroup1" :key="index">
									<div>
										<Avatar v-tooltip.bottom="{
												value:
													value.full_name +
													'<br/>' +
													(value.tenChucVu || '') +
													'<br/>' +
													(value.tenToChuc || ''),
												escape: true,
											}" v-bind:label="value.avatar ? '' : (value.last_name ?? '').substring(0, 1)
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
								<Avatar v-if="l.requestGroup2.length > 0
									" :label="'+' +
		(l.requestGroup2.length) +
		''
		" class="cursor-pointer" shape="circle" style="
								                background-color: #e9e9e9 !important;
								                color: #98a9bc;
								                font-size: 14px !important;
								                width: 32px;
								                margin-left: -10px;
								                height: 32px;
								              " />
							</AvatarGroup>
						</div>
					</div>
				</template>
			</Column>
			<Column field="reviewed_point" header="Form"
				headerStyle="text-align:center;max-width:8rem;height:50px;border-left:none;border-right:none;"
				bodyStyle="text-align:center;max-width:8rem;border-left:none;border-right:none;"
				class="align-items-center justify-content-center text-center">
				<template #body="data">
					<i class="pi pi-cog" style="font-size:1.5rem;cursor:pointer;" @click="openModalSetting(data.data)"
						v-tooltip.top="'Thiết lập form'"></i>
				</template>
			</Column>
			<Column field="status" header="Trạng thái"
				headerStyle="text-align:center;max-width:8rem;height:50px;border-left:none;border-right:none;"
				bodyStyle="text-align:center;max-width:8rem;border-left:none;border-right:none;"
				class="align-items-center justify-content-center text-center">
				<template #body="data">
					<Checkbox :disabled="!(store.state.user.is_super == true || store.state.user.user_id == data.data.created_by ||
							(store.state.user.role_id == 'admin' && store.state.user.organization_id == data.data.organization_id)
						)
						" :binary="true" v-model="data.data.status" @click="onCheckBox(data.data, true, true)" />
				</template>
			</Column>
			<Column field="self_point" header="Quy trình"
				headerStyle="text-align:center;max-width:8rem;height:50px;border-left:none;border-right:none;"
				bodyStyle="text-align:center;max-width:8rem;border-left:none;border-right:none;"
				class="align-items-center justify-content-center text-center">
				<template #body="data">
					<Button icon="pi pi-users" class="p-button-danger" @click="openSlideBarprocedure(data.data)"
						v-tooltip.top="'Chọn nhóm duyệt'" style="width:2rem;height:2rem;" />
				</template>
			</Column>
			<Column field="" header="Chức năng"
				headerStyle="text-align:center;height:50px;max-width:10rem;border-left:none;border-right:none;"
				bodyStyle="max-width:10rem;border-left:none;border-right:none;"
				class="align-items-center justify-content-center text-center">
				<template #body="Tem">
					<div v-if="store.state.user.is_super == true || store.state.user.user_id == Tem.data.created_by ||
						(store.state.user.role_id == 'admin' && store.state.user.organization_id == Tem.data.organization_id)
						">
						<Button @click="editForm(Tem.data)" class="
																									p-button-rounded
																									p-button-secondary
																									p-button-outlined
																									mx-1
																								" type="button" icon="pi pi-pencil" v-tooltip.top="'Sửa'"></Button>
						<Button class="
																									p-button-rounded
																									p-button-danger
																									p-button-outlined
																									mx-1
																								" type="button" icon="pi pi-trash" @click="delTem(Tem.data)" v-tooltip.top="'Xóa'"></Button>
					</div>
				</template>
			</Column>
			<template #expansion="slotProps">
				<div class="w-full">
					<DataTable class="tablesub-team" :value="slotProps.data.group_request" responsiveLayout="scroll"
						:scrollable="false">
						<Column headerStyle="display:none;" bodyStyle="max-width:2rem;border-left:none;border-right:none;">
						</Column>
						<Column headerStyle="display:none;"
							bodyStyle="align-items:center;border-left:none;border-right:none;">
							<template #body="data">
								<div>{{ "--- " + (data.data.request_team_name || '') }}</div>
							</template>
						</Column>
						<Column headerStyle="display:none;"
							bodyStyle="justify-content:center;max-width:8rem;border-left:none;border-right:none;">
							<template #body="data">
								<Checkbox :disabled="true" :binary="true" v-model="data.data.status" />
							</template>
						</Column>
						<Column headerStyle="display:none;"
							bodyStyle="justify-content:center;max-width:8rem;border-left:none;border-right:none;">
							<template #body="data">
								<Button icon="pi pi-users" class="p-button-danger" @click="openAddGroupApproved(data.data)"
									v-tooltip.top="'Chọn nhóm duyệt'" style="width:2rem;height:2rem;" />
							</template>
						</Column>
						<Column headerStyle="display:none;" bodyStyle="max-width:10rem;border-left:none;border-right:none;">
						</Column>
					</DataTable>
				</div>
			</template>
			<template #empty>
				<div class="block w-full h-full format-center" v-if="datalists.length == 0">
					<img src="../../../assets/background/nodata.png" height="144" />
					<h3 class="m-1">Không có dữ liệu</h3>
				</div>
			</template>
		</DataTable>
	</div>

	<dialogAddFormRequest :key="cpnAddFormRequest" :headerDialog="headerDialogForm" :displayDialog="displayBasic"
		:dataForm="request_form" :listGroupTeams="listGroupTeams" :listTeamUses="listTeamUses" :reloadData="loadData"
		:closeDialog="closeDialog" :isAdd="isAdd" :groups="listRequestCaGroup"></dialogAddFormRequest>

	<dialogShowFormTeam :key="cpnFormTeam" :dataForm="request_form" :isSave="true" :id="selectedFormID"
		:headerDialog="headerDialogFormShowTeam" :listTeamUses="listTeamUses" :displayDialog="showSelectTeam"
		:closeDialog1="closeDialogFormShowTeam"></dialogShowFormTeam>
	<dialogSettingForm :key="cpnSettingForm" :listSettingForms="listSettingForms" :dataForm="request_form"
		:id="selectedFormID" :headerDialog="headerSettingForm" :displayDialog="displaySettingForm"
		:closeDialogSetting="closeDialogFormSetting"></dialogSettingForm>

	<Sidebar v-model:visible="showProcedure" :position="PositionSideBar" :style="{
			width:
				PositionSideBar == 'right'
					? width1 > 1800
						? '65vw'
						: '80vw'
					: '100vw',
			'min-height': '100vh !important',
			'z-index': '100',
			'background-color': '#eee',
		}" :showCloseIcon="false">
		<procedureDetail :isShow="showProcedure" :id="selectedFormID" :headerShowProcedure="headerShowProcedure" :turn="0">
		</procedureDetail>
	</Sidebar>
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

	.p-datatable-tbody .p-datatable-row-expansion>td {
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
.p-dialog-mask.p-component-overlay {
	z-index: 10;
}
</style>