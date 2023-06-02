<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { VuemojiPicker } from "vuemoji-picker";
import moment from "moment";
import { encr } from "../../util/function";
const cryoptojs = inject("cryptojs");
//Khai báo
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const router = inject("router");

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const rules = {
  user_id: {
    required,
	maxLength: maxLength(50),
    $errors: [
      {
        $property: "user_id",
        $validator: "required",
        $message: "Tên đăng nhập không được để trống!",
      },
    ],
  },
};
const infoAccount = ref({
	user_id: null
});
const v$ = useVuelidate(rules, infoAccount);
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const toast = useToast();
const layout = ref("list");
let files = [];
const isDisplayAvt = ref(false);
const handleFileAvtUpload = (event) => {
	isDisplayAvt.value = true;
	files = event.target.files;
	var output = document.getElementById("userAvt");
	output.src = URL.createObjectURL(event.target.files[0]);
	output.onload = function () {
		URL.revokeObjectURL(output.src); // free memory		
		if (clickChange.value == true) {
			saveUserAvatar();
		}
	};
};
const delAvatar = () => {
	files = [];
	isDisplayAvt.value = false;
	var output = document.getElementById("userAvt");
	output.src = basedomainURL + "/Portals/Image/noimg.jpg";
	
	let formData = new FormData();
	formData.append("user_id_change_avt", store.getters.user.user_id);
	axios({
		method: "post",
		url: baseUrlCheck + `/api/Account/Delete_Avatar`,
		data: formData,
		headers: {
			Authorization: `Bearer ${store.getters.token}`,
		},
	})
    .then((response) => {
		if (response.data.err != "1") {
			store.getters.user.avatar = null;
		} else {
			//console.log(response.data.ms);
		}
    })
    .catch((error) => {
		swal.close();
		swal.fire({
			title: "Thông báo!",
			text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
			icon: "error",
			confirmButtonText: "OK",
		});
    });
};
const clickChange = ref(false);
const chooseImage = (id) => {
	clickChange.value = true;
  	document.getElementById(id).click();
};
const saveUserAvatar = () => {
	let formData = new FormData();
	for (var i = 0; i < files.length; i++) {
		let file = files[i];
		formData.append("avatarGroup", file);
	}
	formData.append("user_id_change_avt", store.getters.user.user_id);
	
	axios({
		method: "post",
		url: baseUrlCheck + `/api/Account/Update_Avatar`,
		data: formData,
		headers: {
			Authorization: `Bearer ${store.getters.token}`,
		},
	})
    .then((response) => {
		if (response.data.err != "1") {
			if (response.data.urlAvatar != null) {
				files = [];
				clickChange.value = false;
				store.getters.user.avatar = response.data.urlAvatar;
			}
		} else {
			//console.log(response.data.ms);
		}
    })
    .catch((error) => {
		swal.close();
		swal.fire({
			title: "Thông báo!",
			text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
			icon: "error",
			confirmButtonText: "OK",
		});
    });
};

