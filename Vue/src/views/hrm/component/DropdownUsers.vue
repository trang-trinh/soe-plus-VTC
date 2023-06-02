<script setup>
import { ref, inject, onMounted, watch, onUpdated } from "vue";

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
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "sys_users_list_dd",
            par: [
              { par: "search", va: null },
              { par: "user_id", va: store.getters.user.user_id },
              { par: "role_id", va: null },
              {
                par: "organization_id",
                va: store.getters.user.organization_id,
              },
              { par: "department_id", va: null },
              { par: "position_id", va: null },
              { par: "pageno", va: 1 },
              { par: "pagesize", va: 100000 },
              { par: "isadmin", va: null },
              { par: "status", va: null },
              { par: "start_date", va: null },
              { par: "end_date", va: null },
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
          full_name: element.full_name,
          code: {
            user_id: element.user_id,
            full_name: element.full_name,
            avatar: element.avatar,
          },
          user_id: element.user_id,
          avatar: element.avatar,
          department_name: element.department_name,
          department_id: element.department_id,
          role_name: element.role_name,
          position_name: element.position_name,
          organization_name: element.organization_name,
          organization_id: element.organization_id,
        });
      });
      model.value = [];
      props.model.forEach((element) => {
        var models = listDataUsers.value.find((x) => x.user_id == element);
        if (models)
          model.value.push({
            user_id: models.user_id,
            full_name: models.full_name,
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
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
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
});
const model = ref();
const submitModel = () => {
  emitter.emit("emitData", {
    type: "submitDropdownUsers",
    data: { data: model.value, type: props.type },
  });
};
const removeUser = (item) => {
  model.value=model.value.filter(x=>x.user_id!=item.user_id);
  emitter.emit("emitData", {
    type: "submitDropdownUsers",
    data: { data: model.value, type: props.type },
  });
};
onMounted(() => {
  loadUserProfiles();
  return {
    loadUserProfiles,
    model,
  };
});
// onUpdated(() => {
//   model.value = [];
//   props.model.forEach((element) => {
//     var models = listDataUsersSave.value.find((x) => x.user_id == element);
//     if (models)
//       model.value.push({
//         user_id: models.user_id,
//         full_name: models.full_name,
//         avatar: models.avatar,
//       });
//     else model.value = [];
//   });
// });
</script>

<template>
  <MultiSelect
    v-model="model"
    :options="listDataUsers"
    optionLabel="full_name"
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
              v-bind:label="item.avatar ? '' : item.full_name.substring(0, 1)"
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
                background: bgColor[item.full_name.length % 7],
              }"
              size="xlarge"
              shape="circle"
              class="p-0"
            />
            <div class="p-chip-text px-1">{{ item.full_name }}</div>
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
                : slotProps.option.full_name.substring(0, 1)
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
              background: bgColor[slotProps.option.full_name.length % 7],
            }"
            size="xlarge"
            shape="circle"
          />
        </div>
        <div class="format-center text-left ml-3">
          <div>
            <div class="mb-1 font-bold">
              {{ slotProps.option.full_name }}
            </div>
            <div class="description">
              <div>
                <span v-if="slotProps.option.position_name">{{
                  slotProps.option.position_name
                }}</span>
                <span v-else-if="slotProps.option.role_name">{{
                  slotProps.option.role_name
                }}</span>

                <span v-if="slotProps.option.department_name">
                  | {{ slotProps.option.department_name }}</span
                >
                <span v-else-if="slotProps.option.organization_name">
                  | {{ slotProps.option.organization_name }}</span
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