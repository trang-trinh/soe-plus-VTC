<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
//import dialogAddToScreen from "../../components/tivi/tivi_video_list.vue";
import treeuser from "../../components/user/treeuser.vue";
import { change_unsigned, encr, decr, checkURL } from "../../util/function.js";
 
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const router = inject("router");
const toast = useToast();
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
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

const getConfigScreen = () => {
	axios
		.post(
			baseUrlCheck + "api/Tivi/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "tivi_get_config_user",
						par: [
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "organization_id", va: store.getters.user.organization_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    ).then((response) => {
		var data = JSON.parse(response.data.data);
		listUserAccess.value = data[0];
		optionsTV.value.loading = false;
	})
    .catch((error) => {
		if (error.status === 401) {
			swal.fire({				
				text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",				
				confirmButtonText: "OK",
			});
		}
    });
};
const savingConfigTV = ref(false);
const saveConfig = () => {
	if (savingConfigTV.value == true) {
		return;
	}
	var formData = new FormData();
	formData.append("listUser", JSON.stringify(listUserAccess.value));
	savingConfigTV.value = true;
	swal.fire({
		width: 110,
		didOpen: () => {
			swal.showLoading();
		},
	});
	
	axios({
		method: "post",
		url: baseUrlCheck + `/api/Tivi/UpdateConfigUser`,
		data: formData,
		headers: {
			Authorization: `Bearer ${store.getters.token}`,
		},
	})
	.then((response) => {
		if (response.data.err != "1") {
			swal.close();
			toast.success("Cập nhật thiết lập người dùng thành công!");
			getConfigScreen();
			savingConfigTV.value = false;
		} else {
			swal.close();
			savingConfigTV.value = false;
			swal.fire({
				title: "Thông báo",
				text: "Có lỗi xảy ra khi lưu thiết lập người dùng tivi!",
				icon: "error",
				confirmButtonText: "OK",
			});
		}
	})
	.catch((error) => {
		swal.close();
		savingConfigTV.value = false;
		if (error.status === 401) {
			swal.fire({				
				text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",				
				confirmButtonText: "OK",
			});
		}
	});
};

const componentKeyTV = ref(0);
const forceRerenderTV = () => {
	componentKeyTV.value += 1;
};
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
const selectedUser = ref([]);
const is_one = ref(false);
const is_type = ref();
const headerDialogUser = ref();
const displayDialogUser = ref(false);
const listUserAccess = ref([]);
const numeric_user_access = ref(0);
const showModalUser = (one, type) => {
	selectedUser.value = [];
	switch (type) {
		case 0:
			selectedUser.value = [...listUserAccess.value];
			headerDialogUser.value = "Chọn người truy cập";
			break;
		default:
			break;
	}
	is_one.value = one;
	is_type.value = type;
	displayDialogUser.value = true;
	forceRerender();
}; 
const closeDialogUser = () => {
	displayDialogUser.value = false;
};
const choiceUser = () => {
	switch (is_type.value) {
		case 0:
			var notexist = selectedUser.value.filter((a) => listUserAccess.value.findIndex((b) => b["user_id"] === a["user_id"]) === -1);
			if (notexist.length > 0) {
				notexist.forEach((el, idx) => {
					el.is_access_insurance = false;
					el.is_access_docs = false;
				});
				listUserAccess.value = listUserAccess.value.concat(notexist);
			}
			break;	
		default:
			break;
	}
	numeric_user_access.value = listUserAccess.value.length;
	closeDialogUser();
};
const optionsTV = ref({
	loading: false,
});

// Access data
const searchUserView = ref('');
const enterSearchUser = ref('');
const searchUserAccessTV = () => {
	if (searchUserView.value != null && searchUserView.value.trim() != "") {
		enterSearchUser.value = searchUserView.value.trim();
	}
	else {
		enterSearchUser.value = "";
	}
};
const filterListUserView = () => {	
	if (enterSearchUser.value != null && enterSearchUser.value != "") {
		let filterU = change_unsigned(enterSearchUser.value);
		return listUserAccess.value.filter(user => change_unsigned(user.full_name).includes(filterU));
	}
	else {
		return listUserAccess.value;
	}
}
const removeUserAccess = (data) => {
	let indexD = listUserAccess.value.findIndex(x => x.user_id == data.user_id);
	if (indexD >= 0) {
		listUserAccess.value.splice(indexD, 1);
	}
};
const removeAllUserAccess = () => {
	listUserAccess.value = listUserAccess.value.filter(x => selectedUsersAccess.value.findIndex(y => y.user_id == x.user_id) == -1);
};
const selectedUsersAccess = ref([]);
const checkDelList = ref(false);
watch(selectedUsersAccess, () => {
	if (selectedUsersAccess.value.length > 0) {
		checkDelList.value = true;
	} else {
		checkDelList.value = false;
	}
});

// const getDataToTivi = () => {
//   axios
//     .post(
//       "http://localhost:8080/" 
// 	  //basedomainURL 
// 	  + "/api/Tivi/List_TiviPublic",
//       {
// 		str: encr(JSON.stringify({
// 					token: "TiXaxJ1/u6DxEoLsEqP4YzpJ2IcBQmrMQQ0PGkUkr8rLbiug4nhtKkx9yFbPyVg4tnzIf4Or2bMn43/Cyj7CtsqhO5m4nts4siYbSwrxq2vppnidhiHpceuKF+4EuhKbiJ2qEMoAYGG01AWVoUyzEQ==",
// 					//token: "ZeiCsoE1M7hmz1lzmjU7WLIvGOOU/LEIGyxYT2HlWzmW2tNaMpYSn+c/DyXx+QKHDOdiAxwqYqorpcrgvgyDCp7lxt5HpENBD8kzikqjunb5grYabietoS5COYOFPE5iaXIvux4bYGn0C8RXba+pc0B062Ncurnuj8iwRCkXNUHWIXtYxM3yGJh7I+3chpcEfjIEmckJdDRALTGXUhN8h+NqbySHCB4c5T7J3626w4V9cgLBH2/0Gnc4+xxRyN20ttt6qqdR2y2cIkkCw8YxrU8lqZyFj27Z95HWIi4jo7c=",
// 					user_id: 'admin.bhbqp',
// 					idtivi: "F6FCB0CF-A7E0-582D-A696-54E044B99782",
// 					tiviname: "tivi test 02"
// 				}), SecretKey, cryoptojs
// 			).toString()
//       },
//       config
//     )
//     .then((response) => {
//       if (response != null && response.data != null) {
//         var data = response.data.data;
//         if (data != null) {
//           var dataTivi = JSON.parse(decr(data, SecretKey, cryoptojs));
// 		  debugger;
//         }
// 		if (response.data.err == "0") {
// 			toast.success("Thành công");
// 		}
// 		else {
// 			toast.error(response.data.ms);
// 		}
//       }
//     })
//     .catch((error) => {});
// };
onMounted(() => {
	if (!checkURL(window.location.pathname, store.getters.listModule)) {
		//router.back();
	}
	getConfigScreen();
	//getDataToTivi();
	return {
		getConfigScreen,
	};
});
</script>
<template>
	<div class="main-layout h-full p-3">
		<Card class="h-full card-tivi-screen">
			<template #content>
				<div>
					<div class="flex align-items-center mb-3" style="justify-content: space-between;">
						<div>
							<h3 class="m-0 pl-3">
								<i class="pi pi-cog"></i>
								 Cấu hình user tra cứu dữ liệu
							</h3>
						</div>
						<div class="mr-3">
							<Button
								class="p-button-sm p-button-outlined p-button-secondary bg-white px-2 py-2"
								type="button"
								icon="pi pi-plus-circle"
								label="Thêm người dùng"
								@click="showModalUser(false, 0)"
								style="font-size: 1rem;"
							/>
						</div>
					</div>					
					<div class="grid w-full p-0 m-0" style="border-top: 1px solid #dee2e6;">
						<DataTable
							:value="filterListUserView()"
							:paginator="false"
							:scrollable="true"
							scrollHeight="flex"
							:loading="optionsTV.loading"
							v-model:selection="selectedUsersAccess"
							:lazy="true"
							dataKey="user_id"
							:rowHover="true"
							:showGridlines="true"
							responsiveLayout="scroll"
							class="w-full table-user-access"
						>
							<template #header>
								<div class="flex justify-content-center align-items-center" style="justify-content: space-between !important;">
									<h3 class="m-0">Danh sách cá nhân có quyền truy cập ({{(filterListUserView()).length}})</h3>
									<span class="p-input-icon-left w-30rem">
										<i class="pi pi-search" />
										<InputText class="w-full" v-model="searchUserView" @keydown.enter.exact.prevent="searchUserAccessTV()" placeholder="Tìm kiếm người dùng" />
									</span>
									<Button icon="pi pi-trash" class="p-button-danger" label="Xóa" 
										:style="!checkDelList ? 'background-color: transparent; color: transparent; border: transparent;' : ''" 
										:disabled="!checkDelList" 
										@click="removeAllUserAccess()" />
								</div>
							</template>
							<Column
								selectionMode="multiple"
								headerStyle="text-align:center;max-width:75px;height:50px"
								bodyStyle="text-align:center;max-width:75px;"
								class="align-items-center justify-content-center text-center"
							></Column>
							<Column
								field="full_name"
								header="Người dùng"
								headerStyle="height:50px"
							>
								<template #body="slotProps">
									<div class="field col-12 flex p-0 m-0 cursor-pointer"
										:style="
											slotProps.data.active
											? 'background-color:#bed3f5'
											: 'background-color:none'
										"
									>
										<div class="w-5rem p-0 flex align-items-center justify-content-center">
											<img class="ava" width="42" height="42" 
												v-bind:src="basedomainURL + slotProps.data.avatar"
												@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
												style="border:1px solid #ccc;"
												v-if="slotProps.data.avatar"
											/>
											<Avatar v-else
												class="mt-2"
												size="large"
												shape="circle"
												v-bind:label="(slotProps.data.last_name ?? '').substring(0, 1)"
												style="cursor: pointer;width: 42px; height: 42px;border:1px solid #ccc;"
												:style="{ background: bgColor[(slotProps && slotProps.data && slotProps.data.index ? slotProps.data.index : 0) % 7], }"
											/>
										</div>
										<div class="flex flex-1 p-0 pl-2 align-items-center">
											<div class="pt-0" style="line-height: 1.5;">
												<div class="font-bold">
													{{ slotProps.data.full_name }}
												</div>
												<div class="flex w-full" style="flex-direction: column;">
													<div class="flex">Phòng ban: {{ slotProps.data.organization_name || '' }}</div>
													<div class="flex">Chức vụ: {{ slotProps.data.position_name || '' }}</div>
												</div>
											</div>
										</div>
									</div>
								</template>
							</Column>
							<Column
								field="status"
								header="Bảo hiểm"
								headerStyle="text-align:center;max-width:120px;height:50px"
								bodyStyle="text-align:center;max-width:120px;"
								class="align-items-center justify-content-center text-center"
							>
								<template #body="slotProps">
									<Checkbox
										:binary="true"
										v-model="slotProps.data.is_access_insurance"
									/>
								</template>
							</Column>
							<Column
								field="status"
								header="Văn bản, công việc"
								headerStyle="text-align:center;max-width:150px;height:50px"
								bodyStyle="text-align:center;max-width:150px;"
								class="align-items-center justify-content-center text-center"
							>
								<template #body="slotProps">
									<Checkbox
										:binary="true"
										v-model="slotProps.data.is_access_docs"
									/>
								</template>
							</Column>
							<Column
								header="Chức năng"
								class="align-items-center justify-content-center text-center"
								headerStyle="text-align:center;max-width:120px;height:50px"
								bodyStyle="text-align:center;max-width:120px;"
							>
								<template #body="user">
									<div>
										<Button
											@click="removeUserAccess(user.data)"
											class="p-button-rounded p-button-danger p-button-outlined mx-1"
											type="button"
											icon="pi pi-trash"
											v-tooltip="'Xóa'"
										/>
									</div>
								</template>
							</Column>
							<template #empty>
								<div
									class="align-items-center justify-content-center p-4 text-center m-auto"
									v-if="(filterListUserView()).length == 0"
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
			</template>
			<template #footer>
				<div class="text-center">
					<Button
						icon="pi pi-save"
						label="Cập nhật"
						@click="saveConfig"
					/>
				</div>
			</template>
		</Card>
	</div>
	<treeuser
		:key="componentKey"
		:headerDialog="headerDialogUser"
		:displayDialog="displayDialogUser"
		:closeDialog="closeDialogUser"
		:one="is_one"
		:selected="selectedUser"
		:choiceUser="choiceUser"
	/>
</template>
<style scoped>
	img.ava {
		border-radius: 50%;
		vertical-align: middle;
		object-fit: cover;
	}
</style>
<style lang="scss" scoped>
	::v-deep(.card-tivi-screen) {
		.p-card-content {
			padding: 0;
			height: calc(100vh - 160px);
			overflow-y: auto;
		}
	}
	::v-deep(.table-user-access) {
		.p-datatable-wrapper {
			margin-bottom: 1rem;
    		display: inline-table;
		}
	}
</style>
