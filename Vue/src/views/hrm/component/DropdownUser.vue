<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { encr, checkURL } from "../../../util/function.js";
import moment from "moment";
const emitter = inject("emitter");
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const isDynamicSQL = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = baseURL;
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const listDataUsers = ref([]);
const listDataUsersSave = ref([]);
const loadUserProfiles = () => {
  listDataUsers.value = [];

  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_profile_list_filter",
            par: [
              { par: "search", va: null },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "work_position_id", va: null },
              { par: "position_id", va: null },
              { par: "department_id", va: null },
              { par: "status", va: 1 },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];

      data.forEach((element, i) => {
        listDataUsers.value.push({
          profile_user_name: element.profile_user_name,
          code: element.profile_id,
          avatar: element.avatar,
          department_name: element.department_name,
          department_id: element.department_id,
          work_position_name: element.work_position_name,
          position_name: element.position_name,
          profile_code: element.profile_code,
          organization_id: element.organization_id,
        });
      });
      listDataUsersSave.value = [...listDataUsers.value];
      isShow.value = true;
    })
    .catch((error) => {
      console.log(error);

      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const isShow = ref(false);
const props = defineProps({
  model: Object,
  placeholder: String,
  class: String,
  display: String,
 
});
const model = ref();
const submitModel=()=>{
 
    emitter.emit("emitData", { type: "submitModel", data:   model.value });
}


onMounted(() => {
  model.value = props.model;
  loadUserProfiles();
  return {
    loadUserProfiles,
    model,
  };
});
 
</script>

<template>
  <MultiSelect
    v-model="model"
    :options="listDataUsers"
    optionLabel="profile_user_name"
    optionValue="code"
    :placeholder="props.placeholder"
    @change="submitModel"
    panelClass="d-design-dropdown"
    class="w-full p-0"
    :class="props.class"
    :display="props.display"
    :filter="true"
    v-if="isShow"
  >
    <template #option="slotProps">
      <div v-if="slotProps.option" class="flex">
        <div class="format-center">
          <Avatar
            v-bind:label="
              slotProps.option.avatar
                ? ''
                : slotProps.option.profile_user_name.substring(0, 1)
            "
            v-bind:image="
              slotProps.option.avatar
                ? basedomainURL + slotProps.option.avatar
                : basedomainURL + '/Portals/Image/noimg.jpg'
            "
            style="
              color: #ffffff;
              width: 3rem;
              height: 3rem;
              font-size: 1.4rem !important;
            "
            :style="{
              background:
                bgColor[slotProps.option.profile_user_name.length % 7],
            }"
            size="xlarge"
            shape="circle"
          />
        </div>
        <div class="format-center text-left ml-3">
          <div>
            <div class="mb-1 font-bold">
              {{ slotProps.option.profile_user_name }}
            </div>
            <div class="description">
              <div>
                <span v-if="slotProps.option.position_name">{{
                  slotProps.option.position_name
                }}</span>
                <span v-else>{{ slotProps.option.profile_code }}</span>

                <span v-if="slotProps.option.department_name">
                  | {{ slotProps.option.department_name }}</span
                >
              </div>
            </div>
          </div>
        </div>
      </div>
      <span v-else> Chưa có dữ liệu </span>
    </template>
  </MultiSelect>
</template>