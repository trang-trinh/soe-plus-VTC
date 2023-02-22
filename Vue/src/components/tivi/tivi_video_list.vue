<script setup>
import { ref, inject, onMounted } from "vue";
//import { useToast } from "vue-toastification";
import { change_unsigned } from "../../util/function";
//const cryoptojs = inject("cryptojs");
//const axios = inject("axios");
const store = inject("store");
//const swal = inject("$swal");
//const toast = useToast();
const basedomainURL = baseURL;
//const baseUrlCheck = baseURL;
// const config = {
// 	headers: {
// 		Authorization: `Bearer ${store.getters.token}`,
// 	},
// };
const props = defineProps({
	typeList: Number,
	titleModal: String,
	showAddData: Boolean,
	listDataInScreen: Object,
	listDataFromData: Object,
	options: Object,
	closeDialog: Function,
	onPage: Function,
	addDataToScreen: Function,
});
const dataChoose = ref([]);
const changeCheckData = (event, data) => {
	let indexExist = -1;
	let choosenData = -1;
	if (props.typeList == 0) {
		indexExist = dataChoose.value.findIndex(x => x.video_id == data.video_id);
		choosenData = props.listDataInScreen.findIndex(x => x.video_id == data.video_id);
	}
	else {
		indexExist = dataChoose.value.findIndex(x => x.shows_id == data.shows_id);
		choosenData = props.listDataInScreen.findIndex(x => x.shows_id == data.shows_id);
	}
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
const searchFile = ref("");
const enterSearch = ref("");
const searchFileTV = () => {
	if (searchFile.value != null && searchFile.value.trim() != "") {
		enterSearch.value = searchFile.value.trim();
	}
	else {
		enterSearch.value = "";
	}
};
const listFileTV_Filter = () => {
	if (enterSearch.value != null && enterSearch.value != "") {
		let keySearch = change_unsigned(enterSearch.value);
		return props.listDataFromData.filter(x => change_unsigned(x.title || "").includes(keySearch));
	}
	else {
		return props.listDataFromData;
	}
};
const resetChecked = () => {
	dataChoose.value = [];
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
		:baseZIndex="1000"
		:modal="true"
		:position="'top'"
		style="width: 80%"
	>
		<template #header>			
			<div class="flex justify-content-center align-items-center w-full" style="justify-content: space-between !important;">
				<span class="font-bold" style="font-size: 1.25rem;">{{ props.titleModal }}</span>
				<span class="p-input-icon-left w-30rem">
					<i class="pi pi-search" />
					<InputText class="w-full" style="border-radius:5px;" 
						v-model="searchFile" @keydown.enter.exact.prevent="searchFileTV()" placeholder="Tìm kiếm ..." />
				</span>
			</div>
		</template>
		<div class="grid formgrid m-2">
			<DataView
				class="w-full h-full e-sm modal-add-tv"
				:lazy="true"
				:value="listFileTV_Filter()"
				layout="grid"
				:loading="props.options.loading"
				:paginator="false"
				:rows="props.options.pagesize"
				:totalRecords="props.options.totalRecords"
				:pageLinkSize="props.options.pagesize"
				@page="props.onPage($event)"
				paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
				:rowsPerPageOptions="[30, 60, 120]"
				currentPageReportTemplate=""
				responsiveLayout="scroll"
				:scrollable="false"
			>
				<template #grid="slotProps">
					<div style="padding: 0.5rem"
						class="col-2 md:col-2 item-video"
					>
						<Card class="no-paddcontent cursor-pointer">
							<template #title>
								<div class="product-grid-item-bottom flex" style="position:absolute;z-index:1;">
									<Checkbox class="" :binary="true" @change="changeCheckData($event, slotProps.data)" v-model="slotProps.data.checked" />
								</div>
								<div style="position: relative">
									<Image
										v-bind:src="
											slotProps.data.is_file_upload
												? (slotProps.data.image
													? basedomainURL + slotProps.data.image
														: basedomainURL + '/Portals/Image/noimg.jpg')
												: slotProps.data.image
													? basedomainURL + slotProps.data.image
														: slotProps.data.thumbnail
										"
									/>
									<span v-if="slotProps.data.video_duration" class="video-duration">
										{{ slotProps.data.video_duration }}
									</span>
								</div>
							</template>
							<template #content>
								<div class="flex col-12 p-0">
									<div class="px-0" style="flex: 2">
										<Avatar
											v-bind:label="slotProps.data.avatar? '' : slotProps.data.last_name.substring(0, 1)"
											v-bind:image="basedomainURL + slotProps.data.avatar"
											style="background-color: #2196f3; color: #ffffff;"
											class="mr-2"
											size="large"
											shape="circle"
										/>
									</div>
									<div class="item-video" style="flex: 11">
										<div>
											<span class="font-bold text-2line" style="font-size: 0.85rem;">
												{{ slotProps.data.title }}
											</span>
										</div>
										<div>
											<span class="font-normal" style="font-size: 0.85rem;">
												{{ slotProps.data.created_name }}
											</span>
										</div>
									</div>
								</div>
							</template>
						</Card>
					</div>
				</template>
				<!-- <template #paginatorstart></template>
				<template #paginatorend>
					<Button
						label="Hủy"
						icon="pi pi-times"
						@click="props.closeDialog()"
						class="p-button-text mr-2"
					/>
					<Button
						label="Chọn"
						icon="pi pi-check"
						@click="props.addDataToScreen(dataChoose, props.typeList)"
					/>
				</template> -->
				<template #empty>
					<div class="align-items-center justify-content-center p-4 text-center m-auto"
						v-if="props.listDataFromData.length == 0"
					>
						<img src="../../assets/background/nodata.png" height="144" />
						<h3 class="m-1">Không có dữ liệu</h3>
					</div>
				</template>
			</DataView>
		</div>
		<template #footer>
			<Button
				label="Hủy"
				icon="pi pi-times"
				@click="props.closeDialog()"
				class="p-button-text mr-2"
			/>
			<Button
				label="Chọn"
				icon="pi pi-check"
				@click="props.addDataToScreen(dataChoose, props.typeList)"
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
	.video-duration {
		position: absolute;
		background: rgba(0, 0, 0, 0.4);
		font-size: 1rem;
		color: #fff;
		border-radius: 15%;
		padding: 1 3px;
		bottom: 8px;
		right: 5px;
		z-index: 99;
	}
</style>
<style lang="scss" scoped>
	::v-deep(.modal-add-tv) {
		.p-card {
			box-shadow: none !important;
			background: #f9f9f9;
		}
		.p-card-title img {
			width: 100%;
			height: 6.4vw;
		}
		.p-card .p-card-title {
			margin-bottom: 0;
			position: relative;
		}
		.p-card .p-card-body {
			padding: 0.5rem;
		}
	}
	
</style>
