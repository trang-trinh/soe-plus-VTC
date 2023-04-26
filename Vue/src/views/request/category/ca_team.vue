<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
import moment from "moment";
import dialogUpdateMember from "../category/component/dialog_add_member.vue";
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
	request_team_name: {
		operator: FilterOperator.AND,
		constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
	},
});
const rules = {
	request_team_name: {
		required,
		maxLength: maxLength(500),
		$errors: [
			{
				$property: "request_team_name",
				$validator: "required",
				$message: "Tên team không được để trống!",
			},
		],
	},
	
	description: {
		maxLength: maxLength(500),
	},
};
const bgColor = ref([
    "#F8E69A", "#AFDFCF", "#F4B2A3", "#9A97EC", "#CAE2B0", "#8BCFFB", "#CCADD7"
]);
//Lấy số bản ghi
const loadCount = () => {
	axios
		.post(
			baseUrlCheck + "/api/request/getData",
			{
				str: encr(
					JSON.stringify({
						proc: "request_ca_team_count",
						par: [
							{ par: "user_id", va: store.getters.user.user_id },
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
				options.value.totalRecords = data[0].totalRecords;
				sttData.value = data[0].totalRecords + 1;
			}
		})
		.catch((error) => {});
};
//Lấy dữ liệu request_team
const loadData = (rf, activeTeam) => {
	if (rf) {
		if (isDynamicSQL.value) {
			loadDataSQL(activeTeam);
			return false;
		}
		if (rf) {
			if (options.value.PageNo == 0) {
				loadCount();
			}
		}
		axios
			.post(
				baseUrlCheck + "/api/request/getData",
				{
					str: encr(
						JSON.stringify({
							proc: "request_ca_team_list",
							par: [
								{ par: "pageno", va: options.value.PageNo },
								{ par: "pagesize", va: options.value.PageSize },
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
				if (isFirst.value) isFirst.value = false;
				data.forEach((element, i) => {
					element.STT = options.value.PageNo * options.value.PageSize + i + 1;
					if (element.members != null){
						element.members = JSON.parse(element.members);
					}
				});
				datalists.value = data;
				if (activeTeam == true) {
					goUserTeam(teamActive.value, true);
				}
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
	}
};
//Phân trang dữ liệu
const onPage = (event) => {
	if (event.rows != options.value.PageSize) {
		options.value.PageSize = event.rows;
	}
	if (event.page == 0) {
		//Trang đầu
		options.value.id = null;
		options.value.IsNext = true;
	} else if (event.page > options.value.PageNo + 1) {
		//Trang cuối
		options.value.id = -1;
		options.value.IsNext = false;
	} else if (event.page > options.value.PageNo) {
		//Trang sau

		options.value.id =
			datalists.value[datalists.value.length - 1].request_team_id;
		options.value.IsNext = true;
	} else if (event.page < options.value.PageNo) {
		//Trang trước
		options.value.id = datalists.value[0].request_team_id;
		options.value.IsNext = false;
	}
	options.value.PageNo = event.page;
	loadData(true);
};

const request_team = ref({
	request_team_name: "",
	status: true,
	is_order: 1,
});

const selectedDatas = ref();
const submitted = ref(false);
const v$ = useVuelidate(rules, request_team);
const isSaveTem = ref(false);
const datalists = ref();
const toast = useToast();
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const checkDelList = ref(false);

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
const selectCapcha = ref();
const openBasic = (str) => {
	submitted.value = false;
	request_team.value = {
		request_team_name: "",
		request_group_team_id: null,
		description: "",
		is_type_team: null,
		status: true,
		is_order: sttData.value,
		organization_id: store.getters.user.organization_id,
	};
	selectCapcha.value = {};
	selectCapcha.value["-1"] = true;
	isSaveTem.value = false;
	headerDialog.value = str;
	displayBasic.value = true;
};

const closeDialog = () => {
	request_team.value = {
		request_team_name: "",
		request_group_team_id: null,
		description: "",
		is_type_team: null,
		status: true,
		is_order: 1,
	};

	displayBasic.value = false;
	loadData(true);
};

//Thêm bản ghi
const sttData = ref(1);
const saveData = (isFormValid) => {
	submitted.value = true;
	if (!isFormValid) {
		return;
	}

	if (request_team.value.request_team_name.length > 250) {
		swal.fire({
			title: "Cảnh báo",
			text: "Tên team không được vượt quá 500 ký tự!",
			icon: "error",
			confirmButtonText: "OK",
		});
		return;
	}
	request_team.value.request_team_name = request_team.value.request_team_name.trim();
	let formData = new FormData();
	formData.append("model", JSON.stringify(request_team.value));
	swal.fire({
		width: 110,
		didOpen: () => {
			swal.showLoading();
		},
	});
	if (!isSaveTem.value) {
		axios
			.post(
				baseUrlCheck + "/api/request_ca_team/add_request_ca_team",
				formData,
				config
			)
			.then((response) => {
				if (response.data.err != "1") {
					swal.close();
					toast.success("Thêm team thành công!");
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
				baseUrlCheck + "/api/request_ca_team/update_request_ca_team",
				formData,
				config
			)
			.then((response) => {
				if (response.data.err != "1") {
					swal.close();
					toast.success("Cập nhật team thành công!");
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

//Sửa bản ghi
const editTem = (dataTem) => {
	submitted.value = false;
	request_team.value = dataTem;
	selectCapcha.value = {};
	selectCapcha.value[request_team.value.request_group_team_id || "-1"] = true;
	headerDialog.value = "Cập nhật thông tin team";
	isSaveTem.value = true;
	displayBasic.value = true;
};
//Xóa bản ghi
const delTem = (Tem) => {
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
						"/api/request_ca_team/delete_request_ca_team",
					{
						headers: {
							Authorization: `Bearer ${store.getters.token}`,
						},
						data: Tem != null ? [Tem.request_team_id] : -1,
					}
				)
				.then((response) => {
					swal.close();
					if (response.data.err != "1") {
						swal.close();
						toast.success("Xoá team thành công!");
						loadData(true);
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
							text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
const loadDataSQL = (activeTeam) => {
	datalists.value = [];

	let data = {
		id: "request_team_id",
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
			baseUrlCheck + "/api/request_ca_team/Filter_request_ca_team",
			data,
			config
		)
		.then((response) => {
			let dt = JSON.parse(response.data.data);
			let data = dt[0];
			if (data.length > 0) {
				data.forEach((element, i) => {
					element.STT =
						options.value.PageNo * options.value.PageSize + i + 1;
				});

				datalists.value = data;
			} else {
				datalists.value = [];
			}
			if (isFirst.value) isFirst.value = false;
			options.value.loading = false;
			//Show Count nếu có
			if (dt.length == 2) {
				options.value.totalRecords = dt[1][0].totalRecords;
			}			
			if (activeTeam == true) {
				goUserTeam(teamActive.value, true);
			}
		})
		.catch((error) => {
			options.value.loading = false;
			toast.error("Tải dữ liệu không thành công!");

			if (error && error.status === 401) {
				swal.fire({
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
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
	selectedDatas.value = [];
	isDynamicSQL.value = false;
	filterSQL.value = [];
	showListUser.value = false;
	teamActive.value = null;
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
			IntID: value.request_team_id,
			TextID: value.request_team_id + "",
			IntTrangthai: 1,
			BitTrangthai: value.status,
		};
		axios
			.put(
				baseUrlCheck +
					"/api/request_ca_team/update_status_request_ca_team",
				data,
				config
			)
			.then((response) => {
				if (response.data.err != "1") {
					swal.close();
					toast.success("Cập nhật trạng thái team thành công!");
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
//Xóa nhiều
const deleteList = () => {
	let listId = new Array(selectedDatas.value.length);
	if (selectedDatas.value.length == 0) {
		swal.fire({
			text: "Chọn team muốn xóa!",
			icon: "warning",
			confirmButtonText: "OK",
		});
		return;
	}
	swal.fire({
		title: "Thông báo",
		text: "Bạn có muốn xoá team này không!",
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

			selectedDatas.value.forEach((item) => {
				listId.push(item.request_team_id);
			});
			axios
				.delete(
					baseUrlCheck +
						"/api/request_ca_team/delete_request_ca_team",
					{
						headers: {
							Authorization: `Bearer ${store.getters.token}`,
						},
						data: listId != null ? listId : -1,
					}
				)
				.then((response) => {
					swal.close();
					if (response.data.err != "1") {
						swal.close();
						toast.success("Xoá team thành công!");
						checkDelList.value = false;
						loadData(true);
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
							text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
							confirmButtonText: "OK",
						});
					}
				});
		}
	});
};

//Filter
const trangThai = ref([
	{ name: "Hiển thị", code: 1 },
	{ name: "Không hiển thị", code: 0 },
]);
const listTypeTeam = ref([
	{ name: "Bình thường", code: 0 },
	{ name: "Change quy trình (Cho phép thay đổi quy trình)", code: 1 },
]);

const filterTrangthai = ref();

const reFilterTeam = () => {
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
watch(selectedDatas, () => {
	if (selectedDatas.value.length > 0) {
		checkDelList.value = true;
	} else {
		checkDelList.value = false;
	}
});
const op = ref();
const toggle = (event) => {
	op.value.toggle(event);
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
const treedatalists = ref();
const listNhomTeam = () => {
	axios
		.post(
			baseUrlCheck + "/api/request/getData",
			{
				str: encr(
					JSON.stringify({
						proc: "request_ca_groupteam_list",
						par: [
							{ par: "user_id", va: store.getters.user.user_id, },
							{ par: "status", va: true },
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
			} else {
			}
		})
		.catch((error) => {
			//toast.error("Tải dữ liệu không thành công!");
			if (error && error.status === 401) {
				swal.fire({
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};
const showListUser = ref(false);
const listUserTeam = ref([]);
const teamActive = ref();
const goUserTeam = (dataTeam, autoOpen) => {
	if (showListUser.value == false || autoOpen == true || teamActive.value.request_team_id != dataTeam.request_team_id) {
		teamActive.value = dataTeam;
		axios
		.post(
			baseUrlCheck + "/api/request/getData",
			{
				str: encr(
					JSON.stringify({
						proc: "request_user_team_list",
						par: [
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "request_team_id", va: dataTeam.request_team_id },
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
			if (data.length > 0) {
				if (data[0].length > 0) {
					data[0].forEach((el, idx) => {
						el.STT = idx + 1;
					});
				}
				listUserTeam.value = data[0];
			} else {
				listUserTeam.value = [];
			}
			showListUser.value = true;
		})
		.catch((error) => {
			//toast.error("Tải dữ liệu không thành công!");
			if (error && error.status === 401) {
				swal.fire({
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});	
	}
	else {
		showListUser.value = false;
		teamActive.value = null;
	}
};
const displayAddMember = ref(false);
const headerDialogMember = ref();
const listUserTeamModal = ref([]);
const openAddMember = () => {
	displayAddMember.value = true;
	headerDialogMember.value = "Cập nhật thành viên";
	axios
		.post(
			baseUrlCheck + "/api/request/getData",
			{
				str: encr(
					JSON.stringify({
						proc: "request_user_team_list",
						par: [
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "request_team_id", va: teamActive.value.request_team_id },
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
			if (data.length > 0) {
				if (data[0].length > 0) {
					data[0].forEach((el, idx) => {
						el.STT = idx + 1;
					});
				}
				listUserTeamModal.value = data[0];
			} else {
				listUserTeamModal.value = [];
			}			
			forceRerenderMember();
		})
		.catch((error) => {
			//toast.error("Tải dữ liệu không thành công!");
			if (error && error.status === 401) {
				swal.fire({
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});	
};
const closeDialogEditUser = () => {
	displayAddMember.value = false;
};
const componentAddMember = ref(0);
const forceRerenderMember = () => {
	componentAddMember.value += 1;
};

onMounted(() => {
	if (!checkURL(window.location.pathname, store.getters.listModule)) {
		//router.back();
	}
	loadData(true);
	listNhomTeam();
	return {
		datalists,
		options,
		onPage,
		loadData,
		loadCount,
		isFirst,
		selectedDatas,
		listNhomTeam,
		teamActive,
	};
});
</script>
<template>
	<div class="main-layout true flex-grow-1 p-2 pb-0 pr-0">
		<div class="header-view p-3">
			<h3 class="module-title mt-0 ml-2 mb-2">
				<i class="pi pi-compass"></i> Danh sách team ({{ options.totalRecords }})
			</h3>
			<Toolbar class="w-full custoolbar">
				<template #start>
					<span class="p-input-icon-left">
						<i class="pi pi-search" />
						<InputText
							v-model="options.SearchText"
							@keyup="searchData"
							type="text"
							spellcheck="false"
							placeholder="Tìm kiếm"
							style="min-width:30rem;"
						/>
						<Button
							type="button"
							class="ml-2"
							icon="pi pi-filter"
							@click="toggle"
							aria:haspopup="true"
							aria-controls="overlay_panel"
							v-tooltip="'Bộ lọc'"
							:class="filterTrangthai != null && checkFilter ? '' : 'p-button-secondary p-button-outlined'"
						/>
						<OverlayPanel
							ref="op"
							appendTo="body"
							class="p-0 m-0"
							:showCloseIcon="false"
							id="overlay_panel"
							style="width: 300px"
						>
							<div class="grid formgrid m-0">
								<div class="flex field col-12 p-0">
									<div class="col-4 text-left pt-2 p-0" style="text-align: left">
										Trạng thái
									</div>
									<div class="col-8">
										<Dropdown
											class="col-12 p-0 m-0"
											v-model="filterTrangthai"
											:options="trangThai"
											optionLabel="name"
											optionValue="code"
											placeholder="Trạng thái"
										/>
									</div>
								</div>
								<div class="flex col-12 p-0">
									<Toolbar
										class="
											border-none
											surface-0
											outline-none
											pb-0
											w-full
										"
									>
										<template #start>
											<Button
												@click="reFilterTeam"
												class="p-button-outlined"
												label="Xóa"
											></Button>
										</template>
										<template #end>
											<Button
												@click="filterFileds"
												label="Lọc"
											></Button>
										</template>
									</Toolbar>
								</div>
							</div>
						</OverlayPanel>
					</span>
				</template>

				<template #end>
					<Button
						@click="openBasic('Thêm team')"
						label="Thêm mới"
						icon="pi pi-plus"
						class="mr-2"
					/>
					<Button
						@click="openAddMember()"
						label="Cập nhật thành viên"
						icon="pi pi-plus"
						class="mr-2"
						v-if="showListUser == true"
					/>
					<Button
						v-if="checkDelList"
						@click="deleteList()"
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
		</div>
		<div class="body-view h-full flex">
			<DataTable
				class="table-ca-request"
				@page="onPage($event)"
				@sort="onSort($event)"
				@filter="onFilter($event)"
				v-model:filters="filters"
				filterDisplay="menu"
				filterMode="lenient"
				:filters="filters"
				:scrollable="true"
				scrollHeight="flex"
				:showGridlines="true"
				columnResizeMode="fit"
				:lazy="true"
				:totalRecords="options.totalRecords"
				:loading="options.loading"
				:value="datalists"
				removableSort
				v-model:rows="options.PageSize"
				paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
				:rowsPerPageOptions="[20, 30, 50, 100, 200]"
				:paginator="true"
				dataKey="request_team_id"
				responsiveLayout="scroll"
				v-model:selection="selectedDatas"
				:row-hover="true"
				:style="showListUser == true ? 'width:70%' : 'width:100%'"
			>			
				<Column
					class="align-items-center justify-content-center text-center"
					headerStyle="text-align:center;max-width:70px;height:50px"
					bodyStyle="text-align:center;max-width:70px"
					selectionMode="multiple"
				>
				</Column>

				<Column
					field="STT"
					header="STT"
					class="align-items-center justify-content-center text-center"
					headerStyle="text-align:center;max-width:70px;height:50px"
					bodyStyle="text-align:center;max-width:70px"
				></Column>

				<Column
					field="request_team_name"
					header="Tên team"
					headerStyle="text-align:left;height:50px"
					bodyStyle="text-align:left"
					:sortable="true"
				>
					<!-- <template #filter="{ filterModel }">
						<InputText
							type="text"
							v-model="filterModel.value"
							class="p-column-filter"
							placeholder="Từ khoá"
						/>
					</template> -->
				</Column>
				<Column
					field="description"
					header="Mô tả"
					class="align-items-center justify-content-center text-center"
					headerStyle="text-align:center;max-width:300px;height:50px"
					bodyStyle="text-align:center;max-width:300px"
				></Column>

				<Column
					field="is_type_team"
					header="Loại"
					headerStyle="justify-content:center;height:50px;max-width:200px;"
					bodyStyle="justify-content:center;max-width:200px;"
				>
					<template #body="{ data }">
						<span v-if="data.is_type_team == 0">Bình thường</span>
						<div class="text-center" v-if="data.is_type_team == 1">
							<span>Change quy trình</span><br/>
							<span>(Cho phép thay đổi quy trình)</span>
						</div>
					</template>
				</Column>
				<Column
					field="is_type_team"
					header="Thành viên"
					headerStyle="justify-content:center;height:50px;max-width:180px;"
					bodyStyle="text-align:center;justify-content:center;max-width:180px;"
				>
					<template #body="{ data }">
						<a class="list-user" style="cursor:pointer;" 
							@click="goUserTeam(data)"
							v-if="data.countUser == 0 || data.countUser == null">
							<Badge :value="data.countUser" severity="danger"></Badge>
						</a>
						<div v-if="data.countUser > 0" class="card-users" style="display: flex; justify-content: center">
							<AvatarGroup class="m-0"
								@click="goUserTeam(data)"
							>								
								<Avatar v-for="(u, idxUser) in data.members" :key="idxUser" 
									v-tooltip.top="{ value: u.full_name+'<br/>'+u.position_name+'<br/>'+u.department_name, escape: true }" 
									:image="u.avatar
											? basedomainURL + u.avatar
											: basedomainURL + '/Portals/Image/noimg.jpg'" 
									@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
									size="large" shape="circle"
									style="cursor: pointer;width:32px; height:32px;"
								/>
								<Avatar class="avt-over-user"
									:label="'+' + (data.countUser - 3)" 
									shape="circle" size="large" 
									style="cursor: pointer;width:32px; height:32px;background-color:#2196f3;color:#ffffff;" 
									v-if="(data.countUser - 3) > 0"
								/>
							</AvatarGroup>
						</div>
					</template>
				</Column>
				<Column
					field="status"
					header="Trạng thái"
					headerStyle="text-align:center;max-width:120px;height:50px"
					bodyStyle="text-align:center;max-width:120px"
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
					header="Chức năng"
					class="align-items-center justify-content-center text-center"
					headerStyle="text-align:center;max-width:150px;height:50px"
					bodyStyle="text-align:center;max-width:150px"
				>
					<template #body="Tem">
						<div v-if="
								store.state.user.is_super == true || store.state.user.user_id == Tem.data.created_by ||
								(store.state.user.role_id == 'admin' && store.state.user.organization_id == Tem.data.organization_id)
							"
						>
							<Button
								@click="editTem(Tem.data)"
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
								@click="delTem(Tem.data)"
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
			</DataTable>
			<DataTable
				class="tablesub-ca-request"
				filterDisplay="menu"
				filterMode="lenient"
				:scrollable="true"
				scrollHeight="flex"
				:showGridlines="true"
				columnResizeMode="fit"
				:lazy="true"
				:value="listUserTeam"
				:paginator="false"
				dataKey="request_team_id"
				responsiveLayout="scroll"
				:row-hover="true"
				style="width:calc(30% - 5px);padding-left:5px;"
				v-if="showListUser == true"
			>
				<Column
					field="STT"
					header="STT"
					class="align-items-center justify-content-center text-center"
					headerStyle="text-align:center;max-width:50px;height:50px"
					bodyStyle="text-align:center;max-width:50px"
				></Column>

				<Column
					field="user_id"
					header="Thành viên"
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
									/>
									<Avatar v-else
										class="avt-replace"
										size="large"
										shape="circle"
										v-bind:label="(data.last_name ?? '').substring(0, 1)"
										style="cursor: pointer;width:32px; height:32px;"
										:style="{ background: bgColor[data.STT % 7] + '!important'}"
									/>
							</div>
							<div class="flex" style="flex-direction:column;">
								<div>{{data.full_name}}</div>
								<div>{{data.position_name}}</div>
								<div>{{data.user_id}}</div>
							</div>
						</div>
					</template>
				</Column>
				<Column
					field="is_type"
					header="Vai trò"
					class="align-items-center justify-content-center text-center"
					headerStyle="text-align:center;max-width:120px;height:50px"
					bodyStyle="text-align:center;max-width:120px"
				>
					<template #body="{ data }">
						{{ data.is_type == 0 ? "Bình thường" : data.is_type == 1 ? "Leader" : data.is_type == 2 ? "Manager" : "" }}
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
						v-if="listUserTeam.length == 0"
					>
						<img src="../../../assets/background/nodata.png" height="144" />
						<h3 class="m-1">Không có dữ liệu</h3>
					</div>
				</template>
			</DataTable>
		</div>
	</div>

	<Dialog
		:header="headerDialog"
		v-model:visible="displayBasic"
		:style="{ width: '40vw' }"
		:closable="true"
		:modal="true"
	>
		<form @submit.prevent="">
			<div class="grid formgrid m-0">
				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					<div class="col-3 text-left flex p-0" style="align-items:center;">
						Tên team <span class="redsao pl-1"> (*)</span>
					</div>
					<Textarea
						v-model="request_team.request_team_name"
						spellcheck="false"
						class="col-9 ip36 p-2"
						autoResize
						autofocus
						rows="1"
						:class="{ 'p-invalid': v$.request_team_name.$invalid && submitted, }"
					/>
				</div>
				<div class="field col-12 md:col-12 flex p-0"
					v-if="
						((v$.request_team_name.$invalid && submitted) || v$.request_team_name.$pending.$response)
						|| ((v$.request_team_name.maxLength.$invalid && submitted) || v$.request_team_name.maxLength.$pending.$response)
					"
				>
					<div class="col-3 text-left"></div>
					<small
						v-if="(v$.request_team_name.$invalid && submitted) || v$.request_team_name.$pending.$response"
						class="col-9 p-error"
					>
						<span class="col-12 p-0">
							{{
								v$.request_team_name.required.$message
									.replace(
										"Value",
										"Tên công việc chuyên môn"
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
						v-if="(v$.request_team_name.maxLength.$invalid && submitted) || v$.request_team_name.maxLength.$pending.$response"
					>
						<div class="col-12 md:col-12 flex">
							<label class="col-2 text-left"></label>
							<span class="col-10 p-0">
								{{
									v$.request_team_name.maxLength.$message.replace(
										"The maximum length allowed is",
										"Tên team không được vượt quá"
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
						v-model="request_team.description"
						spellcheck="false"
						class="col-9 ip36 p-2"
						autoResize
						autofocus
						rows="2"
						:class="{ 'p-invalid': v$.description.maxLength.$invalid && submitted, }"
					/>
				</div>

				<div class="field col-12 md:col-12 flex p-0"
					v-if="(v$.description.maxLength.$invalid && submitted) || v$.description.maxLength.$pending.$response "
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
						v-model="selectCapcha"
						:options="treedatalists"
						:showClear="true"
						placeholder="--- Chọn nhóm ---"
						optionLabel="data.request_group_team_id"
						optionValue="data.request_group_team_team"
					></TreeSelect>
				</div>

				<div class="col-12 field md:col-12 flex p-0">
					<label class="col-3 mb-0 text-left flex p-0" style="align-items:center;">Phân loại team</label>
					<Dropdown
						class="col-9"
						v-model="request_team.is_type_team"
						:options="listTypeTeam"
						placeholder="--- Chọn loại team ---"
						optionLabel="name"
						optionValue="code"
					/>
				</div>

				<div class="col-12 field md:col-12 flex">
					<div class="field col-6 md:col-6 p-0 align-items-center flex">
						<div class="col-6 text-left p-0">STT</div>
						<InputNumber
							v-model="request_team.is_order"
							class="col-6 ip36 p-0"
						/>
					</div>
					<div class="field col-6 md:col-6 p-0 align-items-center flex">
						<div class="col-6 text-center p-0">Trạng thái</div>
						<InputSwitch v-model="request_team.status" />
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

	<dialogUpdateMember 
		:key="componentAddMember"
		:headerDialogMember="headerDialogMember"
		:displayAddMember="displayAddMember"
		:listUserTeamModal="listUserTeamModal"
		:teamActive="teamActive"
		:reloadData="loadData"
		:closeDialogEditUser="closeDialogEditUser"
	/>
	
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
.header-view {
	background-color: #f8f9fa;
}
.card-users ul {
	display: flex;
    margin: 0;
	padding: 0;
	list-style: none;
}
</style>
<style lang="scss" scoped>
	::v-deep(.table-ca-request.p-datatable-scrollable) {
		.p-datatable-tbody {
			height: calc(100vh - 260px);
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
	::v-deep(.tablesub-ca-request.p-datatable-scrollable) {
		.p-datatable-tbody {
			height: calc(100vh - 207px);
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
	
	::v-deep(.list-user) {
		.p-badge {
			min-width: 1.5rem;
			height: 1.5rem;
		}
	}

	::v-deep(.avt-over-user) {
		.p-avatar-text {
			font-size: 0.9rem;
		}
	}
</style>
    