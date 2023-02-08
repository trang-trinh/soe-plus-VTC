<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { VuemojiPicker } from "vuemoji-picker";
import moment from "moment";
import { encr } from "../../util/function.js";
import { Chart as ChartJS } from 'chart.js'
import ChartDataLabels from "chartjs-plugin-datalabels"
ChartJS.register(ChartDataLabels)
const cryoptojs = inject("cryptojs");
//Khai báo
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` }, 
};
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const toast = useToast();
const formatByte = ((bytes, precision) => {
	if (isNaN(parseFloat(bytes)) || !isFinite(bytes)) return '-';
	if (typeof precision === 'undefined') precision = 1;
	let units = ['B', 'KB', 'MB', 'GB', 'TB', 'PB'];
	if (typeof bytes === 'string' || bytes instanceof String){
		bytes = parseFloat(bytes);
	}
	let	number = Math.floor(Math.log(bytes) / Math.log(1024));
	return (bytes / Math.pow(1024, Math.floor(number))).toFixed(precision) + ' ' + units[number];
});
const listOrganiztion = ref([]);
const totalUsed = ref();
const listDataOrganization = () => {
	if (store.getters.user.is_super) {
		axios
			.post(
				baseUrlCheck + "api/law_comment_emotes/GetDataProc",
				{ 
					str: encr(JSON.stringify({
							//proc: "sys_organization_data_using",
							proc: "sys_data_using_all_org",
							par: [
								{ par: "user_id", va: store.getters.user.user_id },
								{ par: "organization_id", va: store.getters.user.organization_id },
							],
						}), SecretKey, cryoptojs
					).toString()
				},
				config
			)
			.then((response) => {
				let data = JSON.parse(response.data.data);
				if (data.length > 0) {
					totalUsed.value = { data_used: 0, data_free: 0, data_organization: 0 };
					data[0].forEach((el, idx) => {
						el.is_order = idx + 1;
						totalUsed.value.data_used += el.data_used;
						totalUsed.value.data_free += el.data_free;
						totalUsed.value.data_organization += el.data_organization;
					});
					listOrganiztion.value = data[0];
				}
			})
			.catch((error) => {
				console.log(error);
				toast.error("Tải dữ liệu không thành công!");

				if (error && error.status === 401) {
					swal.fire({
					title: "Thông báo",
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
					icon: "error",
					confirmButtonText: "OK",
					});
					store.commit("gologout");
				}
			});
	}
	else {
		onRowSelect(null, store.getters.user.organization_id);
	}
};
const selectedOrg = ref();
const selectedOrganization = ref();
const detailDataUsing = ref();
const listOrgData = ref([]);
const listOrgSub = ref([]);
const listOrgSubAll = ref([]);
const onRowSelect = (event, org_id) => {
	selectedOrg.value = event != null && event.data != null && event.data.organization_id != null ? event.data.organization_id : org_id;
	//Get_DetailUsing();
	axios
		.post(
			baseUrlCheck + "api/law_comment_emotes/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						//proc: "sys_org_sub_data_using",
						proc: "sys_data_using_by_org",
						par: [
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "organization_id", va: selectedOrg.value },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data);
			if (data.length > 0) {
				var dataOrgSum = { data_used: 0, data_free: 0, data_organization: 0 };
				data[0].forEach((el, idx) => {
					el.is_order = idx + 1;
					dataOrgSum.data_used += el.data_used;
					dataOrgSum.data_free += el.data_free;
					dataOrgSum.data_organization += el.data_organization;
				});
				data[0].forEach((el, idx) => {
					el.dataFreeOrgAll = dataOrgSum.data_free;
				});
				listOrgData.value = data[0];
				detailDataUsing.value = { data_used: dataOrgSum.data_used, data_free: dataOrgSum.data_free, data_organization: dataOrgSum.data_organization };
				chartData.value.datasets[0].data = [detailDataUsing.value.data_used, detailDataUsing.value.data_free];
				listOrgSub.value = data[1];
				listOrgSubAll.value = data[2];
				var dataAll = listOrgSubAll.value[0];
				basicData.value.datasets[0].data = [dataAll.task, dataAll.device, dataAll.doc, dataAll.calendar, dataAll.law, dataAll.chat, dataAll.video, dataAll.file];
			}
		})
		.catch((error) => {
			console.log(error);
			//toast.error("Tải dữ liệu không thành công!");
			if (error && error.status === 401) {
				swal.fire({
				title: "Thông báo",
				text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
				icon: "error",
				confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};
const Get_DetailUsing = () => {
	swal.showLoading();
	axios
		.post(
			baseUrlCheck + "api/law_comment_emotes/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "sys_data_using_by_org",
						par: [
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "organization_id", va: selectedOrg.value || -1 },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			swal.close();
			let data = JSON.parse(response.data.data);
			if (data.length > 0) {
				
			}
		})
		.catch((error) => {
			console.log(error);
			swal.close();
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
const BackListOrg = () => {
	selectedOrg.value = null;
	selectedFilterOrg.value = null;
};

const chartData = ref({
	labels: ['Đã dùng','Còn trống'],
	datasets: [
		{
			data: [detailDataUsing.value != null ? detailDataUsing.value.data_used : 10, detailDataUsing.value != null ? detailDataUsing.value.data_free : 10],
			backgroundColor: ["#e85500","#27ae60"],
			hoverBackgroundColor: ["#e85500","#27ae60"],
		}
	]
});
const lightOptions = ref({
	plugins: {
		legend: {
			labels: { color: '#495057', },
			display: false,
		},
		datalabels: {
			formatter: function (value) {
				return value == 0 ? '' : `${formatByte(value)}`
			},
			color: '#ffffff',
			font: {
				size: "14",
			}
		},
		tooltip: {
			callbacks: {
				label: function(context) {
					let labelContent = context.dataset.data[context.dataIndex];
					return context.label + ': ' + (labelContent == 0 ? '0 KB' : `${formatByte(labelContent)}`)
				}
			},
		}
	},
	cutout: '50%'
});
const selectedStatistic = ref(1);
const selectedFilterOrg = ref();
const typeStatistic = [
	{ code: 1, name: "Công ty" },
	{ code: 2, name: "Biểu đồ" },
];
const DataFilter = () => {
	if (selectedFilterOrg.value != null) {
		//return listOrgSub.value.filter(x => x.listparent_id.includes(selectedFilterOrg.value + "/"));
		return listOrgSub.value.filter(x => x.organization_id == selectedFilterOrg.value);
	}
	return listOrgSub.value;
};
const basicData = ref({
	labels: ['Công việc', 'Thiết bị', 'Văn bản', 'Lịch công tác', 'Luật', 'Trao đổi', 'Truyền thông', 'Kho dữ liệu'],
	datasets: [
		{
			//label: 'Biểu đồ sử dụng dữ liệu',
			backgroundColor: ['#2ecc71','#B0DE09','#3498db','#8e44ad','#f1c40f','#f39c12','#e74c3c','#cc2e50'],
			data: [10, 10, 10, 10, 10, 10, 10, 10],
			categoryPercentage: 0.6,
			maintainAspectRatio: false,
			datalabels: {
				color: '#000000',
			}
		},
	],
});
const horizontalOptions = ref(
	{
		indexAxis: 'y',
		plugins: {
			legend: {
				display: false,
				font: {
					style: "bold",
				},
			},
			datalabels: {
				formatter: function (value) {
					return value == 0 ? '' : `${formatByte(value)}`
				},
				align: "end",
				anchor: "end",
				offset: 0,
				display : "auto",
				font: {
					size: "14",
				},
			},
			tooltip: {
				callbacks: {
                    label: function(context) {
                        let labelContent = context.dataset.data[context.dataIndex];
                        return labelContent == 0 ? '0 KB' : `${formatByte(labelContent)}`
                    }
                },
			}
		},
		scales: {
			x: {
				ticks: {
					color: '#495057',
					beginAtZero: true,
				},
				grid: {
					color: '#ebedef'
				}
			},
			y: {
				ticks: {
					color: '#495057',
					font: {
						weight: 'bold',
					}
				},
				grid: {
					color: '#ebedef'
				}
			},
		}
	}
);
onMounted(() => {
	listDataOrganization();
	return {
		listDataOrganization
	};
});
</script>
<template>
	<div class="main-layout flex-grow-1 p-2" style="background-color: transparent;">
		<DataTable v-if="listOrganiztion.length > 1 && selectedOrg == null"
			:value="listOrganiztion" 
			v-model:selection="selectedOrganization" 
			selectionMode="single" 
			dataKey="organization_id"
			@rowSelect="onRowSelect"
			:scrollable="true"
			responsiveLayout="scroll"
			class="table-data-using"
			style="border-left: 1px solid #e9ecef; border-right: 1px solid #e9ecef;"
		>
			<template #header>
				<div>
					<i class="pi pi-list"></i>
					Thống kê dung lượng hệ thống
				</div>
			</template>
			<Column field="is_order" header="STT"
				headerStyle="text-align:center;max-width:80px;"
				bodyStyle="text-align:center;max-width:80px;"
        		class="align-items-center justify-content-center cell-data px-2"
			></Column>
			<Column field="organization_name" header="Tên công ty"
				class="cell-data px-2 font-bold"
			></Column>
			<Column field="data_used" header="Dung lượng đã sử dụng"
				headerStyle="max-width:200px;background:linear-gradient(60deg, #e74c00, #eb9800); color:#fff;"
				bodyStyle="text-align:center;max-width:200px;"
        		class="align-items-center justify-content-center cell-data px-2"
			>
				<template #body="slotProps">
                    <div class="">
                        {{slotProps.data.data_used != null && slotProps.data.data_used != 0 ? formatByte(slotProps.data.data_used) : (0 + ' KB')}}
                    </div>
                </template>
			</Column>
			<Column field="data_free" header="Dung lượng còn trống"
				headerStyle="max-width:200px;background:linear-gradient(60deg, #27ae60, #58d68d); color:#fff;"
				bodyStyle="text-align:center;max-width:200px;"
        		class="align-items-center justify-content-center cell-data px-2"
			>
				<template #body="slotProps">
                    <div class="">
                        {{slotProps.data.data_free != null && slotProps.data.data_free != 0 ? formatByte(slotProps.data.data_free) : (0 + ' KB')}}
                    </div>
                </template>
			</Column>
			<Column field="data_organization" header="Dung lượng hệ thống cho phép"
				headerStyle="max-width:230px;background:linear-gradient(60deg, #2980b9, #3498db); color: #fff;border-right: 0;"
				bodyStyle="text-align:center;max-width:230px;"
        		class="align-items-center justify-content-center cell-data px-2"
			>
				<template #body="slotProps">
                    <div class="">
                        {{slotProps.data.data_organization != null && slotProps.data.data_organization != 0 ? formatByte(slotProps.data.data_organization) : (0 + ' KB')}}
                    </div>
                </template>
			</Column>
			<ColumnGroup type="footer">
				<Row>
					<Column class="cell-data" footer="Tổng số" :colspan="2" footerStyle="text-align:center;"/>
					<Column class="cell-data" style="text-align:center;max-width:200px;"
						:footer="totalUsed.data_used > 0 ? formatByte(totalUsed.data_used) : '0 KB'" />
					<Column class="cell-data" style="text-align:center;max-width:200px;"
						:footer="totalUsed.data_free > 0 ? formatByte(totalUsed.data_free) : '0 KB'" />
					<Column class="cell-data" style="text-align:center;max-width:230px;"
						:footer="totalUsed.data_organization > 0 ? formatByte(totalUsed.data_organization) : '0 KB'" />
				</Row>
			</ColumnGroup>
		</DataTable>
		<div v-else>
			<div class="flex py-1 mb-1" style="flex-direction: row-reverse;background-color:#f8f9fa;border-radius:5px;border:1px solid #e9ecef;" v-if="listOrganiztion.length > 0 && selectedOrg != null">
				<Button icon="pi pi-arrow-circle-left" label="Trở lại" @click="BackListOrg" class="mr-2"></Button>
			</div>
			<div class="pt-0" style="overflow-y: auto;" :style="listOrganiztion.length > 0 && selectedOrg != null ? 'max-height: calc(100vh - 100px);' : 'max-height: calc(100vh - 60px);'">
				<div class="flex col-12 md:col-12 px-0 pt-0">
					<div class="col-6 md:col-6 pl-0">
						<div style="border:1px solid #e9ecef;">
							<div class="font-bold p-3" style="background-color:#f8f9fa;">
								<i class="pi pi-list"></i>
								Thống kê dung lượng hệ thống
							</div>
							<table style="width:100%;background-color:#fff;border-spacing: inherit;">
								<thead>
									<tr>
										<th class="uppercase text-center p-3" style="background:linear-gradient(60deg, #e74c00, #eb9800); color:#fff; border-right:1px solid;">
											Dung lượng đã sử dụng
										</th>
										<th class="uppercase text-center p-3" style="background:linear-gradient(60deg, #27ae60, #58d68d); color:#fff;">
											Dung lượng còn trống
										</th>
									</tr>
								</thead>
								<tbody>
									<tr>
										<td class="text-center p-3 font-bold" style="border-right: 1px solid #e9ecef; border-bottom: 1px solid #e9ecef;">
											{{detailDataUsing ? (detailDataUsing.data_used ? formatByte(detailDataUsing.data_used)  : (0 + ' KB')) : (0 + ' KB')}}
										</td>
										<td class="text-center p-3 font-bold" style="border-bottom: 1px solid #e9ecef;">
											{{(detailDataUsing ? (detailDataUsing.data_free ? formatByte(detailDataUsing.data_free)  : (0 + ' KB')) : (0 + ' KB'))
											+ ' / ' + (detailDataUsing ? (detailDataUsing.data_organization ? formatByte(detailDataUsing.data_organization)  : (0 + ' KB')) : (0 + ' KB'))}}
										</td>
									</tr>
								</tbody>
							</table>
							<div class="flex justify-content-center mt-3 mb-4">
								<Chart type="doughnut" id="chartDoughnut" :data="chartData" :options="lightOptions" style="width:15rem;" v-if="detailDataUsing != null"/>
								<div class="flex" style="flex-direction: column; margin-left: 5rem">
									<div class="font-bold mb-2">
										<button class="mr-2" style="background-color: #e85500; width: 2rem; height: 1rem; border: 1px solid #ccc;"></button> 
										Đã dùng: {{detailDataUsing ? (detailDataUsing.data_used ? formatByte(detailDataUsing.data_used)  : (0 + ' KB')) : (0 + ' KB')}}
									</div>
									<div class="font-bold">
										<button class="mr-2" style="background-color: #27ae60; width: 2rem; height: 1rem; border: 1px solid #ccc;"></button> 
										Còn trống: {{detailDataUsing ? (detailDataUsing.data_free ? formatByte(detailDataUsing.data_free)  : (0 + ' KB')) : (0 + ' KB')}}
									</div>
								</div>
							</div>
						</div>
					</div>
					<div class="col-6 md:col-6 pr-0">
						<DataTable
							:value="listOrgData"
							selectionMode="single" 
							dataKey="organization_id"
							responsiveLayout="scroll"
							scrollHeight="flex"
							rowGroupMode="rowspan"
              				groupRowsBy="dataFreeOrgAll"
							class="table-data-using-sub"
							style="border-left: 1px solid #e9ecef; border-right: 1px solid #e9ecef;"
						>
							<template #header>
								<div>
									<i class="pi pi-list"></i>
									Thống kê dung lượng theo công ty
								</div>
							</template>
							<Column field="is_order" header="STT"
								headerStyle="text-align:center;max-width:50px;"
								bodyStyle="text-align:center;max-width:50px;"
								class="align-items-center justify-content-center cell-data px-2"
							></Column>
							<Column field="organization_name" header="Tên công ty"
								class="cell-data px-2 font-bold"
							></Column>
							<Column field="data_used" header="Dung lượng đã sử dụng"
								headerStyle="max-width:200px;background:linear-gradient(60deg, #e74c00, #eb9800); color:#fff;"
								bodyStyle="text-align:center;max-width:200px;"
								class="align-items-center justify-content-center cell-data px-2"
							>
								<template #body="slotProps">
									{{slotProps.data.data_used != null && slotProps.data.data_used != 0 ? formatByte(slotProps.data.data_used) : (0 + ' KB')}}
								</template>
							</Column>
							<Column field="dataFreeOrgAll" header="Dung lượng còn trống"
								headerStyle="max-width:200px;background:linear-gradient(60deg, #27ae60, #58d68d); color:#fff; border-right:0;"
								bodyStyle="text-align:center;max-width:200px;"
								class="align-items-center justify-content-center cell-data px-2"
							>
								<template #body="slotProps">
									{{slotProps.data.dataFreeOrgAll != null && slotProps.data.dataFreeOrgAll != 0 ? formatByte(slotProps.data.dataFreeOrgAll) : (0 + ' KB')}}
								</template>
							</Column>
						</DataTable>
					</div>
				</div>
				<div class="flex col-12 md:col-12 p-0">
					<div style="border:1px solid #e9ecef;width: 100%;">
						<div>
							<div class="flex align-items-center font-bold px-3 py-2" style="background-color:#f8f9fa;">
								<div>
									<i class="pi pi-list"></i>
									Thống kê dung lượng theo module<span class="mx-2">|</span>
								</div>
								<div>
									<span style="color:#727272;">Thống kê theo: </span>
									<Dropdown v-model="selectedStatistic" :options="typeStatistic" optionLabel="name" optionValue="code" 
										placeholder="Thống kê" 
										style="width:10rem;"
									/>
								</div>
								<div style="margin-left:0.75rem;">
									<Dropdown
										class="p-0 m-0"
										v-model="selectedFilterOrg"
										:options="listOrgSubAll"
										optionLabel="organization_name"
										optionValue="organization_id"
										placeholder="-- Chọn đơn vị --"										
										style="width:25rem;"
									/>
								</div>
							</div>
						</div>
						<table style="width:100%;background-color:#fff;border-spacing: inherit;" v-if="selectedStatistic == 1">
							<thead>
								<tr class="header-data">
									<th width="50" class="text-center p-3 sticky" style="background-color: #f8f9fa;">
										<span>STT</span>
									</th>
									<th class="text-left p-3 sticky" style="min-width: 250px;background-color: #f8f9fa;">
										<span>Tên công ty</span>
									</th>
									<th width="250" class="text-center p-3 sticky" style="background-color: #2ecc71; color: #fff;">
										<span>Công việc</span>
									</th>
									<th width="250" class="text-center p-3 sticky" style="background-color: #B0DE09; color: #fff;">
										<span>Thiết bị</span>
									</th>
									<th width="250" class="text-center p-3 sticky" style="background-color: #3498db; color: #fff;">
										<span>Văn bản</span>
									</th>
									<th width="250" class="text-center p-3 sticky" style="background-color: #8e44ad; color: #fff;">
										<span>Lịch công tác</span>
									</th>
									<th width="250" class="text-center p-3 sticky" style="background-color: #f1c40f; color: #fff;">
										<span>Luật</span>
									</th>
									<th width="250" class="text-center p-3 sticky" style="background-color: #f39c12; color: #fff;">
										<span>Trao đổi</span>
									</th>
									<th width="250" class="text-center p-3 sticky" style="background-color: #e74c3c; color: #fff;">
										<span>Truyền thông</span>
									</th>
									<th width="250" class="text-center p-3 sticky" style="background-color: #cc2e50; color: #fff;">
										<span>Kho dữ liệu</span>
									</th>
								</tr>
							</thead>
							<tbody class="body-data">
								<tr class="body-tr-data" v-for="(ct, index) in DataFilter()" :key="index">
									<td class="text-center p-3">
										{{index + 1}}
									</td>
									<td class="p-3 font-bold">
										{{ct.organization_name}}
									</td>
									<td class="text-center p-3 font-bold">
										{{ct.task ? formatByte(ct.task) : '0 KB'}}
									</td>
									<td class="text-center p-3 font-bold">
										{{ct.device ? formatByte(ct.device) : '0 KB'}}
									</td>
									<td class="text-center p-3 font-bold">
										{{ct.docs ? formatByte(ct.doc) : '0 KB'}}
									</td>
									<td class="text-center p-3 font-bold">
										{{ct.calendar ? formatByte(ct.calendar) : '0 KB'}}
									</td>
									<td class="text-center p-3 font-bold">
										{{ct.law ? formatByte(ct.law) : '0 KB'}}
									</td>
									<td class="text-center p-3 font-bold">
										{{ct.chat ? formatByte(ct.chat) : '0 KB'}}
									</td>
									<td class="text-center p-3 font-bold">
										{{ct.video ? formatByte(ct.video) : '0 KB'}}
									</td>
									<td class="text-center p-3 font-bold" style="border-bottom: 1px solid #e9ecef;">
										{{ct.file? formatByte(ct.file) : '0 KB'}}
									</td>
								</tr>
							</tbody>
						</table>
						<div class="pb-2" style="width:100%;" v-if="selectedStatistic == 2">
							<Chart type="bar" id="chartHorizontal" :height="100" :data="basicData" :options="horizontalOptions" v-if="detailDataUsing != null"/>
						</div>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>
<style scoped>
	tr.header-data th.sticky {
		top: 0px;
		border-right:1px solid #e9ecef;
		border-top: 1px solid #e9ecef;
		border-bottom: 1px solid #e9ecef;
		font-weight: bold;
	}
	tr.header-data th.sticky:last-child {
		border-right:none;
	}
	tr.body-tr-data td {
		border-right: 1px solid #e9ecef;
		border-bottom: 1px solid #e9ecef;
	}
	tr.body-tr-data td:last-child {
		border-right: none;
	}
	tbody.body-data tr.body-tr-data:last-child td {
		border-bottom: none;
	}
</style>
<style lang="scss" scoped>
	::v-deep(.p-datatable.table-data-using) {
		.p-datatable-thead > tr > th.cell-data {
			border-right: 1px solid #ededed;
		}
		.p-datatable-tbody > tr > td.cell-data {
			border-right: 1px solid #ededed;
			padding: 1rem 0.5rem !important;
		}
		.p-datatable-wrapper {
			max-height: calc(100vh - 110px);
		}
	}
	::v-deep(.p-datatable.table-data-using-sub) {
		.p-datatable-wrapper {
			max-height: calc(100vh - 633px);
			overflow-y: auto;
		}
		.p-datatable-thead > tr > th.cell-data {
			border-right: 1px solid #ededed;
			position: sticky;
			top: 0;
		}
		th.justify-content-center .p-column-header-content {
			justify-content: center;
		}
		
		.p-datatable-tbody > tr > td.cell-data {
			border-right: 1px solid #ededed;
			padding: 1rem 0.5rem !important;
		}		
	}
	::v-deep(.p-datatable-scrollable.p-datatable-grouped-footer) {
		.p-datatable-tfoot > tr {
			display: flex;
		}
		.p-datatable-tfoot > tr > td.cell-data {
			border-right: 1px solid #ededed;
			padding: 1rem 0.5rem !important;
		}
	}
</style>
