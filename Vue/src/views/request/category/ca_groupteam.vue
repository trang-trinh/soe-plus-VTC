<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
import moment from "moment";
//Khai báo

const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const config = {
	headers: { Authorization: `Bearer ${store.getters.token}` },
};
const filters = ref({
	global: { value: null, matchMode: FilterMatchMode.CONTAINS },
	request_group_team_name: {
		operator: FilterOperator.AND,
		constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
	},
});
const rules = {
	request_group_team_name: {
		required,
		maxLength: maxLength(500),
		$errors: [
			{
				$property: "request_group_team_name",
				$validator: "required",
				$message: "Tên nhóm team không được để trống!",
			},
		],
	},
	description: {
		maxLength: maxLength(500),
	},
};

const renderTree = (data, id, name, title) => {
	let arrChils = [];
	let arrtreeChils = [];
	data
		.filter((x) => x.parent_group_team_id == null)
		.forEach((m, i) => {
		m.IsOrder = i + 1;
		let om = { key: m[id], data: m };
		const rechildren = (mm, pid) => {
			let dts = data.filter((x) => x.parent_group_team_id == pid);
			if (dts.length > 0) {
			if (!mm.children) mm.children = [];
			dts.forEach((em) => {
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
			let dts = data.filter((x) => x.parent_group_team_id == pid);
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
		label: "----- Chọn " + title + " ----",
	});
	return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
//Lấy dữ liệu request_groupteam
const loadData = (rf) => {
	if (rf) {
		if (isDynamicSQL.value) {
			loadDataSQL();
			return false;
		}
		// if (rf) {
		// 	if (options.value.PageNo == 0) {
		// 		loadCount();
		// 	}
		// }
		axios
			.post(
				baseUrlCheck + "/api/request/getData",
				{
					str: encr(
						JSON.stringify({
							proc: "request_ca_groupteam_list",
							par: [
								{ par: "user_id", va: store.getters.user.user_id, },
								{ par: "status", va: null },
							],
						}),
						SecretKey,
						cryoptojs
					).toString(),
				},
				config
			)
			.then((response) => {
				let data = JSON.parse(response.data.data)[0];
				if (data.length > 0) {
					let obj = renderTree(data, "request_group_team_id", "request_group_team_name", "nhóm team");
					treedatalists.value = obj.arrtreeChils;
					datalists.value = obj.arrChils;
				} else {
					datalists.value = [];
				}
				if (isFirst.value) isFirst.value = false;
				options.value.loading = false;
			})
			.catch((error) => {
				toast.error("Tải dữ liệu không thành công!");
				options.value.loading = false;

				if (error && error.status === 401) {
					swal.fire({
						text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
						confirmButtonText: "OK",
					});
					store.commit("gologout");
				}
			});
	}
};

const request_groupteam = ref({
	request_group_team_name: "",
	parent_group_team_id: null,
	description: "",
	status: true,
	is_order: 1,
});

const submitted = ref(false);
const v$ = useVuelidate(rules, request_groupteam);
const isSaveTem = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const checkDelList = ref(false);

const selectCapcha = ref();
const selectedKey = ref();
const selectedNodes = ref([]);
const treedatalists = ref();

const options = ref({
	IsNext: true,
	sort: "created_date",
	SearchText: "",
	PageNo: 0,
	PageSize: 20,
	loading: true,
	totalRecords: 0,
});

//Hiển thị dialog
const headerDialog = ref();
const displayBasic = ref(false);
const openBasic = (str) => {
	submitted.value = false;	
  	selectCapcha.value = {};
	request_groupteam.value = {
		request_group_team_name: "",
		parent_group_team_id: null,
		description: "",
		status: true,
		is_order: datalists.value.length + 1,
		organization_id: store.getters.user.organization_id,
	};

	isSaveTem.value = false;
	headerDialog.value = str;
	displayBasic.value = true;
};

const closeDialog = () => {
	request_groupteam.value = {
		request_group_team_name: "",
		parent_group_team_id: null,
		description: "",
		status: true,
		is_order: 1,
	};

	displayBasic.value = false;
	loadData(true);
};

//Thêm bản ghi
const saveData = (isFormValid) => {
	submitted.value = true;
	if (!isFormValid) {
		return;
	}
	if (request_groupteam.value.request_group_team_name.length > 250) {
		swal.fire({
			title: "Cảnh báo",
			text: "Tên nhóm team không được vượt quá 500 ký tự!",
			icon: "error",
			confirmButtonText: "OK",
		});
		return;
	}
	let keys = Object.keys(selectCapcha.value);
	request_groupteam.value.parent_group_team_id = keys[0];
	if (request_groupteam.value.parent_group_team_id == -1) {
		request_groupteam.value.parent_group_team_id = null;
	}
	request_groupteam.value.request_group_team_name = request_groupteam.value.request_group_team_name.trim();
	request_groupteam.value.description = request_groupteam.value.description.trim();
	let formData = new FormData();
	formData.append("model", JSON.stringify(request_groupteam.value));
	swal.fire({
		width: 110,
		didOpen: () => {
			swal.showLoading();
		},
	});
	if (!isSaveTem.value) {
		axios
			.post(
				baseUrlCheck + "/api/request_ca_groupteam/add_request_ca_groupteam",
				formData,
				config
			)
			.then((response) => {
				if (response.data.err != "1") {
					swal.close();
					toast.success("Thêm nhóm team thành công!");
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
	} else {
		axios
			.put(
				baseUrlCheck + "/api/request_ca_groupteam/update_request_ca_groupteam",
				formData,
				config
			)
			.then((response) => {
				if (response.data.err != "1") {
					swal.close();
					toast.success("Cập nhật nhóm team thành công!");
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
// Thêm bản ghi con
const addTemChild = (dataParent) => {
	let is_order = datalists.value.length + 1;
	if (dataParent.children) {
		is_order = dataParent.children.length + 1;
	} else {
		is_order = 1;
	}
	selectCapcha.value = {};
	selectCapcha.value[dataParent.data.request_group_team_id] = true;
	request_groupteam.value = {
		request_group_team_name: "",
		parent_group_team_id: dataParent.data.request_group_team_id,
		description: "",
		status: true,
		is_order: is_order,
		organization_id: store.getters.user.organization_id,
	};
	submitted.value = false;
	isSaveTem.value = false;
	headerDialog.value = "Thêm nhóm team";
	displayBasic.value = true;
};

//Sửa bản ghi
const editTem = (dataTem) => {
	submitted.value = false;
	headerDialog.value = "Cập nhật nhóm team";
	displayBasic.value = true;
	isSaveTem.value = true;
	axios
		.post(
		baseUrlCheck + "/api/request/getData",
		{
			str: encr(
				JSON.stringify({
					proc: "request_ca_groupteam_get",
					par: [{ par: "request_group_team_id", va: dataTem.request_group_team_id }],
				}), SecretKey, cryoptojs
			).toString(),
		},
		config
		)
		.then((response) => {
			swal.close();
			let data = JSON.parse(response.data.data);
			if (data.length > 0) {
				request_groupteam.value = data[0][0];
				selectCapcha.value = {};
				selectCapcha.value[request_groupteam.value.parent_group_team_id || "-1"] = true;
			}
		})
		.catch((error) => {
			if (error.status === 401) {
				errorMessage();
			}
		});
};
//Xóa bản ghi
const delData = (Tem) => {
	swal.fire({
		title: "Thông báo",
		text: "Bạn có muốn xoá bản ghi này không!",
		icon: "warning",
		showCancelButton: true,
		confirmButtonColor: "#3085d6",
		cancelButtonColor: "#d33",
		confirmButtonText: "Có",
		cancelButtonText: "Không",
	}).then((result) => {
		if (result.isConfirmed) {
			swal.fire({
				width: 110,
				didOpen: () => {
					swal.showLoading();
				},
			});

			axios
				.delete(
					baseUrlCheck +
						"/api/request_ca_groupteam/delete_request_ca_groupteam",
					{
						headers: {
							Authorization: `Bearer ${store.getters.token}`,
						},
						data: Tem != null ? [Tem.request_group_team_id] : selectedNodes.value,
					}
				)
				.then((response) => {
					swal.close();
					if (response.data.err != "1") {
						swal.close();
						toast.success("Xoá nhóm team thành công!");
						loadData(true);
						if (!Tem) selectedNodes.value = [];
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
					if (error.status === 401) {
						swal.fire({
							text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
							confirmButtonText: "OK",
						});
					}
				});
		}
	});
};

//Sort
const onSort = (event) => {
	options.value.PageNo = 0;

	if (event.sortField == null) {
		isDynamicSQL.value = false;
		loadData(true);
	} else {
		options.value.sort =
			event.sortField + (event.sortOrder == 1 ? " ASC" : " DESC");
		if (event.sortField == "STT") {
			options.value.sort =
				"is_order" + (event.sortOrder == 1 ? " ASC" : " DESC");
		}
		isDynamicSQL.value = true;
		loadDataSQL();
	}
};
const checkFilter = ref(false);
const filterSQL = ref([]);
const isFirst = ref(true);
const loadDataSQL = () => {
	datalists.value = [];

	let data = {
		id: "request_group_team_id",
		sqlS: filterTrangthai.value != null ? filterTrangthai.value : null,
		sqlO: options.value.sort,
		Search: options.value.SearchText,
		PageNo: options.value.PageNo,
		PageSize: options.value.PageSize,
		next: true,
		sqlF: null,
		fieldSQLS: filterSQL.value,
	};
	options.value.loading = true;
	axios
		.post(
			baseUrlCheck + "/api/request_ca_groupteam/Filter_request_ca_groupteam",
			data,
			config
		)
		.then((response) => {
			let dt = JSON.parse(response.data.data);
			let data = dt[0];
			if (data.length > 0) {
				let obj = renderTree(data, "request_group_team_id", "request_group_team_name", "nhóm team");
				treedatalists.value = obj.arrtreeChils;
				datalists.value = obj.arrChils;
			} else {
				datalists.value = [];
			}
			
			if (isFirst.value) isFirst.value = false;
			options.value.loading = false;
			
		})
		.catch((error) => {
			options.value.loading = false;
			toast.error("Tải dữ liệu không thành công!");

			if (error && error.status === 401) {
				swal.fire({
					text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};
//Tìm kiếm
const searchData = (event) => {
	if (event.code == "Enter") {
		if (options.value.SearchText == "") {
			isDynamicSQL.value = false;
			options.value.loading = true;
			loadData(true);
		} else {
			isDynamicSQL.value = true;
			options.value.loading = true;
			loadData(true);
		}
	}
};
const refreshData = () => {
	options.value.SearchText = null;
	filterTrangthai.value = null;
	options.value.loading = true;
	isDynamicSQL.value = false;
	filterSQL.value = [];	
  	selectedKey.value = {};
  	selectedNodes.value = [];
	loadData(true);
};
const onFilter = (event) => {
	filterSQL.value = [];

	for (const [key, value] of Object.entries(event.filters)) {
		if (key != "global") {
			let obj = {
				key: key,
				filteroperator: value.operator,
				filterconstraints: value.constraints,
			};

			if (value.value && value.value.length > 0) {
				obj.filteroperator = value.matchMode;
				obj.filterconstraints = [];
				value.value.forEach(function (vl) {
					obj.filterconstraints.push({ value: vl[obj.key] });
				});
			} else if (value.matchMode) {
				obj.filteroperator = "and";
				obj.filterconstraints = [value];
			}
			if (
				obj.filterconstraints &&
				obj.filterconstraints.filter((x) => x.value != null).length > 0
			) {
				filterSQL.value.push(obj);
			}
		}
	}
	options.value.PageNo = 0;
	options.value.id = null;
	isDynamicSQL.value = true;
	loadDataSQL();
};
//Checkbox
const onCheckBox = (value, check) => {
	if (check) {
		let data = {
			IntID: value.request_group_team_id,
			TextID: value.request_group_team_id + "",
			IntTrangthai: 1,
			BitTrangthai: value.status,
		};
		axios
			.put(
				baseUrlCheck +
					"/api/request_ca_groupteam/update_status_request_ca_groupteam",
				data,
				config
			)
			.then((response) => {
				if (response.data.err != "1") {
					swal.close();
					toast.success("Cập nhật trạng thái nhóm team thành công!");
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

//Filter
const trangThai = ref([
	{ name: "Hiển thị", code: 1 },
	{ name: "Không hiển thị", code: 0 },
]);

const filterTrangthai = ref();

const reFilterEmail = () => {
	filterTrangthai.value = null;
	isDynamicSQL.value = false;
	checkFilter.value = false;
	filterSQL.value = [];
	options.value.SearchText = null;
	op.value.hide();
	loadData(true);
};
const filterFileds = () => {
	filterSQL.value = [];
	checkFilter.value = true;
	let filterS = {
		filterconstraints: [
			{ value: filterTrangthai.value, matchMode: "equals" },
		],
		filteroperator: "and",
		key: "status",
	};
	filterSQL.value.push(filterS);
	loadDataSQL();
};

const op = ref();
const toggle = (event) => {
	op.value.toggle(event);
};
const onNodeSelect = (node) => {
  	selectedNodes.value.push(node.data.request_group_team_id);
};
const onNodeUnselect = (node) => {
	selectedNodes.value.splice(
		selectedNodes.value.indexOf(node.data.request_group_team_id),
		1
	);
};
const onChangeParent = (item) => {
	let request_group_team_id = parseInt(Object.keys(item)[0]);
	let dataParent = datalists.value.filter(x => x.key == request_group_team_id);
	let is_order = datalists.value.length + 1;
	if (dataParent.length > 0) {
		if (dataParent[0].children) {
			is_order = dataParent[0].children.length + 1;
		} else {
			is_order = 1;
		}
	}
	request_groupteam.value.is_order = is_order;
	// axios
	// 	.post(
	// 		baseURL + "/api/request/getData",
	// 		{
	// 			str: encr(JSON.stringify({}), SecretKey, cryoptojs).toString(),
	// 		},
	// 		config
	// 	)
	// 	.then((response) => {
	// 		let data = JSON.parse(response.data.data);
	// 		if (data.length > 0) {
	// 			request_groupteam.value.is_order = data[0][0].c + 1;
	// 		}
	// 	});
};
onMounted(() => {
	if (!checkURL(window.location.pathname, store.getters.listModule)) {
		//router.back();
	}
	loadData(true);
	return {
		datalists,
		options,
		loadData,
		isFirst,
	};
});
</script>
<template>	
	<div class="main-layout flex-grow-1 p-2 pb-0 pr-0" v-if="store.getters.islogin">
		<TreeTable
			:value="datalists"
			v-model:selectionKeys="selectedKey"
			:loading="options.loading"
			@nodeSelect="onNodeSelect"
			@nodeUnselect="onNodeUnselect"
			:filters="filters"
			:showGridlines="true"
			selectionMode="checkbox"
			filterMode="strict"
			class="p-treetable-sm table-ca-request"
			:paginator="datalists && datalists.length > 20"
			:rows="20"
			:rowHover="true"
			responsiveLayout="scroll"
			:lazy="true"
			:scrollable="true"
			scrollHeight="flex"
			:globalFilterFields="['request_group_team_name']"
			>
			<template #header>
				<h3 class="module-title module-title-hidden mt-0 ml-1 mb-2">
				<i class="pi pi-microsoft"></i> Danh sách nhóm team
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
							@click="openBasic('Thêm nhóm team')"
							label="Thêm mới"
							icon="pi pi-plus"
							class="mr-2"
						/>
						<Button
							v-if="selectedNodes.length > 0"
							@click="delData"
							label="Xóa"
							icon="pi pi-trash"
							class="mr-2 p-button-danger"
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
			<Column
				field="is_order"
				header="STT"
				class="align-items-center justify-content-center text-center font-bold"
				headerStyle="text-align:center;height:50px;max-width:100px"
				bodyStyle="text-align:center;max-width:100px"
			>
				<template #body="md">
					<div v-bind:class="md.node.data.status ? '' : 'text-error'">
						{{ md.node.data.is_order }}
					</div>
				</template>
			</Column>
			<Column
				field="request_group_team_name"
				header="Tên nhóm team"
				:sortable="true"
				:expander="true"
			>
				<template #body="md">
					<div
						v-bind:class="[
							md.node.data.parent_group_team_id ? '' : 'font-bold',
							md.node.data.status ? '' : 'text-error',
						]"
					>
						{{ md.node.data.request_group_team_name }}
					</div>
				</template>
			</Column>
			<Column
				field="status"
				header="Trạng thái"
				headerStyle="text-align:center;max-width:150px;height:50px"
				bodyStyle="text-align:center;max-width:150px"
				class="align-items-center justify-content-center text-center"
			>
				<template #body="data">
					<Checkbox
						:disabled="
							!(store.state.user.is_super == true || store.state.user.user_id == data.node.data.created_by ||
								(store.state.user.role_id == 'admin' && store.state.user.organization_id == data.node.data.organization_id)
							)
						"
						:binary="true"
						v-model="data.node.data.status"
						@click="onCheckBox(data.node.data, true, true)"
					/>
				</template>
			</Column>

			<Column
				header="Chức năng"
				headerClass="text-center"
				class="align-items-center justify-content-center text-center"
				headerStyle="text-align:center;max-width:150px"
				bodyStyle="text-align:center;max-width:150px"
			>
				<template #body="Tem">
					<div v-if="
							store.state.user.is_super == true || store.state.user.user_id == Tem.node.data.created_by ||
							(store.state.user.role_id == 'admin' && store.state.user.organization_id == Tem.node.data.organization_id)
						"
					>
						<Button
							@click="addTemChild(Tem.node)"
							class="
								p-button-rounded
								p-button-secondary
								p-button-outlined
								mx-1
							"
							type="button"
							icon="pi pi-plus-circle"
							v-tooltip.top="'Thêm nhóm trực thuộc'"
						></Button>
						<Button
							@click="editTem(Tem.node.data)"
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
								p-button-secondary
								p-button-outlined
								mx-1
							"
							type="button"
							icon="pi pi-trash"
							@click="delData(Tem.node.data)"
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
					v-if="!isFirst"
				>
					<img src="../../../assets/background/nodata.png" height="144" />
					<h3 class="m-1">Không có dữ liệu</h3>
				</div>
			</template>
		</TreeTable>
	</div>
	<Dialog
		:header="headerDialog"
		v-model:visible="displayBasic"
		:style="{ width: '40vw' }"
		:closable="true"
		:modal="true"
	>
		<form>
			<div class="grid formgrid m-0">
				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					<div class="col-3 text-left flex p-0" style="align-items:center;">
						Tên nhóm team <span class="redsao pl-1"> (*)</span>
					</div>
					<Textarea
						v-model="request_groupteam.request_group_team_name"
						spellcheck="false"
						class="col-9 ip36 p-2"
						autoResize
						autofocus
						rows="1"
						:class="{ 'p-invalid': v$.request_group_team_name.$invalid && submitted, }"
					/>
				</div>
				<div class="field col-12 md:col-12 flex p-0"
					v-if="
						((v$.request_group_team_name.$invalid && submitted) || v$.request_group_team_name.$pending.$response)
						|| ((v$.request_group_team_name.maxLength.$invalid && submitted) || v$.request_group_team_name.maxLength.$pending.$response)
					"
				>
					<div class="col-3 text-left"></div>
					<small
						v-if="
							(v$.request_group_team_name.$invalid && submitted) ||
							v$.request_group_team_name.$pending.$response
						"
						class="col-9 p-error"
					>
						<span class="col-12 p-0">
							{{
								v$.request_group_team_name.required.$message
									.replace(
										"Value",
										"Tên nhóm team"
									)
									.replace(
										"is required",
										"không được để trống"
									)
							}}
						</span>
					</small>

					<small
						class="col-12 p-error mt-2"
						v-if="
							(v$.request_group_team_name.maxLength.$invalid &&
								submitted) ||
							v$.request_group_team_name.maxLength.$pending.$response
						"
					>
						<div class="col-12 md:col-12 flex">
							<label class="col-2 text-left"></label>
							<span class="col-10 p-0">
								{{
									v$.request_group_team_name.maxLength.$message.replace(
										"The maximum length allowed is",
										"Tên nhóm team không được vượt quá"
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
					<Textarea
						v-model="request_groupteam.description"
						spellcheck="false"
						class="col-9 ip36 p-2"
						autoResize
						autofocus
						rows="2"
						:class="{ 'p-invalid': v$.description.maxLength.$invalid && submitted, }"
					/>
				</div>

				<div class="field col-12 md:col-12 flex p-0"
					v-if="
						(v$.description.maxLength.$invalid && submitted) ||
						v$.description.maxLength.$pending.$response
					"
				>
					<div class="col-3 text-left"></div>
					<small class="col-12 p-error mt-2">
						<div class="col-12 md:col-12 flex">
							<label class="col-2 text-left"></label>
							<span class="col-10 p-0">
								{{
									v$.description.maxLength.$message.replace(
										"The maximum length allowed is",
										"Mô tả không được vượt quá"
									)
								}}
								ký tự
							</span>
						</div>
					</small>
				</div>

				<div class="col-12 field md:col-12 flex p-0">
					<label class="col-3 mb-0 text-left flex p-0" style="align-items:center;">Thuộc nhóm</label>
					<TreeSelect
						class="col-9"
						@change="onChangeParent"
						v-model="selectCapcha"
						:options="treedatalists"
						:showClear="true"
						placeholder="--- Chọn nhóm ---"
						optionLabel="data.request_group_team_id"
						optionValue="data.request_group_team_team"
					></TreeSelect>
				</div>

				<div class="col-12 field md:col-12 flex p-0">
					<div class="field col-6 md:col-6 p-0 align-items-center flex">
						<div class="col-6 text-left p-0">STT</div>
						<InputNumber
							v-model="request_groupteam.is_order"
							class="col-6 ip36 p-0"
						/>
					</div>
					<div class="field col-6 md:col-6 p-0 align-items-center flex">
						<div class="col-6 text-center p-0">Trạng thái</div>
						<InputSwitch v-model="request_groupteam.status" />
					</div>
				</div>
			</div>
		</form>
		<template #footer>
			<Button
				label="Hủy"
				icon="pi pi-times"
				@click="closeDialog"
				class="p-button-outlined"
			/>

			<Button
				label="Lưu"
				icon="pi pi-check"
				@click="saveData(!v$.$invalid)"
				autofocus
			/>
		</template>
	</Dialog>
</template>
    
<style scoped>
.inputanh {
	border: 1px solid #ccc;
	width: 8rem;
	height: 8rem;
	cursor: pointer;
	padding: 0.063rem;
	display: block;
	margin-left: auto;
	margin-right: auto;
}
.ipnone {
	display: none;
}
.inputanh img {
	object-fit: cover;
	width: 100%;
	height: 100%;
}
img.ava {
	border-radius: 50%;
	vertical-align: middle;
	object-fit: cover;
	border: 1px solid #e7e7e7;
}
</style>
<style lang="scss" scoped>
	::v-deep(.table-ca-request.p-treetable-scrollable) {
		.p-treetable-tbody {
			height: calc(100vh - 205px);
			background-color: #fff;
		}
		.p-treetable-emptymessage {
			height: 100%;
		}
		tr.p-treetable-emptymessage:not(.p-highlight):hover {
			background-color: #fff !important;
		}
	}
	::v-deep(.avt-replace) {
		.p-avatar-text {
			display: flex;
			width: inherit;
			align-items: center;
			justify-content: center;
			font-size: initial;
			text-transform: uppercase;
			color: #000;
		}
	}
</style>
    