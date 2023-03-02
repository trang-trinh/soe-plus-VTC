<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr, checkURL } from "../../../../util/function.js";
//import moment from "moment";
//import treeuser from "../../../../components/user/treeuser.vue";
//const cryoptojs = inject("cryptojs");
const toast = useToast();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
//const emitter = inject("emitter");
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const bgColor = ref([
    "#F8E69A", "#AFDFCF", "#F4B2A3", "#9A97EC", "#CAE2B0", "#8BCFFB", "#CCADD7"
]);
const props = defineProps({	
	key: Number,
	headerDialog: String,
	displayDialog: Boolean,
	dataForm: Object,
	reloadData: Function,
	closeDialog: Function,
});

const rules = {
	request_form_name: {
		required,
		maxLength: maxLength(500),
		$errors: [
			{
				$property: "request_form_name",
				$validator: "required",
				$message: "Tên loại đề xuất không được để trống!",
			},
		],
	},
	request_form_code: {
		required,
		maxLength: maxLength(50),
		$errors: [
			{
				$property: "request_form_code",
				$validator: "required",
				$message: "Mã loại đề xuất không được để trống!",
			},
		],
	},
};
const request_form = ref(props.dataForm);
const v$ = useVuelidate(rules, request_form);

onMounted(() => {
	return {};
});
</script>
<template>
	<Dialog
		:header="props.headerDialog"
		v-model:visible="props.displayDialog"
		:style="{ width: '40vw' }"
		:closable="false"
		:modal="true"
	>
		<form @submit.prevent="">
			<div class="grid formgrid m-0">
				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					<div class="col-3 text-left flex p-0" style="align-items:center;">
						Mã loại đề xuất <span class="redsao pl-1"> (*)</span>
					</div>
					<Textarea
						v-model="request_form.request_form_code"
						spellcheck="false"
						class="col-9 ip36 p-2"
						autoResize
						autofocus
						rows="1"
						:class="{ 'p-invalid': v$.request_form_code.$invalid && submitted, }"
					/>
				</div>
				<div class="field col-12 md:col-12 flex p-0">
					<div class="col-3 text-left"></div>
					<small
						v-if="
							(v$.request_form_code.$invalid && submitted) ||
							v$.request_form_code.$pending.$response
						"
						class="col-9 p-error"
					>
						<span class="col-12 p-0">
							{{
								v$.request_form_code.required.$message
									.replace(
										"Value",
										"Mã loại đề xuất"
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
						v-if="
							(v$.request_form_code.maxLength.$invalid &&
								submitted) ||
							v$.request_form_code.maxLength.$pending.$response
						"
					>
						<div class="col-12 md:col-12 flex">
							<label class="col-2 text-left"></label>
							<span class="col-10 p-0">
								{{
									v$.request_form_code.maxLength.$message.replace(
										"The maximum length allowed is",
										"Mã loại đề xuất không được vượt quá"
									)
								}}
								ký tự
							</span>
						</div>
					</small>
				</div>

				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					<div class="col-3 text-left flex p-0" style="align-items:center;">
						Tên loại đề xuất <span class="redsao pl-1"> (*)</span>
					</div>
					<Textarea
						v-model="request_form.request_form_name"
						spellcheck="false"
						class="col-9 ip36 p-2"
						autoResize
						autofocus
						rows="1"
						:class="{ 'p-invalid': v$.request_form_name.$invalid && submitted, }"
					/>
				</div>
				<div class="field col-12 md:col-12 flex p-0">
					<div class="col-3 text-left"></div>
					<small
						v-if="
							(v$.request_form_name.$invalid && submitted) ||
							v$.request_form_name.$pending.$response
						"
						class="col-9 p-error"
					>
						<span class="col-12 p-0">
							{{
								v$.request_form_name.required.$message
									.replace(
										"Value",
										"Tên loại đề xuất"
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
						v-if="
							(v$.request_form_name.maxLength.$invalid &&
								submitted) ||
							v$.request_form_name.maxLength.$pending.$response
						"
					>
						<div class="col-12 md:col-12 flex">
							<label class="col-2 text-left"></label>
							<span class="col-10 p-0">
								{{
									v$.request_form_name.maxLength.$message.replace(
										"The maximum length allowed is",
										"Tên loại đề xuất không được vượt quá"
									)
								}}
								ký tự
							</span>
						</div>
					</small>
				</div>

				<div class="col-12 field md:col-12 flex p-0">
					<div class="field col-6 md:col-6 p-0 align-items-center flex">
						<div class="col-6 text-left p-0">STT</div>
						<InputNumber
							v-model="request_form.is_order"
							class="col-6 ip36 p-0"
						/>
					</div>
					<div class="field col-6 md:col-6 p-0 align-items-center flex">
						<div class="col-6 text-center p-0">Trạng thái</div>
						<InputSwitch v-model="request_form.status" />
					</div>
				</div>
			</div>
		</form>
		<template #footer>
			<Button
				label="Hủy"
				icon="pi pi-times"
				@click="props.closeDialog"
				class="p-button-outlined"
			/>

			<Button
				label="Lưu"
				icon="pi pi-check"
				@click="saveData()"
				autofocus
			/>
		</template>
	</Dialog>
</template>
<style scoped>

</style>