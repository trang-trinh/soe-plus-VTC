<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
//Khai báo
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const isFirst = ref(true);

const config = {
	headers: { Authorization: `Bearer ${store.getters.token}` },
};

const options = ref({
	IsNext: true,
	sort: "project_id",
	SearchText: "",
	SearchTable: "",
	PageNo: 0,
	PageSize: 20,
	FilterUsers_ID: null,
	loading: true,
	totalRecords: null,
});

const listDatabase = ref([]);
const dbSelected = ref();
const projectLogo = ref();
const tableSelected = ref();
const listGroupName = ref();
const groupTable = ref([]);
const loadData = (rf) => {
	(async () => {
		listDatabase.value = [];
		await axios
			.post(
				baseURL + "/api/Proc/CallProc",
				{
					proc: "api_project_database",
					par: [{ par: "db_name", va: dbSelected.value }],
				},

				config
			)
			.then((response) => {
				let data = JSON.parse(response.data.data)[0];
				dbSelected.value = data[0].db_name;
				projectLogo.value = data[0].project_logo;

				data.forEach((element) => {
					let db = { name: element.db_name, code: element.db_name };
					listDatabase.value.push(db);
				});
				refeshTable();
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

		listTable.value = [];
		listGroupName.value = [];
		groupTable.value = [];
		await axios
			.post(
				baseURL + "/api/Proc/CallProc",
				{
					proc: "api_groupname_list",
					par: [{ par: "db_name", va: dbSelected.value }],
				},
				config
			)
			.then((response) => {
				let data = JSON.parse(response.data.data)[0];

				data.forEach((element) => {
					if (element.group_name != null) {
						groupTable.value.push({
							name: element.group_name,
							code: element.group_name,
						});
						listGroupName.value.push(element.group_name);
					}
				});
				listGroupName.value.push("Khác");
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
		await axios
			.post(
				baseURL + "/api/Proc/CallProc",
				{
					proc: "api_table_list",
					par: [
						{ par: "db_name", va: dbSelected.value },
						{ par: "search", va: options.value.SearchText },
						{ par: "status", va: options.value.Status },
					],
				},
				config
			)
			.then((response) => {
				let data = JSON.parse(response.data.data)[0];
				let i = 0;
				data.forEach((element) => {
					element.is_order = i + 1;

					i++;
				});
				listTable.value = data;
				renderTree(data);
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
	})();
};
const datalists = ref();
const renderTree = (data) => {
	let arrChils = [];
	let i = 0;
	listGroupName.value
		.filter((x) => x != null)
		.forEach((m) => {
			let om = { key: m, data: m, label: m, isGroup: true };
			if (m != "Khác") {
				let dts = data.filter((x) => x.group_name == m);
				if (dts.length > 0) {
					if (!om.children) om.children = [];
					dts.forEach((em) => {
						em.is_order = i + 1;
						i++;
						let om1 = {
							key: em.table_id,
							data: em,
							label: em.table_name,
						};
						om.children.push(om1);
					});
				}
				if (om.children && om.children.length > 0) {
					arrChils.push(om);
				}
			} else {
				let dsn = data.filter((x) => x.group_name == null);
				if (dsn.length > 0) {
					if (!om.children) om.children = [];
					dsn.forEach((em) => {
						em.is_order = i + 1;
						i++;
						let om1 = {
							key: em.table_id,
							data: em,
							label: em.table_name,
						};
						om.children.push(om1);
					});
					if (om.children && om.children.length > 0) {
						arrChils.push(om);
					}
				}
			}
		});
	datalists.value = arrChils;
};
const loadTable = () => {
	nameTable.value = null;
	desTable.value = null;
	tableID.value = null;
	tableSelected.value = null;
	listCol.value = [];
	listTable.value = [];
	listGroupName.value = [];
	(async () => {
		await axios
			.post(
				baseURL + "/api/Proc/CallProc",
				{
					proc: "api_project_database",
					par: [{ par: "db_name", va: dbSelected.value }],
				},
				config
			)
			.then((response) => {
				let data = JSON.parse(response.data.data)[0];
				projectLogo.value = data[0].project_logo;
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

		await axios
			.post(
				baseURL + "/api/Proc/CallProc",
				{
					proc: "api_groupname_list",
					par: [{ par: "db_name", va: dbSelected.value }],
				},
				config
			)
			.then((response) => {
				let data = JSON.parse(response.data.data)[0];

				data.forEach((element) => {
					if (element.group_name != null) {
						listGroupName.value.push(element.group_name);
					}
				});
				listGroupName.value.push("Khác");
			});
		await axios
			.post(
				baseURL + "/api/Proc/CallProc",
				{
					proc: "api_table_list",
					par: [
						{ par: "db_name", va: dbSelected.value },
						{ par: "search", va: options.value.SearchTable },
						{ par: "status", va: options.value.Status },
					],
				},
				config
			)
			.then((response) => {
				let data = JSON.parse(response.data.data)[0];
				let i = 0;
				data.forEach((element) => {
					element.is_order = i + 1;
				});

				listTable.value = data;
				renderTree(data);
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
	})();
};
const listTable = ref([]);
const listCol = ref([]);
function renderCol(data) {
	listCol.value = data;

	listCol.value.forEach((element) => {
		if (element.table_forenkey_id != null) {
			axios
				.post(
					baseURL + "/api/Proc/CallProc",
					{
						proc: "api_col_get",
						par: [{ par: "col_id", va: element.col_forenkey_id }],
					},
					config
				)
				.then((response) => {
					let data = JSON.parse(response.data.data)[0];
					if (!element.col_forenkey_name)
						element.col_forenkey_name = "";
					element.col_forenkey_name = data[0].col_name;
				});

			listTable.value.forEach((item) => {
				if (item.table_id == element.table_forenkey_id) {
					if (!element.table_forenkey_name)
						element.table_forenkey_name = "";
					element.table_forenkey_name = item.table_name;
					return;
				}
			});
		}
	});
}
const nameTable = ref("");
const desTable = ref("");
const tableID = ref();
const nodeSelected = ref();
const loadCol = (node) => {
	if (node.data.table_id) {
		nodeSelected.value = node;
		nameTable.value = node.data.table_name;
		desTable.value = node.data.des;
		tableID.value = node.data.table_id;
		axios
			.post(
				baseURL + "/api/Proc/CallProc",
				{
					proc: "api_col_list",
					par: [
						{ par: "table_id", va: node.data.table_id },
						{ par: "pageno", va: options.value.PageNo },
						{ par: "pagesize", va: options.value.PageSize },
						{ par: "search", va: options.value.SearchText },
						{ par: "status", va: options.value.Status },
					],
				},
				config
			)
			.then((response) => {
				let data = JSON.parse(response.data.data)[0];
				data.forEach((element, i) => {
					element.is_order = i + 1;
					if (element.col_type.includes("(-1)")) {
						element.col_type = element.col_type.replace(
							"(-1)",
							"(MAX)"
						);
					}
				});
				renderCol(data);
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
const onCellEditComplete = (event) => {
	let { data, newValue, field } = event;
	data[field] = newValue;

	axios
		.put(baseURL + "/api/api_table/Update_Colum", data, config)
		.then((response) => {
			if (response.data.err != "1") {
				updateDescription(data);
				swal.close();
			} else {
				console.log("LỖI A:", response);
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
};
const updateDescription = (data) => {
	axios
		.post(
			baseURL + "/api/Proc/CallProc",
			{
				proc: "api_AutoUpdateDescriptionCol",
				par: [
					{ par: "table_id", va: data["table_id"] },
					{ par: "col_name", va: data["col_name"] },
					{ par: "content_des", va: data["des"] },
				],
			},
			config
		)
		.then((response) => {
			toast.success("Cập nhật bảng thành công");
		});
};
const searchCol = (event) => {
	if (event.code == "Enter") {
		options.value.loading = true;
		loadCol(nodeSelected.value);
	}
};
const refeshCol = (node) => {
	if (node) {		
		loadCol(node);
	}
	else {		
		loadCol(nodeSelected.value);
	}
	axios
		.post(
			baseURL + "/api/Proc/CallProc",
			{
				proc: "api_AutoRefershCol",
				par: [{ par: "db_name", va: dbSelected.value }],
			},
			config
		)
		.then(() => {
			if (!node) {
				toast.success("Cập nhật các trường thành công!");
			}
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
};
const delColumn = (Col) => {
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá cột này không!",
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
			.delete(baseURL + "/api/api_table/Delete_api_cols", {
			headers: { Authorization: `Bearer ${store.getters.token}` },
            data: Col != null ? [Col.col_id] : 1,
          })
          .then((response) => {
            swal.close();
            //console.log(response.data.err);
            if (response.data.err != "1") {
              	swal.close();
              	toast.success("Xoá cột dữ liệu thành công!");
				//loadData(true);
				refeshCol();
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
const refeshTable = () => {
	axios
		.post(
			baseURL + "/api/Proc/CallProc",
			{
				proc: "api_AutoRefershTable",
				par: [{ par: "db_name", va: dbSelected.value }],
			},
			config
		)
		.then(() => {
			toast.success("Cập nhật bảng thành công!");
			options.value.SearchTable = "";
			loadTable(nodeSelected.value);
		});
};
const searchTable = (event) => {
	if (event.code == "Enter") {
		options.value.loading = true;
		loadTable(nodeSelected.value);
	}
};
//Xuất excel
const projectButs = ref();
const itemButs = ref([
	{
		label: "Xuất Excel",
		icon: "pi pi-file-excel",
		command: (event) => {
			exportData("ExportExcel");
		},
	},
	// {
	// 	label: "Import Excel",
	// 	icon: "pi pi-file-excel",
	// 	command: (event) => {
	// 		exportData("ExportExcel");
	// 	},
	// },
]);
const toggleExport = (event) => {
	projectButs.value.toggle(event);
};
const exportData = (method) => {
	swal.fire({
		width: 110,
		didOpen: () => {
			swal.showLoading();
		},
	});
	axios
		.post(
			baseURL + "/api/Excel/ExportExcel_API_Table",
			{
				excelname: "DANH SÁCH TRƯỜNG DỮ LIỆU",
				proc: "api_col_listexport",
				par: [
					{ par: "table_id", va: tableID.value },
					{ par: "db_name", va: dbSelected.value }
				],
			},
			config
		)
		.then((response) => {
			swal.close();
			if (response.data.err != "1") {
				swal.close();
				toast.success("Kết xuất Data thành công!");
				if (response.data.path != null) {
					let pathReplace = response.data.path.replace(/\\+/g, '/').replace(/\/+/g, '/').replace(/^\//g, '');
					var listPath = pathReplace.split('/');
					var pathFile = "";
					listPath.forEach(item => {
					if (item.trim() != "")
					{
						pathFile += "/" + item;
					}
					});
					//window.open(baseURL + response.data.path);
					window.open(baseURL + pathFile);
				}
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
			if (error.status === 401) {
				swal.fire({
					text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};
const headerTable = ref();
const displayTable = ref(false);
const table = ref({
	table_name: "",
	des: "",
	is_order: 1,
	status: true,
});
const openTable = (data) => {
	table.value = data;
	headerTable.value = "Cập nhật bảng";
	displayTable.value = true;
};
const closeTable = () => {
	displayTable.value = false;
	table.value = {
		table_name: "",
		des: "",
		is_order: 1,
		status: true,
	};
};
const saveTable = () => {
	axios
		.put(baseURL + "/api/api_table/Update_Table", table.value, config)
		.then((response) => {
			if (response.data.err != "1") {
				swal.close();
				toast.success("Cập nhật bảng thành công");
				loadData();
				closeTable();
			} else {
				console.log("LỖI A:", response);
				swal.fire({
					title: "Error!",
					text: response.data.ms,
					icon: "error",
					confirmButtonText: "OK",
				});
			}
		})
		.catch((error) => {
			console.log("moe", error);
			swal.close();
			swal.fire({
				title: "Error!",
				text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
				icon: "error",
				confirmButtonText: "OK",
			});
		});
};
const columns = ref([
	{ field: "is_null", header: "Null" },
	{ field: "is_key", header: "Key" },
	{ field: "is_identity", header: "Identity" },
	{ field: "status", header: "Trạng thái" },
	{ field: "table_forenkey_name", header: "Bảng liên quan" },
	{ field: "col_forenkey_name", header: "Cột liên quan" },
]);
const selectedColumns = ref();
const isCheckTable = ref(true);
const onToggle = (val) => {
	selectedColumns.value = columns.value.filter((col) => val.includes(col));

	if (selectedColumns.value.length == 0) {
		isCheckTable.value = true;
	} else {
		isCheckTable.value = false;
	}
};
const expandedKeys = ref({});
const basedomainURL = baseURL;
const groupName = ref();
const onNodeSelect = (node) => {
	groupName.value = node.key;
	//loadCol(node);
	refeshCol(node);
	expandedKeys.value[node.key] = true;
};
const onNodeUnselect = (node) => {
	expandedKeys.value[node.key] = false;
};
const checkEditGroupName = ref();
const editGroupName = (data) => {
	groupNameNew.value = null;
	checkEditGroupName.value = data.key;
};
const cancelEditGroupName = (value) => {
	checkEditGroupName.value = "1";
};
const saveGroupName = (oldGroupName) => {
	let check = false;
	expandedKeys.value[oldGroupName.key] = false;
	listGroupName.value.forEach((element) => {
		if (
			element == groupNameNew.value &&
			groupNameNew.value != oldGroupName.key
		) {
			toast.warning("Đã tồn tại tên nhóm.");
			check = true;
			return;
		}
	});
	if (check == true) {
		return;
	}
	if (groupNameNew.value == "") {
		groupNameNew.value = null;
	}
	let groupNameC = {
		groupNameOld: oldGroupName.key,
		groupNameNew: groupNameNew.value,
	};
	axios
		.put(baseURL + "/api/api_table/Update_Groupname", groupNameC, config)
		.then((response) => {
			if (response.data.err != "1") {
				swal.close();
				toast.success("Cập nhật bảng thành công");
				loadData();
			} else {
				console.log("LỖI A:", response);
				swal.fire({
					title: "Error!",
					text: response.data.ms,
					icon: "error",
					confirmButtonText: "OK",
				});
			}
		})
		.catch((error) => {
			console.log("moe", error);
			swal.close();
			swal.fire({
				title: "Error!",
				text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
				icon: "error",
				confirmButtonText: "OK",
			});
		});
};
const groupNameNew = ref("");
onMounted(() => {

	loadData();
	return {
		loadData,				
	};
});
</script>
<template>
	<div class="surface-100 w-full">
		<div class="w-full">
			<Splitter class="w-full">
				<SplitterPanel :size="20">
					<div class="m-3 flex" style="align-items: center;">
						<div>
							<img
								:src="
									projectLogo
										? basedomainURL + projectLogo
										: '/src/assets/image/noimg.jpg'
								"
								alt=""
								class="p-0 pr-2"
								width="45"
							/>
						</div>
						<Dropdown
							v-model="dbSelected"
							:options="listDatabase"
							optionLabel="name"
							optionValue="code"
							placeholder="Chọn database"
							class="w-full"
							@change="loadTable"
							style="height: 34px;"
						>
						</Dropdown>
						<Button
							class="
								w-2
								ml-2
								p-button-outlined p-button-secondary
							"
							icon="pi pi-refresh"
							style="border-color: #ced4da !important;height: 34px;"
							@click="refeshTable"
						/>
					</div>
					<div style="height: calc(100vh - 130px)">
						<div class="flex col-12 p-0" style="border-top: 1px solid #dee2e6">
							<Toolbar class="border-none surface-0 outline-none pb-0 w-full" style="display:block">
								<template #start>
									<span class="p-input-icon-left" style="width: 100%">
										<i class="pi pi-search" />
										<InputText
											v-model="options.SearchTable"
											@keyup="searchTable"
											type="text"
											spellcheck="false"
											placeholder="Tìm kiếm"
											style="width: 100%"
										/>
									</span>
								</template>
								<template #end></template>
							</Toolbar>
						</div>
						<Tree
							:value="datalists"
							selectionMode="single"
							v-model:selectionKeys="tableSelected"
							:metaKeySelection="false"
							class="h-full w-full overflow-x-hidden"
							style="border:none;height:calc(100% - 40px) !important;"
							scrollHeight="flex"
							:expandedKeys="expandedKeys"
							@nodeSelect="onNodeSelect"
							@nodeUnselect="onNodeUnselect"
							
						>
						<!-- v-if="slotProps.node.children && slotProps.node.children.length > 0" -->
							<template #default="slotProps" >
								<div class="flex">
									<div
										class="country-item flex w-full pt-2"
										style="
											padding: 3px 0 !important;
											align-items: center;
										"
										v-if="(slotProps.node.children && slotProps.node.children.length > 0) || slotProps.node.isGroup"
									>
										<img
											src="../../assets/image/folder.png"
											width="22"
											height="30"
											style="object-fit: contain"
										/>
										<div>
											<div
												v-if="checkEditGroupName != slotProps.node.key"
												class="px-2 text-lg"
												style="line-height: 36px"
											>
												{{ slotProps.node.label }} ({{slotProps.node.children && slotProps.node.children.length > 0 ? slotProps.node.children.length : 0}})
											</div>
											<div v-else class="flex">
												<InputText
													@keyup.enter="saveGroupName(slotProps.node)"
													v-model="groupNameNew"
													spellcheck="false"
													autofocus="true"
													class="m-2"
												></InputText>
												<Button
													class="
														p-button-rounded
														p-button-secondary
														p-button-outlined
														m-2
													"
													type="button"
													icon="pi pi-times"
													@click="cancelEditGroupName(slotProps.node.key)"
												></Button>
											</div>
										</div>

										<div
											class="w-2 pt-1"
											style="padding-top:0.1rem !important;"
											v-if="groupName == slotProps.node.key"
										>
											<Button
												v-if="checkEditGroupName != slotProps.node.key"
												@click="editGroupName(slotProps.node)"
												class="
													p-button-rounded
													p-button-secondary
													p-button-outlined
													mx-1
												"
												type="button"
												icon="pi pi-pencil"
												style="width:2rem; height:2rem;"
											></Button>
										</div>
									</div>
									<div
										class="country-item flex w-full pt-2"
										style="padding: 3px 0 !important;"
										v-else
									>
										<div
											class="w-full"
											style="
												justify-content: center;
												display: flex;
												flex-direction: column;
											"
										>
											<div class="flex">
												<i
													class="pi pi-table pt-1"
													style="font-size: 1.125rem"
												></i>
												<div class="px-2 text-lg">
													{{ slotProps.node.label }}
												</div>
											</div>
											<div
												class="flex"
												v-if="slotProps.node.data.des"
											>
												<i
													class="pi pi-table"
													style="
														font-size: 1.125rem;
														color: transparent;
													"
												></i>
												<small class="px-2 font-italic">
													{{
														slotProps.node.data.des
													}}
												</small>
											</div>
										</div>
										<div
											class="w-2 pt-1"
											style="padding-top:0.1rem !important;"
											v-if="
												tableID ==
												slotProps.node.data.table_id
											"
										>
											<Button
												@click="
													openTable(
														slotProps.node.data
													)
												"
												class="
													p-button-rounded
													p-button-secondary
													p-button-outlined
													mx-1
												"
												type="button"
												icon="pi pi-pencil"
												style="width:2rem; height:2rem;"
											></Button>
										</div>
									</div>
								</div>
							</template>
						</Tree>
					</div>
				</SplitterPanel>
				<SplitterPanel :size="80">
					<div>
						<div class="h-2rem p-3 pb-0 mb-0 surface-0 w-full">
							<h3 class="m-0 text-primary">
								<i
									class="pi pi-table"
									style="font-size: 1rem"
								></i>
								{{ nameTable ? nameTable : "Chưa chọn bảng" }}
								<Chip
									:label="desTable"
									class="mr-2 mb-2 custom-chip"
								/>
							</h3>
						</div>
						<Toolbar
							class="outline-none mr-0 surface-0 border-none mt-1"
						>
							<template #start>
								<span class="p-input-icon-left">
									<i class="pi pi-search" />
									<InputText
										v-model="options.SearchText"
										@keyup="searchCol"
										type="text"
										spellcheck="false"
										placeholder="Tìm kiếm"
									/>
								</span>
							</template>

							<template #end>
								<MultiSelect
									:modelValue="selectedColumns"
									:options="columns"
									optionLabel="header"
									class="mx-2"
									placeholder="Select Columns"
									style="width: 10em"
									@update:modelValue="onToggle"
								/>
								<Button
									class="
										mr-2
										p-button-outlined p-button-secondary
									"
									icon="pi pi-refresh"
									@click="refeshCol"
								/>

								<Button
									label="Xuất Excel"
									icon="pi pi-file-excel"
									class="
										mr-2
										p-button-outlined p-button-secondary
									"
									aria-haspopup="true"
									aria-controls="overlay_Export"
									@click="exportData"
								/>
								<!-- <Menu
									id="overlay_Export"
									ref="projectButs"
									:popup="true"
									:model="itemButs"
								/> -->
							</template>
						</Toolbar>
						<div class="d-lang-table mx-3">
							<DataTable
								:value="listCol"
								:scrollable="true"
								scrollHeight="flex"
								:lazy="true"
								:rowHover="true"
								filterDisplay="menu"
								:showGridlines="true"
								responsiveLayout="scroll"
								editMode="cell"
								@cell-edit-complete="onCellEditComplete"
								data-key="col_id"
							>
								<Column
									field="is_order"
									header="STT"
									headerStyle="text-align:center;max-width:75px;height:50px"
									bodyStyle="text-align:center;max-width:75px;;max-height:60px"
									class="
										align-items-center
										justify-content-center
										text-center
									"
								>
									<template #body="data">
										<span 
										:style="data.data['status'] == false ? 'color:red' : ''"
										>
										{{ data.data['is_order'] }}
										</span>
									</template>
								</Column>

								<Column
									field="col_name"
									header="Tên cột"
									headerStyle="max-width:200px;height:50px"
									bodyStyle="max-width:200px;max-height:60px"
								>
									<template #body="data">
										<span 
										:style="data.data['status'] == false ? 'color:red' : ''"
										>
										{{ data.data['col_name'] }}
										</span>
									</template>
								</Column>
								<Column
									field="col_type"
									header="Kiểu dữ liệu"
									headerStyle="text-align:center;max-width:150px;height:50px"
									bodyStyle="text-align:center;max-width:150px;;max-height:60px"
									class="
										align-items-center
										justify-content-center
										text-center
									"
								>
									<template #body="data">
										<span 
										:style="data.data['status'] == false ? 'color:red' : ''"
										>
										{{ data.data['col_type'] }}
										</span>
									</template>
								</Column>
								<Column
									field="des"
									header="Mô tả"
									headerStyle="text-align:center;height:50px; padding:0"
									bodyStyle="text-align:center;max-height:60px; padding:0"
									class="px-2"
									v-if="isCheckTable"
								>
									<template #editor="{ data, field }">
										<Textarea
											v-model="data[field]"
											autofocus
											class="
												w-full
												p-0
												h-full
												surface-200
											"
											style="padding:5px !important; {{data[status] == false ? 'color:red' : ''}}"
										/>
									</template>
								</Column>
								<Column
									headerStyle="text-align:center;max-width:150px;height:50px"
									bodyStyle="text-align:center;max-width:150px;;max-height:60px"
									class="
										align-items-center
										justify-content-center
										text-center
									"
									v-for="(col, index) of selectedColumns"
									:field="col.field"
									:header="col.header"
									:key="index"
								>
									<template
									:style="data.data['status'] == false ? 'color:red' : ''"
										v-if="
											col.field == 'is_key' ||
											col.field == 'is_identity' ||
											col.field == 'is_null' ||
											col.field == 'status'
										"
										#body="data"
									>
										<Checkbox
											:binary="data.data[col.field]"
											v-model="data.data[col.field]"
											disabled="true"
										/>
									</template>
								</Column>
								<Column
									header=""
									headerStyle="text-align:center;max-width:55px;height:50px"
									bodyStyle="text-align:center;max-width:55px;max-height:60px"
									class=" align-items-center justify-content-center text-center"
								>
									<template #body="Col">
										<Button 
										v-if="Col.data.status == false"
										@click="delColumn(Col.data, true)"
										class="p-button-rounded p-button-secondary p-button-outlined mx-1"
										type="button"
										v-tooltip="'Xóa'"
										icon="pi pi-trash"
										style="color:red"
										></Button>
									</template>
								</Column>

								<template #empty>
									<div
										class="
											align-items-center
											justify-content-center
											p-4
											text-center
											m-auto
										"
										v-if="!isFirst"
									>
										<img
											src="../../assets/background/nodata.png"
											height="144"
										/>
										<h3 class="m-1">Không có dữ liệu</h3>
									</div>
								</template>
							</DataTable>
						</div>
					</div>
				</SplitterPanel>
			</Splitter>
		</div>
	</div>
	<Dialog
		:header="headerTable"
		v-model:visible="displayTable"
		:style="{ width: '40vw' }"
	>
		<form>
			<div class="grid formgrid m-2">
				<div class="field col-12 md:col-12">
					<label class="col-2 text-left p-0">Tên bảng </label>
					<InputText
						v-model="table.table_name"
						spellcheck="false"
						class="col-10 ip36 px-2"
						:disabled="true"
					/>
				</div>
				<div style="display: flex" class="col-12 field md:col-12">
					<div class="field col-4 md:col-4 p-0">
						<label class="col-6 text-left p-0">Số thứ tự </label>
						<InputNumber
							v-model="table.is_order"
							class="col-6 ip36 p-0"
						/>
					</div>
					<div class="field col-6 md:col-6 p-0">
						<label
							style="vertical-align: text-bottom"
							class="col-6 text-center p-0"
							>Trạng thái
						</label>
						<InputSwitch v-model="table.status" class="col-6" />
					</div>
				</div>
				<div class="field col-12 md:col-12 flex">
					<label class="col-2 text-left p-0">Mô tả</label>
					<div class="col-10 p-0">
						<Textarea
							spellcheck="false"
							v-model="table.des"
							class="col-12 ip36"
							autoResize
							autofocus
							style="padding:0.5rem;"
						/>
					</div>
				</div>
				<div class="field col-12 md:col-12 flex">
					<label class="col-2 text-left p-0">Nhóm</label>
					<div class="col-10 p-0">
						<Dropdown
							spellcheck="false"
							v-model="table.group_name"
							:options="groupTable"
							optionLabel="name"
							optionValue="code"
							:editable="true"
						/>
					</div>
				</div>
			</div>
		</form>
		<template #footer>
			<Button
				label="Hủy"
				icon="pi pi-times"
				@click="closeTable"
				class="p-button-text"
			/>

			<Button label="Lưu" icon="pi pi-check" @click="saveTable()" />
		</template>
	</Dialog>
</template>

<style scoped>
.d-lang-table {
	height: calc(100vh - 170px);
}
.d-table-container {
	height: calc(100vh - 500px);
}
.d-btn-function {
	border-radius: 50%;
	margin-left: 6px;
}
.inputanh {
	border: 1px solid #ccc;
	width: 96px;
	height: 96px;
	cursor: pointer;
	padding: 1px;
}
.ipnone {
	display: none;
}
.inputanh img {
	object-fit: cover;
	width: 100%;
	height: 100%;
}
</style>