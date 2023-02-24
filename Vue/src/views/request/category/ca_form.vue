<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
import moment from "moment";
//Khai báo

const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const config = {
	headers: { Authorization: `Bearer ${store.getters.token}` },
};

const datalists = ref([]);

onMounted(() => {
	if (!checkURL(window.location.pathname, store.getters.listModule)) {
		//router.back();
	}
	//loadData(true);
	return {
		datalists,
		options,
		//loadData,
	};
});
</script>
<template>	
	<div class="main-layout flex-grow-1 p-2 pb-0 pr-0" v-if="store.getters.islogin">
		
	</div>
</template>
    
<style scoped>
</style>