const showSetting = ref(0);
const toggleSetting = () => {
	axios
		.post(
		basedomainURL + "/api/Proc/CallProc",
		{
			proc: "sys_users_get",
			par: [
				{ par: "user_id", va: store.getters.user.user_id },
			],
		},
		config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data);
			if (data[0].length > 0) {
				infoAccount.value = data[0][0];
				if (infoAccount.value.birthday != null) {					
					infoAccount.value.birthday = new Date(infoAccount.value.birthday);
				}
				if (infoAccount.value.gender == 0) {
					infoAccount.value.gender_woman = true;
					infoAccount.value.gender_man = false;
				}
				else if (infoAccount.value.gender == 1) {
					infoAccount.value.gender_woman = false;
					infoAccount.value.gender_man = true;
				}
				showSetting.value = 1;
			}
			else{
				infoAccount.value = {
					user_id: null
				}
				swal.fire({
					title: "Thông báo",
					text: "Thông tin người dùng không tồn tại!",
					icon: "error",
					confirmButtonText: "OK",
				});
			}
		})
		.catch((error) => {
			console.log(error);
			toast.error("Tải dữ liệu không thành công!");
			console.log(error);
			if (error && error.status === 401) {
				swal.fire({
					
					text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
					
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};
const togglePass = () => {
	showSetting.value = 2;
};
const listHistoryAccess = ref([]);
const option = ref({
	IsNext: true,
	totalRecords: 0,
	loading: false,
	PageNo: 0,
	PageSize: 50,
});
const toggleHistory = (type) => {
	if (type == true) {
		showSetting.value = 3;
	}
	axios
		.post(
		basedomainURL + "/api/Proc/CallProc",
		{
			proc: "sys_access_by_user_list",
			par: [
				{ par: "user_id", va: store.getters.user.user_id },
				{ par: "pageno", va: option.value.PageNo },
				{ par: "pagesize", va: option.value.PageSize },
			],
		},
		config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data);
			if (data[0].length > 0) {
				data[0].forEach((el, idx) => {
					if (el.is_time != null) {
						el.is_time = new Date(el.is_time);
					}
					el.is_order = option.value.PageNo * option.value.PageSize + idx + 1;
				});
			}
			listHistoryAccess.value = data[0];
			if (data.length == 2) {
				option.value.totalRecords = data[1][0].totalRecords;
			}
		})
		.catch((error) => {
			console.log(error);
			toast.error("Tải dữ liệu không thành công!");
			console.log(error);
			if (error && error.status === 401) {
				swal.fire({
					
					text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
					
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};
const handleSubmit = () => {
	let data = {...infoAccount.value};
	axios
		.put(
			baseUrlCheck + "/api/Account/Update_InfoUser",
			data,
			config
		)
		.then((response) => {
			if (response.data.err == "0") {
				toast.success("Cập nhật thông tin tài khoản thành công!");
			}
			else {
				//console.log(response.data.ms);
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
};
const submitChangePass = () => {
	if (infoAccount.value.new_pass.length < 8) {
		swal.fire({
			title: "Thông báo!",
			text: "Vui lòng chọn mật khẩu có ít nhất 8 ký tự!",
			icon: "error",
			confirmButtonText: "OK",
		});
		return;
	}
	if (infoAccount.value.new_pass != infoAccount.value.check_pass) {
		swal.fire({
			title: "Thông báo",
			text: "Mật khẩu mới không trùng nhau!",
			icon: "error",
			confirmButtonText: "OK",
		});
		return false;
	}
	if (infoAccount.value.user_id == null) {
		infoAccount.value.user_id = store.getters.user.user_id;
	}
	let data = { old_pass: encr(infoAccount.value.old_pass, SecretKey, cryoptojs).toString(), 
		new_pass: encr(infoAccount.value.new_pass, SecretKey, cryoptojs).toString(), 
		user_id: infoAccount.value.user_id };
	axios
		.post(
			baseUrlCheck + "/api/Account/Change_PassUser",
			data,
			config
		)
		.then((response) => {
			if (response.data.err == "0") {
				toast.success("Cập nhật mật khẩu thành công!");
				setTimeout(() => {
					axios.get(baseURL + "/api/Login/Logout", {
						headers: { Authorization: `Bearer ${store.getters.token}` },
					});
					store.commit("gologout");
					if (router) router.push({ path: "/login" });
				},1000);				
			}
			else {
				swal.fire({
					title: "Thông báo",
					text: "Mật khẩu không chính xác. Vui lòng kiểm tra lại.",
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
};
const closeDialog = () => {
	showSetting.value = 0;
};
const changeGender = (type) => {
	if (type == 0 && infoAccount.value.gender_woman == true) {
		infoAccount.value.gender = 0;
		infoAccount.value.gender_man = false;
	}
	else if (type == 0 && infoAccount.value.gender_woman == false) {
		infoAccount.value.gender = 1;
		infoAccount.value.gender_man = true;
	}
	else if (type == 1 && infoAccount.value.gender_man == true) {
		infoAccount.value.gender = 1;
		infoAccount.value.gender_woman = false;
	}
	else if (type == 1 && infoAccount.value.gender_man == false) {
		infoAccount.value.gender = 0;
		infoAccount.value.gender_woman = true;
	}
};
//Phân trang dữ liệu
const onPage = (event) => {
	if (event.page == 0) {
		//Trang đầu
		option.value.id = null;
		option.value.IsNext = true;
	} else if (event.page > option.value.PageNo + 1) {
		//Trang cuối
		option.value.id = -1;
		option.value.IsNext = false;
	} else if (event.page > option.value.PageNo) {
		//Trang sau
		option.value.id = listHistoryAccess.value[listHistoryAccess.value.length - 1].web_access_id;
		option.value.IsNext = true;
	} else if (event.page < option.value.PageNo) {
		//Trang trước
		option.value.id = listHistoryAccess.value[0].web_access_id;
		option.value.IsNext = false;
	}
	option.value.PageNo = event.page;
	toggleHistory();
};

onMounted(() => {
	return {
		
	};
});
</script>
<template>
	<div class="surface-100 bg-white" style="max-height: calc(100vh - 50px); overflow-y: auto; margin-right: -5px;">
		<div class="row" style="margin:15px 0 0;">
			<div class="col-12">
				<div class="inputanh relative flex" style="margin: 0 auto">
					<img
						@click="chooseImage('imageUser')"
						id="userAvt"
						v-bind:src="store.getters.user.avatar
							? basedomainURL + store.getters.user.avatar
							: basedomainURL + '/Portals/Image/noimg.jpg'
						"
					/>
					<Button
						v-if="store.getters.user.avatar || isDisplayAvt"
						style="width: 1.5rem; height: 1.5rem"
						icon="pi pi-times"
						@click="delAvatar"
						class="p-button-rounded absolute top-0 right-0 cursor-pointer btn-del-avt"
					/>
					<input
						id="imageUser"
						type="file"
						accept="image/*"
						@change="handleFileAvtUpload"
						style="display: none;"
					/>
				</div>
			</div>
			<div class="col-12">
				<h1 class="acc-txt">
					Xin chào {{store.getters.user.full_name}}
				</h1>
				<div class="acc-txt-bot">
					Quản lý thông tin, quyền riêng tư và các tùy chọn bảo mật tài khoản của bạn trong hệ thống Smart Office
				</div>
			</div>
			<div class="row" v-if="showSetting == 0">
				<div class="col-12 md:col-12 flex" style="padding:0.5rem 20rem;">
					<div class="col-6 box-info-acc">
						<div @click="toggleSetting()" class="left-box">
							<div class="left-content">
								<h2 class="mt-0 mb-3">Thông tin cá nhân</h2>
								<div class="left-txt">
									<div class="text-acc">Xem và điều chỉnh thông tin cá nhân của bạn trong hệ thống khi có thay đổi từ bạn.</div>
								</div>
							</div>
							<div class="right-content">
								<div>
									<font-awesome-icon icon="fa-solid fa-circle-user" />
								</div>
							</div>
						</div>
					</div>
					<div class="col-6 box-info-acc">
						<div @click="togglePass()" class="right-box">
							<div class="left-content">
								<h2 class="mt-0 mb-3">Thay đổi mật khẩu</h2>
								<div class="right-txt">
									<div class="text-acc">Bảo vệ tài khoản cá nhân của bạn trong hệ thống bằng cách thay đổi mật khẩu khi cần thiết.</div>
								</div>
							</div>
							<div class="right-content">
								<div>
									<font-awesome-icon icon="fa-solid fa-shield-halved" />
								</div>
							</div>
						</div>
					</div>
				</div>
				<div class="col-12 md:col-12 flex" style="padding:0.5rem 20rem;">				
					<div class="col-6 box-info-acc pt-0">
						<div @click="toggleHistory(true)" class="left-box">
							<div class="left-content">
								<h2 class="mt-0 mb-3">Lịch sử đăng nhập</h2>
								<div class="left-txt">
									<div class="text-acc">Theo dõi lịch sử truy cập tài khoản của chính mình (thời gian, địa chỉ IP mạng, thiết bị).</div>
								</div>
							</div>
							<div class="right-content">
								<div>
									<font-awesome-icon icon="fa-solid fa-clock-rotate-left" />
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>
			<div class="row" v-if="showSetting == 1">
				<div class="flex justify-content-center">
					<div class="card" style="width:40%;">
						<form @submit.prevent="handleSubmit()" class="p-fluid">
							<div class="col-12 md:col-12 p-0 m-0 flex" style="flex-direction:column;">
								<label class="col-12 p-0 text-left flex mt-3 mb-2">
									Họ tên
								</label>
								<InputText
									v-model="infoAccount.full_name"
									class="col-12"
									disabled
								/>
							</div>
							<div class="col-12 md:col-12 p-0 m-0 flex" style="flex-direction:column;">
								<label class="col-12 p-0 text-left flex mt-3 mb-2">
									Tên truy cập
								</label>
								<InputText
									v-model="infoAccount.user_id"
									class="col-12"
									disabled
								/>
							</div>
							<div class="col-12 md:col-12 p-0 m-0 flex" style="flex-direction:column;">
								<label class="col-12 p-0 text-left flex mt-3 mb-2">
									Điện thoại
								</label>
								<InputText
									v-model="infoAccount.phone"
									class="col-12"
								/>
							</div>
							<div class="col-12 md:col-12 p-0 m-0 flex" style="flex-direction:column;">
								<label class="col-12 p-0 text-left flex mt-3 mb-2">
									Email
								</label>
								<InputText
									v-model="infoAccount.email"
									class="col-12"
								/>
							</div>
							<div class="col-12 md:col-12 p-0 m-0 flex" style="flex-direction:column;">
								<label class="col-12 p-0 text-left flex mt-3 mb-2">
									Ngày sinh
								</label>
								<Calendar class="col-12 p-0" id="date_publish" v-model="infoAccount.birthday" placeholder="dd/mm/yyyy" :manualInput="false" :showIcon="true" />
							</div>
							<div class="col-12 md:col-12 p-0 m-0 mt-3 mb-2 flex">
								<div class="col-6 md:col-6 p-0 flex align-items-center">
									<label style="vertical-align: text-bottom" class="col-2 pl-0 text-left">Nam </label>
									<InputSwitch v-model="infoAccount.gender_man" class="col-10" @change="changeGender(1)" />
								</div>
								<div class="col-6 md:col-6 p-0 flex align-items-center">
									<label style="vertical-align: text-bottom" class="col-2 pl-0 text-left">Nữ </label>
									<InputSwitch v-model="infoAccount.gender_woman" class="col-10" @change="changeGender(0)" />
								</div>
							</div>
							<div class="flex mt-5" style="align-items: center; justify-content: center;">
								<Button label="Quay lại" icon="pi pi-arrow-circle-left" class="p-button-text w-10rem mr-3" style="border: 1px solid #e5e5e5;" @click="closeDialog()" />
								<Button label="Lưu" icon="pi pi-check" class="w-7rem" @click="handleSubmit" />
							</div>
						</form>
					</div>
				</div>
			</div>
			<div class="row" v-if="showSetting == 2">
				<div class="flex justify-content-center">
					<div class="card" style="width:40%;">
						<form @submit.prevent="submitChangePass()" class="p-fluid">
							<div class="col-12 md:col-12 p-0 m-0 flex" style="flex-direction:column;">
								<label class="col-12 p-0 text-left flex mt-3 mb-2">
									Mật khẩu cũ
								</label>
								<Password v-model="infoAccount.old_pass" toggleMask :feedback="false"></Password>
							</div>
							<div class="col-12 md:col-12 p-0 m-0 flex" style="flex-direction:column;">
								<label class="col-12 p-0 text-left flex mt-3 mb-2">
									Mật khẩu mới
								</label>
								<Password v-model="infoAccount.new_pass" toggleMask :feedback="false"></Password>
							</div>
							<div class="col-12 md:col-12 p-0 m-0 flex" style="flex-direction:column;">
								<label class="col-12 p-0 text-left flex mt-3 mb-2">
									Nhập lại mật khẩu
								</label>
								<Password v-model="infoAccount.check_pass" toggleMask :feedback="false"></Password>
							</div>
							<div class="flex mt-5" style="align-items: center; justify-content: center;">
								<Button label="Quay lại" icon="pi pi-arrow-circle-left" class="p-button-text w-10rem mr-3" style="border: 1px solid #e5e5e5;" @click="closeDialog()" />
								<Button label="Lưu" icon="pi pi-check" class="w-7rem" @click="submitChangePass" />
							</div>
						</form>
					</div>
				</div>
			</div>
			<div class="row" style="margin: 10px auto 0; max-width: 70%;" v-if="showSetting == 3">
				<DataTable
					class="w-full p-datatable-sm e-sm cursor-pointer table-access"
					:value="listHistoryAccess"
					:paginator="true"
					:rows="option.PageSize"
					:rowsPerPageOptions="[ 20, 50, 100, 200]"
					:scrollable="true"
					scrollHeight="flex"
					:loading="option.loading"
					:lazy="true"
					@page="onPage($event)"
					:totalRecords="option.totalRecords"
					dataKey="web_acess_id"
					:rowHover="true"
					filterDisplay="menu"
					:showGridlines="true"
					filterMode="lenient"
					paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
					responsiveLayout="scroll"
					style="max-height: calc(100vh - 285px); overflow-y: auto;"
				>
					<Column
						field="is_order"
						header="STT"
						class="align-items-center justify-content-center text-center"
						headerStyle="text-align:center;max-width:7rem"
						bodyStyle="text-align:center;max-width:7rem"
					></Column>
					<Column
						field="is_time"
						dataType="date"
						header="Ngày truy cập"
						class="align-items-center justify-content-center text-center"
						headerStyle="text-align:center;"
						bodyStyle="text-align:center;"
					>
						<template #body="{ data }">
						{{ data.is_time ? moment(new Date(data.is_time)).format("HH:mm:SS DD/MM/YYYY") : '' }}
						</template>
					</Column>
					<Column
						field="from_ip"
						header="IP"
						class="align-items-center justify-content-center text-center"
						headerStyle="text-align:center;"
						bodyStyle="text-align:center;"
					>
						<template #body="{ data }">
						{{ data.from_ip }}
						</template>
					</Column>
					<Column
						field="from_device"
						header="Thiết bị"
						class="align-items-center justify-content-center text-center"
						headerStyle="text-align:center;"
						bodyStyle="text-align:center;"
					>
						<template #body="{ data }">
							<span>{{ data.from_device }}</span>
						</template>
					</Column>
					<template #paginatorstart>
						<Button type="button" label="Quay lại" icon="pi pi-arrow-circle-left" class="p-button-text" style="border: 1px solid #e5e5e5;" @click="closeDialog()" />
					</template>
					<template #paginatorend>
						
					</template>
					<template #empty>
						<div
							class="
								align-items-center
								justify-content-center
								p-4
								text-center
								w-full
							"
							v-if="!isFirst"
						>
							<img src="../../assets/background/nodata.png" height="144" />
							<h3 class="m-1">Không có dữ liệu</h3>
						</div>
					</template>
				</DataTable>
			</div>
		</div>
	</div>
</template>
<style scoped>
	.acc-txt {
		font-size: 1.75rem;
		letter-spacing: 0;
		line-height: 2.25rem;
		hyphens: auto;
		word-break: break-word;
		word-wrap: break-word;
		color: #202124;
		text-align: center;
		margin: 1rem 0;
	}
	.acc-txt-bot {
		font-size: 1.25rem;
		font-weight: 400;
		line-height: 1.5rem;
		hyphens: auto;
		word-break: break-word;
		word-wrap: break-word;
		color: #7c7c7c;
		margin-top: 1rem;
		text-align: center;
	}
	.box-info-acc {
		display: flex;
		padding: 1.5rem;
	}
	.inputanh {
		width: 100px;
		height: 100px;
		cursor: pointer;
		border-radius: 50%;
    	border: 1px solid #ced4da;
	}
	.inputanh .btn-del-avt {
		display: none;
	}
	.inputanh:hover .btn-del-avt {
		display: inline-flex;
	}
	.inputanh img {
		object-fit: cover;
		width: 100%;
		height: 100%;
		border-radius: 50%;
    	border: 3px solid #fff;
	}
	.left-box {
		display: flex;
		border: 1px solid #ccc;
		border-radius: 10px;
		padding: 1.5rem;
	}
	.left-box:hover {
		cursor: pointer;
		background-color: aliceblue;
	}
	.left-content {
		float: left;
		width: calc(100% - 5rem);
		margin-right: 2rem;
	}
	.right-box {
		display: flex;
		border: 1px solid #ccc;
		border-radius: 10px;
		padding: 1.5rem;
	}
	.right-box:hover {
		cursor: pointer;
		background-color: aliceblue;
	}
	.right-content {
		float: right;
		width: 5rem;
		display: flex;
		align-items: center;
		justify-content: center;
	}
	.right-content svg {
		font-size: 5rem;
		color: #4285f4;
	}
	.text-acc {
		font-size: 1.125rem;
		color: #7c7c7c;
	}
</style>
<style lang="scss" scoped>
	::v-deep(.table-access) {
		.p-paginator-bottom {
			border-top: 1px solid #e9ecef;
		}
	}
</style>