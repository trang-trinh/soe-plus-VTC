<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";
const cryoptojs = inject("cryptojs");
const toast = useToast();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const emitter = inject("emitter");
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const bgColor = ref([
    "#F8E69A", "#AFDFCF", "#F4B2A3", "#9A97EC", "#CAE2B0", "#8BCFFB", "#CCADD7"
]);
const props = defineProps({
	isShow: Boolean,
	listLog: Object,
	infoTivi: Object,
	key: Number
});
const showContentLog = ref(false);
const contentLog = ref("");
const openContentLog = (dataLog) => {
	showContentLog.value = true;
	contentLog.value = dataLog.content;
};
const closeDialogLog = () => {
	showContentLog.value = false;
};
const showSidebar = ref(false);
const closeSideBar = () => {
	showSidebar.value = false;
	//emitter.emit("sidebar_tivilog", false);
	emitter.emit("emitDataLog", {
		type: "sidebar_tivilog",
		data: false
	});
};
const isDelLog = ref(false);
const deleteContentLog = (dataLog, isDelMulti) => {
	if (isDelLog.value) {
		return;
	}
	swal
		.fire({
			title: "Thông báo",
			text: "Bạn có muốn xoá " + (isDelMulti == true ? "tất cả " : "") + "log không?",
			icon: "warning",
			showCancelButton: true,
			confirmButtonColor: "#3085d6",
			cancelButtonColor: "#d33",
			confirmButtonText: "Có",
			cancelButtonText: "Không",
		})
		.then((result) => {
		if (result.isConfirmed) {
			var dataDel = [];
			if (isDelMulti) {
				props.listLog.forEach((e) => {
					if (e.log_id != null) {
						dataDel.push(e.log_id);
					}
				});
			}
			else {
				if (dataLog != null) {
					dataDel.push(dataLog.log_id);
				}
			}
			isDelLog.value = true;
			swal.fire({
				width: 110,
				didOpen: () => {
					swal.showLoading();
				},
			});

			axios
				.delete(baseUrlCheck + "/api/Tivi/Delete_Log_Tivi", {
					headers: { Authorization: `Bearer ${store.getters.token}` },
					data: dataDel,
			})
			.then((response) => {
				swal.close();
				isDelLog.value = false;
				if (response.data.err != "1") {
					toast.success("Xoá log thành công!");
					if (isDelMulti) {
						closeSideBar();
					} else {
						emitter.emit("emitDataLog", {
							type: "reload_list_log",
							data:  null
						});
					}					
				} else {
					swal.fire({
						title: "Thông báo",
						text: "Xảy ra lỗi khi xóa log",
						icon: "error",
						confirmButtonText: "OK",
					});
				}
			})
			.catch((error) => {
				swal.close();
				isDelLog.value = false;
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
const exportExcelLog = () => {
	swal.fire({
		width: 110,
		didOpen: () => {
			swal.showLoading();
		},
	});
	axios
		.post(
			baseUrlCheck + "/api/Excel/ExportExcel",
			{
				excelname: "DANH SÁCH LOG TIVI " + (props.infoTivi != null ? props.infoTivi.tivi_name : ""),
				proc: "tivi_log_listexport",
				par: [
					{ par: "tivi_id", va: props.listLog[0].tivi_id },
					{ par: "topdata", va: 200 }
				],
			},
			config
		)
		.then((response) => {
			swal.close();
			if (response.data.err != "1") {
				swal.close();
				toast.success("Kết xuất Data thành công!");
				window.open(baseUrlCheck + response.data.path);
			} else {
				swal.fire({
					title: "Thông báo",
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
onMounted(() => {
	showSidebar.value = props.isShow;
});
</script>
<template>
	<Sidebar
		v-model:visible="showSidebar"
		:baseZIndex="100"
		:autoZIndex="true"
		position="right"
    	:showCloseIcon="false"
		style="width:60%;z-index:100;"
	>
		<div class="flex p-2" style="align-items:center;">
			<div class="p-0 m-0 flex btn-close-log">
				<Button
					icon="pi pi-times"
					class="p-button-rounded p-button-info p-button-outlined"
					type="button"
					v-tooltip.bottom="'Đóng'"
					@click="closeSideBar()"
				/>
			</div>
			<div class="w-full px-0 py-2 m-0 ml-2 flex" style="align-items: center;justify-content: space-between;">
				<span class="font-bold" style="font-size: 1.2rem;">Danh sách Log</span>
				<div>
					<Button
						@click="deleteContentLog(null, true)"
						class="p-button-danger m-0"
						type="button"
						icon="pi pi-trash"
						label="Xóa log"
					/>
					<Button
						@click="exportExcelLog()"
						class="p-button-primary m-0 ml-2"
						type="button"
						icon="pi pi-file-excel"
						label="Xuất excel"
						v-if="props.listLog.length > 0"
					/>
				</div>
			</div>
		</div>
		<div style="max-height: calc(100vh - 75px); overflow-y: auto;">
			<DataTable class="table-list-log" 
				:value="props.listLog" 
      			:scrollable="true"
				:rowHover="true"
      			scrollHeight="flex"
				responsiveLayout="scroll">
				<Column field="action" header="Action" 
					headerStyle="padding:0.5rem;height:40px;border-right:1px solid #ccc;"
					bodyStyle="padding:0.5rem;border-right:1px solid #ccc;">
				</Column>
				<Column field="method" header="Method" 
					headerStyle="padding:0.5rem;height:40px;width:120px;border-right:1px solid #ccc;"
					bodyStyle="padding:0.5rem;width:100px;border-right:1px solid #ccc;">
				</Column>
				<Column field="full_name" header="Người tạo"
					headerStyle="padding:0.5rem;height:40px;border-right:1px solid #ccc;"
					bodyStyle="padding:0.5rem;border-right:1px solid #ccc;">
					<template #body="slotProps">
						<div class="flex" style="align-items:center;">							
							<div>
								<img class="ava"
									v-bind:src="slotProps.data.avatar
												? basedomainURL + slotProps.data.avatar
												: basedomainURL + '/Portals/Image/nouser1.jpg'
											"
									@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
									v-if="slotProps.data.avatar"
									style="border:1px solid #ccc;width:2.5rem;height:2.5rem;"
								/>
								<Avatar v-if="slotProps.data.avatar == null && slotProps.data.last_name != null"
									class="avt-replace"
									size="large"
									shape="circle"
									v-bind:label="(slotProps.data.last_name ?? '').substring(0, 1)"
									style="cursor: pointer;width:2.5rem;height:2.5rem;border:1px solid #ccc;"
									:style="{ background: bgColor[slotProps.data.is_order % 7] + '!important'}"
								/>
							</div>
							<span class="image-text ml-2">{{slotProps.data.full_name}}</span>
						</div>
					</template>
				</Column>
				<Column field="created_date" header="Ngày tạo" 
					headerStyle="padding:0.5rem;height:40px;width:100px;border-right:1px solid #ccc;"
					bodyStyle="padding:0.5rem;width:100px;border-right:1px solid #ccc;">
					<template #body="slotProps">
						<span>{{ slotProps.data.created_date ? moment(new Date(slotProps.data.created_date)).format("HH:mm:ss DD/MM/YYYY") : '' }}</span>					
					</template>
				</Column>
				<Column field="created_ip" header="IP" 
					headerStyle="padding:0.5rem;height:40px;width:140px;border-right:1px solid #ccc;"
					bodyStyle="padding:0.5rem;width:140px;border-right:1px solid #ccc;">
				</Column>
				<Column 
					headerStyle="padding:0.5rem;height:40px;width:60px;border-right:1px solid #ccc;"
					bodyStyle="padding:0.5rem;width:60px;border-right:1px solid #ccc;">
					<template #body="slotProps">
						<div class="flex" style="justify-content:center;align-items:center;">
							<Button
								@click="openContentLog(slotProps.data)"
								class="p-button-rounded p-button-secondary p-button-outlined mx-1"
								type="button"
								icon="pi pi-eye"
								v-tooltip.top="'Chi tiết'"
							></Button>
							<Button
								@click="deleteContentLog(slotProps.data)"
								class="p-button-rounded p-button-danger p-button-outlined mx-1"
								type="button"
								icon="pi pi-trash"
								v-tooltip.top="'Xóa'"
							></Button>
						</div>
					</template>
				</Column>
				<template #empty>
					<div class="text-center font-bold">Không có dữ liệu</div>
				</template>
			</DataTable>
		</div>
	</Sidebar>

	<!-- Modal content log -->
	<Dialog
		:header="'Thông tin'"
		v-model:visible="showContentLog"
		:autoZIndex="true"
		:modal="true"
		style="z-index:1000;width:40vw;"
	>
		<form>
			<div class="grid formgrid m-2">
				<div class="col-12 md:col-12 mb-2" style="word-break: break-word;">
					{{contentLog}}
				</div>
			</div>
		</form>
		<template #footer>
			<Button
				label="Hủy"
				icon="pi pi-times"
				@click="closeDialogLog()"
				class="p-button-text"
			/>
		</template>
	</Dialog>
</template>
<style scoped>
.ava {
	border-radius: 50%;
}
</style>
<style lang="scss" scoped>
	::v-deep(.table-list-log) {
		table.p-datatable-table {
			border-top: 1px solid #ccc;
			border-left: 1px solid #ccc;
		} 
		.p-datatable-emptymessage {
			border-right: 1px solid #ccc;
		}
	}
	::v-deep(.btn-close-log) {
		.p-button {
			width: 2rem;
			height: 2rem;
		}
	}
</style>