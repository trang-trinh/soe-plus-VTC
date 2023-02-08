<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import arrIcons from "../../assets/json/icons.json";
import router from "@/router";

//Declare
const model = ref({
    status: true,
});

//Valid Form
const submitted = ref(false);
const rules = {
  Ten: {
    required,
  },
};
const v$ = useVuelidate(rules, model);


//Function

//Init

</script>
<template>

</template>
<style scoped>

</style>
