<script setup>
import { ref, inject, onMounted } from "vue";
import { change_unsigned } from "../../util/function";
import { useToast } from "vue-toastification";

const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;

const option = ref({
	loading: false,
});

const props = defineProps({
	titleModal: String,
	showAddData: Boolean,
	dataTivi: Object,
	listDataInScreen: Object,
	listDataFromData: Object,
	closeDialog: Function,
	reloadListScreenTivi: Function,
});
const dataChoose = ref([]);
const changeCheckData = (event, data) => {
	let indexExist = -1;
	let choosenData = -1;
	indexExist = dataChoose.value.findIndex(x => x.screen_id == data.screen_id);
	choosenData = props.listDataInScreen.findIndex(x => x.screen_id == data.screen_id);
	if (data.checked) {
		if (indexExist == -1 && choosenData == -1) {
			dataChoose.value.push(data);
		}
	}
	else {
		if (indexExist >= 0) {
			dataChoose.value.splice(indexExist, 1);
		}
	}
};
const searchScreen = ref("");
const enterSearch = ref("");
const searchScreenTV = () => {
	if (searchScreen.value != null && searchScreen.value.trim() != "") {
		enterSearch.value = searchScreen.value.trim();
	}
	else {
		enterSearch.value = "";
	}
};
const filterScreenSearch = () => {
	if (enterSearch.value != null && enterSearch.value != "") {
		let keySearch = change_unsigned(enterSearch.value);
		return props.listDataFromData.filter(x => change_unsigned(x.screen_name || "").includes(keySearch));
	}
	else {
		return props.listDataFromData;
	}
};
const isuploading = ref(false);
const addScreenToTivi = () => {
	if (isuploading.value) {
		return;
	}
	
	let formData = new FormData();
	if (dataChoose.value.length == 0) {
		swal.fire({
			title: "Thông báo!",
			text: "Chọn màn hình muốn sao chép trước khi lưu!",
			icon: "warning",
			confirmButtonText: "OK",
		});
		return false;
	}
	formData.append("modelTivi", JSON.stringify(props.dataTivi));
	formData.append("listScreen", JSON.stringify(dataChoose.value));
	formData.append("listScreenTivi", JSON.stringify(props.listDataInScreen));
	isuploading.value = true;
	swal.fire({
		width: 110,
		didOpen: () => {
		swal.showLoading();
		},
	});
	axios({
		method: 'post',
		url: baseUrlCheck +
			`/api/Tivi/CopyScreenToTivi`,
		data: formData,
		headers: {
			Authorization: `Bearer ${store.getters.token}`,
		},
	})
	.then((response) => {
		swal.close();
		isuploading.value = false;
		if (response.data.err == "2") {
			swal.fire({
				title: "Thông báo",
				text: response.data.ms,
				icon: "warning",
				confirmButtonText: "OK",
			});
		}
		else if (response.data.err != "1") {
			toast.success("Copy màn hình thành công!");
			props.closeDialog();
			props.reloadListScreenTivi();

		} else {
			swal.fire({
				title: "Thông báo",
				text: "Xảy ra lỗi khi upload hình ảnh.",
				icon: "error",
				confirmButtonText: "OK",
			});
		}
	})
	.catch((error) => {
		swal.close();
		isuploading.value = false;
		swal.fire({
			title: "Thông báo",
			text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
			icon: "error",
			confirmButtonText: "OK",
		});
	});
};

const resetChecked = () => {
	dataChoose.value = [];
	searchScreen.value = "";
	enterSearch.value = "";
};
onMounted(() => {
	resetChecked();
	return {};
});
</script>
<template>
	<Dialog class="modal-tivi-config"
		v-model:visible="props.showAddData"
		:autoZIndex="true"
		:baseZIndex="1001"
		:modal="true"
		:position="'top'"
		style="width: 35vw"
	>
		<template #header>			
			<div class="flex justify-content-center align-items-center w-full" style="justify-content: space-between !important;">
				<span class="font-bold" style="font-size: 1.25rem;">{{ props.titleModal }}</span>
			</div>
		</template>
		<div class="grid formgrid m-2">
			<DataTable
				class="w-full p-datatable-sm e-sm table-screen-db"
				:value="filterScreenSearch()"
				dataKey="screen_id"
				:showGridlines="true"
				:rowHover="true"
				currentPageReportTemplate=""
				responsiveLayout="scroll"
				:scrollable="true"
				scrollHeight="flex"
				rowGroupMode="subheader"
				groupRowsBy="tivi_id"
				:lazy="true"
				:loading="option.loading"
				:paginator="false"
				v-model:first="first"
			>
				<template #header>
					<Toolbar class="w-full custoolbar toolbar-screen-db">
						<template #start>
							<span class="p-input-icon-left" style="width: 100%">
								<i class="pi pi-search" />
								<InputText
									v-model="searchScreen"
									@keydown.enter.exact.prevent="searchScreenTV()"
									type="text"
									spellcheck="false"
									placeholder=" Tìm kiếm màn hình"
									style="width: 100%"
								/>
							</span>
						</template>
					</Toolbar>
				</template>
				<template #groupheader="slotProps">
					<div class="py-1" style="font-size:1.1rem;">
						<i class="pi pi-desktop mr-2"></i>
						{{ slotProps.data.tivi_name }}
					</div>
				</template>
				<Column
					class="align-items-center justify-content-center text-center"
					headerStyle="text-align:center;max-width:70px;height:40px;background-color:#f5f5f5;"
					bodyStyle="text-align:center;max-width:70px"
					field=""
					header=""
				>
					<template #body="slotProps">
						<Checkbox class="" :binary="true" @change="changeCheckData($event, slotProps.data)" v-model="slotProps.data.checked" />
					</template>
				</Column>
				<Column
					headerStyle="text-align:center;height:40px;font-size:1.1rem;background-color:#f5f5f5;"
					bodyStyle="text-align:left;"
					field="screen_name"
					header="Tên màn hình"
				>
					<template #body="slotProps">
						<div class="flex" style="flex-direction:column;">
							<span class="font-bold text-2line mb-2" style="font-size: 1.1rem;">
								{{ slotProps.data.screen_name }}
							</span>
							<span class="font-normal text-2line" style="font-size: 0.85rem;">
								{{ '(' + slotProps.data.content_display.substring(2) + ')' }}
							</span>
						</div>
					</template>
				</Column>
				<template #empty>
					<div class="align-items-center justify-content-center p-4 text-center m-auto"
						v-if="props.listDataFromData.length == 0"
					>
						<img src="../../assets/background/nodata.png" height="144" />
						<h3 class="m-1">Không có dữ liệu</h3>
					</div>
				</template>
			</DataTable>
		</div>
		<template #footer>
			<Button
				label="Hủy"
				icon="pi pi-times"
				@click="props.closeDialog()"
				class="p-button-text mr-2"
			/>
			<Button
				label="Lưu"
				icon="pi pi-check"
				@click="addScreenToTivi()"
			/>
		</template>
	</Dialog>
</template>
<style scoped>
	.text-2line {
		text-overflow: ellipsis;
		overflow: hidden;
		column-gap: initial;
		-webkit-line-clamp: 2;
		display: -webkit-box;
		-webkit-box-orient: vertical;
	}
</style>
<style lang="scss" scoped>
	::v-deep(.toolbar-screen-db) {
		.p-toolbar-group-left {
			width: 100%;
		}
	}
	::v-deep(.table-screen-db) {
		.p-rowgroup-header td {
			background-color: #2196f3;
    		color: #ffffff;
		}
		tbody.p-datatable-tbody {
			max-height: calc(100vh - 320px);
		}
	}
</style>
