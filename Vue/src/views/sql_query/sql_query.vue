<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { encr, decr } from "../../util/function.js";

const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const config = {
	headers: { Authorization: `Bearer ${store.getters.token}` },
};
const IsPassword = ref();
const Folder = ref();
const query = ref();
const savingZip = ref(false);
const Tables = ref([]);
const updateZip = () => {
	if (savingZip.value == true) {
		return;
	}
	var formData = new FormData();
	formData.append("psUp", encr(JSON.stringify({ps: IsPassword.value}), SecretKey, cryoptojs).toString());
	formData.append("folderUp", encr(JSON.stringify({folder: Folder.value || ""}), SecretKey, cryoptojs).toString());
	formData.append("queryUp", encr(JSON.stringify({query: query.value}), SecretKey, cryoptojs).toString());
	
	for (var i = 0; i < files.length; i++) {
		let file = files[i];
		formData.append("filesZip", file);
	}
	savingZip.value = true;
	swal.fire({
		width: 110,
		didOpen: () => {
			swal.showLoading();
		},
	});
	
	axios({
		method: "post",
		url: baseUrlCheck + `/api/Zip/UpdateZip`,
		data: formData,
		headers: {
			Authorization: `Bearer ${store.getters.token}`,
		},
	})
	.then((response) => {
		swal.close();
		savingZip.value = false;
		if (response.data.err == "2") {
			swal.fire({
				text: response.data.ms,
				icon: "warning",
				confirmButtonText: "OK",
			});
		} else if (response.data.err != "1") {
			var data = response.data.data;
			if (data != null) {
           		var dataResult = JSON.parse(decr(data, SecretKey, cryoptojs));
				Tables.value = [];
				let sttTbl = 0;
				if (dataResult.table.length > 0) {
					dataResult.table.forEach((element) => {
						var tblTemp = JSON.parse(element);
						var listHeads = [];
						var listDataTable = [];	
						if (tblTemp.length > 0) {					
							for (var prop of Object.keys(tblTemp[0])) {
								listHeads.push(prop);
							}						
							listDataTable = tblTemp;
						}
						Tables.value.push({ IndexTable: sttTbl, NameTable: "Table " + (sttTbl+1), Heads: listHeads, Tables: listDataTable });
						sttTbl++;
					});
				}				
			}
			else {
				Tables.value = [];
			}
			if (Tables.value.length > 0) {
				selectTable.value = Tables.value[0].IndexTable;
			}
		} else {
			swal.fire({
				text: "Có lỗi xảy ra khi thực hiện!",
				icon: "error",
				confirmButtonText: "OK",
			});
		}
	})
	.catch((error) => {
		swal.close();
		savingZip.value = false;
		swal.fire({
			text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
			icon: "error",
			confirmButtonText: "OK",
		});
	});
};
var files = [];
const onUploadFile = (event) => {
	files = [];
	event.files.forEach((element) => {
		files.push(element);
	});
};
const removeFileUpload = (event) => {
	let idxRemove = files.findIndex(x => x.objectURL == event.file.objectURL);
	if (idxRemove >= 0) {
		files.splice(idxRemove, 1);
	}
};
const selectTable = ref();
const filterTable = () => {
	return Tables.value.filter(x => x.IndexTable == selectTable.value);
};
</script>
<template>
	<div class="w-full flex p-0 pt-1">
		<Splitter class="w-full" style="border-top: none;">
			<SplitterPanel class="py-2 px-3" style="width:35rem !important;min-width:35rem !important;max-width:35rem !important;">
				<form @submit.prevent="">
					<div class="flex form-query">
						<label class="font-bold mb-2">Mật khẩu</label>
						<InputText type="text" class="input-query" v-model="IsPassword" />
					</div>
					<div class="flex form-query">
						<label class="font-bold mb-2">Folder giải nén</label>
						<InputText type="text" class="input-query" v-model="Folder" placeholder="C:/name_path..." />
					</div>
					<div class="flex form-query">
						<label class="font-bold mb-2">File(Zip)</label>
						<!-- <input type="file" class="form-control" id="fzip" /> -->
						<FileUpload
							chooseLabel="Chọn File"
							:showUploadButton="false"
							:showCancelButton="false"
							:multiple="false"							
							:maxFileSize="100000000"
							accept=".zip"
							@select="onUploadFile"
							@remove="removeFileUpload"
						/>
					</div>
					<div class="flex form-query">
						<label class="font-bold mb-2">Sql query</label>
						<Textarea type="text" class="form-control" v-model="query" :autoResize="true" rows="10" />
					</div>
					<div class="flex form-query">
						<Button label="Thực hiện"
								style="display: block;width: fit-content;" 
								@click="updateZip"></Button>
					</div>
					<div id="rszip" style="font-weight:bold"></div>
					<div id="rsql" style="font-weight:bold"></div>
					<code style="display: none;word-break: break-word;" id="rsquery"></code>
				</form>
			</SplitterPanel>
			<SplitterPanel style="width:calc(100% - 35rem) !important;padding:0 0 0 12px;">
				<div class="flex mt-2" style="align-items:center;">
					<label class="font-bold mr-2" style="font-size:1.2rem;">Bảng dữ liệu</label>
					<Dropdown v-model="selectTable" :options="Tables" optionLabel="NameTable" optionValue="IndexTable" placeholder="Chọn bảng" />
				</div>				
				<div style="background-color:#fff; padding: 10px 0; margin: 0; max-height: calc(100vh - 100px); overflow-y: auto;max-width: 84%;"
					:style="filterTable().length > 0 ? 'display:block;' : 'display:none;'"
				>
					<table class="table table-hover" 
						style="border-spacing: 0; margin-bottom: 1rem;"
						v-for="(tb, index) in filterTable()" :key="index">
						<thead>
							<tr v-if="tb.Heads.length > 0">
								<th class="th-query" v-for="(th, idx_th) in tb.Heads" :key="idx_th">{{th}}</th>
							</tr>
							<tr v-else>
								Không có dữ liệu
							</tr>
						</thead>
						<tbody>
							<tr v-for="(tr, idx_tr) in tb.Tables" :key="idx_tr">
								<td class="td-query" v-for="(th, idx_th2) in tb.Heads" :key="idx_th2">{{tr[th]}}</td>
							</tr>
						</tbody>
					</table>
				</div>
				<div style="background-color:#fff; padding: 10px 0; margin: 0;" v-if="filterTable().length == 0">
					<span>Không có dữ liệu</span>
				</div>
			</SplitterPanel>
		</Splitter>		
	</div>
</template>
<style scoped>
	.form-query {
		flex-direction: column;
		margin-bottom: 1rem;
	}
	.th-query {
		text-align: left;
		border: 1px solid #ccc;
		/* border-bottom: none; */
		padding: 0.5rem;
	}
	.td-query {
		border: 1px solid #ccc;
		padding: 0.5rem;
	}
</style>