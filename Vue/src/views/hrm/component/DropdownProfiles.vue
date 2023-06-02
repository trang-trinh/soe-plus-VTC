<script setup>
import { ref, inject, onMounted, onBeforeUpdate, onUpdated } from "vue";

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
            proc: "hrm_profile_list_all_d",
            par: [{ par: "user_id", va: store.getters.user.user_id },
            { par: "search", va: null },
            { par: "tab ", va: 1 },
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
          code: {
            profile_id: element.profile_id,
            profile_user_name: element.profile_user_name,
            avatar: element.avatar,
          },
          profile_id: element.profile_id,
          avatar: element.avatar,
          department_name: element.department_name,
          department_id: element.department_id,
          work_position_name: element.work_position_name,
          position_name: element.position_name,
          profile_code: element.profile_code,
          organization_id: element.organization_id,
        });
      });

      model.value = [];
      props.model.forEach((itemsa) => {
        var models = listDataUsers.value.find((x) => x.profile_id == itemsa);
        if (models)
          model.value.push({
            profile_id: models.profile_id,
            profile_user_name: models.profile_user_name,
            avatar: models.avatar,
          });
        else model.value = [];
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
  disabled: Boolean,
  type: Intl,
  callbackFun: Function,
  key_user: String,
});
const model = ref();
const submitModel = () => {
  props.callbackFun(props.key_user, model.value);
  return false;
};
const removeUser = (item) => {
  model.value = model.value.filter((x) => x.profile_id != item.profile_id);
  props.callbackFun(props.key_user, model.value);
  return false;
};
onMounted(() => {
  loadUserProfiles();
  return {
    loadUserProfiles,
    model,
  };
});
onBeforeUpdate(() => {
  
  model.value = [];
  if(props.model && listDataUsersSave.value.length>0)
      props.model.forEach((itemsa) => {
        var models = listDataUsersSave.value.find((x) => x.profile_id == itemsa);
        if (models)
          model.value.push({
            profile_id: models.profile_id,
            profile_user_name: models.profile_user_name,
            avatar: models.avatar,
          });
        else model.value = [];
      });
      else
      loadUserProfiles();

 
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
    class="w-full p-0 d-multi-design"
    :class="props.class"
    :display="props.display"
    :filter="true"
    v-if="isShow"
    :disabled="props.disabled"
  >
    <template #value="slotProps">
      <div style="min-height: 1.5rem; cursor: default">
        <span
          class="mx-1 relative"
          v-for="(item, index) in slotProps.value"
          :key="index"
          style="vertical-align: top"
        >
          <div class="p-chip d-chip-design p-0 my-1">
            <Avatar
              v-bind:label="
                item.avatar ? '' : item.profile_user_name.substring(0, 1)
              "
              v-bind:image="
                item.avatar
                  ? basedomainURL + item.avatar
                  : basedomainURL + '/Portals/Image/noimg.jpg'
              "
              style="
                color: #ffffff;
                width: 2.5rem;
                height: 2.5rem;
                font-size: 1.4rem !important;
              "
              :style="{
                background: bgColor[item.profile_user_name.length % 7],
              }"
              size="xlarge"
              shape="circle"
              class="p-0"
            />
            <div class="p-chip-text px-1">{{ item.profile_user_name }}</div>
            <div
              class="p-2 align-items-center format-center p-multiselect-token-icon"
              @click="removeUser(item)"
            >
              <i class="pi pi-times-circle"></i>
            </div>
          </div>
        </span>
      </div>
    </template>
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