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
    headerDialog: String,
	displayDialog: Boolean,
    closeDialog: Function,
});
onMounted(() => {
	return {};
});
</script>
<template>
	<Dialog
		:header="props.headerDialog"
		v-model:visible="props.displayDialog"
		:style="{ width: '70vw' }"
		:closable="false"
		:modal="true"
	>
		<form @submit.prevent="">
			<div class="grid formgrid m-0">
				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					Danh sách Team đề xuất
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