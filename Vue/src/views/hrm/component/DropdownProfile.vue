<script setup>
import { ref, inject, onMounted, onBeforeUpdate, onUpdated } from "vue";

import { encr, checkURL } from "../../../util/function.js";

const emitter = inject("emitter");
const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
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
            proc: "hrm_profile_list_all_dd",
            par: [{ par: "user_id", va: store.getters.user.user_id },
            { par: "search", va: null },
 
            { par: "tab ", va: 1 },
            { par: "listprofile ", va:props.model },
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
        if (props.except_user_now == null || (props.except_user_now != null && element.profile_id != props.except_user_now)) {
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
        }
      });
      var models = null;
      if (props.optionValue == "code") {
        models = listDataUsers.value.find((x) => x.profile_id == props.model);
        if (models)
          model.value = {
            profile_id: models.profile_id,
            profile_user_name: models.profile_user_name,
            avatar: models.avatar,
          };
        else model.value = null;
      } else {
        models = listDataUsers.value.find(
          (x) => x.profile_user_name == props.model
        );
        if (models) model.value = models.profile_user_name;
        else model.value = null;
      }

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
  model: String,
  placeholder: String,
  class: String,
  display: String,
  disabled: Boolean,
  editable: Boolean,
  optionLabel: String,
  optionValue: String,
  style: String,
  callbackFun: Function,
  key_user: String,
  except_user_now: String,
});
const model = ref();
const submitModel = () => {
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
  var models = null;
  if (listDataUsersSave.value.length > 0) {
    if (props.optionValue == "code") {
      models = listDataUsersSave.value.find((x) => x.profile_id == props.model);
      if (models)
        model.value = {
          profile_id: models.profile_id,
          profile_user_name: models.profile_user_name,
          avatar: models.avatar,
        };
      else model.value = null;
    } else {
      models = listDataUsersSave.value.find(
        (x) => x.profile_user_name == props.model
      );
      if (models) model.value = models.profile_user_name;
      else model.value = null;
    }
  } else loadUserProfiles();
});
</script>

<template>
  <Dropdown
    :options="listDataUsers"
    :filter="true"
    :showClear="true"
    :editable="props.editable"
    :optionLabel="props.optionLabel"
    :optionValue="props.optionValue"
    :style="props.style"
    v-model="model"
    class="d-dropdown-design"
    style="height: auto; min-height: 36px"
    :placeholder="props.placeholder"
    @change="submitModel"
    :class="props.class"
    v-if="isShow"
    :disabled="props.disabled"
  >
    <template #value="slotProps">
      <div class="m-0 p-0 h-full" v-if="slotProps.value">
        <div class="flex align-items-center h-full">
          <div class="format-center h-full">
            <Avatar
              v-bind:label="
                slotProps.value.avatar
                  ? ''
                  : (slotProps.value.profile_user_name ?? '').substring(0, 1)
              "
              v-bind:image="
                slotProps.value.avatar
                  ? basedomainURL + slotProps.value.avatar
                  : basedomainURL + '/Portals/Image/noimg.jpg'
              "
              :style="{
                background:
                  bgColor[slotProps.value.profile_user_name.length % 7],
                color: '#ffffff',
                width: '2rem',
                height: '2rem',
              }"
              class="mr-2 text-avatar"
              size="xlarge"
              shape="circle"
            />
          </div>
          <div class="format-flex-center">
            <span>{{ slotProps.value.profile_user_name }}</span>
          </div>
        </div>
      </div>
      <span v-else> {{ slotProps.placeholder }} </span>
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
              background-color: #2196f3;
              color: #ffffff;
              width: 3rem;
              height: 3rem;
              font-size: 1.4rem !important;
            "
            :style="{
              background:
                bgColor[slotProps.option.profile_user_name.length % 7],
            }"
            class="text-avatar"
            size="xlarge"
            shape="circle"
          />
        </div>
        <div class="format-center text-left ml-3">
          <div>
            <div class="mb-1">
              {{ slotProps.option.profile_user_name }}
            </div>
            <div class="description">
              <div>
                <span>{{ slotProps.option.profile_code }}</span
                ><span v-if="slotProps.option.department_name">
                  | {{ slotProps.option.department_name }}</span
                >
              </div>
            </div>
          </div>
        </div>
      </div>
      <span v-else> Chưa có dữ liệu </span>
    </template>
  </Dropdown>
</template>