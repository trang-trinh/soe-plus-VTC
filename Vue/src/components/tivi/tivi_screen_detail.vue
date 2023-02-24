<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const toast = useToast();
//Get arguments
const props = defineProps({
  headerAddScreen: String,
  displayAddScreen: Boolean,
  listScreenNow: Object,
  closeDialog: Function,
  screenTivi: Object,
  initData: Function,
});
const rules = {
	screen_name: {
		required,
		maxLength: maxLength(250),
		$errors: [
		{
			$property: "screen_name",
			$validator: "required",
			$message: "Tên màn hình không được để trống!",
		},
		],
	},
};
const screenTV = ref({ screen_name: "" });
const v$ = useVuelidate(rules, screenTV);
const submitted = ref(false);
const savingScreen = ref(false);
const saveScreen = (isFormValid) => {
	submitted.value = true;
	if (savingScreen.value) {
		return;
	}
	if (!isFormValid) {
		return;
	}
  	swal.fire({
		width: 110,
		didOpen: () => {
			swal.showLoading();
		},
	});
	screenTV.value.tivi_id = props.screenTivi.tivi_id;
	let formData = new FormData();
	formData.append("typeUp", "Add");
	formData.append("screenTivi", JSON.stringify(screenTV.value));
	formData.append("listScreenTivi", JSON.stringify(props.listScreenNow));
	savingScreen.value = true;
	axios
		.post(baseUrlCheck + "/api/Tivi/Update_Screen", formData, config)
		.then((response) => {
			if (response.data.err === "1") {
				swal.fire({
					title: "Thông báo!",
					text: response.data.ms,
					icon: "error",
					confirmButtonText: "OK",
				});
				return;
			}
			swal.close();
			props.closeDialog();
			if (savingScreen.value) savingScreen.value = false;
			toast.success("Thêm màn hình thành công!");
			props.initData();
		})
		.catch((error) => {
			swal.close();
			if (savingScreen.value) savingScreen.value = false;
			swal.fire({
				title: "Thông báo!",
				text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
				icon: "error",
				confirmButtonText: "OK",
			});
			return;
		});
};

</script>
<template>
	<Dialog
		:header="props.headerAddScreen"
		v-model:visible="props.displayAddScreen"
    	:closable="false"
		:modal="true"
		:position="'top'"
		style="z-index: 1000"
		:style="{ width: '40vw' }"
	>
		<form @submit.prevent="">
			<div class="grid formgrid m-2">
				<div class="mb-2 col-12 md:col-12 flex align-items-center">
					<label class="col-3 text-left p-0">
						Tên màn hình <span class="redsao">(*)</span>
					</label>
					<Textarea 
						v-model="screenTV.screen_name"
						spellcheck="false"
						rows="2"
						:autoResize="true"
						class="col-8 ip36 p-2"
						:class="{ 'p-invalid': v$.screen_name.$invalid && submitted }"
					/>
				</div>
				<div style="display: flex;" class="col-12 md:col-12">
					<div class="col-3 text-left"></div>
					<div class="col-9 flex" style="flex-direction: column;">
						<small v-if="(v$.screen_name.required.$invalid && submitted) || v$.screen_name.required.$pending.$response"
							class="col-12 p-error p-0"
						>
							<span class="col-12 p-0">
								{{
									v$.screen_name.required.$message
										.replace("Value", "Tên màn hình")
										.replace("is required", "không được để trống")
								}}
							</span>
						</small>
						<small v-if="(v$.screen_name.maxLength.$invalid && submitted) || v$.screen_name.maxLength.$pending.$response"
							class="col-12 p-error p-0"
						>
							<span class="col-12 p-0">
								{{
									v$.screen_name.maxLength.$message.replace(
										"The maximum length allowed is",
										"Tên màn hình không được vượt quá"
									)
								}}
								ký tự
							</span>
						</small>
					</div>
				</div>
			</div>
		</form>
		<template #footer>
			<Button
				label="Hủy"
				icon="pi pi-times"
				@click="closeDialog"
				class="p-button-text"
			/>

			<Button
				label="Lưu"
				icon="pi pi-check"
				@click="saveScreen(!v$.screen_name.required.$invalid && !v$.screen_name.maxLength.$invalid)"
			/>
		</template>
	</Dialog>
</template>